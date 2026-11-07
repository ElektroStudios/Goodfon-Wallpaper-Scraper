#Region " Option Statements "

Option Explicit On
Option Strict On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Collections.Generic
Imports System.Linq
Imports System.Threading
Imports System.Threading.Tasks

#End Region

#Region " GoodFonAccountPoolTests "

''' <summary>
''' Unit tests for the <see cref="GoodFonAccountPool"/> class.
''' </summary>
<TestClass>
Public NotInheritable Class GoodFonAccountPoolTests

#Region " Fields "

    ''' <summary>
    ''' The cooldown period used by the tests.
    ''' </summary>
    Private Shared ReadOnly TestCooldown As TimeSpan = TimeSpan.FromHours(24)

    ''' <summary>
    ''' The accounts created by the current test, released after each test.
    ''' </summary>
    Private ReadOnly createdAccounts As New List(Of GoodFonAuth)()

    ''' <summary>
    ''' The fake current date and time returned by the test clock.
    ''' </summary>
    Private fakeNow As Date

#End Region

#Region " Test Initialization / Cleanup "

    ''' <summary>
    ''' Initializes the fake clock before each test.
    ''' </summary>
    <TestInitialize>
    Public Sub TestInitialize()

        Me.fakeNow = New Date(2026, 1, 1, 12, 0, 0, DateTimeKind.Local)
    End Sub

    ''' <summary>
    ''' Releases the accounts created by the current test.
    ''' </summary>
    <TestCleanup>
    Public Sub TestCleanup()

        For Each auth As GoodFonAuth In Me.createdAccounts
            auth.Dispose()
        Next auth
        Me.createdAccounts.Clear()
    End Sub

#End Region

#Region " Test Helpers "

    ''' <summary>
    ''' Creates the specified number of (not logged-in) accounts.
    ''' </summary>
    '''
    ''' <param name="count">
    ''' The number of accounts to create.
    ''' </param>
    '''
    ''' <returns>
    ''' The created accounts.
    ''' </returns>
    Private Function CreateAccounts(count As Integer) As GoodFonAuth()

        Dim accounts As GoodFonAuth() = Enumerable.Range(0, count).Select(Function(i As Integer) New GoodFonAuth()).ToArray()
        Me.createdAccounts.AddRange(accounts)
        Return accounts
    End Function

    ''' <summary>
    ''' Creates a pool with the specified accounts, using the test cooldown and the fake clock.
    ''' </summary>
    '''
    ''' <param name="accounts">
    ''' The accounts.
    ''' </param>
    '''
    ''' <returns>
    ''' The created pool.
    ''' </returns>
    Private Function CreatePool(accounts As IEnumerable(Of GoodFonAuth)) As GoodFonAccountPool

        Return New GoodFonAccountPool(accounts, GoodFonAccountPoolTests.TestCooldown, Function() Me.fakeNow)
    End Function

#End Region

#Region " Constructor "

    ''' <summary>
    ''' Verifies the default cooldown period (24 hours + 2 minutes).
    ''' </summary>
    <TestMethod>
    Public Sub DefaultLimitResetCooldown_Is24HoursAnd2Minutes()

        Assert.AreEqual(TimeSpan.FromHours(24) + TimeSpan.FromMinutes(2), GoodFonAccountPool.DefaultLimitResetCooldown)
        Assert.AreEqual(GoodFonAccountPool.DefaultLimitResetCooldown, New GoodFonAccountPool(Me.CreateAccounts(1)).LimitResetCooldown)
    End Sub

    ''' <summary>
    ''' Verifies that the pool keeps the accounts in the specified order.
    ''' </summary>
    <TestMethod>
    Public Sub Constructor_ValidAccounts_KeepsOrder()

        Dim accounts As GoodFonAuth() = Me.CreateAccounts(3)
        Dim pool As GoodFonAccountPool = Me.CreatePool(accounts)

        Assert.AreEqual(3, pool.Count)
        CollectionAssert.AreEqual(accounts, pool.Accounts.ToArray())
        Assert.AreEqual(0, pool.CurrentIndex)
    End Sub

    ''' <summary>
    ''' Verifies the constructor argument validation.
    ''' </summary>
    <TestMethod>
    Public Sub Constructor_InvalidArguments_Throws()

        Dim accounts As GoodFonAuth() = Me.CreateAccounts(1)

        Assert.ThrowsExactly(Of ArgumentNullException)(Function() New GoodFonAccountPool(Nothing))
        Assert.ThrowsExactly(Of ArgumentNullException)(Function() New GoodFonAccountPool(accounts, TimeSpan.Zero, Nothing))
        Assert.ThrowsExactly(Of ArgumentOutOfRangeException)(Function() New GoodFonAccountPool(accounts, TimeSpan.FromSeconds(-1), Function() Date.Now))
        Assert.ThrowsExactly(Of ArgumentException)(Function() New GoodFonAccountPool(New GoodFonAuth() {accounts(0), Nothing}))
        Assert.ThrowsExactly(Of ArgumentException)(Function() New GoodFonAccountPool(New GoodFonAuth() {accounts(0), accounts(0)}))
    End Sub

    ''' <summary>
    ''' Verifies that the pool takes a snapshot of the source collection.
    ''' </summary>
    <TestMethod>
    Public Sub Constructor_SourceCollectionModifiedLater_PoolIsNotAffected()

        Dim source As New List(Of GoodFonAuth)(Me.CreateAccounts(2))
        Dim pool As GoodFonAccountPool = Me.CreatePool(source)

        source.Clear()

        Assert.AreEqual(2, pool.Count)
    End Sub

#End Region

#Region " GetAvailable "

    ''' <summary>
    ''' Verifies that an empty pool never returns an account.
    ''' </summary>
    <TestMethod>
    Public Sub GetAvailable_EmptyPool_ReturnsNothing()

        Dim pool As GoodFonAccountPool = Me.CreatePool(Array.Empty(Of GoodFonAuth)())

        Assert.IsNull(pool.GetAvailable())
        Assert.AreEqual(0, pool.Count)
        Assert.AreEqual(0, pool.CurrentIndex)
    End Sub

    ''' <summary>
    ''' Verifies that the first account is returned initially, and repeatedly while it is not exhausted.
    ''' </summary>
    <TestMethod>
    Public Sub GetAvailable_FreshPool_ReturnsFirstAccountRepeatedly()

        Dim accounts As GoodFonAuth() = Me.CreateAccounts(2)
        Dim pool As GoodFonAccountPool = Me.CreatePool(accounts)

        Assert.AreSame(accounts(0), pool.GetAvailable())
        Assert.AreSame(accounts(0), pool.GetAvailable())
    End Sub

#End Region

#Region " MarkLimitReached "

    ''' <summary>
    ''' Verifies that marking the current account rotates to the next one.
    ''' </summary>
    <TestMethod>
    Public Sub MarkLimitReached_CurrentAccount_RotatesToNextAccount()

        Dim accounts As GoodFonAuth() = Me.CreateAccounts(3)
        Dim pool As GoodFonAccountPool = Me.CreatePool(accounts)

        pool.MarkLimitReached(accounts(0))

        Assert.IsTrue(accounts(0).DailyLimitReached)
        Assert.AreEqual(1, pool.CurrentIndex)
        Assert.AreSame(accounts(1), pool.GetAvailable())
    End Sub

    ''' <summary>
    ''' Verifies that exhausting every account returns <see langword="Nothing"/>
    ''' and raises <see cref="GoodFonAccountPool.AllAccountsExhausted"/> exactly once.
    ''' </summary>
    <TestMethod>
    Public Sub MarkLimitReached_AllAccounts_ReturnsNothingAndRaisesEventOnce()

        Dim accounts As GoodFonAuth() = Me.CreateAccounts(2)
        Dim pool As GoodFonAccountPool = Me.CreatePool(accounts)
        Dim exhaustedCount As Integer = 0
        AddHandler pool.AllAccountsExhausted, Sub(sender As Object, e As EventArgs) exhaustedCount += 1

        pool.MarkLimitReached(accounts(0))
        Assert.AreEqual(0, exhaustedCount)

        pool.MarkLimitReached(accounts(1))
        pool.MarkLimitReached(accounts(1)) ' Already marked: no effect.

        Assert.AreEqual(1, exhaustedCount)
        Assert.AreEqual(2, pool.CurrentIndex)
        Assert.IsNull(pool.GetAvailable())
    End Sub

    ''' <summary>
    ''' Verifies that marking an already exhausted account has no effect (the index advances only once).
    ''' </summary>
    <TestMethod>
    Public Sub MarkLimitReached_SameAccountTwice_AdvancesOnlyOnce()

        Dim accounts As GoodFonAuth() = Me.CreateAccounts(3)
        Dim pool As GoodFonAccountPool = Me.CreatePool(accounts)

        pool.MarkLimitReached(accounts(0))
        pool.MarkLimitReached(accounts(0))

        Assert.AreEqual(1, pool.CurrentIndex)
        Assert.AreSame(accounts(1), pool.GetAvailable())
    End Sub

    ''' <summary>
    ''' Verifies that marking an account that is not the current one does not change the current account.
    ''' </summary>
    <TestMethod>
    Public Sub MarkLimitReached_NonCurrentAccount_DoesNotRotate()

        Dim accounts As GoodFonAuth() = Me.CreateAccounts(3)
        Dim pool As GoodFonAccountPool = Me.CreatePool(accounts)

        pool.MarkLimitReached(accounts(1))

        Assert.IsTrue(accounts(1).DailyLimitReached)
        Assert.AreEqual(0, pool.CurrentIndex)
        Assert.AreSame(accounts(0), pool.GetAvailable())
    End Sub

    ''' <summary>
    ''' Regression test: when the current account is exhausted, the rotation must skip any following account
    ''' that was already exhausted while it was not the current one, instead of stopping on it
    ''' (which made the pool return <see langword="Nothing"/> while other accounts were still available).
    ''' </summary>
    <TestMethod>
    Public Sub MarkLimitReached_NextAccountAlreadyExhausted_SkipsToNextAvailableAccount()

        Dim accounts As GoodFonAuth() = Me.CreateAccounts(3)
        Dim pool As GoodFonAccountPool = Me.CreatePool(accounts)

        pool.MarkLimitReached(accounts(1)) ' Non-current account exhausted first.
        pool.MarkLimitReached(accounts(0)) ' Current account exhausted.

        Assert.AreEqual(2, pool.CurrentIndex)
        Assert.AreSame(accounts(2), pool.GetAvailable())
    End Sub

    ''' <summary>
    ''' Verifies that skipping exhausted accounts up to the end raises <see cref="GoodFonAccountPool.AllAccountsExhausted"/>.
    ''' </summary>
    <TestMethod>
    Public Sub MarkLimitReached_RemainingAccountsAlreadyExhausted_RaisesAllAccountsExhausted()

        Dim accounts As GoodFonAuth() = Me.CreateAccounts(3)
        Dim pool As GoodFonAccountPool = Me.CreatePool(accounts)
        Dim exhaustedRaised As Boolean = False
        AddHandler pool.AllAccountsExhausted, Sub(sender As Object, e As EventArgs) exhaustedRaised = True

        pool.MarkLimitReached(accounts(2))
        pool.MarkLimitReached(accounts(1))
        Assert.IsFalse(exhaustedRaised)

        pool.MarkLimitReached(accounts(0))

        Assert.IsTrue(exhaustedRaised)
        Assert.AreEqual(3, pool.CurrentIndex)
        Assert.IsNull(pool.GetAvailable())
    End Sub

    ''' <summary>
    ''' Verifies the argument validation.
    ''' </summary>
    <TestMethod>
    Public Sub MarkLimitReached_InvalidArguments_Throws()

        Dim pool As GoodFonAccountPool = Me.CreatePool(Me.CreateAccounts(1))
        Dim foreignAccount As GoodFonAuth = Me.CreateAccounts(1)(0)

        Assert.ThrowsExactly(Of ArgumentNullException)(Sub() pool.MarkLimitReached(Nothing))
        Assert.ThrowsExactly(Of ArgumentException)(Sub() pool.MarkLimitReached(foreignAccount))
        Assert.IsFalse(foreignAccount.DailyLimitReached)
    End Sub

    ''' <summary>
    ''' Verifies that concurrent calls for the same account mark it only once,
    ''' advancing the index once and raising <see cref="GoodFonAccountPool.AllAccountsExhausted"/> once.
    ''' </summary>
    <TestMethod>
    Public Sub MarkLimitReached_ConcurrentCalls_AreThreadSafe()

        Dim accounts As GoodFonAuth() = Me.CreateAccounts(1)
        Dim pool As GoodFonAccountPool = Me.CreatePool(accounts)
        Dim exhaustedCount As Integer = 0
        AddHandler pool.AllAccountsExhausted, Sub(sender As Object, e As EventArgs) Interlocked.Increment(exhaustedCount)

        Parallel.For(0, 64, Sub(i As Integer) pool.MarkLimitReached(accounts(0)))

        Assert.AreEqual(1, exhaustedCount)
        Assert.AreEqual(1, pool.CurrentIndex)
    End Sub

#End Region

#Region " Cooldown Reset "

    ''' <summary>
    ''' Verifies that an exhausted account stays unavailable until the cooldown period elapses,
    ''' and then it is automatically reset and <see cref="GoodFonAccountPool.AccountLimitReset"/> is raised.
    ''' </summary>
    <TestMethod>
    Public Sub GetAvailable_CooldownElapsed_ResetsAccountAndRaisesEvent()

        Dim accounts As GoodFonAuth() = Me.CreateAccounts(1)
        Dim pool As GoodFonAccountPool = Me.CreatePool(accounts)
        Dim resetIndices As New List(Of Integer)()
        AddHandler pool.AccountLimitReset, Sub(sender As Object, index As Integer) resetIndices.Add(index)

        pool.MarkLimitReached(accounts(0))

        Me.fakeNow = Me.fakeNow + GoodFonAccountPoolTests.TestCooldown - TimeSpan.FromSeconds(1)
        Assert.IsNull(pool.GetAvailable())
        Assert.AreEqual(0, resetIndices.Count)

        Me.fakeNow = Me.fakeNow + TimeSpan.FromSeconds(1)
        Assert.AreSame(accounts(0), pool.GetAvailable())
        Assert.IsFalse(accounts(0).DailyLimitReached)
        CollectionAssert.AreEqual(New Integer() {0}, resetIndices)
        Assert.AreEqual(0, pool.CurrentIndex)
    End Sub

    ''' <summary>
    ''' Verifies that the reset of a previously skipped account moves the rotation index back to it.
    ''' </summary>
    <TestMethod>
    Public Sub GetAvailable_SkippedAccountReset_MovesIndexBack()

        Dim accounts As GoodFonAuth() = Me.CreateAccounts(3)
        Dim pool As GoodFonAccountPool = Me.CreatePool(accounts)

        pool.MarkLimitReached(accounts(0))
        Me.fakeNow = Me.fakeNow.AddHours(1)
        pool.MarkLimitReached(accounts(1))
        Assert.AreEqual(2, pool.CurrentIndex)

        ' Only the first account cooldown has elapsed.
        Me.fakeNow = Me.fakeNow + GoodFonAccountPoolTests.TestCooldown - TimeSpan.FromHours(1)

        Assert.AreSame(accounts(0), pool.GetAvailable())
        Assert.AreEqual(0, pool.CurrentIndex)
        Assert.IsTrue(accounts(1).DailyLimitReached)
    End Sub

    ''' <summary>
    ''' Verifies that a reset account becomes exhaustible again, restarting its cooldown.
    ''' </summary>
    <TestMethod>
    Public Sub MarkLimitReached_AfterReset_RestartsCooldown()

        Dim accounts As GoodFonAuth() = Me.CreateAccounts(1)
        Dim pool As GoodFonAccountPool = Me.CreatePool(accounts)

        pool.MarkLimitReached(accounts(0))
        Me.fakeNow = Me.fakeNow + GoodFonAccountPoolTests.TestCooldown
        Assert.AreSame(accounts(0), pool.GetAvailable())

        pool.MarkLimitReached(accounts(0))
        Assert.IsNull(pool.GetAvailable())

        Me.fakeNow = Me.fakeNow + GoodFonAccountPoolTests.TestCooldown
        Assert.AreSame(accounts(0), pool.GetAvailable())
    End Sub

    ''' <summary>
    ''' Verifies that an account manually flagged as exhausted (without being marked through the pool)
    ''' is never reset automatically, because there is no recorded limit time.
    ''' </summary>
    <TestMethod>
    Public Sub GetAvailable_AccountFlaggedOutsidePool_IsNeverReset()

        Dim accounts As GoodFonAuth() = Me.CreateAccounts(1)
        Dim pool As GoodFonAccountPool = Me.CreatePool(accounts)

        accounts(0).DailyLimitReached = True
        Me.fakeNow = Me.fakeNow.AddDays(30)

        Assert.IsNull(pool.GetAvailable())
        Assert.IsTrue(accounts(0).DailyLimitReached)
    End Sub

#End Region

#Region " GetPosition / DisposeAccounts "

    ''' <summary>
    ''' Verifies that the account position is one-based.
    ''' </summary>
    <TestMethod>
    Public Sub GetPosition_PoolAccounts_ReturnsOneBasedPosition()

        Dim accounts As GoodFonAuth() = Me.CreateAccounts(3)
        Dim pool As GoodFonAccountPool = Me.CreatePool(accounts)

        Assert.AreEqual(1, pool.GetPosition(accounts(0)))
        Assert.AreEqual(2, pool.GetPosition(accounts(1)))
        Assert.AreEqual(3, pool.GetPosition(accounts(2)))
    End Sub

    ''' <summary>
    ''' Verifies the argument validation.
    ''' </summary>
    <TestMethod>
    Public Sub GetPosition_InvalidArguments_Throws()

        Dim pool As GoodFonAccountPool = Me.CreatePool(Me.CreateAccounts(1))
        Dim foreignAccount As GoodFonAuth = Me.CreateAccounts(1)(0)

        Assert.ThrowsExactly(Of ArgumentNullException)(Sub() pool.GetPosition(Nothing))
        Assert.ThrowsExactly(Of ArgumentException)(Sub() pool.GetPosition(foreignAccount))
    End Sub

    ''' <summary>
    ''' Verifies that disposing the accounts can be done on empty pools and repeated safely.
    ''' </summary>
    <TestMethod>
    Public Sub DisposeAccounts_CalledTwice_DoesNotThrow()

        Dim pool As GoodFonAccountPool = Me.CreatePool(Me.CreateAccounts(2))

        pool.DisposeAccounts()
        pool.DisposeAccounts()
        Me.CreatePool(Array.Empty(Of GoodFonAuth)()).DisposeAccounts()
    End Sub

#End Region

End Class

#End Region
