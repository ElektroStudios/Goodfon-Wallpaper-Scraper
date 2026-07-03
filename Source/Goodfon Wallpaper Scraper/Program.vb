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
Imports System.Security.Cryptography
Imports System.Text
Imports System.Text.Json
Imports System.Text.RegularExpressions
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
    ''' The list of search terms to query on BT4G website.
    ''' </summary>
    Private searchTerms As HashSet(Of String)

    ''' <summary>
    ''' The list of keywords used to filter out unwanted results. 
    ''' <para></para>
    ''' If any of these keywords are present in the torrent name, it will be skipped.
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
    ''' Lock object to manipulate <see cref="Program.accountAuths"/> with thread-safety.
    ''' </summary>
    Private ReadOnly accountLockObject As New Object()

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
    ''' The authentication handlers used to download restricted (adult) wallpapers on GoodFon.com.
    ''' </summary>
    Private accountAuths As IReadOnlyList(Of GoodFonAuth)

    ''' <summary>
    ''' The account authentication configurations.
    ''' </summary>
    Private accountConfigs As IReadOnlyList(Of GoodFonAccountConfig)

    ''' <summary>
    ''' Keeps track of the current item index being used in <see cref="Program.accountAuths"/> list.
    ''' </summary>
    Private currentAccountIndex As Integer = 0

    ''' <summary>
    ''' Dictionary to track the exact time an account reached its daily limit for the 24h cooldown reset.
    ''' </summary>
    Private ReadOnly accountLimitReachedTimes As New Dictionary(Of GoodFonAuth, Date)()

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
        Console.WriteLine("│   A GoodFon account can also be configured in the following file:                │")
        Console.WriteLine("│     - <PROGRAM DIRECTORY>\Config\Accounts.json                                   │")
        Console.WriteLine("│                                                                                  │")
        Console.WriteLine("│   The account will only be used to download wallpapers with insufficient rights. │")
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

            Program.accountConfigs = Program.LoadGoodFonAccounts()
            If Program.accountConfigs IsNot Nothing Then
                Program.WriteColoredLine($"  Loaded {Program.accountConfigs.Count} user-account credentials from config file.", ConsoleColor.Green)
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

            If (Program.accountConfigs IsNot Nothing) Then

                Program.WriteColoredLine($"Loging-in to GoodFon.com website using the user-defined credentials...", ConsoleColor.White)
                Console.WriteLine()

                Dim authsList As New List(Of GoodFonAuth)

                For Each accountConfig As GoodFonAccountConfig In Program.accountConfigs

#If DEBUG Then
                    Program.WriteColoredLine($"  - Username: {accountConfig.Username}", ConsoleColor.White)
                    Program.WriteColoredLine($"  - Password: {accountConfig.Password}", ConsoleColor.White)
#Else
                    Program.WriteColoredLine($"  - Username: {accountConfig.Username.Substring(0, 4)}{New String("*"c, accountConfig.Username.Length - 4)}", ConsoleColor.White)
                    Program.WriteColoredLine($"  - Password: {accountConfig.Password.Substring(0, 4)}{New String("*"c, accountConfig.Password.Length - 4)}", ConsoleColor.White)
#End If

                    If accountConfig.Enabled Then


                        Dim auth As New GoodFonAuth()
                        Dim result As Boolean
                        Try
                            result = auth.LoginAsync(accountConfig.Username, accountConfig.Password).Result
                            authsList.Add(auth)
                            Program.WriteColoredLine($"  Login successful.", ConsoleColor.Green)

                        Catch ex As TaskCanceledException
                            Program.WriteColoredLine($"  Login request has timed out ({AppGlobals.HttpClientTimeOut:mm:ss} minutes).", ConsoleColor.Red)

                        Catch ex As Exception
                            Dim errMsg As String = If(ex.InnerException IsNot Nothing, ex.InnerException.Message, ex.Message)
                            Program.WriteColoredLine($"  Login error: {errMsg}", ConsoleColor.Red)

                        End Try
                    Else
                        Program.WriteColoredLine($"  Account is disabled.", ConsoleColor.Yellow)

                    End If

                    Console.WriteLine()
                Next accountConfig

                If authsList.Count <> 0 Then
                    Program.accountAuths = authsList
                End If
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

                    Dim wallpaperUserUrl As String = $"{AppGlobals.BaseGoodfonUserUrl}/{searchTerm.Replace(" "c, "+"c)}"
                    Program.WriteColoredLine($"Discovering user profile page: {wallpaperUserUrl}", ConsoleColor.Cyan)
                    Program.ProcessUrl(wallpaperUserUrl, client, outputDirPath).Wait()

                    For tagIndex As Integer = 0 To 3
                        Dim wallpaperTagUrl As String = If(tagIndex = 0,
                            $"{AppGlobals.BaseGoodfonTagUrl}/{searchTerm.Replace(" "c, "+"c)}/",
                            $"{AppGlobals.BaseGoodfonTagUrl}/{searchTerm.Replace(" "c, "+"c)}-{tagIndex}/")

                        Program.WriteColoredLine($"Discovering tag page: {wallpaperTagUrl}", ConsoleColor.Cyan)
                        Program.ProcessUrl(wallpaperTagUrl, client, outputDirPath).Wait()
                    Next tagIndex

                    Dim wallpaperSearchUrl As String = $"{AppGlobals.BaseGoodfonSearchUrl}/?q={searchTerm.Replace(" "c, "+"c)}"
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
            Program.ExitWithMessage($"FATAL ERROR 0x{ex.HResult}: {errMsg}", exitCode:=ex.HResult, ConsoleColor.Red)

        Finally
            Program.vpnRotationInProgress = 0
            Program.PageTimeoutWatch?.Reset()

            For Each auth As GoodFonAuth In Program.accountAuths
                Try
                    auth?.Dispose()
                Catch
                End Try
            Next

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
        Dim availableAuth As GoodFonAuth = Program.GetAvailableAuthHandler()

        Try
            ' Try get page number with authenticated client as it will list more wallpapers.
            Dim pageNumberHttpClient As HttpClient = client

            ' Use an available auth, instead of strictly using accountAuths(0) which could be exhausted.
            If availableAuth IsNot Nothing Then
                startedWithAccount = True
                pageNumberHttpClient = availableAuth.Client
            End If

            Dim pageInfo As (LastPage As Integer, TotalWallpapers As Integer) = Await Program.GetLastPageAndTotalItemsAsync(pageNumberHttpClient, baseUrl)
            lastPageNumber = pageInfo.LastPage
            totalWallpapers = pageInfo.TotalWallpapers

        Catch ex As TaskCanceledException
            Program.WriteColoredLine($"HTTP request has timed out ({AppGlobals.HttpClientTimeOut:mm:ss} minutes)", ConsoleColor.Red)
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

        ' ── Outer sequential page loop ─────────────────────────────────────────
        For page As Integer = 1 To lastPageNumber

            Program.PageTimeoutWatch.Restart()

            Dim pageUrl As String = Nothing

            If baseUrl.IndexOf("/tags/", StringComparison.OrdinalIgnoreCase) > 0 Then
                pageUrl = If(page = 1, baseUrl, $"{baseUrl}index-{page}.html")

            ElseIf baseUrl.IndexOf("/user/", StringComparison.OrdinalIgnoreCase) > 0 Then
                pageUrl = If(page = 1, baseUrl, $"{baseUrl}/?&page={page}")

            ElseIf baseUrl.IndexOf("?q", StringComparison.OrdinalIgnoreCase) > 0 Then
                pageUrl = If(page = 1, baseUrl, $"{baseUrl}&page={page}")

            Else
                Throw New InvalidOperationException("Cannot identify URL kind (tag, user profile or search query).")

            End If

            Program.WriteColoredLine($"[Page {page:N0} of {lastPageNumber:N0}]: {pageUrl.Replace("https://www.", String.Empty).Replace("http://www.", String.Empty)}", ConsoleColor.White)

            Dim pageResp As HttpResponseMessage = Nothing
            Dim pageHtml As String = String.Empty
            Dim pageHtmlEx As Exception = Nothing
            Try
                Dim pageReqEx As Exception = Nothing
                Dim pageReqUri As New Uri(pageUrl)
                Using pageReq As New HttpRequestMessage(HttpMethod.Get, pageReqUri)
                    pageReq.Headers.Referrer = AppGlobals.Referrer

                    ' Use the available auth handler for browsing, instead of accountAuths(0).
                    If startedWithAccount Then
                        Dim browseAuth As GoodFonAuth = Program.GetAvailableAuthHandler()
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
                        Program.WriteColoredLine($"Page request has timed out ({AppGlobals.HttpClientTimeOut:mm:ss} minutes)", ConsoleColor.Red)
                        pageReqEx = ex

                    Catch ex As Exception
                        Program.WriteColoredLine($"Page request error: {pageReqEx.Message}", ConsoleColor.Red)
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

            Dim doc As New HtmlDocument()
            doc.LoadHtml(pageHtml)

            Dim wallpaperNodes As HtmlNodeCollection =
                doc.DocumentNode.SelectNodes("//div[contains(@class,'wallpapers__item')]/div/a")

            If wallpaperNodes Is Nothing Then
                Program.WriteColoredLine($"No wallpaper nodes found on current page.", ConsoleColor.Yellow)
                Program.WriteColoredLine(String.Empty, Console.ForegroundColor)
                Await Task.Delay(Program.threadLocalRng.Value.Next(1, 1000))
                Continue For
            End If

            Dim urls As New List(Of String)()
            For Each node As HtmlNode In wallpaperNodes

                Dim href As String = node.GetAttributeValue("href", String.Empty)
                If String.IsNullOrWhiteSpace(href) Then
                    Continue For
                End If

                If href.Equals("#"c) Then
                    Continue For
                End If

                If href.IndexOf("/user/", StringComparison.OrdinalIgnoreCase) > 0 Then
                    Continue For
                End If

                If Not href.StartsWith("http", StringComparison.OrdinalIgnoreCase) Then
                    href = $"{AppGlobals.BaseUrl}/{href.TrimStart("/"c)}"
                End If

                urls.Add(href)
            Next node

            Dim totalItems As Integer = urls.Count
            If totalItems = 0 Then
                Program.WriteColoredLine("No valid wallpaper links after filtering.", ConsoleColor.Yellow)
                Program.WriteColoredLine(String.Empty, Console.ForegroundColor)
                Await Task.Delay(Program.threadLocalRng.Value.Next(1, 1000))
                Continue For
            End If

            ' ── MODIFICATION: PREVENTIVE VPN ROTATION CHECK ──────────────────────────
            ' If processing this page risks exceeding the 100 anonymous downloads limit,
            ' we rotate the VPN server right now before spinning up any parallel tasks.
            Dim currentIpUsage As Integer = Thread.VolatileRead(Program.downloadsOnCurrentIp)
            If (currentIpUsage + totalItems) > AppGlobals.MaxDownloadsPerIp AndAlso Program.GetAvailableAuthHandler() Is Nothing Then
                Await Program.RotateVpnServerAsync($"{currentIpUsage} downloads on current IP plus {totalItems} items on next page (total projected: {currentIpUsage + totalItems:N0}) would exceed the daily limit of {AppGlobals.MaxDownloadsPerIp} daily downloads per IP")
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
                                    Dim hashUrl As String = Program.GenerateMD5FromUrl(capturedUrl)
                                    alreadyDone = Program.processedUrlHashes.Contains(hashUrl)
                                End SyncLock

                                If alreadyDone Then
                                    Program.WriteColoredLine($"[Wall {capturedItem:D2}/{capturedTotal:D2}] [Downloaded: {Program.totalSavedWallpaperCount:N0}] Already downloaded", ConsoleColor.DarkGray)
                                    Interlocked.Increment(skippedOnThisPage)
                                    Interlocked.Increment(Program.totalSkippedWallpaperCount)
                                    Return
                                End If

                                Dim refForbiddenKeywordMatched As String = Nothing
                                If Program.UrlContainsAnyKeyword(capturedUrl, Program.forbiddenKeywords, refForbiddenKeywordMatched) Then
                                    Program.WriteColoredLine($"[Wall {capturedItem:D2}/{capturedTotal:D2}] [Downloaded: {Program.totalSavedWallpaperCount:N0}] Forbidden match ({refForbiddenKeywordMatched})", ConsoleColor.DarkYellow)
                                    Interlocked.Increment(skippedOnThisPage)
                                    Interlocked.Increment(Program.totalSkippedWallpaperCount)
                                    Return
                                End If
                                ' ─────────────────────────────────────────────────────────

                                Dim downloadSuccess As TriState = TriState.False
                                Dim downloadEx As Exception = Nothing
                                Try
                                    downloadSuccess = Await Program.DownloadOriginalWallpaperAsync(client, capturedUrl, saveFolder, capturedItem, capturedTotal, Nothing)
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
                                        Program.WriteColoredLine($"[Wall {capturedItem:D2}/{capturedTotal:D2}] [Downloaded: {Program.totalSavedWallpaperCount:N0}] OK (with user-account {Program.currentAccountIndex + 1:N0}/{Program.accountAuths.Count:N0})", ConsoleColor.Green)
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

        Dim originalUrl As String = wallpaperPageUrl
        Dim doc As New HtmlDocument()
        Dim downloadNode As HtmlNode
        Dim lastHtml As String = String.Empty

        ' STEP 1 — Fetch wallpaper page
        Try
            Dim reqUri As New Uri(wallpaperPageUrl)
            Using req As New HttpRequestMessage(HttpMethod.Get, wallpaperPageUrl)
                req.Headers.Referrer = AppGlobals.Referrer

                Dim resp As HttpResponseMessage = Nothing
                Try
                    Try
                        resp = Await client.SendAsync(req, HttpCompletionOption.ResponseContentRead)

                    Catch ex As TaskCanceledException
                        Program.WriteColoredLine($"[Wall {capturedItem:D2}/{capturedTotal:D2}] [Downloaded: {Program.totalSavedWallpaperCount:N0}] HTTP request for wallpaper page {Program.GetRelativeUrl(wallpaperPageUrl)} has timed out ({AppGlobals.HttpClientTimeOut:mm:ss} minutes)", ConsoleColor.Red)
                        Interlocked.Increment(Program.totalFailedWallpaperCount)
                        Return TriState.False

                    Catch ex As Exception
                        Program.WriteColoredLine($"[Wall {capturedItem:D2}/{capturedTotal:D2}] [Downloaded: {Program.totalSavedWallpaperCount:N0}] HTTP request error for wallpaper page {Program.GetRelativeUrl(wallpaperPageUrl)}: {ex.Message}", ConsoleColor.Red)
                        Interlocked.Increment(Program.totalFailedWallpaperCount)
                        Return TriState.False

                    End Try

                    If (resp.StatusCode = HttpStatusCode.Forbidden) AndAlso currentAuth Is Nothing Then

                        Dim authToUse As GoodFonAuth = Program.GetAvailableAuthHandler()
                        If authToUse IsNot Nothing Then
                            Return Await Program.DownloadOriginalWallpaperAsync(authToUse.Client, wallpaperPageUrl, saveFolder, capturedItem, capturedTotal, authToUse)
                        End If

                        Program.WriteColoredLine($"[Wall {capturedItem:D2}/{capturedTotal:D2}] [Downloaded: {Program.totalSavedWallpaperCount:N0}] User account required", ConsoleColor.DarkYellow)
                        Interlocked.Increment(Program.totalSkippedWallpaperCount)
                        Return TriState.False
                    End If

                    If (resp.StatusCode = HttpStatusCode.NotFound) OrElse Not resp.IsSuccessStatusCode Then
                        Program.WriteColoredLine($"[Wall {capturedItem:D2}/{capturedTotal:D2}] [Downloaded: {Program.totalSavedWallpaperCount:N0}] HTTP response error for wallpaper page {Program.GetRelativeUrl(wallpaperPageUrl)}: {CInt(resp.StatusCode)} ({resp.StatusCode})", ConsoleColor.Red)
                        Interlocked.Increment(Program.totalFailedWallpaperCount)
                        Return TriState.False
                    End If

                    Try
                        lastHtml = Await resp.Content.ReadAsStringAsync()

                    Catch ex As Exception
                        Program.WriteColoredLine($"[Wall {capturedItem:D2}/{capturedTotal:D2}] [Downloaded: {Program.totalSavedWallpaperCount:N0}] Failed to read content for wallpaper page {Program.GetRelativeUrl(wallpaperPageUrl)}: {ex.Message}", ConsoleColor.Red)
                        Interlocked.Increment(Program.totalFailedWallpaperCount)
                        Return TriState.False

                    End Try

                Finally
                    resp?.Dispose()

                End Try
            End Using

            Await Task.Delay(300)

            doc.LoadHtml(lastHtml)

            downloadNode = doc.DocumentNode.SelectSingleNode(
                "//a[contains(@class,'wallpaper__download__rbut') and contains(@class,'js-size')]")

        Catch ex As Exception
            Program.WriteColoredLine($"[Wall {capturedItem:D2}/{capturedTotal:D2}] [Downloaded: {Program.totalSavedWallpaperCount:N0}] Failed to get wallpaper page {Program.GetRelativeUrl(originalUrl)}: {ex.Message}", ConsoleColor.Red)
            Interlocked.Increment(Program.totalFailedWallpaperCount)
            Return TriState.False
        End Try

        If downloadNode Is Nothing Then
            Program.WriteColoredLine($"[Wall {capturedItem:D2}/{capturedTotal:D2}] [Downloaded: {Program.totalSavedWallpaperCount:N0}] Download button disabled or not found.", ConsoleColor.Yellow)
            Interlocked.Increment(Program.totalSkippedWallpaperCount)
            Return TriState.False
        End If

        Dim downloadRelativeUrl As String = downloadNode.GetAttributeValue("href", String.Empty)
        If String.IsNullOrWhiteSpace(downloadRelativeUrl) Then
            Program.WriteColoredLine($"[Wall {capturedItem:D2}/{capturedTotal:D2}] [Downloaded: {Program.totalSavedWallpaperCount:N0}] Download href empty on {Program.GetRelativeUrl(wallpaperPageUrl)}", ConsoleColor.Red)
            Interlocked.Increment(Program.totalFailedWallpaperCount)
            Return TriState.False
        End If

        Dim downloadAbsoluteUrl As String = If(
            downloadRelativeUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase),
            downloadRelativeUrl,
            $"{AppGlobals.BaseUrl}/{downloadRelativeUrl.TrimStart("/"c)}")

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
                    Program.WriteColoredLine($"[Wall {capturedItem:D2}/{capturedTotal:D2}] [Downloaded: {Program.totalSavedWallpaperCount:N0}] HTTP request for download page {Program.GetRelativeUrl(downloadAbsoluteUrl)} has timed out ({AppGlobals.HttpClientTimeOut:mm:ss} minutes)", ConsoleColor.Red)
                    Interlocked.Increment(Program.totalFailedWallpaperCount)
                    Return TriState.False

                Catch ex As Exception
                    Program.WriteColoredLine($"[Wall {capturedItem:D2}/{capturedTotal:D2}] [Downloaded: {Program.totalSavedWallpaperCount:N0}] HTTP request error for download page {Program.GetRelativeUrl(downloadAbsoluteUrl)}: {ex.Message}", ConsoleColor.Red)
                    Interlocked.Increment(Program.totalFailedWallpaperCount)
                    Return TriState.False

                End Try

                If (resp2.StatusCode = HttpStatusCode.NotFound) OrElse Not resp2.IsSuccessStatusCode Then
                    Program.WriteColoredLine($"[Wall {capturedItem:D2}/{capturedTotal:D2}] [Downloaded: {Program.totalSavedWallpaperCount:N0}] HTTP response error for download page {Program.GetRelativeUrl(downloadAbsoluteUrl)}: {CInt(resp2.StatusCode)} ({resp2.StatusCode})", ConsoleColor.Red)
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

        Dim downloadDoc As New HtmlDocument()
        downloadDoc.LoadHtml(downloadHtml)

        ' ── Daily download-limit check ────────────────────────────────────────────
        If downloadDoc.DocumentNode.OuterHtml.IndexOf("Unfortunately, you have reached your download limit for today", StringComparison.OrdinalIgnoreCase) >= 0 Then

            If currentAuth IsNot Nothing Then
                Program.MarkAccountLimitReached(currentAuth)
                Program.WriteColoredLine($"[Wall {capturedItem:D2}/{capturedTotal:D2}] [Downloaded: {Program.totalSavedWallpaperCount:N0}] Download limit reached for today with user-account {Program.currentAccountIndex:N0}/{Program.accountAuths.Count:N0}.", ConsoleColor.Red)
            Else
                Program.WriteColoredLine($"[Wall {capturedItem:D2}/{capturedTotal:D2}] [Downloaded: {Program.totalSavedWallpaperCount:N0}] Download limit reached for today.", ConsoleColor.Red)
            End If

            Interlocked.Increment(Program.totalFailedWallpaperCount)

            If currentAuth Is Nothing Then
                Await Program.RotateVpnServerAsync($"Daily {AppGlobals.MaxDownloadsPerIp} download limit reached on the current IP")
            End If

            Return TriState.False
        End If
        ' ──────────────────────────────────────────────────────────────────────────

        ' STEP 4 — Locate image node
        Dim imgNode As HtmlNode =
            If(If(downloadDoc.DocumentNode.SelectSingleNode(
                "//a[contains(@class,'wallpaper__download__link')]/img"), downloadDoc.DocumentNode.SelectSingleNode(
                "//img[contains(@class,'wallpaper') or contains(@class,'wallpaper__img')]")), downloadDoc.DocumentNode.SelectSingleNode("//img"))

        If imgNode Is Nothing Then
            Program.WriteColoredLine($"[Wall {capturedItem:D2}/{capturedTotal:D2}] [Downloaded: {Program.totalSavedWallpaperCount:N0}] Image node not found on download page {Program.GetRelativeUrl(downloadAbsoluteUrl)}", ConsoleColor.Red)
            Interlocked.Increment(Program.totalFailedWallpaperCount)
            Return TriState.False
        End If

        Dim imageUrl As String = imgNode.GetAttributeValue("src", String.Empty)
        If String.IsNullOrWhiteSpace(imageUrl) Then
            Program.WriteColoredLine($"[Wall {capturedItem:D2}/{capturedTotal:D2}] [Downloaded: {Program.totalSavedWallpaperCount:N0}] Image src empty on {Program.GetRelativeUrl(downloadAbsoluteUrl)}", ConsoleColor.Red)
            Interlocked.Increment(Program.totalFailedWallpaperCount)
            Return TriState.False
        End If
        If Not imageUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase) Then
            imageUrl = $"{AppGlobals.BaseUrl}/{imageUrl.TrimStart("/"c)}"
        End If

        ' STEP 5 — Resolve filename and save path
        Dim fileName As String = Path.GetFileName(New Uri(imageUrl).LocalPath)
        If String.IsNullOrWhiteSpace(fileName) Then
            Program.WriteColoredLine($"[Wall {capturedItem:D2}/{capturedTotal:D2}] [Downloaded: {Program.totalSavedWallpaperCount:N0}] Cannot resolve file name for URL: {Program.GetRelativeUrl(imageUrl)}", ConsoleColor.Red)
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

            'If usingAuthClient AndAlso (Program.accountAuths) IsNot Nothing AndAlso (Program.accountAuths.Count <> 0) Then
            '    Dim cookieHeader As String = Program.accountAuths(Program.currentAccountIndex).CookieContainer.GetCookieHeader(imgReqUri)
            '    If Not String.IsNullOrWhiteSpace(cookieHeader) Then
            '        imgReq.Headers.Add("Cookie", cookieHeader)
            '    End If
            'End If

            Dim imageBytes As Byte() = Nothing
            Dim imgResp As HttpResponseMessage = Nothing
            Try

                Try
                    imgResp = Await client.SendAsync(imgReq, HttpCompletionOption.ResponseContentRead)

                Catch ex As TaskCanceledException
                    Program.WriteColoredLine($"[Wall {capturedItem:D2}/{capturedTotal:D2}] [Downloaded: {Program.totalSavedWallpaperCount:N0}] HTTP request for image {downloadAbsoluteUrl} has timed out ({AppGlobals.HttpClientTimeOut:mm:ss} minutes)", ConsoleColor.Red)
                    Interlocked.Increment(Program.totalFailedWallpaperCount)
                    Return TriState.False

                Catch ex As Exception
                    Program.WriteColoredLine($"[Wall {capturedItem:D2}/{capturedTotal:D2}] [Downloaded: {Program.totalSavedWallpaperCount:N0}] HTTP request error for image {Program.GetRelativeUrl(imageUrl)}: {ex.Message}", ConsoleColor.Red)
                    Interlocked.Increment(Program.totalFailedWallpaperCount)
                    Return TriState.False

                End Try

                If (imgResp.StatusCode = HttpStatusCode.Forbidden) AndAlso currentAuth Is Nothing Then

                    Dim authToUse As GoodFonAuth = Program.GetAvailableAuthHandler()
                    If authToUse IsNot Nothing Then
                        Return Await Program.DownloadOriginalWallpaperAsync(authToUse.Client, wallpaperPageUrl, saveFolder, capturedItem, capturedTotal, currentAuth)
                    End If

                    Program.WriteColoredLine($"[Wall {capturedItem:D2}/{capturedTotal:D2}] [Downloaded: {Program.totalSavedWallpaperCount:N0}] User account required", ConsoleColor.DarkYellow)
                    Interlocked.Increment(Program.totalSkippedWallpaperCount)
                    Return TriState.False
                End If

                If (imgResp.StatusCode = HttpStatusCode.NotFound) OrElse Not imgResp.IsSuccessStatusCode Then
                    Program.WriteColoredLine($"[Wall {capturedItem:D2}/{capturedTotal:D2}] [Downloaded: {Program.totalSavedWallpaperCount:N0}] HTTP response error for image {Program.GetRelativeUrl(imageUrl)}: {CInt(imgResp.StatusCode)} ({imgResp.StatusCode})", ConsoleColor.Red)
                    Interlocked.Increment(Program.totalFailedWallpaperCount)
                    Return TriState.False
                End If

                Try
                    imageBytes = Await imgResp.Content.ReadAsByteArrayAsync()

                Catch ex As Exception
                    Program.WriteColoredLine($"[Wall {capturedItem:D2}/{capturedTotal:D2}] [Downloaded: {Program.totalSavedWallpaperCount:N0}] Failed to read byte-content for image: {Program.GetRelativeUrl(imageUrl)}", ConsoleColor.Red)
                    Interlocked.Increment(Program.totalFailedWallpaperCount)
                    Return TriState.False

                End Try

            Finally
                imgResp?.Dispose()

            End Try

            If imageBytes Is Nothing OrElse imageBytes.Length = 0 Then
                Program.WriteColoredLine($"[Wall {capturedItem:D2}/{capturedTotal:D2}] [Downloaded: {Program.totalSavedWallpaperCount:N0}] Downloaded image is empty: {Program.GetRelativeUrl(imageUrl)}", ConsoleColor.Red)
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
                Interlocked.Increment(Program.downloadsOnCurrentIp)
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

            Dim urlHash As String = Program.GenerateMD5FromUrl(url)
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
                        ' Pausar el hilo actual brevemente para dar tiempo al antivirus a liberar el archivo
                        Thread.Sleep(retryDelayMilliseconds)
                    Catch ex As Exception
                        ' Para cualquier otra excepción que no sea de I/O (por ejemplo, disco lleno o permisos) salimos directo
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
    ''' </summary>
    <DebuggerStepThrough>
    Private Async Function RotateVpnServerAsync(reason As String) As Task

        ' Only the thread that atomically flips 0→1 gets to run the rotation.
        ' Every other concurrent caller sees 1 already set and bails out.
        If Interlocked.CompareExchange(Program.vpnRotationInProgress, 1, 0) <> 0 Then
            Return
        End If

        Try
            Program.WriteColoredLine($"Proton VPN rotation triggered ({reason})...", ConsoleColor.Magenta)
            UtilNetwork.ChangeServerInProtonVpnClient()
            Await Task.Delay(1000)
            Interlocked.Exchange(Program.downloadsOnCurrentIp, 0)
            Program.WriteColoredLine("Proton VPN rotation done.", ConsoleColor.Magenta)

        Catch ex As Exception
            Dim errMsg As String = If(ex.InnerException IsNot Nothing, ex.InnerException.Message, ex.Message)
            Program.WriteColoredLine($"  Proton VPN rotation error: {errMsg}", ConsoleColor.Red)

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
        Dim doc As New HtmlDocument()
        doc.LoadHtml(html)

        Dim lastPageNumber As Integer? = Nothing

        ' 1. Category listing pages (RESTORED ORIGINAL EXACT LOGIC)
        Dim pageLinks As HtmlNodeCollection = doc.DocumentNode.SelectNodes("//a[contains(@href,'index-') and contains(@href,'.html')]")

        If pageLinks IsNot Nothing Then
            For Each link As HtmlNode In pageLinks
                Dim href As String = link.GetAttributeValue("href", String.Empty)
                Dim match As Match = AppGlobals.PaginationIndexRegex.Match(href)
                If match.Success Then
                    Dim pageNumber As Integer
                    If Integer.TryParse(match.Groups(1).Value, pageNumber) Then
                        If Not lastPageNumber.HasValue OrElse (pageNumber > lastPageNumber.Value) Then
                            lastPageNumber = pageNumber
                        End If
                    End If
                End If
            Next
        End If

        ' 2. Search result paginator (RESTORED ORIGINAL EXACT LOGIC)
        Dim paginator As HtmlNode = doc.DocumentNode.SelectSingleNode("//div[contains(@class,'paginator')]")

        If paginator IsNot Nothing Then
            Dim lastLink As HtmlNode = paginator.SelectSingleNode(".//a[contains(.,'Last')]")
            If lastLink IsNot Nothing Then
                Dim href As String = lastLink.GetAttributeValue("href", String.Empty)
                Dim match As Match = AppGlobals.PaginationQueryRegex.Match(href)
                If match.Success Then
                    Dim pageNumber As Integer
                    If Integer.TryParse(match.Groups(1).Value, pageNumber) Then
                        If Not lastPageNumber.HasValue OrElse (pageNumber > lastPageNumber.Value) Then
                            lastPageNumber = pageNumber
                        End If
                    End If
                End If
            End If
        End If

        ' Helper function to accurately count ONLY valid wallpaper nodes, excluding user profiles and empty hrefs
        Dim countValidWallpapers As Func(Of HtmlDocument, Integer) =
            Function(d As HtmlDocument) As Integer
                Dim nodes As HtmlNodeCollection = d.DocumentNode.SelectNodes("//div[contains(@class,'wallpapers__item')]/div/a")
                If nodes Is Nothing Then
                    Return 0
                End If

                Dim count As Integer = 0
                For Each node As HtmlNode In nodes
                    Dim href As String = node.GetAttributeValue("href", String.Empty)
                    If Not String.IsNullOrWhiteSpace(href) AndAlso Not href.Equals("#"c) AndAlso href.IndexOf("/user/", StringComparison.OrdinalIgnoreCase) = -1 Then
                        count += 1
                    End If
                Next
                Return count
            End Function

        Dim finalLastPage As Integer = If(lastPageNumber.HasValue, lastPageNumber.Value, 1)
        Dim totalWallpapers As Integer = 0

        If finalLastPage <= 1 Then
            totalWallpapers = countValidWallpapers(doc)

            ' Matches original exception throw if strictly 0 results are found overall
            If totalWallpapers = 0 AndAlso Not lastPageNumber.HasValue Then
                Throw New InvalidOperationException("Failed to identify last page number in the provided HTML.")
            End If
        Else
            Dim lastPageUrl As String = String.Empty

            If baseUrl.IndexOf("/tags/", StringComparison.OrdinalIgnoreCase) > 0 Then
                lastPageUrl = $"{baseUrl}index-{finalLastPage}.html"
            ElseIf baseUrl.IndexOf("/user/", StringComparison.OrdinalIgnoreCase) > 0 Then
                lastPageUrl = $"{baseUrl}/?&page={finalLastPage}"
            ElseIf baseUrl.IndexOf("?q", StringComparison.OrdinalIgnoreCase) > 0 Then
                lastPageUrl = $"{baseUrl}&page={finalLastPage}"
            End If

            If Not String.IsNullOrWhiteSpace(lastPageUrl) Then
                Try
                    Dim lastPageHtml As String = Await client.GetStringAsync(lastPageUrl)
                    Dim lastPageDoc As New HtmlDocument()
                    lastPageDoc.LoadHtml(lastPageHtml)

                    Dim itemsOnLastPage As Integer = countValidWallpapers(lastPageDoc)
                    totalWallpapers = ((finalLastPage - 1) * AppGlobals.MaxWallpaperItemsPerPage) + itemsOnLastPage

                Catch ex As Exception
                    totalWallpapers = (finalLastPage - 1) * AppGlobals.MaxWallpaperItemsPerPage
                End Try
            Else
                totalWallpapers = (finalLastPage - 1) * AppGlobals.MaxWallpaperItemsPerPage
            End If
        End If

        Return (finalLastPage, totalWallpapers)
    End Function

    ''' <summary>
    ''' Checks if the given text contains any of the keywords in the provided set, using case-insensitive comparison.
    ''' </summary>
    ''' 
    ''' <param name="url">
    ''' The URL to check for keywords.
    ''' </param>
    ''' 
    ''' <param name="keywords">
    ''' The set of keywords to look for in the text. If any keyword is found, the function returns True.
    ''' </param>
    ''' 
    ''' <param name="refKeywordMatched">
    ''' When this method returns, contains the first keyword that was found in the text, or an empty string if no keywords were found.
    ''' </param>
    ''' 
    ''' <returns>
    ''' Returns True if any of the keywords are present in the text; 
    ''' otherwise, returns False.
    ''' </returns>
    <DebuggerStepThrough>
    Private Function UrlContainsAnyKeyword(url As String, keywords As HashSet(Of String), ByRef refKeywordMatched As String) As Boolean

        If keywords Is Nothing OrElse keywords.Count = 0 Then
            Return False
        End If

        For Each word As String In keywords
            If url.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0 Then
                refKeywordMatched = word
                Return True
            End If
        Next word

        Return False
    End Function

    ''' <summary>
    ''' Loads the search terms, required keywords and forbidden keywords from the config text files into the corresponding HashSet fields.
    ''' <para></para>
    ''' Exits with error if any file is missing.
    ''' </summary>
    <DebuggerStepThrough>
    Private Sub LoadConfigFiles()

        Dim extendedSearchTermsFilePath As String =
            FileSystemHelper.GetExtendedPath(Path.GetFullPath(AppGlobals.SearchTermsFilePath))

        If Not File.Exists(extendedSearchTermsFilePath) Then
            Program.ExitWithMessage($"[ERROR] Search terms file not found: {AppGlobals.SearchTermsFilePath}", 2, ConsoleColor.Red)
        End If

        Program.searchTerms =
            (From line In File.ReadAllLines(extendedSearchTermsFilePath, AppGlobals.TextEncoding)
             Let trimmed = line.Trim()
             Where Not String.IsNullOrWhiteSpace(trimmed) AndAlso Not trimmed.StartsWith("#"c)
             Select trimmed
            ).ToHashSet(StringComparer.OrdinalIgnoreCase)

        Dim extendedForbiddenKeywordsFilePath As String =
            FileSystemHelper.GetExtendedPath(Path.GetFullPath(AppGlobals.ForbiddenKeywordsFilePath))

        If File.Exists(extendedForbiddenKeywordsFilePath) Then
            Program.forbiddenKeywords =
                (From line As String In File.ReadAllLines(extendedForbiddenKeywordsFilePath, AppGlobals.TextEncoding)
                 Let trimmed As String = line.Trim()
                 Where Not String.IsNullOrWhiteSpace(trimmed) AndAlso Not trimmed.StartsWith("#"c)
                 Select trimmed
                ).ToHashSet(StringComparer.OrdinalIgnoreCase)
        Else
            Program.WriteColoredLine($"[WARN] Forbidden keywords file not found: {AppGlobals.ForbiddenKeywordsFilePath}", ConsoleColor.Yellow)
            Program.forbiddenKeywords = New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        End If

        If Program.searchTerms Is Nothing OrElse Program.searchTerms.Count = 0 Then
            Program.ExitWithMessage($"[ERROR] No search terms found in: {AppGlobals.SearchTermsFilePath}", 3, ConsoleColor.Red)
        End If
    End Sub

    ''' <summary>
    ''' Loads the GoodFon authentication configurations from a JSON file.
    ''' </summary>
    ''' 
    ''' <returns>
    ''' A <see cref="IReadOnlyList(Of GoodFonAccountConfig)"/> instance populated from the configuration file,
    ''' or <c>Nothing</c> if the file does not exist.
    ''' </returns>
    ''' 
    ''' <exception cref="InvalidOperationException"></exception>
    <DebuggerStepThrough>
    Private Function LoadGoodFonAccounts() As IReadOnlyList(Of GoodFonAccountConfig)

        Dim extendedGoodfonAccountsFilePath As String =
            FileSystemHelper.GetExtendedPath(Path.GetFullPath(AppGlobals.GoodfonAccountsFilePath))

        If Not File.Exists(extendedGoodfonAccountsFilePath) Then
            Return Nothing
            ' Throw New FileNotFoundException($"Config file not found: {AppGlobals.GoodfonAuthConfigFilePath}")
        End If

        Dim jsonContent As String = File.ReadAllText(extendedGoodfonAccountsFilePath, AppGlobals.TextEncoding)

        If String.IsNullOrWhiteSpace(jsonContent) Then
            Throw New InvalidOperationException("Accounts json file is empty.")
        End If

        Dim options As New JsonSerializerOptions With {
            .PropertyNameCaseInsensitive = True,
            .ReadCommentHandling = JsonCommentHandling.Skip
        }

        Dim configsList As List(Of GoodFonAccountConfig) = JsonSerializer.Deserialize(Of List(Of GoodFonAccountConfig))(jsonContent, options)

        If configsList Is Nothing Then
            Throw New InvalidOperationException("Failed to deserialize accounts json file content.")
        End If

        Return configsList
    End Function

    ''' <summary>
    ''' Loads the history of already-processed image URLs from the history text file into a HashSet for quick lookup.
    ''' </summary>
    <DebuggerStepThrough>
    Private Sub LoadUrlsHistoryHashes()

        Dim extendedHistoryFilePath As String =
            FileSystemHelper.GetExtendedPath(Path.GetFullPath(AppGlobals.HistoryFilePath))

        Program.processedUrlHashes =
            If(Not File.Exists(extendedHistoryFilePath),
                New HashSet(Of String)(StringComparer.OrdinalIgnoreCase),
                (From line As String In File.ReadAllLines(extendedHistoryFilePath, AppGlobals.TextEncoding)
                 Let trimmed As String = line.Trim()
                 Where Not String.IsNullOrWhiteSpace(trimmed) AndAlso Not trimmed.StartsWith("#"c)
                 Select trimmed
                ).ToHashSet(StringComparer.OrdinalIgnoreCase)
            )
    End Sub

    ''' <summary>
    ''' Generates an MD5 hash from the given URL.
    ''' </summary>
    ''' 
    ''' <param name="url">
    ''' The URL to hash it.
    ''' </param>
    ''' 
    ''' <returns>
    ''' The resulting MD5 hash in its hexadecimal representation.
    ''' </returns>
    <DebuggerStepThrough>
    Private Function GetRelativeUrl(url As String) As String

        Const RelativeUrlDelimiter As String = ".com/"

        Dim delimiterIndex As Integer = url.IndexOf(RelativeUrlDelimiter, StringComparison.OrdinalIgnoreCase)

        Dim relativeUrl As String =
            If(delimiterIndex <> -1,
                url.Substring(delimiterIndex + (RelativeUrlDelimiter.Length - 1)),
                url)

        Return relativeUrl
    End Function

    ''' <summary>
    ''' Generates an MD5 hash from the given URL.
    ''' </summary>
    ''' 
    ''' <param name="url">
    ''' The URL to hash it.
    ''' </param>
    ''' 
    ''' <returns>
    ''' The resulting MD5 hash in its hexadecimal representation.
    ''' </returns>
    <DebuggerStepThrough>
    Private Function GenerateMD5FromUrl(url As String) As String

        Dim relativeUrl As String = Program.GetRelativeUrl(url)

        Dim inputBytes As Byte() = AppGlobals.TextEncoding.GetBytes(relativeUrl)

        Dim hashBytes() As Byte
        Using md5Provider As MD5 = MD5.Create()
            hashBytes = md5Provider.ComputeHash(inputBytes)
        End Using

        Dim hash As String = BitConverter.ToString(hashBytes).Replace("-"c, "")
        Return hash
    End Function

    ''' <summary>
    ''' Writes a message to the console in a specified foreground color, 
    ''' then resets the color back to the original.
    ''' </summary>
    ''' 
    ''' <param name="line">
    ''' The message to display. If empty or null, no message is displayed.
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

        Console.WriteLine($"Exiting application with exit code: {exitCode} (0x{exitCode}) ...")
#If DEBUG Then
        Console.WriteLine()
        Program.WriteColoredLine("[!] This message only appears in DEBUG mode to prevent accidental termination.", ConsoleColor.Yellow)
        Console.WriteLine("Press any key to exit...")
        Console.ReadKey(intercept:=True)
#End If
        Environment.Exit(exitCode)
    End Sub

    <DebuggerStepThrough>
    Private Function GetAvailableAuthHandler() As GoodFonAuth

        SyncLock Program.accountLockObject
            Dim authToUse As GoodFonAuth = Nothing

            If (Program.accountAuths IsNot Nothing) AndAlso (Program.accountAuths.Count <> 0) Then

                ' Check for any account that has exhausted its limits to see if 24 hours + 2 mins have passed.
                For i As Integer = 0 To Program.accountAuths.Count - 1
                    Dim auth As GoodFonAuth = Program.accountAuths(i)

                    If auth.DailyLimitReached AndAlso Program.accountLimitReachedTimes.ContainsKey(auth) Then
                        Dim limitTime As Date = Program.accountLimitReachedTimes(auth)
                        Dim timeSpanSinceLimit As TimeSpan = Date.Now - limitTime

                        ' 24 hours + 2 minutes = 1442 minutes
                        If timeSpanSinceLimit.TotalMinutes >= 1442 Then
                            auth.DailyLimitReached = False
                            Program.accountLimitReachedTimes.Remove(auth)
                            Program.WriteColoredLine($"[Auth] The daily limit for user-account {i + 1:N0} has been automatically reset after 24 hours.", ConsoleColor.Green)

                            ' Return the index point if we skipped past this account previously
                            If Program.currentAccountIndex > i Then
                                Program.currentAccountIndex = i
                            End If
                        End If
                    End If
                Next

                ' Retrieve the current active account.
                If Program.currentAccountIndex < Program.accountAuths.Count Then
                    Dim currentAth As GoodFonAuth = Program.accountAuths(Program.currentAccountIndex)

                    If Not currentAth.DailyLimitReached Then
                        authToUse = currentAth
                    End If
                End If
            End If

            Return authToUse
        End SyncLock

    End Function

    ''' <summary>
    ''' Thread-safe method to mark a specific account as exhausted and rotate to the next one.
    ''' </summary>
    ''' <param name="failedAuth">The authentication handler that received the limit-reached response.</param>
    <DebuggerStepThrough>
    Private Sub MarkAccountLimitReached(failedAuth As GoodFonAuth)

        SyncLock Program.accountLockObject
            If Not failedAuth.DailyLimitReached Then
                failedAuth.DailyLimitReached = True

                ' Record the exact time the limit was reached to evaluate the 24h reset cooldown.
                Program.accountLimitReachedTimes(failedAuth) = Date.Now

                ' Only increment the global index if the account we just exhausted is still the current active one.
                If (Program.currentAccountIndex < Program.accountAuths.Count) AndAlso (Program.accountAuths(Program.currentAccountIndex) Is failedAuth) Then

                    Program.currentAccountIndex += 1

                    If Program.currentAccountIndex >= Program.accountAuths.Count Then
                        Program.WriteColoredLine("[Auth] All user-accounts have reached their daily download limit. Operating on anonymous IP limit mode.", ConsoleColor.Yellow)
                    End If

                End If
            End If
        End SyncLock

    End Sub

#End Region

End Module
