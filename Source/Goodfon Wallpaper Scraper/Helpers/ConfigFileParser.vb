#Region " Option Statements "

Option Explicit On
Option Strict On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.Text.Json

#End Region

#Region " ConfigFileParser "

''' <summary>
''' Provides parsing helpers for the user configuration files of this application.
''' </summary>
Friend Module ConfigFileParser

#Region " Static Methods "

    ''' <summary>
    ''' Parses the lines of a list-based configuration file
    ''' (e.g., "Search Terms.txt", "Forbidden Keywords.txt" or the downloads history file).
    ''' <para></para>
    ''' Each line is trimmed; empty lines and comment lines (starting with "#") are ignored.
    ''' Duplicated entries are removed using case-insensitive comparison.
    ''' </summary>
    '''
    ''' <param name="lines">
    ''' The lines to parse.
    ''' </param>
    '''
    ''' <returns>
    ''' A case-insensitive <see cref="HashSet(Of String)"/> containing the parsed entries, in order of appearance.
    ''' </returns>
    '''
    ''' <exception cref="ArgumentNullException">
    ''' Thrown when <paramref name="lines"/> is <see langword="Nothing"/>.
    ''' </exception>
    <DebuggerStepThrough>
    Friend Function ParseListLines(lines As IEnumerable(Of String)) As HashSet(Of String)

        If lines Is Nothing Then
            Throw New ArgumentNullException(NameOf(lines))
        End If

        Dim result As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

        For Each line As String In lines
            If line Is Nothing Then
                Continue For
            End If

            Dim trimmed As String = line.Trim()
            If String.IsNullOrWhiteSpace(trimmed) OrElse trimmed.StartsWith("#"c) Then
                Continue For
            End If

            result.Add(trimmed)
        Next line

        Return result
    End Function

    ''' <summary>
    ''' Parses the JSON content of the "Accounts.json" configuration file.
    ''' <para></para>
    ''' Property names are matched case-insensitively, and JSON comments are allowed.
    ''' </summary>
    '''
    ''' <param name="jsonContent">
    ''' The JSON content to parse.
    ''' </param>
    '''
    ''' <returns>
    ''' The list of parsed <see cref="GoodFonAccountConfig"/> objects.
    ''' </returns>
    '''
    ''' <exception cref="InvalidOperationException">
    ''' Thrown when <paramref name="jsonContent"/> is empty, or when it deserializes to <see langword="Nothing"/>.
    ''' </exception>
    '''
    ''' <exception cref="JsonException">
    ''' Thrown when <paramref name="jsonContent"/> is not valid JSON.
    ''' </exception>
    <DebuggerStepThrough>
    Friend Function ParseAccountsJson(jsonContent As String) As List(Of GoodFonAccountConfig)

        Return ConfigFileParser.ParseAccountsFile(jsonContent).Accounts
    End Function

    ''' <summary>
    ''' Parses the JSON content of the "Accounts.json" configuration file, including the login settings.
    ''' <para></para>
    ''' Two formats are supported:
    ''' <list type="bullet">
    '''   <item><description>
    '''   Current format: a JSON object with "LoginSettings" (optional) and "Accounts" (required) properties.
    '''   </description></item>
    '''   <item><description>
    '''   Legacy format: a JSON array of accounts. Default login settings are used.
    '''   </description></item>
    ''' </list>
    ''' Property names are matched case-insensitively, and JSON comments and trailing commas are allowed.
    ''' </summary>
    '''
    ''' <param name="jsonContent">
    ''' The JSON content to parse.
    ''' </param>
    '''
    ''' <returns>
    ''' The parsed <see cref="GoodFonAccountsFile"/>, with validated login settings.
    ''' </returns>
    '''
    ''' <exception cref="InvalidOperationException">
    ''' Thrown when <paramref name="jsonContent"/> is empty, deserializes to <see langword="Nothing"/>,
    ''' has an unexpected root element, lacks the "Accounts" property, or contains invalid login settings.
    ''' </exception>
    '''
    ''' <exception cref="JsonException">
    ''' Thrown when <paramref name="jsonContent"/> is not valid JSON.
    ''' </exception>
    <DebuggerStepThrough>
    Friend Function ParseAccountsFile(jsonContent As String) As GoodFonAccountsFile

        If String.IsNullOrWhiteSpace(jsonContent) Then
            Throw New InvalidOperationException("Accounts json file is empty.")
        End If

        Dim options As New JsonSerializerOptions With {
            .PropertyNameCaseInsensitive = True,
            .ReadCommentHandling = JsonCommentHandling.Skip,
            .AllowTrailingCommas = True
        }

        Dim documentOptions As New JsonDocumentOptions With {
            .CommentHandling = JsonCommentHandling.Skip,
            .AllowTrailingCommas = True
        }

        Dim result As GoodFonAccountsFile
        Using document As JsonDocument = JsonDocument.Parse(jsonContent, documentOptions)
            Dim root As JsonElement = document.RootElement

            Select Case root.ValueKind
                Case JsonValueKind.Array ' Legacy format.
                    result = New GoodFonAccountsFile() With {
                        .Accounts = JsonSerializer.Deserialize(Of List(Of GoodFonAccountConfig))(root.GetRawText(), options)
                    }

                Case JsonValueKind.Object ' Current format.
                    result = JsonSerializer.Deserialize(Of GoodFonAccountsFile)(root.GetRawText(), options)

                Case JsonValueKind.Null
                    Throw New InvalidOperationException("Failed to deserialize accounts json file content.")

                Case Else
                    Throw New InvalidOperationException($"Unexpected root element in accounts json file: {root.ValueKind}. Expected an object or an array.")

            End Select
        End Using

        If result.Accounts Is Nothing Then
            Throw New InvalidOperationException("Accounts json file does not contain the 'Accounts' list.")
        End If

        If result.LoginSettings Is Nothing Then
            Throw New InvalidOperationException("Accounts json file contains a null 'LoginSettings' value.")
        End If

        If result.Accounts.Contains(Nothing) Then
            Throw New InvalidOperationException("Accounts json file contains null account entries.")
        End If

        result.LoginSettings.Validate()

        Return result
    End Function

    ''' <summary>
    ''' Masks a credential value (username or password) for safe display in the console,
    ''' keeping only its first characters visible and replacing the rest with "*" characters.
    ''' <para></para>
    ''' The number of visible characters is <paramref name="visibleChars"/> at most,
    ''' but never more than half of the value length, so short values are never fully revealed.
    ''' </summary>
    '''
    ''' <param name="value">
    ''' The credential value to mask.
    ''' </param>
    '''
    ''' <param name="visibleChars">
    ''' The maximum number of leading characters to keep visible.
    ''' </param>
    '''
    ''' <returns>
    ''' The masked value. An empty string is returned if <paramref name="value"/> is <see langword="Nothing"/> or empty.
    ''' </returns>
    '''
    ''' <exception cref="ArgumentOutOfRangeException">
    ''' Thrown when <paramref name="visibleChars"/> is negative.
    ''' </exception>
    <DebuggerStepThrough>
    Friend Function MaskCredential(value As String, Optional visibleChars As Integer = 4) As String

        If visibleChars < 0 Then
            Throw New ArgumentOutOfRangeException(NameOf(visibleChars), visibleChars, "Visible characters count cannot be negative.")
        End If

        If String.IsNullOrEmpty(value) Then
            Return String.Empty
        End If

        Dim visibleCount As Integer = Math.Min(visibleChars, value.Length \ 2)

        Return $"{value.Substring(0, visibleCount)}{New String("*"c, value.Length - visibleCount)}"
    End Function

#End Region

End Module

#End Region
