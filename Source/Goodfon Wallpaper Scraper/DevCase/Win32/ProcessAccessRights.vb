
#Disable Warning BC42309 ' XML comment has a tag with a 'cref' attribute that could not be resolved

#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Process Access-Rights "

' ReSharper disable once CheckNamespace

Namespace DevCase.Win32.Enums

    ''' <summary>
    ''' Specifies process-specific access rights.
    ''' </summary>
    '''
    ''' <remarks>
    ''' <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms684880%28v=vs.85%29.aspx"/>
    ''' </remarks>
    <Flags>
    Public Enum ProcessAccessRights As Integer

        ' ***************************************************************************************
        '  ⚠️                          IMPORTANT NOTICE
        '
        '  THIS ENUMERATION IS PARTIALLY DEFINED FOR THE SPECIFIC PURPOSES OF THE GOODFON SCRAPER
        ' ***************************************************************************************

        ''' <summary>
        ''' Required to perform an operation on the address space of a process.
        ''' </summary>
        VirtualMemoryOperation = &H8

        ''' <summary>
        ''' Required to read memory in a process using <see cref="NativeMethods.ReadProcessMemory"/> function.
        ''' </summary>
        VirtualMemoryRead = &H10

        ''' <summary>
        ''' Combines <see cref="StandardAccessRights.Delete"/>, 
        ''' <see cref="StandardAccessRights.ReadControl"/>, <see cref="StandardAccessRights.WriteDac"/>, and
        ''' <see cref="StandardAccessRights.WriteOwner"/> access.
        ''' </summary>
        StandardRightsRequired = CInt(StandardAccessRights.StandardRightsRequired)

    End Enum

End Namespace

#End Region

#Enable Warning BC42309 ' XML comment has a tag with a 'cref' attribute that could not be resolved
