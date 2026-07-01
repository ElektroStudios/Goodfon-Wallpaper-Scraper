
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

#Region " MenuItem Info "

' ReSharper disable once CheckNamespace

Namespace DevCase.Win32.Structures

    ''' <summary>
    ''' Contains information about a menu item.
    ''' </summary>
    '''
    ''' <remarks>
    ''' <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms647578%28v=vs.85%29.aspx"/>
    ''' </remarks>
    <DebuggerStepThrough>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure MenuItemInfo

#Region " Fields "

        ''' <summary>
        ''' The size of the structure, in bytes.
        ''' <para></para>
        ''' This member must be set to <c>Marshal.SizeOf(Of MenuItemInfo)</c> before calling any function. 
        ''' </summary>
        Public SizeOfStruct As Integer

        ''' <summary>
        ''' The members to be retrieved or set.
        ''' <para></para>
        ''' This member can be one or more of the <see cref="Enums.MenuMask"/> Enum values.
        ''' </summary>
        Public Mask As Integer

        ''' <summary>
        ''' The menu item type.
        ''' </summary>
        Public Type As MenuItemType

        ''' <summary>
        ''' The menu item state.
        ''' <para></para>
        ''' Set <see cref="Mask"/> to <see cref="Enums.MenuItemMask.State"/> to use <see cref="State"/>. 
        ''' </summary>
        Public State As MenuItemInfoStates

        ''' <summary>
        ''' An application-defined value that identifies the menu item.
        ''' <para></para>
        ''' Set <see cref="Mask"/> to <see cref="Enums.MenuItemMask.Id"/> to use <see cref="Id"/>. 
        ''' </summary>
        Public Id As Integer

        ''' <summary>
        ''' A handle to the drop-down menu or submenu associated with the menu item.
        ''' <para></para>
        ''' If the menu item is not an item that opens a drop-down menu or submenu, this member is <see cref="IntPtr.Zero"/>.
        ''' <para></para>
        ''' Set <see cref="Mask"/> to <see cref="Enums.MenuItemMask.SubMenu"/> to use <see cref="SubMenu"/>.
        ''' </summary>
        Public SubMenu As IntPtr

        ''' <summary>
        ''' A handle to the bitmap to display next to the item if it is selected.
        ''' <para></para>
        ''' If this member is <see cref="IntPtr.Zero"/>, a default bitmap is used.
        ''' <para></para>
        ''' If the <see cref="Enums.MenuItemType.Radiocheck"/> type value is specified, the default bitmap is a bullet,
        ''' Otherwise it is a check mark.
        ''' <para></para>
        ''' Set <see cref="Mask"/> to <see cref="Enums.MenuItemMask.Checkmarks"/> to use <see cref="BmpChecked"/>.
        ''' </summary>
        Public BmpChecked As IntPtr

        ''' <summary>
        ''' A handle to the bitmap to display next to the item if it is not selected.
        ''' <para></para>
        ''' If this member is <see cref="IntPtr.Zero"/>, no bitmap is used.
        ''' <para></para>
        ''' Set <see cref="Mask"/> to <see cref="Enums.MenuItemMask.Checkmarks"/> to use <see cref="BmpUnchecked"/>.
        ''' </summary>
        Public BmpUnchecked As IntPtr

        ''' <summary>
        ''' An application-defined value associated with the menu item.
        ''' <para></para>
        ''' Set <see cref="Mask"/> to <see cref="Enums.MenuItemMask.Data"/> to use <see cref="ItemData"/>.
        ''' </summary>
        Public ItemData As IntPtr

        ''' <summary>
        ''' The contents of the menu item.
        ''' <para></para>
        ''' The meaning of this member depends on the value of fType,
        ''' and is used only if the <see cref="Enums.MenuItemMask.Type"/> flag is set in the <see cref="Mask"/> member.
        ''' </summary>
        Public TypeData As String

        ''' <summary>
        ''' The length of the menu item text, in characters,
        ''' when information is received about a menu item of the "<see cref="Enums.MenuItemMask.Text"/> type.
        ''' <para></para>
        ''' However, <see cref="TextLen"/> is used only if the <see cref="Enums.MenuItemMask.Type"/> flag is set in the 
        ''' <see cref="Mask"/> member and is zero otherwise.
        ''' <para></para>
        ''' Also, <see cref="TextLen"/> is ignored when the content of a menu item is set by calling 
        ''' <see cref="NativeMethods.SetMenuItemInfo"/> function.
        ''' <para></para>
        ''' The <see cref="TextLen"/> member is used when 
        ''' the <see cref="Enums.MenuItemMask.Text"/> flag is set in the <see cref="Mask"/> member. 
        ''' </summary>
        Public TextLen As Integer

        ''' <summary>
        ''' A handle to the bitmap to be displayed.
        ''' <para></para>
        ''' It is used when the <see cref="Enums.MenuItemMask.Bitmap"/> flag is set in the <see cref="Mask"/> member. 
        ''' </summary>
        Public BmpItem As IntPtr

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Initializes a new instance of the <see cref="MenuItemInfo"/> structure.
        ''' </summary>
        '''
        ''' <param name="initializeSize">
        ''' Allows automatic initialization of <see cref="MenuItemInfo.SizeOfStruct"/> member.
        ''' </param>
        Public Sub New(initializeSize As Boolean)
            If initializeSize Then
                Me.SizeOfStruct = Marshal.SizeOf(GetType(MenuItemInfo))
            End If
        End Sub

#End Region

    End Structure

End Namespace

#End Region

#Enable Warning BC42309 ' XML comment has a tag with a 'cref' attribute that could not be resolved
