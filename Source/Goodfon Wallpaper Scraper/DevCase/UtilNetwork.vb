
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Diagnostics
Imports System.Diagnostics.CodeAnalysis
Imports System.Drawing
Imports System.IO
Imports System.Linq
Imports System.Net.NetworkInformation
Imports System.ServiceProcess
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Threading
Imports System.Windows
Imports System.Windows.Automation

Imports DevCase.Core.Diagnostics.Processes
Imports DevCase.Core.IO.Devices.Input
Imports DevCase.Core.Windows.Desktop
Imports DevCase.Win32
Imports DevCase.Win32.Enums
Imports DevCase.Win32.Structures

#End Region

#Region " Network Util "

' ReSharper disable once CheckNamespace

Namespace DevCase.Core.Networking.Common

    ''' <summary>
    ''' Contains networking related utilities.
    ''' </summary>
    <SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification:="Required to migrate this code to .NET Core")>
    Public NotInheritable Class UtilNetwork

#Region " Constructors "

        ''' <summary>
        ''' Prevents a default instance of the <see cref="UtilNetwork"/> class from being created.
        ''' </summary>
        <DebuggerNonUserCode>
        Private Sub New()
        End Sub

#End Region

#Region " Public Methods "

        ''' <summary>
        ''' Determines whether the user is running the Free version of ProtonVPN client for Windows by inspecting the latest UserSettings JSON file.
        ''' </summary>
        ''' 
        ''' <returns>
        ''' <see langword="True"/> if the allowed devices count indicates the Free tier; otherwise, <see langword="False"/>.
        ''' </returns>
        ''' 
        ''' <exception cref="DirectoryNotFoundException">
        ''' Thrown when the Proton VPN storage directory does not exist.
        ''' </exception>
        ''' 
        ''' <exception cref="FileNotFoundException">
        ''' Thrown when no 'UserSettings.*.json' files are found in the directory.
        ''' </exception>
        <DebuggerStepThrough>
        Public Shared Function IsProtonVpnClientFree() As Boolean

            Dim localAppData As String = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
            Dim storagePath As String = Path.Combine(localAppData, "Proton", "Proton VPN", "Storage")

            If Not Directory.Exists(storagePath) Then
                Throw New DirectoryNotFoundException($"The Proton VPN storage directory was not found at: '{storagePath}'.")
            End If

            ' List files matching the wildcard pattern
            Dim searchPattern As String = "UserSettings.*.json"
            Dim configFiles As String() = Directory.GetFiles(storagePath, searchPattern, SearchOption.TopDirectoryOnly)

            If configFiles.Length = 0 Then
                Throw New FileNotFoundException($"No 'UserSettings.*.json' files matching the pattern '{searchPattern}' were found in '{storagePath}'.")
            End If

            ' Order files by Last Write Time descending to pick the most recent one
            Dim sortedFiles As String() = configFiles.OrderByDescending(Function(f As String) New FileInfo(f).LastWriteTime).ToArray()
            Dim latestConfigFile As String = sortedFiles(0)

            ' Read the latest file and analyze using Case-Insensitive Regex
            Dim fileContent As String = File.ReadAllText(latestConfigFile, Encoding.Default)

            ' Matches "MaxDevicesAllowed": "X" ignoring spaces and case
            Dim match As Match = Regex.Match(fileContent, """MaxDevicesAllowed""\s*:\s*""(\d+)""", RegexOptions.IgnoreCase)

            Dim isFreeAccount As Boolean = False

            If match.Success Then
                Dim maxDevices As Integer = Convert.ToInt32(match.Groups(1).Value)

                ' Free tier accounts are limited to 1 or 2 devices. Paid plans offer 10 or more.
                If maxDevices <= 2 Then
                    isFreeAccount = True
                End If
            End If

            Return isFreeAccount
        End Function

        ''' <summary>
        ''' Attempts to connect or randomly change the current VPN server in the ProtonVPN client for Windows.
        ''' <para></para>
        ''' Three strategies are attempted in the following order:
        ''' <list type="number">
        ''' <item>
        '''     <description>
        '''         Restarting the ProtonVPN Windows service.
        '''     </description>
        ''' </item>
        ''' <item>
        '''     <description>
        '''         Automating the right-click context menu of the ProtonVPN system tray icon.
        '''     </description>
        ''' </item>
        ''' <item>
        '''     <description>
        '''         Automating the main application graphical user interface (GUI) of the ProtonVPN client.
        '''     </description>
        ''' </item>
        ''' </list>
        ''' </summary>
        ''' 
        ''' <remarks>
        ''' <para>Requirements:</para>
        ''' <list type="bullet">
        ''' <item>
        '''     <description>
        '''         The Proton VPN client must be running.
        '''     </description>
        ''' <item>
        '''     <description>
        '''         The "Random Country" mode must be enabled within the client settings.
        '''     </description>
        ''' </item>
        ''' </item>
        ''' </list>
        ''' </remarks>
        ''' 
        ''' <exception cref="Exception">
        ''' Thrown if all strategies to change the VPN server in the ProtonVPN client have been attempted without success.
        ''' </exception>
        <EditorBrowsable(EditorBrowsableState.Always)>
        <DebuggerStepThrough>
        Public Shared Sub ChangeServerInProtonVpnClient()

            Dim isFreeProtonVpnClient As Boolean
            Try
                isFreeProtonVpnClient = UtilNetwork.IsProtonVpnClientFree()
            Catch ex As DirectoryNotFoundException
                Throw
            Catch ex As FileNotFoundException
                Throw
            Catch
            End Try

            ' 1st. Try change VPN Server via restarting ProtonVPN Windows Service.
            '      Note: In the free ProtonVPN client, restarting the Windows service does not automatically change VPN server.
            If Not isFreeProtonVpnClient Then
                Try
                    UtilNetwork.ChangeProtonVpnServerByWindowsService()
                    Return
                Catch ex As Exception
                End Try
            End If

            ' 2nd. Try change VPN Server via right-click context menu on the ProtonVPN system tray icon.
            '      Note: The free ProtonVPN client does not expose buttons to change the server in teh context menu.
            If Not isFreeProtonVpnClient Then
                Try
                    UtilNetwork.ChangeProtonVpnServerBySysTrayContextMenu()
                    Return
                Catch
                End Try
            End If

            ' 3rd. Try change VPN Server via automating the main application graphical user interface (GUI).
            Try
                UtilNetwork.ChangeProtonVpnServerByGuiAutomation(isFreeProtonVpnClient)
                Return
            Catch
            End Try

            Throw New Exception("Failed to change VPN server in ProtonVPN client. All strategies have been attempted without success.")
        End Sub

#End Region

#Region " Private Methods "

        ''' <summary>
        ''' Attempts to connect or randomly change the current VPN server in the ProtonVPN client for Windows 
        ''' by restarting the ProtonVPN Windows service.
        ''' </summary>
        ''' 
        ''' <remarks>
        ''' <para>Requirements:</para>
        ''' <list type="bullet">
        ''' <item>
        '''     <description>
        '''         The Proton VPN client must be running.
        '''     </description>
        ''' <item>
        '''     <description>
        '''         The "Random Country" mode must be enabled within the client settings.
        '''     </description>
        ''' </item>
        ''' </item>
        ''' </list>
        ''' </remarks>
        ''' 
        ''' <exception cref="InvalidOperationException">
        ''' Thrown when the Proton VPN network interface is not found or is not connected,
        ''' or if the <b>ProtonVPN</b> Windows service fails to complete the restart sequence.
        ''' </exception>
        ''' 
        ''' <exception cref="Exception">
        ''' Thrown when the <b>ProtonVPN</b> Windows service service restarted and network interface is UP, 
        ''' but no data flow (Bytes Received) was detected.
        ''' </exception>
        <EditorBrowsable(EditorBrowsableState.Advanced)>
        <DebuggerStepThrough>
        Private Shared Sub ChangeProtonVpnServerByWindowsService()

            Const ProtonVPN_NetworkInterfaceName As String = "ProtonVPN"
            Const ProtonVPN_WindowsServiceName As String = "ProtonVPN Service"

            Dim isProtonVpnConnected As Boolean = False

            Dim interfaces As NetworkInterface() = NetworkInterface.GetAllNetworkInterfaces()
            For Each ni As NetworkInterface In interfaces
                ' We look for the specific name Proton uses for its virtual adapters
                If ni.Name.Equals(ProtonVPN_NetworkInterfaceName, StringComparison.OrdinalIgnoreCase) Then
                    If ni.OperationalStatus = OperationalStatus.Up Then
                        isProtonVpnConnected = True
                        Exit For
                    End If
                End If
            Next ni

            If Not isProtonVpnConnected Then
                Throw New InvalidOperationException(
                $"The {ProtonVPN_NetworkInterfaceName} network interface is not connected to a server." & Environment.NewLine &
                "The reconnection logic requires an active VPN connection to be present.")
            End If

            ' Restart the ProtonVPN Windows service.
            Using sc As New ServiceController(ProtonVPN_WindowsServiceName)

                Select Case sc.Status

                    Case ServiceControllerStatus.StartPending, ServiceControllerStatus.Running
                        sc.Stop()
                        sc.WaitForStatus(ServiceControllerStatus.StopPending, TimeSpan.FromSeconds(10))

                    Case ServiceControllerStatus.Stopped
                        sc.Start()

                    Case Else
                        Throw New InvalidOperationException(
                        $"The {ProtonVPN_WindowsServiceName} Windows service is in a unexpected state: {sc.Status}")
                End Select

                sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(10))
            End Using

            UtilNetwork.WaitForProtonNetworkInterfaceDataFlow(TimeSpan.FromSeconds(30))
        End Sub

        ''' <summary>
        ''' Attempts to connect or randomly change the current VPN server in the ProtonVPN client for Windows 
        ''' by automating the right-click context menu of the ProtonVPN system tray icon.
        ''' </summary>
        ''' 
        ''' <remarks>
        ''' <para>Requirements:</para>
        ''' <list type="bullet">
        ''' <item>
        '''     <description>
        '''         The Proton VPN client must be running.
        '''     </description>
        ''' <item>
        '''     <description>
        '''         The "Random Country" mode must be enabled within the client settings.
        '''     </description>
        ''' </item>
        ''' </item>
        ''' </list>
        ''' </remarks>
        <EditorBrowsable(EditorBrowsableState.Advanced)>
        <DebuggerStepThrough>
        Private Shared Sub ChangeProtonVpnServerBySysTrayContextMenu()

            Dim hWndToolbar As IntPtr = UtilNotificationArea.VisibleHwnd
            If hWndToolbar = IntPtr.Zero Then
                Throw New Exception("Window ToolbarWindow32 not found.")
            End If

            ' Check if the System Tray toolbar is actually visible on screen
            Dim trayToolbar As AutomationElement = AutomationElement.FromHandle(hWndToolbar)
            If trayToolbar.Current.IsOffscreen Then
                Throw New Exception("The System Tray is currently offscreen or hidden.")
            End If

            Dim ProtonVpnClient_ProcessName As String = "ProtonVPN.Client"
            Dim protonProcess As Process = Process.GetProcessesByName(ProtonVpnClient_ProcessName).SingleOrDefault()
            If protonProcess Is Nothing Then
                Throw New InvalidOperationException($"Process '{ProtonVpnClient_ProcessName}.exe' not found.")
            End If

            Dim oldpos As Drawing.Point = UtilMouse.Position
            NativeMethods.BlockInput(True) ' Block user input to prevent interference during automation.
            Try
                If Environment.Is64BitProcess Then

                    Dim protonIconNative As NativeToolBarButton =
                       (From bt As NativeToolBarButton In UtilNotificationArea.VisibleButtons
                        Where bt.ToolTipText.Equals("Proton VPN", StringComparison.OrdinalIgnoreCase)
                       ).SingleOrDefault()

                    If protonIconNative.IconHandle = IntPtr.Zero Then
                        Throw New Exception("Proton VPN icon was not found in System Tray.")
                    End If

                    ' Invoke right-click on the Proton system tray icon to open its context menu.
                    UtilUIAutomation.InvokeMouseButtonClickOnSysTrayIcon(protonIconNative, MouseButtonClick.RightButton)

                    ' Wait for the Proton context menu window to appear (class name: #32768).
                    Dim menuWindowClassName As String = "#32768"
                    Dim popupMenuHwnd As IntPtr = UtilUIAutomation.WaitForFirstTopLevelWindow(protonProcess.Id, menuWindowClassName, TimeSpan.FromSeconds(5))
                    ' Give time enough for the menu to finish opening and populating its items before trying to interact with it.
                    Thread.Sleep(1000)

                    ' Proton VPN tray menu structure using NativeToolBarButton approach:
                    '
                    '   Index 0: Open Proton VPN
                    '   Index 1: {separator}
                    '   Index 2: Connect
                    '   Index 3: Disconnect
                    '   Index 4: {separator}
                    '   Index 5: Exit

                    Dim connectMenuItemIndex As Integer = 2
                    Dim disconnectMenuItemIndex As Integer = 3

                    ' Firstly, try to click on "Connect" menu item.
                    Dim isConnectMenuItemDisabled As Boolean
                    Try
                        UtilUIAutomation.InvokeMenuItem(popupMenuHwnd, connectMenuItemIndex)
                    Catch ex As InvalidOperationException When ex.Message Like $"The menu item at index {connectMenuItemIndex} is disabled."
                        isConnectMenuItemDisabled = True
                    End Try

                    ' If "Connect" menu item is disabled we are currently connected and need to click on "Disconnect" first.
                    If isConnectMenuItemDisabled Then
                        ' Click on "Disconnect" menu item.
                        UtilUIAutomation.InvokeMenuItem(popupMenuHwnd, disconnectMenuItemIndex)
                        ' Give time enough for the disconnect action to complete and menu to update (e.g. "Connect" item to become enabled).
                        Thread.Sleep(1000)

                        ' Reopen the context menu and click on the "Connect" menu item.
                        UtilUIAutomation.InvokeMouseButtonClickOnSysTrayIcon(protonIconNative, MouseButtonClick.RightButton)
                        popupMenuHwnd = UtilUIAutomation.WaitForFirstTopLevelWindow(protonProcess.Id, menuWindowClassName, TimeSpan.FromSeconds(5))
                        ' Give time enough for the menu to finish opening and populating its items before trying to interact with it.
                        Thread.Sleep(1000)

                        ' Click on "Connect" menu item.
                        UtilUIAutomation.InvokeMenuItem(popupMenuHwnd, connectMenuItemIndex)
                    End If

                Else
                    ' ----------------------------------------------------------------------------------------------------------------------
                    ' PREVIOUS IMPLEMENTATION USING UI AUTOMATION TO INTERACT WITH THE SYSTEM TRAY ICON CONTEXT MENU.
                    ' IT WORKS PERFECT FOR x86 AND x64, BUT IT HAS THE INCONVENIENCE OF MOVING THE MOUSE CURSOR TO PERFORM THE INTERACTIONS.
                    ' ----------------------------------------------------------------------------------------------------------------------

                    Dim protonIcon As AutomationElement =
                        (From el As AutomationElement In UtilUIAutomation.GetSystemTrayButtons.Cast(Of AutomationElement)
                         Where el.Current.Name.Equals("Proton VPN", StringComparison.OrdinalIgnoreCase)
                        ).SingleOrDefault()

                    If protonIcon Is Nothing Then
                        Throw New Exception("Proton VPN icon was not found in System Tray.")
                    End If

                    ' Double check the icon itself is not offscreen (e.g. hidden in the overflow 'arrow' menu)
                    If protonIcon.Current.IsOffscreen Then
                        Throw New Exception("Proton VPN sys-tray icon is currently hidden (check the overflow notification area).")
                    End If

                    ' Action to right-click on the Proton system tray icon to open its context menu.
                    Dim openProtonSysTrayMenuAction As Action =
                        Sub()
                            Dim iconPt As System.Windows.Point = protonIcon.GetClickablePoint()
                            NativeMethods.SetCursorPos(CInt(iconPt.X), CInt(iconPt.Y))
                            UtilMouse.SendMouseInput(MouseButtonClick.RightButton)
                            Thread.Sleep(1000)
                        End Sub

                    ' Action to click on a menu item element (Connect or Disconnect).
                    ' 
                    ' This is required instead of using InvokePattern class from UIAutomation to invoke the menu item,
                    ' because InvokePattern often fails to work with the menu items.
                    Dim clickMenuElementAction As Action(Of AutomationElement) =
                        Sub(element As AutomationElement)
                            Dim rect As Rect = element.Current.BoundingRectangle
                            If rect.Width <= 0 OrElse rect.Height <= 0 Then
                                Throw New Exception("Target menu element has an invalid bounding rectangle.")
                            End If

                            ' Calculate center and perform left mouse button click.
                            NativeMethods.SetCursorPos(CInt(rect.Left + (rect.Width / 2)), CInt(rect.Top + (rect.Height / 2)))
                            Thread.Sleep(100)
                            UtilMouse.SendMouseInput(MouseButtonClick.LeftButton)
                            Thread.Sleep(100)
                        End Sub

                    ' Open Proton context menu.
                    openProtonSysTrayMenuAction()

                    ' Find Proton context menu window by class.
                    Dim root As AutomationElement = AutomationElement.RootElement
                    Dim menuCondition As New PropertyCondition(AutomationElement.ClassNameProperty, "#32768")
                    Dim menuWindow As AutomationElement = If(root.FindFirst(TreeScope.Children, menuCondition),
                                                         root.FindFirst(TreeScope.Descendants, menuCondition))

                    If menuWindow Is Nothing Then
                        Throw New Exception("Proton context menu window (#32768) could not be located.")
                    End If

                    ' Get all Proton menu items.
                    Dim itemCondition As New PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.MenuItem)
                    Dim menuItems As AutomationElementCollection = menuWindow.FindAll(TreeScope.Children, itemCondition)

                    ' Proton VPN tray menu structure using NativeToolBarButton approach:
                    '
                    '   Index 0: Open Proton VPN
                    '   Index 1: Connect
                    '   Index 2: Disconnect
                    '   Index 3: Exit

                    If menuItems.Count < 4 Then
                        Throw New Exception($"Unexpected Proton menu item count. Found {menuItems.Count}, expected at least 4.")
                    End If

                    Dim connectItem As AutomationElement = menuItems(1)
                    Dim disconnectItem As AutomationElement = menuItems(2)

                    If disconnectItem.Current.IsEnabled Then
                        ' Current State: Connected ("Disconnect" menu item is enabled, "Connect" menu item is disabled)

                        ' Diconnect from server.
                        clickMenuElementAction(disconnectItem)
                        Thread.Sleep(1000)

                        ' Re-open Proton context menu to connect again.
                        openProtonSysTrayMenuAction()

                        ' Re-locate the new menu instance
                        menuWindow = If(root.FindFirst(TreeScope.Children, menuCondition),
                                    root.FindFirst(TreeScope.Descendants, menuCondition))

                        If menuWindow Is Nothing Then
                            Throw New Exception("Proton context menu window (#32768) could not be located.")
                        End If

                        menuItems = menuWindow.FindAll(TreeScope.Children, itemCondition)
                        connectItem = menuItems(1)
                    End If

                    ' Current State: Disconnected ("Connect" menu item is enabled, "Disconnect" menu item is disabled)

                    ' Connect to server.
                    clickMenuElementAction(connectItem)
                End If

            Catch
                Throw

            Finally
                UtilMouse.Position = oldpos
                NativeMethods.BlockInput(False) ' Ensure input is unblocked in case of any failure.)

            End Try

            UtilNetwork.WaitForProtonNetworkInterfaceDataFlow(TimeSpan.FromSeconds(30))
        End Sub

        ''' <summary>
        ''' Attempts to connect or randomly change the current VPN server in the ProtonVPN client for Windows 
        ''' by automating the graphical user interface (GUI) window.
        ''' </summary>
        ''' 
        ''' <remarks>
        ''' <para>Requirements for the Free version of the ProtonVPN client:</para>
        ''' <list type="bullet">
        ''' <item>
        '''     <description>
        '''         The Proton VPN client must be running, and the window must not be hidden. 
        '''         <para></para>
        '''         Being minimized to the Windows taskbar is acceptable and does not cause issues.
        '''     </description>
        ''' </item>
        ''' </list>
        ''' 
        ''' <para>Requirements for the Paid version of the ProtonVPN client:</para>
        ''' <list type="bullet">
        ''' <item>
        '''     <description>
        '''         Same requirements as the Free version.
        '''     </description>
        ''' </item>
        ''' <item>
        '''     <description>
        '''         The "Random Country" mode must be enabled within the client settings.
        '''     </description>
        ''' </item>
        ''' </list>
        ''' </remarks>
        ''' 
        ''' <param name="isFreeProtonVpnClient">
        ''' A value indicating whether the ProtonVPN client is the Free version 
        ''' see langword="True"/>) or the Paid version (<see langword="False"/>).
        ''' </param>
        ''' 
        ''' <exception cref="InvalidOperationException">
        ''' Thrown when the ProtonVPN client process is not found.
        ''' <para></para>
        ''' Thrown if the main window handle of the ProtonVPN client process is null.
        ''' </exception>
        ''' 
        ''' <exception cref="Exception">
        ''' Thrown if ProtonVPN client process automation has failed for unexpected reasons.
        ''' </exception>
        <EditorBrowsable(EditorBrowsableState.Advanced)>
        <DebuggerStepThrough>
        Private Shared Sub ChangeProtonVpnServerByGuiAutomation(isFreeProtonVpnClient As Boolean)

            Dim ProtonVpnClient_ProcessName As String = "ProtonVPN.Client"

            Dim pr As Process = Process.GetProcessesByName(ProtonVpnClient_ProcessName).SingleOrDefault()
            If pr Is Nothing Then
                Throw New InvalidOperationException($"Process '{ProtonVpnClient_ProcessName}.exe' not found.")
            End If

            Dim windowHandles As List(Of IntPtr) = UtilNetwork.GetProtonVpnWindowHandles(pr.Id)

            ' Strategy: Find the handle with the largest RESTORED dimensions (using GetWindowPlacement)
            Dim largestWindowHandle As IntPtr = IntPtr.Zero
            Dim maxArea As Integer = -1

            For Each hWnd As IntPtr In windowHandles
                ' Using WindowPlacement to look at its normal bounds, ignoring current minimized/hidden status
                Dim placement As New WindowPlacement(initializeSize:=True)

                If NativeMethods.GetWindowPlacement(hWnd, placement) Then
                    Dim width As Integer = placement.NormalPosition.Right - placement.NormalPosition.Left
                    Dim height As Integer = placement.NormalPosition.Bottom - placement.NormalPosition.Top
                    Dim currentArea As Integer = width * height

                    ' If this window's restored size is larger than the previously evaluated ones, keep it
                    If currentArea > maxArea Then
                        maxArea = currentArea
                        largestWindowHandle = hWnd
                    End If
                End If
            Next hWnd

            ' If no window handle has valid dimensions, fail gracefully
            If largestWindowHandle = IntPtr.Zero OrElse maxArea <= 0 Then
                Throw New InvalidOperationException($"Could not dynamically isolate the main GUI window by restored size for process ID {pr.Id}.")
            End If

            ' Target the isolated winner handle directly.
            Dim isMinimized As Boolean = NativeMethods.IsIconic(largestWindowHandle)
            Dim isVisible As Boolean = NativeMethods.IsWindowVisible(largestWindowHandle)

            ' If it is in the systray, isVisible will be False and isMinimized will be False.
            If Not isVisible Then
                ' Bring out of complete stealth (SW_HIDE)
                NativeMethods.ShowWindow(largestWindowHandle, NativeWindowState.Show)
                ' Ensure it is restored to original size and position
                NativeMethods.ShowWindow(largestWindowHandle, NativeWindowState.Restore)
                NativeMethods.SetForegroundWindow(largestWindowHandle)
            ElseIf isMinimized Then
                ' It is just minimized to the taskbar
                NativeMethods.ShowWindow(largestWindowHandle, NativeWindowState.Restore)
                NativeMethods.SetForegroundWindow(largestWindowHandle)
            Else
                ' It is already visible and normal on screen
                NativeMethods.SetForegroundWindow(largestWindowHandle)
            End If

            Dim mainWindow As AutomationElement = AutomationElement.FromHandle(largestWindowHandle)

            ' Check if "MainPage" exists.
            Dim ProtonVpnClient_MainPageId As String = "MainPage"
            Dim pageCondition As New PropertyCondition(AutomationElement.AutomationIdProperty, ProtonVpnClient_MainPageId)
            Dim mainPage As AutomationElement = mainWindow.FindFirst(TreeScope.Descendants, pageCondition)
            If mainPage IsNot Nothing Then
                Throw New InvalidOperationException("The ProtonVPN client is not currently on the main page. Please ensure the 'Random Country' mode is enabled and the client is on the main page before attempting to change servers.")
            End If

            Try
                If isFreeProtonVpnClient Then
                    UtilNetwork.ChangeServerInProtonVpnGuiClientFreeVersion(mainWindow)
                Else
                    UtilNetwork.ChangeServerInProtonVpnGuiClientPaidVersion(mainWindow)
                End If

            Catch ex As Exception
                Throw New Exception($"ProtonVPN client process automation (GUI window) failed: {ex.Message}", ex)

            End Try

            ' Previous implementation that relied on AutomationId to distinguish between Free and Paid versions:
            ' --------------------------------------------------------------------------------------------------
            '
            'Dim ProtonVpnClient_ProcessName As String = "ProtonVPN.Client"
            'Dim ProtonVpnClient_MainPageId As String = "MainPage"
            '
            'Dim pr As Process = Process.GetProcessesByName(ProtonVpnClient_ProcessName).SingleOrDefault()
            'If pr Is Nothing Then
            '    Throw New InvalidOperationException($"Process '{ProtonVpnClient_ProcessName}.exe' not found.")
            'End If
            '
            'If pr.MainWindowHandle = IntPtr.Zero Then
            '    Throw New InvalidOperationException($"Main window handle for '{ProtonVpnClient_ProcessName}' is not available.")
            'End If
            '
            'Dim mainWindow As AutomationElement = AutomationElement.FromHandle(pr.MainWindowHandle)
            '
            '' Check if "MainPage" exists to distinguish between Free (UIA2) and Paid (WinUI3) versions.
            'Dim pageCondition As New PropertyCondition(AutomationElement.AutomationIdProperty, ProtonVpnClient_MainPageId)
            'Dim mainPage As AutomationElement = mainWindow.FindFirst(TreeScope.Descendants, pageCondition)
            'Dim isFreeClientVersion As Boolean = mainPage IsNot Nothing
            '
            'Try
            '    If isFreeClientVersion Then
            '        UtilNetwork.ChangeServerInProtonVpnGuiClientFreeVersion(mainWindow)
            '    Else
            '        UtilNetwork.ChangeServerInProtonVpnGuiClientPaidVersion(pr)
            '    End If
            '
            'Catch ex As Exception
            '    Throw New Exception($"ProtonVPN client process (GUI window) automation failed: {ex.Message}", ex)
            '
            'End Try
        End Sub

        ''' <summary>
        ''' Attempts to connect or randomly change the current VPN server in free version of ProtonVPN client for Windows 
        ''' by automating the graphical user interface (GUI) window.
        ''' <para></para>
        ''' This method blocks the current thread and retries the search until the "Change Server" button is available, 
        ''' effectively handling cooldown periods for the free version of the ProtonVPN client.
        ''' </summary>
        ''' 
        ''' <remarks>
        ''' <para>Requirements:</para>
        ''' <list type="bullet">
        ''' <item>
        '''     <description>
        '''         The Proton VPN client must be running, and the window must not be hidden. 
        '''         <para></para>
        '''         Being minimized to the Windows taskbar is acceptable and does not cause issues.
        '''     </description>
        ''' </item>
        ''' </list>
        ''' </remarks>
        ''' 
        ''' <param name="mainWindow">
        ''' The <see cref="AutomationElement"/> representing the main window of the ProtonVPN client for Windows.
        ''' </param>
        <EditorBrowsable(EditorBrowsableState.Advanced)>
        <DebuggerStepThrough>
        Private Shared Sub ChangeServerInProtonVpnGuiClientFreeVersion(mainWindow As AutomationElement)

            UtilNetwork.ChangeServerInProtonVpnGuiClientPaidVersion(mainWindow)

            ' OBSOLETE:
            '   The following code is commented out because it was replaced by a more
            '   robust implementation that uses mouse position and pixel color detection to determine the button state.
            '
            '            Dim ProtonVpnClient_ConnectButtonId As String = "ConnectionCardConnectButton"
            '            Dim ProtonVpnClient_ChangeServerButtonId As String = "ConnectionCardChangeServerButton"
            '
            '            Dim retryDelay As TimeSpan = TimeSpan.FromSeconds(1)
            '            Dim serverConnectDelay As TimeSpan = TimeSpan.FromSeconds(5)
            '
            '            Dim actionButton As AutomationElement
            '            Dim condConnect As New PropertyCondition(AutomationElement.AutomationIdProperty, ProtonVpnClient_ConnectButtonId)
            '            Dim condChange As New PropertyCondition(AutomationElement.AutomationIdProperty, ProtonVpnClient_ChangeServerButtonId)
            '
            '            Do
            '                ' Look for 'Change Server' button first, then 'Connect' button.
            '                actionButton = If(mainWindow.FindFirst(TreeScope.Descendants, condChange),
            '                                  mainWindow.FindFirst(TreeScope.Descendants, condConnect))
            '
            '                ' Button can be missing due to cooldown/timer of free ProtonVPN client version. Wait and retry.
            '                If actionButton Is Nothing Then
            '                    Thread.Sleep(retryDelay)
            '                End If
            '#If debug Then
            '                Thread.CurrentThread.Join(0) ' Prevents ContextSwitchDeadlock exception in long-running operations.
            '#End If
            '            Loop While actionButton Is Nothing
            '
            '            Dim pattern As InvokePattern = DirectCast(actionButton.GetCurrentPattern(InvokePattern.Pattern), InvokePattern)
            '            pattern.Invoke()
            '
            '            ' Wait for server connection.
            '            UtilNetwork.WaitForProtonNetworkInterfaceDataFlow(TimeSpan.FromSeconds(30))
        End Sub

        ''' <summary>
        ''' Attempts to connect or randomly change the current VPN server in paid version of ProtonVPN client for Windows 
        ''' by automating the graphical user interface (GUI) window.
        ''' </summary>
        ''' 
        ''' <remarks>
        ''' <para>Requirements for the Paid version of the ProtonVPN client:</para>
        ''' <list type="bullet">
        ''' <item>
        '''     <description>
        '''         The Proton VPN client must be running, and the window must not be hidden. 
        '''         <para></para>
        '''         Being minimized to the Windows taskbar is acceptable and does not cause issues.
        '''     </description>
        ''' </item>
        ''' <item>
        '''     <description>
        '''         The "Random Country" mode must be enabled within the client settings.
        '''     </description>
        ''' </item>
        ''' </list>
        ''' </remarks>
        ''' 
        ''' <param name="mainWindow">
        ''' The <see cref="AutomationElement"/> representing the main window of the ProtonVPN client for Windows.
        ''' </param>
        ''' 
        ''' <exception cref="InvalidOperationException">
        ''' Thrown when the required window with target class name cannot be found.
        ''' </exception>
        ''' 
        ''' <exception cref="Exception">
        ''' Thrown if failed to set ProtonVPN client window as the foreground window.
        ''' </exception>
        <EditorBrowsable(EditorBrowsableState.Advanced)>
        <DebuggerStepThrough>
        Private Shared Sub ChangeServerInProtonVpnGuiClientPaidVersion(mainWindow As AutomationElement)

            Dim windowHandle As New IntPtr(mainWindow.Current.NativeWindowHandle)
            NativeMethods.SetWindowPos(windowHandle, IntPtr.Zero, 0, 0, 850, 590, SetWindowPosFlags.IgnoreMove Or SetWindowPosFlags.IgnoreZOrder)

            Dim pid As Integer = mainWindow.Current.ProcessId
            Dim pr As Process = Process.GetProcessById(pid)

            Dim ProtonVpnClient_ChildSiteBridgeClassName As String = "Microsoft.UI.Content.DesktopChildSiteBridge"
            Dim XOffset As Integer = -50 ' Offset compensation for collapsed left sidebar.

            Dim serverDisconnectDelay As TimeSpan = TimeSpan.FromSeconds(2.5)
            Dim serverConnectDelay As TimeSpan = TimeSpan.FromSeconds(5)

            Dim bridgeHwnd As IntPtr = IntPtr.Zero
            NativeMethods.EnumChildWindows(pr.MainWindowHandle,
                         Function(hWnd, lParam)
                             Dim sb As New StringBuilder(ProtonVpnClient_ChildSiteBridgeClassName.Length + 1)
                             Dim result As Integer = NativeMethods.GetClassName(hWnd, sb, sb.Capacity)
                             If sb.ToString() = ProtonVpnClient_ChildSiteBridgeClassName Then
                                 bridgeHwnd = hWnd
                                 Return False
                             End If
                             Return True
                         End Function, IntPtr.Zero)

            If bridgeHwnd = IntPtr.Zero Then
                Throw New InvalidOperationException($"Could not find window with class name: {ProtonVpnClient_ChildSiteBridgeClassName}. The UI structure might have changed.")
            End If

            Dim oldPos As Drawing.Point = UtilMouse.Position
            NativeMethods.BlockInput(blockIt:=True)

            Try
                Dim isWindowMinimized As Boolean = NativeMethods.IsIconic(pr.MainWindowHandle)
                If isWindowMinimized Then
                    NativeMethods.ShowWindow(pr.MainWindowHandle, NativeWindowState.ShowNoActivate)
                End If

                If Not NativeMethods.SetForegroundWindow(pr.MainWindowHandle) Then
                    Throw New Exception("Failed to set ProtonVPN client window as the foreground window.")
                End If
                Thread.Sleep(500)

                Dim nativeRect As New NativeRectangle()
                If NativeMethods.GetWindowRect(bridgeHwnd, nativeRect) Then

                    Dim w As Integer = nativeRect.Right - nativeRect.Left
                    Dim h As Integer = nativeRect.Bottom - nativeRect.Top

                    ' Linear Regression based on adaptative data points
                    Dim finalX As Integer = CInt(567 + (0.4186 * (w - 898))) + XOffset
                    Dim finalY As Integer = CInt(146 + (-0.023 * (h - 650)))

                    Dim newPos As New Drawing.Point(nativeRect.Left + finalX, nativeRect.Top + finalY)

                    Dim pixelColor As Color
                    Dim hdcScreen As IntPtr = NativeMethods.GetDC(IntPtr.Zero)
                    Try
                        Dim nativeColor As NativeColor = NativeMethods.GetPixel(hdcScreen, newPos.X, newPos.Y)
                        pixelColor = Color.FromArgb(nativeColor.R, nativeColor.G, nativeColor.B)
                    Finally
                        NativeMethods.ReleaseDC(IntPtr.Zero, hdcScreen)
                    End Try

                    ' The "Disconnect" button is predominantly black color background, while its hover state is predominantly red.
                    ' The "Connect" button is predominantly blue color background, with its hover state being a slightly lighter shade of blue.
                    ' Typical observed values for the "Connect" button: R ~100-130, G ~90-130, B ~200-240 (approx).
                    Dim isConnectButton As Boolean =
                    pixelColor.B > 150 AndAlso
                    pixelColor.B > pixelColor.R AndAlso
                    pixelColor.B > pixelColor.G

                    UtilMouse.Position = newPos

                    If isConnectButton Then
                        ' Button shows "Connect": send a single click to connect.
                        UtilMouse.SendMouseInput(MouseButtonClick.LeftButton)
                    Else
                        ' Button shows "Disconnect": send two clicks (disconnect + reconnect).
                        UtilMouse.SendMouseInput(MouseButtonClick.LeftButton)
                        Thread.Sleep(serverDisconnectDelay) ' Wait for server disconnection.
                        UtilMouse.SendMouseInput(MouseButtonClick.LeftButton)
                    End If

                    ' Unblock user input before waiting for server connection to avoid
                    ' unnecessary blocking of user input during the wait period.
                    NativeMethods.BlockInput(False)

                    ' Wait for server connection.
                    UtilNetwork.WaitForProtonNetworkInterfaceDataFlow(TimeSpan.FromSeconds(30))

                    If isWindowMinimized Then
                        NativeMethods.ShowWindow(pr.MainWindowHandle, NativeWindowState.ShowMinNoActive)
                    End If

                End If

            Catch ex As Exception
                Throw

            Finally
                UtilMouse.Position = oldPos
                NativeMethods.BlockInput(blockIt:=False)

            End Try
        End Sub

        ''' <summary>
        ''' Enumerates all windows to find the real main GUI window for a specific Process ID.
        ''' Filters out hidden system/message windows by checking for Window Text and no Owner.
        ''' </summary>
        ''' 
        ''' <param name="processId">
        ''' The Process ID of the ProtonVPN client for Windows.
        ''' </param>
        <DebuggerStepThrough>
        Private Shared Function GetProtonVpnWindowHandles(processId As Integer) As List(Of IntPtr)

            Dim foundHandles As New List(Of IntPtr)

            NativeMethods.EnumWindows(
                Function(hWnd As IntPtr, lParam As IntPtr) As Boolean
                    Dim windowProcessId As Integer = 0
                    Dim result As Integer = NativeMethods.GetWindowThreadProcessId(hWnd, windowProcessId)
                    If (result = 0) OrElse (windowProcessId = 0) Then
                        ' Failed to get process ID for this window, skip it.
                        Return True
                    End If

                    If windowProcessId = processId Then
                        ' 1. The main GUI window almost always has a Title (Window Text). 
                        ' Hidden message windows and IME workers usually have a length of 0.
                        Dim titleLength As Integer = NativeMethods.GetWindowTextLength(hWnd)

                        If titleLength > 0 Then
                            ' 2. The main GUI window is a top-level window and has no owner.
                            Dim ownerHandle As IntPtr = NativeMethods.GetWindow(hWnd, GetWindowCmd.Owner)

                            If ownerHandle = IntPtr.Zero Then
                                foundHandles.Add(hWnd)
                            End If
                        End If
                    End If

                    ' Continue enumerating
                    Return True
                End Function, IntPtr.Zero)

            Return foundHandles
        End Function

        ''' <summary>
        ''' Waits for the "ProtonVPN" network interface to return to 'Up' status and have verified data flow (bytes received).
        ''' </summary>
        ''' 
        ''' <param name="timeout">
        ''' The maximum amount of time to wait for the "ProtonVPN" network interface to return to 'Up' status 
        ''' and have verified data flow (bytes received).
        ''' </param>
        <DebuggerStepThrough>
        Private Shared Sub WaitForProtonNetworkInterfaceDataFlow(timeout As TimeSpan)

            Dim ProtonVPN_NetworkInterfaceName As String = "ProtonVPN"

            ' Wait for the ProtonVPN network interface to return to 'Up' status and has verified data flow (bytes received).
            Dim startTime As Date = Date.Now
            Dim isDataFlowEstablished As Boolean = False
            Dim initialBytes As Long = -1

            Do While (Date.Now - startTime) < timeout
                Dim protonVpnInterface As NetworkInterface = NetworkInterface.GetAllNetworkInterfaces().
                    SingleOrDefault(Function(ni) ni.Name.Equals(ProtonVPN_NetworkInterfaceName, StringComparison.OrdinalIgnoreCase))

                If (protonVpnInterface IsNot Nothing) AndAlso
                   (protonVpnInterface.OperationalStatus = OperationalStatus.Up) Then

                    Dim stats As IPv4InterfaceStatistics = protonVpnInterface.GetIPv4Statistics()

                    ' On first detection of 'Up' status, capture baseline.
                    If initialBytes = -1 Then
                        initialBytes = stats.BytesReceived
                    End If

                    ' Check if we have received new data since being 'Up'.
                    If stats.BytesReceived > initialBytes Then
                        isDataFlowEstablished = True
                        Exit Do
                    End If
                End If

                Thread.Sleep(500)
            Loop

            If Not isDataFlowEstablished Then
                Throw New Exception(
                    $"No data flow (bytes received) was detected within {timeout.TotalSeconds} seconds for '{ProtonVPN_NetworkInterfaceName}' network interface.")
            End If

            Thread.Sleep(1000) ' Additional delay to try ensure connection has established with a VPN server before return.
        End Sub

#End Region

    End Class

End Namespace

#End Region
