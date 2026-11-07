#Region " Option Statements "

Option Explicit On
Option Strict On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Globalization
Imports System.IO
Imports System.Text.RegularExpressions

#End Region

#Region " AppGlobalsTests "

''' <summary>
''' Unit tests for the <see cref="AppGlobals"/> module.
''' </summary>
<TestClass>
Public NotInheritable Class AppGlobalsTests

#Region " URLs "

    ''' <summary>
    ''' Verifies that every GoodFon.com URL constant is built on top of the base URL, over HTTPS.
    ''' </summary>
    <TestMethod>
    Public Sub UrlConstants_AreBuiltOnBaseUrl()

        Dim baseUrl As String = AppGlobals.BaseUrl
        Dim tagUrl As String = AppGlobals.BaseGoodfonTagUrl
        Dim userUrl As String = AppGlobals.BaseGoodfonUserUrl
        Dim searchUrl As String = AppGlobals.BaseGoodfonSearchUrl

        Assert.AreEqual("https://www.goodfon.com", baseUrl)
        Assert.AreEqual("https://www.goodfon.com/tags", tagUrl)
        Assert.AreEqual("https://www.goodfon.com/user", userUrl)
        Assert.AreEqual("https://www.goodfon.com/search", searchUrl)
    End Sub

    ''' <summary>
    ''' Verifies that the URL constants have no trailing slash (the URL builders add it).
    ''' </summary>
    <TestMethod>
    Public Sub UrlConstants_HaveNoTrailingSlash()

        For Each url As String In New String() {AppGlobals.BaseUrl, AppGlobals.BaseGoodfonTagUrl, AppGlobals.BaseGoodfonUserUrl, AppGlobals.BaseGoodfonSearchUrl}
            Assert.IsFalse(url.EndsWith("/", StringComparison.Ordinal), url)
        Next url
    End Sub

    ''' <summary>
    ''' Verifies that the HTTP Referrer is the website root.
    ''' </summary>
    <TestMethod>
    Public Sub Referrer_IsWebsiteRoot()

        Assert.AreEqual(New Uri("https://www.goodfon.com/"), AppGlobals.Referrer)
    End Sub

#End Region

#Region " Regular Expressions "

    ''' <summary>
    ''' Verifies that the "index-N.html" pagination regex captures the page number, case-insensitively.
    ''' </summary>
    <TestMethod>
    Public Sub PaginationIndexRegex_IndexHref_CapturesPageNumber()

        Dim match As Match = AppGlobals.PaginationIndexRegex.Match("/tags/cats/index-12.html")
        Assert.IsTrue(match.Success)
        Assert.AreEqual("12", match.Groups(1).Value)

        Assert.AreEqual("3", AppGlobals.PaginationIndexRegex.Match("/TAGS/INDEX-3.HTML").Groups(1).Value)
        Assert.IsFalse(AppGlobals.PaginationIndexRegex.IsMatch("/tags/cats/index.html"))
        Assert.IsFalse(AppGlobals.PaginationIndexRegex.IsMatch("/tags/cats/index-x.html"))
    End Sub

    ''' <summary>
    ''' Verifies that the "page=N" pagination regex captures the page number, case-insensitively.
    ''' </summary>
    <TestMethod>
    Public Sub PaginationQueryRegex_PageQuery_CapturesPageNumber()

        Assert.AreEqual("7", AppGlobals.PaginationQueryRegex.Match("/search/?q=cats&page=7").Groups(1).Value)
        Assert.AreEqual("42", AppGlobals.PaginationQueryRegex.Match("/user/bob/?&PAGE=42").Groups(1).Value)
        Assert.IsFalse(AppGlobals.PaginationQueryRegex.IsMatch("/search/?q=cats"))
    End Sub

#End Region

#Region " Limits and Timeouts "

    ''' <summary>
    ''' Verifies the website limits used by the scraping logic.
    ''' </summary>
    <TestMethod>
    Public Sub Limits_HaveExpectedValues()

        Assert.AreEqual(80, AppGlobals.MaxDownloadsPerIp)
        Assert.AreEqual(24, AppGlobals.MaxWallpaperItemsPerPage)
        Assert.AreEqual(TimeSpan.FromMinutes(10), AppGlobals.PageFetchingMaxTimeout)
        Assert.AreEqual(TimeSpan.FromSeconds(300), AppGlobals.HttpClientTimeOut)
    End Sub

    ''' <summary>
    ''' Verifies that the HTTP client timeout is displayed in "mm:ss" format.
    ''' </summary>
    <TestMethod>
    Public Sub HttpClientTimeOutDisplay_IsFormattedAsMinutesAndSeconds()

        Assert.AreEqual("05:00", AppGlobals.HttpClientTimeOutDisplay)
    End Sub

    ''' <summary>
    ''' Regression test documenting why <see cref="AppGlobals.HttpClientTimeOutDisplay"/> exists:
    ''' an unescaped "mm:ss" custom format (previously used in the timeout messages) throws <see cref="FormatException"/>
    ''' for <see cref="TimeSpan"/> values, so every timeout message crashed instead of being displayed.
    ''' </summary>
    <TestMethod>
    Public Sub TimeSpanFormat_UnescapedColon_ThrowsFormatException()

        Assert.ThrowsExactly(Of FormatException)(Sub() AppGlobals.HttpClientTimeOut.ToString("mm:ss", CultureInfo.InvariantCulture))
    End Sub

#End Region

#Region " Encoding and Paths "

    ''' <summary>
    ''' Verifies that the text files encoding is UTF-8 with BOM.
    ''' </summary>
    <TestMethod>
    Public Sub TextEncoding_IsUtf8WithBom()

        CollectionAssert.AreEqual(New Byte() {&HEF, &HBB, &HBF}, AppGlobals.TextEncoding.GetPreamble())
        Assert.AreEqual(65001, AppGlobals.TextEncoding.CodePage)
    End Sub

    ''' <summary>
    ''' Verifies the file and directory names used by the application.
    ''' </summary>
    <TestMethod>
    Public Sub FilePaths_HaveExpectedNames()

        Assert.AreEqual("downloads history.txt", Path.GetFileName(AppGlobals.HistoryFilePath))
        Assert.AreEqual("accounts.json", Path.GetFileName(AppGlobals.GoodfonAccountsFilePath), ignoreCase:=True)
        Assert.AreEqual("search terms.txt", Path.GetFileName(AppGlobals.SearchTermsFilePath), ignoreCase:=True)
        Assert.AreEqual("forbidden keywords.txt", Path.GetFileName(AppGlobals.ForbiddenKeywordsFilePath), ignoreCase:=True)
        Assert.AreEqual("output", Path.GetFileName(AppGlobals.OutputDirPath))
    End Sub

    ''' <summary>
    ''' Verifies that the application title contains the version (three fields) and the author.
    ''' </summary>
    <TestMethod>
    Public Sub AppTitleAndVersion_ContainsVersionAndAuthor()

        StringAssert.Matches(AppGlobals.AppTitleAndVersion, New Regex(" v\d+\.\d+\.\d+ - by ElektroStudios$"))
    End Sub

#End Region

End Class

#End Region
