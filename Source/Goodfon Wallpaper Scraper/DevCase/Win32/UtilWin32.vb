
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Linq
Imports System.Diagnostics
Imports System.Diagnostics.CodeAnalysis

#End Region

#Region " Native Util "

' ReSharper disable once CheckNamespace

Namespace DevCase.Win32

    ''' <summary>
    ''' Contains win32 related utilites.
    ''' </summary>
    '''
    ''' <remarks>
    ''' <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/aa383751%28v=vs.85%29.aspx"/>
    ''' </remarks>
    <SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification:="Required to migrate this code to .NET Core")>
    Public NotInheritable Class UtilWin32

#Region " Constructors "

        ''' <summary>
        ''' Prevents a default instance of the <see cref="UtilWin32"/> class from being created.
        ''' </summary>
        <DebuggerNonUserCode>
        Private Sub New()
        End Sub

#End Region

#Region " Public Methods "

        ''' <summary>
        ''' Creates a LONG Param (LParam) value for a window message, from two <see cref="Integer"/> values.
        ''' <para></para>
        ''' You must call this method if you need to use negative values.
        ''' <para></para>
        ''' <seealso cref="Windows.Forms.Message.LParam"/>
        ''' </summary>
        '''
        ''' <remarks>
        ''' <see href="https://docs.microsoft.com/en-us/windows/desktop/api/winuser/nf-winuser-makelparam"/>
        ''' </remarks>
        '''
        ''' <example> This is a code example.
        ''' <code language="VB.NET">
        ''' Dim lParam as IntPtr = MakeLParam(Integer.MaxValue, Integer.MaxValue)
        ''' </code>
        ''' </example>
        '''
        ''' <param name="lo">
        ''' The low-order <see cref="Integer"/> value.
        ''' </param>
        ''' 
        ''' <param name="hi">
        ''' The high-order <see cref="Integer"/> value.
        ''' </param>
        '''
        ''' <returns>
        ''' The resulting LParam value.
        ''' </returns>
        <DebuggerStepThrough>
        Public Shared Function MakeLParam(lo As Integer, hi As Integer) As IntPtr

            Dim loBytes As Byte() = {BitConverter.GetBytes(lo)(0), BitConverter.GetBytes(lo)(1)}
            Dim hiBytes As Byte() = {BitConverter.GetBytes(hi)(0), BitConverter.GetBytes(hi)(1)}
            Dim combined As Byte() = loBytes.Concat(hiBytes).ToArray()

            Return New IntPtr(BitConverter.ToInt32(combined, 0))
        End Function

        ''' <summary>
        ''' Creates a WORD Param (WParam) value for a window message, from two <see cref="Integer"/> values.
        ''' <para></para>
        ''' You must call this method overload if you need to use negative values.
        ''' <para></para>
        ''' <seealso cref="Windows.Forms.Message.WParam"/>
        ''' </summary>
        '''
        ''' <remarks>
        ''' <see href="https://docs.microsoft.com/en-us/windows/desktop/api/winuser/nf-winuser-makewparam"/>
        ''' </remarks>
        '''
        ''' <example> This is a code example.
        ''' <code language="VB.NET">
        ''' Dim wParam as IntPtr = MakeWParam(Integer.MaxValue, Integer.MaxValue)
        ''' </code>
        ''' </example>
        '''
        ''' <param name="lo">
        ''' The low-order <see cref="Integer"/> value.
        ''' </param>
        ''' 
        ''' <param name="hi">
        ''' The high-order <see cref="Integer"/> value.
        ''' </param>
        '''
        ''' <returns>
        ''' The resulting WParam value.
        ''' </returns>
        <DebuggerStepThrough>
        Public Shared Function MakeWParam(lo As Integer, hi As Integer) As IntPtr

            Return MakeLParam(lo, hi)
        End Function

#End Region

    End Class

End Namespace

#End Region
