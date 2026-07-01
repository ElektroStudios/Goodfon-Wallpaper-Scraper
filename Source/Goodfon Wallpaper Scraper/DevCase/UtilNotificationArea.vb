
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Collections.ObjectModel
Imports System.Diagnostics

Imports DevCase.Core.Diagnostics.Processes
Imports DevCase.Win32
Imports DevCase.Win32.Structures

#End Region

#Region " Shell Util ( Notification Area ) "

' ReSharper disable once CheckNamespace

Namespace DevCase.Core.Windows.Desktop

    ''' <summary>
    ''' Contains System-tray's Notification Area related utilities.
    ''' </summary>
    Public NotInheritable Class UtilNotificationArea

#Region " Constructors "

        ''' <summary>
        ''' Prevents a default instance of the <see cref="UtilNotificationArea"/> class from being created.
        ''' </summary>
        <DebuggerNonUserCode>
        Private Sub New()
        End Sub

#End Region

#Region " Properties "

        ''' <summary>
        ''' Gets the class name of the Notification Area window.
        ''' </summary>
        '''
        ''' <value>
        ''' The class name of the Notification Area window.
        ''' </value>
        Public Shared ReadOnly Property ClassName As String
            Get
                Return "ToolbarWindow32"
            End Get
        End Property

        ''' <summary>
        ''' Gets the window handle of the visible section of the notification area window.
        ''' </summary>
        '''
        ''' <value>
        ''' The window handle of the visible section of the notification area window.
        ''' </value>
        Public Shared ReadOnly Property VisibleHwnd As IntPtr
            <DebuggerStepThrough>
            Get
                Dim sysPagerHwnd As IntPtr = NativeMethods.FindWindowEx(UtilSystemTray.Hwnd, IntPtr.Zero, "SysPager", Nothing)
                Return NativeMethods.FindWindowEx(sysPagerHwnd, IntPtr.Zero, UtilNotificationArea.ClassName, Nothing)
            End Get
        End Property

        ''' <summary>
        ''' Gets the buttons of the visible section of the notification area window.
        ''' </summary>
        '''
        ''' <value>
        ''' The buttons of the visible section of the notification area window.
        ''' </value>
        Public Shared ReadOnly Property VisibleButtons As ReadOnlyCollection(Of NativeToolBarButton)
            <DebuggerStepThrough>
            Get
                Return UtilUIAutomation.GetToolbarButtons64(UtilNotificationArea.VisibleHwnd)
            End Get
        End Property

        '''' <summary>
        '''' Gets the window handle of the hidden section of the notification area window, usually called the overflow window.
        '''' </summary>
        ''''
        '''' <value>
        '''' The window handle of the hidden section of the notification area window, usually called the overflow window.
        '''' </value>
        'Public Shared ReadOnly Property HiddenHwnd As IntPtr
        '    <DebuggerStepThrough>
        '    Get
        '        ' original solution in C#: https://stackoverflow.com/a/33784254/1248295
        '        Dim overflowWindow As IntPtr = NativeMethods.FindWindow("NotifyIconOverflowWindow", Nothing)
        '        If overflowWindow <> IntPtr.Zero Then
        '            Return NativeMethods.FindWindowEx(overflowWindow, IntPtr.Zero, UtilNotificationArea.ClassName, Nothing)
        '        End If
        '    End Get
        'End Property

        '''' <summary>
        '''' Gets the buttons of the hidden section of the notification area window.
        '''' </summary>
        ''''
        '''' <value>
        '''' The buttons of the hidden section of the notification area window.
        '''' </value>
        'Public Shared ReadOnly Property HiddenButtons As ReadOnlyCollection(Of NativeToolBarButton)
        '    <DebuggerStepThrough>
        '    Get
        '        Return UtilUIAutomation.GetToolbarButtons64(UtilNotificationArea.HiddenHwnd)
        '    End Get
        'End Property

#End Region

    End Class

End Namespace

#End Region
