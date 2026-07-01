
#Disable Warning BC42309 ' XML comment has a tag with a 'cref' attribute that could not be resolved

#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Runtime.InteropServices

#End Region

#Region " IconInfo "

' ReSharper disable once CheckNamespace

Namespace DevCase.Win32.Structures

    ''' <summary>
    ''' Contains information about an icon or a cursor.
    ''' </summary>
    '''
    ''' <remarks>
    ''' <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms648052%28v=vs.85%29.aspx"/>
    ''' </remarks>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure IconInfo

        ''' <summary>
        ''' Specifies whether this structure defines an icon or a cursor.
        ''' <para></para>
        ''' A value of <see langword="True"/> specifies an icon; <see langword="False"/> specifies a cursor.
        ''' </summary>
        <MarshalAs(UnmanagedType.Bool)>
        Public IsIcon As Boolean

        ''' <summary>
        ''' The x-coordinate of a cursor's hot spot.
        ''' <para></para>
        ''' If this structure defines an icon, the hot spot is always in the center of the icon, and this member is ignored.
        ''' </summary>
        Public X As Integer

        ''' <summary>
        ''' The y-coordinate of the cursor's hot spot.
        ''' <para></para>
        ''' If this structure defines an icon, the hot spot is always in the center of the icon, and this member is ignored
        ''' </summary>
        Public Y As Integer

        ''' <summary>
        ''' The icon bitmask bitmap.
        ''' <para></para>
        ''' If this structure defines a black and white icon, 
        ''' this bitmask is formatted so that the upper half is the icon <c>AND</c> bitmask and the lower half is the icon <c>XOR</c> bitmask.
        ''' <para></para>
        ''' Under this condition, the height should be an even multiple of two.
        ''' <para></para>
        ''' If this structure defines a color icon, this mask only defines the <c>AND</c> bitmask of the icon.
        ''' </summary>
        Public Mask As IntPtr

        ''' <summary>
        ''' A handle to the icon color bitmap.
        ''' <para></para>
        ''' This member can be optional if this structure defines a black and white icon.
        ''' <para></para>
        ''' The <c>AND</c> bitmask of <see cref="IconInfo.Mask"/> member is applied 
        ''' with the <see cref="TernaryRasterOperations.SrcAnd"/> flag to the destination; 
        ''' subsequently, the color bitmap is applied (using <c>XOR</c>) to the destination 
        ''' by using the <see cref="TernaryRasterOperations.SrcInvert"/> flag.
        ''' </summary>
        Public Color As IntPtr

    End Structure

End Namespace

#End Region

#Enable Warning BC42309 ' XML comment has a tag with a 'cref' attribute that could not be resolved
