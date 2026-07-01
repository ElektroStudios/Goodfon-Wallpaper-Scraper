
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Diagnostics
Imports System.Runtime.InteropServices

#End Region

#Region " ToolBar Button "

' ReSharper disable once CheckNamespace

Namespace DevCase.Win32.Structures

    ' C# definition: https://stackoverflow.com/a/5518286/1248295

    ''' <summary>
    ''' Contains information about a button in a ToolBar control.
    ''' </summary>
    '''
    ''' <remarks>
    ''' <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/bb760476%28v=vs.85%29.aspx"/>
    ''' </remarks>
    <DebuggerStepThrough>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure ToolbarButton

        ''' <summary>
        ''' Zero-based index of the button image. 
        ''' <para></para>
        ''' Set this member to <c>I_IMAGECALLBACK</c>, 
        ''' and the toolbar will send the <c>TBN_GETDISPINFO</c> notification code 
        ''' to retrieve the image index when it is needed.
        ''' </summary>
        Public BitmapIndex As Integer

        ''' <summary>
        ''' Command identifier associated with the button.
        ''' </summary>
        Public CommandId As Integer

        <StructLayout(LayoutKind.Explicit)>
        Private Structure TBBUTTON_U
            ''' <summary>
            ''' State flags of the button.
            ''' </summary>
            <FieldOffset(0)>
            Public State As Byte ' ToolbarButtonStates

            ''' <summary>
            ''' Style flags of the button.
            ''' </summary>
            <FieldOffset(1)>
            Public Style As Byte

            ''' <summary>
            ''' Reserved value by the system.
            ''' </summary>
            <FieldOffset(0)>
            Private ReadOnly Reserved As IntPtr
        End Structure

        Private union As TBBUTTON_U

        ''' <summary>
        ''' State flags of the button.
        ''' </summary>
        Public Property State As Byte ' ToolbarButtonStates
            Get
                Return Me.union.State
            End Get
            Set(value As Byte) ' ToolbarButtonStates
                Me.union.State = value
            End Set
        End Property

        ''' <summary>
        ''' Style flags of the button.
        ''' </summary>
        Public Property Style As Byte
            Get
                Return Me.union.Style
            End Get
            Set(value As Byte)
                Me.union.Style = value
            End Set
        End Property

        ''' <summary>
        ''' Application-defined value.
        ''' </summary>
        Public Data As UIntPtr

        ''' <summary>
        ''' Zero-based index of the button string, 
        ''' or a pointer to a string buffer that contains text for the button.
        ''' <para></para>
        ''' The <see cref="ToolbarButton.TextOrIndex"/> member can return either a string pointer or an index.
        ''' </summary>
        <MarshalAs(UnmanagedType.SysInt)>
        Public TextOrIndex As IntPtr

    End Structure

End Namespace

#End Region
