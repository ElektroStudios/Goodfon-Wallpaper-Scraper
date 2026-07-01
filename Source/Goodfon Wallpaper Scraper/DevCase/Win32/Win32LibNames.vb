
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Runtime.InteropServices

#End Region

#Region " Win32LibNames "

' ReSharper disable once CheckNamespace

Namespace DevCase.Win32

    ''' <summary>
    ''' Contains the filenames to specify in <see cref="DllImportAttribute.Value"/> for all used Win32 API libraries.
    ''' </summary>
    Friend Module Win32LibNames

        ' ReSharper disable InconsistentNaming

        ''' <summary>
        ''' GDI32.dll
        ''' </summary>
        Friend Const Gdi32 As String = "gdi32.dll"

        ''' <summary>
        ''' Kernel32.dll
        ''' </summary>
        Friend Const Kernel32 As String = "kernel32.dll"

        ''' <summary>
        ''' NtDll.dll
        ''' </summary>
        Friend Const NtDll As String = "ntdll.dll"

        ''' <summary>
        ''' User32.dll
        ''' </summary>
        Friend Const User32 As String = "user32.dll"

    End Module

End Namespace

#End Region
