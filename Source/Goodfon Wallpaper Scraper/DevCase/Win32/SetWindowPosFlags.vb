
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Diagnostics.CodeAnalysis

#End Region

#Region " SetWindowPos Flags "

' ReSharper disable once CheckNamespace

Namespace DevCase.Win32.Enums

    ''' <summary>
    ''' The window sizing and positioning flags.
    ''' <para></para>
    ''' Flags combination for <c>uFlags</c> parameter of <see cref="NativeMethods.SetWindowPos"/> function.
    ''' </summary>
    '''
    ''' <remarks>
    ''' <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms633545%28v=vs.85%29.aspx"/>
    ''' </remarks>
    <Flags>
    <SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification:="Required to migrate this code to .NET Core")>
    <SuppressMessage("Design", "CA1069:Enums values should not be duplicated", Justification:="")>
    Public Enum SetWindowPosFlags As UInteger

        ' ***************************************************************************************
        '  ⚠️                          IMPORTANT NOTICE
        '
        '  THIS ENUMERATION IS PARTIALLY DEFINED FOR THE SPECIFIC PURPOSES OF THE GOODFON SCRAPER
        ' ***************************************************************************************

        ''' <summary>
        ''' Indicates any flag.
        ''' </summary>
        None = &H0UI

        ''' <summary>
        ''' Retains the current position (ignores X and Y parameters).
        ''' </summary>
        IgnoreMove = &H2

        ''' <summary>
        ''' Retains the current Z order (ignores the <c>hwndInsertAfter</c> parameter).
        ''' </summary>
        IgnoreZOrder = &H4

        ''' <summary>
        ''' Displays the window.
        ''' </summary>
        ShowWindow = &H40

    End Enum

End Namespace

#End Region
