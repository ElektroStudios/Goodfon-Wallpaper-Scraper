
#Disable Warning BC42309 ' XML comment has a tag with a 'cref' attribute that could not be resolved

#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Runtime.InteropServices
Imports System.Security
Imports System.Text

Imports DevCase.Win32.Delegates
Imports DevCase.Win32.Enums
Imports DevCase.Win32.Structures

Imports System.Diagnostics.CodeAnalysis

#End Region

#Region " P/Invoking "

' ReSharper disable once CheckNamespace

Namespace DevCase.Win32.NativeMethods

    ''' <summary>
    ''' Platform Invocation methods (P/Invoke), access unmanaged code.
    ''' <para></para>
    ''' User32.dll.
    ''' </summary>
    <HideModuleName>
    <SuppressUnmanagedCodeSecurity>
    <SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification:="Required to migrate this code to .NET Core")>
    <SuppressMessage("Interoperability", "CA1401:P/Invokes should not be visible", Justification:="")>
    Public Module User32

        ' ReSharper disable VBWarnings::BC42309

#Region " User32.dll "

        ''' <summary>
        ''' Blocks keyboard and mouse input events from reaching applications.
        ''' <para></para>
        ''' Note that only the thread that blocked input can successfully unblock input.
        ''' </summary>
        '''
        ''' <remarks>
        ''' <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms646290%28v=vs.85%29.aspx"/>
        ''' </remarks>
        '''
        ''' <param name="blockIt">
        ''' The function's purpose.
        ''' <para></para>
        ''' If this parameter is <see langword="True"/>, keyboard and mouse input events are blocked.
        ''' <para></para>
        ''' If this parameter is <see langword="False"/>, keyboard and mouse events are unblocked.
        ''' </param>
        '''
        ''' <returns>
        ''' If the function succeeds, the return value is <see langword="True"/>.
        ''' <para></para>
        ''' If input is already blocked, the return value is <see langword="False"/>.
        ''' </returns>
        <SuppressUnmanagedCodeSecurity>
        <DllImport(Win32LibNames.User32, SetLastError:=True)>
        Public Function BlockInput(blockIt As Boolean
        ) As <MarshalAs(UnmanagedType.Bool)> Boolean
        End Function

        ''' <summary>
        ''' Converts the client-area coordinates of a specified point to screen coordinates.
        ''' </summary>
        '''
        ''' <remarks>
        ''' <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/dd183434%28v=vs.85%29.aspx"/>
        ''' </remarks>
        '''
        ''' <param name="hWnd">
        ''' A handle to the window whose client area is used for the conversion.
        ''' </param>
        ''' 
        ''' <param name="refPoint">
        ''' A <see cref="NativePoint"/> structure that contains the client coordinates to be converted.
        ''' <para></para>
        ''' The new screen coordinates are copied into this structure if the function succeeds.
        ''' </param>
        '''
        ''' <returns>
        ''' If the function succeeds, the return value is <see langword="True"/>.
        ''' <para></para>
        ''' If the function fails, the return value is <see langword="False"/>.
        ''' </returns>
        <SuppressUnmanagedCodeSecurity>
        <DllImport(Win32LibNames.User32)>
        Public Function ClientToScreen(hWnd As IntPtr,
                                              ByRef refPoint As NativePoint
        ) As <MarshalAs(UnmanagedType.Bool)> Boolean
        End Function

        ''' <summary>
        ''' Enumerates the child windows that belong to the specified parent window by passing the handle to each child window, 
        ''' in turn, to an application-defined callback function.
        ''' <para></para>
        ''' <see cref="NativeMethods.EnumChildWindows"/> continues until the last child window is enumerated 
        ''' or the callback function returns <see langword="False"/>.
        ''' </summary>
        '''
        ''' <remarks>
        ''' <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms633494%28v=vs.85%29.aspx"/>
        ''' </remarks>
        '''
        ''' <param name="hWndParent">
        ''' A handle to the parent window whose child windows are to be enumerated.
        ''' <para></para>
        ''' If this parameter is <see cref="IntPtr.Zero"/>, this function is equivalent to calling 
        ''' <see cref="NativeMethods.EnumWindows"/> function. 
        ''' </param>
        ''' 
        ''' <param name="enumFunc">
        ''' An application-defined callback function.
        ''' </param>
        ''' 
        ''' <param name="lParam">
        ''' An application-defined value to be passed to the callback function. 
        ''' </param>
        '''
        ''' <returns>
        ''' The return value is not used.
        ''' </returns>
        <SuppressUnmanagedCodeSecurity>
        <DllImport(Win32LibNames.User32, SetLastError:=True)>
        Public Function EnumChildWindows(hWndParent As IntPtr,
                                         enumFunc As EnumChildWindowsProc,
                                         lParam As IntPtr
        ) As <MarshalAs(UnmanagedType.Bool)> Boolean
        End Function

        ''' <summary>
        ''' Retrieves a handle to the top-level window whose class name and window name match the specified strings.
        ''' <para></para>
        ''' This function does not search child windows. 
        ''' This function does not perform a case-sensitive search.
        ''' <para></para>
        ''' To search child windows, beginning with a specified child window, use the <see cref="NativeMethods.FindWindowEx"/> function.
        ''' </summary>
        '''
        ''' <remarks>
        ''' <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms633499%28v=vs.85%29.aspx"/>
        ''' </remarks>
        '''
        ''' <param name="className">
        ''' The class name or a class atom created by a previous call to the RegisterClass or RegisterClassEx function. 
        ''' <para></para>
        ''' The atom must be in the low-order word of <paramref name="className"/>; the high-order word must be zero.
        ''' <para></para>
        ''' If <paramref name="className"/> is NULL, it finds any window whose title matches the <paramref name="windowName"/> parameter.
        ''' </param>
        ''' 
        ''' <param name="windowName">
        ''' The window name (the window's titlebar title). 
        ''' <para></para>
        ''' If this parameter is <see langword="Nothing"/>, all window names match.
        ''' </param>
        '''
        ''' <returns>
        ''' If the function succeeds, the return value is a handle to the window that has the specified class name and window name.
        ''' <para></para>
        ''' If the function fails, the return value is <see langword="Nothing"/>.
        ''' </returns>
        <SuppressUnmanagedCodeSecurity>
        <DllImport(Win32LibNames.User32, SetLastError:=True, CharSet:=CharSet.Auto, BestFitMapping:=False, ThrowOnUnmappableChar:=True)>
        Public Function FindWindow(className As String,
                                          windowName As String
        ) As IntPtr
        End Function

        ''' <summary>
        ''' Retrieves a handle to a window whose class name and window name match the specified strings.
        ''' <para></para>
        ''' The function searches child windows, beginning with the one following the specified child window.
        ''' <para></para>
        ''' This function does not perform a case-sensitive search.
        ''' </summary>
        '''
        ''' <remarks>
        ''' <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms633500%28v=vs.85%29.aspx"/>
        ''' </remarks>
        '''
        ''' <param name="hWndParent">
        ''' A <see cref="IntPtr"/> handle to the parent window whose child windows are to be searched.
        ''' <para></para>
        ''' If <paramref name="hwndParent"/> is <see cref="IntPtr.Zero"/>, the function uses the desktop window as the parent window.
        ''' <para></para> 
        ''' The function searches among windows that are child windows of the desktop. 
        ''' </param>
        ''' 
        ''' <param name="hWndChildAfter">
        ''' A <see cref="IntPtr"/> handle to a child window. 
        ''' <para></para>
        ''' The search begins with the next child window in the Z order. 
        ''' <para></para>
        ''' The child window must be a direct child window of hwndParent, not just a descendant window.
        ''' <para></para>
        ''' If <paramref name="hwndChildAfter"/> is <see cref="IntPtr.Zero"/>, 
        ''' the search begins with the first child window of <paramref name="hwndParent"/>.
        ''' </param>
        ''' 
        ''' <param name="className">
        ''' The window class name.
        ''' </param>
        ''' 
        ''' <param name="windowName">
        ''' The window name (the window title).
        ''' <para></para>
        ''' If this parameter is <see langword="Nothing"/>, all window names match.
        ''' </param>
        '''
        ''' <returns>
        ''' If the function succeeds, the return value is a <see cref="IntPtr"/> handle to the window that 
        ''' has the specified class and window names.
        ''' <para></para>
        ''' If the function fails, the return value is <see cref="IntPtr.Zero"/>.
        ''' </returns>
        <SuppressUnmanagedCodeSecurity>
        <DllImport(Win32LibNames.User32, SetLastError:=True, CharSet:=CharSet.Auto, BestFitMapping:=False, ThrowOnUnmappableChar:=True)>
        Public Function FindWindowEx(hWndParent As IntPtr,
                                            hWndChildAfter As IntPtr,
                                            className As String,
                                            windowName As String
        ) As IntPtr
        End Function

        ''' <summary>
        ''' Retrieves the name of the class to which the specified window belongs.
        ''' </summary>
        '''
        ''' <remarks>
        ''' <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms633582(v=vs.85).aspx"/>
        ''' </remarks>
        '''
        ''' <param name="hWnd">
        ''' A handle to the window and, indirectly, the class to which the window belongs.
        ''' </param>
        ''' 
        ''' <param name="className">
        ''' The class name string. 
        ''' </param>
        ''' 
        ''' <param name="maxCount">
        ''' The length of the <paramref name="className"/> buffer, in characters. 
        ''' <para></para>
        ''' The buffer must be large enough to include the terminating null character; 
        ''' otherwise, the class name string is truncated to <paramref name="maxCount"/>-1 characters. 
        ''' </param>
        '''
        ''' <returns>
        ''' If the function succeeds, the return value is the number of characters copied to the buffer, 
        ''' not including the terminating null character.
        ''' <para></para>
        ''' If the function fails, the return value is <c>0</c>. 
        ''' <para></para>
        ''' To get extended error information, call <see cref="Marshal.GetLastWin32Error"/>.
        ''' </returns>
        <SuppressUnmanagedCodeSecurity>
        <DllImport(Win32LibNames.User32, CharSet:=CharSet.Auto, SetLastError:=True, BestFitMapping:=False, ThrowOnUnmappableChar:=True)>
        Public Function GetClassName(hWnd As IntPtr,
                                            className As StringBuilder,
                                            maxCount As Integer
        ) As Integer
        End Function

        ''' <summary>
        ''' Retrieves a <see cref="IntPtr"/> handle to a device context (DC) for the client area of a specified window or for the entire screen.
        ''' <para></para>
        ''' You can use the returned handle in subsequent GDI functions to draw in the DC.
        ''' <para></para>
        ''' The device context is an opaque data structure, whose values are used internally by GDI.
        ''' </summary>
        '''
        ''' <remarks>
        ''' <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/dd144871%28v=vs.85%29.aspx"/>
        ''' </remarks>
        '''
        ''' <param name="hWnd">
        ''' A <see cref="IntPtr"/> handle to the window whose DC is to be retrieved. 
        ''' <para></para>
        ''' If this value is <see cref="IntPtr.Zero"/>, <see cref="GetDC"/> retrieves the DC for the entire screen.
        ''' </param>
        '''
        ''' <returns>
        ''' If the function succeeds, the return value is a <see cref="IntPtr"/> handle to the DC for the specified window's client area.
        ''' <para></para>
        ''' If the function fails, the return value is <see cref="IntPtr.Zero"/>.
        ''' </returns>
        <SuppressUnmanagedCodeSecurity>
        <DllImport(Win32LibNames.User32, SetLastError:=False)>
        Public Function GetDC(hWnd As IntPtr
        ) As IntPtr
        End Function

        ''' <summary>
        ''' Retrieves information about the specified icon or cursor.
        ''' </summary>
        '''
        ''' <remarks>
        ''' <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms648070%28v=vs.85%29.aspx"/>
        ''' </remarks>
        '''
        ''' <param name="hIcon">
        ''' A handle to the icon or cursor.
        ''' </param>
        ''' 
        ''' <param name="refIconInfo">
        ''' An <see cref="IconInfo"/> structure that receives the information.
        ''' </param>
        '''
        ''' <returns>
        ''' If the function succeeds, the return value is <see langword="True"/>, 
        ''' and the function fills in the members of the specified <see cref="IconInfo"/> structure.
        ''' <para></para>
        ''' If the function fails, the return value is <see langword="False"/>.
        ''' <para></para>
        ''' To get extended error information, call <see cref="Marshal.GetLastWin32Error"/>.
        ''' </returns>
        <SuppressUnmanagedCodeSecurity>
        <DllImport(Win32LibNames.User32, EntryPoint:="GetIconInfo", SetLastError:=True)>
        Public Function GetIconInfo(hIcon As IntPtr,
                              ByRef refIconInfo As IconInfo
        ) As <MarshalAs(UnmanagedType.Bool)> Boolean
        End Function

        ''' <summary>
        ''' Determines the number of items in the specified menu.
        ''' </summary>
        '''
        ''' <remarks>
        ''' <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms647978%28v=vs.85%29.aspx"/>
        ''' </remarks>
        '''
        ''' <param name="hMenu">
        ''' A handle to the menu to be examined.
        ''' </param>
        '''
        ''' <returns>
        ''' If the function succeeds, the return value specifies the number of items in the menu.
        ''' <para></para>
        ''' If the function fails, the return value is <c>-1</c>.
        ''' <para></para>
        ''' To get extended error information, call <see cref="Marshal.GetLastWin32Error"/>. 
        ''' </returns>
        <SuppressUnmanagedCodeSecurity>
        <DllImport(Win32LibNames.User32, SetLastError:=True)>
        Public Function GetMenuItemCount(hMenu As IntPtr
        ) As Integer
        End Function

        ''' <summary>
        ''' Retrieves the menu item identifier of a menu item located at the specified position in a menu. 
        ''' </summary>
        '''
        ''' <remarks>
        ''' <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms647979%28v=vs.85%29.aspx"/>
        ''' </remarks>
        '''
        ''' <param name="hMenu">
        ''' A handle to the menu that contains the item whose identifier is to be retrieved. 
        ''' </param>
        ''' 
        ''' <param name="nPos">
        ''' The zero-based relative position of the menu item whose identifier is to be retrieved. 
        ''' </param>
        '''
        ''' <returns>
        ''' The return value is the identifier of the specified menu item.
        ''' <para></para>
        ''' If the menu item identifier is <see cref="IntPtr.Zero"/> or if the specified item opens a submenu, 
        ''' the return value is <c>-1</c>. 
        ''' </returns>
        <SuppressUnmanagedCodeSecurity>
        <DllImport(Win32LibNames.User32)>
        Public Function GetMenuItemID(hMenu As IntPtr,
                                             nPos As Integer
        ) As Integer
        End Function

        ''' <summary>
        ''' Gets the handle of the form's system menu.
        ''' </summary>
        '''
        ''' <remarks>
        ''' <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms647980%28v=vs.85%29.aspx"/>
        ''' </remarks>
        '''
        ''' <param name="hMenu">
        ''' The handle to the menu that contains the menu item.
        ''' </param>
        ''' 
        ''' <param name="uItem">
        ''' The identifier or position of the menu item to get information about.
        ''' <para></para>
        ''' The meaning of this parameter depends on the value of fByPosition.
        ''' </param>
        ''' 
        ''' <param name="byPosition">
        ''' The meaning of <paramref name="uItem"/>.
        ''' If this parameter is <see langword="False"/>, uItem is a menu item identifier.
        ''' <para></para>
        ''' If this parameter is <see langword="True"/>, it is a menu item position.
        ''' </param>
        ''' 
        ''' <param name="refInfo">
        ''' A <see cref="MenuItemInfo"/> structure that specifies the information to retrieve.
        ''' <para></para>
        ''' Note that you must set the <see cref="MenuItemInfo.SizeOfStruct"/> member to <c>Marshal.SizeOf(Of MenuItemInfo)</c> before calling this function.
        ''' </param>
        '''
        ''' <returns>
        ''' If the function succeeds, the return is <see langword="True"/>.
        ''' <para></para>
        ''' If the function fails, the return value is <see langword="False"/>.
        ''' <para></para>
        ''' To get extended error information, call <see cref="Marshal.GetLastWin32Error"/>. 
        ''' </returns>
        <SuppressUnmanagedCodeSecurity>
        <DllImport(Win32LibNames.User32, SetLastError:=True)>
        Public Function GetMenuItemInfo(hMenu As IntPtr,
                                               uItem As UInteger,
               <MarshalAs(UnmanagedType.Bool)> byPosition As Boolean,
                                               ByRef refInfo As MenuItemInfo
        ) As <MarshalAs(UnmanagedType.Bool)> Boolean
        End Function

        ''' <summary>
        ''' Retrieves the menu flags associated with the specified menu item.
        ''' <para></para>
        ''' If the menu item opens a submenu, this function also returns the number of items in the submenu. 
        ''' </summary>
        '''
        ''' <remarks>
        ''' <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms647982%28v=vs.85%29.aspx"/>
        ''' </remarks>
        '''
        ''' <param name="hMenu">
        ''' A handle to the menu that contains the menu item whose flags are to be retrieved.
        ''' </param>
        ''' 
        ''' <param name="id">
        ''' The menu item for which the menu flags are to be retrieved, as determined by the <paramref name="flags"/> parameter.
        ''' </param>
        ''' 
        ''' <param name="flags">
        ''' Indicates how the <paramref name="id"/> parameter is interpreted.
        ''' </param>
        '''
        ''' <returns>
        ''' If the specified item does not exist, the return value is -1.
        ''' <para></para>
        ''' If the menu item opens a submenu, the low-order byte of the return value contains the 
        ''' menu flags associated with the item, 
        ''' and the high-order byte contains the number of items in the submenu opened by the item.
        ''' <para></para>
        ''' Otherwise, the return value is a mask (Bitwise <c>OR</c>) of the menu flags.
        ''' </returns>
        <Obsolete("The GetMenuState function has been superseded by the GetMenuItemInfo. You can still use GetMenuState, however, if you do not need any of the extended features of GetMenuItemInfo.", False)>
        <SuppressUnmanagedCodeSecurity>
        <DllImport(Win32LibNames.User32)>
        Public Function GetMenuState(hMenu As IntPtr,
                                     id As UInteger,
                                     flags As MenuPosition
        ) As UInteger
        End Function

        ''' <summary>
        ''' Retrieves a handle to a window that has the specified relationship (Z-Order or owner) to the specified window.
        ''' </summary>
        '''
        ''' <remarks>
        ''' <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms633515%28v=vs.85%29.aspx"/>
        ''' </remarks>
        '''
        ''' <param name="hWnd">
        ''' A handle to a window.
        ''' <para></para>
        ''' The window handle retrieved is relative to this window, based on the value of the uCmd parameter.
        ''' </param>
        ''' 
        ''' <param name="gwCmd">
        ''' The relationship between the specified window and the window whose handle is to be retrieved.
        ''' </param>
        '''
        ''' <returns>
        ''' If the function succeeds, the return value is a window handle.
        ''' <para></para>
        ''' If no window exists with the specified relationship to the specified window, the return value is <see cref="IntPtr.Zero"/>.
        ''' <para></para>
        ''' To get extended error information, call <see cref="Marshal.GetLastWin32Error"/>.
        ''' </returns>
        <SuppressUnmanagedCodeSecurity>
        <DllImport(Win32LibNames.User32, SetLastError:=True)>
        Public Function GetWindow(hWnd As IntPtr,
                                         gwCmd As GetWindowCmd
        ) As IntPtr
        End Function

        ''' <summary>
        ''' Retrieves the dimensions of the bounding rectangle of the specified window. 
        ''' <para></para>
        ''' The dimensions are given in screen coordinates that are relative to the upper-left corner of the screen.
        ''' </summary>
        '''
        ''' <remarks>
        ''' <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms633519%28v=vs.85%29.aspx"/>
        ''' </remarks>
        '''
        ''' <param name="hWnd">
        ''' A <see cref="IntPtr"/> handle to the window.
        ''' </param>
        ''' 
        ''' <param name="refRect">
        ''' A <see cref="NativeRectangle"/> structure that receives the screen coordinates of the 
        ''' upper-left and lower-right corners of the window.
        ''' </param>
        '''
        ''' <returns>
        ''' <see langword="True"/> if the function succeeds, <see langword="False"/> otherwise.
        ''' </returns>
        <SuppressUnmanagedCodeSecurity>
        <DllImport(Win32LibNames.User32, SetLastError:=True)>
        Public Function GetWindowRect(hWnd As IntPtr,
                                       <Out> ByRef refRect As NativeRectangle
        ) As <MarshalAs(UnmanagedType.Bool)> Boolean
        End Function

        ''' <summary>
        ''' Retrieves the identifier of the thread that created the specified window 
        ''' and, optionally, the identifier of the process that created the window.
        ''' </summary>
        '''
        ''' <remarks>
        ''' <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms633522%28v=vs.85%29.aspx"/>
        ''' </remarks>
        '''
        ''' <param name="hWnd">
        ''' A <see cref="IntPtr"/> handle to the window.
        ''' </param>
        ''' 
        ''' <param name="refPid">
        ''' A variable that receives the process identifier (PID). 
        ''' <para></para>
        ''' If this parameter is not <see langword="Nothing"/>, <see cref="NativeMethods.GetWindowThreadProcessId"/> copies the identifier of 
        ''' the process to the variable; otherwise, it does not.
        ''' </param>
        '''
        ''' <returns>
        ''' The identifier of the thread that created the window.
        ''' </returns>
        <SuppressUnmanagedCodeSecurity>
        <DllImport(Win32LibNames.User32, SetLastError:=True)>
        Public Function GetWindowThreadProcessId(hWnd As IntPtr,
                                                <[Out]> ByRef refPid As Integer
        ) As Integer
        End Function

        ''' <summary>
        ''' Determines whether the specified window is minimized (iconic). 
        ''' </summary>
        '''
        ''' <remarks>
        ''' <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms633527%28v=vs.85%29.aspx"/>
        ''' </remarks>
        '''
        ''' <param name="hWnd">
        ''' A handle to the window to be tested.
        ''' </param>
        '''
        ''' <returns>
        ''' If the window is iconic, the return value is <see langword="True"/>.
        ''' <para></para>
        ''' If the window is not iconic, the return value is <see langword="False"/>.
        ''' </returns>
        <SuppressUnmanagedCodeSecurity>
        <DllImport(Win32LibNames.User32, SetLastError:=True)>
        Public Function IsIconic(hWnd As IntPtr
        ) As <MarshalAs(UnmanagedType.Bool)> Boolean
        End Function

        ''' <summary>
        ''' Determines whether the specified window handle identifies an existing window.
        ''' </summary>
        '''
        ''' <remarks>
        ''' <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms633528%28v=vs.85%29.aspx"/>
        ''' </remarks>
        '''
        ''' <param name="hWnd">
        ''' A handle to the window to be tested. 
        ''' </param>
        '''
        ''' <returns>
        ''' If the window handle identifies an existing window, the return value is <see langword="True"/>.
        ''' <para></para>
        ''' If the window handle does not identify an existing window, the return value is <see langword="False"/>.
        ''' </returns>
        <SuppressUnmanagedCodeSecurity>
        <DllImport(Win32LibNames.User32, SetLastError:=True)>
        Public Function IsWindow(hWnd As IntPtr
        ) As <MarshalAs(UnmanagedType.Bool)> Boolean
        End Function

        ''' <summary>
        ''' Determines the visibility state of the specified window.
        ''' </summary>
        '''
        ''' <remarks>
        ''' <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms633530%28v=vs.85%29.aspx"/>
        ''' </remarks>
        '''
        ''' <param name="hWnd">
        ''' A handle to the window to be tested. 
        ''' </param>
        '''
        ''' <returns>
        ''' If the specified window, its parent window, its parent's parent window, and so forth, 
        ''' have the <see cref="WindowStyles.Visible"/> style, the return value is <see langword="True"/>.
        ''' <para></para>
        ''' Otherwise, the return value is <see langword="False"/>.
        ''' <para></para>
        ''' Because the return value specifies whether the window has the WS_VISIBLE style, 
        ''' it may be nonzero even if the window is totally obscured by other windows.
        ''' </returns>
        <SuppressUnmanagedCodeSecurity>
        <DllImport(Win32LibNames.User32, SetLastError:=True)>
        Public Function IsWindowVisible(hWnd As IntPtr
        ) As <MarshalAs(UnmanagedType.Bool)> Boolean
        End Function

        ''' <summary>
        ''' Releases a device context (DC), freeing it for use by other applications.
        ''' <para></para>
        ''' The effect of the <see cref="ReleaseDC"/> function depends on the type of DC. 
        ''' It frees only common and window DCs. 
        ''' It has no effect on class or private DCs.
        ''' </summary>
        '''
        ''' <remarks>
        ''' <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/dd162920%28v=vs.85%29.aspx"/>
        ''' </remarks>
        '''
        ''' <param name="hWnd">
        ''' A <see cref="IntPtr"/> handle to the window whose DC is to be released.
        ''' </param>
        ''' 
        ''' <param name="hdc">
        ''' A <see cref="IntPtr"/> handle to the DC to be released.
        ''' </param>
        '''
        ''' <returns>
        ''' <see langword="True"/> if the DC was released, <see langword="False"/> if the DC was not released.
        ''' </returns>
        <SuppressUnmanagedCodeSecurity>
        <DllImport(Win32LibNames.User32, SetLastError:=False)>
        Public Function ReleaseDC(hWnd As IntPtr,
                                         hdc As IntPtr
        ) As <MarshalAs(UnmanagedType.Bool)> Boolean
        End Function

        ''' <summary>
        ''' Synthesizes keystrokes, mouse motions, and button clicks.
        ''' </summary>
        '''
        ''' <remarks>
        ''' <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms646310%28v=vs.85%29.aspx"/>
        ''' </remarks>
        '''
        ''' <param name="nInputs">
        ''' The number of structures in the pInputs array.
        ''' </param>
        ''' 
        ''' <param name="pInputs">
        ''' An array of <see cref="Input"/> structures.
        ''' <para></para>
        ''' Each structure represents an event to be inserted into the keyboard or mouse input stream.
        ''' </param>
        ''' 
        ''' <param name="cbSize">
        ''' The size, in bytes, of an <see cref="Input"/> structure.
        ''' <para></para>
        ''' If <paramref name="cbSize"/> is not the size of an <see cref="Input"/> structure, the function fails.
        ''' </param>
        '''
        ''' <returns>
        ''' The function returns the number of events that it successfully inserted into the keyboard or mouse input stream.
        ''' <para></para>
        ''' If the function returns zero, the input was already blocked by another thread.
        ''' </returns>
        <SuppressUnmanagedCodeSecurity>
        <DllImport(Win32LibNames.User32, SetLastError:=True)>
        Public Function SendInput(nInputs As Integer,
<MarshalAs(UnmanagedType.LPArray), [In]> pInputs As Input(),
                                         cbSize As Integer
        ) As Integer
        End Function

        ''' <summary>
        ''' Moves the cursor to the specified screen coordinates. 
        ''' <para></para>
        ''' If the new coordinates are not within the screen rectangle set by the most recent <see cref="ClipCursor"/> function call, 
        ''' the system automatically adjusts the coordinates so that the cursor stays within the rectangle.
        ''' </summary>
        '''
        ''' <remarks>
        ''' <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms648394(v=vs.85).aspx"/>
        ''' </remarks>
        '''
        ''' <param name="x">
        ''' The new x-coordinate of the cursor, in screen coordinates.
        ''' </param>
        ''' 
        ''' <param name="y">
        ''' The new y-coordinate of the cursor, in screen coordinates.
        ''' </param>
        '''
        ''' <returns>
        ''' If the function succeeds, the return value is <see langword="True"/>.
        ''' <para></para>
        ''' If the function fails, the return value is <see langword="False"/>.
        ''' <para></para>
        ''' To get extended error information, call <see cref="Marshal.GetLastWin32Error"/>.
        ''' </returns>
        <SuppressUnmanagedCodeSecurity>
        <DllImport(Win32LibNames.User32, SetLastError:=True)>
        Public Function SetCursorPos(x As Integer,
                                            y As Integer
        ) As <MarshalAs(UnmanagedType.Bool)> Boolean
        End Function

        ''' <summary>
        ''' Brings the thread that created the specified window into the foreground and activates the window. 
        ''' <para></para>
        ''' Keyboard input is directed to the window, and various visual cues are changed for the user. 
        ''' <para></para>
        ''' The system assigns a slightly higher priority to the thread that created the foreground window than it does to other threads.
        ''' </summary>
        '''
        ''' <remarks>
        ''' <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms633539%28v=vs.85%29.aspx"/>
        ''' </remarks>
        '''
        ''' <param name="hWnd">
        ''' A <see cref="IntPtr"/> handle to the window that should be activated and brought to the foreground.
        ''' </param>
        '''
        ''' <returns>
        ''' If the window was brought to the foreground, the return value is <see langword="True"/>.
        ''' If the window was not brought to the foreground, the return value is <see langword="False"/>.
        ''' </returns>
        <SuppressUnmanagedCodeSecurity>
        <DllImport(Win32LibNames.User32, SetLastError:=True)>
        Public Function SetForegroundWindow(hWnd As IntPtr
        ) As <MarshalAs(UnmanagedType.Bool)> Boolean
        End Function

        ''' <summary>
        ''' Sets the specified window's show state.
        ''' </summary>
        '''
        ''' <remarks>
        ''' <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms633548%28v=vs.85%29.aspx"/>
        ''' </remarks>
        '''
        ''' <param name="hWnd">
        ''' A <see cref="IntPtr"/> handle to the window.
        ''' </param>
        ''' 
        ''' <param name="windowState">
        ''' Controls how the window is to be shown.
        ''' </param>
        '''
        ''' <returns>
        ''' If the window was previously visible, the return value is <see langword="True"/>.
        ''' <para></para>
        ''' If the window was previously hidden, the return value is <see langword="False"/>.
        ''' </returns>
        <SuppressUnmanagedCodeSecurity>
        <DllImport(Win32LibNames.User32, SetLastError:=True)>
        Public Function ShowWindow(hWnd As IntPtr,
            <MarshalAs(UnmanagedType.I4)> windowState As NativeWindowState
        ) As <MarshalAs(UnmanagedType.Bool)> Boolean
        End Function

        ''' <summary>
        ''' Sends the specified message to a window or windows.
        ''' <para></para>
        ''' The SendMessage function calls the window procedure for the specified window
        ''' and does not return until the window procedure has processed the message.
        ''' </summary>
        '''
        ''' <remarks>
        ''' <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms644950%28v=vs.85%29.aspx"/>
        ''' </remarks>
        '''
        ''' <param name="hWnd">
        ''' A handle to the window whose window procedure will receive the message.
        ''' </param>
        ''' 
        ''' <param name="msg">
        ''' The message to be sent.
        ''' </param>
        ''' 
        ''' <param name="wParam">
        ''' Additional message-specific information.
        ''' </param>
        ''' 
        ''' <param name="lParam">
        ''' Additional message-specific information.
        ''' </param>
        '''
        ''' <returns>
        ''' The return value specifies the result of the message processing; it depends on the message sent.
        ''' </returns>
        <DllImport(Win32LibNames.User32, SetLastError:=True)>
        Public Function SendMessage(hWnd As IntPtr,
                                    msg As Integer,
                                    wParam As IntPtr,
                                    lParam As IntPtr
        ) As IntPtr
        End Function

        ''' <summary>
        ''' Places (posts) a message in the message queue associated with the thread that created 
        ''' the specified window and returns without waiting for the thread to process the message.
        ''' </summary>
        '''
        ''' <remarks>
        ''' <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms644944%28v=vs.85%29.aspx"/>
        ''' </remarks>
        '''
        ''' <param name="hWnd">
        ''' Handle to the window whose window procedure will receive the message.
        ''' <para></para>
        ''' If this parameter is <see cref="WindowMessages.HWND_Broadcast"/>, 
        ''' the message is sent to all top-level windows in the system, 
        ''' including disabled or invisible unowned windows, overlapped windows, and pop-up windows; 
        ''' but the message is not sent to child windows.
        ''' </param>
        ''' 
        ''' <param name="message">
        ''' Specifies the message to be sent.
        ''' </param>
        ''' 
        ''' <param name="wparam">
        ''' Specifies additional message-specific information.
        ''' </param>
        ''' 
        ''' <param name="lparam">
        ''' Specifies additional message-specific information.
        ''' </param>
        '''
        ''' <returns>
        ''' If the function succeeds, the return value is <see langword="True"/>.
        ''' <para></para>
        ''' If the function fails, the return value is <see langword="False"/>.
        ''' <para></para>
        ''' To get extended error information, call <see cref="Marshal.GetLastWin32Error"/>. 
        ''' </returns>
        <DllImport(Win32LibNames.User32, SetLastError:=True)>
        Public Function PostMessage(hWnd As IntPtr,
                                           message As Integer,
                                           wparam As IntPtr,
                                           lparam As IntPtr
        ) As <MarshalAs(UnmanagedType.Bool)> Boolean
        End Function

        ''' <summary>
        ''' Enumerates all top-level windows on the screen by passing the handle to each window, in turn, 
        ''' to an application-defined callback function.
        ''' <para></para>
        ''' <see cref="EnumWindows"/> continues until the last top-level window is enumerated 
        ''' or the callback function returns <see langword="False"/>. 
        ''' </summary>
        '''
        ''' <remarks>
        ''' <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms633497%28v=vs.85%29.aspx"/>
        ''' </remarks>
        '''
        ''' <param name="lpEnumFunc">
        ''' An application-defined callback function.
        ''' </param>
        ''' 
        ''' <param name="lParam">
        ''' An application-defined value to be passed to the callback function. 
        ''' </param>
        '''
        ''' <returns>
        ''' <see langword="True"/> if the function succeeds, <see langword="False"/> otherwise.
        ''' <para></para>
        ''' If <c>EnumWindowsProc</c> returns <see langword="False"/>, the return value is also <see langword="False"/>.
        ''' <para></para>
        ''' To get extended error information, call <see cref="Marshal.GetLastWin32Error"/>.
        ''' </returns>
        <SuppressUnmanagedCodeSecurity>
        <DllImport(Win32LibNames.User32, SetLastError:=True)>
        Public Function EnumWindows(lpEnumFunc As EnumWindowsProc,
                                           lParam As IntPtr
        ) As <MarshalAs(UnmanagedType.Bool)> Boolean
        End Function

        ''' <summary>
        ''' Retrieves the length, in characters, of the specified window's title bar text (if the window has a title bar). 
        ''' <para></para>
        ''' If the specified window is a control, the function retrieves the length of the text within the control. 
        ''' However, <see cref="NativeMethods.GetWindowTextLength"/> cannot retrieve the 
        ''' length of the text of an edit control in another application.
        ''' </summary>
        '''
        ''' <remarks>
        ''' <see href="https://docs.microsoft.com/en-us/windows/desktop/api/winuser/nf-winuser-getwindowtextlengtha"/>
        ''' </remarks>
        '''
        ''' <param name="hWnd">
        ''' A handle to the window or control.
        ''' </param>
        '''
        ''' <returns>
        ''' If the function succeeds, the return value is the length, in characters, of the text. 
        ''' Under certain conditions, this value may actually be greater than the length of the text.
        ''' <para></para>
        ''' If the window has no text, the return value is <see langword="False"/>.
        ''' <para></para>
        ''' To get extended error information, call <see cref="Marshal.GetLastWin32Error"/>. 
        ''' </returns>
        <DllImport(Win32LibNames.User32, SetLastError:=True, CharSet:=CharSet.Auto)>
        Public Function GetWindowTextLength(hWnd As IntPtr
        ) As Integer
        End Function

        ''' <summary>
        ''' Retrieves the show state and the restored, minimized, and maximized positions of the specified window.
        ''' </summary>
        '''
        ''' <remarks>
        ''' <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms633518%28v=vs.85%29.aspx"/>
        ''' </remarks>
        '''
        ''' <param name="hWnd">
        ''' A handle to the window.
        ''' </param>
        ''' 
        ''' <param name="refWindowPlacement">
        ''' The <see cref="WindowPlacement"/> structure that receives the 
        ''' show state and position information.
        ''' <para></para>
        ''' Before calling <see cref="NativeMethods.GetWindowPlacement"/>, 
        ''' set the <see cref="WindowPlacement.SizeOfStruct"/> member to <c>Marshal.SizeOf(Of WindowPlacement)</c>.
        ''' <para></para>
        ''' <see cref="NativeMethods.GetWindowPlacement"/> fails if <paramref name="refWindowPlacement"/> length is not set correctly.
        ''' </param>
        '''
        ''' <returns>
        ''' If the function succeeds, the return value is <see langword="True"/>.
        ''' <para></para>
        ''' If the function fails, the return value is <see langword="False"/>.
        ''' <para></para>
        ''' To get extended error information, call <see cref="Marshal.GetLastWin32Error"/>.
        ''' </returns>
        <SuppressUnmanagedCodeSecurity>
        <DllImport(Win32LibNames.User32, SetLastError:=True)>
        Public Function GetWindowPlacement(hWnd As IntPtr,
                                                  ByRef refWindowPlacement As WindowPlacement
        ) As <MarshalAs(UnmanagedType.Bool)> Boolean
        End Function

        ''' <summary>
        ''' Changes the size, position, and Z order of a child, pop-up, or top-level window.
        ''' <para></para>
        ''' These windows are ordered according to their appearance on the screen.
        ''' <para></para>
        ''' The topmost window receives the highest rank and is the first window in the Z order.
        ''' </summary>
        '''
        ''' <remarks>
        ''' <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms633545(v=vs.85).aspx"/>
        ''' </remarks>
        '''
        ''' <param name="hWnd">
        ''' A handle to the window.
        ''' </param>
        ''' 
        ''' <param name="hWndInsertAfter">
        ''' A handle to the window to precede the positioned window in the Z order.
        ''' </param>
        ''' 
        ''' <param name="x">
        ''' The new position of the left side of the window, in client coordinates.
        ''' </param>
        ''' 
        ''' <param name="y">
        ''' The new position of the top of the window, in client coordinates.
        ''' </param>
        ''' 
        ''' <param name="cx">
        ''' The new width of the window, in pixels.
        ''' </param>
        ''' 
        ''' <param name="cy">
        ''' The new height of the window, in pixels.
        ''' </param>
        ''' 
        ''' <param name="uFlags">
        ''' The window sizing and positioning flags.
        ''' </param>
        '''
        ''' <returns>
        ''' If the function succeeds, the return value is <see langword="True"/>.
        ''' <para></para>
        ''' If the function fails, the return value is <see langword="False"/>.
        ''' <para></para>
        ''' To get extended error information, call <see cref="Marshal.GetLastWin32Error"/>. 
        ''' </returns>
        <SuppressUnmanagedCodeSecurity>
        <DllImport(Win32LibNames.User32, SetLastError:=True)>
        Public Function SetWindowPos(hWnd As IntPtr,
                                            hWndInsertAfter As IntPtr,
                                            x As Integer,
                                            y As Integer,
                                            cx As Integer,
                                            cy As Integer,
                                            uFlags As SetWindowPosFlags
        ) As <MarshalAs(UnmanagedType.Bool)> Boolean
        End Function

#End Region

    End Module

End Namespace

#End Region

#Enable Warning BC42309 ' XML comment has a tag with a 'cref' attribute that could not be resolved
