
#Disable Warning BC42309 ' XML comment has a tag with a 'cref' attribute that could not be resolved

#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Security

#End Region

#Region " P/Invoking "

' ReSharper disable once CheckNamespace

Namespace DevCase.Win32.Delegates

    ''' <summary>
    ''' Contains delegate callbacks for Win32 functions.
    ''' </summary>
    <HideModuleName>
    Public Module Delegates

#Region " Delegates "

        ''' <summary>
        ''' An application-defined callback function used with the <see cref="NativeMethods.EnumChildWindows"/> function.
        ''' <para></para>
        ''' It receives the child window handles.
        ''' <para></para>
        ''' The <c>WNDENUMPROC</c> type defines a pointer to this callback function.
        ''' <para></para>
        ''' <see cref="EnumChildWindowsProc"/> is a placeholder for the application-defined function name. 
        ''' </summary>
        '''
        ''' <remarks>
        ''' <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms633493%28v=vs.85%29.aspx"/>
        ''' </remarks>
        '''
        ''' <param name="hWnd">
        ''' A handle to a child window of the parent window specified in <see cref="NativeMethods.EnumChildWindows"/> function. 
        ''' </param>
        ''' 
        ''' <param name="lParam">
        ''' The application-defined value given in <see cref="NativeMethods.EnumChildWindows"/> function.
        ''' </param>
        '''
        ''' <returns>
        ''' To continue enumeration, the callback function must return <see langword="True"/>; 
        ''' to stop enumeration, it must return <see langword="False"/>. 
        ''' </returns>
        <SuppressUnmanagedCodeSecurity>
        Public Delegate Function EnumChildWindowsProc(hWnd As IntPtr,
                                                      lParam As IntPtr
        ) As Boolean

        ''' <summary>
        ''' An application-defined callback function used with the <see cref="NativeMethods.EnumWindows"/> 
        ''' or <see cref="NativeMethods.EnumDesktopWindows"/> function.
        ''' <para></para>
        ''' It receives top-level window handles.
        ''' <para></para>
        ''' The <c>WNDENUMPROC</c> type defines a pointer to this callback function.
        ''' <para></para>
        ''' <see cref="EnumWindowsProc"/> is a placeholder for the application-defined function name. 
        ''' </summary>
        '''
        ''' <remarks>
        ''' <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms633498%28v=vs.85%29.aspx"/>
        ''' </remarks>
        '''
        ''' <param name="hWnd">
        ''' A handle to a top-level window.
        ''' </param>
        ''' 
        ''' <param name="lParam">
        ''' The application-defined value given in <see cref="NativeMethods.EnumWindows"/> 
        ''' or <see cref="NativeMethods.EnumDesktopWindows"/> functions.
        ''' </param>
        '''
        ''' <returns>
        ''' To continue enumeration, the callback function must return <see langword="True"/>; 
        ''' to stop enumeration, it must return <see langword="False"/>. 
        ''' </returns>
        <SuppressUnmanagedCodeSecurity>
        Public Delegate Function EnumWindowsProc(hWnd As IntPtr,
                                                 lParam As IntPtr
        ) As Boolean

#End Region

    End Module

End Namespace

#End Region

#Enable Warning BC42309 ' XML comment has a tag with a 'cref' attribute that could not be resolved