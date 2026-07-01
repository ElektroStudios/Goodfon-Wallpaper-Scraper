
#Disable Warning BC42309 ' XML comment has a tag with a 'cref' attribute that could not be resolved

#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Diagnostics.CodeAnalysis
Imports System.Runtime.ConstrainedExecution
Imports System.Runtime.InteropServices
Imports System.Security

Imports DevCase.Win32.Enums

Imports Microsoft.Win32.SafeHandles

#End Region

#Region " P/Invoking "

' ReSharper disable once CheckNamespace

Namespace DevCase.Win32.NativeMethods

    ''' <summary>
    ''' Platform Invocation methods (P/Invoke), access unmanaged code.
    ''' <para></para>
    ''' Kernel32.dll.
    ''' </summary>
    <HideModuleName>
    <SuppressUnmanagedCodeSecurity>
    <SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification:="Required to migrate this code to .NET Core")>
    <SuppressMessage("Interoperability", "CA1401:P/Invokes should not be visible", Justification:="")>
    Public Module Kernel32

        ' ReSharper disable VBWarnings::BC42309

#Region " Kernel32.dll "

#Disable Warning SYSLIB0004 ' Type or member is obsolete

        ''' <summary>
        ''' Closes the specified object handle.
        ''' </summary>
        '''
        ''' <remarks>
        ''' <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms724211%28v=vs.85%29.aspx"/>
        ''' </remarks>
        '''
        ''' <param name="hObject">
        ''' The handle to the object being closed.
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
        <ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)>
        <DllImport(Win32LibNames.Kernel32, SetLastError:=True)>
        Public Function CloseHandle(hObject As IntPtr
        ) As <MarshalAs(UnmanagedType.Bool)> Boolean
        End Function
#Enable Warning SYSLIB0004 ' Type or member is obsolete

#Disable Warning SYSLIB0004 ' Type or member is obsolete

        ''' <summary>
        ''' Closes the specified object handle.
        ''' </summary>
        '''
        ''' <remarks>
        ''' <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms724211%28v=vs.85%29.aspx"/>
        ''' </remarks>
        '''
        ''' <param name="hObject">
        ''' The handle to the object being closed.
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
        <ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)>
        <DllImport(Win32LibNames.Kernel32, SetLastError:=True)>
        Public Function CloseHandle(hObject As SafeHandle
        ) As <MarshalAs(UnmanagedType.Bool)> Boolean
        End Function
#Enable Warning SYSLIB0004 ' Type or member is obsolete

        ''' <summary>
        ''' Retrieves a handle for the current process.
        ''' </summary>
        '''
        ''' <remarks>
        ''' <see href="https://docs.microsoft.com/en-us/windows/desktop/api/processthreadsapi/nf-processthreadsapi-getcurrentprocess"/>
        ''' </remarks>
        '''
        ''' <returns>
        ''' A handle to the current process.
        ''' </returns>
        <DllImport(Win32LibNames.Kernel32)>
        Public Function GetCurrentProcess(
        ) As IntPtr
        End Function

        ''' <summary>
        ''' Opens an existing local process object.
        ''' </summary>
        '''
        ''' <remarks>
        ''' <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms684320(v=vs.85).aspx"/>
        ''' </remarks>
        '''
        ''' <param name="desiredAccess">
        ''' The access to the process object.
        ''' <para></para>
        ''' This access right is checked against the security descriptor for the process.
        ''' </param>
        '''
        ''' <param name="inheritHandle">
        ''' If this value is <see langword="True"/>, processes created by this process will inherit the handle.
        ''' Otherwise, the processes do not inherit this handle.
        ''' </param>
        '''
        ''' <param name="pid">
        ''' The identifier of the local process to be opened
        ''' </param>
        '''
        ''' <returns>
        ''' If the function succeeds, the return value is an open handle to the specified process.
        ''' <para></para>
        ''' If the function fails, the return value is <see cref="IntPtr.Zero"/>.
        ''' <para></para>
        ''' To get extended error information, call <see cref="Marshal.GetLastWin32Error"/>.
        ''' </returns>
        <DllImport(Win32LibNames.Kernel32, SetLastError:=True, CharSet:=CharSet.Unicode)>
        Public Function OpenProcess(desiredAccess As ProcessAccessRights,
                                           inheritHandle As Boolean,
                                           pid As Integer
        ) As SafeProcessHandle
        End Function

        ''' <summary>
        ''' Reads data from an area of memory in a specified process.
        ''' <para></para>
        ''' The entire area to be read must be accessible or the operation fails.
        ''' </summary>
        '''
        ''' <remarks>
        ''' <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms680553%28v=vs.85%29.aspx"/>
        ''' </remarks>
        '''
        ''' <param name="hProcess">
        ''' A handle to the process with memory that is being read.
        ''' <para></para>
        ''' The handle must have PROCESS_VM_READ access to the process.
        ''' </param>
        '''
        ''' <param name="baseAddress">
        ''' The base address in the specified process from which to read.
        ''' <para></para>
        ''' Before any data transfer occurs, the system verifies that all data in the base address and memory of the
        ''' specified size is accessible for read access, and if it is not accessible the function fails.
        ''' </param>
        '''
        ''' <param name="buffer">
        ''' A buffer that receives the contents from the address space of the specified process.
        ''' </param>
        '''
        ''' <param name="size">
        ''' The number of bytes to be read from the specified process.
        ''' </param>
        '''
        ''' <param name="refNumberOfBytesRead">
        ''' A variable that receives the number of bytes transferred into the specified buffer.
        ''' <para></para>
        ''' If <paramref name="refNumberOfBytesRead"/> is <c>0</c>, the parameter is ignored.
        ''' </param>
        '''
        ''' <returns>
        ''' <see langword="True"/> If the function succeeds, <see langword="False"/> otherwise.
        ''' </returns>
        <SuppressUnmanagedCodeSecurity>
        <DllImport(Win32LibNames.Kernel32, SetLastError:=True)>
        Public Function ReadProcessMemory(<[In]> hProcess As SafeProcessHandle,
                                                 <[In]> baseAddress As UIntPtr,
                                                        buffer As IntPtr,
                                                        size As IntPtr,
                      <MarshalAs(UnmanagedType.SysInt)> ByRef refNumberOfBytesRead As IntPtr
        ) As <MarshalAs(UnmanagedType.Bool)> Boolean
        End Function

        ''' <summary>
        ''' Reads data from an area of memory in a specified process.
        ''' <para></para>
        ''' The entire area to be read must be accessible or the operation fails.
        ''' </summary>
        '''
        ''' <remarks>
        ''' <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms680553%28v=vs.85%29.aspx"/>
        ''' </remarks>
        '''
        ''' <param name="hProcess">
        ''' A handle to the process with memory that is being read.
        ''' <para></para>
        ''' The handle must have PROCESS_VM_READ access to the process.
        ''' </param>
        '''
        ''' <param name="baseAddress">
        ''' The base address in the specified process from which to read.
        ''' <para></para>
        ''' Before any data transfer occurs, the system verifies that all data in the base address and memory of the
        ''' specified size is accessible for read access, and if it is not accessible the function fails.
        ''' </param>
        '''
        ''' <param name="buffer">
        ''' A buffer that receives the contents from the address space of the specified process.
        ''' </param>
        '''
        ''' <param name="size">
        ''' The number of bytes to be read from the specified process.
        ''' </param>
        '''
        ''' <param name="refNumberOfBytesRead">
        ''' A variable that receives the number of bytes transferred into the specified buffer.
        ''' <para></para>
        ''' If <paramref name="refNumberOfBytesRead"/> is <c>0</c>, the parameter is ignored.
        ''' </param>
        '''
        ''' <returns>
        ''' <see langword="True"/> If the function succeeds, <see langword="False"/> otherwise.
        ''' </returns>
        <SuppressUnmanagedCodeSecurity>
        <DllImport(Win32LibNames.Kernel32, SetLastError:=True)>
        Public Function ReadProcessMemory(<[In]> hProcess As SafeProcessHandle,
                                                 <[In]> baseAddress As IntPtr,
                                                <Out> buffer As Byte(),
                                                        size As IntPtr,
                      <MarshalAs(UnmanagedType.SysInt)> ByRef refNumberOfBytesRead As IntPtr
        ) As <MarshalAs(UnmanagedType.Bool)> Boolean
        End Function

        ''' <summary>
        ''' Reads data from an area of memory in a specified process.
        ''' <para></para>
        ''' The entire area to be read must be accessible or the operation fails.
        ''' </summary>
        '''
        ''' <remarks>
        ''' <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms680553%28v=vs.85%29.aspx"/>
        ''' </remarks>
        '''
        ''' <param name="hProcess">
        ''' A handle to the process with memory that is being read.
        ''' <para></para>
        ''' The handle must have PROCESS_VM_READ access to the process.
        ''' </param>
        '''
        ''' <param name="baseAddress">
        ''' The base address in the specified process from which to read.
        ''' <para></para>
        ''' Before any data transfer occurs, the system verifies that all data in the base address and memory of the
        ''' specified size is accessible for read access, and if it is not accessible the function fails.
        ''' </param>
        '''
        ''' <param name="buffer">
        ''' A buffer that receives the contents from the address space of the specified process.
        ''' </param>
        '''
        ''' <param name="size">
        ''' The number of bytes to be read from the specified process.
        ''' </param>
        '''
        ''' <param name="refNumberOfBytesRead">
        ''' A variable that receives the number of bytes transferred into the specified buffer.
        ''' <para></para>
        ''' If <paramref name="refNumberOfBytesRead"/> is <c>0</c>, the parameter is ignored.
        ''' </param>
        '''
        ''' <returns>
        ''' <see langword="True"/> If the function succeeds, <see langword="False"/> otherwise.
        ''' </returns>
        <SuppressUnmanagedCodeSecurity>
        <DllImport(Win32LibNames.Kernel32, SetLastError:=True)>
        Public Function ReadProcessMemory(<[In]> hProcess As SafeProcessHandle,
                                                 <[In]> baseAddress As UIntPtr,
                                                <Out> buffer As Byte(),
                                                        size As UIntPtr,
                      <MarshalAs(UnmanagedType.SysInt)> ByRef refNumberOfBytesRead As IntPtr
        ) As <MarshalAs(UnmanagedType.Bool)> Boolean
        End Function

        ''' <summary>
        ''' Reserves, commits, or changes the state of a region of memory
        ''' within the virtual address space of a specified process.
        ''' <para></para>
        ''' The function initializes the memory it allocates to zero.
        ''' </summary>
        '''
        ''' <remarks>
        ''' <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/aa366890%28v=vs.85%29.aspx"/>
        ''' </remarks>
        '''
        ''' <param name="hProcess">
        ''' The handle to a process.
        ''' <para></para>
        ''' The function allocates memory within the virtual address space of this process.
        ''' <para></para>
        ''' The handle must have the <see cref="ProcessAccessRights.VirtualMemoryOperation"/> access right
        ''' </param>
        '''
        ''' <param name="address">
        ''' The pointer that specifies a desired starting address for the region of pages that you want to allocate.
        ''' <para></para>
        ''' If you are reserving memory,
        ''' the function rounds this address down to the nearest multiple of the allocation granularity.
        ''' <para></para>
        ''' If you are committing memory that is already reserved,
        ''' the function rounds this address down to the nearest page boundary.
        ''' <para></para>
        ''' To determine the size of a page and the allocation granularity on the host computer,
        ''' use the <c>GetSystemInfo</c> function.
        ''' <para></para>
        ''' If <paramref name="address"/> is <see cref="UIntPtr.Zero"/>,
        ''' the function determines where to allocate the region.
        ''' <para></para>
        ''' If this address is within an enclave that you have not initialized by calling <c>InitializeEnclave</c> function,
        ''' <see cref="NativeMethods.VirtualAllocEx"/> allocates a page of zeros for the enclave at that address.
        ''' <para></para>
        ''' The page must be previously uncommitted, and will not be measured with the <c>EEXTEND</c> instruction
        ''' of the <c>Intel Software Guard Extensions</c> programming model.
        ''' <para></para>
        ''' If the address in within an enclave that you initialized,
        ''' then the allocation operation fails with the <c>ERROR_INVALID_ADDRESS</c> error.
        ''' </param>
        '''
        ''' <param name="size">
        ''' The size of the region of memory to allocate, in bytes.
        ''' <para></para>
        ''' If <paramref name="address"/> is <see cref="UIntPtr.Zero"/>,
        ''' the function rounds <paramref name="size"/> up to the next page boundary.
        ''' <para></para>
        ''' If <paramref name="address"/> is not <see cref="UIntPtr.Zero"/>,
        ''' the function allocates all pages that contain one or more bytes in the range from
        ''' <paramref name="address"/> to <paramref name="address"/>+<paramref name="size"/>.
        ''' This means, for example, that a 2-byte range that straddles a page boundary
        ''' causes the function to allocate both pages.
        ''' </param>
        '''
        ''' <param name="allocationType">
        ''' The type of memory allocation.
        ''' </param>
        '''
        ''' <param name="protection">
        ''' The memory protection for the region of pages to be allocated.
        ''' <para></para>
        ''' If <paramref name="address"/> parameter specifies an address within an enclave,
        ''' <paramref name="protection"/> cannot be any of the following values:
        ''' <para></para>
        ''' <see cref="MemoryProtectionOptions.NoAccess"/>,
        ''' <see cref="MemoryProtectionOptions.Guard"/>,
        ''' <see cref="MemoryProtectionOptions.NoCache"/>,
        ''' <see cref="MemoryProtectionOptions.WriteCombine"/>
        ''' </param>
        '''
        ''' <returns>
        ''' If the function succeeds, the return value is the base address of the allocated region of pages.
        ''' <para></para>
        ''' If the function fails, the return value is <see cref="IntPtr.Zero"/>.
        ''' <para></para>
        ''' To get extended error information, call <see cref="Marshal.GetLastWin32Error"/>.
        ''' </returns>
        <DllImport(Win32LibNames.Kernel32, ExactSpelling:=True, SetLastError:=True)>
        Public Function VirtualAllocEx(<[In]> hProcess As SafeProcessHandle,
                                              <[In]> address As IntPtr,
                                                     size As IntPtr,
                                                     allocationType As MemoryAllocationType,
                                                     protection As MemoryProtectionOptions
        ) As IntPtr
        End Function

        ''' <summary>
        ''' Releases, decommits, or releases and decommits a region of memory
        ''' within the virtual address space of a specified process.
        ''' </summary>
        '''
        ''' <remarks>
        ''' <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/aa366894%28v=vs.85%29.aspx"/>
        ''' </remarks>
        '''
        ''' <param name="process">
        ''' A handle to a process. The function frees memory within the virtual address space of the process.
        ''' <para></para>
        ''' The handle must have the <see cref="ProcessAccessRights.VirtualMemoryOperation"/> access right.
        ''' </param>
        '''
        ''' <param name="address">
        ''' The starting address of the region of memory to be freed.
        ''' <para></para>
        ''' If the <paramref name="freeType"/> parameter is <see cref="MemoryFreeType.Release"/>,
        ''' this parameter must be the base address returned by the <see cref="NativeMethods.VirtualAllocEx"/> function
        ''' when the region of pages is reserved.
        ''' </param>
        '''
        ''' <param name="size">
        ''' The size of the region of memory to be freed, in bytes.
        ''' <para></para>
        ''' If the <paramref name="freeType"/> parameter is <see cref="MemoryFreeType.Release"/>,
        ''' this parameter must be <c>0</c> (zero).
        ''' <para></para>
        ''' The function frees the entire region that is reserved in the initial allocation call to
        ''' <see cref="NativeMethods.VirtualAlloc"/> function.
        ''' <para></para>
        ''' If <paramref name="freeType"/> parameter is <see cref="MemoryFreeType.Decommit"/>,
        ''' the function decommits all memory pages that contain one or more bytes
        ''' in the range from the <paramref name="address"/> parameter to (<paramref name="address"/>+<paramref name="size"/>).
        ''' This means, for example, that a 2-byte region of memory that straddles a page boundary
        ''' causes both pages to be decommitted.
        ''' <para></para>
        ''' If <paramref name="address"/> parameter is the base address returned by
        ''' <see cref="NativeMethods.VirtualAllocEx"/> and <paramref name="size"/> parameter is <c>0</c> (zero),
        ''' the function decommits the entire region that is allocated by <see cref="NativeMethods.VirtualAllocEx"/>.
        ''' After that, the entire region is in the reserved state.
        ''' </param>
        '''
        ''' <param name="freeType">
        ''' The type of free operation.
        ''' </param>
        '''
        ''' <returns>
        ''' If the function succeeds, the return value is <see langword="True"/>.
        ''' <para></para>
        ''' If the function fails, the return value is <see langword="False"/>.
        ''' <para></para>
        ''' To get extended error information, call <see cref="Marshal.GetLastWin32Error"/>.
        ''' </returns>
        <DllImport(Win32LibNames.Kernel32, SetLastError:=True)>
        Public Function VirtualFreeEx(<[In]> process As SafeProcessHandle,
                                      <[In]> address As IntPtr,
                                             size As UIntPtr,
                                             freeType As MemoryFreeType
 ) As <MarshalAs(UnmanagedType.Bool)> Boolean
        End Function

#End Region

    End Module

End Namespace

#End Region

#Enable Warning BC42309 ' XML comment has a tag with a 'cref' attribute that could not be resolved