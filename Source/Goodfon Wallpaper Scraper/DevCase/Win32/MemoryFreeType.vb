
#Disable Warning BC42309 ' XML comment has a tag with a 'cref' attribute that could not be resolved

#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Memory Free Type "

' ReSharper disable once CheckNamespace

Namespace DevCase.Win32.Enums

    ''' <summary>
    ''' Specifies the type of a memory free operation.
    ''' </summary>
    '''
    ''' <remarks>
    ''' <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/aa366892%28v=vs.85%29.aspx"/>
    ''' </remarks>
    Public Enum MemoryFreeType As UInteger

        ' ***************************************************************************************
        '  ⚠️                          IMPORTANT NOTICE
        '
        '  THIS ENUMERATION IS PARTIALLY DEFINED FOR THE SPECIFIC PURPOSES OF THE GOODFON SCRAPER
        ' ***************************************************************************************

        ''' <summary>
        ''' Releases the specified region of pages. 
        ''' <para></para>
        ''' After this operation, the pages are in the free state. 
        ''' <para></para>
        ''' When using the <see cref="NativeMethods.VirtualAlloc"/> function, 
        ''' if you specify this value, <c>size</c> parameter must be <see cref="IntPtr.Zero"/>, 
        ''' and <c>address</c> parameter must point to the base address returned by the 
        ''' <see cref="NativeMethods.VirtualAlloc"/> function when the region is reserved. 
        ''' The function fails if either of these conditions are not meet.
        ''' <para></para>
        ''' If any pages in the region are committed currently, 
        ''' the function first decommits, and then releases them.
        ''' <para></para>
        ''' The function does not fail if you attempt to release pages that are in different states, 
        ''' some reserved and some committed. 
        ''' This means that you can release a range of pages 
        ''' without first determining the current commitment state.
        ''' </summary>
        Release = &H8000

    End Enum

End Namespace

#End Region

#Enable Warning BC42309 ' XML comment has a tag with a 'cref' attribute that could not be resolved
