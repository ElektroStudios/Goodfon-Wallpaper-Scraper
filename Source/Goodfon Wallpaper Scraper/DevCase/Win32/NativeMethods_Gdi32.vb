
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Diagnostics.CodeAnalysis
Imports System.Runtime.InteropServices
Imports System.Security

Imports DevCase.Win32.Structures

#End Region

#Region " P/Invoking "

' ReSharper disable once CheckNamespace

Namespace DevCase.Win32.NativeMethods

    ''' <summary>
    ''' Platform Invocation methods (P/Invoke), access unmanaged code.
    ''' <para></para>
    ''' Gdi32.dll.
    ''' </summary>
    <HideModuleName>
    <SuppressUnmanagedCodeSecurity>
    <SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification:="Required to migrate this code to .NET Core")>
    <SuppressMessage("Interoperability", "CA1401:P/Invokes should not be visible", Justification:="")>
    Public Module Gdi32

        ' ReSharper disable VBWarnings::BC42309

#Region " Gdi32.dll "

        ''' <summary>
        ''' Retrieves the Red, Green, Blue (RGB) color value of the pixel at the specified coordinates.
        ''' </summary>
        '''
        ''' <remarks>
        ''' <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/dd144909%28v=vs.85%29.aspx"/>
        ''' </remarks>
        '''
        ''' <param name="hdc">
        ''' A <see cref="IntPtr"/> handle to the device context.
        ''' </param>
        ''' 
        ''' <param name="nXPos">
        ''' The x-coordinate, in logical units, of the pixel to be examined.
        ''' </param>
        ''' 
        ''' <param name="nYPos">
        ''' The y-coordinate, in logical units, of the pixel to be examined.
        ''' </param>
        '''
        ''' <returns>
        ''' The return value is the <c>COLORREF</c> value that specifies the RGB of the pixel. 
        ''' If the pixel is outside of the current clipping region, the return value is <c>CLR_INVALID</c> (<c>&amp;HFFFFFFFF</c>)
        ''' </returns>
        <SuppressUnmanagedCodeSecurity>
        <DllImport(Win32LibNames.Gdi32, SetLastError:=False)>
        Public Function GetPixel(hdc As IntPtr,
                                        nXPos As Integer,
                                        nYPos As Integer
        ) As NativeColor
        End Function

#End Region

    End Module

End Namespace

#End Region
