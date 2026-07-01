
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Diagnostics

Imports DevCase.Win32

#End Region

#Region " Shell Util ( TaskBar ) "

' ReSharper disable once CheckNamespace

Namespace DevCase.Core.Windows.Desktop

    ''' <summary>
    ''' Contains Windows TaskBar related utilities.
    ''' </summary>
    Public NotInheritable Class UtilTaskBar

#Region " Constructors "

        ''' <summary>
        ''' Prevents a default instance of the <see cref="UtilTaskBar"/> class from being created.
        ''' </summary>
        <DebuggerNonUserCode>
        Private Sub New()
        End Sub

#End Region

#Region " Properties "

        ''' <summary>
        ''' Gets the class name of the TaskBar window.
        ''' </summary>
        '''
        ''' <value>
        ''' The class name of the TaskBar window
        ''' </value>
        Public Shared ReadOnly Property ClassName As String
            Get
                Return "Shell_TrayWnd"
            End Get
        End Property

        ''' <summary>
        ''' Gets the window handle of the TaskBar window.
        ''' </summary>
        '''
        ''' <value>
        ''' The window handle of the TaskBar window
        ''' </value>
        Public Shared ReadOnly Property Hwnd As IntPtr
            <DebuggerStepThrough>
            Get
                Return NativeMethods.FindWindow(UtilTaskBar.ClassName, Nothing)
            End Get
        End Property

#End Region

    End Class

End Namespace

#End Region
