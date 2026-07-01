
#Disable Warning BC42309 ' XML comment has a tag with a 'cref' attribute that could not be resolved

#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Standard Access Rights "

' ReSharper disable once CheckNamespace

Namespace DevCase.Win32.Enums

    ''' <summary>
    ''' Specifies the access rights that correspond to operations specific to a standard object type.
    ''' </summary>
    '''
    ''' <remarks>
    ''' <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/aa379607(v=vs.85).aspx"/>
    ''' </remarks>
    <Flags>
    Public Enum StandardAccessRights As UInteger

        ' ***************************************************************************************
        '  ⚠️                          IMPORTANT NOTICE
        '
        '  THIS ENUMERATION IS PARTIALLY DEFINED FOR THE SPECIFIC PURPOSES OF THE GOODFON SCRAPER
        ' ***************************************************************************************

        ''' <summary>
        ''' Combines <see cref="StandardAccessRights.Delete"/>, 
        ''' <see cref="StandardAccessRights.ReadControl"/>, <see cref="StandardAccessRights.WriteDac"/>, and
        ''' <see cref="StandardAccessRights.WriteOwner"/> access.
        ''' </summary>
        StandardRightsRequired = &HF0000

    End Enum

End Namespace

#End Region

#Enable Warning BC42309 ' XML comment has a tag with a 'cref' attribute that could not be resolved
