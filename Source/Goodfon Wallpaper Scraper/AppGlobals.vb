#Region " Option Statements "

Option Explicit On
Option Strict On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Collections.Generic
Imports System.Globalization
Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions

#End Region

''' <summary>
''' Provides global constants, read-only fields and properties for this application.
''' </summary>
Friend Module AppGlobals

    ''' <summary>
    ''' The title of the application.
    ''' </summary>
    Friend ReadOnly AppTitleAndVersion As String =
        $"{My.Application.Info.Title} v{My.Application.Info.Version.ToString(fieldCount:=3)} - by ElektroStudios"

    ''' <summary>
    ''' The file path where to save the downloads history.
    ''' </summary>
    Friend ReadOnly HistoryFilePath As String =
         Path.Combine(My.Application.Info.DirectoryPath, "cache\downloads history.txt")

    ''' <summary>
    ''' The file path where to read GoodFon's user-account credentials.
    ''' </summary>
    Friend ReadOnly GoodfonAccountsFilePath As String =
        Path.Combine(My.Application.Info.DirectoryPath, "config\accounts.json")

    ''' <summary>
    ''' The file paths from which to load the user-configured search terms list.
    ''' </summary>
    Friend ReadOnly SearchTermsFilePath As String =
        Path.Combine(My.Application.Info.DirectoryPath, "config\search terms.txt")

    ''' <summary>
    ''' The file paths from which to load the user-configured forbidden keywords list.
    ''' </summary>
    Friend ReadOnly ForbiddenKeywordsFilePath As String =
        Path.Combine(My.Application.Info.DirectoryPath, "config\forbidden keywords.txt")

    ''' <summary>
    ''' The base directory where to save the output files containing the magnet URLs.
    ''' </summary>
    Friend ReadOnly OutputDirPath As String =
        Path.Combine(My.Application.Info.DirectoryPath, "output")

    ''' <summary>
    ''' The base URL for the Goodfon.com website.
    ''' </summary>
    Friend Const BaseUrl As String = "https://www.goodfon.com"

    ''' <summary>
    ''' The base URL of the Goodfon website for browsing wallpapers by tags.
    ''' </summary>
    Friend Const BaseGoodfonTagUrl As String = "https://www.goodfon.com/tags"

    ''' <summary>
    ''' The base URL of the Goodfon website for browsing wallpapers by user name.
    ''' </summary>
    Friend Const BaseGoodfonUserUrl As String = "https://www.goodfon.com/user"

    ''' <summary>
    ''' The base URL of the Goodfon website for searching wallpapers.
    ''' </summary>
    Friend Const BaseGoodfonSearchUrl As String = "https://www.goodfon.com/search"

    ''' <summary>
    ''' The Referrer header for HTTP requests.
    ''' </summary>
    Friend ReadOnly Referrer As New Uri($"{AppGlobals.BaseUrl}/")

    ''' <summary>
    ''' Regular expression used to extract page numbers from category index URLs  in the format "index-N.html".
    ''' </summary>
    Friend ReadOnly PaginationIndexRegex As New Regex("index-(\d+)\.html", RegexOptions.Compiled Or RegexOptions.IgnoreCase)

    ''' <summary>
    ''' Regular expression used to extract page numbers from query string URLs in the format "page=N".
    ''' </summary>
    Friend ReadOnly PaginationQueryRegex As New Regex("page=(\d+)", RegexOptions.Compiled Or RegexOptions.IgnoreCase)

    ''' <summary>
    ''' Maximum wallpaper downloads allowed by the website per anonymous IP address.
    ''' </summary>
    Friend Const MaxDownloadsPerIp As Integer = 80

    ''' <summary>
    ''' Maximum wallpaper items that can be presents in a page.
    ''' </summary>
    Friend Const MaxWallpaperItemsPerPage As Integer = 24

    ''' <summary>
    ''' The maximum allowed duration for a page fetching operation before a timeout occurs.
    ''' </summary>
    Friend ReadOnly PageFetchingMaxTimeout As TimeSpan = TimeSpan.FromMinutes(10)

    ''' <summary>
    ''' The timeout duration for all HTTP client requests.
    ''' </summary>
    Friend ReadOnly HttpClientTimeOut As TimeSpan = TimeSpan.FromMilliseconds(150000) ' 2:30 min; .NET default is 100000 ms (1:40 min)

    ''' <summary>
    ''' The encoding used for console output, 
    ''' and reading and writing to text files.
    ''' <para></para>
    ''' It is set to UTF-8 with BOM (Byte Order Mark).
    ''' </summary>
    Friend ReadOnly TextEncoding As New UTF8Encoding(encoderShouldEmitUTF8Identifier:=True)

End Module
