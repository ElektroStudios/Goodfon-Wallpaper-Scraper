#Region " Option Statements "

Option Explicit On
Option Strict On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Net
Imports System.Net.Http
Imports System.Text
Imports System.Threading
Imports System.Threading.Tasks

Imports DevCase.Core.Networking.Common

Imports HtmlAgilityPack

Imports HtmlDocument = HtmlAgilityPack.HtmlDocument

#End Region

''' <summary>
''' Represents the main module of the GoodFon Wallpaper Scraper application.
''' </summary>
Public Module Program

#Region " Fields "

    ''' <summary>
    ''' The list of search terms to query on GoodFon.com website.
    ''' </summary>
    Private searchTerms As HashSet(Of String)

    ''' <summary>
    ''' The list of keywords used to filter out unwanted results. 
    ''' <para></para>
    ''' If any of these keywords are present in the wallpaper page URL, it will be skipped.
    ''' </summary>
    Private forbiddenKeywords As HashSet(Of String)

    ''' <summary>
    ''' In-memory cache of wallpaper page URLs that have already been processed.
    ''' <para></para>
    ''' Used to prevent duplicate processing within the current session.
    ''' </summary>
    Private processedUrlHashes As HashSet(Of String)

    ''' <summary>
    ''' The total number of wallpapers successfully saved during the current execution.
    ''' </summary>
    Private totalSavedWallpaperCount As Integer

    ''' <summary>
    ''' The total number of wallpapers skipped due already downloaded or forbidden keyword matches.
    ''' </summary>
    Private totalSkippedWallpaperCount As Integer

    ''' <summary>
    ''' The total number of wallpaper downloads failed.
    ''' </summary>
    Private totalFailedWallpaperCount As Integer

    ''' <summary>
    ''' Tracks the number of successful downloads performed on the current VPN IP address.
    ''' </summary>
    Private downloadsOnCurrentIp As Integer

    ''' <summary>
    ''' Atomic flag: 1 = a VPN rotation is already in progress, 0 = free.
    ''' Any thread that loses the race skips the call entirely instead of queuing.
    ''' </summary>
    Private vpnRotationInProgress As Integer = 0

    ''' <summary>
    ''' Lock object to call <see cref="Program.WriteColoredLine(String, ConsoleColor)"/> with thread-safety.
    ''' </summary>
    Private ReadOnly consoleLock As New Object()

    ''' <summary>
    ''' Lock object to manipulate <see cref="Program.processedUrlHashes"/> with thread-safety.
    ''' </summary>
    Private ReadOnly historyLock As New Object()

    ''' <summary>
    ''' Thread-safe random: each OS thread gets its own independently seeded instance,
    ''' so concurrent calls to .Next() never corrupt shared state.
    ''' </summary>
    Private ReadOnly threadLocalRng As New ThreadLocal(Of Random)(Function() New Random(Environment.TickCount))

    ''' <summary>
    ''' The <see cref="Stopwatch"/> instance used to track the elapsed time for processing the current page 
    ''' to prevent potential hangs or long-running requests.
    ''' </summary>
    Private ReadOnly PageTimeoutWatch As New Stopwatch()

    ''' <summary>
    ''' The pool of authenticated accounts used to download restricted (adult) wallpapers on GoodFon.com.
    ''' <para></para>
    ''' The pool is empty when no user-account is configured, enabled or successfully logged-in.
    ''' </summary>
    Private accountPool As New GoodFonAccountPool(Array.Empty(Of GoodFonAuth)())

    ''' <summary>
    ''' The account authentication configurations.
    ''' </summary>
    Private accountsFile As GoodFonAccountsFile

    ''' <summary>
    ''' Tracks whether the anonymous daily download limit has been reached on the current IP address, 
    ''' and the result of the VPN server rotations.
    ''' </summary>
    Private ReadOnly anonymousIp As New AnonymousIpState()

    ''' <summary>
    ''' Last reported fallback status: 0 = none, 1 = available, 2 = unavailable.
    ''' </summary>
    Private accountFallbackNoticeShown As Integer = 0

    ''' <summary>
    ''' The encoding used for console output, 
    ''' and reading and writing to text files.
    ''' <para></para>
    ''' It is set to UTF-8 with BOM (Byte Order Mark).
    ''' </summary>
    Friend ReadOnly ConsoleEncoding As New UTF8Encoding(encoderShouldEmitUTF8Identifier:=False)

    ''' <summary>
    ''' The <see cref="CultureInfo"/> instance representing the "en-US" culture.
    ''' </summary>
    Friend ReadOnly CultureInfoEnUs As New CultureInfo("en-US")

#End Region

#Region " Entry Point "

    ''' <summary>
    ''' The main entry point of the application.
    ''' </summary>
    <DebuggerStepperBoundary>
    Public Sub Main()
        Thread.CurrentThread.CurrentCulture = Program.CultureInfoEnUs
        Thread.CurrentThread.CurrentUICulture = Program.CultureInfoEnUs

        Console.OutputEncoding = Program.ConsoleEncoding
        Console.BackgroundColor = ConsoleColor.Black
        Console.ForegroundColor = ConsoleColor.White

#If DEBUG Then
        Console.Title = AppGlobals.AppTitleAndVersion
#End If
        Program.WriteColoredLine(" " & AppGlobals.AppTitleAndVersion, ConsoleColor.Cyan)
        Program.WriteColoredLine(" " & AppGlobals.RepositoryUrl, ConsoleColor.DarkCyan)
        Console.WriteLine("╭──────────────────────────────────────────────────────────────────────────────────╮")
        Console.WriteLine("│ Purpose:                                                                         │")
        Console.WriteLine("│   This program automates the process of downloading wallpapers from Goodfon.com  │")
        Console.WriteLine("│   based on user-defined rules that must be configured in the following files:    │")
        Console.WriteLine("│                                                                                  │")
        Console.WriteLine("│     - <PROGRAM DIRECTORY>\Config\Search Terms.txt                                │")
        Console.WriteLine("│     - <PROGRAM DIRECTORY>\Config\Forbidden Keywords.txt                          │")
        Console.WriteLine("│                                                                                  │")
        Console.WriteLine("│   The program automatically processes search results and tag pages, and saves    │")
        Console.WriteLine("│   downloaded wallpapers at their original resolution into the following dir:     │")
        Console.WriteLine("│                                                                                  │")
        Console.WriteLine("│     - <PROGRAM DIRECTORY>\Output\                                                │")
        Console.WriteLine("│                                                                                  │")
        Console.WriteLine("│   GoodFon user-accounts can also be configured in the following file:            │")
        Console.WriteLine("│     - <PROGRAM DIRECTORY>\Config\Accounts.json                                   │")
        Console.WriteLine("│                                                                                  │")
        Console.WriteLine("│   The user-accounts are used to download the wallpapers that require a login.    │")
        Console.WriteLine("│   They are also used to keep downloading the free wallpapers when the daily      │")
        Console.WriteLine("│   download limit of the current IP is reached and the VPN server cannot be       │")
        Console.WriteLine("│   rotated (each account has its own daily download limit).                       │")
        Console.WriteLine("│                                                                                  │")
        Console.WriteLine("│ Requeriments:                                                                    │")
        Console.WriteLine("│   Since Goodfon.com has a hard daily limit of 80 maximum downloads per IP,       │")
        Console.WriteLine("│   this program requires the 'Proton VPN' client for Windows to be running        │")
        Console.WriteLine("│   and conencted to a server. The program will rotate the server when needed.     │")
        Console.WriteLine("│                                                                                  │")
        Console.WriteLine("│   Download Proton VPN client for Windows here: https://protonvpn.com/download    │")
        Console.WriteLine("│                                                                                  │")
        Console.WriteLine("│ [!] Disclaimer:                                                                  │")
        Console.WriteLine("│   This program is shared 'as-is', without any warranty; Use it at your own risk. │")
        Console.WriteLine("╰──────────────────────────────────────────────────────────────────────────────────╯")
        Console.WriteLine()

        Try
            Program.WriteColoredLine("Loading user configuration and cached items...", Console.ForegroundColor)
            Program.LoadConfigFiles()
            Program.WriteColoredLine($"  Loaded {Program.searchTerms.Count:N0} search terms from config file.", ConsoleColor.Green)
            Program.WriteColoredLine($"  Loaded {Program.forbiddenKeywords.Count:N0} forbidden keywords from config file.", ConsoleColor.Green)

            Program.LoadUrlsHistoryHashes()
            If Program.processedUrlHashes.Count > 0 Then
                Program.WriteColoredLine($"  Loaded {Program.processedUrlHashes.Count:N0} URL history hashes from history file.", ConsoleColor.Green)
            Else
                Program.WriteColoredLine($"  Loaded {Program.processedUrlHashes.Count:N0} URL history hashes from history file.", ConsoleColor.Green)
            End If

            Program.accountsFile = Program.LoadGoodFonAccounts()
            If Program.accountsFile IsNot Nothing Then
                Program.WriteColoredLine($"  Loaded {Program.accountsFile.Accounts.Count} user-account credentials from config file.", ConsoleColor.Green)
            Else
                Program.WriteColoredLine($"  Loaded 0 user-account credentials.", ConsoleColor.Green)
            End If
            Console.WriteLine()

#If DEBUG Then
            Program.WriteColoredLine("Press 'Y' key to start fetching wallpapers, or 'Escape' key to exit...", ConsoleColor.Yellow)
            Program.WriteColoredLine("[!] This message only appears in DEBUG mode to prevent accidental execution.", ConsoleColor.Yellow)
            Do
                Dim keyInfo As ConsoleKeyInfo = Console.ReadKey(intercept:=True)
                If keyInfo.Key = ConsoleKey.Y Then
                    Exit Do
                ElseIf keyInfo.Key = ConsoleKey.Escape Then
                    Environment.Exit(0)
                End If
            Loop
            Console.WriteLine()
#End If

            Program.RotateVpnServerAsync("Initial VPN rotation to ensure a clean IP with no prior download activity").Wait()
            Console.WriteLine()

            If (Program.accountsFile IsNot Nothing) Then

                Dim loginSettings As GoodFonLoginSettings = Program.accountsFile.LoginSettings

                Program.WriteColoredLine($"Loging-in to GoodFon.com website using the user-defined credentials...", ConsoleColor.White)
                Program.WriteColoredLine($"  (Login settings: rate-limit wait {loginSettings.RateLimitWaitSeconds:N0} seconds, max. {loginSettings.MaxLoginAttempts:N0} attempts per account, {loginSettings.DelayBetweenLoginsSeconds:N0} seconds between logins.)", ConsoleColor.DarkGray)
                Console.WriteLine()

                Dim authsList As New List(Of GoodFonAuth)
                Dim isFirstLogin As Boolean = True

                ' Used to estimate the remaining time of the login procedure:
                ' (pending logins x average login duration) + (pending delays x delay between logins).
                Dim pendingEnabledAccounts As Integer = Program.accountsFile.Accounts.Where(Function(account As GoodFonAccountConfig) account.Enabled).Count()
                Dim loginDurationsSum As TimeSpan = TimeSpan.Zero
                Dim completedLogins As Integer = 0
                Dim loginWatch As New Stopwatch()

                Dim accountsCount As Integer = Program.accountsFile.Accounts.Count
                For accountIndex As Integer = 0 To accountsCount - 1

                    Dim accountConfig As GoodFonAccountConfig = Program.accountsFile.Accounts(accountIndex)
                    Dim remainingAccounts As Integer = accountsCount - accountIndex - 1

                    Dim etaText As String = String.Empty
                    If accountConfig.Enabled Then
                        Dim pendingDelays As Integer = If(isFirstLogin, pendingEnabledAccounts - 1, pendingEnabledAccounts)
                        Dim averageLogin As TimeSpan = If(completedLogins = 0, TimeSpan.Zero, TimeSpan.FromTicks(loginDurationsSum.Ticks \ completedLogins))
                        Dim loginEta As TimeSpan = TimeSpan.FromTicks(averageLogin.Ticks * pendingEnabledAccounts) +
                                                   TimeSpan.FromSeconds(CDbl(loginSettings.DelayBetweenLoginsSeconds) * Math.Max(0, pendingDelays))
                        etaText = $" [ETA: {EtaEstimator.FormatDuration(loginEta)}]"
                    End If
                    Program.WriteColoredLine($"  [Account {accountIndex + 1:N0} of {accountsCount:N0}] ({remainingAccounts:N0} remaining, {authsList.Count:N0} logged-in so far){etaText}", ConsoleColor.Cyan)
#If DEBUG Then
                    Program.WriteColoredLine($"  - Username: {accountConfig.Username}", ConsoleColor.White)
                    Program.WriteColoredLine($"  - Password: {accountConfig.Password}", ConsoleColor.White)
#Else
                    Program.WriteColoredLine($"  - Username: {ConfigFileParser.MaskCredential(accountConfig.Username)}", ConsoleColor.White)
                    Program.WriteColoredLine($"  - Password: {ConfigFileParser.MaskCredential(accountConfig.Password)}", ConsoleColor.White)
#End If

                    If accountConfig.Enabled Then

                        ' Pause between consecutive logins to reduce the chance of triggering the website login rate-limit.
                        If Not isFirstLogin AndAlso loginSettings.DelayBetweenLoginsSeconds > 0 Then
                            Dim loginAt As Date = Date.Now.AddSeconds(loginSettings.DelayBetweenLoginsSeconds)
                            Program.WriteColoredLine($"  Waiting {loginSettings.DelayBetweenLoginsSeconds:N0} seconds before attempting to log in (at {loginAt:HH:mm:ss})...", ConsoleColor.DarkGray)
                            Thread.Sleep(TimeSpan.FromSeconds(loginSettings.DelayBetweenLoginsSeconds))
                        End If
                        isFirstLogin = False
                        loginWatch.Restart()

                        Dim auth As New GoodFonAuth()
                        Try
                            auth.LoginWithRetryAsync(accountConfig.Username, accountConfig.Password, loginSettings,
                                                     AddressOf Program.OnLoginRetry,
                                                     Function(waitTime As TimeSpan) Task.Delay(waitTime)).GetAwaiter().GetResult()
                            authsList.Add(auth)
                            Program.WriteColoredLine($"  Login successful.", ConsoleColor.Green)

                        Catch ex As GoodFonRateLimitException
                            auth.Dispose()
                            Program.WriteColoredLine($"  Login error: still rate-limited by the website after {loginSettings.MaxLoginAttempts:N0} attempts (""Too many attempts. Please wait a few minutes""). Consider increasing 'RateLimitWaitSeconds' or 'MaxLoginAttempts' in Accounts.json.", ConsoleColor.Red)

                        Catch ex As TaskCanceledException
                            auth.Dispose()
                            Program.WriteColoredLine($"  Login request has timed out ({AppGlobals.HttpClientTimeOutDisplay} minutes) after {loginSettings.MaxLoginAttempts:N0} attempts.", ConsoleColor.Red)

                        Catch ex As HttpRequestException
                            auth.Dispose()
                            Program.WriteColoredLine($"  Login error after {loginSettings.MaxLoginAttempts:N0} attempts: {Program.GetInnermostMessage(ex)}", ConsoleColor.Red)

                        Catch ex As Exception
                            auth.Dispose()
                            Program.WriteColoredLine($"  Login error: {Program.GetInnermostMessage(ex)}", ConsoleColor.Red)

                        End Try

                        loginDurationsSum += loginWatch.Elapsed
                        completedLogins += 1
                        pendingEnabledAccounts -= 1
                    Else
                        Program.WriteColoredLine($"  Account is disabled.", ConsoleColor.Yellow)

                    End If

                    Console.WriteLine()
                Next accountIndex

                Program.WriteColoredLine($"Logged-in {authsList.Count:N0} of {accountsCount:N0} user-accounts.", If(authsList.Count = 0, ConsoleColor.Yellow, ConsoleColor.Green))
                Console.WriteLine()

                Program.accountPool = New GoodFonAccountPool(authsList)
                AddHandler Program.accountPool.AccountLimitReset, AddressOf Program.AccountPool_AccountLimitReset
                AddHandler Program.accountPool.AllAccountsExhausted, AddressOf Program.AccountPool_AllAccountsExhausted
            End If

            Using client As New HttpClient(New HttpClientHandler() With {
               .UseCookies = False,
               .AutomaticDecompression = DecompressionMethods.GZip Or DecompressionMethods.Deflate
            }) With {
                .Timeout = AppGlobals.HttpClientTimeOut
            }

                client.DefaultRequestHeaders.Clear()
                client.DefaultRequestHeaders.Add("User-Agent", Guid.NewGuid().ToString())
                client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8")
                client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9")

                Dim searchTermsCount As Integer = Program.searchTerms.Count
                For termIndex As Integer = 0 To searchTermsCount - 1

                    Dim searchTerm As String = Program.searchTerms(termIndex)

                    Program.WriteColoredLine($"Processing search term {termIndex + 1:N0} of {searchTermsCount:N0}: ""{searchTerm}"" ...", ConsoleColor.White)
                    Console.WriteLine()

                    Dim outputDirPath As String = $"{AppGlobals.OutputDirPath}\{searchTerm}"

                    Dim wallpaperUserUrl As String = UrlHelper.BuildUserProfileUrl(searchTerm)
                    Program.WriteColoredLine($"Discovering user profile page: {wallpaperUserUrl}", ConsoleColor.Cyan)
                    Program.ProcessUrl(wallpaperUserUrl, client, outputDirPath).Wait()

                    For tagIndex As Integer = 0 To 3
                        Dim wallpaperTagUrl As String = UrlHelper.BuildTagUrl(searchTerm, tagIndex)

                        Program.WriteColoredLine($"Discovering tag page: {wallpaperTagUrl}", ConsoleColor.Cyan)
                        Program.ProcessUrl(wallpaperTagUrl, client, outputDirPath).Wait()
                    Next tagIndex

                    Dim wallpaperSearchUrl As String = UrlHelper.BuildSearchUrl(searchTerm)
                    Program.WriteColoredLine($"Discovering search query results: {wallpaperSearchUrl}", ConsoleColor.Cyan)
                    Program.ProcessUrl(wallpaperSearchUrl, client, outputDirPath).Wait()
#If DEBUG Then
                    Thread.CurrentThread.Join(0) ' Prevents ContextSwitchDeadlock on long-running iterations.
#End If
                Next termIndex
            End Using

        Catch ex As Exception
            Console.WriteLine()
            Dim errMsg As String = If(ex.InnerException IsNot Nothing, ex.InnerException.Message, ex.Message)
            Program.ExitWithMessage($"FATAL ERROR 0x{ex.HResult:X8}: {errMsg}", exitCode:=ex.HResult, ConsoleColor.Red)

        Finally
            Program.vpnRotationInProgress = 0
            Program.PageTimeoutWatch?.Reset()

            Program.accountPool.DisposeAccounts()

        End Try

        Dim exitCode As Integer = If(Program.totalFailedWallpaperCount = 0, 0, 1)
        Dim exitColor As ConsoleColor = If(Program.totalFailedWallpaperCount = 0, ConsoleColor.Green, ConsoleColor.Yellow)
        Console.WriteLine()
        Program.ExitWithMessage($"All search terms have been processed. Wallpapers downloaded: {Program.totalSavedWallpaperCount:N0}, Skipped: {Program.totalSkippedWallpaperCount:N0}, Failed: {Program.totalFailedWallpaperCount:N0}.", exitCode, exitColor)
    End Sub

#End Region

#Region " Private Methods "

    <DebuggerStepperBoundary>
    Private Async Function ProcessUrl(baseUrl As String, client As HttpClient, saveFolder As String) As Task

        Dim lastPageNumber As Integer
        Dim totalWallpapers As Integer

        Dim startedWithAccount As Boolean = False
        Dim availableAuth As GoodFonAuth = Program.accountPool.GetAvailable()

        Try
            ' Try get page number with authenticated client as it will list more wallpapers.
            Dim pageNumberHttpClient As HttpClient = client

            ' Use an available auth, instead of strictly using the first account, which could be exhausted.
            If availableAuth IsNot Nothing Then
                startedWithAccount = True
                pageNumberHttpClient = availableAuth.Client
            End If

            Dim pageInfo As (LastPage As Integer, TotalWallpapers As Integer) = Await Program.GetLastPageAndTotalItemsAsync(pageNumberHttpClient, baseUrl)
            lastPageNumber = pageInfo.LastPage
            totalWallpapers = pageInfo.TotalWallpapers

        Catch ex As TaskCanceledException
            Program.WriteColoredLine($"HTTP request has timed out ({AppGlobals.HttpClientTimeOutDisplay} minutes)", ConsoleColor.Red)
            Interlocked.Increment(Program.totalFailedWallpaperCount)
            Return

        Catch ex As InvalidOperationException When _
              ex.Message.Equals("Failed to identify last page number in the provided HTML.")
            Program.WriteColoredLine($"Zero results. No wallpapers found using this criteria.", ConsoleColor.Yellow)
            Program.WriteColoredLine(String.Empty, Console.ForegroundColor)
            Return

        Catch ex As HttpRequestException When _
              ex.Message.Equals("Response status code does not indicate success: 404 (Not Found).")
            Program.WriteColoredLine($"Zero results. No wallpapers found using this criteria.", ConsoleColor.Yellow)
            Program.WriteColoredLine(String.Empty, Console.ForegroundColor)
            Return

        Catch ex As Exception
            Program.WriteColoredLine($"Failed to identify last page number: {ex.Message}", ConsoleColor.Red)
            Program.WriteColoredLine(String.Empty, Console.ForegroundColor)
            Return

        End Try

        Program.WriteColoredLine($"Found {lastPageNumber:N0} pages (total {totalWallpapers:N0} wallpapers)", ConsoleColor.DarkCyan)
        Console.WriteLine()

        ' Estimates the remaining time of the current query, based on the duration of the most recent pages.
        Dim pagesEta As New EtaEstimator()
        pagesEta.StartItem()

        ' ── Outer sequential page loop ─────────────────────────────────────────
        For page As Integer = 1 To lastPageNumber

            ' Record the duration of the previous page (whatever the way it ended), then start timing this page.
            If page > 1 Then
                pagesEta.CompleteItem()
            End If

            Program.PageTimeoutWatch.Restart()

            Dim pageUrl As String = UrlHelper.BuildPageUrl(baseUrl, page)

            Program.WriteColoredLine($"[ETA: {pagesEta.GetEtaText(lastPageNumber - page + 1)}]", ConsoleColor.DarkCyan)
            Program.WriteColoredLine($"[Page {page:N0} of {lastPageNumber:N0}]: {UrlHelper.ToDisplayUrl(pageUrl)}", ConsoleColor.White)

            Dim pageResp As HttpResponseMessage = Nothing
            Dim pageHtml As String = String.Empty
            Dim pageHtmlEx As Exception = Nothing
            Try
                Dim pageReqEx As Exception = Nothing
                Dim pageReqUri As New Uri(pageUrl)
                Using pageReq As New HttpRequestMessage(HttpMethod.Get, pageReqUri)
                    pageReq.Headers.Referrer = AppGlobals.Referrer

                    ' Use the available auth handler for browsing, instead of the first account.
                    If startedWithAccount Then
                        Dim browseAuth As GoodFonAuth = Program.accountPool.GetAvailable()
                        If browseAuth IsNot Nothing Then
                            Dim cookieHeader As String = browseAuth.CookieContainer.GetCookieHeader(pageReqUri)
                            If Not String.IsNullOrWhiteSpace(cookieHeader) Then
                                pageReq.Headers.Add("Cookie", cookieHeader)
                            End If
                        End If
                    End If

                    Try
                        pageResp = Await client.SendAsync(pageReq, HttpCompletionOption.ResponseContentRead)

                    Catch ex As TaskCanceledException
                        Program.WriteColoredLine($"Page request has timed out ({AppGlobals.HttpClientTimeOutDisplay} minutes)", ConsoleColor.Red)
                        pageReqEx = ex

                    Catch ex As Exception
                        Program.WriteColoredLine($"Page request error: {ex.Message}", ConsoleColor.Red)
                        pageReqEx = ex

                    End Try
                End Using

                If pageReqEx IsNot Nothing Then
                    Program.WriteColoredLine(String.Empty, Console.ForegroundColor)
                    Await Task.Delay(Program.threadLocalRng.Value.Next(1, 1000))
                    Continue For
                End If

                If (pageResp.StatusCode = HttpStatusCode.NotFound) OrElse Not pageResp.IsSuccessStatusCode Then
                    Program.WriteColoredLine($"Page response error {CInt(pageResp.StatusCode)} ({pageResp.StatusCode})", ConsoleColor.Red)
                    Program.WriteColoredLine(String.Empty, Console.ForegroundColor)
                    Await Task.Delay(Program.threadLocalRng.Value.Next(1, 1000))
                    Continue For
                End If

                Try
                    pageHtml = Await pageResp.Content.ReadAsStringAsync()

                Catch ex As Exception
                    pageHtmlEx = ex

                End Try

                If pageHtmlEx IsNot Nothing Then
                    Program.WriteColoredLine($"Failed to read page HTML: {pageHtmlEx.Message}", ConsoleColor.Red)
                    Program.WriteColoredLine(String.Empty, Console.ForegroundColor)
                    Await Task.Delay(Program.threadLocalRng.Value.Next(1, 1000))
                    Continue For
                End If

            Finally
                pageResp?.Dispose()

            End Try

            Dim doc As HtmlDocument = GoodfonHtmlParser.LoadDocument(pageHtml)

            If Not GoodfonHtmlParser.HasWallpaperNodes(doc) Then
                Program.WriteColoredLine($"No wallpaper nodes found on current page.", ConsoleColor.Yellow)
                Program.WriteColoredLine(String.Empty, Console.ForegroundColor)
                Await Task.Delay(Program.threadLocalRng.Value.Next(1, 1000))
                Continue For
            End If

            Dim urls As List(Of String) = GoodfonHtmlParser.ExtractWallpaperPageUrls(doc)

            Dim totalItems As Integer = urls.Count
            If totalItems = 0 Then
                Program.WriteColoredLine("No valid wallpaper links after filtering.", ConsoleColor.Yellow)
                Program.WriteColoredLine(String.Empty, Console.ForegroundColor)
                Await Task.Delay(Program.threadLocalRng.Value.Next(1, 1000))
                Continue For
            End If

            ' ── MODIFICATION: PREVENTIVE VPN ROTATION CHECK ──────────────────────────
            ' If processing this page risks exceeding the anonymous downloads limit per IP,
            ' we rotate the VPN server right now before spinning up any parallel tasks.
            Dim currentIpUsage As Integer = Thread.VolatileRead(Program.downloadsOnCurrentIp)
            If (currentIpUsage + totalItems) > AppGlobals.MaxDownloadsPerIp AndAlso Program.accountPool.GetAvailable() Is Nothing Then
                Await Program.RotateVpnServerAsync($"{currentIpUsage} downloads on current IP plus {totalItems} items on next page (total projected: {currentIpUsage + totalItems:N0}) would exceed the daily limit of {AppGlobals.MaxDownloadsPerIp} daily downloads per IP")

            ElseIf Program.anonymousIp.IsLimitReached AndAlso Program.anonymousIp.CanAttemptRotation() Then
                ' Free wallpapers are being downloaded with user-accounts because a previous VPN rotation failed.
                ' Periodically retry the rotation to resume anonymous downloads and save the user-accounts daily downloads.
                Await Program.RotateVpnServerAsync($"Retrying to resume anonymous downloads; the daily {AppGlobals.MaxDownloadsPerIp} download limit was reached on the current IP")
            End If

            ' ── Inner parallel section: up to 24 concurrent wallpaper tasks ───────
            '
            '   SemaphoreSlim(24,24) acts as a concurrency gate: at most 24 tasks
            '   run simultaneously.  We spin up all tasks immediately so WhenAll
            '   can await the full batch, but each task must acquire a slot before
            '   doing any real work.
            '
            Using wallpaperSemaphore As New SemaphoreSlim(24, 24)

                Dim skippedOnThisPage As Integer = 0
                Dim pageTasks As New List(Of Task)

                For i As Integer = 0 To urls.Count - 1

                    ' Capture loop variables for the closure.
                    Dim capturedItem As Integer = i + 1
                    Dim capturedUrl As String = urls(i)
                    Dim capturedTotal As Integer = totalItems

                    ' Throttle: block the launching thread until a slot is free.
                    Await wallpaperSemaphore.WaitAsync()

                    Dim wallTask As Task = Task.Run(
                        Async Function()
                            Try
                                ' ── History check (thread-safe) ──────────────────────────
                                Dim alreadyDone As Boolean = False
                                SyncLock Program.historyLock
                                    Dim hashUrl As String = UrlHelper.GenerateMD5FromUrl(capturedUrl)
                                    alreadyDone = Program.processedUrlHashes.Contains(hashUrl)
                                End SyncLock

                                If alreadyDone Then
                                    Program.WriteColoredLine($"[Wall {capturedItem:D2}/{capturedTotal:D2}] [Downloaded: {Program.totalSavedWallpaperCount:N0}] Already downloaded", ConsoleColor.DarkGray)
                                    Interlocked.Increment(skippedOnThisPage)
                                    Interlocked.Increment(Program.totalSkippedWallpaperCount)
                                    Return
                                End If

                                Dim refForbiddenKeywordMatched As String = Nothing
                                If UrlHelper.ContainsAnyKeyword(capturedUrl, Program.forbiddenKeywords, refForbiddenKeywordMatched) Then
                                    Program.WriteColoredLine($"[Wall {capturedItem:D2}/{capturedTotal:D2}] [Downloaded: {Program.totalSavedWallpaperCount:N0}] Forbidden match ({refForbiddenKeywordMatched})", ConsoleColor.DarkYellow)
                                    Interlocked.Increment(skippedOnThisPage)
                                    Interlocked.Increment(Program.totalSkippedWallpaperCount)
                                    Return
                                End If
                                ' ─────────────────────────────────────────────────────────

                                Dim downloadSuccess As TriState = TriState.False
                                Dim downloadEx As Exception = Nothing
                                Try
                                    downloadSuccess = Await Program.DownloadOriginalWallpaperAsync(client, capturedUrl, saveFolder, capturedItem, capturedTotal, Nothing).ConfigureAwait(False)
                                Catch ex As Exception
                                    downloadEx = ex
                                End Try
                                If downloadEx IsNot Nothing Then
                                    Program.WriteColoredLine($"[Wall {capturedItem:D2}/{capturedTotal:D2}] [Downloaded: {Program.totalSavedWallpaperCount:N0}] Error: {downloadEx.Message}", ConsoleColor.Red)
                                    Interlocked.Increment(Program.totalFailedWallpaperCount)
                                    Await Task.Delay(Program.threadLocalRng.Value.Next(1, 1000))
                                    Return
                                End If

                                ' ── Register & report ────────────────────────────────────
                                If downloadSuccess <> TriState.False Then
                                    Program.RegisterProcessedUrl(capturedUrl)
                                    If downloadSuccess = TriState.UseDefault Then
                                        Program.WriteColoredLine($"[Wall {capturedItem:D2}/{capturedTotal:D2}] [Downloaded: {Program.totalSavedWallpaperCount:N0}] OK", ConsoleColor.Green)
                                    Else ' TriState.True
                                        Program.WriteColoredLine($"[Wall {capturedItem:D2}/{capturedTotal:D2}] [Downloaded: {Program.totalSavedWallpaperCount:N0}] OK (with user-account {Math.Min(Program.accountPool.CurrentIndex + 1, Program.accountPool.Count):N0}/{Program.accountPool.Count:N0})", ConsoleColor.Green)
                                    End If
                                End If

                                ' ─────────────────────────────────────────────────────────

                                Await Task.Delay(Program.threadLocalRng.Value.Next(1, 1000))

                                ' ── VPN timeout check (double-checked pattern) ────────────
                                If Program.PageTimeoutWatch.Elapsed > AppGlobals.PageFetchingMaxTimeout Then
                                    Await Program.RotateVpnServerAsync($"Wallpaper fetching on current page has timed out ({Program.PageTimeoutWatch.Elapsed.TotalMinutes:F2} minutes)")
                                    Program.PageTimeoutWatch.Restart()
                                End If
                                ' ─────────────────────────────────────────────────────────

                            Finally
                                ' Always release the concurrency slot, even if an unhandled
                                ' exception propagates, so subsequent tasks are not starved.
                                wallpaperSemaphore.Release()
                            End Try
                        End Function)

                    pageTasks.Add(wallTask)
                Next i

                ' Wait for all 24-wide parallel tasks to finish before moving to the
                ' next page.  Exceptions from individual tasks are swallowed per-task
                ' above; WhenAll re-surfaces any that slipped through.
                Await Task.WhenAll(pageTasks)
                ' ── End parallel section ──────────────────────────────────────────────

                Program.WriteColoredLine(String.Empty, Console.ForegroundColor)

                If skippedOnThisPage = totalItems Then
                    Await Task.Delay(Program.threadLocalRng.Value.Next(10, 100))
                End If
            End Using
        Next page

    End Function

    <DebuggerStepThrough>
    Private Async Function DownloadOriginalWallpaperAsync(client As HttpClient,
                                                          wallpaperPageUrl As String,
                                                          saveFolder As String,
                                                          capturedItem As Integer,
                                                          capturedTotal As Integer,
                                                          currentAuth As GoodFonAuth) As Task(Of TriState)

        ' Apply fallback at the download entry point, including calls already queued when the IP was exhausted.
        If currentAuth Is Nothing AndAlso Program.anonymousIp.IsLimitReached Then
            currentAuth = Program.GetFallbackAccount()
            If currentAuth Is Nothing Then
                Interlocked.Increment(Program.totalFailedWallpaperCount)
                Return TriState.False
            End If
            client = currentAuth.Client
        End If

        Dim originalUrl As String = wallpaperPageUrl
        Dim doc As HtmlDocument
        Dim downloadButtonFound As Boolean
        Dim downloadRelativeUrl As String = Nothing
        Dim lastHtml As String = String.Empty

        ' STEP 1 — Fetch wallpaper page
        Try
            Using req As New HttpRequestMessage(HttpMethod.Get, wallpaperPageUrl)
                req.Headers.Referrer = AppGlobals.Referrer

                Dim resp As HttpResponseMessage = Nothing
                Try
                    Try
                        resp = Await client.SendAsync(req, HttpCompletionOption.ResponseContentRead)

                    Catch ex As TaskCanceledException
                        Program.WriteColoredLine($"[Wall {capturedItem:D2}/{capturedTotal:D2}] [Downloaded: {Program.totalSavedWallpaperCount:N0}] HTTP request for wallpaper page {UrlHelper.GetRelativeUrl(wallpaperPageUrl)} has timed out ({AppGlobals.HttpClientTimeOutDisplay} minutes)", ConsoleColor.Red)
                        Interlocked.Increment(Program.totalFailedWallpaperCount)
                        Return TriState.False

                    Catch ex As Exception
                        Program.WriteColoredLine($"[Wall {capturedItem:D2}/{capturedTotal:D2}] [Downloaded: {Program.totalSavedWallpaperCount:N0}] HTTP request error for wallpaper page {UrlHelper.GetRelativeUrl(wallpaperPageUrl)}: {ex.Message}", ConsoleColor.Red)
                        Interlocked.Increment(Program.totalFailedWallpaperCount)
                        Return TriState.False

                    End Try

                    If (resp.StatusCode = HttpStatusCode.Forbidden) AndAlso currentAuth Is Nothing Then

                        Dim authToUse As GoodFonAuth = Program.accountPool.GetAvailable()
                        If authToUse IsNot Nothing Then
                            Return Await Program.DownloadOriginalWallpaperAsync(authToUse.Client, wallpaperPageUrl, saveFolder, capturedItem, capturedTotal, authToUse)
                        End If

                        Program.WriteColoredLine($"[Wall {capturedItem:D2}/{capturedTotal:D2}] [Downloaded: {Program.totalSavedWallpaperCount:N0}] User account required", ConsoleColor.DarkYellow)
                        Interlocked.Increment(Program.totalSkippedWallpaperCount)
                        Return TriState.False
                    End If

                    If (resp.StatusCode = HttpStatusCode.NotFound) OrElse Not resp.IsSuccessStatusCode Then
                        Program.WriteColoredLine($"[Wall {capturedItem:D2}/{capturedTotal:D2}] [Downloaded: {Program.totalSavedWallpaperCount:N0}] HTTP response error for wallpaper page {UrlHelper.GetRelativeUrl(wallpaperPageUrl)}: {CInt(resp.StatusCode)} ({resp.StatusCode})", ConsoleColor.Red)
                        Interlocked.Increment(Program.totalFailedWallpaperCount)
                        Return TriState.False
                    End If

                    Try
                        lastHtml = Await resp.Content.ReadAsStringAsync()

                    Catch ex As Exception
                        Program.WriteColoredLine($"[Wall {capturedItem:D2}/{capturedTotal:D2}] [Downloaded: {Program.totalSavedWallpaperCount:N0}] Failed to read content for wallpaper page {UrlHelper.GetRelativeUrl(wallpaperPageUrl)}: {ex.Message}", ConsoleColor.Red)
                        Interlocked.Increment(Program.totalFailedWallpaperCount)
                        Return TriState.False

                    End Try

                Finally
                    resp?.Dispose()

                End Try
            End Using

            Await Task.Delay(300)

            doc = GoodfonHtmlParser.LoadDocument(lastHtml)
            downloadButtonFound = GoodfonHtmlParser.TryFindDownloadButtonHref(doc, downloadRelativeUrl)

        Catch ex As Exception
            Program.WriteColoredLine($"[Wall {capturedItem:D2}/{capturedTotal:D2}] [Downloaded: {Program.totalSavedWallpaperCount:N0}] Failed to get wallpaper page {UrlHelper.GetRelativeUrl(originalUrl)}: {ex.Message}", ConsoleColor.Red)
            Interlocked.Increment(Program.totalFailedWallpaperCount)
            Return TriState.False
        End Try

        If Not downloadButtonFound Then
            Program.WriteColoredLine($"[Wall {capturedItem:D2}/{capturedTotal:D2}] [Downloaded: {Program.totalSavedWallpaperCount:N0}] Download button disabled or not found.", ConsoleColor.Yellow)
            Interlocked.Increment(Program.totalSkippedWallpaperCount)
            Return TriState.False
        End If

        If String.IsNullOrWhiteSpace(downloadRelativeUrl) Then
            Program.WriteColoredLine($"[Wall {capturedItem:D2}/{capturedTotal:D2}] [Downloaded: {Program.totalSavedWallpaperCount:N0}] Download href empty on {UrlHelper.GetRelativeUrl(wallpaperPageUrl)}", ConsoleColor.Red)
            Interlocked.Increment(Program.totalFailedWallpaperCount)
            Return TriState.False
        End If

        Dim downloadAbsoluteUrl As String = UrlHelper.ToAbsoluteUrl(downloadRelativeUrl)

        ' STEP 3 — Fetch intermediate download page
        Dim downloadHtml As String = String.Empty
        Dim req2Uri As New Uri(downloadAbsoluteUrl)
        Using req2 As New HttpRequestMessage(HttpMethod.Get, req2Uri)
            Try
                req2.Headers.Referrer = New Uri(wallpaperPageUrl)
            Catch
            End Try

            Dim resp2 As HttpResponseMessage = Nothing
            Try
                Try
                    resp2 = Await client.SendAsync(req2, HttpCompletionOption.ResponseContentRead)

                Catch ex As TaskCanceledException
                    Program.WriteColoredLine($"[Wall {capturedItem:D2}/{capturedTotal:D2}] [Downloaded: {Program.totalSavedWallpaperCount:N0}] HTTP request for download page {UrlHelper.GetRelativeUrl(downloadAbsoluteUrl)} has timed out ({AppGlobals.HttpClientTimeOutDisplay} minutes)", ConsoleColor.Red)
                    Interlocked.Increment(Program.totalFailedWallpaperCount)
                    Return TriState.False

                Catch ex As Exception
                    Program.WriteColoredLine($"[Wall {capturedItem:D2}/{capturedTotal:D2}] [Downloaded: {Program.totalSavedWallpaperCount:N0}] HTTP request error for download page {UrlHelper.GetRelativeUrl(downloadAbsoluteUrl)}: {ex.Message}", ConsoleColor.Red)
                    Interlocked.Increment(Program.totalFailedWallpaperCount)
                    Return TriState.False

                End Try

                If (resp2.StatusCode = HttpStatusCode.NotFound) OrElse Not resp2.IsSuccessStatusCode Then
                    Program.WriteColoredLine($"[Wall {capturedItem:D2}/{capturedTotal:D2}] [Downloaded: {Program.totalSavedWallpaperCount:N0}] HTTP response error for download page {UrlHelper.GetRelativeUrl(downloadAbsoluteUrl)}: {CInt(resp2.StatusCode)} ({resp2.StatusCode})", ConsoleColor.Red)
                    Interlocked.Increment(Program.totalFailedWallpaperCount)
                    Return TriState.False
                End If

                Try
                    downloadHtml = Await resp2.Content.ReadAsStringAsync()

                Catch ex As Exception
                    Program.WriteColoredLine($"[Wall {capturedItem:D2}/{capturedTotal:D2}] [Downloaded: {Program.totalSavedWallpaperCount:N0}] Failed to read content for download page {downloadAbsoluteUrl}: {ex.Message}", ConsoleColor.Red)
                    Interlocked.Increment(Program.totalFailedWallpaperCount)
                    Return TriState.False

                End Try

            Finally
                resp2?.Dispose()

            End Try
        End Using

        Dim downloadDoc As HtmlDocument = GoodfonHtmlParser.LoadDocument(downloadHtml)

        ' ── Daily download-limit check ────────────────────────────────────────────
        If GoodfonHtmlParser.IsDailyLimitReached(downloadDoc) Then

            If currentAuth IsNot Nothing Then
                Program.WriteColoredLine($"[Wall {capturedItem:D2}/{capturedTotal:D2}] [Downloaded: {Program.totalSavedWallpaperCount:N0}] Download limit reached for today with user-account {Program.accountPool.GetPosition(currentAuth):N0}/{Program.accountPool.Count:N0}.", ConsoleColor.Red)
                Program.accountPool.MarkLimitReached(currentAuth)

                ' Retry this wallpaper instead of losing it when the active account becomes exhausted.
                Dim nextAuth As GoodFonAuth = Program.GetFallbackAccount()
                If nextAuth IsNot Nothing Then
                    Return Await Program.DownloadOriginalWallpaperAsync(nextAuth.Client, wallpaperPageUrl, saveFolder, capturedItem, capturedTotal, nextAuth).ConfigureAwait(False)
                End If

            Else
                Program.WriteColoredLine($"[Wall {capturedItem:D2}/{capturedTotal:D2}] [Downloaded: {Program.totalSavedWallpaperCount:N0}] Download limit reached for today.", ConsoleColor.Red)
                Program.anonymousIp.ReportLimitReached()

                Await Program.RotateVpnServerAsync($"Daily {AppGlobals.MaxDownloadsPerIp} download limit reached on the current IP")

                ' If the VPN server could not be rotated, keep downloading the free wallpaper using an available user-account.
                If Program.anonymousIp.IsLimitReached Then
                    Dim authToUse As GoodFonAuth = Program.GetFallbackAccount()
                    If authToUse IsNot Nothing Then
                        Return Await Program.DownloadOriginalWallpaperAsync(authToUse.Client, wallpaperPageUrl, saveFolder, capturedItem, capturedTotal, authToUse).ConfigureAwait(False)
                    End If
                End If
            End If

            Interlocked.Increment(Program.totalFailedWallpaperCount)
            Return TriState.False
        End If
        ' ──────────────────────────────────────────────────────────────────────────

        ' STEP 4 — Locate image node
        Dim imageUrl As String = Nothing
        If Not GoodfonHtmlParser.TryFindImageSource(downloadDoc, imageUrl) Then
            Program.WriteColoredLine($"[Wall {capturedItem:D2}/{capturedTotal:D2}] [Downloaded: {Program.totalSavedWallpaperCount:N0}] Image node not found on download page {UrlHelper.GetRelativeUrl(downloadAbsoluteUrl)}", ConsoleColor.Red)
            Interlocked.Increment(Program.totalFailedWallpaperCount)
            Return TriState.False
        End If

        If String.IsNullOrWhiteSpace(imageUrl) Then
            Program.WriteColoredLine($"[Wall {capturedItem:D2}/{capturedTotal:D2}] [Downloaded: {Program.totalSavedWallpaperCount:N0}] Image src empty on {UrlHelper.GetRelativeUrl(downloadAbsoluteUrl)}", ConsoleColor.Red)
            Interlocked.Increment(Program.totalFailedWallpaperCount)
            Return TriState.False
        End If
        imageUrl = UrlHelper.ToAbsoluteUrl(imageUrl)

        ' STEP 5 — Resolve filename and save path
        Dim fileName As String = Path.GetFileName(New Uri(imageUrl).LocalPath)
        If String.IsNullOrWhiteSpace(fileName) Then
            Program.WriteColoredLine($"[Wall {capturedItem:D2}/{capturedTotal:D2}] [Downloaded: {Program.totalSavedWallpaperCount:N0}] Cannot resolve file name for URL: {UrlHelper.GetRelativeUrl(imageUrl)}", ConsoleColor.Red)
            Interlocked.Increment(Program.totalFailedWallpaperCount)
            Return TriState.False
        End If

        ' STEP 6 — Download image bytes
        Dim imgReqUri As New Uri(imageUrl)
        Using imgReq As New HttpRequestMessage(HttpMethod.Get, imgReqUri)
            Try
                imgReq.Headers.Referrer = New Uri(downloadAbsoluteUrl)
            Catch
            End Try

            Dim imageBytes As Byte() = Nothing
            Dim imgResp As HttpResponseMessage = Nothing
            Try

                Try
                    imgResp = Await client.SendAsync(imgReq, HttpCompletionOption.ResponseContentRead)

                Catch ex As TaskCanceledException
                    Program.WriteColoredLine($"[Wall {capturedItem:D2}/{capturedTotal:D2}] [Downloaded: {Program.totalSavedWallpaperCount:N0}] HTTP request for image {downloadAbsoluteUrl} has timed out ({AppGlobals.HttpClientTimeOutDisplay} minutes)", ConsoleColor.Red)
                    Interlocked.Increment(Program.totalFailedWallpaperCount)
                    Return TriState.False

                Catch ex As Exception
                    Program.WriteColoredLine($"[Wall {capturedItem:D2}/{capturedTotal:D2}] [Downloaded: {Program.totalSavedWallpaperCount:N0}] HTTP request error for image {UrlHelper.GetRelativeUrl(imageUrl)}: {ex.Message}", ConsoleColor.Red)
                    Interlocked.Increment(Program.totalFailedWallpaperCount)
                    Return TriState.False

                End Try

                If (imgResp.StatusCode = HttpStatusCode.Forbidden) AndAlso currentAuth Is Nothing Then

                    Dim authToUse As GoodFonAuth = Program.accountPool.GetAvailable()
                    If authToUse IsNot Nothing Then
                        Return Await Program.DownloadOriginalWallpaperAsync(authToUse.Client, wallpaperPageUrl, saveFolder, capturedItem, capturedTotal, authToUse)
                    End If

                    Program.WriteColoredLine($"[Wall {capturedItem:D2}/{capturedTotal:D2}] [Downloaded: {Program.totalSavedWallpaperCount:N0}] User account required", ConsoleColor.DarkYellow)
                    Interlocked.Increment(Program.totalSkippedWallpaperCount)
                    Return TriState.False
                End If

                If (imgResp.StatusCode = HttpStatusCode.NotFound) OrElse Not imgResp.IsSuccessStatusCode Then
                    Program.WriteColoredLine($"[Wall {capturedItem:D2}/{capturedTotal:D2}] [Downloaded: {Program.totalSavedWallpaperCount:N0}] HTTP response error for image {UrlHelper.GetRelativeUrl(imageUrl)}: {CInt(imgResp.StatusCode)} ({imgResp.StatusCode})", ConsoleColor.Red)
                    Interlocked.Increment(Program.totalFailedWallpaperCount)
                    Return TriState.False
                End If

                Try
                    imageBytes = Await imgResp.Content.ReadAsByteArrayAsync()

                Catch ex As Exception
                    Program.WriteColoredLine($"[Wall {capturedItem:D2}/{capturedTotal:D2}] [Downloaded: {Program.totalSavedWallpaperCount:N0}] Failed to read byte-content for image: {UrlHelper.GetRelativeUrl(imageUrl)}", ConsoleColor.Red)
                    Interlocked.Increment(Program.totalFailedWallpaperCount)
                    Return TriState.False

                End Try

            Finally
                imgResp?.Dispose()

            End Try

            If imageBytes Is Nothing OrElse imageBytes.Length = 0 Then
                Program.WriteColoredLine($"[Wall {capturedItem:D2}/{capturedTotal:D2}] [Downloaded: {Program.totalSavedWallpaperCount:N0}] Downloaded image is empty: {UrlHelper.GetRelativeUrl(imageUrl)}", ConsoleColor.Red)
                Interlocked.Increment(Program.totalFailedWallpaperCount)
                Return TriState.False
            End If

            If imageBytes.Length < 512 Then ' Bytes
                Program.WriteColoredLine($"[Wall {capturedItem:D2}/{capturedTotal:D2}] [Downloaded: {Program.totalSavedWallpaperCount:N0}] Skipping small placeholder image ({imageBytes.Length:N0} bytes): {imageUrl}", ConsoleColor.Red)
                Interlocked.Increment(Program.totalFailedWallpaperCount)
                Return TriState.False
            End If

            Dim savePath As String = Path.Combine(Path.GetFullPath(saveFolder), fileName)
            Dim extendedSavePath As String = FileSystemHelper.GetExtendedPath(savePath)
            Dim extendedSaveDir As String = Path.GetDirectoryName(extendedSavePath)

            Try
                If Not Directory.Exists(extendedSaveDir) Then
                    Directory.CreateDirectory(extendedSaveDir)
                End If

                File.WriteAllBytes(extendedSavePath, imageBytes)
                Interlocked.Increment(Program.totalSavedWallpaperCount)
                ' Only anonymous downloads count towards the daily download limit of the current IP.
                If currentAuth Is Nothing Then
                    Interlocked.Increment(Program.downloadsOnCurrentIp)
                End If
            Catch ex As Exception
                Program.WriteColoredLine($"[Wall {capturedItem:D2}/{capturedTotal:D2}] [Downloaded: {Program.totalSavedWallpaperCount:N0}] Failed to write to disk imgae file {savePath}: {ex.Message}", ConsoleColor.Red)
                Interlocked.Increment(Program.totalFailedWallpaperCount)
                Return TriState.False
            End Try
        End Using

        Return If(currentAuth Is Nothing, TriState.UseDefault, TriState.True)
    End Function

    ''' <summary>
    ''' Adds a wallpaper page URL to the in-memory history set and appends it to
    ''' the history file on disk.  All HashSet accesses are guarded by HistoryLock
    ''' so that concurrent tasks cannot corrupt the collection.
    ''' </summary>
    <DebuggerStepThrough>
    Private Sub RegisterProcessedUrl(url As String)

        SyncLock Program.historyLock

            Dim urlHash As String = UrlHelper.GenerateMD5FromUrl(url)
            Dim added As Boolean = Program.processedUrlHashes.Add(urlHash)

            If added Then
                Dim extendedHistoryPath As String = FileSystemHelper.GetExtendedPath(Path.GetFullPath(AppGlobals.HistoryFilePath))
                Dim extendedHistoryDir As String = Path.GetDirectoryName(extendedHistoryPath)

                If Not Directory.Exists(extendedHistoryDir) Then
                    Directory.CreateDirectory(extendedHistoryDir)
                End If

                Const maxRetries As Integer = 10
                Dim currentRetry As Integer = 0
                Dim isWritten As Boolean = False
                Const retryDelayMilliseconds As Integer = 100

                While Not isWritten AndAlso currentRetry < maxRetries
                    Try
                        File.AppendAllText(extendedHistoryPath, $"{urlHash}{Environment.NewLine}", AppGlobals.TextEncoding)
                        isWritten = True
                    Catch ex As IOException
                        currentRetry += 1
                        If currentRetry >= maxRetries Then
                            Program.ExitWithMessage($"Cannot append URL hash to history file ({AppGlobals.HistoryFilePath}) after {maxRetries} retries: {ex.Message}.", ex.HResult, ConsoleColor.Red)
                        End If
                        ' Briefly pause the current thread to give the antivirus time to release the file.
                        Thread.Sleep(retryDelayMilliseconds)
                    Catch ex As Exception
                        ' For any other non-I/O exception (e.g., access denied), exit immediately.
                        Program.ExitWithMessage($"Cannot append URL hash to history file ({AppGlobals.HistoryFilePath}): {ex.Message}.", ex.HResult, ConsoleColor.Red)
                    End Try
                End While
            End If
        End SyncLock
    End Sub

    ''' <summary>
    ''' Fires ChangeServerInProtonVpnClient exactly once per rotation cycle.
    ''' If a rotation is already in progress every concurrent caller returns
    ''' immediately without queuing — no serialised pile-up of waiting threads.
    ''' <para></para>
    ''' After a failed rotation, new rotations are not attempted until 
    ''' <see cref="AnonymousIpState.RotationRetryInterval"/> has elapsed, 
    ''' so a failing (and slow) rotation is not repeated for every wallpaper or page.
    ''' </summary>
    ''' 
    ''' <param name="reason">
    ''' The reason of the rotation, displayed in the console.
    ''' </param>
    <DebuggerStepThrough>
    Private Async Function RotateVpnServerAsync(reason As String) As Task

        If Not Program.anonymousIp.CanAttemptRotation() Then
            Return
        End If

        ' Only the thread that atomically flips 0→1 gets to run the rotation.
        ' Every other concurrent caller sees 1 already set and bails out.
        If Interlocked.CompareExchange(Program.vpnRotationInProgress, 1, 0) <> 0 Then
            Return
        End If

        Try
            ' Another caller may have failed a rotation between our first check and acquiring the flag.
            If Not Program.anonymousIp.CanAttemptRotation() Then
                Return
            End If

            Program.WriteColoredLine($"Proton VPN rotation triggered ({reason})...", ConsoleColor.Magenta)
            UtilNetwork.ChangeServerInProtonVpnClient()
            Await Task.Delay(1000)
            Interlocked.Exchange(Program.downloadsOnCurrentIp, 0)
            Program.anonymousIp.ReportRotationSucceeded()
            Interlocked.Exchange(Program.accountFallbackNoticeShown, 0)
            Program.WriteColoredLine("Proton VPN rotation done.", ConsoleColor.Magenta)

        Catch ex As Exception
            Program.anonymousIp.ReportRotationFailed()
            Dim errMsg As String = If(ex.InnerException IsNot Nothing, ex.InnerException.Message, ex.Message)
            Program.WriteColoredLine($"  Proton VPN rotation error: {errMsg}", ConsoleColor.Red)
            Program.WriteColoredLine($"  The next VPN rotation will not be attempted until {Date.Now + Program.anonymousIp.RotationRetryInterval:HH:mm:ss}.", ConsoleColor.Red)

        Finally
            ' Reset flag so future rotations are possible again.
            Interlocked.Exchange(Program.vpnRotationInProgress, 0)

        End Try
    End Function

    ''' <summary>
    ''' Detects the last page number available for the specified GoodFon.com category listing or search URL.
    ''' </summary>
    ''' 
    ''' <param name="client">
    ''' The HTTP client used to request the page.
    ''' </param>
    ''' 
    ''' <param name="baseUrl">
    ''' The base listing or search URL.
    ''' </param>
    ''' 
    ''' <returns>
    ''' The last page number available.
    ''' </returns>
    ''' 
    ''' <exception cref="InvalidOperationException">
    ''' Thrown when pagination cannot be detected from the provided HTML.
    ''' </exception>
    <DebuggerStepThrough>
    Private Async Function GetLastPageAndTotalItemsAsync(client As HttpClient, baseUrl As String) As Task(Of (LastPage As Integer, TotalWallpapers As Integer))

        Dim html As String = Await client.GetStringAsync(baseUrl)
        Dim doc As HtmlDocument = GoodfonHtmlParser.LoadDocument(html)

        Dim lastPageNumber As Integer? = GoodfonHtmlParser.FindLastPageNumber(doc)

        Dim finalLastPage As Integer = If(lastPageNumber, 1)
        Dim totalWallpapers As Integer

        If finalLastPage <= 1 Then
            totalWallpapers = GoodfonHtmlParser.CountValidWallpaperLinks(doc)

            ' Throw if strictly 0 results are found overall.
            If totalWallpapers = 0 AndAlso Not lastPageNumber.HasValue Then
                Throw New InvalidOperationException("Failed to identify last page number in the provided HTML.")
            End If

        ElseIf UrlHelper.GetUrlKind(baseUrl) = GoodfonUrlKind.Unknown Then
            totalWallpapers = GoodfonHtmlParser.CalculateTotalWallpapers(finalLastPage, 0)

        Else
            Dim lastPageUrl As String = UrlHelper.BuildPageUrl(baseUrl, finalLastPage)
            Try
                Dim lastPageHtml As String = Await client.GetStringAsync(lastPageUrl)
                Dim lastPageDoc As HtmlDocument = GoodfonHtmlParser.LoadDocument(lastPageHtml)

                Dim itemsOnLastPage As Integer = GoodfonHtmlParser.CountValidWallpaperLinks(lastPageDoc)
                totalWallpapers = GoodfonHtmlParser.CalculateTotalWallpapers(finalLastPage, itemsOnLastPage)

            Catch ex As Exception
                ' The total is only informative; fall back to an estimation that excludes the last page items.
                totalWallpapers = GoodfonHtmlParser.CalculateTotalWallpapers(finalLastPage, 0)

            End Try
        End If

        Return (finalLastPage, totalWallpapers)
    End Function

    ''' <summary>
    ''' Loads the search terms and forbidden keywords from the config text files into the corresponding HashSet fields.
    ''' <para></para>
    ''' Exits with error if the search terms file is missing or empty.
    ''' </summary>
    <DebuggerStepThrough>
    Private Sub LoadConfigFiles()

        Dim extendedSearchTermsFilePath As String =
            FileSystemHelper.GetExtendedPath(Path.GetFullPath(AppGlobals.SearchTermsFilePath))

        If Not File.Exists(extendedSearchTermsFilePath) Then
            Program.ExitWithMessage($"[ERROR] Search terms file not found: {AppGlobals.SearchTermsFilePath}", 2, ConsoleColor.Red)
        End If

        Program.searchTerms = ConfigFileParser.ParseListLines(File.ReadAllLines(extendedSearchTermsFilePath, AppGlobals.TextEncoding))

        Dim extendedForbiddenKeywordsFilePath As String =
            FileSystemHelper.GetExtendedPath(Path.GetFullPath(AppGlobals.ForbiddenKeywordsFilePath))

        If File.Exists(extendedForbiddenKeywordsFilePath) Then
            Program.forbiddenKeywords = ConfigFileParser.ParseListLines(File.ReadAllLines(extendedForbiddenKeywordsFilePath, AppGlobals.TextEncoding))
        Else
            Program.WriteColoredLine($"[WARN] Forbidden keywords file not found: {AppGlobals.ForbiddenKeywordsFilePath}", ConsoleColor.Yellow)
            Program.forbiddenKeywords = New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        End If

        If Program.searchTerms.Count = 0 Then
            Program.ExitWithMessage($"[ERROR] No search terms found in: {AppGlobals.SearchTermsFilePath}", 3, ConsoleColor.Red)
        End If
    End Sub

    ''' <summary>
    ''' Loads the GoodFon accounts and login settings from the "Accounts.json" configuration file.
    ''' </summary>
    '''
    ''' <returns>
    ''' A <see cref="GoodFonAccountsFile"/> instance populated from the configuration file,
    ''' or <see langword="Nothing"/> if the file does not exist.
    ''' </returns>
    '''
    ''' <exception cref="InvalidOperationException">
    ''' Thrown when the file is empty or its content cannot be deserialized.
    ''' </exception>
    <DebuggerStepThrough>
    Private Function LoadGoodFonAccounts() As GoodFonAccountsFile

        Dim extendedGoodfonAccountsFilePath As String =
            FileSystemHelper.GetExtendedPath(Path.GetFullPath(AppGlobals.GoodfonAccountsFilePath))

        If Not File.Exists(extendedGoodfonAccountsFilePath) Then
            Return Nothing
        End If

        Dim jsonContent As String = File.ReadAllText(extendedGoodfonAccountsFilePath, AppGlobals.TextEncoding)
        Return ConfigFileParser.ParseAccountsFile(jsonContent)
    End Function

    ''' <summary>
    ''' Loads the history of already-processed wallpaper URL hashes from the history text file into a HashSet for quick lookup.
    ''' </summary>
    <DebuggerStepThrough>
    Private Sub LoadUrlsHistoryHashes()

        Dim extendedHistoryFilePath As String =
            FileSystemHelper.GetExtendedPath(Path.GetFullPath(AppGlobals.HistoryFilePath))

        Program.processedUrlHashes =
            If(File.Exists(extendedHistoryFilePath),
               ConfigFileParser.ParseListLines(File.ReadAllLines(extendedHistoryFilePath, AppGlobals.TextEncoding)),
               New HashSet(Of String)(StringComparer.OrdinalIgnoreCase))
    End Sub

    ''' <summary>
    ''' Writes a message to the console in a specified foreground color,
    ''' then resets the color back to the original.
    ''' </summary>
    '''
    ''' <param name="line">
    ''' The message to display.
    ''' </param>
    '''
    ''' <param name="foreColor">
    ''' The console foreground color to use when displaying the message.
    ''' <para></para>
    ''' After writing the message, the console color is reset to its original value.
    ''' </param>
    <DebuggerStepThrough>
    Private Sub WriteColoredLine(line As String, foreColor As ConsoleColor)

        SyncLock Program.consoleLock
            Dim originalForeColor As ConsoleColor = Console.ForegroundColor
            Console.ForegroundColor = foreColor
            Console.WriteLine(line)
            Console.ForegroundColor = originalForeColor
        End SyncLock
    End Sub

    ''' <summary>
    ''' Displays a message to the console and exits the application with the specified exit code.
    ''' </summary>
    '''
    ''' <param name="message">
    ''' The message to display before exiting. If empty or null, no message is displayed.
    ''' </param>
    '''
    ''' <param name="exitCode">
    ''' The exit code to return to the operating system. Typically 0 for success, non-zero for errors.
    ''' </param>
    '''
    ''' <param name="foreColor">
    ''' The console foreground color to use when displaying the message.
    ''' <para></para>
    ''' After writing the message, the console color is reset to its original value.
    ''' </param>
    <DebuggerStepThrough>
    Private Sub ExitWithMessage(message As String, exitCode As Integer, foreColor As ConsoleColor)

        If Not String.IsNullOrEmpty(message) Then
            Program.WriteColoredLine(message, foreColor)
            Console.WriteLine()
        End If

        Console.WriteLine($"Exiting application with exit code: {exitCode} (0x{exitCode:X}) ...")
#If DEBUG Then
        Console.WriteLine()
        Program.WriteColoredLine("[!] This message only appears in DEBUG mode to prevent accidental termination.", ConsoleColor.Yellow)
        Console.WriteLine("Press any key to exit...")
        Console.ReadKey(intercept:=True)
#End If
        Environment.Exit(exitCode)
    End Sub

    ''' <summary>
    ''' Gets an available account and reports why account fallback is unavailable when none can be used.
    ''' </summary>
    ''' <returns>An available logged-in account, or <see langword="Nothing"/>.</returns>
    Private Function GetFallbackAccount() As GoodFonAuth

        Dim auth As GoodFonAuth = Program.accountPool.GetAvailable()
        If auth IsNot Nothing Then
            If Program.anonymousIp.IsLimitReached Then
                Program.ShowAccountFallbackNotice()
            End If
            Return auth
        End If

        If Interlocked.Exchange(Program.accountFallbackNoticeShown, 2) <> 2 Then
            Dim configured As Integer = If(Program.accountsFile Is Nothing, 0, Program.accountsFile.Accounts.Count)
            Dim enabled As Integer = If(Program.accountsFile Is Nothing, 0,
                Program.accountsFile.Accounts.Where(Function(account As GoodFonAccountConfig) account.Enabled).Count())
            Dim reason As String
            If Program.accountPool.Count > 0 Then
                reason = "All logged-in accounts have reached their daily download limit."
            ElseIf enabled > 0 Then
                reason = "No enabled account logged in successfully. Check the login errors above."
            Else
                reason = "No accounts are enabled. Check Enabled in Accounts.json."
            End If
            Program.WriteColoredLine($"[Auth] Account fallback unavailable: {reason} Configured: {configured:N0}; enabled: {enabled:N0}; logged-in: {Program.accountPool.Count:N0}. Blocked anonymous downloads will not be retried while the IP limit remains active.", ConsoleColor.Yellow)
        End If
        Return Nothing
    End Function

    ''' <summary>
    ''' Shows, once per IP address, a notice that the free wallpapers are being downloaded using the user-accounts 
    ''' because the anonymous daily download limit was reached and the VPN server could not be rotated.
    ''' </summary>
    <DebuggerStepThrough>
    Private Sub ShowAccountFallbackNotice()

        If Interlocked.Exchange(Program.accountFallbackNoticeShown, 1) <> 1 Then
            Program.WriteColoredLine($"[Auth] The daily {AppGlobals.MaxDownloadsPerIp} download limit was reached on the current IP and the VPN server could not be rotated. Free wallpapers will be downloaded using the user-accounts.", ConsoleColor.Yellow)
        End If
    End Sub

#End Region

#Region " Event Handlers "

    ''' <summary>
    ''' Called when a login attempt fails with a retryable error 
    ''' (website login rate-limit or transient network error), before waiting to retry.
    ''' </summary>
    '''
    ''' <param name="attempt">
    ''' The failed attempt number (1-based).
    ''' </param>
    '''
    ''' <param name="waitTime">
    ''' The time to wait before the next attempt.
    ''' </param>
    '''
    ''' <param name="reason">
    ''' The exception that caused the failed attempt.
    ''' </param>
    <DebuggerStepThrough>
    Private Sub OnLoginRetry(attempt As Integer, waitTime As TimeSpan, reason As Exception)

        Dim maxAttempts As Integer = Program.accountsFile.LoginSettings.MaxLoginAttempts

        If TypeOf reason Is GoodFonRateLimitException Then
            Program.WriteColoredLine($"  Login attempt {attempt:N0} of {maxAttempts:N0} rejected by the website rate-limit (""Too many attempts. Please wait a few minutes"").", ConsoleColor.Yellow)
        Else
            Program.WriteColoredLine($"  Login attempt {attempt:N0} of {maxAttempts:N0} failed due to a network error: {Program.GetInnermostMessage(reason)}", ConsoleColor.Yellow)
        End If

        Dim retryAt As Date = Date.Now + waitTime
        Program.WriteColoredLine($"  Waiting {waitTime.TotalSeconds:N0} seconds before retrying (at {retryAt:HH:mm:ss})...", ConsoleColor.Yellow)
    End Sub

    ''' <summary>
    ''' Gets the message of the innermost exception of the specified exception 
    ''' (e.g., the underlying <see cref="WebException"/> message of an <see cref="HttpRequestException"/>).
    ''' </summary>
    '''
    ''' <param name="ex">
    ''' The exception.
    ''' </param>
    '''
    ''' <returns>
    ''' The innermost exception message.
    ''' </returns>
    <DebuggerStepThrough>
    Private Function GetInnermostMessage(ex As Exception) As String

        If ex Is Nothing Then
            Throw New ArgumentNullException(NameOf(ex))
        End If

        Return ex.GetBaseException().Message
    End Function

    ''' <summary>
    ''' Handles the <see cref="GoodFonAccountPool.AccountLimitReset"/> event of <see cref="Program.accountPool"/>.
    ''' </summary>
    '''
    ''' <param name="sender">
    ''' The source of the event.
    ''' </param>
    '''
    ''' <param name="accountIndex">
    ''' The zero-based index of the account whose daily limit has been reset.
    ''' </param>
    <DebuggerStepThrough>
    Private Sub AccountPool_AccountLimitReset(sender As Object, accountIndex As Integer)

        Program.WriteColoredLine($"[Auth] The daily limit for user-account {accountIndex + 1:N0} has been automatically reset after 24 hours.", ConsoleColor.Green)
    End Sub

    ''' <summary>
    ''' Handles the <see cref="GoodFonAccountPool.AllAccountsExhausted"/> event of <see cref="Program.accountPool"/>.
    ''' </summary>
    '''
    ''' <param name="sender">
    ''' The source of the event.
    ''' </param>
    '''
    ''' <param name="e">
    ''' The event data.
    ''' </param>
    <DebuggerStepThrough>
    Private Sub AccountPool_AllAccountsExhausted(sender As Object, e As EventArgs)

        Program.WriteColoredLine("[Auth] All user-accounts have reached their daily download limit. Operating on anonymous IP limit mode.", ConsoleColor.Yellow)
    End Sub

#End Region

End Module
