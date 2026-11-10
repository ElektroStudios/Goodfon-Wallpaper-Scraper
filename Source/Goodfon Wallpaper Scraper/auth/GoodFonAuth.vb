#Region " Option Statements "

Option Explicit On
Option Strict On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.Net
Imports System.Net.Http
Imports System.Net.Http.Headers
Imports System.Text.Json
Imports System.Threading.Tasks

Imports HtmlAgilityPack

#End Region

#Region " GoodFonAuth "

''' <summary>
''' Handles login to GoodFon.com website and provides an authenticated <see cref="HttpClient"/>.
''' </summary>
Public NotInheritable Class GoodFonAuth : Implements IDisposable ' NOSONAR

#Region " Constants "

    ''' <summary>
    ''' The URL of the GoodFon.com login page.
    ''' </summary>
    Friend Const LoginUrl As String = "https://www.goodfon.com/auth/signin/"

    ''' <summary>
    ''' The HTTP 429 (Too Many Requests) status code, which is not defined in <see cref="HttpStatusCode"/> on .NET Framework.
    ''' </summary>
    Friend Const HttpStatusTooManyRequests As HttpStatusCode = CType(429, HttpStatusCode)

#End Region

#Region " Fields "

    ''' <summary>
    ''' The HTTP message handler used by <see cref="GoodFonAuth.Client"/>.
    ''' </summary>
    Private ReadOnly clientHandler As HttpMessageHandler

    ''' <summary>
    ''' The (random) User-Agent shared by all instances during the application lifetime.
    ''' </summary>
    Private Shared ReadOnly UserAgent As String = Guid.NewGuid().ToString()

    ''' <summary>
    ''' Flag to detect redundant calls to <see cref="GoodFonAuth.Dispose()"/>.
    ''' </summary>
    Private disposedValue As Boolean

#End Region

#Region " Properties "

    ''' <summary>
    ''' Gets the authenticated HTTP client instance.
    ''' </summary>
    Public ReadOnly Property Client As HttpClient

    ''' <summary>
    ''' Gets or sets the cookie container that stores the session cookies.
    ''' </summary>
    Public Property CookieContainer As New CookieContainer()

    ''' <summary>
    ''' Gets or sets a value indicating whether the daily wallpaper download limit has been reached on the website.
    ''' </summary>
    Public Property DailyLimitReached As Boolean = False

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Initializes a new instance of the <see cref="GoodFonAuth"/> class.
    ''' </summary>
    Public Sub New()

        Me.clientHandler = New HttpClientHandler() With {
            .UseCookies = True,
            .CookieContainer = Me.CookieContainer,
            .AutomaticDecompression = DecompressionMethods.GZip Or DecompressionMethods.Deflate
        }

        Me.Client = GoodFonAuth.CreateClient(Me.clientHandler)
    End Sub

    ''' <summary>
    ''' Initializes a new instance of the <see cref="GoodFonAuth"/> class using a custom HTTP message handler.
    ''' <para></para>
    ''' This constructor is intended for unit testing (e.g., to simulate the website responses).
    ''' </summary>
    '''
    ''' <param name="handler">
    ''' The HTTP message handler. It is disposed when this instance is disposed.
    ''' </param>
    '''
    ''' <exception cref="ArgumentNullException">
    ''' Thrown when <paramref name="handler"/> is <see langword="Nothing"/>.
    ''' </exception>
    Friend Sub New(handler As HttpMessageHandler)

        If handler Is Nothing Then
            Throw New ArgumentNullException(NameOf(handler))
        End If

        Me.clientHandler = handler
        Me.Client = GoodFonAuth.CreateClient(Me.clientHandler)
    End Sub

#End Region

#Region " Public Methods "

    ''' <summary>
    ''' Asynchronously authenticates against the Goodfon.com website using the provided credentials.
    ''' </summary>
    '''
    ''' <param name="username">
    ''' The account username.
    ''' </param>
    '''
    ''' <param name="password">
    ''' The account password.
    ''' </param>
    '''
    ''' <returns>
    ''' A <see cref="Task(Of Boolean)"/> representing the asynchronous operation,
    ''' returning <see langword="True"/> if authentication succeeds.
    ''' </returns>
    '''
    ''' <exception cref="GoodFonRateLimitException">
    ''' Thrown when the website rejects the login attempt due to its login rate-limit.
    ''' </exception>
    '''
    ''' <exception cref="InvalidOperationException">
    ''' Thrown when the CSRF token is not found in the login page,
    ''' or the login response does not contain the expected data.
    ''' </exception>
    '''
    ''' <exception cref="FormatException">
    ''' Thrown when the login response is not valid JSON.
    ''' </exception>
    '''
    ''' <exception cref="UnauthorizedAccessException">
    ''' Thrown when the website rejects the credentials.
    ''' </exception>
    Public Async Function LoginAsync(username As String, password As String) As Task(Of Boolean)

        ' STEP 1 — GET login page (CSRF)
        Dim loginHtml As String
        Using pageResponse As HttpResponseMessage = Await Me.Client.GetAsync(GoodFonAuth.LoginUrl).ConfigureAwait(False)

            ' The website rate-limit can also be applied to the login page itself.
            If GoodFonAuth.IsRateLimitResponse(pageResponse.StatusCode) Then
                Throw New GoodFonRateLimitException(
                    $"Login page request rejected by the website rate-limit (HTTP {CInt(pageResponse.StatusCode)}): too many attempts, please wait a few minutes.",
                    GoodFonAuth.GetRetryAfter(pageResponse.Headers.RetryAfter, DateTimeOffset.UtcNow))
            End If

            pageResponse.EnsureSuccessStatusCode()
            loginHtml = Await pageResponse.Content.ReadAsStringAsync().ConfigureAwait(False)
        End Using

        Dim doc As New HtmlDocument()
        doc.LoadHtml(loginHtml)

        Dim csrfNode As HtmlNode = doc.DocumentNode.SelectSingleNode("//input[@name='csrfmiddlewaretoken']")

        If csrfNode Is Nothing Then
            Throw New InvalidOperationException("CSRF token not found in login page.")
        End If

        Dim csrfToken As String = csrfNode.GetAttributeValue("value", String.Empty)

        ' STEP 2 — POST login
        Dim form As New Dictionary(Of String, String) From {
            {"login", username},
            {"password", password},
            {"csrfmiddlewaretoken", csrfToken}
        }

        Using content As New FormUrlEncodedContent(form),
              request As New HttpRequestMessage(HttpMethod.Post, GoodFonAuth.LoginUrl)

            request.Content = content
            request.Headers.Referrer = New Uri(GoodFonAuth.LoginUrl)

            Using response As HttpResponseMessage = Await Me.Client.SendAsync(request, HttpCompletionOption.ResponseContentRead).ConfigureAwait(False)

                Dim resultJson As String = Await response.Content.ReadAsStringAsync().ConfigureAwait(False)
#If DEBUG Then
                Console.WriteLine($"Goodfon.com login response ({CInt(response.StatusCode)}): {resultJson}")
#End If

                ' STEP 3 — Detect the login rate-limit ("Too many attempts. Please wait a few minutes").
                If GoodFonAuth.IsRateLimitResponse(response.StatusCode) Then
                    Dim retryAfter As TimeSpan? = GoodFonAuth.GetRetryAfter(response.Headers.RetryAfter, DateTimeOffset.UtcNow)
                    Throw New GoodFonRateLimitException(
                        $"Login rejected by the website rate-limit (HTTP {CInt(response.StatusCode)}): too many attempts, please wait a few minutes.",
                        retryAfter)
                End If

                ' STEP 4 — Parse JSON
                Dim status As String = GoodFonAuth.GetLoginResponseStatus(resultJson, response.StatusCode)
                If Not String.Equals(status, "success", StringComparison.OrdinalIgnoreCase) Then
                    Throw New UnauthorizedAccessException($"Login failed. Response JSON: {resultJson}")
                End If

                Return True

            End Using ' response
        End Using     ' content, request

    End Function

    ''' <summary>
    ''' Asynchronously authenticates against the Goodfon.com website using the provided credentials,
    ''' waiting and retrying when the website rejects the login attempt due to its login rate-limit.
    ''' </summary>
    '''
    ''' <param name="username">
    ''' The account username.
    ''' </param>
    '''
    ''' <param name="password">
    ''' The account password.
    ''' </param>
    '''
    ''' <param name="settings">
    ''' The login settings (wait time and maximum attempts).
    ''' </param>
    '''
    ''' <param name="onRetry">
    ''' The action invoked before each wait, with the failed attempt number (1-based) and the wait time.
    ''' </param>
    '''
    ''' <param name="delayAsync">
    ''' The function that performs the asynchronous wait (normally <see cref="Task.Delay(TimeSpan)"/>).
    ''' </param>
    '''
    ''' <returns>
    ''' A <see cref="Task(Of Boolean)"/> representing the asynchronous operation,
    ''' returning <see langword="True"/> if authentication succeeds.
    ''' </returns>
    '''
    ''' <exception cref="ArgumentNullException">
    ''' Thrown when <paramref name="settings"/>, <paramref name="onRetry"/> or <paramref name="delayAsync"/> is <see langword="Nothing"/>.
    ''' </exception>
    '''
    ''' <exception cref="GoodFonRateLimitException">
    ''' Thrown when the last allowed attempt is also rejected by the website login rate-limit.
    ''' </exception>
    Public Async Function LoginWithRetryAsync(username As String, password As String,
                                              settings As GoodFonLoginSettings,
                                              onRetry As Action(Of Integer, TimeSpan, Exception),
                                              delayAsync As Func(Of TimeSpan, Task)) As Task(Of Boolean)

        If settings Is Nothing Then
            Throw New ArgumentNullException(NameOf(settings))
        End If

        If onRetry Is Nothing Then
            Throw New ArgumentNullException(NameOf(onRetry))
        End If

        If delayAsync Is Nothing Then
            Throw New ArgumentNullException(NameOf(delayAsync))
        End If

        settings.Validate()

        Dim attempt As Integer = 1
        Do
            Dim retryReason As Exception
            Dim waitTime As TimeSpan
            Try
                Return Await Me.LoginAsync(username, password).ConfigureAwait(False)

            Catch ex As GoodFonRateLimitException When attempt < settings.MaxLoginAttempts
                retryReason = ex
                waitTime = settings.GetRateLimitWait(ex.RetryAfter)

            Catch ex As Exception When attempt < settings.MaxLoginAttempts AndAlso GoodFonAuth.IsTransientNetworkError(ex)
                retryReason = ex
                waitTime = TimeSpan.FromSeconds(settings.NetworkErrorWaitSeconds)

            End Try

            ' Note: VB.NET does not allow "Await" inside a "Catch" block.
            onRetry.Invoke(attempt, waitTime, retryReason)
            Await delayAsync.Invoke(waitTime).ConfigureAwait(False)

            attempt += 1
        Loop
    End Function

    ''' <summary>
    ''' Releases all resources used by the current <see cref="GoodFonAuth"/> instance.
    ''' </summary>
    Public Sub Dispose() Implements IDisposable.Dispose

        Me.Dispose(disposing:=True)
        GC.SuppressFinalize(Me)
    End Sub

#End Region

#Region " Friend Methods "

    ''' <summary>
    ''' Determines whether a login response status code indicates that the website login rate-limit was hit
    ''' ("Too many attempts. Please wait a few minutes").
    ''' <para></para>
    ''' This mirrors the website's own client-side logic, which treats HTTP 403 (Forbidden)
    ''' and 429 (Too Many Requests) login responses as rate-limited.
    ''' <para></para>
    ''' Note: the response body is intentionally not evaluated,
    ''' because the regular login page HTML always contains the (hidden) rate-limit error text.
    ''' </summary>
    '''
    ''' <param name="statusCode">
    ''' The HTTP status code of the login response.
    ''' </param>
    '''
    ''' <returns>
    ''' <see langword="True"/> if the login response indicates a rate-limit; otherwise, <see langword="False"/>.
    ''' </returns>
    <DebuggerStepThrough>
    Friend Shared Function IsRateLimitResponse(statusCode As HttpStatusCode) As Boolean

        Return statusCode = HttpStatusCode.Forbidden OrElse statusCode = GoodFonAuth.HttpStatusTooManyRequests
    End Function

    ''' <summary>
    ''' Gets the wait time requested by an HTTP "Retry-After" header value.
    ''' </summary>
    '''
    ''' <param name="retryAfter">
    ''' The "Retry-After" header value. It can be <see langword="Nothing"/>.
    ''' </param>
    '''
    ''' <param name="utcNow">
    ''' The current date and time, used when the header specifies an absolute date.
    ''' </param>
    '''
    ''' <returns>
    ''' The requested wait time (never negative), or <see langword="Nothing"/> if the header is not present.
    ''' </returns>
    <DebuggerStepThrough>
    Friend Shared Function GetRetryAfter(retryAfter As RetryConditionHeaderValue, utcNow As DateTimeOffset) As TimeSpan?

        If retryAfter Is Nothing Then
            Return Nothing
        End If

        If retryAfter.Delta.HasValue Then
            Return If(retryAfter.Delta.Value < TimeSpan.Zero, TimeSpan.Zero, retryAfter.Delta.Value)
        End If

        If retryAfter.Date.HasValue Then
            Dim delta As TimeSpan = retryAfter.Date.Value - utcNow
            Return If(delta < TimeSpan.Zero, TimeSpan.Zero, delta)
        End If

        Return Nothing
    End Function

    ''' <summary>
    ''' Determines whether the specified exception is a transient network error that is worth retrying:
    ''' an <see cref="HttpRequestException"/> (e.g., "The underlying connection was closed:
    ''' An unexpected error occurred on a send", connection reset, DNS failure, or a non-success
    ''' status code when requesting the login page), or a <see cref="TaskCanceledException"/> (request timeout).
    ''' </summary>
    '''
    ''' <param name="ex">
    ''' The exception to evaluate.
    ''' </param>
    '''
    ''' <returns>
    ''' <see langword="True"/> if the exception is a transient network error; otherwise, <see langword="False"/>.
    ''' </returns>
    '''
    ''' <exception cref="ArgumentNullException">
    ''' Thrown when <paramref name="ex"/> is <see langword="Nothing"/>.
    ''' </exception>
    <DebuggerStepThrough>
    Friend Shared Function IsTransientNetworkError(ex As Exception) As Boolean

        If ex Is Nothing Then
            Throw New ArgumentNullException(NameOf(ex))
        End If

        Return TypeOf ex Is HttpRequestException OrElse TypeOf ex Is TaskCanceledException
    End Function

#End Region

#Region " Private Methods "

    ''' <summary>
    ''' Gets the value of the "status" property (matched case-insensitively) from a login response JSON object.
    ''' </summary>
    '''
    ''' <param name="json">
    ''' The login response JSON.
    ''' </param>
    '''
    ''' <param name="statusCode">
    ''' The HTTP status code of the login response (only used in error messages).
    ''' </param>
    '''
    ''' <returns>
    ''' The "status" property value.
    ''' </returns>
    '''
    ''' <exception cref="FormatException">
    ''' Thrown when <paramref name="json"/> is not a valid JSON object.
    ''' </exception>
    '''
    ''' <exception cref="InvalidOperationException">
    ''' Thrown when the JSON object does not contain a string "status" property.
    ''' </exception>
    Private Shared Function GetLoginResponseStatus(json As String, statusCode As HttpStatusCode) As String

        Dim document As JsonDocument
        Try
            document = JsonDocument.Parse(json)
        Catch ex As Exception When TypeOf ex Is JsonException OrElse TypeOf ex Is ArgumentException
            Throw New FormatException($"Invalid JSON returned from login request (HTTP {CInt(statusCode)}). Raw response: {json}", ex)
        End Try

        Using document
            If document.RootElement.ValueKind <> JsonValueKind.Object Then
                Throw New FormatException($"Invalid JSON returned from login request (HTTP {CInt(statusCode)}), an object was expected. Raw response: {json}")
            End If

            For Each prop As JsonProperty In document.RootElement.EnumerateObject()
                If String.Equals(prop.Name, "status", StringComparison.OrdinalIgnoreCase) Then
                    If prop.Value.ValueKind <> JsonValueKind.String Then
                        Throw New InvalidOperationException($"Login response JSON 'status' field is not a string: {json}")
                    End If
                    Return prop.Value.GetString()
                End If
            Next prop
        End Using

        Throw New InvalidOperationException($"Login response JSON does not contain 'status' field: {json}")
    End Function

    ''' <summary>
    ''' Creates the HTTP client used by this instance.
    ''' </summary>
    '''
    ''' <param name="handler">
    ''' The HTTP message handler.
    ''' </param>
    '''
    ''' <returns>
    ''' The HTTP client.
    ''' </returns>
    Private Shared Function CreateClient(handler As HttpMessageHandler) As HttpClient

        Dim client As New HttpClient(handler, disposeHandler:=False) With {
            .Timeout = AppGlobals.HttpClientTimeOut
        }
        client.DefaultRequestHeaders.Clear()
        client.DefaultRequestHeaders.Add("User-Agent", GoodFonAuth.UserAgent)
        client.DefaultRequestHeaders.Add("Accept", "application/json, text/html, */*")
        Return client
    End Function

    ''' <summary>
    ''' Releases managed and, optionally, unmanaged resources.
    ''' </summary>
    '''
    ''' <param name="disposing">
    ''' <see langword="True"/> to release both managed and unmanaged resources;
    ''' <see langword="False"/> to release only unmanaged resources.
    ''' </param>
    Private Sub Dispose(disposing As Boolean)

        If Not Me.disposedValue Then
            If disposing Then
                Me.Client.Dispose()
                Me.clientHandler.Dispose()
            End If

            Me.disposedValue = True
        End If
    End Sub

#End Region

End Class

#End Region
