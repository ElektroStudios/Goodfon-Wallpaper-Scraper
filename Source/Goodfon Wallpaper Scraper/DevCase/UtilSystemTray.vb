
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Diagnostics

Imports DevCase.Win32

#End Region

#Region " Shell Util ( System Tray ) "

' ReSharper disable once CheckNamespace

Namespace DevCase.Core.Windows.Desktop

    ''' <summary>
    ''' Contains System-Tray related utilities.
    ''' </summary>
    Public NotInheritable Class UtilSystemTray

#Region " Constructors "

        ''' <summary>
        ''' Prevents a default instance of the <see cref="UtilSystemTray"/> class from being created.
        ''' </summary>
        <DebuggerNonUserCode>
        Private Sub New()
        End Sub

#End Region

#Region " Properties "

        ''' <summary>
        ''' Gets the class name of the SystemTray window.
        ''' </summary>
        '''
        ''' <value>
        ''' The class name of the SystemTray window.
        ''' </value>
        Public Shared ReadOnly Property ClassName As String
            Get
                Return "TrayNotifyWnd"
            End Get
        End Property

        ''' <summary>
        ''' Gets the window handle of the SystemTray window.
        ''' </summary>
        '''
        ''' <value>
        ''' The window handle of the SystemTray window.
        ''' </value>
        Public Shared ReadOnly Property Hwnd As IntPtr
            <DebuggerStepThrough>
            Get
                Return NativeMethods.FindWindowEx(UtilTaskBar.Hwnd, IntPtr.Zero, UtilSystemTray.ClassName, Nothing)
            End Get
        End Property

#End Region

    End Class

End Namespace

#End Region
