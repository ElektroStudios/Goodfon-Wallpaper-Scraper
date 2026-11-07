#Region " Option Statements "

Option Explicit On
Option Strict On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Collections.Generic
Imports System.Text.Json.Serialization

#End Region

#Region " GoodFonAccountsFile "

''' <summary>
''' Represents the content of the "Accounts.json" configuration file:
''' the login settings and the list of GoodFon.com accounts.
''' </summary>
Public NotInheritable Class GoodFonAccountsFile

    ''' <summary>
    ''' Gets or sets the login settings.
    ''' </summary>
    <JsonPropertyName("LoginSettings")>
    Public Property LoginSettings As New GoodFonLoginSettings()

    ''' <summary>
    ''' Gets or sets the list of account configurations.
    ''' </summary>
    <JsonPropertyName("Accounts")>
    Public Property Accounts As List(Of GoodFonAccountConfig)

End Class

#End Region
