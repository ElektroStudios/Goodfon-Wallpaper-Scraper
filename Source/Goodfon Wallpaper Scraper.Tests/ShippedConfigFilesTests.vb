#Region " Option Statements "

Option Explicit On
Option Strict On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Collections.Generic
Imports System.IO
Imports System.Linq

#End Region

#Region " ShippedConfigFilesTests "

''' <summary>
''' Validates the configuration files shipped with the application ("config" directory),
''' which are linked into the test output directory as "TestData\config".
''' </summary>
<TestClass>
Public NotInheritable Class ShippedConfigFilesTests

#Region " Test Helpers "

    ''' <summary>
    ''' Gets the full path of a shipped configuration file in the test output directory.
    ''' </summary>
    '''
    ''' <param name="fileName">
    ''' The configuration file name.
    ''' </param>
    '''
    ''' <returns>
    ''' The full path of the configuration file.
    ''' </returns>
    Private Shared Function GetConfigFilePath(fileName As String) As String

        Dim filePath As String = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData", "config", fileName)
        Assert.IsTrue(File.Exists(filePath), $"Shipped config file not found in test output: {filePath}")
        Return filePath
    End Function

    ''' <summary>
    ''' Reads and parses a shipped list-based configuration file.
    ''' </summary>
    '''
    ''' <param name="fileName">
    ''' The configuration file name.
    ''' </param>
    '''
    ''' <returns>
    ''' The parsed entries.
    ''' </returns>
    Private Shared Function ParseListFile(fileName As String) As HashSet(Of String)

        Dim filePath As String = ShippedConfigFilesTests.GetConfigFilePath(fileName)
        Return ConfigFileParser.ParseListLines(File.ReadAllLines(filePath, AppGlobals.TextEncoding))
    End Function

#End Region

#Region " Tests "

    ''' <summary>
    ''' Verifies that the shipped search terms file contains valid entries, without comments.
    ''' </summary>
    <TestMethod>
    Public Sub SearchTermsFile_ContainsValidSearchTerms()

        Dim terms As HashSet(Of String) = ShippedConfigFilesTests.ParseListFile("Search Terms.txt")

        Assert.IsTrue(terms.Count > 0, "The search terms file must contain at least one term.")
        Assert.IsTrue(terms.Contains("cats"))
        Assert.IsFalse(terms.Any(Function(term As String) term.StartsWith("#", StringComparison.Ordinal)))
        Assert.IsFalse(terms.Any(Function(term As String) term.Length > 0 AndAlso term(0) = ChrW(&HFEFF)), "BOM leaked into a term.")

        ' Every term must produce valid URLs.
        For Each term As String In terms
            Assert.IsTrue(Uri.IsWellFormedUriString(UrlHelper.BuildTagUrl(term, 0), UriKind.Absolute), term)
            Assert.IsTrue(Uri.IsWellFormedUriString(UrlHelper.BuildUserProfileUrl(term), UriKind.Absolute), term)
            Assert.IsTrue(Uri.IsWellFormedUriString(UrlHelper.BuildSearchUrl(term), UriKind.Absolute), term)
        Next term
    End Sub

    ''' <summary>
    ''' Verifies that the shipped forbidden keywords file contains valid entries that filter matching URLs.
    ''' </summary>
    <TestMethod>
    Public Sub ForbiddenKeywordsFile_ContainsKeywordsThatFilterUrls()

        Dim keywords As HashSet(Of String) = ShippedConfigFilesTests.ParseListFile("Forbidden Keywords.txt")
        Dim matched As String = Nothing

        Assert.IsTrue(keywords.Count > 0)
        Assert.IsTrue(keywords.Contains("-ai-"))
        Assert.IsFalse(keywords.Any(Function(keyword As String) keyword.StartsWith("#", StringComparison.Ordinal)))
        Assert.IsTrue(UrlHelper.ContainsAnyKeyword("https://www.goodfon.com/ai-art/wallpaper-x.html", keywords, matched))
        Assert.IsFalse(UrlHelper.ContainsAnyKeyword("https://www.goodfon.com/cats/wallpaper-koshka.html", keywords, matched))
    End Sub

    ''' <summary>
    ''' Verifies that the shipped accounts file is valid JSON, and that every account is disabled by default.
    ''' </summary>
    <TestMethod>
    Public Sub AccountsFile_IsValidAndAllAccountsAreDisabledByDefault()

        Dim filePath As String = ShippedConfigFilesTests.GetConfigFilePath("Accounts.json")
        Dim accounts As List(Of GoodFonAccountConfig) = ConfigFileParser.ParseAccountsJson(File.ReadAllText(filePath, AppGlobals.TextEncoding))

        Assert.IsTrue(accounts.Count > 0)
        Assert.IsTrue(accounts.All(Function(account As GoodFonAccountConfig) Not account.Enabled), "Shipped accounts must be disabled.")
        Assert.IsTrue(accounts.All(Function(account As GoodFonAccountConfig) Not String.IsNullOrWhiteSpace(account.Username)))
    End Sub

    ''' <summary>
    ''' Verifies that the shipped accounts file uses the current format,
    ''' and that its login settings match the documented default values.
    ''' </summary>
    <TestMethod>
    Public Sub AccountsFile_ContainsDefaultLoginSettings()

        Dim filePath As String = ShippedConfigFilesTests.GetConfigFilePath("Accounts.json")
        Dim json As String = File.ReadAllText(filePath, AppGlobals.TextEncoding)
        Dim settings As GoodFonLoginSettings = ConfigFileParser.ParseAccountsFile(json).LoginSettings

        StringAssert.Contains(json, """LoginSettings""")
        Assert.AreEqual(GoodFonLoginSettings.DefaultRateLimitWaitSeconds, settings.RateLimitWaitSeconds)
        Assert.AreEqual(GoodFonLoginSettings.DefaultMaxLoginAttempts, settings.MaxLoginAttempts)
        Assert.AreEqual(GoodFonLoginSettings.DefaultDelayBetweenLoginsSeconds, settings.DelayBetweenLoginsSeconds)
        Assert.AreEqual(GoodFonLoginSettings.DefaultNetworkErrorWaitSeconds, settings.NetworkErrorWaitSeconds)
    End Sub

#End Region

End Class

#End Region
