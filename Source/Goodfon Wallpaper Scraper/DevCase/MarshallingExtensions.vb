
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.ComponentModel
Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices

#End Region

#Region " Marshalling Extensions "

' ReSharper disable once CheckNamespace

Namespace DevCase.Extensions.MarshallingExtensions

    ''' <summary>
    ''' Provides extension methods for marshaling operations.
    ''' </summary>
    <HideModuleName>
    Public Module MarshallingExtensions

#Region " Public Extension Methods "

        ''' <summary>
        ''' Converts a byte array into a managed structure of type <typeparamref name="T"/>.
        ''' </summary>
        ''' 
        ''' <typeparam name="T">
        ''' The structure type to convert the byte array into.
        ''' </typeparam>
        ''' 
        ''' <param name="structBytes">
        ''' The byte array containing the raw data for the structure.
        ''' </param>
        ''' 
        ''' <returns>
        ''' A managed structure of type <typeparamref name="T"/> populated with data from the <paramref name="structBytes"/> byte array.
        ''' </returns>
        <Extension>
        <EditorBrowsable(EditorBrowsableState.Always)>
        Public Function ConvertToStructure(Of T As Structure)(structBytes As Byte()) As T

            Dim handle As GCHandle = GCHandle.Alloc(structBytes, GCHandleType.Pinned)
            Try
                Return Marshal.PtrToStructure(Of T)(handle.AddrOfPinnedObject())
            Finally
                handle.Free()
            End Try
        End Function

#End Region

    End Module

End Namespace

#End Region
