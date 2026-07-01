
#Disable Warning BC42309 ' XML comment has a tag with a 'cref' attribute that could not be resolved

#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports DevCase.Win32.Structures

#End Region

#Region " Toolbar Messages "

' ReSharper disable once CheckNamespace

Namespace DevCase.Win32.Enums

    ''' <summary>
    ''' The system sends or posts a system-defined message when it communicates with an application. 
    ''' <para></para>
    ''' It uses these messages to control the operations of applications and to provide input and other information for applications to process. 
    ''' <para></para>
    ''' An application can also send or post system-defined messages.
    ''' <para></para>
    ''' Applications generally use these messages to control the operation of control windows created by using preregistered window classes.
    ''' </summary>
    '''
    ''' <remarks>
    ''' <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/ff486063%28v=vs.85%29.aspx"/>
    ''' <para></para>
    ''' The definitions can be found in the Windows SDK file: CommCtrl.h
    ''' </remarks>
    Public Enum ToolbarMessages As Integer

        ' ***************************************************************************************
        '  ⚠️                          IMPORTANT NOTICE
        '
        '  THIS ENUMERATION IS PARTIALLY DEFINED FOR THE SPECIFIC PURPOSES OF THE GOODFON SCRAPER
        ' ***************************************************************************************

        ''' <summary>
        ''' The <see cref="Null"/> message performs no operation.
        ''' <para></para>
        ''' An application sends the <see cref="Null"/> message if it wants to 
        ''' post a message that the recipient window will ignore.
        ''' </summary>
        Null = &H0

        ''' <summary>
        ''' Retrieves a count of the buttons currently in a ToolBar control.
        ''' <para></para>
        ''' 
        ''' <c>wParam</c> 
        ''' Must be zero.
        ''' <para></para>
        ''' 
        ''' <c>lParam</c> 
        ''' Must be zero.
        ''' <para></para>
        ''' 
        ''' Returns:
        ''' The count of the buttons
        ''' </summary>
        '''
        ''' <remarks>
        ''' <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/bb787297%28v=vs.85%29.aspx"/>
        ''' </remarks>
        ButtonCount = WindowMessages.WM_User + 24

        ''' <summary>
        ''' Retrieves the zero-based index for the button 
        ''' associated with the specified command identifier on a ToolBar control.
        ''' <para></para>
        ''' 
        ''' <c>wParam</c> 
        ''' Command identifier associated with the button.
        ''' <para></para>
        ''' 
        ''' <c>lParam</c> 
        ''' Must be zero.
        ''' <para></para>
        ''' 
        ''' Returns:
        ''' The zero-based index for the button or <c>-1</c> if the specified command identifier is invalid.
        ''' </summary>
        '''
        ''' <remarks>
        ''' <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/bb787305%28v=vs.85%29.aspx"/>
        ''' </remarks>
        CommandIdToIndex = WindowMessages.WM_User + 25

        ''' <summary>
        ''' Retrieves information about the specified button in a ToolBar control.
        ''' <para></para>
        ''' 
        ''' <c>wParam</c> 
        ''' Zero-based index of the button for which to retrieve information.
        ''' <para></para>
        ''' 
        ''' <c>lParam</c> 
        ''' A <see cref="Structures.ToolbarButton"/> structure that receives the button information.
        ''' <para></para>
        ''' 
        ''' Returns:
        ''' <see langword="True"/> if successful, <see langword="False"/> otherwise.
        ''' </summary>
        '''
        ''' <remarks>
        ''' <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/bb787319%28v=vs.85%29.aspx"/>
        ''' </remarks>
        GetButton = WindowMessages.WM_User + 23

        ''' <summary>
        ''' Retrieves extended information for a button in a ToolBar control.
        ''' <para></para>
        ''' 
        ''' <c>wParam</c> 
        ''' Command identifier of the button.
        ''' <para></para>
        ''' 
        ''' <c>lParam</c> 
        ''' A <see cref="ToolBarButtonInfoW"/> structure 
        ''' that receives the button information.
        ''' <para></para>
        ''' The <see cref="ToolBarButtonInfoW.SizeOfStruct"/> and <see cref="ToolBarButtonInfoW.Mask"/> members 
        ''' of this structure must be filled in prior to sending this message. 
        ''' <para></para>
        ''' 
        ''' Returns:
        ''' The zero-based index of the button, or <c>-1</c> if an error occurs. 
        ''' </summary>
        '''
        ''' <remarks>
        ''' <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/bb787321%28v=vs.85%29.aspx"/>
        ''' </remarks>
        GetButtonInfoUnicode = WindowMessages.WM_User + 63

        ''' <summary>
        ''' Retrieves the display text (in <c>Unicode</c>) of a button on a ToolBar control.
        ''' <para></para>
        ''' 
        ''' <c>wParam</c> 
        ''' Command identifier of the button whose text is to be retrieved.
        ''' <para></para>
        ''' 
        ''' <c>lParam</c> 
        ''' A buffer that receives the button text.
        ''' <para></para>
        ''' 
        ''' Returns:
        ''' The length, in characters, of the string pointed to by lParam. 
        ''' The length does not include the terminating null character. 
        ''' If unsuccessful, the return value is <c>-1</c>.
        ''' </summary>
        '''
        ''' <remarks>
        ''' <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/bb787325%28v=vs.85%29.aspx"/>
        ''' </remarks>
        GetButtonTextUnicode = WindowMessages.WM_User + 75

        ''' <summary>
        ''' Retrieves the bounding rectangle for a specified toolbar button.
        ''' <para></para>
        ''' 
        ''' <c>wParam</c> 
        ''' Command identifier of the button.
        ''' <para></para>
        ''' 
        ''' <c>lParam</c> 
        ''' Pointer to a <see cref="NativeRectangle"/> structure that will receive the bounding rectangle information.
        ''' <para></para>
        ''' 
        ''' Returns:
        ''' Returns <see langword="True"/> if successful, or <see langword="False"/> otherwise.
        ''' </summary>
        '''
        ''' <remarks>
        ''' <see href="https://docs.microsoft.com/en-us/windows/desktop/controls/tb-getrect"/>
        ''' </remarks>
        GetRect = WindowMessages.WM_User + 51

        ''' <summary>
        ''' Retrieves the image list that a toolbar control uses to display buttons in their default state. 
        ''' <para></para>
        ''' A toolbar control uses this image list to display buttons when they are not hot or disabled.
        ''' <para></para>
        ''' 
        ''' <c>wParam</c> 
        ''' Must be zero.
        ''' <para></para>
        ''' 
        ''' <c>lParam</c> 
        ''' Must be zero.
        ''' <para></para>
        ''' 
        ''' Returns:
        ''' Returns the handle to the image list, or <see cref="IntPtr.Zero"/> if no image list is set.
        ''' </summary>
        '''
        ''' <remarks>
        ''' <see href="https://docs.microsoft.com/en-us/windows/desktop/controls/tb-getimagelist"/>
        ''' </remarks>
        GetImageList = WindowMessages.WM_User + 49

    End Enum

End Namespace

#End Region

#Enable Warning BC42309 ' XML comment has a tag with a 'cref' attribute that could not be resolved
