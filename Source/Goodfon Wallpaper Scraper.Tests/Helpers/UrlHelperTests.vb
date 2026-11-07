#Region " Option Statements "

Option Explicit On
Option Strict On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Collections.Generic
Imports System.Text.RegularExpressions

#End Region

#Region " UrlHelperTests "

''' <summary>
''' Unit tests for the <see cref="UrlHelper"/> module.
''' </summary>
<TestClass>
Public NotInheritable Class UrlHelperTests

#Region " GetRelativeUrl "

    ''' <summary>
    ''' Verifies that the relative part of a GoodFon.com URL starts at the slash after ".com".
    ''' </summary>
    <TestMethod>
    Public Sub GetRelativeUrl_GoodfonUrl_ReturnsPathStartingWithSlash()

        Dim result As String = UrlHelper.GetRelativeUrl("https://www.goodfon.com/cats/wallpaper-koshka.html")

        Assert.AreEqual("/cats/wallpaper-koshka.html", result)
    End Sub

    ''' <summary>
    ''' Verifies that the ".com/" delimiter is matched case-insensitively and the path casing is preserved.
    ''' </summary>
    <TestMethod>
    Public Sub GetRelativeUrl_UpperCaseDomain_IsCaseInsensitiveAndPreservesPathCasing()

        Dim result As String = UrlHelper.GetRelativeUrl("HTTPS://WWW.GOODFON.COM/Cats/Wallpaper.html")

        Assert.AreEqual("/Cats/Wallpaper.html", result)
    End Sub

    ''' <summary>
    ''' Verifies that the root URL returns a single slash.
    ''' </summary>
    <TestMethod>
    Public Sub GetRelativeUrl_RootUrl_ReturnsSlash()

        Assert.AreEqual("/", UrlHelper.GetRelativeUrl("https://www.goodfon.com/"))
    End Sub

    ''' <summary>
    ''' Verifies that URLs without a ".com/" delimiter are returned unchanged.
    ''' </summary>
    <TestMethod>
    Public Sub GetRelativeUrl_NoComDelimiter_ReturnsInputUnchanged()

        Assert.AreEqual("https://example.org/x", UrlHelper.GetRelativeUrl("https://example.org/x"))
        Assert.AreEqual(String.Empty, UrlHelper.GetRelativeUrl(String.Empty))
    End Sub

    ''' <summary>
    ''' Verifies that only the first ".com/" occurrence is used as delimiter.
    ''' </summary>
    <TestMethod>
    Public Sub GetRelativeUrl_MultipleComDelimiters_UsesFirstOccurrence()

        Dim result As String = UrlHelper.GetRelativeUrl("https://www.goodfon.com/redirect/other.com/page")

        Assert.AreEqual("/redirect/other.com/page", result)
    End Sub

    ''' <summary>
    ''' Verifies that a null URL throws <see cref="ArgumentNullException"/>.
    ''' </summary>
    <TestMethod>
    Public Sub GetRelativeUrl_Nothing_ThrowsArgumentNullException()

        Assert.ThrowsExactly(Of ArgumentNullException)(Sub() UrlHelper.GetRelativeUrl(Nothing))
    End Sub

#End Region

#Region " GenerateMD5FromUrl "

    ''' <summary>
    ''' Verifies the MD5 hash against known values computed from the relative URL bytes (UTF-8 without BOM).
    ''' <para></para>
    ''' This guarantees backward compatibility with existing "downloads history.txt" files.
    ''' </summary>
    <TestMethod>
    Public Sub GenerateMD5FromUrl_KnownUrls_ReturnsKnownHashes()

        Assert.AreEqual("5BEF669C644056C44AAF3406FA537C09", UrlHelper.GenerateMD5FromUrl("https://www.goodfon.com/cats/wallpaper-koshka.html"))
        Assert.AreEqual("6666CD76F96956469E7BE39D750CC7D9", UrlHelper.GenerateMD5FromUrl("https://www.goodfon.com/"))
        Assert.AreEqual("C0CD021216B68311B9921BDBA50A3697", UrlHelper.GenerateMD5FromUrl("https://example.org/x"))
    End Sub

    ''' <summary>
    ''' Verifies that the hash is a 32-character uppercase hexadecimal string without separators.
    ''' </summary>
    <TestMethod>
    Public Sub GenerateMD5FromUrl_AnyUrl_ReturnsUpperCaseHexWithoutSeparators()

        Dim hash As String = UrlHelper.GenerateMD5FromUrl("https://www.goodfon.com/nature/wallpaper-les.html")

        Assert.AreEqual(32, hash.Length)
        Assert.IsTrue(Regex.IsMatch(hash, "^[0-9A-F]{32}$"), $"Unexpected hash format: {hash}")
    End Sub

    ''' <summary>
    ''' Verifies that the hash depends only on the relative URL, so scheme and subdomain variations produce the same hash.
    ''' </summary>
    <TestMethod>
    Public Sub GenerateMD5FromUrl_SchemeAndHostVariations_ProduceSameHash()

        Dim expected As String = UrlHelper.GenerateMD5FromUrl("https://www.goodfon.com/cats/wallpaper-1.html")

        Assert.AreEqual(expected, UrlHelper.GenerateMD5FromUrl("http://www.goodfon.com/cats/wallpaper-1.html"))
        Assert.AreEqual(expected, UrlHelper.GenerateMD5FromUrl("https://goodfon.com/cats/wallpaper-1.html"))
        Assert.AreEqual(expected, UrlHelper.GenerateMD5FromUrl("https://ru.goodfon.com/cats/wallpaper-1.html"))
    End Sub

    ''' <summary>
    ''' Verifies that different wallpaper paths produce different hashes, and that the path is case-sensitive.
    ''' </summary>
    <TestMethod>
    Public Sub GenerateMD5FromUrl_DifferentPaths_ProduceDifferentHashes()

        Dim hash1 As String = UrlHelper.GenerateMD5FromUrl("https://www.goodfon.com/cats/wallpaper-1.html")
        Dim hash2 As String = UrlHelper.GenerateMD5FromUrl("https://www.goodfon.com/cats/wallpaper-2.html")
        Dim hash3 As String = UrlHelper.GenerateMD5FromUrl("https://www.goodfon.com/Cats/wallpaper-1.html")

        Assert.AreNotEqual(hash1, hash2)
        Assert.AreNotEqual(hash1, hash3)
    End Sub

    ''' <summary>
    ''' Verifies that a null URL throws <see cref="ArgumentNullException"/>.
    ''' </summary>
    <TestMethod>
    Public Sub GenerateMD5FromUrl_Nothing_ThrowsArgumentNullException()

        Assert.ThrowsExactly(Of ArgumentNullException)(Sub() UrlHelper.GenerateMD5FromUrl(Nothing))
    End Sub

#End Region

#Region " ContainsAnyKeyword "

    ''' <summary>
    ''' Verifies that a keyword is matched case-insensitively and returned through the ByRef parameter.
    ''' </summary>
    <TestMethod>
    Public Sub ContainsAnyKeyword_KeywordPresentWithDifferentCasing_ReturnsTrueAndMatchedKeyword()

        Dim keywords As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase) From {"-food-", "/AI-ART/"}
        Dim matched As String = Nothing

        Dim result As Boolean = UrlHelper.ContainsAnyKeyword("https://www.goodfon.com/ai-art/wallpaper-1.html", keywords, matched)

        Assert.IsTrue(result)
        Assert.AreEqual("/AI-ART/", matched)
    End Sub

    ''' <summary>
    ''' Verifies that the first matching keyword (in enumeration order) is returned.
    ''' </summary>
    <TestMethod>
    Public Sub ContainsAnyKeyword_SeveralKeywordsMatch_ReturnsFirstInEnumerationOrder()

        Dim keywords As New List(Of String) From {"-new-year-", "wallpaper", "cats"}
        Dim matched As String = Nothing

        Dim result As Boolean = UrlHelper.ContainsAnyKeyword("https://www.goodfon.com/cats/wallpaper-new-year-tree.html", keywords, matched)

        Assert.IsTrue(result)
        Assert.AreEqual("-new-year-", matched)
    End Sub

    ''' <summary>
    ''' Verifies that no match returns <see langword="False"/> and leaves the ByRef parameter unchanged.
    ''' </summary>
    <TestMethod>
    Public Sub ContainsAnyKeyword_NoMatch_ReturnsFalseAndLeavesMatchedUnchanged()

        Dim keywords As New HashSet(Of String) From {"-food-"}
        Dim matched As String = "unchanged"

        Dim result As Boolean = UrlHelper.ContainsAnyKeyword("https://www.goodfon.com/cats/wallpaper-1.html", keywords, matched)

        Assert.IsFalse(result)
        Assert.AreEqual("unchanged", matched)
    End Sub

    ''' <summary>
    ''' Verifies that null or empty keyword collections never match.
    ''' </summary>
    <TestMethod>
    Public Sub ContainsAnyKeyword_NullOrEmptyKeywords_ReturnsFalse()

        Dim matched As String = Nothing

        Assert.IsFalse(UrlHelper.ContainsAnyKeyword("https://www.goodfon.com/x", Nothing, matched))
        Assert.IsFalse(UrlHelper.ContainsAnyKeyword("https://www.goodfon.com/x", New HashSet(Of String)(), matched))
        Assert.IsNull(matched)
    End Sub

    ''' <summary>
    ''' Verifies that null or empty keywords are ignored instead of matching every URL.
    ''' </summary>
    <TestMethod>
    Public Sub ContainsAnyKeyword_EmptyOrNullKeywordItems_AreIgnored()

        Dim keywords As New List(Of String) From {String.Empty, Nothing}
        Dim matched As String = Nothing

        Assert.IsFalse(UrlHelper.ContainsAnyKeyword("https://www.goodfon.com/x", keywords, matched))
        Assert.IsNull(matched)
    End Sub

    ''' <summary>
    ''' Verifies that a null URL throws <see cref="ArgumentNullException"/>.
    ''' </summary>
    <TestMethod>
    Public Sub ContainsAnyKeyword_NullUrl_ThrowsArgumentNullException()

        Dim matched As String = Nothing
        Assert.ThrowsExactly(Of ArgumentNullException)(
            Sub() UrlHelper.ContainsAnyKeyword(Nothing, New List(Of String) From {"a"}, matched))
    End Sub

#End Region

#Region " ToAbsoluteUrl "

    ''' <summary>
    ''' Verifies that relative hrefs (with or without leading slashes) are combined with the base URL.
    ''' </summary>
    <TestMethod>
    Public Sub ToAbsoluteUrl_RelativeHref_CombinesWithBaseUrl()

        Assert.AreEqual("https://www.goodfon.com/cats/wallpaper-1.html", UrlHelper.ToAbsoluteUrl("/cats/wallpaper-1.html"))
        Assert.AreEqual("https://www.goodfon.com/cats/wallpaper-1.html", UrlHelper.ToAbsoluteUrl("cats/wallpaper-1.html"))
        Assert.AreEqual("https://www.goodfon.com/cats/wallpaper-1.html", UrlHelper.ToAbsoluteUrl("///cats/wallpaper-1.html"))
        Assert.AreEqual("https://www.goodfon.com/", UrlHelper.ToAbsoluteUrl(String.Empty))
    End Sub

    ''' <summary>
    ''' Verifies that absolute hrefs are returned unchanged, regardless of the scheme casing.
    ''' </summary>
    <TestMethod>
    Public Sub ToAbsoluteUrl_AbsoluteHref_ReturnsUnchanged()

        Assert.AreEqual("https://img.goodfon.com/original/1.jpg", UrlHelper.ToAbsoluteUrl("https://img.goodfon.com/original/1.jpg"))
        Assert.AreEqual("http://example.org/a", UrlHelper.ToAbsoluteUrl("http://example.org/a"))
        Assert.AreEqual("HTTPS://EXAMPLE.ORG/A", UrlHelper.ToAbsoluteUrl("HTTPS://EXAMPLE.ORG/A"))
    End Sub

    ''' <summary>
    ''' Verifies that a null href throws <see cref="ArgumentNullException"/>.
    ''' </summary>
    <TestMethod>
    Public Sub ToAbsoluteUrl_Nothing_ThrowsArgumentNullException()

        Assert.ThrowsExactly(Of ArgumentNullException)(Sub() UrlHelper.ToAbsoluteUrl(Nothing))
    End Sub

#End Region

#Region " ToDisplayUrl "

    ''' <summary>
    ''' Verifies that the "https://www." and "http://www." prefixes are removed.
    ''' </summary>
    <TestMethod>
    Public Sub ToDisplayUrl_WwwPrefixedUrls_RemovesPrefix()

        Assert.AreEqual("goodfon.com/tags/cats/", UrlHelper.ToDisplayUrl("https://www.goodfon.com/tags/cats/"))
        Assert.AreEqual("goodfon.com/tags/cats/", UrlHelper.ToDisplayUrl("http://www.goodfon.com/tags/cats/"))
    End Sub

    ''' <summary>
    ''' Verifies that URLs without a "www." prefix are returned unchanged.
    ''' </summary>
    <TestMethod>
    Public Sub ToDisplayUrl_UrlWithoutWww_ReturnsUnchanged()

        Assert.AreEqual("https://goodfon.com/tags/cats/", UrlHelper.ToDisplayUrl("https://goodfon.com/tags/cats/"))
    End Sub

    ''' <summary>
    ''' Verifies that a null URL throws <see cref="ArgumentNullException"/>.
    ''' </summary>
    <TestMethod>
    Public Sub ToDisplayUrl_Nothing_ThrowsArgumentNullException()

        Assert.ThrowsExactly(Of ArgumentNullException)(Sub() UrlHelper.ToDisplayUrl(Nothing))
    End Sub

#End Region

#Region " GetUrlKind "

    ''' <summary>
    ''' Verifies the identification of every supported URL kind.
    ''' </summary>
    <TestMethod>
    Public Sub GetUrlKind_SupportedUrls_ReturnsExpectedKind()

        Assert.AreEqual(GoodfonUrlKind.Tag, UrlHelper.GetUrlKind("https://www.goodfon.com/tags/cats/"))
        Assert.AreEqual(GoodfonUrlKind.UserProfile, UrlHelper.GetUrlKind("https://www.goodfon.com/user/bob"))
        Assert.AreEqual(GoodfonUrlKind.SearchQuery, UrlHelper.GetUrlKind("https://www.goodfon.com/search/?q=cats"))
    End Sub

    ''' <summary>
    ''' Verifies that the URL kind is identified case-insensitively.
    ''' </summary>
    <TestMethod>
    Public Sub GetUrlKind_UpperCaseUrl_IsCaseInsensitive()

        Assert.AreEqual(GoodfonUrlKind.Tag, UrlHelper.GetUrlKind("HTTPS://WWW.GOODFON.COM/TAGS/CATS/"))
        Assert.AreEqual(GoodfonUrlKind.UserProfile, UrlHelper.GetUrlKind("HTTPS://WWW.GOODFON.COM/USER/BOB"))
        Assert.AreEqual(GoodfonUrlKind.SearchQuery, UrlHelper.GetUrlKind("HTTPS://WWW.GOODFON.COM/SEARCH/?Q=CATS"))
    End Sub

    ''' <summary>
    ''' Verifies that the tag kind takes precedence over the other kinds.
    ''' </summary>
    <TestMethod>
    Public Sub GetUrlKind_AmbiguousUrl_TagTakesPrecedence()

        Assert.AreEqual(GoodfonUrlKind.Tag, UrlHelper.GetUrlKind("https://www.goodfon.com/tags/user/?q=x"))
        Assert.AreEqual(GoodfonUrlKind.UserProfile, UrlHelper.GetUrlKind("https://www.goodfon.com/user/bob/?q=x"))
    End Sub

    ''' <summary>
    ''' Verifies that unsupported URLs return <see cref="GoodfonUrlKind.Unknown"/>.
    ''' </summary>
    <TestMethod>
    Public Sub GetUrlKind_UnsupportedUrls_ReturnsUnknown()

        Assert.AreEqual(GoodfonUrlKind.Unknown, UrlHelper.GetUrlKind("https://www.goodfon.com/cats/"))
        Assert.AreEqual(GoodfonUrlKind.Unknown, UrlHelper.GetUrlKind(String.Empty))
        ' The markers must not be at the very start of the string.
        Assert.AreEqual(GoodfonUrlKind.Unknown, UrlHelper.GetUrlKind("/tags/cats/"))
    End Sub

    ''' <summary>
    ''' Verifies that a null URL throws <see cref="ArgumentNullException"/>.
    ''' </summary>
    <TestMethod>
    Public Sub GetUrlKind_Nothing_ThrowsArgumentNullException()

        Assert.ThrowsExactly(Of ArgumentNullException)(Sub() UrlHelper.GetUrlKind(Nothing))
    End Sub

#End Region

#Region " BuildPageUrl "

    ''' <summary>
    ''' Verifies that the first page URL is always the base URL, for every supported kind.
    ''' </summary>
    <TestMethod>
    Public Sub BuildPageUrl_FirstPage_ReturnsBaseUrl()

        Assert.AreEqual("https://www.goodfon.com/tags/cats/", UrlHelper.BuildPageUrl("https://www.goodfon.com/tags/cats/", 1))
        Assert.AreEqual("https://www.goodfon.com/user/bob", UrlHelper.BuildPageUrl("https://www.goodfon.com/user/bob", 1))
        Assert.AreEqual("https://www.goodfon.com/search/?q=cats", UrlHelper.BuildPageUrl("https://www.goodfon.com/search/?q=cats", 1))
    End Sub

    ''' <summary>
    ''' Verifies the tag pages URL format ("index-N.html").
    ''' </summary>
    <TestMethod>
    Public Sub BuildPageUrl_TagUrl_AppendsIndexPage()

        Assert.AreEqual("https://www.goodfon.com/tags/cats/index-2.html", UrlHelper.BuildPageUrl("https://www.goodfon.com/tags/cats/", 2))
        Assert.AreEqual("https://www.goodfon.com/tags/cats-3/index-15.html", UrlHelper.BuildPageUrl("https://www.goodfon.com/tags/cats-3/", 15))
    End Sub

    ''' <summary>
    ''' Verifies the user profile pages URL format ("/?&amp;page=N").
    ''' </summary>
    <TestMethod>
    Public Sub BuildPageUrl_UserProfileUrl_AppendsPageQuery()

        Assert.AreEqual("https://www.goodfon.com/user/bob/?&page=4", UrlHelper.BuildPageUrl("https://www.goodfon.com/user/bob", 4))
    End Sub

    ''' <summary>
    ''' Verifies the search query pages URL format ("&amp;page=N").
    ''' </summary>
    <TestMethod>
    Public Sub BuildPageUrl_SearchQueryUrl_AppendsPageParameter()

        Assert.AreEqual("https://www.goodfon.com/search/?q=red+car&page=7", UrlHelper.BuildPageUrl("https://www.goodfon.com/search/?q=red+car", 7))
    End Sub

    ''' <summary>
    ''' Verifies that page numbers lower than 1 throw <see cref="ArgumentOutOfRangeException"/>.
    ''' </summary>
    <TestMethod>
    Public Sub BuildPageUrl_PageLowerThanOne_ThrowsArgumentOutOfRangeException()

        Assert.ThrowsExactly(Of ArgumentOutOfRangeException)(Sub() UrlHelper.BuildPageUrl("https://www.goodfon.com/tags/cats/", 0))
        Assert.ThrowsExactly(Of ArgumentOutOfRangeException)(Sub() UrlHelper.BuildPageUrl("https://www.goodfon.com/tags/cats/", -1))
    End Sub

    ''' <summary>
    ''' Verifies that an unknown URL kind throws <see cref="InvalidOperationException"/>, even for the first page.
    ''' </summary>
    <TestMethod>
    Public Sub BuildPageUrl_UnknownUrlKind_ThrowsInvalidOperationException()

        Assert.ThrowsExactly(Of InvalidOperationException)(Sub() UrlHelper.BuildPageUrl("https://www.goodfon.com/cats/", 1))
        Assert.ThrowsExactly(Of InvalidOperationException)(Sub() UrlHelper.BuildPageUrl("https://www.goodfon.com/cats/", 2))
    End Sub

    ''' <summary>
    ''' Verifies that a null base URL throws <see cref="ArgumentNullException"/>.
    ''' </summary>
    <TestMethod>
    Public Sub BuildPageUrl_NullBaseUrl_ThrowsArgumentNullException()

        Assert.ThrowsExactly(Of ArgumentNullException)(Sub() UrlHelper.BuildPageUrl(Nothing, 2))
    End Sub

#End Region

#Region " BuildUserProfileUrl / BuildTagUrl / BuildSearchUrl "

    ''' <summary>
    ''' Verifies the user profile URL format, replacing white-spaces with "+" characters.
    ''' </summary>
    <TestMethod>
    Public Sub BuildUserProfileUrl_TermWithSpaces_ReplacesSpacesWithPlus()

        Assert.AreEqual("https://www.goodfon.com/user/bob", UrlHelper.BuildUserProfileUrl("bob"))
        Assert.AreEqual("https://www.goodfon.com/user/pretty+girls", UrlHelper.BuildUserProfileUrl("pretty girls"))
    End Sub

    ''' <summary>
    ''' Verifies the plain tag URL format (tag index zero).
    ''' </summary>
    <TestMethod>
    Public Sub BuildTagUrl_IndexZero_ReturnsPlainTagUrl()

        Assert.AreEqual("https://www.goodfon.com/tags/cats/", UrlHelper.BuildTagUrl("cats", 0))
        Assert.AreEqual("https://www.goodfon.com/tags/red+car/", UrlHelper.BuildTagUrl("red car", 0))
    End Sub

    ''' <summary>
    ''' Verifies the numbered tag variant URL format (positive tag index).
    ''' </summary>
    <TestMethod>
    Public Sub BuildTagUrl_PositiveIndex_ReturnsNumberedTagUrl()

        Assert.AreEqual("https://www.goodfon.com/tags/cats-1/", UrlHelper.BuildTagUrl("cats", 1))
        Assert.AreEqual("https://www.goodfon.com/tags/red+car-3/", UrlHelper.BuildTagUrl("red car", 3))
    End Sub

    ''' <summary>
    ''' Verifies that a negative tag index throws <see cref="ArgumentOutOfRangeException"/>.
    ''' </summary>
    <TestMethod>
    Public Sub BuildTagUrl_NegativeIndex_ThrowsArgumentOutOfRangeException()

        Assert.ThrowsExactly(Of ArgumentOutOfRangeException)(Sub() UrlHelper.BuildTagUrl("cats", -1))
    End Sub

    ''' <summary>
    ''' Verifies the search query URL format.
    ''' </summary>
    <TestMethod>
    Public Sub BuildSearchUrl_TermWithSpaces_ReturnsSearchQueryUrl()

        Assert.AreEqual("https://www.goodfon.com/search/?q=cats", UrlHelper.BuildSearchUrl("cats"))
        Assert.AreEqual("https://www.goodfon.com/search/?q=red+car", UrlHelper.BuildSearchUrl("red car"))
    End Sub

    ''' <summary>
    ''' Verifies that null, empty or white-space search terms throw <see cref="ArgumentException"/>.
    ''' </summary>
    <TestMethod>
    Public Sub BuildUrls_BlankSearchTerm_ThrowsArgumentException()

        For Each term As String In New String() {Nothing, String.Empty, "   "}
            Dim capturedTerm As String = term
            Assert.ThrowsExactly(Of ArgumentException)(Sub() UrlHelper.BuildUserProfileUrl(capturedTerm))
            Assert.ThrowsExactly(Of ArgumentException)(Sub() UrlHelper.BuildTagUrl(capturedTerm, 0))
            Assert.ThrowsExactly(Of ArgumentException)(Sub() UrlHelper.BuildSearchUrl(capturedTerm))
        Next term
    End Sub

#End Region

#Region " Integration "

    ''' <summary>
    ''' Verifies that the URLs built for a search term are identified as the expected kind,
    ''' and that their page URLs are built as expected.
    ''' </summary>
    <TestMethod>
    Public Sub BuiltUrls_AreRecognizedAndPaginatedConsistently()

        Dim userUrl As String = UrlHelper.BuildUserProfileUrl("bob")
        Dim tagUrl As String = UrlHelper.BuildTagUrl("cats", 2)
        Dim searchUrl As String = UrlHelper.BuildSearchUrl("red car")

        Assert.AreEqual(GoodfonUrlKind.UserProfile, UrlHelper.GetUrlKind(userUrl))
        Assert.AreEqual(GoodfonUrlKind.Tag, UrlHelper.GetUrlKind(tagUrl))
        Assert.AreEqual(GoodfonUrlKind.SearchQuery, UrlHelper.GetUrlKind(searchUrl))

        Assert.AreEqual("https://www.goodfon.com/user/bob/?&page=2", UrlHelper.BuildPageUrl(userUrl, 2))
        Assert.AreEqual("https://www.goodfon.com/tags/cats-2/index-2.html", UrlHelper.BuildPageUrl(tagUrl, 2))
        Assert.AreEqual("https://www.goodfon.com/search/?q=red+car&page=2", UrlHelper.BuildPageUrl(searchUrl, 2))
    End Sub

#End Region

End Class

#End Region
