#Region " Option Statements "

Option Explicit On
Option Strict On
Option Infer Off

#End Region

#Region " EtaEstimatorTests "

''' <summary>
''' Unit tests for the <see cref="EtaEstimator"/> class.
''' </summary>
<TestClass>
Public NotInheritable Class EtaEstimatorTests

#Region " Fields "

    ''' <summary>
    ''' The fake elapsed time returned by the test time source.
    ''' </summary>
    Private fakeElapsed As TimeSpan

#End Region

#Region " Test Initialization / Helpers "

    ''' <summary>
    ''' Resets the fake time source before each test.
    ''' </summary>
    <TestInitialize>
    Public Sub TestInitialize()

        Me.fakeElapsed = TimeSpan.Zero
    End Sub

    ''' <summary>
    ''' Creates a new estimator using the fake time source.
    ''' </summary>
    '''
    ''' <param name="windowSize">
    ''' The window size.
    ''' </param>
    '''
    ''' <returns>
    ''' The created estimator, with its first item already started.
    ''' </returns>
    Private Function CreateStartedEstimator(windowSize As Integer) As EtaEstimator

        Dim estimator As New EtaEstimator(windowSize, Function() Me.fakeElapsed)
        estimator.StartItem()
        Return estimator
    End Function

    ''' <summary>
    ''' Advances the fake time source and completes the current item.
    ''' </summary>
    '''
    ''' <param name="estimator">
    ''' The estimator.
    ''' </param>
    '''
    ''' <param name="seconds">
    ''' The duration of the item, in seconds.
    ''' </param>
    Private Sub CompleteItemAfter(estimator As EtaEstimator, seconds As Double)

        Me.fakeElapsed += TimeSpan.FromSeconds(seconds)
        estimator.CompleteItem()
    End Sub

#End Region

#Region " GetEta / GetEtaText "

    ''' <summary>
    ''' Verifies that the ETA is not available until an item is completed.
    ''' </summary>
    <TestMethod>
    Public Sub GetEta_NoCompletedItems_ReturnsNothing()

        Dim estimator As EtaEstimator = Me.CreateStartedEstimator(20)

        Assert.IsNull(estimator.GetEta(10))
        Assert.IsNull(estimator.AverageItemDuration)
        Assert.AreEqual(EtaEstimator.CalculatingText, estimator.GetEtaText(10))
        Assert.AreEqual(0, estimator.CompletedCount)
    End Sub

    ''' <summary>
    ''' Verifies that the ETA is the average item duration multiplied by the remaining items.
    ''' </summary>
    <TestMethod>
    Public Sub GetEta_CompletedItems_ReturnsAverageTimesRemaining()

        Dim estimator As EtaEstimator = Me.CreateStartedEstimator(20)
        Me.CompleteItemAfter(estimator, 2)
        Me.CompleteItemAfter(estimator, 4)

        Assert.AreEqual(2, estimator.CompletedCount)
        Assert.AreEqual(TimeSpan.FromSeconds(3), estimator.AverageItemDuration)
        Assert.AreEqual(TimeSpan.FromSeconds(30), estimator.GetEta(10))
        Assert.AreEqual("00:00:30", estimator.GetEtaText(10))
        Assert.AreEqual(TimeSpan.Zero, estimator.GetEta(0))
    End Sub

    ''' <summary>
    ''' Verifies that only the most recent items are averaged, so the ETA adapts to speed changes
    ''' (e.g., from fast "already downloaded" pages to slow pages with new wallpapers).
    ''' </summary>
    <TestMethod>
    Public Sub GetEta_SlidingWindow_AdaptsToSpeedChanges()

        Dim estimator As EtaEstimator = Me.CreateStartedEstimator(3)
        For i As Integer = 1 To 10
            Me.CompleteItemAfter(estimator, 1) ' Fast pages.
        Next i
        For i As Integer = 1 To 3
            Me.CompleteItemAfter(estimator, 60) ' Slow pages.
        Next i

        Assert.AreEqual(13, estimator.CompletedCount)
        Assert.AreEqual(TimeSpan.FromSeconds(60), estimator.AverageItemDuration)
        Assert.AreEqual(TimeSpan.FromMinutes(100), estimator.GetEta(100))
    End Sub

    ''' <summary>
    ''' Verifies that each completed item starts the next one (durations are consecutive, not cumulative).
    ''' </summary>
    <TestMethod>
    Public Sub CompleteItem_StartsNextItemAutomatically()

        Dim estimator As EtaEstimator = Me.CreateStartedEstimator(20)
        Me.CompleteItemAfter(estimator, 10)
        Me.CompleteItemAfter(estimator, 10)
        Me.CompleteItemAfter(estimator, 10)

        Assert.AreEqual(TimeSpan.FromSeconds(10), estimator.AverageItemDuration)
    End Sub

    ''' <summary>
    ''' Verifies that the time spent before <see cref="EtaEstimator.StartItem()"/> is not counted.
    ''' </summary>
    <TestMethod>
    Public Sub StartItem_ResetsTheCurrentItemStart()

        Dim estimator As New EtaEstimator(20, Function() Me.fakeElapsed)
        Me.fakeElapsed = TimeSpan.FromHours(1)
        estimator.StartItem()

        Me.CompleteItemAfter(estimator, 5)

        Assert.AreEqual(TimeSpan.FromSeconds(5), estimator.AverageItemDuration)
    End Sub

    ''' <summary>
    ''' Verifies that a time source going backwards never produces negative durations.
    ''' </summary>
    <TestMethod>
    Public Sub CompleteItem_TimeSourceGoesBackwards_RecordsZeroDuration()

        Dim estimator As EtaEstimator = Me.CreateStartedEstimator(20)
        Me.fakeElapsed = TimeSpan.FromSeconds(-10)
        estimator.CompleteItem()

        Assert.AreEqual(TimeSpan.Zero, estimator.AverageItemDuration)
    End Sub

    ''' <summary>
    ''' Verifies the argument and state validation.
    ''' </summary>
    <TestMethod>
    Public Sub InvalidUsage_Throws()

        Assert.ThrowsExactly(Of ArgumentOutOfRangeException)(Function() New EtaEstimator(0, Function() TimeSpan.Zero))
        Assert.ThrowsExactly(Of ArgumentNullException)(Function() New EtaEstimator(1, Nothing))
        Dim notStarted As New EtaEstimator(1, Function() TimeSpan.Zero)
        Assert.ThrowsExactly(Of InvalidOperationException)(Sub() notStarted.CompleteItem())
        Assert.ThrowsExactly(Of ArgumentOutOfRangeException)(Sub() Me.CreateStartedEstimator(1).GetEta(-1))
    End Sub

    ''' <summary>
    ''' Verifies that the default constructor uses a real (monotonic) time source and the default window size.
    ''' </summary>
    <TestMethod>
    Public Sub DefaultConstructor_UsesDefaultWindowSize()

        Dim estimator As New EtaEstimator()
        estimator.StartItem()
        estimator.CompleteItem()

        Assert.AreEqual(EtaEstimator.DefaultWindowSize, estimator.WindowSize)
        Assert.IsTrue(estimator.AverageItemDuration.Value >= TimeSpan.Zero)
    End Sub

#End Region

#Region " FormatDuration "

    ''' <summary>
    ''' Verifies the "HH:mm:ss" format, rounding fractions of a second up.
    ''' </summary>
    <TestMethod>
    Public Sub FormatDuration_VariousDurations_FormatsAsHoursMinutesSeconds()

        Assert.AreEqual("00:00:00", EtaEstimator.FormatDuration(TimeSpan.Zero))
        Assert.AreEqual("00:00:01", EtaEstimator.FormatDuration(TimeSpan.FromMilliseconds(1)))
        Assert.AreEqual("00:01:05", EtaEstimator.FormatDuration(TimeSpan.FromSeconds(65)))
        Assert.AreEqual("02:03:04", EtaEstimator.FormatDuration(New TimeSpan(2, 3, 4)))
    End Sub

    ''' <summary>
    ''' Verifies that durations of 24 hours or more show the total hours instead of wrapping around.
    ''' </summary>
    <TestMethod>
    Public Sub FormatDuration_MoreThanOneDay_ShowsTotalHours()

        Assert.AreEqual("26:15:00", EtaEstimator.FormatDuration(New TimeSpan(1, 2, 15, 0)))
        Assert.AreEqual("100:00:00", EtaEstimator.FormatDuration(TimeSpan.FromHours(100)))
    End Sub

    ''' <summary>
    ''' Verifies that negative durations throw <see cref="ArgumentOutOfRangeException"/>.
    ''' </summary>
    <TestMethod>
    Public Sub FormatDuration_NegativeDuration_Throws()

        Assert.ThrowsExactly(Of ArgumentOutOfRangeException)(Function() EtaEstimator.FormatDuration(TimeSpan.FromSeconds(-1)))
    End Sub

#End Region

End Class

#End Region
