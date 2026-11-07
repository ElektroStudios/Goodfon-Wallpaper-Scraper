#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Diagnostics

#End Region

#Region " FileSystemHelper "

''' <summary>
''' Provides file system related helpers.
''' </summary>
Friend Module FileSystemHelper

#Region " Static Methods "

    ''' <summary>
    ''' Converts a fully qualified path into an Extended-Length Path (prefixed with \\?\)
    ''' to bypass the 260 character MAX_PATH limitation.
    ''' <para></para>
    ''' Paths that cannot be converted are returned unchanged:
    ''' null, empty or white-space paths, paths that already are extended-length paths or device paths (\\.\),
    ''' relative paths, root-relative paths (e.g., "\Folder") and drive-relative paths (e.g., "C:Folder").
    ''' </summary>
    '''
    ''' <param name="targetPath">
    ''' The absolute path to convert.
    ''' </param>
    '''
    ''' <returns>
    ''' The extended-length path string.
    ''' </returns>
    <DebuggerStepThrough>
    Friend Function GetExtendedPath(targetPath As String) As String

        If String.IsNullOrWhiteSpace(targetPath) Then
            Return targetPath
        End If

        ' Already an extended path, or a device path.
        If targetPath.StartsWith("\\?\", StringComparison.Ordinal) OrElse
           targetPath.StartsWith("\\.\", StringComparison.Ordinal) Then
            Return targetPath
        End If

        ' Handle UNC paths: \\Server\Share -> \\?\UNC\Server\Share
        If targetPath.Length > 2 AndAlso targetPath.StartsWith("\\", StringComparison.Ordinal) Then
            Return $"\\?\UNC\{targetPath.Substring(2)}"
        End If

        ' Handle fully qualified local paths: C:\Folder -> \\?\C:\Folder
        If FileSystemHelper.IsFullyQualifiedDrivePath(targetPath) Then
            Return $"\\?\{targetPath}"
        End If

        ' Relative, root-relative and drive-relative paths cannot be converted to Extended-Length paths.
        Return targetPath
    End Function

#End Region

#Region " Private Methods "

    ''' <summary>
    ''' Determines whether the specified path is a fully qualified local drive path (e.g., "C:\Folder").
    ''' </summary>
    '''
    ''' <param name="targetPath">
    ''' The path to evaluate.
    ''' </param>
    '''
    ''' <returns>
    ''' <see langword="True"/> if the path is a fully qualified local drive path; otherwise, <see langword="False"/>.
    ''' </returns>
    <DebuggerStepThrough>
    Private Function IsFullyQualifiedDrivePath(targetPath As String) As Boolean

        Return targetPath.Length >= 3 AndAlso
               Char.IsLetter(targetPath(0)) AndAlso
               targetPath(1) = ":"c AndAlso
               targetPath(2) = "\"c
    End Function

#End Region

End Module

#End Region
