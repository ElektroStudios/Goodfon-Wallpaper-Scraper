Option Explicit On
Option Strict On
Option Infer Off

Imports System.Collections.Generic
Imports System.IO
Imports System.Net
Imports System.Net.Http
Imports System.Reflection
Imports System.Threading.Tasks
Imports Microsoft.VisualBasic

''' <summary>Exercises the real download entry point with simulated HTTP responses and no VPN access.</summary>
<TestClass, DoNotParallelize>
Public NotInheritable Class DownloadFallbackTests

    ''' <summary>The wallpaper retried by each scenario.</summary>
    Private Const WallpaperUrl As String = "https://www.goodfon.com/wallpaper/test.html"
    ''' <summary>A wallpaper page with an original download link.</summary>
    Private Const WallpaperHtml As String = "<a class='wallpaper__download__rbut js-size' href='/download/test/'>Download</a>"
    ''' <summary>The fields changed by the fixture and their original values.</summary>
    Private ReadOnly savedFields As New Dictionary(Of String, Object)()
    ''' <summary>Accounts owned by this fixture.</summary>
    Private ReadOnly accounts As New List(Of GoodFonAuth)()
    ''' <summary>Captured diagnostic output.</summary>
    Private output As StringWriter
    ''' <summary>The original console output.</summary>
    Private originalOutput As TextWriter
    ''' <summary>The anonymous state shared by the application.</summary>
    Private state As AnonymousIpState

    ''' <summary>Isolates global state and prevents real VPN calls by activating the cooldown.</summary>
    <TestInitialize>
    Public Sub Initialize()
        For Each name As String In {"accountPool", "accountsFile", "accountFallbackNoticeShown", "totalFailedWallpaperCount", "totalSkippedWallpaperCount"}
            Me.savedFields.Add(name, GetField(name).GetValue(Nothing))
        Next
        GetField("accountPool").SetValue(Nothing, New GoodFonAccountPool(Array.Empty(Of GoodFonAuth)()))
        GetField("accountsFile").SetValue(Nothing, Nothing)
        GetField("accountFallbackNoticeShown").SetValue(Nothing, 0)
        Me.state = DirectCast(GetField("anonymousIp").GetValue(Nothing), AnonymousIpState)
        Me.state.ReportRotationSucceeded()
        Me.state.ReportRotationFailed()
        Me.originalOutput = Console.Out
        Me.output = New StringWriter()
        Console.SetOut(Me.output)
    End Sub

    ''' <summary>Restores the console and application state and disposes fixture accounts.</summary>
    <TestCleanup>
    Public Sub Cleanup()
        Console.SetOut(Me.originalOutput)
        Me.output.Dispose()
        For Each auth As GoodFonAuth In Me.accounts
            auth.Dispose()
        Next
        For Each entry As KeyValuePair(Of String, Object) In Me.savedFields
            GetField(entry.Key).SetValue(Nothing, entry.Value)
        Next
        Me.state.ReportRotationSucceeded()
    End Sub

    ''' <summary>A queued anonymous call must switch clients before sending its first request.</summary>
    <TestMethod>
    Public Async Function KnownIpLimit_UsesAccountWithoutAnonymousRequest() As Task
        Dim accountHandler As New FakeHttpMessageHandler()
        accountHandler.Enqueue(HttpStatusCode.OK, "<html>No download button</html>")
        Me.AddAccount(accountHandler)
        Me.state.ReportLimitReached()
        Dim anonymousHandler As New FakeHttpMessageHandler()
        Using client As New HttpClient(anonymousHandler)
            Await DownloadAsync(client).ConfigureAwait(False)
        End Using
        Assert.AreEqual(0, anonymousHandler.Requests.Count)
        Assert.AreEqual(1, accountHandler.Requests.Count)
        Assert.AreEqual(WallpaperUrl, accountHandler.Requests(0).RequestUri.AbsoluteUri)
        StringAssert.Contains(Me.output.ToString(), "Free wallpapers will be downloaded using the user-accounts")
    End Function

    ''' <summary>A newly detected limit retries the same wallpaper with an account during VPN cooldown.</summary>
    <TestMethod>
    Public Async Function NewIpLimit_DuringVpnCooldown_RetriesWithAccount() As Task
        Dim accountHandler As New FakeHttpMessageHandler()
        accountHandler.Enqueue(HttpStatusCode.OK, "<html>No download button</html>")
        Me.AddAccount(accountHandler)
        Dim anonymousHandler As FakeHttpMessageHandler = LimitHandler()
        Using client As New HttpClient(anonymousHandler)
            Await DownloadAsync(client).ConfigureAwait(False)
        End Using
        Assert.AreEqual(2, anonymousHandler.Requests.Count)
        Assert.AreEqual(1, accountHandler.Requests.Count)
        Assert.IsTrue(Me.state.IsLimitReached)
        Assert.IsFalse(Me.output.ToString().Contains("Proton VPN rotation triggered"))
    End Function

    ''' <summary>An exhausted account must not cause the current wallpaper to be abandoned.</summary>
    <TestMethod>
    Public Async Function AccountLimit_RetriesSameWallpaperWithNextAccount() As Task
        Dim firstHandler As FakeHttpMessageHandler = LimitHandler()
        Dim first As GoodFonAuth = Me.AddAccount(firstHandler)
        Dim secondHandler As New FakeHttpMessageHandler()
        secondHandler.Enqueue(HttpStatusCode.OK, "<html>No download button</html>")
        Me.AddAccount(secondHandler)
        Await DownloadAsync(first.Client, first).ConfigureAwait(False)
        Assert.IsTrue(first.DailyLimitReached)
        Assert.AreEqual(1, secondHandler.Requests.Count)
        Assert.AreEqual(WallpaperUrl, secondHandler.Requests(0).RequestUri.AbsoluteUri)
    End Function

    ''' <summary>Repeated account limits terminate after all accounts have been tried.</summary>
    <TestMethod>
    Public Async Function AllAccountsReachLimit_TerminatesWithoutAnonymousRetry() As Task
        Dim firstHandler As FakeHttpMessageHandler = LimitHandler()
        Dim first As GoodFonAuth = Me.AddAccount(firstHandler)
        Dim secondHandler As FakeHttpMessageHandler = LimitHandler()
        Me.AddAccount(secondHandler)
        Dim result As TriState = Await DownloadAsync(first.Client, first).ConfigureAwait(False)
        Assert.AreEqual(TriState.False, result)
        Assert.AreEqual(2, firstHandler.Requests.Count)
        Assert.AreEqual(2, secondHandler.Requests.Count)
        StringAssert.Contains(Me.output.ToString(), "All logged-in accounts have reached their daily download limit")
    End Function

    ''' <summary>An exhausted anonymous IP with no accounts does not issue more HTTP requests.</summary>
    <TestMethod>
    Public Async Function KnownIpLimit_EmptyPool_ReportsOnceAndSendsNoRequests() As Task
        Me.state.ReportLimitReached()
        Dim handler As New FakeHttpMessageHandler()
        Using client As New HttpClient(handler)
            Assert.AreEqual(TriState.False, Await DownloadAsync(client).ConfigureAwait(False))
            Assert.AreEqual(TriState.False, Await DownloadAsync(client).ConfigureAwait(False))
        End Using
        Assert.AreEqual(0, handler.Requests.Count)
        Dim message As String = Me.output.ToString()
        StringAssert.Contains(message, "No accounts are enabled")
        Assert.AreEqual(message.IndexOf("[Auth]", StringComparison.Ordinal), message.LastIndexOf("[Auth]", StringComparison.Ordinal))
    End Function

    ''' <summary>Configured but unsuccessfully authenticated accounts are reported distinctly.</summary>
    <TestMethod>
    Public Async Function KnownIpLimit_FailedLogins_ReportsLoginFailure() As Task
        Dim config As New GoodFonAccountsFile() With {.Accounts = New List(Of GoodFonAccountConfig)()}
        config.Accounts.Add(New GoodFonAccountConfig() With {.Enabled = True})
        GetField("accountsFile").SetValue(Nothing, config)
        Me.state.ReportLimitReached()
        Using client As New HttpClient(New FakeHttpMessageHandler())
            Await DownloadAsync(client).ConfigureAwait(False)
        End Using
        StringAssert.Contains(Me.output.ToString(), "No enabled account logged in successfully")
        StringAssert.Contains(Me.output.ToString(), "Configured: 1; enabled: 1; logged-in: 0")
    End Function

    ''' <summary>Without an IP limit, free wallpapers still use the anonymous client.</summary>
    <TestMethod>
    Public Async Function AvailableIp_KeepsAnonymousDownloads() As Task
        Dim accountHandler As New FakeHttpMessageHandler()
        Me.AddAccount(accountHandler)
        Dim handler As New FakeHttpMessageHandler()
        handler.Enqueue(HttpStatusCode.OK, "<html>No download button</html>")
        Using client As New HttpClient(handler)
            Await DownloadAsync(client).ConfigureAwait(False)
        End Using
        Assert.AreEqual(1, handler.Requests.Count)
        Assert.AreEqual(0, accountHandler.Requests.Count)
    End Function

    ''' <summary>Original login-required wallpapers retain their account fallback.</summary>
    <TestMethod>
    Public Async Function ForbiddenWallpaper_StillUsesAccount() As Task
        Dim accountHandler As New FakeHttpMessageHandler()
        accountHandler.Enqueue(HttpStatusCode.OK, "<html>No download button</html>")
        Me.AddAccount(accountHandler)
        Dim handler As New FakeHttpMessageHandler()
        handler.Enqueue(HttpStatusCode.Forbidden, "Login required")
        Using client As New HttpClient(handler)
            Await DownloadAsync(client).ConfigureAwait(False)
        End Using
        Assert.AreEqual(1, accountHandler.Requests.Count)
        Assert.IsFalse(Me.state.IsLimitReached)
    End Function

    ''' <summary>Creates a wallpaper page followed by its daily-limit response.</summary>
    Private Shared Function LimitHandler() As FakeHttpMessageHandler
        Return New FakeHttpMessageHandler().Enqueue(HttpStatusCode.OK, WallpaperHtml).
            Enqueue(HttpStatusCode.OK, GoodfonHtmlParser.DailyLimitReachedText)
    End Function

    ''' <summary>Adds an account and rebuilds the fixture pool.</summary>
    ''' <param name="handler">The simulated account HTTP handler.</param>
    ''' <returns>The account added to the pool.</returns>
    Private Function AddAccount(handler As FakeHttpMessageHandler) As GoodFonAuth
        Dim auth As New GoodFonAuth(handler)
        Me.accounts.Add(auth)
        GetField("accountPool").SetValue(Nothing, New GoodFonAccountPool(Me.accounts))
        Return auth
    End Function

    ''' <summary>Resolves an application field without exposing mutable state in production APIs.</summary>
    ''' <param name="name">The field name.</param>
    ''' <returns>The matching field.</returns>
    Private Shared Function GetField(name As String) As FieldInfo
        Dim field As FieldInfo = GetType(Program).GetField(name, BindingFlags.NonPublic Or BindingFlags.Static)
        If field Is Nothing Then Throw New MissingFieldException(GetType(Program).FullName, name)
        Return field
    End Function

    ''' <summary>Invokes the production download method; test responses stop before file writes.</summary>
    ''' <param name="client">The initial HTTP client.</param>
    ''' <param name="auth">The initial account, if any.</param>
    ''' <returns>The production download task.</returns>
    Private Shared Function DownloadAsync(client As HttpClient, Optional auth As GoodFonAuth = Nothing) As Task(Of TriState)
        Dim method As MethodInfo = GetType(Program).GetMethod("DownloadOriginalWallpaperAsync", BindingFlags.NonPublic Or BindingFlags.Static)
        If method Is Nothing Then Throw New MissingMethodException(GetType(Program).FullName, "DownloadOriginalWallpaperAsync")
        Return DirectCast(method.Invoke(Nothing, {client, WallpaperUrl, Path.GetTempPath(), 1, 1, auth}), Task(Of TriState))
    End Function
End Class
