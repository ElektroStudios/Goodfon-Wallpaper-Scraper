
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Runtime.InteropServices

#End Region

#Region " TrayData "

' ReSharper disable once CheckNamespace

Namespace DevCase.Win32.Structures

    ''' <summary>
    ''' Contains information about a system tray button icon.
    ''' </summary>
    '''
    ''' <remarks>
    ''' <see href="https://www.codeproject.com/Articles/10807/Shell-Tray-Info-Arrange-your-system-tray-icons"/>
    ''' </remarks>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure TrayData64

        ''' <summary>
        ''' The handle to the window that owns the tray icon.
        ''' </summary>
        Public Hwnd As IntPtr

        ''' <summary>
        ''' The application-defined identifier of the tray icon.
        ''' </summary>
        Public id As UInteger

        ''' <summary>
        ''' The application-defined callback message identifier.
        ''' </summary>
        Public CallbackMessage As UInteger

        ''' <summary>
        ''' Reserved.
        ''' </summary>
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=2, ArraySubType:=UnmanagedType.U4)>
        Public Reserved() As UInteger

        ''' <summary>
        ''' The handle to the icon displayed in the tray.
        ''' </summary>
        Public IconHandle As IntPtr

        'Public ReadOnly Property OwnerProcess As Process
        '    Get
        '        Dim ret As Process = Nothing
        '        If Hwnd <> IntPtr.Zero Then
        '            Dim processIDOfButton As Int32
        '            Dim threadId As Int32 = NativeMethods.GetWindowThreadProcessId(Hwnd, processIDOfButton)
        '            Try ' Process.GetProcessById can throw an exception if the id is not found
        '                ret = Process.GetProcessById(processIDOfButton)
        '            Catch ex As Exception
        '                ' eat it
        '            End Try
        '        End If
        '        Return ret
        '    End Get
        'End Property

    End Structure

End Namespace

#End Region
