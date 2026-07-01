
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Input Type "

' ReSharper disable once CheckNamespace

Namespace DevCase.Win32.Enums

    ''' <summary>
    ''' The type of an input event.
    ''' <para></para>
    ''' For <see cref="Structures.Input.Type"/> field.
    ''' </summary>
    '''
    ''' <remarks>
    ''' <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms646270%28v=vs.85%29.aspx"/>
    ''' </remarks>
    Public Enum InputType As Integer

        ' ***************************************************************************************
        '  ⚠️                          IMPORTANT NOTICE
        '
        '  THIS ENUMERATION IS PARTIALLY DEFINED FOR THE SPECIFIC PURPOSES OF THE GOODFON SCRAPER
        ' ***************************************************************************************

        ''' <summary>
        ''' The event is a mouse event.
        ''' <para></para>
        ''' Use the <see cref="Structures.MouseInput"/> structure of the union.
        ''' </summary>
        Mouse = 0

    End Enum

End Namespace

#End Region
