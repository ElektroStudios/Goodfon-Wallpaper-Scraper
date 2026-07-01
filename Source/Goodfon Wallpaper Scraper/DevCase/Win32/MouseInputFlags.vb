
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " MouseInput Flags "

' ReSharper disable once CheckNamespace

Namespace DevCase.Win32.Enums

    ''' <summary>
    ''' A set of bit flags that specify various aspects of mouse motion and button clicks.
    ''' <para></para>
    ''' Flags combination for <see cref="Structures.MouseInput.Flags"/> field.
    ''' </summary>
    '''
    ''' <remarks>
    ''' <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms646273%28v=vs.85%29.aspx"/>
    ''' </remarks>
    <Flags>
    Public Enum MouseInputFlags As Integer

        ' ***************************************************************************************
        '  ⚠️                          IMPORTANT NOTICE
        '
        '  THIS ENUMERATION IS PARTIALLY DEFINED FOR THE SPECIFIC PURPOSES OF THE GOODFON SCRAPER
        ' ***************************************************************************************

        ''' <summary>
        ''' The <see cref="Structures.MouseInput.X"/> and <see cref="Structures.MouseInput.Y"/> members 
        ''' contain normalized absolute coordinates.
        ''' <para></para>
        ''' If the flag is not set, 
        ''' <see cref="Structures.MouseInput.X"/> and <see cref="Structures.MouseInput.Y"/> contain relative data 
        ''' (the change in position since the last reported position).
        ''' <para></para>
        ''' This flag can be set, or not set,
        ''' regardless of what kind of mouse or other pointing device, if any, is connected to the system.
        ''' </summary>
        Absolute = &H8000

        ''' <summary>
        ''' The left button was pressed.
        ''' </summary>
        LeftDown = &H2

        ''' <summary>
        ''' The left button was released.
        ''' </summary>
        LeftUp = &H4

        ''' <summary>
        ''' The right button was pressed.
        ''' </summary>
        RightDown = &H8

        ''' <summary>
        ''' The right button was released.
        ''' </summary>
        RightUp = &H10

        ''' <summary>
        ''' The middle button was pressed.
        ''' </summary>
        MiddleDown = &H20

        ''' <summary>
        ''' The middle button was released.
        ''' </summary>
        MiddleUp = &H40

        ''' <summary>
        ''' The wheel was moved, if the mouse has a wheel.
        ''' <para></para>
        ''' The amount of movement is specified in <see cref="Structures.MouseInput.MouseData"/>.
        ''' </summary>
        Wheel = &H800I

        ''' <summary>
        ''' The wheel was moved horizontally, if the mouse has a wheel.
        ''' <para></para>
        ''' The amount of movement is specified in <see cref="Structures.MouseInput.MouseData"/>.
        ''' </summary>
        HWheel = &H1000I

    End Enum

End Namespace

#End Region
