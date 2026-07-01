
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Diagnostics
Imports System.Runtime.InteropServices

#End Region

#Region " Input Info "

' ReSharper disable once CheckNamespace

Namespace DevCase.Win32.Structures

    ''' <summary>
    ''' Used by <see cref="NativeMethods.SendInput"/> function 
    ''' to store information for synthesizing input events such as keystrokes, mouse movement, and mouse clicks.
    ''' <para></para>
    ''' For <see cref="Input.InputInfo"/> field.
    ''' </summary>
    '''
    ''' <remarks>
    ''' <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms646270%28v=vs.85%29.aspx"/>
    ''' </remarks>
    <DebuggerStepThrough>
    <StructLayout(LayoutKind.Explicit)>
    Public Structure InputInfo

        ''' <summary>
        ''' The information about a simulated mouse event.
        ''' </summary>
        <FieldOffset(0)>
        Public MouseInput As MouseInput

        ''' <summary>
        ''' The information about a simulated keyboard event.
        ''' </summary>
        <FieldOffset(0)>
        Public KeyboardInput As IntPtr ' KeyboardInput

        ''' <summary>
        ''' The information about a simulated hardware event.
        ''' </summary>
        <FieldOffset(0)>
        Public HardwareInput As IntPtr ' HardwareInput

    End Structure

End Namespace

#End Region
