
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Option Statements "

Imports DevCase.Win32.Structures

#End Region

#Region " MenuItemInfoStates "

' ReSharper disable once CheckNamespace

Namespace DevCase.Win32.Enums

    ''' <summary>
    ''' Specifies a menu item state in <see cref="MenuItemInfo.State"/> field.
    ''' </summary>
    '''
    ''' <remarks>
    ''' <see href="https://learn.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-menuiteminfow"/>
    ''' </remarks>
    <Flags>
    Public Enum MenuItemInfoStates As Integer

        ''' <summary>
        ''' Enables the menu item so that it can be selected. This is the default state. 
        ''' </summary>
        Enabled_Unchecked_Unhighlighted = &H0L

        ''' <summary>
        ''' Disables the menu item and grays it so that it cannot be selected. 
        ''' </summary>
        Disabled_Greyed = &H3L

        ''' <summary>
        ''' Checks the menu item.
        ''' </summary>
        Checked = &H8L

        ''' <summary>
        ''' Highlights the menu item. 
        ''' </summary>
        HiLite = &H80L

        ''' <summary>
        ''' Specifies that the menu item is the default. A menu can contain only one default menu item, which is displayed in bold. 
        ''' </summary>
        [Default] = &H1000L

    End Enum

End Namespace

#End Region
