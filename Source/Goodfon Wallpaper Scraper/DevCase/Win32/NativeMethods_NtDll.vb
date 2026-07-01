
#Disable Warning BC42309 ' XML comment has a tag with a 'cref' attribute that could not be resolved

#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports Microsoft.Win32.SafeHandles
Imports System.Runtime.InteropServices

Imports DevCase.Win32.Enums
Imports System.Security
Imports System.Diagnostics.CodeAnalysis

#End Region

#Region " P/Invoking "

' ReSharper disable once CheckNamespace

Namespace DevCase.Win32.NativeMethods

    ''' <summary>
    ''' Platform Invocation methods (P/Invoke), access unmanaged code.
    ''' <para></para>
    ''' NtDll.dll.
    ''' </summary>
    <HideModuleName>
    <SuppressUnmanagedCodeSecurity>
    <SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification:="Required to migrate this code to .NET Core")>
    <SuppressMessage("Interoperability", "CA1401:P/Invokes should not be visible", Justification:="")>
    Public Module NtDll

        ' ReSharper disable VBWarnings::BC42309

#Region " NtDll.dll "

        ''' <summary>
        ''' Creates a section object.
        ''' <para></para>
        ''' A section object represents a section of memory that can be shared. 
        ''' A process can use a section object to share parts of its memory address space (memory sections) with other processes. 
        ''' <para></para>
        ''' Section objects also provide the mechanism by which a process can map a file into its memory address space.
        ''' <para></para>
        ''' Each memory section has one or more corresponding views. 
        ''' A view of a section is a part of the section that is actually visible to a process. 
        ''' The act of creating a view for a section is known as mapping a view of the section. 
        ''' <para></para>
        ''' Each process that is manipulating the contents of a section has its own view; 
        ''' a process can also have multiple views (to the same or different sections).
        ''' </summary>
        '''
        ''' <remarks>
        ''' <see href="https://docs.microsoft.com/en-us/windows-hardware/drivers/ddi/content/wdm/nf-wdm-Zwcreatesection"/>
        ''' <para></para>
        ''' <see href="https://docs.microsoft.com/en-us/windows-hardware/drivers/kernel/section-objects-and-views"/>
        ''' </remarks>
        '''
        ''' <param name="refSection">
        ''' Pointer to a handle that receives a handle to the section object.
        ''' </param>
        ''' 
        ''' <param name="desiredAccess">
        ''' Specifies the requested access to the section object.
        ''' </param>
        ''' 
        ''' <param name="objectAttributes">
        ''' Pointer to an <see cref="ObjectAttributes"/> structure that specifies the object name and other attributes.
        ''' <para></para>
        ''' Use InitializeObjectAttributes to initialize this structure. 
        ''' <para></para>
        ''' If the caller is not running in a system thread context, 
        ''' it must set the <see cref="ObjectHandleAttributes.KernelHandle"/> attribute when it calls InitializeObjectAttributes.
        ''' </param>
        ''' 
        ''' <param name="refMaxSize">
        ''' Specifies the maximum size, in bytes, of the section. 
        ''' <para></para>
        ''' <see cref="NativeMethods.NtCreateSection"/> rounds this value up to the nearest multiple of PAGE_SIZE. 
        ''' <para></para>
        ''' If the section is backed by the paging file, 
        ''' <paramref name="refMaxSize"/> specifies the actual size of the section. 
        ''' <para></para>
        ''' If the section is backed by an ordinary file, 
        ''' <paramref name="refMaxSize"/> specifies the maximum size that the file can be extended or mapped to.
        ''' </param>
        ''' 
        ''' <param name="pageProtection">
        ''' Specifies the protection to place on each page in the section. 
        ''' <para></para>
        ''' Use one of the following four values: 
        ''' <see cref="MemoryProtectionOptions.ReadOnly"/>, 
        ''' <see cref="MemoryProtectionOptions.ReadWrite"/>, 
        ''' <see cref="MemoryProtectionOptions.Execute"/>, 
        ''' or <see cref="MemoryProtectionOptions.WriteCopy"/>.
        ''' </param>
        ''' 
        ''' <param name="allocationAttributes">
        ''' Flags that determines the allocation attributes of the section. 
        ''' </param>
        ''' 
        ''' <param name="hFile">
        ''' Optionally specifies a handle for an open file object. 
        ''' <para></para>
        ''' If the value of <paramref name="hFile"/> is <see cref="IntPtr.Zero"/>, the section is backed by the paging file. 
        ''' Otherwise, the section is backed by the specified file.
        ''' </param>
        '''
        ''' <returns>
        ''' Returns <see cref="NTStatus.SUCCESS"/> on success, or the appropriate <see cref="NTStatus"/> error code on failure.
        ''' </returns>
        <DllImport(Win32LibNames.NtDll, CallingConvention:=CallingConvention.StdCall)>
        Public Function NtCreateSection(ByRef refSection As IntPtr,
                                               desiredAccess As SectionAccessRights,
                                               objectAttributes As IntPtr,
                                               ByRef refMaxSize As ULong,
                                               pageProtection As MemoryProtectionOptions,
                                               allocationAttributes As SectionAttributes,
                                               hFile As IntPtr
        ) As Integer
        End Function

        ''' <summary>
        ''' Unmaps a view of a section from the virtual address space of a subject process.
        ''' </summary>
        '''
        ''' <remarks>
        ''' <see href="https://docs.microsoft.com/en-us/windows-hardware/drivers/ddi/content/wdm/nf-wdm-Zwunmapviewofsection"/>
        ''' </remarks>
        '''
        ''' <param name="hProcess">
        ''' Handle to a process object that was previously passed to <see cref="NativeMethods.NtMapViewOfSection"/>.
        ''' </param>
        ''' 
        ''' <param name="address">
        ''' Pointer to the base virtual address of the view to unmap. 
        ''' <para></para>
        ''' This value can be any virtual address within the view.
        ''' </param>
        '''
        ''' <returns>
        ''' Returns <see cref="NTStatus.SUCCESS"/> on success, or the appropriate <see cref="NTStatus"/> error code on failure.
        ''' </returns>
        <DllImport(Win32LibNames.NtDll, CallingConvention:=CallingConvention.StdCall)>
        Public Function NtUnmapViewOfSection(<[In]> hProcess As IntPtr,
                                                    <[In]> address As IntPtr
        ) As Integer
        End Function

        ''' <summary>
        ''' Unmaps a view of a section from the virtual address space of a subject process.
        ''' </summary>
        '''
        ''' <remarks>
        ''' <see href="https://docs.microsoft.com/en-us/windows-hardware/drivers/ddi/content/wdm/nf-wdm-Zwunmapviewofsection"/>
        ''' </remarks>
        '''
        ''' <param name="hProcess">
        ''' Handle to a process object that was previously passed to <see cref="NativeMethods.NtMapViewOfSection"/>.
        ''' </param>
        ''' 
        ''' <param name="address">
        ''' Pointer to the base virtual address of the view to unmap. 
        ''' <para></para>
        ''' This value can be any virtual address within the view.
        ''' </param>
        '''
        ''' <returns>
        ''' Returns <see cref="NTStatus.SUCCESS"/> on success, or the appropriate <see cref="NTStatus"/> error code on failure.
        ''' </returns>
        <DllImport(Win32LibNames.NtDll, CallingConvention:=CallingConvention.StdCall)>
        Public Function NtUnmapViewOfSection(<[In]> hProcess As SafeProcessHandle,
                                                    <[In]> address As IntPtr
        ) As Integer
        End Function

        ''' <summary>
        ''' Maps a view of a section into the virtual address space of a subject process.
        ''' </summary>
        '''
        ''' <remarks>
        ''' <see href="https://docs.microsoft.com/en-us/windows-hardware/drivers/ddi/content/wdm/nf-wdm-Zwmapviewofsection"/>
        ''' <para></para>
        ''' <see href="https://docs.microsoft.com/en-us/windows-hardware/drivers/kernel/section-objects-and-views"/>
        ''' </remarks>
        '''
        ''' <param name="hSection">
        ''' Handle to a section object. 
        ''' <para></para>
        ''' This handle is created by a successful call to <see cref="NativeMethods.NtCreateSection"/> 
        ''' or <see cref="NativeMethods.NtOpenSection"/>.
        ''' </param>
        ''' 
        ''' <param name="hProcess">
        ''' Handle to the object that represents the process that the view should be mapped into. 
        ''' <para></para>
        ''' The handle must have been opened with <see cref="ProcessAccessRights.VirtualMemoryOperation"/> access.
        ''' </param>
        ''' 
        ''' <param name="refBaseAddress">
        ''' Pointer to a variable that receives the base address of the view. 
        ''' <para></para>
        ''' If the value of this parameter is not <see cref="IntPtr.Zero"/>, 
        ''' the view is allocated starting at the specified virtual address rounded down to the next 64-kilobyte address boundary.
        ''' </param>
        ''' 
        ''' <param name="zeroBits">
        ''' Specifies the number of high-order address bits that must be zero in the base address of the section view. 
        ''' <para></para>
        ''' The value of this parameter must be less than 21 and is used only if 
        ''' <paramref name="refBaseAddress"/> is NULL—in other words, 
        ''' when the caller allows the system to determine where to allocate the view.
        ''' </param>
        ''' 
        ''' <param name="commitSize">
        ''' Specifies the size, in bytes, of the initially committed region of the view. 
        ''' <para></para>
        ''' <paramref name="commitSize"/> is meaningful only for page-file backed sections 
        ''' and is rounded up to the nearest multiple of PAGE_SIZE. 
        ''' <para></para>
        ''' (for sections that map files, both the data and the image are committed at section-creation time).
        ''' </param>
        ''' 
        ''' <param name="refSectionOffset">
        ''' A variable that receives the offset, in bytes, from the beginning of the section to the view. 
        ''' <para></para>
        ''' If this pointer is not <see cref="IntPtr.Zero"/>, 
        ''' the offset is rounded down to the next allocation-granularity size boundary.
        ''' </param>
        ''' 
        ''' <param name="refViewSize">
        ''' A SIZE_T variable. 
        ''' <para></para>
        ''' If the initial value of this variable is zero, 
        ''' <see cref="NativeMethods.NtMapViewOfSection"/> maps a view of the section that starts at 
        ''' <paramref name="refSectionOffset"/> and continues to the end of the section. 
        ''' Otherwise, the initial value specifies the view's size, in bytes. 
        ''' <para></para>
        ''' <see cref="NativeMethods.NtMapViewOfSection"/> always rounds this value up to the 
        ''' nearest multiple of PAGE_SIZE before mapping the view.
        ''' <para></para>
        ''' On return, the value receives the actual size, in bytes, of the view.
        ''' </param>
        ''' 
        ''' <param name="sectionInherit">
        ''' Specifies how the view is to be shared with child processes.
        ''' </param>
        ''' 
        ''' <param name="allocationType">
        ''' Specifies a set of flags that describes the type of allocation to be performed for the specified region of pages. 
        ''' <para></para>
        ''' The valid flags are <see cref="MemoryAllocationType.LargePages"/>, 
        ''' <see cref="MemoryAllocationType.Reserve"/>, 
        ''' and <see cref="MemoryAllocationType.TopDown"/>. 
        ''' <para></para>
        ''' Although <see cref="MemoryAllocationType.Commit"/> is not allowed, 
        ''' it is implied unless <see cref="MemoryAllocationType.Reserve"/> is specified. 
        ''' </param>
        ''' 
        ''' <param name="protection">
        ''' Specifies the type of protection for the region of initially committed pages.
        ''' <para></para>
        ''' Device and intermediate drivers should set this value to <see cref="MemoryProtectionOptions.ReadWrite"/>.
        ''' </param>
        '''
        ''' <returns>
        ''' Returns <see cref="NTStatus.SUCCESS"/> on success, or the appropriate <see cref="NTStatus"/> error code on failure.
        ''' </returns>
        <DllImport(Win32LibNames.NtDll, CallingConvention:=CallingConvention.StdCall)>
        Public Function NtMapViewOfSection(<[In]> hSection As IntPtr,
                                                  <[In]> hProcess As IntPtr,
                                                         ByRef refBaseAddress As IntPtr,
                                                  <[In]> zeroBits As IntPtr,
                                                  <[In]> commitSize As IntPtr,
                                            <[Optional]> ByRef refSectionOffset As Long,
                                                         ByRef refViewSize As UIntPtr,
                                                  <[In]> sectionInherit As SectionInherit,
                                                  <[In]> allocationType As MemoryAllocationType,
                                                  <[In]> protection As MemoryProtectionOptions
        ) As Integer
        End Function

        ''' <summary>
        ''' Maps a view of a section into the virtual address space of a subject process.
        ''' </summary>
        '''
        ''' <remarks>
        ''' <see href="https://docs.microsoft.com/en-us/windows-hardware/drivers/ddi/content/wdm/nf-wdm-Zwmapviewofsection"/>
        ''' <para></para>
        ''' <see href="https://docs.microsoft.com/en-us/windows-hardware/drivers/kernel/section-objects-and-views"/>
        ''' </remarks>
        '''
        ''' <param name="hSection">
        ''' Handle to a section object. 
        ''' <para></para>
        ''' This handle is created by a successful call to <see cref="NativeMethods.NtCreateSection"/> 
        ''' or <see cref="NativeMethods.NtOpenSection"/>.
        ''' </param>
        ''' 
        ''' <param name="hProcess">
        ''' Handle to the object that represents the process that the view should be mapped into. 
        ''' <para></para>
        ''' The handle must have been opened with <see cref="ProcessAccessRights.VirtualMemoryOperation"/> access.
        ''' </param>
        ''' 
        ''' <param name="refBaseAddress">
        ''' Pointer to a variable that receives the base address of the view. 
        ''' <para></para>
        ''' If the value of this parameter is not <see cref="IntPtr.Zero"/>, 
        ''' the view is allocated starting at the specified virtual address rounded down to the next 64-kilobyte address boundary.
        ''' </param>
        ''' 
        ''' <param name="zeroBits">
        ''' Specifies the number of high-order address bits that must be zero in the base address of the section view. 
        ''' <para></para>
        ''' The value of this parameter must be less than 21 and is used only if 
        ''' <paramref name="refBaseAddress"/> is NULL—in other words, 
        ''' when the caller allows the system to determine where to allocate the view.
        ''' </param>
        ''' 
        ''' <param name="commitSize">
        ''' Specifies the size, in bytes, of the initially committed region of the view. 
        ''' <para></para>
        ''' <paramref name="commitSize"/> is meaningful only for page-file backed sections 
        ''' and is rounded up to the nearest multiple of PAGE_SIZE. 
        ''' <para></para>
        ''' (for sections that map files, both the data and the image are committed at section-creation time).
        ''' </param>
        ''' 
        ''' <param name="refSectionOffset">
        ''' A variable that receives the offset, in bytes, from the beginning of the section to the view. 
        ''' <para></para>
        ''' If this pointer is not <see cref="IntPtr.Zero"/>, 
        ''' the offset is rounded down to the next allocation-granularity size boundary.
        ''' </param>
        ''' 
        ''' <param name="refViewSize">
        ''' A SIZE_T variable. 
        ''' <para></para>
        ''' If the initial value of this variable is zero, 
        ''' <see cref="NativeMethods.NtMapViewOfSection"/> maps a view of the section that starts at 
        ''' <paramref name="refSectionOffset"/> and continues to the end of the section. 
        ''' Otherwise, the initial value specifies the view's size, in bytes. 
        ''' <para></para>
        ''' <see cref="NativeMethods.NtMapViewOfSection"/> always rounds this value up to the 
        ''' nearest multiple of PAGE_SIZE before mapping the view.
        ''' <para></para>
        ''' On return, the value receives the actual size, in bytes, of the view.
        ''' </param>
        ''' 
        ''' <param name="sectionInherit">
        ''' Specifies how the view is to be shared with child processes.
        ''' </param>
        ''' 
        ''' <param name="allocationType">
        ''' Specifies a set of flags that describes the type of allocation to be performed for the specified region of pages. 
        ''' <para></para>
        ''' The valid flags are <see cref="MemoryAllocationType.LargePages"/>, 
        ''' <see cref="MemoryAllocationType.Reserve"/>, 
        ''' and <see cref="MemoryAllocationType.TopDown"/>. 
        ''' <para></para>
        ''' Although <see cref="MemoryAllocationType.Commit"/> is not allowed, 
        ''' it is implied unless <see cref="MemoryAllocationType.Reserve"/> is specified. 
        ''' </param>
        ''' 
        ''' <param name="protection">
        ''' Specifies the type of protection for the region of initially committed pages.
        ''' <para></para>
        ''' Device and intermediate drivers should set this value to <see cref="MemoryProtectionOptions.ReadWrite"/>.
        ''' </param>
        '''
        ''' <returns>
        ''' Returns <see cref="NTStatus.SUCCESS"/> on success, or the appropriate <see cref="NTStatus"/> error code on failure.
        ''' </returns>
        <DllImport(Win32LibNames.NtDll, CallingConvention:=CallingConvention.StdCall)>
        Public Function NtMapViewOfSection(<[In]> hSection As IntPtr,
                                                  <[In]> hProcess As SafeProcessHandle,
                                                         ByRef refBaseAddress As IntPtr,
                                                  <[In]> zeroBits As IntPtr,
                                                  <[In]> commitSize As IntPtr,
                                            <[Optional]> ByRef refSectionOffset As Long,
                                                         ByRef refViewSize As UIntPtr,
                                                  <[In]> sectionInherit As SectionInherit,
                                                  <[In]> allocationType As MemoryAllocationType,
                                                  <[In]> protection As MemoryProtectionOptions
        ) As Integer
        End Function

#End Region

    End Module

End Namespace

#End Region

#Enable Warning BC42309 ' XML comment has a tag with a 'cref' attribute that could not be resolved
