#Region " Option Statements "

Option Explicit On
Option Strict On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Diagnostics

#End Region

#Region " AnonymousIpState "

''' <summary>
''' Tracks, in a thread-safe way, whether the anonymous (not logged-in) daily download limit
''' has been reached on the current IP address, and the result of the VPN server rotations.
''' <para></para>
''' It decides when the free wallpapers must be downloaded using the logged-in user-accounts instead
''' (the anonymous limit was reached and the VPN server could not be rotated),
''' and when a new VPN server rotation can be attempted again.
''' </summary>
Friend NotInheritable Class AnonymousIpState

#Region " Fields "

    ''' <summary>
    ''' The default interval to wait after a failed VPN server rotation before attempting a new one (5 minutes).
    ''' </summary>
    Friend Shared ReadOnly DefaultRotationRetryInterval As TimeSpan = TimeSpan.FromMinutes(5)

    ''' <summary>
    ''' Lock object to access the state with thread-safety.
    ''' </summary>
    Private ReadOnly syncObject As New Object()

    ''' <summary>
    ''' The function that returns the current date and time.
    ''' </summary>
    Private ReadOnly clock As Func(Of Date)

    ''' <summary>
    ''' Whether the anonymous daily download limit has been reached on the current IP address.
    ''' </summary>
    Private limitReachedValue As Boolean

    ''' <summary>
    ''' The time of the last failed VPN server rotation, or <see langword="Nothing"/> if the last rotation did not fail.
    ''' </summary>
    Private lastFailedRotationTime As Date?

#End Region

#Region " Properties "

    ''' <summary>
    ''' Gets the interval to wait after a failed VPN server rotation before attempting a new one.
    ''' </summary>
    Friend ReadOnly Property RotationRetryInterval As TimeSpan

    ''' <summary>
    ''' Gets a value indicating whether the anonymous daily download limit has been reached on the current IP address,
    ''' and no successful VPN server rotation has happened since then.
    ''' <para></para>
    ''' While this value is <see langword="True"/>, free wallpapers should be downloaded using the logged-in user-accounts.
    ''' </summary>
    Friend ReadOnly Property IsLimitReached As Boolean
        Get
            SyncLock Me.syncObject
                Return Me.limitReachedValue
            End SyncLock
        End Get
    End Property

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Initializes a new instance of the <see cref="AnonymousIpState"/> class,
    ''' using <see cref="AnonymousIpState.DefaultRotationRetryInterval"/> and the system clock.
    ''' </summary>
    <DebuggerStepThrough>
    Friend Sub New()

        Me.New(AnonymousIpState.DefaultRotationRetryInterval, Function() Date.Now)
    End Sub

    ''' <summary>
    ''' Initializes a new instance of the <see cref="AnonymousIpState"/> class.
    ''' </summary>
    '''
    ''' <param name="rotationRetryInterval">
    ''' The interval to wait after a failed VPN server rotation before attempting a new one.
    ''' </param>
    '''
    ''' <param name="clock">
    ''' The function that returns the current date and time.
    ''' </param>
    '''
    ''' <exception cref="ArgumentOutOfRangeException">
    ''' Thrown when <paramref name="rotationRetryInterval"/> is negative.
    ''' </exception>
    '''
    ''' <exception cref="ArgumentNullException">
    ''' Thrown when <paramref name="clock"/> is <see langword="Nothing"/>.
    ''' </exception>
    <DebuggerStepThrough>
    Friend Sub New(rotationRetryInterval As TimeSpan, clock As Func(Of Date))

        If rotationRetryInterval < TimeSpan.Zero Then
            Throw New ArgumentOutOfRangeException(NameOf(rotationRetryInterval), rotationRetryInterval, "Interval cannot be negative.")
        End If

        If clock Is Nothing Then
            Throw New ArgumentNullException(NameOf(clock))
        End If

        Me.RotationRetryInterval = rotationRetryInterval
        Me.clock = clock
    End Sub

#End Region

#Region " Public Methods "

    ''' <summary>
    ''' Reports that the website returned the anonymous daily download limit page on the current IP address.
    ''' </summary>
    <DebuggerStepThrough>
    Friend Sub ReportLimitReached()

        SyncLock Me.syncObject
            Me.limitReachedValue = True
        End SyncLock
    End Sub

    ''' <summary>
    ''' Reports a successful VPN server rotation: the new IP address has its own anonymous download limit.
    ''' </summary>
    <DebuggerStepThrough>
    Friend Sub ReportRotationSucceeded()

        SyncLock Me.syncObject
            Me.limitReachedValue = False
            Me.lastFailedRotationTime = Nothing
        End SyncLock
    End Sub

    ''' <summary>
    ''' Reports a failed VPN server rotation: the IP address did not change.
    ''' </summary>
    <DebuggerStepThrough>
    Friend Sub ReportRotationFailed()

        SyncLock Me.syncObject
            Me.lastFailedRotationTime = Me.clock.Invoke()
        End SyncLock
    End Sub

    ''' <summary>
    ''' Determines whether a VPN server rotation can be attempted now.
    ''' <para></para>
    ''' A rotation can always be attempted, unless the last rotation failed less than
    ''' <see cref="AnonymousIpState.RotationRetryInterval"/> ago
    ''' (to avoid repeating a slow and failing rotation on every page or wallpaper).
    ''' </summary>
    '''
    ''' <returns>
    ''' <see langword="True"/> if a VPN server rotation can be attempted now; otherwise, <see langword="False"/>.
    ''' </returns>
    <DebuggerStepThrough>
    Friend Function CanAttemptRotation() As Boolean

        SyncLock Me.syncObject
            If Not Me.lastFailedRotationTime.HasValue Then
                Return True
            End If

            Return (Me.clock.Invoke() - Me.lastFailedRotationTime.Value) >= Me.RotationRetryInterval
        End SyncLock
    End Function

#End Region

End Class

#End Region
