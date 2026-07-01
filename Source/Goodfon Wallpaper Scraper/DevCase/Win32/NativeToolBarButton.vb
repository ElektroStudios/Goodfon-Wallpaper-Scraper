
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Diagnostics
Imports System.Drawing
Imports System.Runtime.InteropServices

#End Region

#Region " NativeToolBarButton "

' ReSharper disable once CheckNamespace

Namespace DevCase.Win32.Structures

    ''' <summary>
    ''' Provides information about a button in a Win32 toolbar control (a window with "ToolbarWin32" class name).
    ''' </summary>
    '''
    ''' <remarks>
    ''' <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/bb760476%28v=vs.85%29.aspx"/>
    ''' <para></para>
    ''' <see href="https://docs.microsoft.com/en-us/windows/desktop/controls/toolbar-controls-overview"/>
    ''' </remarks>
    <DebuggerStepThrough>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure NativeToolBarButton

#Region " Fields "

        ''' <summary>
        ''' Gets the process identifier (PID) of the process that owns the toolbar of this button.
        ''' </summary>
        Public ProcessId As Integer

        ''' <summary>
        ''' Gets a handle to the toolbar window that owns this button.
        ''' </summary>
        Public ToolbarHandle As IntPtr

        ''' <summary>
        ''' Gets a handle to the HIMAGELIST associated with the toolbar of this button.
        ''' </summary>
        Public ImageListHandle As IntPtr

        ''' <summary>
        ''' Gets the zero-based index of the button.
        ''' </summary>
        Public ButtonIndex As Integer

        ''' <summary>
        ''' Gets the zero-based index of the button image in the imagelist.
        ''' </summary>
        Public BitmapIndex As Integer

        ''' <summary>
        ''' Gets the command identifier associated with the button.
        ''' </summary>
        Public CommandId As Integer

        ''' <summary>
        ''' Gets the state flags of the button.
        ''' </summary>
        Public State As Byte ' ToolbarButtonStates

        ''' <summary>
        ''' Gets the style flags of the button.
        ''' </summary>
        Public Style As Byte

        ''' <summary>
        ''' Gets the tooltip text of the button.
        ''' </summary>
        Public ToolTipText As String

        ''' <summary>
        ''' Gets the bounds of the button.
        ''' </summary>
        Public Bounds As NativeRectangle

        ''' <summary>
        ''' Gets the icon.
        ''' </summary>
        Public Icon As Icon

        ''' <summary>
        ''' Gets a handle to the icon.
        ''' </summary>
        Public IconHandle As IntPtr

#End Region

#Region " Public Methods "

        ''' <summary>
        ''' Deletes the button.
        ''' </summary>
        <DebuggerStepThrough>
        Public Sub Delete()
            ' ⚠️ NOT IMPLEMENTED FOR GOODFON SCRAPER ASSEMBLY
        End Sub

#End Region

#Region " Private Methods "

        ''' <summary>
        ''' Change the state of the button.
        ''' </summary>
        <DebuggerStepThrough>
        Public Sub ChangeState(state As Byte) ' ToolbarButtonStates
            ' ⚠️ NOT IMPLEMENTED FOR GOODFON SCRAPER ASSEMBLY
        End Sub

#End Region

    End Structure

End Namespace

#End Region
