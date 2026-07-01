
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " MenuItem Type "

' ReSharper disable once CheckNamespace

Namespace DevCase.Win32.Enums

    ''' <summary>
    ''' Specifies a menu-item type for the <see cref="Structures.MenuItemInfo.Type"/> field.
    ''' </summary>
    '''
    ''' <remarks>
    ''' <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms647578(v=vs.85).aspx"/>
    ''' </remarks>
    <Flags>
    Public Enum MenuItemType As Integer

        ' ***************************************************************************************
        '  ⚠️                          IMPORTANT NOTICE
        '
        '  THIS ENUMERATION IS PARTIALLY DEFINED FOR THE SPECIFIC PURPOSES OF THE GOODFON SCRAPER
        ' ***************************************************************************************

        ''' <summary>
        ''' Specifies that the menu item is a separator.
        ''' <para></para>
        ''' A menu item separator appears as a horizontal dividing line. 
        ''' <para></para>
        ''' The <see cref="Structures.MenuItemInfo.TypeData"/> and cch members are ignored. 
        ''' <para></para>
        ''' This value is valid only in a drop-down menu, submenu, or shortcut menu.
        ''' </summary>
        Separator = &H800L

    End Enum

End Namespace

#End Region
