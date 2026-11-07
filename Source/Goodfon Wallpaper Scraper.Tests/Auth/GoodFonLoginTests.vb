#Region " Option Statements "

Option Explicit On
Option Strict On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Collections.Generic
Imports System.Linq
Imports System.Net
Imports System.Net.Http
Imports System.Net.Http.Headers
Imports System.Threading.Tasks

#End Region

#Region " GoodFonLoginTests "

''' <summary>
''' Unit tests for the login process of <see cref="GoodFonAuth"/> (including the website login rate-limit handling),
''' and for the <see cref="GoodFonLoginSettings"/> class.
''' <para></para>
''' The website responses are simulated with <see cref="FakeHttpMessageHandler"/>; no network requests are performed.
''' </summary>
<TestClass>
Public NotInheritable Class GoodFonLoginTests

#Region " Constants "

    ''' <summary>
    ''' The JSON returned by the website on successful login.
    ''' </summary>
    Private Const SuccessJson As String = "{""status"": ""success""}"

    ''' <summary>
    ''' The JSON returned by the website on wrong credentials.
    ''' </summary>
    Private Const ErrorJson As String = "{""status"": ""error""}"

#End Region

#Region " Test Helpers "

    ''' <summary>
    ''' Records the calls to the rate-limit callback and to the delay function of
    ''' <see cref="GoodFonAuth.LoginWithRetryAsync"/>, without actually waiting.
    ''' </summary>
    Private NotInheritable Class RetryRecorder

        ''' <summary>
        ''' Gets the (attempt, wait time) pairs passed to the rate-limit callback.
        ''' </summary>
        Friend ReadOnly Property RateLimitedCalls As New List(Of (Attempt As Integer, WaitTime As TimeSpan))()

        ''' <summary>
        ''' Gets the exceptions passed to the retry callback.
        ''' </summary>
        Friend ReadOnly Property Reasons As New List(Of Exception)()

        ''' <summary>
        ''' Gets the wait times passed to the delay function.
        ''' </summary>
        Friend ReadOnly Property Delays As New List(Of TimeSpan)()

        ''' <summary>
        ''' The retry callback.
        ''' </summary>
        '''
        ''' <param name="attempt">
        ''' The failed attempt number.
        ''' </param>
        '''
        ''' <param name="waitTime">
        ''' The wait time.
        ''' </param>
        '''
        ''' <param name="reason">
        ''' The exception that caused the failed attempt.
        ''' </param>
        Friend Sub OnRetry(attempt As Integer, waitTime As TimeSpan, reason As Exception)

            Me.RateLimitedCalls.Add((attempt, waitTime))
            Me.Reasons.Add(reason)
        End Sub

        ''' <summary>
        ''' The delay function; it completes immediately.
        ''' </summary>
        '''
        ''' <param name="waitTime">
        ''' The wait time.
        ''' </param>
        '''
        ''' <returns>
        ''' A completed task.
        ''' </returns>
        Friend Function DelayAsync(waitTime As TimeSpan) As Task

            Me.Delays.Add(waitTime)
            Return Task.CompletedTask
        End Function

    End Class

    ''' <summary>
    ''' Calls <see cref="GoodFonAuth.LoginWithRetryAsync"/> using the specified recorder.
    ''' </summary>
    '''
    ''' <param name="auth">
    ''' The authentication instance.
    ''' </param>
    '''
    ''' <param name="settings">
    ''' The login settings.
    ''' </param>
    '''
    ''' <param name="recorder">
    ''' The retry recorder.
    ''' </param>
    '''
    ''' <returns>
    ''' The login task.
    ''' </returns>
    Private Shared Function LoginWithRecorderAsync(auth As GoodFonAuth, settings As GoodFonLoginSettings, recorder As RetryRecorder) As Task(Of Boolean)

        Return auth.LoginWithRetryAsync("alice", "secret", settings,
                                        AddressOf recorder.OnRetry,
                                        AddressOf recorder.DelayAsync)
    End Function

    ''' <summary>
    ''' Validates the specified login settings.
    ''' </summary>
    '''
    ''' <param name="settings">
    ''' The login settings to validate.
    ''' </param>
    Private Shared Sub ValidateSettings(settings As GoodFonLoginSettings)

        settings.Validate()
    End Sub

#End Region

#Region " LoginAsync "

    ''' <summary>
    ''' Verifies a successful login, and that the login form is posted with the credentials and the CSRF token.
    ''' </summary>
    <TestMethod>
    Public Async Function LoginAsync_SuccessResponse_ReturnsTrueAndPostsExpectedForm() As Task

        Dim handler As New FakeHttpMessageHandler()
        handler.EnqueueLoginAttempt(HttpStatusCode.OK, GoodFonLoginTests.SuccessJson)

        Using auth As New GoodFonAuth(handler)
            Assert.IsTrue(Await auth.LoginAsync("alice", "s3cr&t"))
        End Using

        Assert.AreEqual(2, handler.Requests.Count)
        Assert.AreEqual(HttpMethod.Get, handler.Requests(0).Method)
        Assert.AreEqual(GoodFonAuth.LoginUrl, handler.Requests(0).RequestUri.AbsoluteUri)

        Dim post As FakeHttpMessageHandler.RecordedRequest = handler.Requests(1)
        Assert.AreEqual(HttpMethod.Post, post.Method)
        Assert.AreEqual(GoodFonAuth.LoginUrl, post.RequestUri.AbsoluteUri)
        Assert.AreEqual(GoodFonAuth.LoginUrl, post.Referrer.AbsoluteUri)
        StringAssert.Contains(post.Body, "login=alice")
        StringAssert.Contains(post.Body, "password=s3cr%26t")
        StringAssert.Contains(post.Body, $"csrfmiddlewaretoken={FakeHttpMessageHandler.CsrfToken}")
    End Function

    ''' <summary>
    ''' Verifies that wrong credentials throw <see cref="UnauthorizedAccessException"/>.
    ''' </summary>
    <TestMethod>
    Public Async Function LoginAsync_ErrorStatus_ThrowsUnauthorizedAccessException() As Task

        Dim handler As New FakeHttpMessageHandler()
        handler.EnqueueLoginAttempt(HttpStatusCode.OK, GoodFonLoginTests.ErrorJson)

        Using auth As New GoodFonAuth(handler)
            Await Assert.ThrowsExactlyAsync(Of UnauthorizedAccessException)(Function() auth.LoginAsync("alice", "wrong"))
        End Using
    End Function

    ''' <summary>
    ''' Verifies that an HTTP 429 response throws <see cref="GoodFonRateLimitException"/>
    ''' with the wait time requested by the "Retry-After" header.
    ''' </summary>
    <TestMethod>
    Public Async Function LoginAsync_Http429WithRetryAfter_ThrowsRateLimitExceptionWithRetryAfter() As Task

        Dim handler As New FakeHttpMessageHandler()
        handler.EnqueueLoginAttempt(GoodFonAuth.HttpStatusTooManyRequests, String.Empty, New RetryConditionHeaderValue(TimeSpan.FromSeconds(120)))

        Using auth As New GoodFonAuth(handler)
            Dim ex As GoodFonRateLimitException =
                Await Assert.ThrowsExactlyAsync(Of GoodFonRateLimitException)(Function() auth.LoginAsync("alice", "secret"))

            Assert.AreEqual(TimeSpan.FromSeconds(120), ex.RetryAfter)
            StringAssert.Contains(ex.Message, "429")
        End Using
    End Function

    ''' <summary>
    ''' Regression test: an HTTP 403 login response (the website rate-limit, with an HTML or empty body)
    ''' must throw <see cref="GoodFonRateLimitException"/> instead of the previous misleading
    ''' "Invalid JSON returned from login request" <see cref="FormatException"/>.
    ''' </summary>
    <TestMethod>
    Public Async Function LoginAsync_Http403WithoutRetryAfter_ThrowsRateLimitExceptionWithoutRetryAfter() As Task

        Dim handler As New FakeHttpMessageHandler()
        handler.EnqueueLoginAttempt(HttpStatusCode.Forbidden, "<html><body>Forbidden</body></html>")

        Using auth As New GoodFonAuth(handler)
            Dim ex As GoodFonRateLimitException =
                Await Assert.ThrowsExactlyAsync(Of GoodFonRateLimitException)(Function() auth.LoginAsync("alice", "secret"))

            Assert.IsNull(ex.RetryAfter)
        End Using
    End Function

    ''' <summary>
    ''' Verifies that a non-JSON successful response throws <see cref="FormatException"/>,
    ''' even though the login page HTML contains the (hidden) "Too many attempts" text.
    ''' </summary>
    <TestMethod>
    Public Async Function LoginAsync_NonJsonResponse_ThrowsFormatException() As Task

        Dim handler As New FakeHttpMessageHandler()
        handler.EnqueueLoginAttempt(HttpStatusCode.OK, "<div class=""js-auth-ratelimit_err"">Too many attempts. Please wait a few minutes</div>")

        Using auth As New GoodFonAuth(handler)
            Await Assert.ThrowsExactlyAsync(Of FormatException)(Function() auth.LoginAsync("alice", "secret"))
        End Using
    End Function

    ''' <summary>
    ''' Verifies that a JSON response without "status" throws <see cref="InvalidOperationException"/>.
    ''' </summary>
    <TestMethod>
    Public Async Function LoginAsync_JsonWithoutStatus_ThrowsInvalidOperationException() As Task

        Dim handler As New FakeHttpMessageHandler()
        handler.EnqueueLoginAttempt(HttpStatusCode.OK, "{""result"": ""ok""}")

        Using auth As New GoodFonAuth(handler)
            Await Assert.ThrowsExactlyAsync(Of InvalidOperationException)(Function() auth.LoginAsync("alice", "secret"))
        End Using
    End Function

    ''' <summary>
    ''' Verifies that a login page without CSRF token throws <see cref="InvalidOperationException"/>,
    ''' without posting the credentials.
    ''' </summary>
    <TestMethod>
    Public Async Function LoginAsync_MissingCsrfToken_ThrowsInvalidOperationException() As Task

        Dim handler As New FakeHttpMessageHandler()
        handler.Enqueue(HttpStatusCode.OK, "<html><body><form></form></body></html>")

        Using auth As New GoodFonAuth(handler)
            Await Assert.ThrowsExactlyAsync(Of InvalidOperationException)(Function() auth.LoginAsync("alice", "secret"))
        End Using

        Assert.AreEqual(1, handler.Requests.Count)
    End Function

    ''' <summary>
    ''' Verifies that a null handler throws <see cref="ArgumentNullException"/>.
    ''' </summary>
    <TestMethod>
    Public Sub Constructor_NullHandler_ThrowsArgumentNullException()

        Assert.ThrowsExactly(Of ArgumentNullException)(Function() New GoodFonAuth(DirectCast(Nothing, HttpMessageHandler)))
    End Sub

#End Region

#Region " LoginWithRetryAsync "

    ''' <summary>
    ''' Verifies that a successful first attempt neither waits nor retries.
    ''' </summary>
    <TestMethod>
    Public Async Function LoginWithRetryAsync_FirstAttemptSucceeds_DoesNotWait() As Task

        Dim handler As New FakeHttpMessageHandler()
        handler.EnqueueLoginAttempt(HttpStatusCode.OK, GoodFonLoginTests.SuccessJson)
        Dim recorder As New RetryRecorder()

        Using auth As New GoodFonAuth(handler)
            Assert.IsTrue(Await GoodFonLoginTests.LoginWithRecorderAsync(auth, New GoodFonLoginSettings(), recorder))
        End Using

        Assert.AreEqual(0, recorder.RateLimitedCalls.Count)
        Assert.AreEqual(0, recorder.Delays.Count)
    End Function

    ''' <summary>
    ''' Verifies that a rate-limited attempt waits the configured time and retries, succeeding afterwards.
    ''' </summary>
    <TestMethod>
    Public Async Function LoginWithRetryAsync_RateLimitedThenSuccess_WaitsConfiguredTimeAndRetries() As Task

        Dim handler As New FakeHttpMessageHandler()
        handler.EnqueueLoginAttempt(GoodFonAuth.HttpStatusTooManyRequests, String.Empty)
        handler.EnqueueLoginAttempt(HttpStatusCode.OK, GoodFonLoginTests.SuccessJson)
        Dim settings As New GoodFonLoginSettings() With {.RateLimitWaitSeconds = 90, .MaxLoginAttempts = 3}
        Dim recorder As New RetryRecorder()

        Using auth As New GoodFonAuth(handler)
            Assert.IsTrue(Await GoodFonLoginTests.LoginWithRecorderAsync(auth, settings, recorder))
        End Using

        Assert.AreEqual(1, recorder.RateLimitedCalls.Count)
        Assert.AreEqual(1, recorder.RateLimitedCalls(0).Attempt)
        Assert.AreEqual(TimeSpan.FromSeconds(90), recorder.RateLimitedCalls(0).WaitTime)
        CollectionAssert.AreEqual(New TimeSpan() {TimeSpan.FromSeconds(90)}, recorder.Delays)
        Assert.AreEqual(4, handler.Requests.Count)
    End Function

    ''' <summary>
    ''' Verifies that a longer wait time requested by the website ("Retry-After") takes precedence over the configured one.
    ''' </summary>
    <TestMethod>
    Public Async Function LoginWithRetryAsync_RetryAfterLongerThanConfigured_UsesRetryAfter() As Task

        Dim handler As New FakeHttpMessageHandler()
        handler.EnqueueLoginAttempt(HttpStatusCode.Forbidden, String.Empty, New RetryConditionHeaderValue(TimeSpan.FromSeconds(600)))
        handler.EnqueueLoginAttempt(HttpStatusCode.OK, GoodFonLoginTests.SuccessJson)
        Dim settings As New GoodFonLoginSettings() With {.RateLimitWaitSeconds = 60}
        Dim recorder As New RetryRecorder()

        Using auth As New GoodFonAuth(handler)
            Assert.IsTrue(Await GoodFonLoginTests.LoginWithRecorderAsync(auth, settings, recorder))
        End Using

        CollectionAssert.AreEqual(New TimeSpan() {TimeSpan.FromSeconds(600)}, recorder.Delays)
    End Function

    ''' <summary>
    ''' Verifies that, when every attempt is rate-limited, the method waits between attempts
    ''' (never after the last one) and finally throws <see cref="GoodFonRateLimitException"/>.
    ''' </summary>
    <TestMethod>
    Public Async Function LoginWithRetryAsync_AllAttemptsRateLimited_ThrowsAfterMaxAttempts() As Task

        Dim handler As New FakeHttpMessageHandler()
        For i As Integer = 1 To 3
            handler.EnqueueLoginAttempt(GoodFonAuth.HttpStatusTooManyRequests, String.Empty)
        Next i
        Dim settings As New GoodFonLoginSettings() With {.RateLimitWaitSeconds = 30, .MaxLoginAttempts = 3}
        Dim recorder As New RetryRecorder()

        Using auth As New GoodFonAuth(handler)
            Await Assert.ThrowsExactlyAsync(Of GoodFonRateLimitException)(
                Function() GoodFonLoginTests.LoginWithRecorderAsync(auth, settings, recorder))
        End Using

        CollectionAssert.AreEqual(New Integer() {1, 2}, recorder.RateLimitedCalls.Select(Function(c As (Attempt As Integer, WaitTime As TimeSpan)) c.Attempt).ToArray())
        Assert.AreEqual(2, recorder.Delays.Count)
        Assert.AreEqual(6, handler.Requests.Count) ' 3 attempts x (GET + POST).
    End Function

    ''' <summary>
    ''' Verifies that a single allowed attempt never waits nor retries.
    ''' </summary>
    <TestMethod>
    Public Async Function LoginWithRetryAsync_SingleAttemptAllowed_DoesNotRetry() As Task

        Dim handler As New FakeHttpMessageHandler()
        handler.EnqueueLoginAttempt(GoodFonAuth.HttpStatusTooManyRequests, String.Empty)
        Dim settings As New GoodFonLoginSettings() With {.MaxLoginAttempts = 1}
        Dim recorder As New RetryRecorder()

        Using auth As New GoodFonAuth(handler)
            Await Assert.ThrowsExactlyAsync(Of GoodFonRateLimitException)(
                Function() GoodFonLoginTests.LoginWithRecorderAsync(auth, settings, recorder))
        End Using

        Assert.AreEqual(0, recorder.Delays.Count)
    End Function

    ''' <summary>
    ''' Verifies that errors other than the rate-limit (e.g., wrong credentials) are never retried.
    ''' </summary>
    <TestMethod>
    Public Async Function LoginWithRetryAsync_WrongCredentials_IsNotRetried() As Task

        Dim handler As New FakeHttpMessageHandler()
        handler.EnqueueLoginAttempt(HttpStatusCode.OK, GoodFonLoginTests.ErrorJson)
        Dim recorder As New RetryRecorder()

        Using auth As New GoodFonAuth(handler)
            Await Assert.ThrowsExactlyAsync(Of UnauthorizedAccessException)(
                Function() GoodFonLoginTests.LoginWithRecorderAsync(auth, New GoodFonLoginSettings(), recorder))
        End Using

        Assert.AreEqual(0, recorder.Delays.Count)
        Assert.AreEqual(2, handler.Requests.Count)
    End Function

    ''' <summary>
    ''' Verifies the argument validation, which happens before any request is performed.
    ''' </summary>
    <TestMethod>
    Public Async Function LoginWithRetryAsync_InvalidArguments_ThrowsBeforeAnyRequest() As Task

        Dim handler As New FakeHttpMessageHandler()
        Dim recorder As New RetryRecorder()
        Dim onRateLimited As Action(Of Integer, TimeSpan, Exception) = AddressOf recorder.OnRetry
        Dim delayAsync As Func(Of TimeSpan, Task) = AddressOf recorder.DelayAsync
        Dim invalidSettings As New GoodFonLoginSettings() With {.MaxLoginAttempts = 0}

        Using auth As New GoodFonAuth(handler)
            Await Assert.ThrowsExactlyAsync(Of ArgumentNullException)(Function() auth.LoginWithRetryAsync("a", "b", Nothing, onRateLimited, delayAsync))
            Await Assert.ThrowsExactlyAsync(Of ArgumentNullException)(Function() auth.LoginWithRetryAsync("a", "b", New GoodFonLoginSettings(), Nothing, delayAsync))
            Await Assert.ThrowsExactlyAsync(Of ArgumentNullException)(Function() auth.LoginWithRetryAsync("a", "b", New GoodFonLoginSettings(), onRateLimited, Nothing))
            Await Assert.ThrowsExactlyAsync(Of InvalidOperationException)(Function() auth.LoginWithRetryAsync("a", "b", invalidSettings, onRateLimited, delayAsync))
        End Using

        Assert.AreEqual(0, handler.Requests.Count)
    End Function

#End Region

#Region " LoginWithRetryAsync (Network Errors) "

    ''' <summary>
    ''' Creates the network error thrown by .NET Framework when the server drops the connection
    ''' ("The underlying connection was closed: An unexpected error occurred on a send.").
    ''' </summary>
    '''
    ''' <returns>
    ''' The network error.
    ''' </returns>
    Private Shared Function CreateConnectionClosedError() As HttpRequestException

        Return New HttpRequestException("An error occurred while sending the request.",
                                        New WebException("The underlying connection was closed: An unexpected error occurred on a send.",
                                                         WebExceptionStatus.SendFailure))
    End Function

    ''' <summary>
    ''' Regression test: a network error during the login (e.g., "The underlying connection was closed")
    ''' must be retried after the configured network error wait time, instead of giving up on the account.
    ''' </summary>
    <TestMethod>
    Public Async Function LoginWithRetryAsync_NetworkErrorThenSuccess_WaitsNetworkErrorTimeAndRetries() As Task

        Dim handler As New FakeHttpMessageHandler()
        handler.EnqueueException(GoodFonLoginTests.CreateConnectionClosedError())
        handler.EnqueueLoginAttempt(HttpStatusCode.OK, GoodFonLoginTests.SuccessJson)
        Dim settings As New GoodFonLoginSettings() With {.NetworkErrorWaitSeconds = 20, .RateLimitWaitSeconds = 999}
        Dim recorder As New RetryRecorder()

        Using auth As New GoodFonAuth(handler)
            Assert.IsTrue(Await GoodFonLoginTests.LoginWithRecorderAsync(auth, settings, recorder))
        End Using

        CollectionAssert.AreEqual(New TimeSpan() {TimeSpan.FromSeconds(20)}, recorder.Delays)
        Assert.IsInstanceOfType(Of HttpRequestException)(recorder.Reasons.Single())
        Assert.AreEqual(1, recorder.RateLimitedCalls.Single().Attempt)
    End Function

    ''' <summary>
    ''' Verifies that a network error during the login POST (after the login page was loaded) is also retried.
    ''' </summary>
    <TestMethod>
    Public Async Function LoginWithRetryAsync_NetworkErrorOnLoginPost_IsRetried() As Task

        Dim handler As New FakeHttpMessageHandler()
        handler.EnqueueLoginPage().EnqueueException(GoodFonLoginTests.CreateConnectionClosedError())
        handler.EnqueueLoginAttempt(HttpStatusCode.OK, GoodFonLoginTests.SuccessJson)
        Dim recorder As New RetryRecorder()

        Using auth As New GoodFonAuth(handler)
            Assert.IsTrue(Await GoodFonLoginTests.LoginWithRecorderAsync(auth, New GoodFonLoginSettings(), recorder))
        End Using

        Assert.AreEqual(1, recorder.Delays.Count)
        Assert.AreEqual(4, handler.Requests.Count)
    End Function

    ''' <summary>
    ''' Verifies that request timeouts are retried as network errors.
    ''' </summary>
    <TestMethod>
    Public Async Function LoginWithRetryAsync_TimeoutThenSuccess_IsRetried() As Task

        Dim handler As New FakeHttpMessageHandler()
        handler.EnqueueException(New TaskCanceledException("A task was canceled."))
        handler.EnqueueLoginAttempt(HttpStatusCode.OK, GoodFonLoginTests.SuccessJson)
        Dim recorder As New RetryRecorder()

        Using auth As New GoodFonAuth(handler)
            Assert.IsTrue(Await GoodFonLoginTests.LoginWithRecorderAsync(auth, New GoodFonLoginSettings(), recorder))
        End Using

        Assert.IsInstanceOfType(Of TaskCanceledException)(recorder.Reasons.Single())
    End Function

    ''' <summary>
    ''' Verifies that, when every attempt fails with a network error, the last error is thrown after the maximum attempts.
    ''' </summary>
    <TestMethod>
    Public Async Function LoginWithRetryAsync_AllAttemptsNetworkError_ThrowsAfterMaxAttempts() As Task

        Dim handler As New FakeHttpMessageHandler()
        For i As Integer = 1 To 3
            handler.EnqueueException(GoodFonLoginTests.CreateConnectionClosedError())
        Next i
        Dim settings As New GoodFonLoginSettings() With {.MaxLoginAttempts = 3}
        Dim recorder As New RetryRecorder()

        Using auth As New GoodFonAuth(handler)
            Dim ex As HttpRequestException = Await Assert.ThrowsExactlyAsync(Of HttpRequestException)(
                Function() GoodFonLoginTests.LoginWithRecorderAsync(auth, settings, recorder))

            StringAssert.Contains(ex.GetBaseException().Message, "underlying connection was closed")
        End Using

        Assert.AreEqual(2, recorder.Delays.Count)
        Assert.AreEqual(3, handler.Requests.Count)
    End Function

    ''' <summary>
    ''' Verifies that mixed retryable errors (network error, then rate-limit) use their own wait times.
    ''' </summary>
    <TestMethod>
    Public Async Function LoginWithRetryAsync_MixedErrors_UseTheirOwnWaitTimes() As Task

        Dim handler As New FakeHttpMessageHandler()
        handler.EnqueueException(GoodFonLoginTests.CreateConnectionClosedError())
        handler.EnqueueLoginAttempt(GoodFonAuth.HttpStatusTooManyRequests, String.Empty)
        handler.EnqueueLoginAttempt(HttpStatusCode.OK, GoodFonLoginTests.SuccessJson)
        Dim settings As New GoodFonLoginSettings() With {.NetworkErrorWaitSeconds = 10, .RateLimitWaitSeconds = 100, .MaxLoginAttempts = 3}
        Dim recorder As New RetryRecorder()

        Using auth As New GoodFonAuth(handler)
            Assert.IsTrue(Await GoodFonLoginTests.LoginWithRecorderAsync(auth, settings, recorder))
        End Using

        CollectionAssert.AreEqual(New TimeSpan() {TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(100)}, recorder.Delays)
    End Function

    ''' <summary>
    ''' Verifies that the website rate-limit is also detected on the login page request (GET).
    ''' </summary>
    <TestMethod>
    Public Async Function LoginAsync_LoginPageRateLimited_ThrowsRateLimitException() As Task

        Dim handler As New FakeHttpMessageHandler()
        handler.Enqueue(GoodFonAuth.HttpStatusTooManyRequests, String.Empty, New RetryConditionHeaderValue(TimeSpan.FromSeconds(45)))

        Using auth As New GoodFonAuth(handler)
            Dim ex As GoodFonRateLimitException =
                Await Assert.ThrowsExactlyAsync(Of GoodFonRateLimitException)(Function() auth.LoginAsync("alice", "secret"))

            Assert.AreEqual(TimeSpan.FromSeconds(45), ex.RetryAfter)
        End Using

        Assert.AreEqual(1, handler.Requests.Count)
    End Function

    ''' <summary>
    ''' Verifies the classification of transient network errors.
    ''' </summary>
    <TestMethod>
    Public Sub IsTransientNetworkError_ExceptionTypes_AreClassifiedAsExpected()

        Assert.IsTrue(GoodFonAuth.IsTransientNetworkError(GoodFonLoginTests.CreateConnectionClosedError()))
        Assert.IsTrue(GoodFonAuth.IsTransientNetworkError(New TaskCanceledException()))

        Assert.IsFalse(GoodFonAuth.IsTransientNetworkError(New UnauthorizedAccessException()))
        Assert.IsFalse(GoodFonAuth.IsTransientNetworkError(New FormatException()))
        Assert.IsFalse(GoodFonAuth.IsTransientNetworkError(New InvalidOperationException()))
        Assert.ThrowsExactly(Of ArgumentNullException)(Sub() GoodFonAuth.IsTransientNetworkError(Nothing))
    End Sub

#End Region

#Region " IsRateLimitResponse / GetRetryAfter "

    ''' <summary>
    ''' Verifies that only HTTP 403 and 429 are considered rate-limit responses (same logic as the website).
    ''' </summary>
    <TestMethod>
    Public Sub IsRateLimitResponse_StatusCodes_MatchWebsiteLogic()

        Assert.IsTrue(GoodFonAuth.IsRateLimitResponse(HttpStatusCode.Forbidden))
        Assert.IsTrue(GoodFonAuth.IsRateLimitResponse(CType(429, HttpStatusCode)))

        Assert.IsFalse(GoodFonAuth.IsRateLimitResponse(HttpStatusCode.OK))
        Assert.IsFalse(GoodFonAuth.IsRateLimitResponse(HttpStatusCode.BadRequest))
        Assert.IsFalse(GoodFonAuth.IsRateLimitResponse(HttpStatusCode.Unauthorized))
        Assert.IsFalse(GoodFonAuth.IsRateLimitResponse(HttpStatusCode.InternalServerError))
    End Sub

    ''' <summary>
    ''' Verifies the "Retry-After" header interpretation (delta seconds, absolute date and missing header).
    ''' </summary>
    <TestMethod>
    Public Sub GetRetryAfter_HeaderVariants_ReturnsExpectedWaitTime()

        Dim now As New DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero)

        Assert.IsNull(GoodFonAuth.GetRetryAfter(Nothing, now))
        Assert.AreEqual(TimeSpan.FromSeconds(30), GoodFonAuth.GetRetryAfter(New RetryConditionHeaderValue(TimeSpan.FromSeconds(30)), now))
        Assert.AreEqual(TimeSpan.FromSeconds(90), GoodFonAuth.GetRetryAfter(New RetryConditionHeaderValue(now.AddSeconds(90)), now))
        Assert.AreEqual(TimeSpan.Zero, GoodFonAuth.GetRetryAfter(New RetryConditionHeaderValue(now.AddSeconds(-90)), now))
    End Sub

#End Region

#Region " GoodFonLoginSettings "

    ''' <summary>
    ''' Verifies the default settings values.
    ''' </summary>
    <TestMethod>
    Public Sub LoginSettings_NewInstance_HasExpectedDefaults()

        Dim settings As New GoodFonLoginSettings()

        Assert.AreEqual(180, settings.RateLimitWaitSeconds)
        Assert.AreEqual(3, settings.MaxLoginAttempts)
        Assert.AreEqual(5, settings.DelayBetweenLoginsSeconds)
        Assert.AreEqual(15, settings.NetworkErrorWaitSeconds)
        settings.Validate()
    End Sub

    ''' <summary>
    ''' Verifies that invalid values are rejected by <see cref="GoodFonLoginSettings.Validate"/>.
    ''' </summary>
    <TestMethod>
    Public Sub LoginSettings_InvalidValues_ValidateThrowsInvalidOperationException()

        Assert.ThrowsExactly(Of InvalidOperationException)(Sub() GoodFonLoginTests.ValidateSettings(New GoodFonLoginSettings() With {.RateLimitWaitSeconds = -1}))
        Assert.ThrowsExactly(Of InvalidOperationException)(Sub() GoodFonLoginTests.ValidateSettings(New GoodFonLoginSettings() With {.MaxLoginAttempts = 0}))
        Assert.ThrowsExactly(Of InvalidOperationException)(Sub() GoodFonLoginTests.ValidateSettings(New GoodFonLoginSettings() With {.DelayBetweenLoginsSeconds = -1}))
        Assert.ThrowsExactly(Of InvalidOperationException)(Sub() GoodFonLoginTests.ValidateSettings(New GoodFonLoginSettings() With {.NetworkErrorWaitSeconds = -1}))
    End Sub

    ''' <summary>
    ''' Verifies that boundary values (zero waits, one attempt) are valid.
    ''' </summary>
    <TestMethod>
    Public Sub LoginSettings_BoundaryValues_AreValid()

        Dim settings As New GoodFonLoginSettings() With {
            .RateLimitWaitSeconds = 0,
            .MaxLoginAttempts = 1,
            .DelayBetweenLoginsSeconds = 0
        }

        settings.Validate()
        Assert.AreEqual(TimeSpan.Zero, settings.GetRateLimitWait(Nothing))
    End Sub

    ''' <summary>
    ''' Verifies that the rate-limit wait time is the greater value between the configured one and "Retry-After".
    ''' </summary>
    <TestMethod>
    Public Sub LoginSettings_GetRateLimitWait_ReturnsGreaterValue()

        Dim settings As New GoodFonLoginSettings() With {.RateLimitWaitSeconds = 120}

        Assert.AreEqual(TimeSpan.FromSeconds(120), settings.GetRateLimitWait(Nothing))
        Assert.AreEqual(TimeSpan.FromSeconds(120), settings.GetRateLimitWait(TimeSpan.FromSeconds(60)))
        Assert.AreEqual(TimeSpan.FromSeconds(300), settings.GetRateLimitWait(TimeSpan.FromSeconds(300)))
    End Sub

#End Region

End Class

#End Region
