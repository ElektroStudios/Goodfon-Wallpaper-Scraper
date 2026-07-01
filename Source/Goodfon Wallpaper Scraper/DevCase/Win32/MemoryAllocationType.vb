
#Disable Warning BC42309 ' XML comment has a tag with a 'cref' attribute that could not be resolved

#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Memory Allocation Type "

' ReSharper disable once CheckNamespace

Namespace DevCase.Win32.Enums

    ''' <summary>
    ''' Specifies the type of memory allocation operation.
    ''' </summary>
    '''
    ''' <remarks>
    ''' <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/aa366890%28v=vs.85%29.aspx"/>
    ''' </remarks>
    <Flags>
    Public Enum MemoryAllocationType As UInteger

        ' ***************************************************************************************
        '  ⚠️                          IMPORTANT NOTICE
        '
        '  THIS ENUMERATION IS PARTIALLY DEFINED FOR THE SPECIFIC PURPOSES OF THE GOODFON SCRAPER
        ' ***************************************************************************************

        ''' <summary>
        ''' This meaning of this value depends on the function on which it is used.
        ''' </summary>
        [Default] = &H0

        ''' <summary>
        ''' Allocates memory charges (from the overall size of memory and the paging files on disk) 
        ''' for the specified reserved memory pages. 
        ''' <para></para>
        ''' The function also guarantees that when the caller later initially accesses the memory, 
        ''' the contents will be zero. 
        ''' <para></para>
        ''' Actual physical pages are not allocated unless/until the virtual addresses are actually accessed.
        ''' <para></para>
        ''' To reserve and commit pages in one step, call <see cref="NativeMethods.VirtualAllocEx"/> with 
        ''' <see cref="MemoryAllocationType.Commit"/> and <see cref="MemoryAllocationType.Reserve"/>.
        ''' <para></para>
        ''' Attempting to commit a specific address range by specifying 
        ''' <see cref="MemoryAllocationType.Commit"/> without <see cref="MemoryAllocationType.Reserve"/> 
        ''' and a non-<see cref="UIntPtr.Zero"/> <c>address</c> fails unless the entire range has already been reserved. 
        ''' The resulting error code is <c>ERROR_INVALID_ADDRESS</c>.
        ''' <para></para>
        ''' An attempt to commit a page that is already committed does not cause the function to fail. 
        ''' This means that you can commit pages without first determining the current commitment state 
        ''' of each page.
        ''' <para></para>
        ''' If <c>address</c> specifies an address within an enclave, 
        ''' <c>allocationType</c> must be <see cref="MemoryAllocationType.Commit"/>.
        ''' </summary>
        Commit = &H1000

    End Enum

End Namespace

#End Region

#Enable Warning BC42309 ' XML comment has a tag with a 'cref' attribute that could not be resolved
