#Region " Option Statements "

Option Explicit On
Option Strict On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Collections.Generic
Imports System.Net
Imports System.Net.Http
Imports System.Net.Http.Headers
Imports System.Text
Imports System.Threading
Imports System.Threading.Tasks

#End Region

#Region " FakeHttpMessageHandler "

''' <summary>
''' A fake <see cref="HttpMessageHandler"/> that returns queued responses in order,
''' and records the requests it receives. No network requests are performed.
''' </summary>
Friend NotInheritable Class FakeHttpMessageHandler : Inherits HttpMessageHandler

#Region " Fields "

    ''' <summary>
    ''' The queued response factories.
    ''' </summary>
    Private ReadOnly responses As New Queue(Of Func(Of HttpResponseMessage))()

#End Region

#Region " Properties "

    ''' <summary>
    ''' Gets the requests received, in order.
    ''' </summary>
    Friend ReadOnly Property Requests As New List(Of RecordedRequest)()

#End Region

#Region " Constants "

    ''' <summary>
    ''' The CSRF token included in the fake login page.
    ''' </summary>
    Friend Const CsrfToken As String = "TestCsrfToken123"

#End Region

#Region " Public Methods "

    ''' <summary>
    ''' Queues a response with the specified status code and body.
    ''' </summary>
    '''
    ''' <param name="statusCode">
    ''' The HTTP status code.
    ''' </param>
    '''
    ''' <param name="body">
    ''' The response body.
    ''' </param>
    '''
    ''' <param name="retryAfter">
    ''' The optional "Retry-After" header value.
    ''' </param>
    '''
    ''' <returns>
    ''' This instance, for chaining.
    ''' </returns>
    Friend Function Enqueue(statusCode As HttpStatusCode, body As String, Optional retryAfter As RetryConditionHeaderValue = Nothing) As FakeHttpMessageHandler

        Me.responses.Enqueue(
            Function() As HttpResponseMessage
                Dim response As New HttpResponseMessage(statusCode) With {
                    .Content = New StringContent(body, Encoding.UTF8)
                }
                If retryAfter IsNot Nothing Then
                    response.Headers.RetryAfter = retryAfter
                End If
                Return response
            End Function)
        Return Me
    End Function

    ''' <summary>
    ''' Queues an exception to be thrown by the next request (e.g., a network error).
    ''' </summary>
    '''
    ''' <param name="ex">
    ''' The exception to throw.
    ''' </param>
    '''
    ''' <returns>
    ''' This instance, for chaining.
    ''' </returns>
    Friend Function EnqueueException(ex As Exception) As FakeHttpMessageHandler

        Me.responses.Enqueue(Function() As HttpResponseMessage
                                 Throw ex
                             End Function)
        Return Me
    End Function

    ''' <summary>
    ''' Queues the GoodFon.com login page (HTTP 200), containing the CSRF token.
    ''' </summary>
    '''
    ''' <returns>
    ''' This instance, for chaining.
    ''' </returns>
    Friend Function EnqueueLoginPage() As FakeHttpMessageHandler

        Return Me.Enqueue(HttpStatusCode.OK,
            "<html><body><form action=/auth/signin/ class=js-auth-signin method=post>" &
            $"<input name=csrfmiddlewaretoken type=hidden value={FakeHttpMessageHandler.CsrfToken}>" &
            "<input name=login><input name=password type=password>" &
            "<div class=""sigin__form__txt_err js-auth-ratelimit_err"">Too many attempts. Please wait a few minutes</div>" &
            "</form></body></html>")
    End Function

    ''' <summary>
    ''' Queues the login page followed by a login POST response.
    ''' </summary>
    '''
    ''' <param name="statusCode">
    ''' The HTTP status code of the login POST response.
    ''' </param>
    '''
    ''' <param name="body">
    ''' The body of the login POST response.
    ''' </param>
    '''
    ''' <param name="retryAfter">
    ''' The optional "Retry-After" header value of the login POST response.
    ''' </param>
    '''
    ''' <returns>
    ''' This instance, for chaining.
    ''' </returns>
    Friend Function EnqueueLoginAttempt(statusCode As HttpStatusCode, body As String, Optional retryAfter As RetryConditionHeaderValue = Nothing) As FakeHttpMessageHandler

        Return Me.EnqueueLoginPage().Enqueue(statusCode, body, retryAfter)
    End Function

#End Region

#Region " Protected Methods "

    ''' <summary>
    ''' Records the request and returns the next queued response.
    ''' </summary>
    '''
    ''' <param name="request">
    ''' The HTTP request message.
    ''' </param>
    '''
    ''' <param name="cancellationToken">
    ''' The cancellation token.
    ''' </param>
    '''
    ''' <returns>
    ''' The next queued response.
    ''' </returns>
    '''
    ''' <exception cref="InvalidOperationException">
    ''' Thrown when there are no more queued responses.
    ''' </exception>
    Protected Overrides Function SendAsync(request As HttpRequestMessage, cancellationToken As CancellationToken) As Task(Of HttpResponseMessage)

        Dim body As String = If(request.Content Is Nothing, Nothing, request.Content.ReadAsStringAsync().GetAwaiter().GetResult())
        Dim referrer As Uri = request.Headers.Referrer
        Me.Requests.Add(New RecordedRequest(request.Method, request.RequestUri, body, referrer))

        If Me.responses.Count = 0 Then
            Throw New InvalidOperationException($"Unexpected request (no queued responses): {request.Method} {request.RequestUri}")
        End If

        Return Task.FromResult(Me.responses.Dequeue().Invoke())
    End Function

#End Region

#Region " RecordedRequest "

    ''' <summary>
    ''' Represents a request received by <see cref="FakeHttpMessageHandler"/>.
    ''' </summary>
    Friend NotInheritable Class RecordedRequest

        ''' <summary>
        ''' Gets the HTTP method.
        ''' </summary>
        Friend ReadOnly Property Method As HttpMethod

        ''' <summary>
        ''' Gets the request URI.
        ''' </summary>
        Friend ReadOnly Property RequestUri As Uri

        ''' <summary>
        ''' Gets the request body, or <see langword="Nothing"/> if the request has no content.
        ''' </summary>
        Friend ReadOnly Property Body As String

        ''' <summary>
        ''' Gets the Referer header value, or <see langword="Nothing"/> if not set.
        ''' </summary>
        Friend ReadOnly Property Referrer As Uri

        ''' <summary>
        ''' Initializes a new instance of the <see cref="RecordedRequest"/> class.
        ''' </summary>
        '''
        ''' <param name="method">
        ''' The HTTP method.
        ''' </param>
        '''
        ''' <param name="requestUri">
        ''' The request URI.
        ''' </param>
        '''
        ''' <param name="body">
        ''' The request body.
        ''' </param>
        '''
        ''' <param name="referrer">
        ''' The Referer header value.
        ''' </param>
        Friend Sub New(method As HttpMethod, requestUri As Uri, body As String, referrer As Uri)

            Me.Method = method
            Me.RequestUri = requestUri
            Me.Body = body
            Me.Referrer = referrer
        End Sub

    End Class

#End Region

End Class

#End Region
