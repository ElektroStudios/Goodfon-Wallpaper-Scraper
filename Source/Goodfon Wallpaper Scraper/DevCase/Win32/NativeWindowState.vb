
#Disable Warning BC42309 ' XML comment has a tag with a 'cref' attribute that could not be resolved

#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Window State "

' ReSharper disable once CheckNamespace

Namespace DevCase.Win32.Enums

    ''' <summary>
    ''' Controls how a window is to be shown.
    ''' </summary>
    '''
    ''' <remarks>
    ''' <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms633548%28v=vs.85%29.aspx"/>
    ''' </remarks>
    <Flags>
    Public Enum NativeWindowState As Integer

        ' ***************************************************************************************
        '  ⚠️                          IMPORTANT NOTICE
        '
        '  THIS ENUMERATION IS PARTIALLY DEFINED FOR THE SPECIFIC PURPOSES OF THE GOODFON SCRAPER
        ' ***************************************************************************************

        ''' <summary>
        ''' Displays a window in its most recent size and position. 
        ''' This value is similar to <see cref="NativeWindowState.Normal"/>, except the window is not actived.
        ''' </summary>
        ShowNoActivate = 4

        ''' <summary>
        ''' Activates the window and displays it in its current size and position.
        ''' </summary>
        Show = 5

        ''' <summary>
        ''' Displays the window as a minimized window. 
        ''' This value is similar to <see cref="NativeWindowState.ShowMinimized"/>, except the window is not activated.
        ''' </summary>
        ShowMinNoActive = 7

        ''' <summary>
        ''' Activates and displays the window. 
        ''' If the window is minimized or maximized, the system restores it to its original size and position.
        ''' An application should specify this flag when restoring a minimized window.
        ''' </summary>
        Restore = 9

    End Enum

End Namespace

#End Region

#Enable Warning BC42309 ' XML comment has a tag with a 'cref' attribute that could not be resolved
