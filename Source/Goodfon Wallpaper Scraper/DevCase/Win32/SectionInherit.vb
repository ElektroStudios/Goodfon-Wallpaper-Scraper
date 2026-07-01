
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " SectionInherit "

' ReSharper disable once CheckNamespace

Namespace DevCase.Win32.Enums

    ''' <summary>
    ''' Specifies how the view of a section object is to be shared with child processes 
    ''' when calling <see cref="NativeMethods.NtMapViewOfSection"/> function.
    ''' </summary>
    '''
    ''' <remarks>
    ''' <see href="http://undocumented.ntinternals.net/UserMode/Undocumented%20Functions/NT%20Objects/Section/SECTION_INHERIT.html"/>
    ''' </remarks>
    Public Enum SectionInherit As UInteger

        ' ***************************************************************************************
        '  ⚠️                          IMPORTANT NOTICE
        '
        '  THIS ENUMERATION IS PARTIALLY DEFINED FOR THE SPECIFIC PURPOSES OF THE GOODFON SCRAPER
        ' ***************************************************************************************

        ''' <summary>
        ''' The view of the section will not be mapped into child processes.
        ''' </summary>
        ViewUnmap = &H2

    End Enum

End Namespace

#End Region
