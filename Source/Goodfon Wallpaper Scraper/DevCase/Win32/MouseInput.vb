
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

#Region " Mouse Input "

' ReSharper disable once CheckNamespace

Namespace DevCase.Win32.Structures

    ''' <summary>
    ''' Contains information about a simulated mouse event.
    ''' </summary>
    '''
    ''' <remarks>
    ''' <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms646273%28v=vs.85%29.aspx"/>
    ''' </remarks>
    <DebuggerStepThrough>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure MouseInput

        ''' <summary>
        ''' The absolute position of the mouse,
        ''' or the amount of motion since the last mouse event was generated,
        ''' depending on the value of the flags member.
        ''' <para></para>
        ''' Absolute data is specified as the 'x' coordinate of the mouse;
        ''' relative data is specified as the number of pixels moved.
        ''' </summary>
        Public X As Integer

        ''' <summary>
        ''' The absolute position of the mouse,
        ''' or the amount of motion since the last mouse event was generated,
        ''' depending on the value of the flags member.
        ''' <para></para>
        ''' Absolute data is specified as the 'y' coordinate of the mouse;
        ''' relative data is specified as the number of pixels moved.
        ''' </summary>
        Public Y As Integer

        ''' <summary>
        ''' If <see cref="MouseInput.Flags"/> contains <see cref="MouseInputFlags.Wheel"/>,
        ''' then <see cref="MouseInput.MouseData"/> specifies the amount of wheel movement.
        ''' <para></para>
        ''' A positive value indicates that the wheel was rotated forward, away from the user.
        ''' <para></para>
        ''' a negative value indicates that the wheel was rotated backward, toward the user.
        ''' One wheel click is defined as <c>WHEEL_DELTA</c>, which is <c>120</c>.
        '''
        ''' If <see cref="MouseInput.Flags"/> does not contain <see cref="MouseInputFlags.Wheel"/>, 
        ''' <see cref="MouseInputFlags.XDown"/>, or <see cref="MouseInputFlags.XUp"/>,
        ''' then <see cref="MouseInput.MouseData"/> should be <c>0</c>.
        ''' </summary>
        Public MouseData As Integer

        ''' <summary>
        ''' A set of bit flags that specify various aspects of mouse motion and button clicks.
        ''' <para></para>
        ''' The bit flags that specify mouse button status are set to indicate changes in status,
        ''' not ongoing conditions.
        ''' <para></para>
        ''' For example, if the left mouse button is pressed and held down,
        ''' <see cref="MouseInputFlags.LeftDown"/> is set when the left button is first pressed,
        ''' but not for subsequent motions.
        ''' <para></para>
        ''' Similarly, <see cref="MouseInputFlags.LeftUp"/> is set only when the button is first released.
        ''' <para></para>
        ''' You cannot specify both the <see cref="MouseInputFlags.Wheel"/> flag
        ''' and either <see cref="MouseInputFlags.XDown"/> or <see cref="MouseInputFlags.XUp"/> flags simultaneously in the 
        ''' <see cref="MouseInput.Flags"/> parameter,
        ''' because they both require use of the 'mouseData' field.
        ''' </summary>
        Public Flags As MouseInputFlags

        ''' <summary>
        ''' The time stamp for the event, in milliseconds.
        ''' <para></para>
        ''' If this parameter is <c>0</c>, the system will provide its own time stamp.
        ''' </summary>
        Public Time As Integer

        ''' <summary>
        ''' An additional value associated with the mouse event.
        ''' <para></para>
        ''' An application calls <see cref="NativeMethods.GetMessageExtraInfo"/> to obtain this extra information.
        ''' </summary>
        Public ExtraInfo As IntPtr

    End Structure

End Namespace

#End Region

#Enable Warning BC42309 ' XML comment has a tag with a 'cref' attribute that could not be resolved
