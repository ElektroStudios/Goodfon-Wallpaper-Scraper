
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " MouseButtonMessage "

' ReSharper disable once CheckNamespace

Namespace DevCase.Core.IO.Devices.Input

    ''' <summary>
    ''' Defines a button message to send to a window.
    ''' </summary>
    Public Enum MouseButtonMessage As Integer

        ''' <summary>
        ''' Hold down the left mouse button.
        ''' </summary>
        LeftDown

        ''' <summary>
        ''' Release up the left mouse button.
        ''' </summary>
        LeftUp

        ''' <summary>
        ''' Hold down the right mouse button.
        ''' </summary>
        RightDown

        ''' <summary>
        ''' Release up the right mouse button.
        ''' </summary>
        RightUp

        ''' <summary>
        ''' Hold down the middle mouse button.
        ''' </summary>
        MiddleDown

        ''' <summary>
        ''' Release up the middle mouse button.
        ''' </summary>
        MiddleUp

        ''' <summary>
        ''' Move down the mouse wheel.
        ''' </summary>
        WheelDown

        ''' <summary>
        ''' Move up the mouse wheel.
        ''' </summary>
        WheelUp

        ''' <summary>
        ''' Tilt horizontal mouse wheel to left.
        ''' </summary>
        WheelLeft

        ''' <summary>
        ''' Tilt horizontal mouse wheel to right.
        ''' </summary>
        WheelRight

    End Enum

End Namespace

#End Region
