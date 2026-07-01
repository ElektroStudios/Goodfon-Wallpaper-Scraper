
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " MenuItem Mask "

' ReSharper disable once CheckNamespace

Namespace DevCase.Win32.Enums

    ''' <summary>
    ''' Specifies a mask type for the <see cref="Structures.MenuItemInfo.Mask"/> field.
    ''' </summary>
    '''
    ''' <remarks>
    ''' <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms647578(v=vs.85).aspx"/>
    ''' </remarks>
    <Flags>
    Public Enum MenuItemMask As Integer

        ' ***************************************************************************************
        '  ⚠️                          IMPORTANT NOTICE
        '
        '  THIS ENUMERATION IS PARTIALLY DEFINED FOR THE SPECIFIC PURPOSES OF THE GOODFON SCRAPER
        ' ***************************************************************************************

        ''' <summary>
        ''' Retrieves or sets the <see cref="Structures.MenuItemInfo.State"/> member.
        ''' </summary>
        State = &H1

    End Enum

End Namespace

#End Region
