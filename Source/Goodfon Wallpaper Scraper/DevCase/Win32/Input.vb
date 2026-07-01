
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

#Region " Input "

' ReSharper disable once CheckNamespace

Namespace DevCase.Win32.Structures

    ''' <summary>
    ''' Used by <see cref="NativeMethods.SendInput"/> function 
    ''' to store information for synthesizing input events such as keystrokes, mouse movement, and mouse clicks.
    ''' </summary>
    '''
    ''' <remarks>
    ''' <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms646270%28v=vs.85%29.aspx"/>
    ''' </remarks>
    <DebuggerStepThrough>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure Input

        ''' <summary>
        ''' The type of the input event.
        ''' </summary>
        Public [Type] As InputType

        ''' <summary>
        ''' The information about a device event.
        ''' </summary>
        Public InputInfo As InputInfo

    End Structure

End Namespace

#End Region
