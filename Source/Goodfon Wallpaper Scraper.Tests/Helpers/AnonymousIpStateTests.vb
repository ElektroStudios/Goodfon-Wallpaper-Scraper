#Region " Option Statements "

Option Explicit On
Option Strict On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Threading.Tasks

#End Region

#Region " AnonymousIpStateTests "

''' <summary>
''' Unit tests for the <see cref="AnonymousIpState"/> class.
''' </summary>
<TestClass>
Public NotInheritable Class AnonymousIpStateTests

#Region " Fields "

    ''' <summary>
    ''' The VPN rotation retry interval used by the tests.
    ''' </summary>
    Private Shared ReadOnly TestRetryInterval As TimeSpan = TimeSpan.FromMinutes(5)

    ''' <summary>
    ''' The fake current date and time returned by the test clock.
    ''' </summary>
    Private fakeNow As Date

#End Region

#Region " Test Initialization / Helpers "

    ''' <summary>
    ''' Initializes the fake clock before each test.
    ''' </summary>
    <TestInitialize>
    Public Sub TestInitialize()

        Me.fakeNow = New Date(2026, 1, 1, 12, 0, 0, DateTimeKind.Local)
    End Sub

    ''' <summary>
    ''' Creates a new state using the test retry interval and the fake clock.
    ''' </summary>
    '''
    ''' <returns>
    ''' The created state.
    ''' </returns>
    Private Function CreateState() As AnonymousIpState

        Return New AnonymousIpState(AnonymousIpStateTests.TestRetryInterval, Function() Me.fakeNow)
    End Function

#End Region

#Region " Tests "

    ''' <summary>
    ''' Verifies the initial state: limit not reached, and VPN rotations allowed.
    ''' </summary>
    <TestMethod>
    Public Sub NewInstance_LimitNotReachedAndRotationAllowed()

        Dim state As AnonymousIpState = Me.CreateState()

        Assert.IsFalse(state.IsLimitReached)
        Assert.IsTrue(state.CanAttemptRotation())
        Assert.AreEqual(TimeSpan.FromMinutes(5), AnonymousIpState.DefaultRotationRetryInterval)
        Assert.AreEqual(AnonymousIpState.DefaultRotationRetryInterval, New AnonymousIpState().RotationRetryInterval)
    End Sub

    ''' <summary>
    ''' Verifies that reaching the anonymous limit switches the state, so free wallpapers are downloaded with accounts.
    ''' </summary>
    <TestMethod>
    Public Sub ReportLimitReached_SetsLimitReached()

        Dim state As AnonymousIpState = Me.CreateState()

        state.ReportLimitReached()

        Assert.IsTrue(state.IsLimitReached)
        Assert.IsTrue(state.CanAttemptRotation(), "A rotation must be allowed right after reaching the limit.")
    End Sub

    ''' <summary>
    ''' Verifies that a failed VPN rotation keeps the limit reached (accounts keep being used),
    ''' and blocks new rotations until the retry interval elapses.
    ''' </summary>
    <TestMethod>
    Public Sub ReportRotationFailed_KeepsLimitAndThrottlesRotations()

        Dim state As AnonymousIpState = Me.CreateState()
        state.ReportLimitReached()

        state.ReportRotationFailed()

        Assert.IsTrue(state.IsLimitReached)
        Assert.IsFalse(state.CanAttemptRotation())

        Me.fakeNow = Me.fakeNow + AnonymousIpStateTests.TestRetryInterval - TimeSpan.FromSeconds(1)
        Assert.IsFalse(state.CanAttemptRotation())

        Me.fakeNow = Me.fakeNow + TimeSpan.FromSeconds(1)
        Assert.IsTrue(state.CanAttemptRotation())
        Assert.IsTrue(state.IsLimitReached)
    End Sub

    ''' <summary>
    ''' Verifies that a successful VPN rotation resets the limit (anonymous downloads resume) and the throttle.
    ''' </summary>
    <TestMethod>
    Public Sub ReportRotationSucceeded_ResetsLimitAndThrottle()

        Dim state As AnonymousIpState = Me.CreateState()
        state.ReportLimitReached()
        state.ReportRotationFailed()

        Me.fakeNow = Me.fakeNow + AnonymousIpStateTests.TestRetryInterval
        state.ReportRotationSucceeded()

        Assert.IsFalse(state.IsLimitReached)
        Assert.IsTrue(state.CanAttemptRotation())
    End Sub

    ''' <summary>
    ''' Verifies that a failed rotation without reaching the limit (e.g., the initial rotation)
    ''' throttles the rotations but does not switch to accounts.
    ''' </summary>
    <TestMethod>
    Public Sub ReportRotationFailed_WithoutLimit_DoesNotSetLimitReached()

        Dim state As AnonymousIpState = Me.CreateState()

        state.ReportRotationFailed()

        Assert.IsFalse(state.IsLimitReached)
        Assert.IsFalse(state.CanAttemptRotation())
    End Sub

    ''' <summary>
    ''' Verifies that a new failure after the retry interval restarts the throttle.
    ''' </summary>
    <TestMethod>
    Public Sub ReportRotationFailed_Again_RestartsThrottle()

        Dim state As AnonymousIpState = Me.CreateState()
        state.ReportRotationFailed()

        Me.fakeNow = Me.fakeNow + AnonymousIpStateTests.TestRetryInterval
        Assert.IsTrue(state.CanAttemptRotation())
        state.ReportRotationFailed()

        Assert.IsFalse(state.CanAttemptRotation())
    End Sub

    ''' <summary>
    ''' Verifies that a zero retry interval never throttles the rotations.
    ''' </summary>
    <TestMethod>
    Public Sub CanAttemptRotation_ZeroInterval_NeverThrottles()

        Dim state As New AnonymousIpState(TimeSpan.Zero, Function() Me.fakeNow)

        state.ReportRotationFailed()

        Assert.IsTrue(state.CanAttemptRotation())
    End Sub

    ''' <summary>
    ''' Verifies that concurrent reports do not throw and leave a consistent state.
    ''' </summary>
    <TestMethod>
    Public Sub ConcurrentReports_AreThreadSafe()

        Dim state As AnonymousIpState = Me.CreateState()

        Parallel.For(0, 1000,
            Sub(i As Integer)
                state.ReportLimitReached()
                state.ReportRotationFailed()
                Dim unused As Boolean = state.CanAttemptRotation() AndAlso state.IsLimitReached
            End Sub)

        Assert.IsTrue(state.IsLimitReached)
        Assert.IsFalse(state.CanAttemptRotation())
    End Sub

    ''' <summary>
    ''' Verifies the constructor argument validation.
    ''' </summary>
    <TestMethod>
    Public Sub Constructor_InvalidArguments_Throws()

        Assert.ThrowsExactly(Of ArgumentOutOfRangeException)(Function() New AnonymousIpState(TimeSpan.FromSeconds(-1), Function() Date.Now))
        Assert.ThrowsExactly(Of ArgumentNullException)(Function() New AnonymousIpState(TimeSpan.Zero, Nothing))
    End Sub

#End Region

End Class

#End Region
