#Region " Option Statements "

Option Explicit On
Option Strict On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.Text.RegularExpressions

Imports HtmlAgilityPack

Imports HtmlDocument = HtmlAgilityPack.HtmlDocument

#End Region

#Region " GoodfonHtmlParser "

''' <summary>
''' Provides HTML parsing helpers to extract data from GoodFon.com web pages.
''' </summary>
Friend Module GoodfonHtmlParser

#Region " Constants "

    ''' <summary>
    ''' XPath expression that selects the wallpaper links in a listing page.
    ''' </summary>
    Friend Const WallpaperLinksXPath As String = "//div[contains(@class,'wallpapers__item')]/div/a"

    ''' <summary>
    ''' XPath expression that selects the original-resolution download button in a wallpaper page.
    ''' </summary>
    Friend Const DownloadButtonXPath As String = "//a[contains(@class,'wallpaper__download__rbut') and contains(@class,'js-size')]"

    ''' <summary>
    ''' The text shown by the website in the download page when the daily download limit has been reached.
    ''' </summary>
    Friend Const DailyLimitReachedText As String = "Unfortunately, you have reached your download limit for today"

#End Region

#Region " Static Methods "

    ''' <summary>
    ''' Loads the specified HTML source into a new <see cref="HtmlDocument"/>.
    ''' </summary>
    '''
    ''' <param name="html">
    ''' The HTML source.
    ''' </param>
    '''
    ''' <returns>
    ''' The resulting <see cref="HtmlDocument"/>.
    ''' </returns>
    '''
    ''' <exception cref="ArgumentNullException">
    ''' Thrown when <paramref name="html"/> is <see langword="Nothing"/>.
    ''' </exception>
    <DebuggerStepThrough>
    Friend Function LoadDocument(html As String) As HtmlDocument

        If html Is Nothing Then
            Throw New ArgumentNullException(NameOf(html))
        End If

        Dim doc As New HtmlDocument()
        doc.LoadHtml(html)
        Return doc
    End Function

    ''' <summary>
    ''' Determines whether the specified href (taken from a listing page wallpaper item)
    ''' points to a valid wallpaper page.
    ''' <para></para>
    ''' Empty hrefs, "#" anchors and user profile links are considered invalid.
    ''' </summary>
    '''
    ''' <param name="href">
    ''' The href to evaluate.
    ''' </param>
    '''
    ''' <returns>
    ''' <see langword="True"/> if the href points to a wallpaper page; otherwise, <see langword="False"/>.
    ''' </returns>
    <DebuggerStepThrough>
    Friend Function IsValidWallpaperHref(href As String) As Boolean

        If String.IsNullOrWhiteSpace(href) Then
            Return False
        End If

        If href.Equals("#", StringComparison.Ordinal) Then
            Return False
        End If

        If href.IndexOf("/user/", StringComparison.OrdinalIgnoreCase) >= 0 Then
            Return False
        End If

        Return True
    End Function

    ''' <summary>
    ''' Extracts the absolute wallpaper page URLs from a GoodFon.com listing page.
    ''' </summary>
    '''
    ''' <param name="doc">
    ''' The listing page document.
    ''' </param>
    '''
    ''' <returns>
    ''' The list of absolute wallpaper page URLs, in document order.
    ''' <para></para>
    ''' The list is empty if no valid wallpaper links are found.
    ''' </returns>
    '''
    ''' <exception cref="ArgumentNullException">
    ''' Thrown when <paramref name="doc"/> is <see langword="Nothing"/>.
    ''' </exception>
    <DebuggerStepThrough>
    Friend Function ExtractWallpaperPageUrls(doc As HtmlDocument) As List(Of String)

        If doc Is Nothing Then
            Throw New ArgumentNullException(NameOf(doc))
        End If

        Dim urls As New List(Of String)()

        Dim nodes As HtmlNodeCollection = doc.DocumentNode.SelectNodes(GoodfonHtmlParser.WallpaperLinksXPath)
        If nodes Is Nothing Then
            Return urls
        End If

        For Each node As HtmlNode In nodes
            Dim href As String = node.GetAttributeValue("href", String.Empty)
            If GoodfonHtmlParser.IsValidWallpaperHref(href) Then
                urls.Add(UrlHelper.ToAbsoluteUrl(href))
            End If
        Next node

        Return urls
    End Function

    ''' <summary>
    ''' Determines whether the specified listing page document contains any wallpaper item link node,
    ''' regardless of whether the links are valid.
    ''' </summary>
    '''
    ''' <param name="doc">
    ''' The listing page document.
    ''' </param>
    '''
    ''' <returns>
    ''' <see langword="True"/> if the document contains wallpaper item link nodes; otherwise, <see langword="False"/>.
    ''' </returns>
    '''
    ''' <exception cref="ArgumentNullException">
    ''' Thrown when <paramref name="doc"/> is <see langword="Nothing"/>.
    ''' </exception>
    <DebuggerStepThrough>
    Friend Function HasWallpaperNodes(doc As HtmlDocument) As Boolean

        If doc Is Nothing Then
            Throw New ArgumentNullException(NameOf(doc))
        End If

        Return doc.DocumentNode.SelectNodes(GoodfonHtmlParser.WallpaperLinksXPath) IsNot Nothing
    End Function

    ''' <summary>
    ''' Counts the valid wallpaper links present in a GoodFon.com listing page,
    ''' excluding user profile links, "#" anchors and empty hrefs.
    ''' </summary>
    '''
    ''' <param name="doc">
    ''' The listing page document.
    ''' </param>
    '''
    ''' <returns>
    ''' The number of valid wallpaper links.
    ''' </returns>
    '''
    ''' <exception cref="ArgumentNullException">
    ''' Thrown when <paramref name="doc"/> is <see langword="Nothing"/>.
    ''' </exception>
    <DebuggerStepThrough>
    Friend Function CountValidWallpaperLinks(doc As HtmlDocument) As Integer

        Return GoodfonHtmlParser.ExtractWallpaperPageUrls(doc).Count
    End Function

    ''' <summary>
    ''' Finds the highest page number referenced by the pagination elements of a GoodFon.com listing page.
    ''' <para></para>
    ''' Two pagination formats are recognized:
    ''' category/tag index links ("index-N.html") and the search results paginator "Last" link ("page=N").
    ''' </summary>
    '''
    ''' <param name="doc">
    ''' The listing page document.
    ''' </param>
    '''
    ''' <returns>
    ''' The highest page number found, or <see langword="Nothing"/> if the page has no recognizable pagination.
    ''' </returns>
    '''
    ''' <exception cref="ArgumentNullException">
    ''' Thrown when <paramref name="doc"/> is <see langword="Nothing"/>.
    ''' </exception>
    <DebuggerStepThrough>
    Friend Function FindLastPageNumber(doc As HtmlDocument) As Integer?

        If doc Is Nothing Then
            Throw New ArgumentNullException(NameOf(doc))
        End If

        Dim lastPageNumber As Integer? = Nothing

        ' 1. Category listing pages ("index-N.html").
        Dim pageLinks As HtmlNodeCollection = doc.DocumentNode.SelectNodes("//a[contains(@href,'index-') and contains(@href,'.html')]")
        If pageLinks IsNot Nothing Then
            For Each link As HtmlNode In pageLinks
                Dim href As String = link.GetAttributeValue("href", String.Empty)
                lastPageNumber = GoodfonHtmlParser.MaxPageNumber(lastPageNumber, AppGlobals.PaginationIndexRegex, href)
            Next link
        End If

        ' 2. Search results paginator ("page=N" in the "Last" link).
        Dim paginator As HtmlNode = doc.DocumentNode.SelectSingleNode("//div[contains(@class,'paginator')]")
        If paginator IsNot Nothing Then
            Dim lastLink As HtmlNode = paginator.SelectSingleNode(".//a[contains(.,'Last')]")
            If lastLink IsNot Nothing Then
                Dim href As String = lastLink.GetAttributeValue("href", String.Empty)
                lastPageNumber = GoodfonHtmlParser.MaxPageNumber(lastPageNumber, AppGlobals.PaginationQueryRegex, href)
            End If
        End If

        Return lastPageNumber
    End Function

    ''' <summary>
    ''' Calculates the total number of wallpapers of a listing,
    ''' assuming that every page except the last one is full
    ''' (<see cref="AppGlobals.MaxWallpaperItemsPerPage"/> items).
    ''' </summary>
    '''
    ''' <param name="lastPage">
    ''' The last page number (1-based).
    ''' </param>
    '''
    ''' <param name="itemsOnLastPage">
    ''' The number of wallpapers on the last page.
    ''' </param>
    '''
    ''' <returns>
    ''' The total number of wallpapers.
    ''' </returns>
    '''
    ''' <exception cref="ArgumentOutOfRangeException">
    ''' Thrown when <paramref name="lastPage"/> is less than 1, or <paramref name="itemsOnLastPage"/> is negative.
    ''' </exception>
    <DebuggerStepThrough>
    Friend Function CalculateTotalWallpapers(lastPage As Integer, itemsOnLastPage As Integer) As Integer

        If lastPage < 1 Then
            Throw New ArgumentOutOfRangeException(NameOf(lastPage), lastPage, "Last page number must be greater than or equal to 1.")
        End If

        If itemsOnLastPage < 0 Then
            Throw New ArgumentOutOfRangeException(NameOf(itemsOnLastPage), itemsOnLastPage, "Items count cannot be negative.")
        End If

        Return ((lastPage - 1) * AppGlobals.MaxWallpaperItemsPerPage) + itemsOnLastPage
    End Function

    ''' <summary>
    ''' Tries to find the href of the original-resolution download button in a GoodFon.com wallpaper page.
    ''' </summary>
    '''
    ''' <param name="doc">
    ''' The wallpaper page document.
    ''' </param>
    '''
    ''' <param name="refHref">
    ''' When this method returns <see langword="True"/>, contains the href attribute value of the download button
    ''' (it can be an empty string if the button has no href);
    ''' otherwise, <see langword="Nothing"/>.
    ''' </param>
    '''
    ''' <returns>
    ''' <see langword="True"/> if the download button was found;
    ''' <see langword="False"/> if the button is missing (e.g., disabled by the website).
    ''' </returns>
    '''
    ''' <exception cref="ArgumentNullException">
    ''' Thrown when <paramref name="doc"/> is <see langword="Nothing"/>.
    ''' </exception>
    <DebuggerStepThrough>
    Friend Function TryFindDownloadButtonHref(doc As HtmlDocument, ByRef refHref As String) As Boolean

        If doc Is Nothing Then
            Throw New ArgumentNullException(NameOf(doc))
        End If

        Dim downloadNode As HtmlNode = doc.DocumentNode.SelectSingleNode(GoodfonHtmlParser.DownloadButtonXPath)
        If downloadNode Is Nothing Then
            refHref = Nothing
            Return False
        End If

        refHref = downloadNode.GetAttributeValue("href", String.Empty)
        Return True
    End Function

    ''' <summary>
    ''' Determines whether the specified GoodFon.com download page indicates
    ''' that the daily download limit has been reached.
    ''' </summary>
    '''
    ''' <param name="doc">
    ''' The download page document.
    ''' </param>
    '''
    ''' <returns>
    ''' <see langword="True"/> if the daily download limit has been reached; otherwise, <see langword="False"/>.
    ''' </returns>
    '''
    ''' <exception cref="ArgumentNullException">
    ''' Thrown when <paramref name="doc"/> is <see langword="Nothing"/>.
    ''' </exception>
    <DebuggerStepThrough>
    Friend Function IsDailyLimitReached(doc As HtmlDocument) As Boolean

        If doc Is Nothing Then
            Throw New ArgumentNullException(NameOf(doc))
        End If

        Return doc.DocumentNode.OuterHtml.IndexOf(GoodfonHtmlParser.DailyLimitReachedText, StringComparison.OrdinalIgnoreCase) >= 0
    End Function

    ''' <summary>
    ''' Tries to find the source URL of the full-size image in a GoodFon.com download page.
    ''' <para></para>
    ''' The following image nodes are searched, in order of preference:
    ''' the image inside the download link, an image with a "wallpaper" class, and the first image of the page.
    ''' </summary>
    '''
    ''' <param name="doc">
    ''' The download page document.
    ''' </param>
    '''
    ''' <param name="refSrc">
    ''' When this method returns <see langword="True"/>, contains the src attribute value of the image node
    ''' (it can be an empty string if the image has no src);
    ''' otherwise, <see langword="Nothing"/>.
    ''' </param>
    '''
    ''' <returns>
    ''' <see langword="True"/> if an image node was found; otherwise, <see langword="False"/>.
    ''' </returns>
    '''
    ''' <exception cref="ArgumentNullException">
    ''' Thrown when <paramref name="doc"/> is <see langword="Nothing"/>.
    ''' </exception>
    <DebuggerStepThrough>
    Friend Function TryFindImageSource(doc As HtmlDocument, ByRef refSrc As String) As Boolean

        If doc Is Nothing Then
            Throw New ArgumentNullException(NameOf(doc))
        End If

        Dim imgNode As HtmlNode =
            If(doc.DocumentNode.SelectSingleNode("//a[contains(@class,'wallpaper__download__link')]/img"),
            If(doc.DocumentNode.SelectSingleNode("//img[contains(@class,'wallpaper') or contains(@class,'wallpaper__img')]"),
               doc.DocumentNode.SelectSingleNode("//img")))

        If imgNode Is Nothing Then
            refSrc = Nothing
            Return False
        End If

        refSrc = imgNode.GetAttributeValue("src", String.Empty)
        Return True
    End Function

#End Region

#Region " Private Methods "

    ''' <summary>
    ''' Extracts a page number from the specified href using the specified regular expression,
    ''' and returns the greater value between it and the current maximum.
    ''' </summary>
    '''
    ''' <param name="currentMax">
    ''' The current maximum page number, or <see langword="Nothing"/> if none was found yet.
    ''' </param>
    '''
    ''' <param name="pageRegex">
    ''' The regular expression whose first capture group contains the page number.
    ''' </param>
    '''
    ''' <param name="href">
    ''' The href to evaluate.
    ''' </param>
    '''
    ''' <returns>
    ''' The new maximum page number, or <paramref name="currentMax"/> if the href does not contain a valid page number.
    ''' </returns>
    <DebuggerStepThrough>
    Private Function MaxPageNumber(currentMax As Integer?, pageRegex As Regex, href As String) As Integer?

        Dim match As Match = pageRegex.Match(href)
        If Not match.Success Then
            Return currentMax
        End If

        Dim pageNumber As Integer
        If Not Integer.TryParse(match.Groups(1).Value, pageNumber) Then
            Return currentMax
        End If

        If Not currentMax.HasValue OrElse (pageNumber > currentMax.Value) Then
            Return pageNumber
        End If

        Return currentMax
    End Function

#End Region

End Module

#End Region
