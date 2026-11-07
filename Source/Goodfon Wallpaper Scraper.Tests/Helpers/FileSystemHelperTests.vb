#Region " Option Statements "

Option Explicit On
Option Strict On
Option Infer Off

#End Region

#Region " FileSystemHelperTests "

''' <summary>
''' Unit tests for the <see cref="FileSystemHelper"/> module.
''' </summary>
<TestClass>
Public NotInheritable Class FileSystemHelperTests

    ''' <summary>
    ''' Verifies that fully qualified local paths are prefixed with "\\?\".
    ''' </summary>
    <TestMethod>
    Public Sub GetExtendedPath_FullyQualifiedLocalPath_AddsExtendedPrefix()

        Assert.AreEqual("\\?\C:\Output\cats\wallpaper.jpg", FileSystemHelper.GetExtendedPath("C:\Output\cats\wallpaper.jpg"))
        Assert.AreEqual("\\?\d:\folder", FileSystemHelper.GetExtendedPath("d:\folder"))
        Assert.AreEqual("\\?\C:\", FileSystemHelper.GetExtendedPath("C:\"))
    End Sub

    ''' <summary>
    ''' Verifies that UNC paths are converted to the "\\?\UNC\" form.
    ''' </summary>
    <TestMethod>
    Public Sub GetExtendedPath_UncPath_AddsExtendedUncPrefix()

        Assert.AreEqual("\\?\UNC\Server\Share\cats\wallpaper.jpg", FileSystemHelper.GetExtendedPath("\\Server\Share\cats\wallpaper.jpg"))
    End Sub

    ''' <summary>
    ''' Verifies that extended-length paths and device paths are returned unchanged.
    ''' </summary>
    <TestMethod>
    Public Sub GetExtendedPath_AlreadyExtendedOrDevicePath_ReturnsUnchanged()

        Assert.AreEqual("\\?\C:\Output", FileSystemHelper.GetExtendedPath("\\?\C:\Output"))
        Assert.AreEqual("\\?\UNC\Server\Share", FileSystemHelper.GetExtendedPath("\\?\UNC\Server\Share"))
        Assert.AreEqual("\\.\C:\Output", FileSystemHelper.GetExtendedPath("\\.\C:\Output"))
    End Sub

    ''' <summary>
    ''' Verifies that relative paths are returned unchanged.
    ''' </summary>
    <TestMethod>
    Public Sub GetExtendedPath_RelativePath_ReturnsUnchanged()

        Assert.AreEqual("output\cats", FileSystemHelper.GetExtendedPath("output\cats"))
        Assert.AreEqual(".\output", FileSystemHelper.GetExtendedPath(".\output"))
        Assert.AreEqual("..\output", FileSystemHelper.GetExtendedPath("..\output"))
    End Sub

    ''' <summary>
    ''' Regression test: root-relative ("\Folder") and drive-relative ("C:Folder") paths are not fully qualified,
    ''' so they must be returned unchanged instead of producing invalid extended-length paths
    ''' (previously "\\?\\Folder" and "\\?\C:Folder").
    ''' </summary>
    <TestMethod>
    Public Sub GetExtendedPath_RootRelativeOrDriveRelativePath_ReturnsUnchanged()

        Assert.AreEqual("\Output", FileSystemHelper.GetExtendedPath("\Output"))
        Assert.AreEqual("C:Output", FileSystemHelper.GetExtendedPath("C:Output"))
        Assert.AreEqual("C:", FileSystemHelper.GetExtendedPath("C:"))
        Assert.AreEqual("\\", FileSystemHelper.GetExtendedPath("\\"))
    End Sub

    ''' <summary>
    ''' Verifies that paths with forward slashes are not considered fully qualified
    ''' (extended-length paths are not normalized by Windows, so slashes would be invalid).
    ''' </summary>
    <TestMethod>
    Public Sub GetExtendedPath_ForwardSlashPath_ReturnsUnchanged()

        Assert.AreEqual("C:/Output", FileSystemHelper.GetExtendedPath("C:/Output"))
    End Sub

    ''' <summary>
    ''' Verifies that null, empty and white-space paths are returned unchanged.
    ''' </summary>
    <TestMethod>
    Public Sub GetExtendedPath_NullOrBlank_ReturnsUnchanged()

        Assert.IsNull(FileSystemHelper.GetExtendedPath(Nothing))
        Assert.AreEqual(String.Empty, FileSystemHelper.GetExtendedPath(String.Empty))
        Assert.AreEqual("   ", FileSystemHelper.GetExtendedPath("   "))
    End Sub

    ''' <summary>
    ''' Verifies that the conversion is idempotent.
    ''' </summary>
    <TestMethod>
    Public Sub GetExtendedPath_AppliedTwice_IsIdempotent()

        For Each path As String In New String() {"C:\Output\x.jpg", "\\Server\Share\x.jpg", "relative\x.jpg"}
            Dim once As String = FileSystemHelper.GetExtendedPath(path)
            Assert.AreEqual(once, FileSystemHelper.GetExtendedPath(once), path)
        Next path
    End Sub

End Class

#End Region
