
#Disable Warning BC42309 ' XML comment has a tag with a 'cref' attribute that could not be resolved

#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Diagnostics
Imports System.Runtime.InteropServices

Imports DevCase.Win32.Enums

#End Region

#Region " Window Placement "

' ReSharper disable once CheckNamespace

Namespace DevCase.Win32.Structures

    ''' <summary>
    ''' Contains information about the placement of a window on the screen.
    ''' </summary>
    '''
    ''' <remarks>
    ''' <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms632611%28v=vs.85%29.aspx"/>
    ''' </remarks>
    <DebuggerStepThrough>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure WindowPlacement

#Region " Fields "

        ''' <summary>
        ''' The size of the structure, in bytes.
        ''' <para></para>
        ''' This member must be set to <c>Marshal.SizeOf(Of WindowPlacement)</c> before calling any function.
        ''' <para></para>
        ''' <see cref="NativeMethods.GetWindowPlacement"/> and <see cref="NativeMethods.SetWindowPlacement"/> 
        ''' fail if this member is not set correctly.
        ''' </summary>
        Public SizeOfStruct As Integer

        ''' <summary>
        ''' Specifies flags that control the position of the minimized window 
        ''' and the method by which the window is restored.
        ''' </summary>
        Public Flags As Integer

        ''' <summary>
        ''' The current show state of the window.
        ''' </summary>
        Public WindowState As NativeWindowState

        ''' <summary>
        ''' The coordinates of the window's upper-left corner when the window is minimized.
        ''' </summary>
        Public MinPosition As NativePoint

        ''' <summary>
        ''' The coordinates of the window's upper-left corner when the window is maximized.
        ''' </summary>
        Public MaxPosition As NativePoint

        ''' <summary>
        ''' The window's coordinates when the window is in the restored position.
        ''' </summary>
        Public NormalPosition As NativeRectangle

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Initializes a new instance of the <see cref="WindowPlacement"/> structure.
        ''' </summary>
        '''
        ''' <param name="initializeSize">
        ''' Allows automatic initialization of <see cref="WindowPlacement.SizeOfStruct"/> member.
        ''' </param>
        Public Sub New(initializeSize As Boolean)
            If initializeSize Then
                Me.SizeOfStruct = Marshal.SizeOf(GetType(WindowPlacement))
            End If
        End Sub

#End Region

    End Structure

End Namespace

#End Region

#Enable Warning BC42309 ' XML comment has a tag with a 'cref' attribute that could not be resolved
