
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Section Access-Rights "

' ReSharper disable once CheckNamespace

Namespace DevCase.Win32.Enums

    ''' <summary>
    ''' Specifies access rights for section objects.
    ''' </summary>
    '''
    ''' <remarks>
    ''' <see href="https://docs.microsoft.com/en-us/windows-hardware/drivers/ddi/content/wdm/nf-wdm-Zwmapviewofsection"/>
    ''' </remarks>
    <Flags>
    Public Enum SectionAccessRights As Integer

        ' ***************************************************************************************
        '  ⚠️                          IMPORTANT NOTICE
        '
        '  THIS ENUMERATION IS PARTIALLY DEFINED FOR THE SPECIFIC PURPOSES OF THE GOODFON SCRAPER
        ' ***************************************************************************************

        ''' <summary>
        ''' Query the section object for information about the section. 
        ''' </summary>
        Query = &H1

        ''' <summary>
        ''' Write views of the section.
        ''' </summary>
        MapWrite = &H2

        ''' <summary>
        ''' Read views of the section.
        ''' </summary>
        MapRead = &H4

        ''' <summary>
        ''' Execute views of the section.
        ''' </summary>
        MapExecute = &H8

        ''' <summary>
        ''' Dynamically extend the size of the section. 
        ''' </summary>
        ExtendSize = &H10

        ''' <summary>
        ''' Combines all possible access rights for a section object.
        ''' </summary>
        AllAccess = ProcessAccessRights.StandardRightsRequired Or
                     SectionAccessRights.Query Or
                     SectionAccessRights.MapWrite Or
                     SectionAccessRights.MapRead Or
                     SectionAccessRights.MapExecute Or
                     SectionAccessRights.ExtendSize

    End Enum

End Namespace

#End Region
