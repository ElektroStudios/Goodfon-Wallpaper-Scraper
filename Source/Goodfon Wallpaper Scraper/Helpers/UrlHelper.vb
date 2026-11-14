#Region " Option Statements "

Option Explicit On
Option Strict On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.Diagnostics.CodeAnalysis
Imports System.IO
Imports System.Security.Cryptography
Imports System.Text

#End Region

#Region " GoodfonUrlKind "

''' <summary>
''' Specifies the kind of a GoodFon.com wallpaper listing URL.
''' </summary>
Friend Enum GoodfonUrlKind As Integer ' NOSONAR

    ''' <summary>
    ''' The URL kind could not be identified.
    ''' </summary>
    Unknown = 0

    ''' <summary>
    ''' A tag listing URL (e.g., "https://www.goodfon.com/tags/cats/").
    ''' </summary>
    Tag = 1

    ''' <summary>
    ''' A user profile listing URL (e.g., "https://www.goodfon.com/user/name").
    ''' </summary>
    UserProfile = 2

    ''' <summary>
    ''' A search query results URL (e.g., "https://www.goodfon.com/search/?q=cats").
    ''' </summary>
    SearchQuery = 3

    ''' <summary>
    ''' A direct URL to a specific wallpaper page  (e.g., "https://www.goodfon.com/erotic").
    ''' </summary>
    DirectURL = 4

End Enum

#End Region

#Region " UrlHelper "

''' <summary>
''' Provides URL building, classification and hashing helpers for GoodFon.com URLs.
''' </summary>
Friend Module UrlHelper

#Region " Static Methods "

    ''' <summary>
    ''' Sanitizes a given URL to create a safe, valid, and length-compliant directory name for the file system.
    ''' </summary>
    ''' <param name="url">The raw URL string that needs to be sanitized.</param>
    ''' <returns>A sanitized string completely safe to be used as a folder name.</returns>
    ''' <exception cref="ArgumentNullException">Thrown immediately if the provided URL is null, empty, or consists only of white-space characters.</exception>
    <DebuggerStepThrough>
    Friend Function SanitizeUrlForFolderName(url As String) As String

        If String.IsNullOrWhiteSpace(url) Then
            Throw New ArgumentNullException(NameOf(url), "The provided URL cannot be null, empty, or whitespace.")
        End If

        Dim parsedUri As Uri = Nothing
        Dim baseName As String = url

        ' Extract Host and Path to discard protocols like http:// or https://
        If Uri.TryCreate(url, UriKind.Absolute, parsedUri) Then
            baseName = $"{parsedUri.Host}{parsedUri.PathAndQuery}"
        End If

        Dim invalidFileNameChars As Char() = Path.GetInvalidFileNameChars()
        Dim safeNameBuilder As New StringBuilder()
        Dim currentCharacter As Char
        Dim isInvalid As Boolean

        For Each currentCharacter In baseName
            ' Explicitly replacing slashes alongside system-defined invalid characters
            isInvalid = (Array.IndexOf(invalidFileNameChars, currentCharacter) >= 0) OrElse (currentCharacter = "/"c) OrElse (currentCharacter = "\"c)

            If isInvalid Then
                safeNameBuilder.Append("_"c)
            Else
                safeNameBuilder.Append(currentCharacter)
            End If
        Next

        Dim result As String = safeNameBuilder.ToString().TrimEnd("_"c)

        Dim maxFolderNameLength As Integer = 100

        If result.Length > maxFolderNameLength Then
            result = result.Substring(0, maxFolderNameLength)
        End If

        Return result
    End Function

    ''' <summary>
    ''' Gets the relative part of the specified URL, starting from the slash that follows the ".com" domain suffix.
    ''' <para></para>
    ''' For example, "https://www.goodfon.com/cats/wallpaper-1.html" returns "/cats/wallpaper-1.html".
    ''' </summary>
    '''
    ''' <param name="url">
    ''' The URL to process.
    ''' </param>
    '''
    ''' <returns>
    ''' The relative URL, or the source <paramref name="url"/> unchanged if it does not contain ".com/".
    ''' </returns>
    '''
    ''' <exception cref="ArgumentNullException">
    ''' Thrown when <paramref name="url"/> is <see langword="Nothing"/>.
    ''' </exception>
    <DebuggerStepThrough>
    Friend Function GetRelativeUrl(url As String) As String

        If url Is Nothing Then
            Throw New ArgumentNullException(NameOf(url))
        End If

        Const relativeUrlDelimiter As String = ".com/"

        Dim delimiterIndex As Integer = url.IndexOf(relativeUrlDelimiter, StringComparison.OrdinalIgnoreCase)

        Return If(delimiterIndex <> -1,
                  url.Substring(delimiterIndex + (relativeUrlDelimiter.Length - 1)),
                  url)
    End Function

    ''' <summary>
    ''' Generates an MD5 hash from the relative part of the given URL
    ''' (see <see cref="UrlHelper.GetRelativeUrl(String)"/>).
    ''' <para></para>
    ''' The hash is used as the identifier stored in the downloads history file.
    ''' </summary>
    '''
    ''' <param name="url">
    ''' The URL to hash.
    ''' </param>
    '''
    ''' <returns>
    ''' The resulting MD5 hash in its uppercase hexadecimal representation (32 characters, without separators).
    ''' </returns>
    '''
    ''' <exception cref="ArgumentNullException">
    ''' Thrown when <paramref name="url"/> is <see langword="Nothing"/>.
    ''' </exception>
    <DebuggerStepThrough>
    <SuppressMessage("Security", "S4790:Using weak hashing algorithms is security-sensitive",
      Justification:="MD5 is used as a non-cryptographic identifier for the downloads history file.")>
    Friend Function GenerateMD5FromUrl(url As String) As String

        Dim relativeUrl As String = UrlHelper.GetRelativeUrl(url)
        Dim inputBytes As Byte() = AppGlobals.TextEncoding.GetBytes(relativeUrl)

        Dim hashBytes As Byte()
        Using md5Provider As MD5 = MD5.Create()
            hashBytes = md5Provider.ComputeHash(inputBytes)
        End Using

        Return BitConverter.ToString(hashBytes).Replace("-", String.Empty)
    End Function

    ''' <summary>
    ''' Checks if the given URL contains any of the keywords in the provided set, using case-insensitive comparison.
    ''' </summary>
    '''
    ''' <param name="url">
    ''' The URL to check for keywords.
    ''' </param>
    '''
    ''' <param name="keywords">
    ''' The set of keywords to look for in the URL. If any keyword is found, the function returns <see langword="True"/>.
    ''' </param>
    '''
    ''' <param name="refKeywordMatched">
    ''' When this method returns <see langword="True"/>, contains the first keyword that was found in the URL;
    ''' otherwise, the value is left unchanged.
    ''' </param>
    '''
    ''' <returns>
    ''' <see langword="True"/> if any of the keywords are present in the URL; otherwise, <see langword="False"/>.
    ''' </returns>
    '''
    ''' <exception cref="ArgumentNullException">
    ''' Thrown when <paramref name="url"/> is <see langword="Nothing"/>.
    ''' </exception>
    <DebuggerStepThrough>
    Friend Function ContainsAnyKeyword(url As String, keywords As IEnumerable(Of String), ByRef refKeywordMatched As String) As Boolean

        If url Is Nothing Then
            Throw New ArgumentNullException(NameOf(url))
        End If

        If keywords Is Nothing Then
            Return False
        End If

        For Each word As String In keywords
            If String.IsNullOrEmpty(word) Then
                Continue For
            End If

            If url.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0 Then
                refKeywordMatched = word
                Return True
            End If
        Next word

        Return False
    End Function

    ''' <summary>
    ''' Converts a (possibly relative) GoodFon.com href into an absolute URL.
    ''' <para></para>
    ''' Hrefs that already start with "http" are returned unchanged;
    ''' otherwise they are combined with <see cref="AppGlobals.BaseUrl"/>.
    ''' </summary>
    '''
    ''' <param name="href">
    ''' The href to convert.
    ''' </param>
    '''
    ''' <returns>
    ''' The absolute URL.
    ''' </returns>
    '''
    ''' <exception cref="ArgumentNullException">
    ''' Thrown when <paramref name="href"/> is <see langword="Nothing"/>.
    ''' </exception>
    <DebuggerStepThrough>
    Friend Function ToAbsoluteUrl(href As String) As String

        If href Is Nothing Then
            Throw New ArgumentNullException(NameOf(href))
        End If

        If href.StartsWith("http", StringComparison.OrdinalIgnoreCase) Then
            Return href
        End If

        Return $"{AppGlobals.BaseUrl}/{href.TrimStart("/"c)}"
    End Function

    ''' <summary>
    ''' Returns a shortened representation of the specified URL for console display purposes,
    ''' removing the "https://www." or "http://www." prefix.
    ''' </summary>
    '''
    ''' <param name="url">
    ''' The URL to shorten.
    ''' </param>
    '''
    ''' <returns>
    ''' The shortened URL.
    ''' </returns>
    '''
    ''' <exception cref="ArgumentNullException">
    ''' Thrown when <paramref name="url"/> is <see langword="Nothing"/>.
    ''' </exception>
    <DebuggerStepThrough>
    Friend Function ToDisplayUrl(url As String) As String

        If url Is Nothing Then
            Throw New ArgumentNullException(NameOf(url))
        End If

        Return url.Replace("https://www.", String.Empty).Replace("http://www.", String.Empty)
    End Function

    ''' <summary>
    ''' Identifies the kind of the specified GoodFon.com listing URL.
    ''' </summary>
    '''
    ''' <param name="baseUrl">
    ''' The listing URL (first page).
    ''' </param>
    '''
    ''' <returns>
    ''' The resulting <see cref="GoodfonUrlKind"/>.
    ''' </returns>
    '''
    ''' <exception cref="ArgumentNullException">
    ''' Thrown when <paramref name="baseUrl"/> is <see langword="Nothing"/>.
    ''' </exception>
    <DebuggerStepThrough>
    Friend Function GetUrlKind(baseUrl As String) As GoodfonUrlKind

        If baseUrl Is Nothing Then
            Throw New ArgumentNullException(NameOf(baseUrl))
        End If

        If baseUrl.IndexOf("/tags/", StringComparison.OrdinalIgnoreCase) > 0 Then
            Return GoodfonUrlKind.Tag
        End If

        If baseUrl.IndexOf("/user/", StringComparison.OrdinalIgnoreCase) > 0 Then
            Return GoodfonUrlKind.UserProfile
        End If

        If baseUrl.IndexOf("?q", StringComparison.OrdinalIgnoreCase) > 0 Then
            Return GoodfonUrlKind.SearchQuery
        End If

        If baseUrl.IndexOf("HTTP", StringComparison.OrdinalIgnoreCase) = 0 AndAlso
           baseUrl.IndexOf(".com/", StringComparison.OrdinalIgnoreCase) > 0 Then
            Return GoodfonUrlKind.DirectURL
        End If

        Return GoodfonUrlKind.Unknown
    End Function

    ''' <summary>
    ''' Builds the URL of the specified page number for a GoodFon.com listing URL.
    ''' </summary>
    '''
    ''' <param name="baseUrl">
    ''' The listing URL (first page).
    ''' </param>
    '''
    ''' <param name="page">
    ''' The 1-based page number.
    ''' </param>
    '''
    ''' <returns>
    ''' The page URL. For page 1, <paramref name="baseUrl"/> is returned unchanged.
    ''' </returns>
    '''
    ''' <exception cref="ArgumentNullException">
    ''' Thrown when <paramref name="baseUrl"/> is <see langword="Nothing"/>.
    ''' </exception>
    '''
    ''' <exception cref="ArgumentOutOfRangeException">
    ''' Thrown when <paramref name="page"/> is less than 1.
    ''' </exception>
    '''
    ''' <exception cref="InvalidOperationException">
    ''' Thrown when the kind of <paramref name="baseUrl"/> cannot be identified.
    ''' </exception>
    <DebuggerStepThrough>
    Friend Function BuildPageUrl(baseUrl As String, page As Integer) As String

        If page < 1 Then
            Throw New ArgumentOutOfRangeException(NameOf(page), page, "Page number must be greater than or equal to 1.")
        End If

        Dim kind As GoodfonUrlKind = UrlHelper.GetUrlKind(baseUrl)

        Select Case kind

            Case GoodfonUrlKind.Tag
                Return If(page = 1, baseUrl, $"{baseUrl}index-{page}.html")

            Case GoodfonUrlKind.UserProfile
                Return If(page = 1, baseUrl, $"{baseUrl}/?&page={page}")

            Case GoodfonUrlKind.SearchQuery
                Return If(page = 1, baseUrl, $"{baseUrl}&page={page}")

            Case GoodfonUrlKind.DirectURL
                Return If(page = 1, baseUrl, $"{baseUrl.TrimEnd("/"c)}?page={page}")

            Case Else
                Throw New InvalidOperationException("Cannot identify URL kind (tag, user profile, search query or direct URL).")

        End Select
    End Function

    ''' <summary>
    ''' Builds the GoodFon.com user profile URL for the specified search term.
    ''' </summary>
    '''
    ''' <param name="searchTerm">
    ''' The search term (user name).
    ''' </param>
    '''
    ''' <returns>
    ''' The user profile URL.
    ''' </returns>
    '''
    ''' <exception cref="ArgumentException">
    ''' Thrown when <paramref name="searchTerm"/> is <see langword="Nothing"/>, empty or white-space.
    ''' </exception>
    <DebuggerStepThrough>
    Friend Function BuildUserProfileUrl(searchTerm As String) As String

        Return $"{AppGlobals.BaseGoodfonUserUrl}/{UrlHelper.EncodeSearchTerm(searchTerm)}"
    End Function

    ''' <summary>
    ''' Builds the GoodFon.com tag URL for the specified search term and tag variant index.
    ''' </summary>
    '''
    ''' <param name="searchTerm">
    ''' The search term (tag name).
    ''' </param>
    '''
    ''' <param name="tagIndex">
    ''' The tag variant index.
    ''' Zero means the plain tag (e.g., "/tags/cats/");
    ''' a positive value means a numbered variant (e.g., "/tags/cats-1/").
    ''' </param>
    '''
    ''' <returns>
    ''' The tag URL.
    ''' </returns>
    '''
    ''' <exception cref="ArgumentException">
    ''' Thrown when <paramref name="searchTerm"/> is <see langword="Nothing"/>, empty or white-space.
    ''' </exception>
    '''
    ''' <exception cref="ArgumentOutOfRangeException">
    ''' Thrown when <paramref name="tagIndex"/> is negative.
    ''' </exception>
    <DebuggerStepThrough>
    Friend Function BuildTagUrl(searchTerm As String, tagIndex As Integer) As String

        If tagIndex < 0 Then
            Throw New ArgumentOutOfRangeException(NameOf(tagIndex), tagIndex, "Tag index cannot be negative.")
        End If

        Dim encodedTerm As String = UrlHelper.EncodeSearchTerm(searchTerm)

        Return If(tagIndex = 0,
                  $"{AppGlobals.BaseGoodfonTagUrl}/{encodedTerm}/",
                  $"{AppGlobals.BaseGoodfonTagUrl}/{encodedTerm}-{tagIndex}/")
    End Function

    ''' <summary>
    ''' Builds the GoodFon.com search query URL for the specified search term.
    ''' </summary>
    '''
    ''' <param name="searchTerm">
    ''' The search term.
    ''' </param>
    '''
    ''' <returns>
    ''' The search query URL.
    ''' </returns>
    '''
    ''' <exception cref="ArgumentException">
    ''' Thrown when <paramref name="searchTerm"/> is <see langword="Nothing"/>, empty or white-space.
    ''' </exception>
    <DebuggerStepThrough>
    Friend Function BuildSearchUrl(searchTerm As String) As String

        Return $"{AppGlobals.BaseGoodfonSearchUrl}/?q={UrlHelper.EncodeSearchTerm(searchTerm)}"
    End Function

#End Region

#Region " Private Methods "

    ''' <summary>
    ''' Encodes a search term for use in a GoodFon.com URL by replacing white-spaces with "+" characters.
    ''' </summary>
    '''
    ''' <param name="searchTerm">
    ''' The search term.
    ''' </param>
    '''
    ''' <returns>
    ''' The encoded search term.
    ''' </returns>
    '''
    ''' <exception cref="ArgumentException">
    ''' Thrown when <paramref name="searchTerm"/> is <see langword="Nothing"/>, empty or white-space.
    ''' </exception>
    <DebuggerStepThrough>
    Private Function EncodeSearchTerm(searchTerm As String) As String

        If String.IsNullOrWhiteSpace(searchTerm) Then
            Throw New ArgumentException("Search term cannot be null, empty or white-space.", NameOf(searchTerm))
        End If

        Return searchTerm.Replace(" "c, "+"c)
    End Function

#End Region

End Module

#End Region
