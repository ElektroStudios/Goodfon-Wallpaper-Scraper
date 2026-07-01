
#Disable Warning BC42309 ' XML comment has a tag with a 'cref' attribute that could not be resolved

#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.Diagnostics
Imports System.Drawing
Imports System.Linq
Imports System.Runtime.InteropServices
Imports System.Text
Imports System.Threading

#If Not NETCOREAPP Then
Imports System.Windows.Automation
#End If

Imports DevCase.Core.IO.Devices.Input
Imports DevCase.Core.Windows.Desktop
Imports DevCase.Extensions

Imports DevCase.Win32
Imports DevCase.Win32.Enums
Imports DevCase.Win32.Structures

Imports Microsoft.Win32.SafeHandles

#End Region

#Region " UI Automation "

' ReSharper disable once CheckNamespace

Namespace DevCase.Core.Diagnostics.Processes

    ''' <summary>
    ''' Contains related UI automation utilities.
    ''' </summary>
    <ImmutableObject(True)>
    Public NotInheritable Class UtilUIAutomation

#Region " Constructors "

        ''' <summary>
        ''' Prevents a default instance of the <see cref="UtilUIAutomation"/> class from being created.
        ''' </summary>
        <DebuggerNonUserCode>
        Private Sub New()
        End Sub

#End Region

#Region " Public Methods "

        ''' <summary>
        ''' Blocks the calling thread until a visible top-level window with 
        ''' the specified class name, title and owner process ID is found within the specified time, 
        ''' then returns its window handle.
        ''' <para></para>
        ''' If multiple top-level windows match the specified criteria, this function returns the first one found.
        ''' <para></para>
        ''' To wait for multiple top-level windows matching the criteria and return all matching window handles, 
        ''' consider using <see cref="WaitForTopLevelWindows"/> function instead.
        ''' </summary>
        '''
        ''' <param name="ownerPid">
        ''' The process ID that owns the top-level window to wait for.
        ''' </param>
        '''
        ''' <param name="classname">
        ''' The class name of the top-level window to wait for (e.g. <b>#32768</b> for a context menu window).
        ''' </param>
        '''
        ''' <param name="title">
        ''' The window title (caption) of the top-level window to wait for.
        ''' <para></para>
        ''' If this value is <see langword="Nothing"/>, the function will match any window title, only filtering by <paramref name="classname"/>.
        ''' </param>
        '''
        ''' <param name="maxTimeout">
        ''' Maximum time to wait for the top-level window to appear.
        ''' <para></para>
        ''' If at least one window matching the specified criteria is found, the function will return immediately, 
        ''' not completing the full timeout period.
        ''' </param>
        '''
        ''' <returns>
        ''' A handle to the first matching top-level window if found; otherwise, <see cref="IntPtr.Zero"/>.
        ''' </returns>
        '''
        ''' <exception cref="ArgumentOutOfRangeException">
        ''' Thrown if the provided process ID or timeout is not a positive value.
        ''' </exception>
        ''' 
        ''' <exception cref="ArgumentNullException">
        ''' Thrown if the provided classname is null.
        ''' </exception>
        <DebuggerStepThrough>
        Public Shared Function WaitForFirstTopLevelWindow(ownerPid As Integer, classname As String, title As String, maxTimeout As TimeSpan) As IntPtr

            If ownerPid <= 0 Then
                Throw New ArgumentOutOfRangeException(NameOf(ownerPid), "Process ID must be a positive value.")
            End If

            If maxTimeout <= TimeSpan.Zero Then
                Throw New ArgumentOutOfRangeException(NameOf(Timeout), "Timeout must be a positive value.")
            End If

            If String.IsNullOrEmpty(classname) Then
                Throw New ArgumentNullException(NameOf(classname))
            End If

            Dim desktopHandle As IntPtr = IntPtr.Zero
            Dim stopwatch As Stopwatch = Stopwatch.StartNew()

            Do While stopwatch.Elapsed < maxTimeout

                Dim currentHwnd As IntPtr = NativeMethods.FindWindowEx(desktopHandle, IntPtr.Zero, classname, title)
                Do While currentHwnd <> IntPtr.Zero
                    Dim windowPid As Integer = 0
                    NativeMethods.GetWindowThreadProcessId(currentHwnd, windowPid)
                    If (windowPid = ownerPid) AndAlso NativeMethods.IsWindowVisible(currentHwnd) Then
                        Return currentHwnd
                    End If

                    ' Continue enumerating siblings (other top-level windows of the same class and title).
                    currentHwnd = NativeMethods.FindWindowEx(desktopHandle, currentHwnd, classname, title)
                Loop

                Thread.Sleep(50)
            Loop

            Return IntPtr.Zero
        End Function

        ''' <summary>
        ''' Blocks the calling thread until a visible top-level window with 
        ''' the specified class name, title and owner process is found within the specified time, 
        ''' then returns its window handle.
        ''' <para></para>
        ''' If multiple top-level windows match the specified criteria, this function returns the first one found.
        ''' <para></para>
        ''' To wait for multiple top-level windows matching the criteria and return all matching window handles, 
        ''' consider using <see cref="WaitForTopLevelWindows"/> function instead.
        ''' </summary>
        '''
        ''' <param name="pr">
        ''' The process that owns the top-level window to wait for.
        ''' </param>
        '''
        ''' <param name="classname">
        ''' The class name of the top-level window to wait for (e.g. <b>#32768</b> for a context menu window).
        ''' </param>
        '''
        ''' <param name="title">
        ''' The window title (caption) of the top-level window to wait for.
        ''' <para></para>
        ''' If this value is <see langword="Nothing"/>, the function will match any window title, only filtering by <paramref name="classname"/>.
        ''' </param>
        '''
        ''' <param name="maxTimeout">
        ''' Maximum time to wait for the top-level window to appear.
        ''' <para></para>
        ''' If at least one window matching the specified criteria is found, the function will return immediately, 
        ''' not completing the full timeout period.
        ''' </param>
        '''
        ''' <returns>
        ''' A handle to the first matching top-level window if found; otherwise, <see cref="IntPtr.Zero"/>.
        ''' </returns>
        <DebuggerStepThrough>
        Public Shared Function WaitForFirstTopLevelWindow(pr As Process, classname As String, title As String, maxTimeout As TimeSpan) As IntPtr

            Return WaitForFirstTopLevelWindow(pr.Id, classname, title, maxTimeout)
        End Function

        ''' <summary>
        ''' Blocks the calling thread until a visible top-level window with 
        ''' the specified class name and owner process ID is found within the specified time, 
        ''' then returns its window handle.
        ''' <para></para>
        ''' If multiple top-level windows match the specified criteria, this function returns the first one found.
        ''' <para></para>
        ''' To wait for multiple top-level windows matching the criteria and return all matching window handles, 
        ''' consider using <see cref="WaitForTopLevelWindows"/> function instead.
        ''' </summary>
        '''
        ''' <param name="ownerPid">
        ''' The process ID that owns the top-level window to wait for.
        ''' </param>
        '''
        ''' <param name="classname">
        ''' The class name of the top-level window to wait for (e.g. <b>#32768</b> for a context menu window).
        ''' </param>
        '''
        ''' <param name="maxTimeout">
        ''' Maximum time to wait for the top-level window to appear.
        ''' <para></para>
        ''' If at least one window matching the specified criteria is found, the function will return immediately, 
        ''' not completing the full timeout period.
        ''' </param>
        '''
        ''' <returns>
        ''' A handle to the first matching top-level window if found; otherwise, <see cref="IntPtr.Zero"/>.
        ''' </returns>
        <DebuggerStepThrough>
        Public Shared Function WaitForFirstTopLevelWindow(ownerPid As Integer, classname As String, maxTimeout As TimeSpan) As IntPtr

            Return WaitForFirstTopLevelWindow(ownerPid, classname, Nothing, maxTimeout)
        End Function

        ''' <summary>
        ''' Gets the buttons of a Toolbar control (a window with "ToolbarWin32" class name) for a 64-bit process.
        ''' </summary>
        '''
        ''' <example> This is a code example.
        ''' <code language="VB.NET">
        ''' Dim hWnd As IntPtr = UtilNotificationArea.VisibleHwnd
        ''' Dim buttons As ReadOnlyCollection(Of NativeToolBarButton) = UtilUIAutomation.GetToolbarButtons64(hWnd)
        '''
        ''' For Each ntb As NativeToolBarButton In buttons
        '''     Dim sb As New StringBuilder()
        '''     sb.AppendLine(String.Format("{0}: {1}", NameOf(ntb.ProcessId), ntb.ProcessId))
        '''     sb.AppendLine(String.Format("{0}: {1}", NameOf(ntb.ImageListHandle), ntb.ImageListHandle.ToString()))
        '''     sb.AppendLine(String.Format("{0}: {1}", NameOf(ntb.ButtonIndex), ntb.ButtonIndex))
        '''     sb.AppendLine(String.Format("{0}: {1}", NameOf(ntb.BitmapIndex), ntb.BitmapIndex))
        '''     sb.AppendLine(String.Format("{0}: {1}", NameOf(ntb.CommandId), ntb.CommandId))
        '''     sb.AppendLine(String.Format("{0}: {1}", NameOf(ntb.State), ntb.State.ToString()))
        '''     sb.AppendLine(String.Format("{0}: {1}", NameOf(ntb.Style), ntb.Style.ToString()))
        '''     sb.AppendLine(String.Format("{0}: {1}", NameOf(ntb.ToolTipText), ntb.ToolTipText))
        '''     sb.AppendLine(String.Format("{0}: {1}", NameOf(ntb.Bounds), ntb.Bounds.ToString()))
        '''
        '''     Console.WriteLine(sb.ToString())
        ''' Next ntb
        ''' </code>
        ''' </example>
        '''
        ''' <param name="hWnd">
        ''' A handle to the Toolbar control (a window with "ToolbarWin32" class name).
        ''' </param>
        '''
        ''' <returns>
        ''' The toolbar buttons.
        ''' </returns>
        <DebuggerStepThrough>
        Public Shared Function GetToolbarButtons64(hWnd As IntPtr) As ReadOnlyCollection(Of NativeToolBarButton)

            If Not Environment.Is64BitProcess Then
                Throw New PlatformNotSupportedException("This method is only supported in 64-bit processes.")
            End If

            Dim pid As Integer
            Dim buttons As New Collection(Of NativeToolBarButton)

            If NativeMethods.GetWindowThreadProcessId(hWnd, pid) <> 0 Then
                Dim hProcess As SafeProcessHandle = NativeMethods.OpenProcess(ProcessAccessRights.VirtualMemoryOperation Or ProcessAccessRights.VirtualMemoryRead, False, pid)
                If Not hProcess.IsInvalid Then
                    Dim hSection As IntPtr
                    Dim pageSize As ULong = &H4000
                    Dim viewSize As UIntPtr
                    Dim baseAddress As IntPtr
                    Dim remoteBaseAddress As IntPtr

                    If NativeMethods.NtCreateSection(hSection, SectionAccessRights.AllAccess,
                                                      IntPtr.Zero, pageSize,
                                                      MemoryProtectionOptions.ReadWrite,
                                                      SectionAttributes.Commit,
                                                      IntPtr.Zero) = 0 Then ' NTStatus.SUCCESS Then

                        If NativeMethods.NtMapViewOfSection(hSection, NativeMethods.GetCurrentProcess(), baseAddress,
                                                             IntPtr.Zero, IntPtr.Zero, 0, viewSize,
                                                             SectionInherit.ViewUnmap,
                                                             MemoryAllocationType.Default,
                                                             MemoryProtectionOptions.ReadWrite) = 0 Then ' NTStatus.SUCCESS Then

                            If NativeMethods.NtMapViewOfSection(hSection, hProcess, remoteBaseAddress,
                                                                 IntPtr.Zero, IntPtr.Zero, 0, viewSize,
                                                                 SectionInherit.ViewUnmap,
                                                                 MemoryAllocationType.Default,
                                                                 MemoryProtectionOptions.ReadWrite) = 0 Then ' NTStatus.SUCCESS Then

                                Dim btCount As Integer = NativeMethods.SendMessage(hWnd, ToolbarMessages.ButtonCount, IntPtr.Zero, IntPtr.Zero).ToInt32()
                                For i As Integer = 0 To btCount - 1

                                    If NativeMethods.SendMessage(hWnd, ToolbarMessages.GetButton, New IntPtr(i), remoteBaseAddress) <> IntPtr.Zero Then
                                        Dim tb As Structures.ToolbarButton = CType(Marshal.PtrToStructure(baseAddress, GetType(Structures.ToolbarButton)), Structures.ToolbarButton)

                                        Dim ntb As New NativeToolBarButton With {
                                            .ToolbarHandle = hWnd,
                                            .ProcessId = pid,
                                            .BitmapIndex = tb.BitmapIndex,
                                            .CommandId = tb.CommandId,
                                            .State = tb.State,
                                            .Style = tb.Style,
                                            .ButtonIndex = NativeMethods.SendMessage(hWnd, ToolbarMessages.CommandIdToIndex, New IntPtr(i), IntPtr.Zero).ToInt32(),
                                            .ImageListHandle = NativeMethods.SendMessage(hWnd, ToolbarMessages.GetImageList, IntPtr.Zero, IntPtr.Zero)
                                        }

                                        NativeMethods.SendMessage(hWnd, ToolbarMessages.GetRect, New IntPtr(i), remoteBaseAddress)
                                        ntb.Bounds = CType(Marshal.PtrToStructure(baseAddress, GetType(Rectangle)), Rectangle)

                                        NativeMethods.SendMessage(hWnd, ToolbarMessages.GetButtonTextUnicode, New IntPtr(i), remoteBaseAddress)
                                        ntb.ToolTipText = Marshal.PtrToStringUni(baseAddress)

                                        Dim trayDataSize As New IntPtr(Marshal.SizeOf(GetType(TrayData64)))
                                        Dim buffer As IntPtr = Marshal.AllocHGlobal(trayDataSize)

                                        If NativeMethods.ReadProcessMemory(hProcess, tb.Data, buffer, trayDataSize, IntPtr.Zero) Then
                                            Dim trayData As TrayData64 = DirectCast(Marshal.PtrToStructure(buffer, GetType(TrayData64)), TrayData64)

                                            ' Call GetIconInfo() function to validate icon handle
                                            Dim iconInfo As New IconInfo()
                                            If NativeMethods.GetIconInfo(trayData.IconHandle, iconInfo) Then
                                                ntb.Icon = Icon.FromHandle(trayData.IconHandle)
                                                ntb.IconHandle = trayData.IconHandle
                                            Else
                                                ntb.Icon = Nothing
                                                ntb.IconHandle = IntPtr.Zero
                                                ' Throw New Win32Exception(Marshal.GetLastWin32Error)
                                            End If

                                        Else
                                            Throw New Win32Exception(Marshal.GetLastWin32Error)

                                        End If

                                        NativeMethods.NtUnmapViewOfSection(hSection, baseAddress)
                                        buttons.Add(ntb)
                                    Else
                                        Throw New Win32Exception(Marshal.GetLastWin32Error())

                                    End If
                                Next

                                NativeMethods.NtUnmapViewOfSection(hProcess, remoteBaseAddress)
                            Else
                                Throw New Win32Exception(Marshal.GetLastWin32Error())

                            End If

                            NativeMethods.NtUnmapViewOfSection(NativeMethods.GetCurrentProcess(), baseAddress)
                        Else
                            Throw New Win32Exception(Marshal.GetLastWin32Error())

                        End If

                        NativeMethods.CloseHandle(hSection)
                    Else
                        Throw New Win32Exception(Marshal.GetLastWin32Error())

                    End If

                    NativeMethods.CloseHandle(hProcess)
                Else
                    Throw New Win32Exception(Marshal.GetLastWin32Error())

                End If
            Else
                Throw New Win32Exception(Marshal.GetLastWin32Error())

            End If

            Return (From ntb As NativeToolBarButton In buttons Order By ntb.ButtonIndex).ToList().AsReadOnly()

        End Function

        ''' <summary>
        ''' Retrieves the current buttons in the system tray (notification area) on the Windows taskbar.
        ''' </summary>
        '''
        ''' <example> This is a code example.
        ''' <code language="VB">
        ''' Dim sysTrayButtons As AutomationElementCollection = GetSystemTrayButtons()
        ''' For Each el As AutomationElement In sysTrayButtons
        '''     Dim info As AutomationElementInformation = el.Current
        ''' 
        '''     Console.WriteLine($"Name: {info.Name}")
        '''     Console.WriteLine($"Class Name: {info.ClassName}")
        '''     Console.WriteLine($"HasKeyboardFocus: {info.HasKeyboardFocus}")
        '''     Console.WriteLine($"BoundingRectangle: {info.BoundingRectangle}")
        '''     Console.WriteLine("")
        ''' Next
        ''' </code>
        ''' </example>
        '''
        ''' <returns>
        ''' An <see cref="AutomationElementCollection"/> containing <see cref="AutomationElement"/> objects
        ''' for each system tray button (notification area icon) found.
        ''' </returns>
        <DebuggerStepThrough>
        Public Shared Function GetSystemTrayButtons() As AutomationElementCollection

            Dim trayNotifyClassNames As String() = {"TrayNotifyWnd"}
            Return UtilUIAutomation.GetButtonsFromTaskbarChild(trayNotifyClassNames)
        End Function

        ''' <summary>
        ''' Invokes a mouse button click on a icon in the system tray. 
        ''' <para></para>
        ''' Typically you would invoke <see cref="MouseButtonClick.RightButton"/> to open its context menu window, 
        ''' then call <see cref="WaitForFirstTopLevelWindow"/> function with "<b>#32768</b>" classname.
        ''' </summary>
        ''' 
        ''' <param name="icon">
        ''' The system tray toolbar button.
        ''' </param>
        ''' 
        ''' <param name="mouseButton">
        ''' The mouse button click to send.
        ''' </param>
        ''' 
        ''' <exception cref="ArgumentNullException">
        ''' Thrown when the provided NativeToolBarButton is invalid.
        ''' </exception>
        ''' 
        ''' <exception cref="InvalidEnumArgumentException">
        ''' Thrown when a invalid MouseButtonClick value is provided.
        ''' </exception>
        ''' 
        ''' <exception cref="Win32Exception">
        ''' Thrown when the underlying PostMessage function call fails.
        ''' </exception>
        <DebuggerStepThrough>
        Public Shared Sub InvokeMouseButtonClickOnSysTrayIcon(icon As NativeToolBarButton, mouseButton As MouseButtonClick)

            If Not Environment.Is64BitProcess Then
                Throw New PlatformNotSupportedException("This method is only supported in 64-bit processes.")
            End If

            If icon.IconHandle = IntPtr.Zero OrElse
               icon.ToolbarHandle = IntPtr.Zero OrElse
               icon.CommandId = 0 Then

                Throw New ArgumentNullException(NameOf(icon))
            End If

            Dim hWndToolbar As IntPtr = icon.ToolbarHandle

            Dim iconBounds As Rectangle = icon.Bounds
            Dim iconLocation As New Drawing.Point(iconBounds.Left, iconBounds.Top)
            NativeMethods.ClientToScreen(hWndToolbar, iconLocation)

            Dim iconCenterPoint As New Drawing.Point(
                iconLocation.X + (iconBounds.Size.Width \ icon.ButtonIndex),
                iconLocation.Y + (iconBounds.Bottom \ 2)
            )

            Dim refTrayData As TrayData64 = Nothing
            Dim refCallbackMessage As UInteger
            UtilUIAutomation.GetCallbackMessage64(icon, refTrayData, refCallbackMessage)

            Dim wParam As IntPtr = UtilWin32.MakeWParam(iconCenterPoint.X, iconCenterPoint.Y)

            Dim lParam As IntPtr
            Select Case mouseButton

                Case MouseButtonClick.LeftButton
                    lParam = New IntPtr(WindowMessages.WM_LButtonUp)

                Case MouseButtonClick.RightButton
                    lParam = New IntPtr(WindowMessages.WM_RButtonUp)

                Case MouseButtonClick.MiddleButton
                    lParam = New IntPtr(WindowMessages.WM_MButtonUp)

                Case Else
                    Throw New InvalidEnumArgumentException(
                    NameOf(mouseButton), mouseButton, GetType(MouseButtonClick))
            End Select

            Dim success As Boolean = NativeMethods.PostMessage(refTrayData.Hwnd, CInt(refCallbackMessage), wParam, lParam)

            If Not success Then
                Throw New Win32Exception(Marshal.GetLastWin32Error(),
                $"{NameOf(NativeMethods.PostMessage)} failed for system tray icon callback message 0x{refCallbackMessage:X}.")
            End If
        End Sub

        ''' <summary>
        ''' Invokes a menu item in a menu window (classname: <b>#32768</b>).
        ''' </summary>
        ''' 
        ''' <param name="menuHwnd">
        ''' The window handle of the menu.
        ''' </param>
        ''' 
        ''' <param name="itemIndex">
        ''' The zero-based index of the menu item to be invoked.
        ''' <para></para>
        ''' Note that separators are also counted in the item index.
        ''' <para></para>
        ''' Example menu structure:
        ''' <list type="bullet">
        '''     <item><description>0: Open</description></item>
        '''     <item><description>1: Separator</description></item>
        '''     <item><description>2: Connect</description></item>
        '''     <item><description>3: Exit</description></item>
        ''' </list>
        ''' Usage example:
        ''' <list type="bullet">
        '''     <item><description>itemIndex = 0 → invokes "Open"</description></item>
        '''     <item><description>itemIndex = 1 → throws an exception since it's a separator (not invokable)</description></item>
        '''     <item><description>itemIndex = 2 → invokes "Connect"</description></item>
        '''     <item><description>itemIndex = 3 → invokes "Exit"</description></item>
        ''' </list>
        ''' </param>
        ''' 
        ''' <exception cref="ArgumentNullException">
        ''' Thrown if the provided menu handle is NULL.
        ''' </exception>
        ''' 
        ''' <exception cref="ArgumentException">
        ''' Thrown if the provided menu handle does not correspond to a valid menu window.
        ''' </exception>
        ''' 
        ''' <exception cref="ArgumentOutOfRangeException">
        ''' Thrown if the provided item index is negative or exceeds the total number of items in the menu.
        ''' </exception>
        <DebuggerStepThrough>
        Public Shared Sub InvokeMenuItem(menuHwnd As IntPtr, itemIndex As Integer)

            If menuHwnd = IntPtr.Zero Then
                Throw New ArgumentNullException(NameOf(menuHwnd), "The window handle cannot be null.")
            End If

            Dim classNameBuilder As New StringBuilder(256)
            NativeMethods.GetClassName(menuHwnd, classNameBuilder, classNameBuilder.Capacity)

            Dim className As String = classNameBuilder.ToString()

            Dim PopupMenuClassName As String = "#32768"
            If Not className.Equals(PopupMenuClassName) Then
                Throw New ArgumentException($"Window handle does not point to a valid menu window. Class: {className}", NameOf(menuHwnd))
            End If

            Const MN_GETHMENU As Integer = &H1E1
            Dim hMenu As IntPtr = NativeMethods.SendMessage(menuHwnd, MN_GETHMENU, IntPtr.Zero, IntPtr.Zero)

            If hMenu = IntPtr.Zero Then
                Throw New ArgumentException("Failed to retrieve the HMENU handle from window.", NameOf(menuHwnd))
            End If

            Dim totalItems As Integer = NativeMethods.GetMenuItemCount(hMenu)
            If itemIndex < 0 OrElse itemIndex >= totalItems Then
                Throw New ArgumentOutOfRangeException(NameOf(itemIndex), $"item index {itemIndex} is out of range. Total items count: {totalItems}")
            End If

            Dim itemId As Integer = NativeMethods.GetMenuItemID(hMenu, itemIndex)
            Dim ownerHwnd As IntPtr = NativeMethods.GetWindow(menuHwnd, GetWindowCmd.Owner)

            ' --- Validation logic for Disabled/Grayed status ---
            Dim itemInfo As New MenuItemInfo(initializeSize:=True) With {
                .Mask = MenuItemMask.State
            }

            If Not NativeMethods.GetMenuItemInfo(hMenu, CUInt(itemId), False, itemInfo) Then
                Throw New InvalidOperationException($"Failed to retrieve menu item info for index {itemIndex}.")
            End If

            Dim isDisabled As Boolean = (itemInfo.State And MenuItemInfoStates.Disabled_Greyed) <> 0
            If isDisabled Then
                ' ⚠️ Method "UtilNetwork.ChangeProtonVpnServerBySysTrayContextMenu" uses 'Like' operator to parse  
                ' this exception message to detect disabled menu items, so be cautious when modifying it.
                Throw New InvalidOperationException($"The menu item at index {itemIndex} is disabled.")
            End If

            ' If we have a valid ID and owner, send WM_COMMAND directly (cleanest approach).
            If ownerHwnd <> IntPtr.Zero AndAlso itemId <> -1 Then
                NativeMethods.PostMessage(ownerHwnd, WindowMessages.WM_Command, CType(itemId, IntPtr), IntPtr.Zero)

            Else ' Fallback logic.
                Dim pressCount As Integer = 0
                For i As Integer = 0 To itemIndex
#Disable Warning BC40000 ' Type or member is obsolete
                    Dim flags As UInteger = NativeMethods.GetMenuState(hMenu, CUInt(i), MenuPosition.ByPosition)
#Enable Warning BC40000 ' Type or member is obsolete
                    Dim isSeparator As Boolean = (flags And MenuItemType.Separator) <> 0UI
                    If Not isSeparator Then
                        pressCount += 1
                    End If
                Next

                For i As Integer = 1 To pressCount
                    NativeMethods.PostMessage(menuHwnd, WindowMessages.WM_KeyDown, CType(VirtualKeys.Down, IntPtr), IntPtr.Zero)
                    Thread.Sleep(100)
                Next
                NativeMethods.PostMessage(menuHwnd, WindowMessages.WM_KeyDown, CType(VirtualKeys.Return, IntPtr), IntPtr.Zero)
                Thread.Sleep(100)
            End If

        End Sub

#End Region

#Region " Private Methods "

        ''' <summary>
        ''' Retrieves all button elements from a specific child of the Windows taskbar, identified by its class name(s).
        ''' </summary>
        ''' 
        ''' <param name="classNames">
        ''' An array of class names to search for among the immediate children of the taskbar.
        ''' <para></para>
        ''' The first matching child will be used as the container for buttons.
        ''' </param>
        ''' 
        ''' <returns>
        ''' An <see cref="AutomationElementCollection"/> containing all <see cref="ControlType.Button"/> elements
        ''' found within the first child that matches one of the specified class names.
        ''' <para></para>
        ''' Returns null if the taskbar window cannot be found, if the specified child class is not present,
        ''' or if UI Automation is unable to access the elements.
        ''' </returns>
        <DebuggerStepThrough>
        Private Shared Function GetButtonsFromTaskbarChild(classNames As String()) As AutomationElementCollection

            Dim shellHwnd As IntPtr = NativeMethods.FindWindow("Shell_TrayWnd", Nothing)
            If shellHwnd = IntPtr.Zero Then
                Return Nothing
            End If

            Dim taskbarRoot As AutomationElement = AutomationElement.FromHandle(shellHwnd)
            If taskbarRoot Is Nothing Then
                Return Nothing
            End If

            Dim taskbarChildren As AutomationElementCollection = taskbarRoot.FindAll(TreeScope.Children, Condition.TrueCondition)

            Dim targetChild As AutomationElement = Nothing

            ' Find the child whose class matches the specified names.
            For Each child As AutomationElement In taskbarChildren
                Dim className As String = String.Empty
                Try
                    className = child.Current.ClassName
                Catch ex As ElementNotAvailableException
                    Continue For
                End Try

                If classNames.Any(Function(c As String) String.Equals(c, className, StringComparison.OrdinalIgnoreCase)) Then
                    targetChild = child
                    Exit For
                End If
            Next

            If targetChild Is Nothing Then
                Return Nothing
            End If

            Dim buttonCondition As New PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Button)

            Return targetChild.FindAll(TreeScope.Descendants, buttonCondition)
        End Function

        ''' <summary>
        ''' Retrieves the callback message and tray data associated with a system tray icon for a 64-bit process.
        ''' <para></para>
        ''' This method reads memory from the owning process of the system tray toolbar.
        ''' </summary>
        ''' 
        ''' <param name="icon">
        ''' The system tray toolbar button.
        ''' </param>
        ''' 
        ''' <param name="refTrayData">
        ''' When this method returns, contains the retrieved <see cref="TrayData64"/> structure.
        ''' </param>
        ''' 
        ''' <param name="refCallbackMessage">
        ''' When this method returns, contains the callback message identifier.
        ''' </param>
        ''' 
        ''' <exception cref="ArgumentNullException">
        ''' Thrown when the provided NativeToolBarButton is invalid.
        ''' </exception>
        ''' 
        ''' <exception cref="Win32Exception">
        ''' Thrown when native process operations (OpenProcess, ReadProcessMemory, VirtualAllocEx) fails.
        ''' </exception>
        ''' 
        ''' <exception cref="InvalidOperationException">
        ''' Thrown when retrieved memory structures are invalid or contain unexpected values.
        ''' </exception>
        <DebuggerStepThrough>
        Private Shared Sub GetCallbackMessage64(icon As NativeToolBarButton,
                                        ByRef refTrayData As TrayData64,
                                        ByRef refCallbackMessage As UInteger)

            If icon.IconHandle = IntPtr.Zero OrElse
               icon.ToolbarHandle = IntPtr.Zero OrElse
               icon.CommandId = 0 Then

                Throw New ArgumentNullException(NameOf(icon))
            End If

            Dim hWndToolbar As IntPtr = UtilNotificationArea.VisibleHwnd

            Dim iconIndex As Integer = icon.ButtonIndex

            Dim parentPid As Integer = icon.ProcessId ' usually Explorer.exe.

            Dim parentProcessAccessRights As ProcessAccessRights =
                ProcessAccessRights.VirtualMemoryOperation Or
                ProcessAccessRights.VirtualMemoryRead

            Dim safeParentProc As SafeProcessHandle = Nothing
            Dim remoteBuffer As IntPtr
            Dim lastWin32Error As Integer

            Try
                safeParentProc = NativeMethods.OpenProcess(parentProcessAccessRights, inheritHandle:=False, pid:=parentPid)
                lastWin32Error = Marshal.GetLastWin32Error()

                Dim processName As String = "<unknown>"
                Try
                    processName = Process.GetProcessById(parentPid).ProcessName
                Catch
                End Try

                If safeParentProc.IsInvalid Then
                    Throw New Win32Exception(lastWin32Error,
                    $"Failed to open process '{processName}' (PID {parentPid}) with required access rights: {parentProcessAccessRights}")
                End If

                ' NativeToolBarButton requires the buffer to reside in the memory space of the toolbar's owning process,
                ' therefore we use VirtualAllocEx instead of a local buffer.
                Dim tbSize As IntPtr = CType(Marshal.SizeOf(GetType(NativeToolBarButton)), IntPtr)
                remoteBuffer = NativeMethods.VirtualAllocEx(safeParentProc, IntPtr.Zero, tbSize, MemoryAllocationType.Commit, MemoryProtectionOptions.ReadWrite)
                lastWin32Error = Marshal.GetLastWin32Error()

                If remoteBuffer = IntPtr.Zero Then
                    Throw New Win32Exception(lastWin32Error,
                        $"{NameOf(NativeMethods.VirtualAllocEx)} failed while allocating buffer in process '{processName}' (PID {parentPid}).")
                End If

                Dim toolbarButtonBytes(CInt(tbSize) - 1) As Byte
                Dim trayDataBytes(Marshal.SizeOf(GetType(TrayData64)) - 1) As Byte
                Dim bytesRead As IntPtr

                ' Request the toolbar to write into the remote buffer (in parent process).
                NativeMethods.SendMessage(hWndToolbar, ToolbarMessages.GetButton, New IntPtr(iconIndex), remoteBuffer)
                Dim readMemoryResult As Boolean = NativeMethods.ReadProcessMemory(safeParentProc, remoteBuffer, toolbarButtonBytes, tbSize, bytesRead)
                lastWin32Error = Marshal.GetLastWin32Error()

                If Not readMemoryResult Then
                    Throw New Win32Exception(lastWin32Error,
                    $"{NameOf(NativeMethods.ReadProcessMemory)} failed while reading toolbar button data from process '{processName}' (PID {parentPid}).")
                End If

                Dim tbutton As Win32.Structures.ToolbarButton = MarshallingExtensions.ConvertToStructure(Of Win32.Structures.ToolbarButton)(toolbarButtonBytes)
                If tbutton.Data = UIntPtr.Zero Then
                    Throw New InvalidOperationException($"Toolbar button data pointer ({NameOf(Win32.Structures.ToolbarButton.Data)}) is NULL.")
                End If

                ' Read TrayData pointed to by ToolbarButton.Data (also in parent process memory).
                Dim trayDataSize As UIntPtr = CType(trayDataBytes.Length, UIntPtr)
                readMemoryResult = NativeMethods.ReadProcessMemory(safeParentProc, tbutton.Data, trayDataBytes, trayDataSize, bytesRead)
                lastWin32Error = Marshal.GetLastWin32Error()

                If Not readMemoryResult Then
                    Throw New Win32Exception(lastWin32Error,
                    $"{NameOf(NativeMethods.ReadProcessMemory)} failed while reading {NameOf(Structures.TrayData64)} from process '{processName}' (PID {parentPid}).")
                End If

                Dim trayData As TrayData64 = MarshallingExtensions.ConvertToStructure(Of TrayData64)(trayDataBytes)
                If trayData.Hwnd = IntPtr.Zero OrElse
               Not NativeMethods.IsWindow(trayData.Hwnd) Then

                    Throw New InvalidOperationException(
                    $"Invalid window handle in {NameOf(trayData)} (button index {iconIndex}).")
                End If

                If trayData.CallbackMessage = 0 Then
                    Throw New InvalidOperationException(
                      $"{NameOf(trayData)} contains a zero callback message (button index {iconIndex}).")
                End If

                refTrayData = trayData
                refCallbackMessage = trayData.CallbackMessage

            Finally
                If (remoteBuffer <> IntPtr.Zero) AndAlso
                   (safeParentProc IsNot Nothing) AndAlso
                   (Not safeParentProc.IsInvalid) Then

                    NativeMethods.VirtualFreeEx(safeParentProc, remoteBuffer, UIntPtr.Zero, MemoryFreeType.Release)
                End If

                safeParentProc?.Dispose()

            End Try

        End Sub

#End Region

    End Class

End Namespace

#End Region

#Enable Warning BC42309 ' XML comment has a tag with a 'cref' attribute that could not be resolved