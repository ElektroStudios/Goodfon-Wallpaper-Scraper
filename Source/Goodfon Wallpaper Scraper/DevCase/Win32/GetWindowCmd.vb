
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " GetWindow Cmd "

' ReSharper disable once CheckNamespace

Namespace DevCase.Win32.Enums

    ''' <summary>
    ''' The relationship between the specified window and the window whose handle is to be retrieved.
    ''' <para></para>
    ''' <c>gwCmd</c> parameter of 
    ''' <see cref="NativeMethods.GetWindow"/> function.
    ''' </summary>
    '''
    ''' <remarks>
    ''' <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms633515%28v=vs.85%29.aspx"/>
    ''' </remarks>
    Public Enum GetWindowCmd As Integer

        ' ***************************************************************************************
        '  ⚠️                          IMPORTANT NOTICE
        '
        '  THIS ENUMERATION IS PARTIALLY DEFINED FOR THE SPECIFIC PURPOSES OF THE GOODFON SCRAPER
        ' ***************************************************************************************

        ''' <summary>
        ''' The retrieved handle identifies the specified window's owner window, if any.
        ''' <para></para>
        ''' For more information, 
        ''' see <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms632599(v=vs.85).aspx#owned_windows"/> 
        ''' </summary>
        Owner = 4

    End Enum

End Namespace

#End Region
