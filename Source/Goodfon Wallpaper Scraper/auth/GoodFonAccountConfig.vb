
#Region " Option Statements "

Option Explicit On
Option Strict On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Text.Json.Serialization

#End Region

''' <summary>
''' Represents the configuration for a GoodFon account.
''' </summary>
Public NotInheritable Class GoodFonAccountConfig ' NOSONAR

    ''' <summary>
    ''' Gets or sets a value indicating whether this <see cref="GoodFonAccountConfig"/> is enabled.
    ''' </summary>
    <JsonPropertyName("Enabled")>
    Public Property Enabled As Boolean = False

    ''' <summary>
    ''' Gets or sets the account username.
    ''' </summary>
    <JsonPropertyName("Username")>
    Public Property Username As String = String.Empty

    ''' <summary>
    ''' Gets or sets the account password.
    ''' </summary>
    <JsonPropertyName("Password")>
    Public Property Password As String = String.Empty

End Class