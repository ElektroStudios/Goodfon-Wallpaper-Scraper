
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Virtual Keys "

' ReSharper disable once CheckNamespace

Namespace DevCase.Win32.Enums

    ''' <summary>
    ''' Virtual-key codes.
    ''' </summary>
    '''
    ''' <remarks>
    ''' <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/dd375731%28v=vs.85%29.aspx"/>
    ''' </remarks>
    Public Enum VirtualKeys As Integer

        ' ***************************************************************************************
        '  ⚠️                          IMPORTANT NOTICE
        '
        '  THIS ENUMERATION IS PARTIALLY DEFINED FOR THE SPECIFIC PURPOSES OF THE GOODFON SCRAPER
        ' ***************************************************************************************

        ''' <summary>
        ''' Specify none virtual-key.
        ''' </summary>
        None = &H0

        ''' <summary>
        ''' VK_RETURN
        ''' <para></para>
        ''' ENTER key.
        ''' </summary>
        [Return] = &HD

        ''' <summary>
        ''' VK_DOWN
        ''' <para></para>
        ''' DOWN ARROW key.
        ''' </summary>
        Down = &H28

    End Enum

End Namespace

#End Region
