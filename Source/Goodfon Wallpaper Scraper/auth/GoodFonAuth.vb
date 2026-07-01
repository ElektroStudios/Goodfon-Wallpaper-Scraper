#Region " Option Statements "

Option Explicit On
Option Strict On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Collections.Generic
Imports System.Net
Imports System.Net.Http
Imports System.Threading.Tasks
Imports System.Web.Script.Serialization

Imports HtmlAgilityPack

#End Region

#Region " GoodFonAuth "

''' <summary>
''' Handles login to GoodFon.com website and provides an authenticated <see cref="HttpClient"/>.
''' </summary>
Public NotInheritable Class GoodFonAuth : Implements IDisposable

    ''' <summary>
    ''' Gets the authenticated HTTP client instance.
    ''' </summary>
    Public ReadOnly Property Client As HttpClient

    ''' <summary>
    ''' The HTTP client handler responsible for managing cookies and decompression options.
    ''' </summary>
    Private ReadOnly clientHandler As HttpClientHandler

    Public Property CookieContainer As New CookieContainer()

    ''' <summary>
    ''' Gets or sets a value indicating whether the daily wallpaper download limit has been reached on the website.
    ''' </summary>
    Public Property DailyLimitReached As Boolean = False

    Private Shared ReadOnly UserAgent As String = Guid.NewGuid().ToString()

    Private disposedValue As Boolean

    ''' <summary>
    ''' Initializes a new instance of the <see cref="GoodFonAuth"/> class.
    ''' </summary>
    Public Sub New()

        Me.clientHandler = New HttpClientHandler() With {
            .UseCookies = True,
            .CookieContainer = Me.CookieContainer,
            .AutomaticDecompression = DecompressionMethods.GZip Or DecompressionMethods.Deflate
        }

        Me.Client = New HttpClient(Me.clientHandler) With {
            .Timeout = AppGlobals.HttpClientTimeOut
        }
        Me.Client.DefaultRequestHeaders.Clear()
        Me.Client.DefaultRequestHeaders.Add("User-Agent", GoodFonAuth.UserAgent)
        Me.Client.DefaultRequestHeaders.Add("Accept", "application/json, text/html, */*")

    End Sub

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
    ''' returning <see langword="True"/> if authentication succeeds or <see langword="False"/> otherwise.
    ''' </returns>
    ''' 
    ''' <exception cref="InvalidOperationException"></exception>
    ''' <exception cref="FormatException"></exception>
    ''' <exception cref="UnauthorizedAccessException"></exception>
    Public Async Function LoginAsync(username As String, password As String) As Task(Of Boolean)

        Const loginUrl As String = "https://www.goodfon.com/auth/signin/"

        ' STEP 1 — GET login page (CSRF)
        Dim loginHtml As String = Await Me.Client.GetStringAsync(loginUrl)

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
              request As New HttpRequestMessage(HttpMethod.Post, loginUrl)

            request.Content = content
            request.Headers.Referrer = New Uri(loginUrl)

            Using response As HttpResponseMessage = Await Me.Client.SendAsync(request, HttpCompletionOption.ResponseContentRead)

                Dim resultJson As String = Await response.Content.ReadAsStringAsync()
#If DEBUG Then
                Console.WriteLine($"Goodfon.com login response JSON: {resultJson}")
#End If

                ' STEP 3 — Parse JSON
                Dim jsonDict As Dictionary(Of String, Object)
                Try
                    Dim serializer As New JavaScriptSerializer()

                    jsonDict = New Dictionary(Of String, Object)(
                        serializer.Deserialize(Of Dictionary(Of String, Object))(resultJson),
                        StringComparer.OrdinalIgnoreCase
                    )

                Catch ex As Exception
                    Throw New FormatException($"Invalid JSON returned from login request. Raw response: {resultJson}", ex)

                End Try

                If Not jsonDict.ContainsKey("status") Then
                    Throw New InvalidOperationException($"Login response JSON does not contain 'status' field: {resultJson}")
                End If

                Dim status As String = CStr(jsonDict("status"))
                If Not String.Equals(status, "success", StringComparison.OrdinalIgnoreCase) Then
                    Throw New UnauthorizedAccessException($"Login failed. Response JSON: {resultJson}")
                End If

                Return True

            End Using ' response
        End Using     ' content, request

    End Function

    ''' <summary>
    ''' Releases all resources used by the current <see cref="GoodFonAuth"/> instance.
    ''' </summary>
    Public Sub Dispose() Implements IDisposable.Dispose

        Me.Dispose(disposing:=True)
        GC.SuppressFinalize(Me)
    End Sub

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
                Me.Client?.Dispose()
                Me.clientHandler?.Dispose()
            End If

            Me.disposedValue = True
        End If
    End Sub

End Class

#End Region