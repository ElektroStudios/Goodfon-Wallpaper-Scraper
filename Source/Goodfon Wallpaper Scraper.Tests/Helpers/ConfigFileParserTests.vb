#Region " Option Statements "

Option Explicit On
Option Strict On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Collections.Generic
Imports System.Linq
Imports System.Text.Json

#End Region

#Region " ConfigFileParserTests "

''' <summary>
''' Unit tests for the <see cref="ConfigFileParser"/> module.
''' </summary>
<TestClass>
Public NotInheritable Class ConfigFileParserTests

#Region " ParseListLines "

    ''' <summary>
    ''' Verifies that lines are trimmed and that empty, white-space and comment lines are skipped.
    ''' </summary>
    <TestMethod>
    Public Sub ParseListLines_MixedLines_TrimsAndSkipsEmptyAndCommentLines()

        Dim lines As String() = {
            "# Header comment",
            "",
            "   ",
            "  cats  ",
            vbTab & "koshka" & vbTab,
            "   # indented comment",
            "red car"
        }

        Dim result As HashSet(Of String) = ConfigFileParser.ParseListLines(lines)

        CollectionAssert.AreEqual(New String() {"cats", "koshka", "red car"}, result.ToArray())
    End Sub

    ''' <summary>
    ''' Verifies that a "#" character that is not at the start of the trimmed line is preserved.
    ''' </summary>
    <TestMethod>
    Public Sub ParseListLines_HashNotAtStart_IsPreserved()

        Dim result As HashSet(Of String) = ConfigFileParser.ParseListLines(New String() {"c#", "a # b"})

        CollectionAssert.AreEqual(New String() {"c#", "a # b"}, result.ToArray())
    End Sub

    ''' <summary>
    ''' Verifies that duplicated entries are removed case-insensitively, keeping the first occurrence.
    ''' </summary>
    <TestMethod>
    Public Sub ParseListLines_DuplicatedEntries_AreRemovedCaseInsensitively()

        Dim result As HashSet(Of String) = ConfigFileParser.ParseListLines(New String() {"Cats", "cats", " CATS ", "dogs"})

        CollectionAssert.AreEqual(New String() {"Cats", "dogs"}, result.ToArray())
    End Sub

    ''' <summary>
    ''' Verifies that the resulting set performs case-insensitive lookups (required for history hashes and keywords).
    ''' </summary>
    <TestMethod>
    Public Sub ParseListLines_Result_UsesCaseInsensitiveComparer()

        Dim result As HashSet(Of String) = ConfigFileParser.ParseListLines(New String() {"5BEF669C644056C44AAF3406FA537C09"})

        Assert.IsTrue(result.Contains("5bef669c644056c44aaf3406fa537c09"))
        Assert.AreSame(StringComparer.OrdinalIgnoreCase, result.Comparer)
    End Sub

    ''' <summary>
    ''' Verifies that null lines are ignored.
    ''' </summary>
    <TestMethod>
    Public Sub ParseListLines_NullLines_AreIgnored()

        Dim result As HashSet(Of String) = ConfigFileParser.ParseListLines(New String() {Nothing, "cats", Nothing})

        CollectionAssert.AreEqual(New String() {"cats"}, result.ToArray())
    End Sub

    ''' <summary>
    ''' Verifies that an empty input, or an input with only comments, returns an empty set.
    ''' </summary>
    <TestMethod>
    Public Sub ParseListLines_NoEntries_ReturnsEmptySet()

        Assert.AreEqual(0, ConfigFileParser.ParseListLines(Array.Empty(Of String)()).Count)
        Assert.AreEqual(0, ConfigFileParser.ParseListLines(New String() {"# a", "#b", ""}).Count)
    End Sub

    ''' <summary>
    ''' Verifies that a null input throws <see cref="ArgumentNullException"/>.
    ''' </summary>
    <TestMethod>
    Public Sub ParseListLines_Nothing_ThrowsArgumentNullException()

        Assert.ThrowsExactly(Of ArgumentNullException)(Sub() ConfigFileParser.ParseListLines(Nothing))
    End Sub

#End Region

#Region " ParseAccountsJson "

    ''' <summary>
    ''' Verifies that a valid accounts JSON (with block and line comments) is parsed.
    ''' </summary>
    <TestMethod>
    Public Sub ParseAccountsJson_ValidJsonWithComments_ReturnsAccounts()

        Const json As String =
            "/* Header comment */" & vbCrLf &
            "[" & vbCrLf &
            "  { ""Enabled"": true, ""Username"": ""alice"", ""Password"": ""secret1"" }, // line comment" & vbCrLf &
            "  { ""Enabled"": false, ""Username"": ""bob"", ""Password"": ""secret2"" }" & vbCrLf &
            "]"

        Dim accounts As List(Of GoodFonAccountConfig) = ConfigFileParser.ParseAccountsJson(json)

        Assert.AreEqual(2, accounts.Count)
        Assert.IsTrue(accounts(0).Enabled)
        Assert.AreEqual("alice", accounts(0).Username)
        Assert.AreEqual("secret1", accounts(0).Password)
        Assert.IsFalse(accounts(1).Enabled)
        Assert.AreEqual("bob", accounts(1).Username)
        Assert.AreEqual("secret2", accounts(1).Password)
    End Sub

    ''' <summary>
    ''' Verifies that property names are matched case-insensitively.
    ''' </summary>
    <TestMethod>
    Public Sub ParseAccountsJson_DifferentPropertyCasing_IsCaseInsensitive()

        Const json As String = "[{ ""enabled"": true, ""USERNAME"": ""alice"", ""pAsSwOrD"": ""pwd"" }]"

        Dim account As GoodFonAccountConfig = ConfigFileParser.ParseAccountsJson(json).Single()

        Assert.IsTrue(account.Enabled)
        Assert.AreEqual("alice", account.Username)
        Assert.AreEqual("pwd", account.Password)
    End Sub

    ''' <summary>
    ''' Verifies that missing properties keep their default values, and unknown properties are ignored.
    ''' </summary>
    <TestMethod>
    Public Sub ParseAccountsJson_MissingAndUnknownProperties_UsesDefaults()

        Const json As String = "[{ ""Username"": ""alice"", ""Unknown"": 123 }]"

        Dim account As GoodFonAccountConfig = ConfigFileParser.ParseAccountsJson(json).Single()

        Assert.IsFalse(account.Enabled)
        Assert.AreEqual("alice", account.Username)
        Assert.AreEqual(String.Empty, account.Password)
    End Sub

    ''' <summary>
    ''' Verifies that an empty JSON array returns an empty list.
    ''' </summary>
    <TestMethod>
    Public Sub ParseAccountsJson_EmptyArray_ReturnsEmptyList()

        Assert.AreEqual(0, ConfigFileParser.ParseAccountsJson("[]").Count)
    End Sub

    ''' <summary>
    ''' Verifies that empty or white-space content throws <see cref="InvalidOperationException"/>.
    ''' </summary>
    <TestMethod>
    Public Sub ParseAccountsJson_EmptyContent_ThrowsInvalidOperationException()

        Assert.ThrowsExactly(Of InvalidOperationException)(Sub() ConfigFileParser.ParseAccountsJson(Nothing))
        Assert.ThrowsExactly(Of InvalidOperationException)(Sub() ConfigFileParser.ParseAccountsJson(String.Empty))
        Assert.ThrowsExactly(Of InvalidOperationException)(Sub() ConfigFileParser.ParseAccountsJson("  " & vbCrLf & "  "))
    End Sub

    ''' <summary>
    ''' Verifies that a JSON "null" literal throws <see cref="InvalidOperationException"/>.
    ''' </summary>
    <TestMethod>
    Public Sub ParseAccountsJson_NullLiteral_ThrowsInvalidOperationException()

        Assert.ThrowsExactly(Of InvalidOperationException)(Sub() ConfigFileParser.ParseAccountsJson("null"))
    End Sub

    ''' <summary>
    ''' Verifies that malformed JSON, or JSON with an unexpected shape, throws <see cref="JsonException"/>.
    ''' </summary>
    <TestMethod>
    Public Sub ParseAccountsJson_MalformedJson_ThrowsJsonException()

        Assert.Throws(Of JsonException)(Sub() ConfigFileParser.ParseAccountsJson("[{ ""Username"": ""alice"" "))
        Assert.Throws(Of JsonException)(Sub() ConfigFileParser.ParseAccountsJson("[{ ""Enabled"": ""yes"" }]"))
        Assert.Throws(Of JsonException)(Sub() ConfigFileParser.ParseAccountsJson("{ ""Accounts"": { ""Username"": ""alice"" } }"))
    End Sub

#End Region

#Region " ParseAccountsFile "

    ''' <summary>
    ''' Verifies the current format: an object with "LoginSettings" and "Accounts" properties, with comments and trailing commas.
    ''' </summary>
    <TestMethod>
    Public Sub ParseAccountsFile_CurrentFormat_ReturnsSettingsAndAccounts()

        Const json As String =
            "/* Header */" & vbCrLf &
            "{" & vbCrLf &
            "  ""LoginSettings"": {" & vbCrLf &
            "    // Wait time." & vbCrLf &
            "    ""RateLimitWaitSeconds"": 600," & vbCrLf &
            "    ""MaxLoginAttempts"": 5," & vbCrLf &
            "    ""DelayBetweenLoginsSeconds"": 10," & vbCrLf &
            "  }," & vbCrLf &
            "  ""Accounts"": [" & vbCrLf &
            "    { ""Enabled"": true, ""Username"": ""alice"", ""Password"": ""pwd1"" }," & vbCrLf &
            "  ]" & vbCrLf &
            "}"

        Dim file As GoodFonAccountsFile = ConfigFileParser.ParseAccountsFile(json)

        Assert.AreEqual(600, file.LoginSettings.RateLimitWaitSeconds)
        Assert.AreEqual(5, file.LoginSettings.MaxLoginAttempts)
        Assert.AreEqual(10, file.LoginSettings.DelayBetweenLoginsSeconds)
        Assert.AreEqual(1, file.Accounts.Count)
        Assert.AreEqual("alice", file.Accounts(0).Username)
        Assert.IsTrue(file.Accounts(0).Enabled)
    End Sub

    ''' <summary>
    ''' Verifies backward compatibility with the legacy format (a plain JSON array of accounts), using default login settings.
    ''' </summary>
    <TestMethod>
    Public Sub ParseAccountsFile_LegacyArrayFormat_UsesDefaultLoginSettings()

        Const json As String = "[{ ""Enabled"": true, ""Username"": ""alice"", ""Password"": ""pwd"" }]"

        Dim file As GoodFonAccountsFile = ConfigFileParser.ParseAccountsFile(json)

        Assert.AreEqual(1, file.Accounts.Count)
        Assert.AreEqual(GoodFonLoginSettings.DefaultRateLimitWaitSeconds, file.LoginSettings.RateLimitWaitSeconds)
        Assert.AreEqual(GoodFonLoginSettings.DefaultMaxLoginAttempts, file.LoginSettings.MaxLoginAttempts)
        Assert.AreEqual(GoodFonLoginSettings.DefaultDelayBetweenLoginsSeconds, file.LoginSettings.DelayBetweenLoginsSeconds)
    End Sub

    ''' <summary>
    ''' Verifies that missing "LoginSettings" (or missing individual settings) fall back to the default values.
    ''' </summary>
    <TestMethod>
    Public Sub ParseAccountsFile_MissingOrPartialLoginSettings_UsesDefaults()

        Dim noSettings As GoodFonAccountsFile = ConfigFileParser.ParseAccountsFile("{ ""Accounts"": [] }")
        Dim partialSettings As GoodFonAccountsFile = ConfigFileParser.ParseAccountsFile("{ ""loginsettings"": { ""ratelimitwaitseconds"": 30 }, ""accounts"": [] }")

        Assert.AreEqual(GoodFonLoginSettings.DefaultRateLimitWaitSeconds, noSettings.LoginSettings.RateLimitWaitSeconds)
        Assert.AreEqual(0, noSettings.Accounts.Count)

        Assert.AreEqual(30, partialSettings.LoginSettings.RateLimitWaitSeconds)
        Assert.AreEqual(GoodFonLoginSettings.DefaultMaxLoginAttempts, partialSettings.LoginSettings.MaxLoginAttempts)
    End Sub

    ''' <summary>
    ''' Verifies that structurally invalid content throws <see cref="InvalidOperationException"/>.
    ''' </summary>
    <TestMethod>
    Public Sub ParseAccountsFile_InvalidStructure_ThrowsInvalidOperationException()

        ' Object without "Accounts".
        Assert.ThrowsExactly(Of InvalidOperationException)(Sub() ConfigFileParser.ParseAccountsFile("{ ""Username"": ""alice"" }"))
        ' Explicit null values.
        Assert.ThrowsExactly(Of InvalidOperationException)(Sub() ConfigFileParser.ParseAccountsFile("{ ""Accounts"": null }"))
        Assert.ThrowsExactly(Of InvalidOperationException)(Sub() ConfigFileParser.ParseAccountsFile("{ ""LoginSettings"": null, ""Accounts"": [] }"))
        Assert.ThrowsExactly(Of InvalidOperationException)(Sub() ConfigFileParser.ParseAccountsFile("[ null ]"))
        ' Unexpected root element.
        Assert.ThrowsExactly(Of InvalidOperationException)(Sub() ConfigFileParser.ParseAccountsFile("""text"""))
        Assert.ThrowsExactly(Of InvalidOperationException)(Sub() ConfigFileParser.ParseAccountsFile("123"))
    End Sub

    ''' <summary>
    ''' Verifies that invalid login settings values are rejected when parsing.
    ''' </summary>
    <TestMethod>
    Public Sub ParseAccountsFile_InvalidLoginSettings_ThrowsInvalidOperationException()

        Assert.ThrowsExactly(Of InvalidOperationException)(
            Sub() ConfigFileParser.ParseAccountsFile("{ ""LoginSettings"": { ""MaxLoginAttempts"": 0 }, ""Accounts"": [] }"))
        Assert.ThrowsExactly(Of InvalidOperationException)(
            Sub() ConfigFileParser.ParseAccountsFile("{ ""LoginSettings"": { ""RateLimitWaitSeconds"": -5 }, ""Accounts"": [] }"))
    End Sub

#End Region

#Region " MaskCredential "

    ''' <summary>
    ''' Verifies that long values keep their first four characters visible.
    ''' </summary>
    <TestMethod>
    Public Sub MaskCredential_LongValue_KeepsFirstFourCharacters()

        Assert.AreEqual("Your*********", ConfigFileParser.MaskCredential("Your username"))
        Assert.AreEqual("abcd****", ConfigFileParser.MaskCredential("abcdefgh"))
    End Sub

    ''' <summary>
    ''' Regression test: values shorter than four characters must not throw
    ''' (previously, "Substring(0, 4)" threw <see cref="ArgumentOutOfRangeException"/> in Release builds),
    ''' and must never be fully revealed.
    ''' </summary>
    <TestMethod>
    Public Sub MaskCredential_ShortValue_DoesNotThrowAndRevealsAtMostHalf()

        Assert.AreEqual("abc***", ConfigFileParser.MaskCredential("abcdef"))
        Assert.AreEqual("ab**", ConfigFileParser.MaskCredential("abcd"))
        Assert.AreEqual("a**", ConfigFileParser.MaskCredential("abc"))
        Assert.AreEqual("a*", ConfigFileParser.MaskCredential("ab"))
        Assert.AreEqual("*", ConfigFileParser.MaskCredential("a"))
    End Sub

    ''' <summary>
    ''' Verifies that null or empty values return an empty string.
    ''' </summary>
    <TestMethod>
    Public Sub MaskCredential_NullOrEmpty_ReturnsEmptyString()

        Assert.AreEqual(String.Empty, ConfigFileParser.MaskCredential(Nothing))
        Assert.AreEqual(String.Empty, ConfigFileParser.MaskCredential(String.Empty))
    End Sub

    ''' <summary>
    ''' Verifies that the masked value always has the same length as the source value.
    ''' </summary>
    <TestMethod>
    Public Sub MaskCredential_AnyValue_PreservesLength()

        For length As Integer = 1 To 40
            Dim value As New String("x"c, length)
            Assert.AreEqual(length, ConfigFileParser.MaskCredential(value).Length, $"Length {length}")
        Next length
    End Sub

    ''' <summary>
    ''' Verifies the custom visible characters count, including zero (fully masked).
    ''' </summary>
    <TestMethod>
    Public Sub MaskCredential_CustomVisibleChars_IsHonored()

        Assert.AreEqual("********", ConfigFileParser.MaskCredential("password", visibleChars:=0))
        Assert.AreEqual("pa******", ConfigFileParser.MaskCredential("password", visibleChars:=2))
    End Sub

    ''' <summary>
    ''' Verifies that a negative visible characters count throws <see cref="ArgumentOutOfRangeException"/>.
    ''' </summary>
    <TestMethod>
    Public Sub MaskCredential_NegativeVisibleChars_ThrowsArgumentOutOfRangeException()

        Assert.ThrowsExactly(Of ArgumentOutOfRangeException)(Sub() ConfigFileParser.MaskCredential("password", -1))
    End Sub

#End Region

End Class

#End Region
