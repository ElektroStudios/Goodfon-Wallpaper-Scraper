#Region " Option Statements "

Option Explicit On
Option Strict On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Collections.Generic
Imports System.Linq
Imports System.Text

Imports HtmlDocument = HtmlAgilityPack.HtmlDocument

#End Region

#Region " GoodfonHtmlParserTests "

''' <summary>
''' Unit tests for the <see cref="GoodfonHtmlParser"/> module.
''' </summary>
<TestClass>
Public NotInheritable Class GoodfonHtmlParserTests

#Region " Test Helpers "

    ''' <summary>
    ''' Builds a minimal GoodFon.com listing page HTML containing one wallpaper item per specified href.
    ''' </summary>
    '''
    ''' <param name="hrefs">
    ''' The href of each wallpaper item link. A <see langword="Nothing"/> value produces a link without href attribute.
    ''' </param>
    '''
    ''' <returns>
    ''' The HTML source.
    ''' </returns>
    Private Shared Function BuildListingHtml(ParamArray hrefs As String()) As String

        Dim sb As New StringBuilder()
        sb.AppendLine("<html><body><div class=""wallpapers"">")
        For Each href As String In hrefs
            sb.AppendLine("  <div class=""wallpapers__item wallpapers__item--wall"">")
            If href Is Nothing Then
                sb.AppendLine("    <div><a class=""wallpapers__item__wall"">no href</a></div>")
            Else
                sb.AppendLine($"    <div><a class=""wallpapers__item__wall"" href=""{href}"">item</a></div>")
            End If
            sb.AppendLine("  </div>")
        Next href
        sb.AppendLine("</div></body></html>")
        Return sb.ToString()
    End Function

    ''' <summary>
    ''' Builds a full listing page document with the specified number of valid wallpaper items.
    ''' </summary>
    '''
    ''' <param name="count">
    ''' The number of valid wallpaper items.
    ''' </param>
    '''
    ''' <returns>
    ''' The listing page document.
    ''' </returns>
    Private Shared Function BuildListingDocument(count As Integer) As HtmlDocument

        Dim hrefs As String() = Enumerable.Range(1, count).Select(Function(i As Integer) $"/cats/wallpaper-{i}.html").ToArray()
        Return GoodfonHtmlParser.LoadDocument(GoodfonHtmlParserTests.BuildListingHtml(hrefs))
    End Function

#End Region

#Region " LoadDocument "

    ''' <summary>
    ''' Verifies that the HTML is loaded into a queryable document.
    ''' </summary>
    <TestMethod>
    Public Sub LoadDocument_ValidHtml_ReturnsQueryableDocument()

        Dim doc As HtmlDocument = GoodfonHtmlParser.LoadDocument("<html><body><p id='x'>hello</p></body></html>")

        Assert.IsNotNull(doc)
        Assert.AreEqual("hello", doc.GetElementbyId("x").InnerText)
    End Sub

    ''' <summary>
    ''' Verifies that an empty HTML source produces an empty but valid document.
    ''' </summary>
    <TestMethod>
    Public Sub LoadDocument_EmptyHtml_ReturnsEmptyDocument()

        Dim doc As HtmlDocument = GoodfonHtmlParser.LoadDocument(String.Empty)

        Assert.IsNotNull(doc.DocumentNode)
        Assert.IsFalse(GoodfonHtmlParser.HasWallpaperNodes(doc))
    End Sub

    ''' <summary>
    ''' Verifies that a null HTML source throws <see cref="ArgumentNullException"/>.
    ''' </summary>
    <TestMethod>
    Public Sub LoadDocument_Nothing_ThrowsArgumentNullException()

        Assert.ThrowsExactly(Of ArgumentNullException)(Sub() GoodfonHtmlParser.LoadDocument(Nothing))
    End Sub

#End Region

#Region " IsValidWallpaperHref "

    ''' <summary>
    ''' Verifies that wallpaper page hrefs (absolute or relative) are valid.
    ''' </summary>
    <TestMethod>
    Public Sub IsValidWallpaperHref_WallpaperPageHrefs_ReturnsTrue()

        Assert.IsTrue(GoodfonHtmlParser.IsValidWallpaperHref("https://www.goodfon.com/cats/wallpaper-1.html"))
        Assert.IsTrue(GoodfonHtmlParser.IsValidWallpaperHref("/cats/wallpaper-1.html"))
        Assert.IsTrue(GoodfonHtmlParser.IsValidWallpaperHref("#anchor"))
    End Sub

    ''' <summary>
    ''' Verifies that empty hrefs, "#" anchors and user profile links are invalid.
    ''' </summary>
    <TestMethod>
    Public Sub IsValidWallpaperHref_InvalidHrefs_ReturnsFalse()

        Assert.IsFalse(GoodfonHtmlParser.IsValidWallpaperHref(Nothing))
        Assert.IsFalse(GoodfonHtmlParser.IsValidWallpaperHref(String.Empty))
        Assert.IsFalse(GoodfonHtmlParser.IsValidWallpaperHref("   "))
        Assert.IsFalse(GoodfonHtmlParser.IsValidWallpaperHref("#"))
        Assert.IsFalse(GoodfonHtmlParser.IsValidWallpaperHref("https://www.goodfon.com/user/bob/"))
        Assert.IsFalse(GoodfonHtmlParser.IsValidWallpaperHref("https://www.goodfon.com/USER/bob/"))
    End Sub

    ''' <summary>
    ''' Regression test: relative user profile hrefs (starting with "/user/") must be excluded too.
    ''' <para></para>
    ''' Previously, the listing page filter used "IndexOf(...) &gt; 0", so a relative "/user/..." href
    ''' (index 0) was wrongly treated as a wallpaper page.
    ''' </summary>
    <TestMethod>
    Public Sub IsValidWallpaperHref_RelativeUserProfileHref_ReturnsFalse()

        Assert.IsFalse(GoodfonHtmlParser.IsValidWallpaperHref("/user/alice/"))
    End Sub

#End Region

#Region " ExtractWallpaperPageUrls / HasWallpaperNodes / CountValidWallpaperLinks "

    ''' <summary>
    ''' Verifies that only valid wallpaper links are extracted, converted to absolute URLs, in document order.
    ''' </summary>
    <TestMethod>
    Public Sub ExtractWallpaperPageUrls_MixedLinks_ReturnsOnlyValidAbsoluteUrlsInOrder()

        Dim html As String = GoodfonHtmlParserTests.BuildListingHtml(
            "https://www.goodfon.com/cats/wallpaper-1.html",
            "/cats/wallpaper-2.html",
            "#",
            String.Empty,
            Nothing,
            "https://www.goodfon.com/user/bob/",
            "/user/alice/",
            "cats/wallpaper-3.html")

        Dim urls As List(Of String) = GoodfonHtmlParser.ExtractWallpaperPageUrls(GoodfonHtmlParser.LoadDocument(html))

        CollectionAssert.AreEqual(
            New String() {
                "https://www.goodfon.com/cats/wallpaper-1.html",
                "https://www.goodfon.com/cats/wallpaper-2.html",
                "https://www.goodfon.com/cats/wallpaper-3.html"
            },
            urls)
    End Sub

    ''' <summary>
    ''' Verifies that links outside the wallpaper items container are ignored.
    ''' </summary>
    <TestMethod>
    Public Sub ExtractWallpaperPageUrls_LinksOutsideWallpaperItems_AreIgnored()

        Const html As String =
            "<html><body>" &
            "<a href='/cats/wallpaper-outside.html'>outside</a>" &
            "<div class='other'><div><a href='/cats/wallpaper-other.html'>other</a></div></div>" &
            "<div class='wallpapers__item'><div><a href='/cats/wallpaper-inside.html'>inside</a></div></div>" &
            "</body></html>"

        Dim urls As List(Of String) = GoodfonHtmlParser.ExtractWallpaperPageUrls(GoodfonHtmlParser.LoadDocument(html))

        Assert.AreEqual(1, urls.Count)
        Assert.AreEqual("https://www.goodfon.com/cats/wallpaper-inside.html", urls(0))
    End Sub

    ''' <summary>
    ''' Verifies that a page without wallpaper items returns an empty list (never <see langword="Nothing"/>).
    ''' </summary>
    <TestMethod>
    Public Sub ExtractWallpaperPageUrls_NoWallpaperItems_ReturnsEmptyList()

        Dim urls As List(Of String) = GoodfonHtmlParser.ExtractWallpaperPageUrls(GoodfonHtmlParser.LoadDocument("<html><body></body></html>"))

        Assert.IsNotNull(urls)
        Assert.AreEqual(0, urls.Count)
    End Sub

    ''' <summary>
    ''' Verifies the distinction between "no wallpaper nodes" and "wallpaper nodes without valid links".
    ''' </summary>
    <TestMethod>
    Public Sub HasWallpaperNodes_DistinguishesMissingNodesFromInvalidLinks()

        Dim noNodesDoc As HtmlDocument = GoodfonHtmlParser.LoadDocument("<html><body><p>empty</p></body></html>")
        Dim invalidLinksDoc As HtmlDocument = GoodfonHtmlParser.LoadDocument(GoodfonHtmlParserTests.BuildListingHtml("#", "/user/bob/"))

        Assert.IsFalse(GoodfonHtmlParser.HasWallpaperNodes(noNodesDoc))
        Assert.IsTrue(GoodfonHtmlParser.HasWallpaperNodes(invalidLinksDoc))
        Assert.AreEqual(0, GoodfonHtmlParser.ExtractWallpaperPageUrls(invalidLinksDoc).Count)
    End Sub

    ''' <summary>
    ''' Verifies that the count of valid links matches the number of extracted URLs.
    ''' </summary>
    <TestMethod>
    Public Sub CountValidWallpaperLinks_MixedLinks_MatchesExtractedUrlsCount()

        Dim doc As HtmlDocument = GoodfonHtmlParser.LoadDocument(
            GoodfonHtmlParserTests.BuildListingHtml("/cats/1.html", "#", "/user/bob/", "/cats/2.html"))

        Assert.AreEqual(2, GoodfonHtmlParser.CountValidWallpaperLinks(doc))
        Assert.AreEqual(AppGlobals.MaxWallpaperItemsPerPage,
                        GoodfonHtmlParser.CountValidWallpaperLinks(GoodfonHtmlParserTests.BuildListingDocument(AppGlobals.MaxWallpaperItemsPerPage)))
    End Sub

    ''' <summary>
    ''' Verifies that null documents throw <see cref="ArgumentNullException"/>.
    ''' </summary>
    <TestMethod>
    Public Sub ListingMethods_NullDocument_ThrowArgumentNullException()

        Assert.ThrowsExactly(Of ArgumentNullException)(Sub() GoodfonHtmlParser.ExtractWallpaperPageUrls(Nothing))
        Assert.ThrowsExactly(Of ArgumentNullException)(Sub() GoodfonHtmlParser.HasWallpaperNodes(Nothing))
        Assert.ThrowsExactly(Of ArgumentNullException)(Sub() GoodfonHtmlParser.CountValidWallpaperLinks(Nothing))
    End Sub

#End Region

#Region " FindLastPageNumber "

    ''' <summary>
    ''' Verifies that the highest "index-N.html" page number is returned, regardless of the link order.
    ''' </summary>
    <TestMethod>
    Public Sub FindLastPageNumber_IndexLinks_ReturnsHighestPageNumber()

        Const html As String =
            "<div class='pagination'>" &
            "<a href='/tags/cats/index-2.html'>2</a>" &
            "<a href='/tags/cats/index-15.html'>15</a>" &
            "<a href='/tags/cats/index-7.html'>7</a>" &
            "</div>"

        Assert.AreEqual(15, GoodfonHtmlParser.FindLastPageNumber(GoodfonHtmlParser.LoadDocument(html)))
    End Sub

    ''' <summary>
    ''' Verifies that the "Last" link of the search results paginator is recognized.
    ''' </summary>
    <TestMethod>
    Public Sub FindLastPageNumber_SearchPaginatorLastLink_ReturnsPageNumber()

        Const html As String =
            "<div class='paginator'>" &
            "<a href='/search/?q=cats&amp;page=2'>2</a>" &
            "<a href='/search/?q=cats&amp;page=9'>Last</a>" &
            "</div>"

        Assert.AreEqual(9, GoodfonHtmlParser.FindLastPageNumber(GoodfonHtmlParser.LoadDocument(html)))
    End Sub

    ''' <summary>
    ''' Verifies that the paginator is only evaluated through its "Last" link.
    ''' </summary>
    <TestMethod>
    Public Sub FindLastPageNumber_PaginatorWithoutLastLink_ReturnsNothing()

        Const html As String =
            "<div class='paginator'>" &
            "<a href='/search/?q=cats&amp;page=2'>2</a>" &
            "<a href='/search/?q=cats&amp;page=3'>Next</a>" &
            "</div>"

        Assert.IsNull(GoodfonHtmlParser.FindLastPageNumber(GoodfonHtmlParser.LoadDocument(html)))
    End Sub

    ''' <summary>
    ''' Verifies that, when both pagination formats are present, the highest page number wins.
    ''' </summary>
    <TestMethod>
    Public Sub FindLastPageNumber_BothFormats_ReturnsMaximum()

        Const htmlIndexWins As String =
            "<a href='/tags/cats/index-20.html'>20</a>" &
            "<div class='paginator'><a href='/x/?page=5'>Last</a></div>"

        Const htmlPaginatorWins As String =
            "<a href='/tags/cats/index-3.html'>3</a>" &
            "<div class='paginator'><a href='/x/?page=12'>Last</a></div>"

        Assert.AreEqual(20, GoodfonHtmlParser.FindLastPageNumber(GoodfonHtmlParser.LoadDocument(htmlIndexWins)))
        Assert.AreEqual(12, GoodfonHtmlParser.FindLastPageNumber(GoodfonHtmlParser.LoadDocument(htmlPaginatorWins)))
    End Sub

    ''' <summary>
    ''' Verifies that pages without pagination return <see langword="Nothing"/>.
    ''' </summary>
    <TestMethod>
    Public Sub FindLastPageNumber_NoPagination_ReturnsNothing()

        Dim doc As HtmlDocument = GoodfonHtmlParserTests.BuildListingDocument(5)

        Assert.IsNull(GoodfonHtmlParser.FindLastPageNumber(doc))
    End Sub

    ''' <summary>
    ''' Verifies that malformed or overflowing page numbers are ignored.
    ''' </summary>
    <TestMethod>
    Public Sub FindLastPageNumber_MalformedOrOverflowingNumbers_AreIgnored()

        Const html As String =
            "<a href='/tags/cats/index-abc.html'>abc</a>" &
            "<a href='/tags/cats/index-99999999999.html'>overflow</a>" &
            "<div class='paginator'><a href='/x/?page=99999999999'>Last</a></div>"

        Assert.IsNull(GoodfonHtmlParser.FindLastPageNumber(GoodfonHtmlParser.LoadDocument(html)))
    End Sub

    ''' <summary>
    ''' Verifies that the "index-N.html" pattern is matched case-insensitively.
    ''' </summary>
    <TestMethod>
    Public Sub FindLastPageNumber_IndexLinkMatching_IsCaseInsensitiveOnRegexOnly()

        ' The XPath filter is case-sensitive ("index-" and ".html"), so the link must use lower-case markers,
        ' but the regex itself is case-insensitive and tolerates the surrounding path casing.
        Const html As String = "<a href='/TAGS/CATS/index-4.html'>4</a>"

        Assert.AreEqual(4, GoodfonHtmlParser.FindLastPageNumber(GoodfonHtmlParser.LoadDocument(html)))
    End Sub

    ''' <summary>
    ''' Verifies that a null document throws <see cref="ArgumentNullException"/>.
    ''' </summary>
    <TestMethod>
    Public Sub FindLastPageNumber_Nothing_ThrowsArgumentNullException()

        Assert.ThrowsExactly(Of ArgumentNullException)(Sub() GoodfonHtmlParser.FindLastPageNumber(Nothing))
    End Sub

#End Region

#Region " CalculateTotalWallpapers "

    ''' <summary>
    ''' Verifies the total wallpapers calculation, assuming full pages except the last one.
    ''' </summary>
    <TestMethod>
    Public Sub CalculateTotalWallpapers_ValidValues_ReturnsExpectedTotal()

        Assert.AreEqual(10, GoodfonHtmlParser.CalculateTotalWallpapers(1, 10))
        Assert.AreEqual(0, GoodfonHtmlParser.CalculateTotalWallpapers(1, 0))
        Assert.AreEqual(24, GoodfonHtmlParser.CalculateTotalWallpapers(2, 0))
        Assert.AreEqual(53, GoodfonHtmlParser.CalculateTotalWallpapers(3, 5))
        Assert.AreEqual(240, GoodfonHtmlParser.CalculateTotalWallpapers(10, 24))
    End Sub

    ''' <summary>
    ''' Verifies that invalid arguments throw <see cref="ArgumentOutOfRangeException"/>.
    ''' </summary>
    <TestMethod>
    Public Sub CalculateTotalWallpapers_InvalidValues_ThrowsArgumentOutOfRangeException()

        Assert.ThrowsExactly(Of ArgumentOutOfRangeException)(Sub() GoodfonHtmlParser.CalculateTotalWallpapers(0, 5))
        Assert.ThrowsExactly(Of ArgumentOutOfRangeException)(Sub() GoodfonHtmlParser.CalculateTotalWallpapers(1, -1))
    End Sub

#End Region

#Region " TryFindDownloadButtonHref "

    ''' <summary>
    ''' Verifies that the original-resolution download button href is found.
    ''' </summary>
    <TestMethod>
    Public Sub TryFindDownloadButtonHref_ButtonPresent_ReturnsTrueAndHref()

        Const html As String =
            "<div class='wallpaper__download'>" &
            "<a class='wallpaper__download__rbut js-size' href='/download/cats-wallpaper-1/3840x2160/'>Original</a>" &
            "</div>"

        Dim href As String = Nothing
        Dim found As Boolean = GoodfonHtmlParser.TryFindDownloadButtonHref(GoodfonHtmlParser.LoadDocument(html), href)

        Assert.IsTrue(found)
        Assert.AreEqual("/download/cats-wallpaper-1/3840x2160/", href)
    End Sub

    ''' <summary>
    ''' Verifies that a download button without href is found, returning an empty href.
    ''' </summary>
    <TestMethod>
    Public Sub TryFindDownloadButtonHref_ButtonWithoutHref_ReturnsTrueAndEmptyHref()

        Const html As String = "<a class='wallpaper__download__rbut js-size'>Original</a>"

        Dim href As String = Nothing
        Dim found As Boolean = GoodfonHtmlParser.TryFindDownloadButtonHref(GoodfonHtmlParser.LoadDocument(html), href)

        Assert.IsTrue(found)
        Assert.AreEqual(String.Empty, href)
    End Sub

    ''' <summary>
    ''' Verifies that the button requires both CSS classes (a disabled button lacks the "js-size" class).
    ''' </summary>
    <TestMethod>
    Public Sub TryFindDownloadButtonHref_ButtonMissingOrDisabled_ReturnsFalseAndNothing()

        Const disabledHtml As String = "<a class='wallpaper__download__rbut' href='/download/x/'>Original</a>"
        Const missingHtml As String = "<html><body><p>No button</p></body></html>"

        Dim href As String = "unchanged"
        Assert.IsFalse(GoodfonHtmlParser.TryFindDownloadButtonHref(GoodfonHtmlParser.LoadDocument(disabledHtml), href))
        Assert.IsNull(href)

        href = "unchanged"
        Assert.IsFalse(GoodfonHtmlParser.TryFindDownloadButtonHref(GoodfonHtmlParser.LoadDocument(missingHtml), href))
        Assert.IsNull(href)
    End Sub

    ''' <summary>
    ''' Verifies that the first matching download button is used when several are present.
    ''' </summary>
    <TestMethod>
    Public Sub TryFindDownloadButtonHref_SeveralButtons_ReturnsFirst()

        Const html As String =
            "<a class='wallpaper__download__rbut js-size' href='/download/first/'>1</a>" &
            "<a class='wallpaper__download__rbut js-size' href='/download/second/'>2</a>"

        Dim href As String = Nothing
        GoodfonHtmlParser.TryFindDownloadButtonHref(GoodfonHtmlParser.LoadDocument(html), href)

        Assert.AreEqual("/download/first/", href)
    End Sub

    ''' <summary>
    ''' Verifies that a null document throws <see cref="ArgumentNullException"/>.
    ''' </summary>
    <TestMethod>
    Public Sub TryFindDownloadButtonHref_Nothing_ThrowsArgumentNullException()

        Dim href As String = Nothing
        Assert.ThrowsExactly(Of ArgumentNullException)(Sub() GoodfonHtmlParser.TryFindDownloadButtonHref(Nothing, href))
    End Sub

#End Region

#Region " IsDailyLimitReached "

    ''' <summary>
    ''' Verifies that the daily limit message is detected case-insensitively.
    ''' </summary>
    <TestMethod>
    Public Sub IsDailyLimitReached_LimitMessagePresent_ReturnsTrue()

        Dim html As String = $"<div class='alert'><p>{GoodfonHtmlParser.DailyLimitReachedText}. Try again tomorrow.</p></div>"
        Dim upperHtml As String = $"<p>{GoodfonHtmlParser.DailyLimitReachedText.ToUpperInvariant()}</p>"

        Assert.IsTrue(GoodfonHtmlParser.IsDailyLimitReached(GoodfonHtmlParser.LoadDocument(html)))
        Assert.IsTrue(GoodfonHtmlParser.IsDailyLimitReached(GoodfonHtmlParser.LoadDocument(upperHtml)))
    End Sub

    ''' <summary>
    ''' Verifies that a regular download page is not considered as limited.
    ''' </summary>
    <TestMethod>
    Public Sub IsDailyLimitReached_RegularPage_ReturnsFalse()

        Const html As String = "<a class='wallpaper__download__link'><img src='https://img.goodfon.com/original/1.jpg'></a>"

        Assert.IsFalse(GoodfonHtmlParser.IsDailyLimitReached(GoodfonHtmlParser.LoadDocument(html)))
    End Sub

    ''' <summary>
    ''' Verifies that a null document throws <see cref="ArgumentNullException"/>.
    ''' </summary>
    <TestMethod>
    Public Sub IsDailyLimitReached_Nothing_ThrowsArgumentNullException()

        Assert.ThrowsExactly(Of ArgumentNullException)(Sub() GoodfonHtmlParser.IsDailyLimitReached(Nothing))
    End Sub

#End Region

#Region " TryFindImageSource "

    ''' <summary>
    ''' Verifies that the image inside the download link has the highest preference.
    ''' </summary>
    <TestMethod>
    Public Sub TryFindImageSource_DownloadLinkImage_HasHighestPreference()

        Const html As String =
            "<img src='/logo.png'>" &
            "<img class='wallpaper' src='/preview.jpg'>" &
            "<a class='wallpaper__download__link' href='#'><img src='https://img.goodfon.com/original/full.jpg'></a>"

        Dim src As String = Nothing
        Assert.IsTrue(GoodfonHtmlParser.TryFindImageSource(GoodfonHtmlParser.LoadDocument(html), src))
        Assert.AreEqual("https://img.goodfon.com/original/full.jpg", src)
    End Sub

    ''' <summary>
    ''' Verifies that an image with a "wallpaper" class is used when there is no download link image.
    ''' </summary>
    <TestMethod>
    Public Sub TryFindImageSource_WallpaperClassImage_IsSecondPreference()

        Const html As String =
            "<img src='/logo.png'>" &
            "<img class='wallpaper__img' src='/full.jpg'>"

        Dim src As String = Nothing
        Assert.IsTrue(GoodfonHtmlParser.TryFindImageSource(GoodfonHtmlParser.LoadDocument(html), src))
        Assert.AreEqual("/full.jpg", src)
    End Sub

    ''' <summary>
    ''' Verifies that the first image of the page is used as last resort.
    ''' </summary>
    <TestMethod>
    Public Sub TryFindImageSource_AnyImage_IsLastResort()

        Const html As String = "<img src='/first.png'><img src='/second.png'>"

        Dim src As String = Nothing
        Assert.IsTrue(GoodfonHtmlParser.TryFindImageSource(GoodfonHtmlParser.LoadDocument(html), src))
        Assert.AreEqual("/first.png", src)
    End Sub

    ''' <summary>
    ''' Verifies that an image node without src returns <see langword="True"/> and an empty src.
    ''' </summary>
    <TestMethod>
    Public Sub TryFindImageSource_ImageWithoutSrc_ReturnsTrueAndEmptySrc()

        Dim src As String = Nothing
        Assert.IsTrue(GoodfonHtmlParser.TryFindImageSource(GoodfonHtmlParser.LoadDocument("<img alt='x'>"), src))
        Assert.AreEqual(String.Empty, src)
    End Sub

    ''' <summary>
    ''' Verifies that a page without images returns <see langword="False"/> and <see langword="Nothing"/>.
    ''' </summary>
    <TestMethod>
    Public Sub TryFindImageSource_NoImages_ReturnsFalseAndNothing()

        Dim src As String = "unchanged"
        Assert.IsFalse(GoodfonHtmlParser.TryFindImageSource(GoodfonHtmlParser.LoadDocument("<p>no images</p>"), src))
        Assert.IsNull(src)
    End Sub

    ''' <summary>
    ''' Verifies that a null document throws <see cref="ArgumentNullException"/>.
    ''' </summary>
    <TestMethod>
    Public Sub TryFindImageSource_Nothing_ThrowsArgumentNullException()

        Dim src As String = Nothing
        Assert.ThrowsExactly(Of ArgumentNullException)(Sub() GoodfonHtmlParser.TryFindImageSource(Nothing, src))
    End Sub

#End Region

End Class

#End Region
