#Region " Option Statements "

Option Explicit On
Option Strict On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Collections.Generic
Imports System.Collections.ObjectModel
Imports System.Diagnostics

#End Region

#Region " GoodFonAccountPool "

''' <summary>
''' Manages a thread-safe, ordered pool of authenticated GoodFon.com accounts,
''' rotating to the next account when the current one reaches its daily download limit,
''' and automatically resetting exhausted accounts after a cooldown period.
''' </summary>
Friend NotInheritable Class GoodFonAccountPool ' NOSONAR

#Region " Fields "

    ''' <summary>
    ''' The default cooldown period after which an exhausted account becomes available again (24 hours + 2 minutes).
    ''' </summary>
    Friend Shared ReadOnly DefaultLimitResetCooldown As TimeSpan = TimeSpan.FromMinutes(1442)

    ''' <summary>
    ''' Lock object to access the pool state with thread-safety.
    ''' </summary>
    Private ReadOnly syncObject As New Object()

    ''' <summary>
    ''' The function that returns the current date and time.
    ''' </summary>
    Private ReadOnly clock As Func(Of Date)

    ''' <summary>
    ''' The exact time each account reached its daily limit, used to evaluate the cooldown reset.
    ''' </summary>
    Private ReadOnly limitReachedTimes As New Dictionary(Of GoodFonAuth, Date)()

    ''' <summary>
    ''' The index of the current active account in <see cref="GoodFonAccountPool.Accounts"/>.
    ''' </summary>
    Private currentIndexValue As Integer

#End Region

#Region " Properties "

    ''' <summary>
    ''' Gets the authenticated accounts of this pool, in rotation order.
    ''' </summary>
    Friend ReadOnly Property Accounts As ReadOnlyCollection(Of GoodFonAuth)

    ''' <summary>
    ''' Gets the cooldown period after which an exhausted account becomes available again.
    ''' </summary>
    Friend ReadOnly Property LimitResetCooldown As TimeSpan

    ''' <summary>
    ''' Gets the number of accounts in this pool.
    ''' </summary>
    Friend ReadOnly Property Count As Integer
        Get
            Return Me.Accounts.Count
        End Get
    End Property

    ''' <summary>
    ''' Gets the zero-based index of the current active account.
    ''' <para></para>
    ''' A value equal to <see cref="GoodFonAccountPool.Count"/> means all accounts are exhausted.
    ''' </summary>
    Friend ReadOnly Property CurrentIndex As Integer
        Get
            SyncLock Me.syncObject
                Return Me.currentIndexValue
            End SyncLock
        End Get
    End Property

#End Region

#Region " Events "

    ''' <summary>
    ''' Occurs when the daily limit of an account is automatically reset after the cooldown period.
    ''' <para></para>
    ''' The event data is the zero-based index of the account.
    ''' </summary>
    Friend Event AccountLimitReset As EventHandler(Of Integer)

    ''' <summary>
    ''' Occurs when the last available account reaches its daily limit,
    ''' so all the accounts in the pool are exhausted.
    ''' </summary>
    Friend Event AllAccountsExhausted As EventHandler

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Initializes a new instance of the <see cref="GoodFonAccountPool"/> class.
    ''' </summary>
    '''
    ''' <param name="accounts">
    ''' The authenticated accounts, in rotation order. It can be empty.
    ''' </param>
    '''
    ''' <exception cref="ArgumentNullException">
    ''' Thrown when <paramref name="accounts"/> is <see langword="Nothing"/>.
    ''' </exception>
    '''
    ''' <exception cref="ArgumentException">
    ''' Thrown when <paramref name="accounts"/> contains <see langword="Nothing"/> items.
    ''' </exception>
    <DebuggerStepThrough>
    Friend Sub New(accounts As IEnumerable(Of GoodFonAuth))

        Me.New(accounts, GoodFonAccountPool.DefaultLimitResetCooldown, Function() Date.Now)
    End Sub

    ''' <summary>
    ''' Initializes a new instance of the <see cref="GoodFonAccountPool"/> class.
    ''' </summary>
    '''
    ''' <param name="accounts">
    ''' The authenticated accounts, in rotation order. It can be empty.
    ''' </param>
    '''
    ''' <param name="limitResetCooldown">
    ''' The cooldown period after which an exhausted account becomes available again.
    ''' </param>
    '''
    ''' <param name="clock">
    ''' The function that returns the current date and time.
    ''' </param>
    '''
    ''' <exception cref="ArgumentNullException">
    ''' Thrown when <paramref name="accounts"/> or <paramref name="clock"/> is <see langword="Nothing"/>.
    ''' </exception>
    '''
    ''' <exception cref="ArgumentException">
    ''' Thrown when <paramref name="accounts"/> contains <see langword="Nothing"/> or duplicated items.
    ''' </exception>
    '''
    ''' <exception cref="ArgumentOutOfRangeException">
    ''' Thrown when <paramref name="limitResetCooldown"/> is negative.
    ''' </exception>
    <DebuggerStepThrough>
    Friend Sub New(accounts As IEnumerable(Of GoodFonAuth), limitResetCooldown As TimeSpan, clock As Func(Of Date))

        If accounts Is Nothing Then
            Throw New ArgumentNullException(NameOf(accounts))
        End If

        If clock Is Nothing Then
            Throw New ArgumentNullException(NameOf(clock))
        End If

        If limitResetCooldown < TimeSpan.Zero Then
            Throw New ArgumentOutOfRangeException(NameOf(limitResetCooldown), limitResetCooldown, "Cooldown cannot be negative.")
        End If

        Dim list As New List(Of GoodFonAuth)()
        Dim seen As New HashSet(Of GoodFonAuth)()
        For Each auth As GoodFonAuth In accounts
            If auth Is Nothing Then
                Throw New ArgumentException("The accounts collection cannot contain null items.", NameOf(accounts))
            End If

            If Not seen.Add(auth) Then
                Throw New ArgumentException("The accounts collection cannot contain duplicated items.", NameOf(accounts))
            End If

            list.Add(auth)
        Next auth

        Me.Accounts = list.AsReadOnly()
        Me.LimitResetCooldown = limitResetCooldown
        Me.clock = clock
    End Sub

#End Region

#Region " Public Methods "

    ''' <summary>
    ''' Gets the current available account, if any.
    ''' <para></para>
    ''' Before evaluating the current account, any exhausted account whose cooldown period has elapsed
    ''' is reset, and the rotation index moves back to it if it was skipped previously.
    ''' </summary>
    '''
    ''' <returns>
    ''' The current available account, or <see langword="Nothing"/> if the pool is empty
    ''' or all the accounts have reached their daily limit.
    ''' </returns>
    <DebuggerStepThrough>
    Friend Function GetAvailable() As GoodFonAuth

        Dim resetIndices As New List(Of Integer)()
        Dim result As GoodFonAuth = Nothing

        SyncLock Me.syncObject
            If Me.Accounts.Count = 0 Then
                Return Nothing
            End If

            Dim now As Date = Me.clock.Invoke()

            For i As Integer = 0 To Me.Accounts.Count - 1
                Dim auth As GoodFonAuth = Me.Accounts(i)
                Dim limitTime As Date

                If auth.DailyLimitReached AndAlso Me.limitReachedTimes.TryGetValue(auth, limitTime) Then
                    If (now - limitTime) >= Me.LimitResetCooldown Then
                        auth.DailyLimitReached = False
                        Me.limitReachedTimes.Remove(auth)
                        resetIndices.Add(i)

                        ' Return the index point if we skipped past this account previously.
                        If Me.currentIndexValue > i Then
                            Me.currentIndexValue = i
                        End If
                    End If
                End If
            Next i

            If Me.currentIndexValue < Me.Accounts.Count Then
                Dim current As GoodFonAuth = Me.Accounts(Me.currentIndexValue)
                If Not current.DailyLimitReached Then
                    result = current
                End If
            End If
        End SyncLock

        For Each index As Integer In resetIndices
            RaiseEvent AccountLimitReset(Me, index)
        Next index

        Return result
    End Function

    ''' <summary>
    ''' Marks the specified account as exhausted (daily limit reached),
    ''' and rotates to the next non-exhausted account if the specified account is the current active one.
    ''' <para></para>
    ''' Calling this method for an account that is already marked has no effect.
    ''' </summary>
    '''
    ''' <param name="auth">
    ''' The account that received the limit-reached response.
    ''' </param>
    '''
    ''' <exception cref="ArgumentNullException">
    ''' Thrown when <paramref name="auth"/> is <see langword="Nothing"/>.
    ''' </exception>
    '''
    ''' <exception cref="ArgumentException">
    ''' Thrown when <paramref name="auth"/> does not belong to this pool.
    ''' </exception>
    <DebuggerStepThrough>
    Friend Sub MarkLimitReached(auth As GoodFonAuth)

        If auth Is Nothing Then
            Throw New ArgumentNullException(NameOf(auth))
        End If

        If Not Me.Accounts.Contains(auth) Then
            Throw New ArgumentException("The specified account does not belong to this pool.", NameOf(auth))
        End If

        Dim allExhausted As Boolean = False

        SyncLock Me.syncObject
            If auth.DailyLimitReached Then
                Return
            End If

            auth.DailyLimitReached = True

            ' Record the exact time the limit was reached to evaluate the cooldown reset.
            Me.limitReachedTimes(auth) = Me.clock.Invoke()

            ' Only advance the index if the account we just exhausted is still the current active one.
            If (Me.currentIndexValue < Me.Accounts.Count) AndAlso (Me.Accounts(Me.currentIndexValue) Is auth) Then
                Me.currentIndexValue += 1

                ' Skip any following account that was already exhausted while it was not the current one.
                While (Me.currentIndexValue < Me.Accounts.Count) AndAlso Me.Accounts(Me.currentIndexValue).DailyLimitReached
                    Me.currentIndexValue += 1
                End While

                allExhausted = (Me.currentIndexValue >= Me.Accounts.Count)
            End If
        End SyncLock

        If allExhausted Then
            RaiseEvent AllAccountsExhausted(Me, EventArgs.Empty)
        End If
    End Sub

    ''' <summary>
    ''' Gets the one-based position of the specified account in this pool.
    ''' </summary>
    '''
    ''' <param name="auth">
    ''' The account.
    ''' </param>
    '''
    ''' <returns>
    ''' The one-based position of the account.
    ''' </returns>
    '''
    ''' <exception cref="ArgumentNullException">
    ''' Thrown when <paramref name="auth"/> is <see langword="Nothing"/>.
    ''' </exception>
    '''
    ''' <exception cref="ArgumentException">
    ''' Thrown when <paramref name="auth"/> does not belong to this pool.
    ''' </exception>
    <DebuggerStepThrough>
    Friend Function GetPosition(auth As GoodFonAuth) As Integer

        If auth Is Nothing Then
            Throw New ArgumentNullException(NameOf(auth))
        End If

        Dim index As Integer = Me.Accounts.IndexOf(auth)
        If index = -1 Then
            Throw New ArgumentException("The specified account does not belong to this pool.", NameOf(auth))
        End If

        Return index + 1
    End Function

    ''' <summary>
    ''' Releases all the accounts in this pool.
    ''' </summary>
    <DebuggerStepThrough>
    Friend Sub DisposeAccounts()

        For Each auth As GoodFonAuth In Me.Accounts
            auth.Dispose()
        Next auth
    End Sub

#End Region

End Class

#End Region
