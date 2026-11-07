#Region " Option Statements "

Option Explicit On
Option Strict On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.Globalization

#End Region

#Region " EtaEstimator "

''' <summary>
''' Estimates the remaining time (ETA) of a sequential process made of items (e.g., pages or user-account logins),
''' based on the average duration of the most recently completed items.
''' <para></para>
''' Using a sliding window of recent items lets the estimation adapt to changes in the processing speed
''' (e.g., pages whose wallpapers were already downloaded are processed much faster than new pages).
''' </summary>
Friend NotInheritable Class EtaEstimator

#Region " Fields "

    ''' <summary>
    ''' The default number of most recent item durations used to calculate the average.
    ''' </summary>
    Friend Const DefaultWindowSize As Integer = 20

    ''' <summary>
    ''' The text displayed when the ETA cannot be calculated yet (no completed items).
    ''' </summary>
    Friend Const CalculatingText As String = "calculating..."

    ''' <summary>
    ''' The function that returns the elapsed time of a monotonic time source.
    ''' </summary>
    Private ReadOnly timeSource As Func(Of TimeSpan)

    ''' <summary>
    ''' The durations of the most recently completed items.
    ''' </summary>
    Private ReadOnly durations As New Queue(Of TimeSpan)()

    ''' <summary>
    ''' The sum of <see cref="EtaEstimator.durations"/>.
    ''' </summary>
    Private durationsSum As TimeSpan

    ''' <summary>
    ''' The time source value when the current item started, or <see langword="Nothing"/> if not started.
    ''' </summary>
    Private currentItemStart As TimeSpan?

#End Region

#Region " Properties "

    ''' <summary>
    ''' Gets the maximum number of most recent item durations used to calculate the average.
    ''' </summary>
    Friend ReadOnly Property WindowSize As Integer

    ''' <summary>
    ''' Gets the total number of completed items.
    ''' </summary>
    Friend ReadOnly Property CompletedCount As Integer

    ''' <summary>
    ''' Gets the average duration of the most recently completed items,
    ''' or <see langword="Nothing"/> if no item has been completed yet.
    ''' </summary>
    Friend ReadOnly Property AverageItemDuration As TimeSpan?
        Get
            If Me.durations.Count = 0 Then
                Return Nothing
            End If

            Return TimeSpan.FromTicks(Me.durationsSum.Ticks \ Me.durations.Count)
        End Get
    End Property

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Initializes a new instance of the <see cref="EtaEstimator"/> class, using a <see cref="Stopwatch"/> as time source.
    ''' </summary>
    '''
    ''' <param name="windowSize">
    ''' The maximum number of most recent item durations used to calculate the average.
    ''' </param>
    '''
    ''' <exception cref="ArgumentOutOfRangeException">
    ''' Thrown when <paramref name="windowSize"/> is less than 1.
    ''' </exception>
    <DebuggerStepThrough>
    Friend Sub New(Optional windowSize As Integer = EtaEstimator.DefaultWindowSize)

        Me.New(windowSize, EtaEstimator.CreateStopwatchTimeSource())
    End Sub

    ''' <summary>
    ''' Initializes a new instance of the <see cref="EtaEstimator"/> class.
    ''' </summary>
    '''
    ''' <param name="windowSize">
    ''' The maximum number of most recent item durations used to calculate the average.
    ''' </param>
    '''
    ''' <param name="timeSource">
    ''' The function that returns the elapsed time of a monotonic time source.
    ''' </param>
    '''
    ''' <exception cref="ArgumentOutOfRangeException">
    ''' Thrown when <paramref name="windowSize"/> is less than 1.
    ''' </exception>
    '''
    ''' <exception cref="ArgumentNullException">
    ''' Thrown when <paramref name="timeSource"/> is <see langword="Nothing"/>.
    ''' </exception>
    <DebuggerStepThrough>
    Friend Sub New(windowSize As Integer, timeSource As Func(Of TimeSpan))

        If windowSize < 1 Then
            Throw New ArgumentOutOfRangeException(NameOf(windowSize), windowSize, "Window size must be greater than or equal to 1.")
        End If

        If timeSource Is Nothing Then
            Throw New ArgumentNullException(NameOf(timeSource))
        End If

        Me.WindowSize = windowSize
        Me.timeSource = timeSource
    End Sub

#End Region

#Region " Public Methods "

    ''' <summary>
    ''' Marks the start of the next item.
    ''' </summary>
    <DebuggerStepThrough>
    Friend Sub StartItem()

        Me.currentItemStart = Me.timeSource.Invoke()
    End Sub

    ''' <summary>
    ''' Marks the current item as completed, recording its duration, and starts the next item.
    ''' </summary>
    '''
    ''' <exception cref="InvalidOperationException">
    ''' Thrown when no item was started (see <see cref="EtaEstimator.StartItem()"/>).
    ''' </exception>
    <DebuggerStepThrough>
    Friend Sub CompleteItem()

        If Not Me.currentItemStart.HasValue Then
            Throw New InvalidOperationException("No item was started.")
        End If

        Dim now As TimeSpan = Me.timeSource.Invoke()
        Dim duration As TimeSpan = now - Me.currentItemStart.Value
        If duration < TimeSpan.Zero Then
            duration = TimeSpan.Zero
        End If

        Me.durations.Enqueue(duration)
        Me.durationsSum += duration
        If Me.durations.Count > Me.WindowSize Then
            Me.durationsSum -= Me.durations.Dequeue()
        End If

        Me._CompletedCount += 1
        Me.currentItemStart = now
    End Sub

    ''' <summary>
    ''' Gets the estimated remaining time to complete the specified number of items.
    ''' </summary>
    '''
    ''' <param name="remainingItems">
    ''' The number of remaining items (including the current one, if it is not completed yet).
    ''' </param>
    '''
    ''' <returns>
    ''' The estimated remaining time, or <see langword="Nothing"/> if no item has been completed yet.
    ''' </returns>
    '''
    ''' <exception cref="ArgumentOutOfRangeException">
    ''' Thrown when <paramref name="remainingItems"/> is negative.
    ''' </exception>
    <DebuggerStepThrough>
    Friend Function GetEta(remainingItems As Integer) As TimeSpan?

        If remainingItems < 0 Then
            Throw New ArgumentOutOfRangeException(NameOf(remainingItems), remainingItems, "Remaining items cannot be negative.")
        End If

        Dim average As TimeSpan? = Me.AverageItemDuration
        If Not average.HasValue Then
            Return Nothing
        End If

        Return TimeSpan.FromTicks(average.Value.Ticks * remainingItems)
    End Function

    ''' <summary>
    ''' Gets the estimated remaining time to complete the specified number of items, formatted for console display.
    ''' </summary>
    '''
    ''' <param name="remainingItems">
    ''' The number of remaining items (including the current one, if it is not completed yet).
    ''' </param>
    '''
    ''' <returns>
    ''' The estimated remaining time in "HH:mm:ss" format (see <see cref="EtaEstimator.FormatDuration(TimeSpan)"/>),
    ''' or <see cref="EtaEstimator.CalculatingText"/> if no item has been completed yet.
    ''' </returns>
    <DebuggerStepThrough>
    Friend Function GetEtaText(remainingItems As Integer) As String

        Dim eta As TimeSpan? = Me.GetEta(remainingItems)
        Return If(eta.HasValue, EtaEstimator.FormatDuration(eta.Value), EtaEstimator.CalculatingText)
    End Function

    ''' <summary>
    ''' Formats the specified duration in "HH:mm:ss" format, where the hours are the total hours
    ''' (so durations of 24 hours or more are displayed as, e.g., "26:15:00" instead of wrapping around).
    ''' Fractions of a second are rounded up.
    ''' </summary>
    '''
    ''' <param name="duration">
    ''' The duration to format.
    ''' </param>
    '''
    ''' <returns>
    ''' The formatted duration.
    ''' </returns>
    '''
    ''' <exception cref="ArgumentOutOfRangeException">
    ''' Thrown when <paramref name="duration"/> is negative.
    ''' </exception>
    <DebuggerStepThrough>
    Friend Shared Function FormatDuration(duration As TimeSpan) As String

        If duration < TimeSpan.Zero Then
            Throw New ArgumentOutOfRangeException(NameOf(duration), duration, "Duration cannot be negative.")
        End If

        Dim totalSeconds As Long = CLng(Math.Ceiling(duration.TotalSeconds))
        Dim hours As Long = totalSeconds \ 3600
        Dim minutes As Long = (totalSeconds Mod 3600) \ 60
        Dim seconds As Long = totalSeconds Mod 60

        Return String.Format(CultureInfo.InvariantCulture, "{0:00}:{1:00}:{2:00}", hours, minutes, seconds)
    End Function

#End Region

#Region " Private Methods "

    ''' <summary>
    ''' Creates a time source based on a new running <see cref="Stopwatch"/>.
    ''' </summary>
    '''
    ''' <returns>
    ''' The time source.
    ''' </returns>
    Private Shared Function CreateStopwatchTimeSource() As Func(Of TimeSpan)

        Dim watch As Stopwatch = Stopwatch.StartNew()
        Return Function() watch.Elapsed
    End Function

#End Region

End Class

#End Region
