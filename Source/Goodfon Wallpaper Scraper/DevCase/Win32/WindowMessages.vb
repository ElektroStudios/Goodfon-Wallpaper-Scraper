
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Window Messages "

' ReSharper disable once CheckNamespace

Namespace DevCase.Win32.Enums

    ''' <summary>
    ''' Defines window messages.
    ''' <para></para>
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
    ''' <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms644927%28v=vs.85%29.aspx"/>
    ''' </remarks>
    Public Enum WindowMessages As Integer

        ' ReSharper disable InconsistentNaming

        ' ***************************************************************************************
        '  ⚠️                          IMPORTANT NOTICE
        '
        '  THIS ENUMERATION IS PARTIALLY DEFINED FOR THE SPECIFIC PURPOSES OF THE GOODFON SCRAPER
        ' ***************************************************************************************

        ''' <summary>
        ''' The <see cref="WindowMessages.Null"/> message performs no operation.
        ''' <para></para>
        ''' An application sends the <see cref="WindowMessages.Null"/> message if it wants to 
        ''' send a message that the recipient window will ignore.
        ''' </summary>
        Null = &H0

        ''' <summary>
        ''' Message sent when the user selects a command item from a menu,
        ''' when a control sends a notification message to its parent window,
        ''' or when an accelerator keystroke is translated.
        ''' <para></para>
        ''' 
        ''' <c>wParam</c> 
        ''' For a description of this parameter, see Remarks.
        ''' <para></para>
        ''' 
        ''' <c>lParam</c> 
        ''' For a description of this parameter, see Remarks.
        ''' </summary>
        '''
        ''' <remarks>
        ''' <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms647591%28v=vs.85%29.aspx"/>
        ''' </remarks>
        WM_Command = &H111

        ''' <summary>
        ''' The message is posted to the window with the keyboard focus when a nonsystem key is pressed.
        ''' <para></para>
        ''' A nonsystem key is a key that is pressed when the <c>ALT</c> key is not pressed. 
        ''' <para></para>
        ''' 
        ''' <c>wParam</c> 
        ''' The virtual-key code of the nonsystem key.
        ''' <para></para>
        ''' 
        ''' <c>lParam</c> 
        ''' The repeat count, scan code, extended-key flag, context code, 
        ''' previous key-state flag, and transition-state flag, as shown following:
        ''' <para></para>
        ''' Bit 0-15: The repeat count for the current message. 
        ''' <para></para>
        ''' The value is the number of times the keystroke is autorepeated as a result of the user holding down the key. 
        ''' <para></para>
        ''' If the keystroke is held long enough, multiple messages are sent. However, the repeat count is not cumulative
        ''' <para></para>
        ''' Bit 16-23: The scan code. The value depends on the OEM.
        ''' <para></para>
        ''' Bit 24: Indicates whether the key is an extended key, 
        ''' such as the right-hand ALT and CTRL keys that appear on an enhanced 101- or 102-key keyboard. 
        ''' <para></para>
        ''' The value is 1 if it is an extended key; otherwise, it is 0.
        ''' <para></para>
        ''' Bit 25-28: Reserved; do not use
        ''' <para></para>
        ''' Bit 29: The context code. 
        ''' <para></para>
        ''' The value is always 0 for a <see cref="WindowMessages.WM_KeyDown"/> message.
        ''' <para></para>
        ''' Bit 30: The previous key state. 
        ''' <para></para>
        ''' The value is 1 if the key is down before the message is sent, or it is zero if the key is up.
        ''' <para></para>
        ''' Bit 31: The transition state. 
        ''' <para></para>
        ''' The value is always 0 for a <see cref="WindowMessages.WM_KeyDown"/> message.
        ''' </summary>
        '''
        ''' <remarks>
        ''' <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms646280(v=vs.85).aspx"/>
        ''' </remarks>
        WM_KeyDown = &H100

        ''' <summary>
        ''' The message is posted when the user releases the left mouse button while the cursor is in the 
        ''' client area of a window. 
        ''' <para></para>
        ''' If the mouse is not captured the message is posted to the window beneath the cursor. 
        ''' <para></para>
        ''' Otherwise the message is posted to the window that has captured the mouse.
        ''' </summary>
        '''
        ''' <remarks>
        ''' <see href=""/>
        ''' </remarks>
        WM_LButtonUp = &H202

        ''' <summary>
        ''' The message is posted when the user releases the middle mouse button while the cursor is in the 
        ''' client area of a window. 
        ''' <para></para>
        ''' If the mouse is not captured the message is posted to the window beneath the cursor. 
        ''' <para></para>
        ''' Otherwise the message is posted to the window that has captured the mouse.
        ''' </summary>
        '''
        ''' <remarks>
        ''' <see href=""/>
        ''' </remarks>
        WM_MButtonUp = &H208

        ''' <summary>
        ''' The message is posted when the user releases the right mouse button while the cursor is in the 
        ''' client area of a window.
        ''' <para></para>
        ''' If the mouse is not captured the message is posted to the window beneath the cursor.
        ''' <para></para>
        ''' Otherwise the message is posted to the window that has captured the mouse.
        ''' </summary>
        '''
        ''' <remarks>
        ''' <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms646243(v=vs.85).aspx"/>
        ''' </remarks>
        WM_RButtonUp = &H205

        ''' <summary>
        ''' Used to define private messages for use by private window classes, 
        ''' usually of the form '<see cref="WindowMessages.WM_User"/> + x', where 'x' is an integer value.
        ''' </summary>
        '''
        ''' <remarks>
        ''' <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms644931%28v=vs.85%29.aspx"/>
        ''' </remarks>
        WM_User = &H400

    End Enum

End Namespace

#End Region
