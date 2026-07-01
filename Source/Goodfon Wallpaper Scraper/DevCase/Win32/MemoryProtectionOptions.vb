
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Memory-Protection Options "

' ReSharper disable once CheckNamespace

Namespace DevCase.Win32.Enums

    ''' <summary>
    ''' Specifies memory-protection options.
    ''' <para></para>
    ''' You must specify one of the following values when allocating or protecting a page in memory. 
    ''' <para></para>
    ''' Protection attributes cannot be assigned to a portion of a page; 
    ''' they can only be assigned to a whole page
    ''' </summary>
    '''
    ''' <remarks>
    ''' <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/aa366786%28v=vs.85%29.aspx"/>
    ''' </remarks>
    <Flags>
    Public Enum MemoryProtectionOptions As UInteger

        ' ***************************************************************************************
        '  ⚠️                          IMPORTANT NOTICE
        '
        '  THIS ENUMERATION IS PARTIALLY DEFINED FOR THE SPECIFIC PURPOSES OF THE GOODFON SCRAPER
        ' ***************************************************************************************

        ''' <summary>
        ''' None.
        ''' </summary>
        None = &H0

        ''' <summary>
        ''' Enables read-only or read/write access to the committed region of pages.
        ''' <para></para>
        ''' If <c>Data Execution Prevention</c> feature is enabled in the current Windows operating system, 
        ''' attempting to execute code in the committed region results in an access violation.
        ''' </summary>
        ReadWrite = &H4

    End Enum

End Namespace

#End Region
