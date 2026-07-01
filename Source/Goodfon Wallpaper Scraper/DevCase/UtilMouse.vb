
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Diagnostics
Imports System.Drawing
Imports System.Linq
Imports System.Runtime.InteropServices

Imports DevCase.Win32
Imports DevCase.Win32.Enums

#End Region

#Region " Mouse Util "

' ReSharper disable once CheckNamespace

Namespace DevCase.Core.IO.Devices.Input

    ''' <summary>
    ''' Contains mouse related utilities.
    ''' </summary>
    Public NotInheritable Class UtilMouse

#Region " Constructors "

        ''' <summary>
        ''' Prevents a default instance of the <see cref="UtilMouse"/> class from being created.
        ''' </summary>
        <DebuggerNonUserCode>
        Private Sub New()
        End Sub

#End Region

#Region " Properties "

        ''' <summary>
        ''' Gets or sets the mouse position on the screen.
        ''' </summary>
        '''
        ''' <example> This is a code example.
        ''' <code language="VB.NET">
        ''' Position = New Point(x:=100, y:=500)
        ''' </code>
        ''' </example>
        '''
        ''' <value>
        ''' The mouse position.
        ''' </value>
        Public Shared Property Position As Point
            <DebuggerStepThrough>
            Get
                Return Global.System.Windows.Forms.Cursor.Position
            End Get
            <DebuggerStepThrough>
            Set(value As Point)
                System.Windows.Forms.Cursor.Position = value
                '' DEPRECATED CODE FOR BEING NOT NECESSARY AT ALL:
                ' Mouse.Internal_MousePosition(value)
            End Set
        End Property

#End Region

#Region " Public Methods "

        ''' <summary>
        ''' Sends a sequence of mouse button messages to the active (foreground) window, 
        ''' by calling <see cref="Win32.NativeMethods.SendInput"/> function.
        ''' </summary>
        '''
        ''' <example> This is a code example.
        ''' <code language="VB.NET">
        ''' ' Does a left button click.
        ''' SendMouseInput(MouseButtonMessage.LeftDown, MouseButtonMessage.LeftUp)
        ''' </code>
        ''' </example>
        '''
        ''' <param name="messages">
        ''' The mouse button messages to send.
        ''' </param>
        '''
        ''' <returns>
        ''' The function returns the number of events that it successfully inserted into the mouse input stream.
        ''' <para></para>
        ''' If the function returns zero, the input was already blocked by another thread.
        ''' </returns>
        <DebuggerStepThrough>
        Public Shared Function SendMouseInput(ParamArray messages As MouseButtonMessage()) As Integer

            If Not messages?.Any() Then
                Throw New ArgumentNullException(paramName:=NameOf(messages))
            End If

            Dim inputs As New List(Of Structures.Input)

            For Each msg As MouseButtonMessage In messages

                Dim flags As MouseInputFlags
                Dim mouseData As Integer

                Select Case msg
                    Case MouseButtonMessage.LeftDown
                        flags = MouseInputFlags.LeftDown

                    Case MouseButtonMessage.LeftUp
                        flags = MouseInputFlags.LeftUp

                    Case MouseButtonMessage.RightDown
                        flags = MouseInputFlags.RightDown

                    Case MouseButtonMessage.RightUp
                        flags = MouseInputFlags.RightUp

                    Case MouseButtonMessage.MiddleDown
                        flags = MouseInputFlags.MiddleDown

                    Case MouseButtonMessage.MiddleUp
                        flags = MouseInputFlags.MiddleUp

                    Case MouseButtonMessage.WheelDown
                        flags = MouseInputFlags.Wheel
                        mouseData = -120

                    Case MouseButtonMessage.WheelUp
                        flags = MouseInputFlags.Wheel
                        mouseData = 120

                    Case MouseButtonMessage.WheelLeft
                        flags = MouseInputFlags.HWheel
                        mouseData = -120

                    Case MouseButtonMessage.WheelRight
                        flags = MouseInputFlags.HWheel
                        mouseData = 120

                    Case Else
                        Throw New InvalidEnumArgumentException(NameOf(messages), msg, GetType(MouseButtonMessage))
                End Select

                ' Add the mouse button input.
                Dim currentInput As New Structures.Input
                With currentInput
                    .Type = InputType.Mouse
                    .InputInfo.MouseInput.Flags = flags Or MouseInputFlags.Absolute
                    .InputInfo.MouseInput.MouseData = mouseData
                End With

                inputs.Add(currentInput)
            Next msg

            ' Send the mouse inputs.
            Return NativeMethods.SendInput(inputs.Count, inputs.ToArray(), Marshal.SizeOf(GetType(Structures.Input)))

        End Function

        ''' <summary>
        ''' Sends a mouse click to the active (foreground) window,
        ''' by calling <see cref="Win32.NativeMethods.SendInput"/> function.
        ''' </summary>
        '''
        ''' <example> This is a code example.
        ''' <code language="VB.NET">
        ''' SendMouseInput(MouseButtonClick.LeftButton)
        ''' </code>
        ''' </example>
        '''
        ''' <param name="button">
        ''' The mouse button click to send.
        ''' </param>
        '''
        ''' <returns>
        ''' The function returns the number of events that it successfully inserted into the mouse input stream.
        ''' <para></para>
        ''' If the function returns zero, the input was already blocked by another thread.
        ''' </returns>
        <DebuggerStepThrough>
        Public Shared Function SendMouseInput(button As MouseButtonClick) As Integer

            Select Case button
                Case MouseButtonClick.LeftButton
                    Return SendMouseInput(MouseButtonMessage.LeftDown, MouseButtonMessage.LeftUp)

                Case MouseButtonClick.RightButton
                    Return SendMouseInput(MouseButtonMessage.RightDown, MouseButtonMessage.RightUp)

                Case MouseButtonClick.MiddleButton
                    Return SendMouseInput(MouseButtonMessage.MiddleDown, MouseButtonMessage.MiddleUp)

                Case Else
                    Throw New InvalidEnumArgumentException(NameOf(button), button, GetType(MouseButtonClick))
            End Select

        End Function

#End Region

    End Class

End Namespace

#End Region
