
#Disable Warning BC42309 ' XML comment has a tag with a 'cref' attribute that could not be resolved

#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Section Attributes "

' ReSharper disable once CheckNamespace

Namespace DevCase.Win32.Enums

    ''' <summary>
    ''' Specifies the attributes for a section object.
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
    ''' <see href="https://docs.microsoft.com/en-us/windows/desktop/api/winbase/nf-winbase-createfilemappinga"/>
    ''' <para></para>
    ''' <see href="https://docs.microsoft.com/en-us/windows-hardware/drivers/kernel/section-objects-and-views"/>
    ''' </remarks>
    <Flags>
    Public Enum SectionAttributes As UInteger

        ' ***************************************************************************************
        '  ⚠️                          IMPORTANT NOTICE
        '
        '  THIS ENUMERATION IS PARTIALLY DEFINED FOR THE SPECIFIC PURPOSES OF THE GOODFON SCRAPER
        ' ***************************************************************************************

        ''' <summary>
        ''' If the file mapping object is backed by the operating system paging file, 
        ''' specifies that when a view of the file is mapped into a process address space, 
        ''' the entire range of pages is committed rather than reserved. 
        ''' <para></para>
        ''' The system must have enough committable pages to hold the entire mapping. Otherwise, CreateFileMapping fails. 
        ''' <para></para>
        ''' This attribute has no effect for file mapping objects that are backed by executable image files or data files.
        ''' <para></para>
        ''' <see cref="SectionAttributes.Commit"/> cannot be combined with <see cref="SectionAttributes.Reserve"/>.
        ''' <para></para>
        ''' If no attribute is specified, <see cref="SectionAttributes.Commit"/> is assumed.
        ''' </summary>
        Commit = &H8000000

    End Enum

End Namespace

#End Region

#Enable Warning BC42309 ' XML comment has a tag with a 'cref' attribute that could not be resolved
