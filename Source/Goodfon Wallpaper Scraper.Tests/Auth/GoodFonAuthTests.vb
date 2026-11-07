#Region " Option Statements "

Option Explicit On
Option Strict On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Linq
Imports System.Text.Json

#End Region

#Region " GoodFonAuthTests "

''' <summary>
''' Unit tests for the <see cref="GoodFonAuth"/> and <see cref="GoodFonAccountConfig"/> classes.
''' <para></para>
''' These tests never perform network requests.
''' </summary>
<TestClass>
Public NotInheritable Class GoodFonAuthTests

#Region " GoodFonAuth "

    ''' <summary>
    ''' Verifies the initial state of a new instance.
    ''' </summary>
    <TestMethod>
    Public Sub Constructor_NewInstance_HasExpectedInitialState()

        Using auth As New GoodFonAuth()
            Assert.IsNotNull(auth.Client)
            Assert.IsNotNull(auth.CookieContainer)
            Assert.AreEqual(0, auth.CookieContainer.Count)
            Assert.IsFalse(auth.DailyLimitReached)
            Assert.AreEqual(AppGlobals.HttpClientTimeOut, auth.Client.Timeout)
        End Using
    End Sub

    ''' <summary>
    ''' Verifies that the HTTP client is configured with the expected default request headers.
    ''' </summary>
    <TestMethod>
    Public Sub Constructor_NewInstance_ConfiguresDefaultRequestHeaders()

        Using auth As New GoodFonAuth()
            Dim userAgent As String = String.Join(" ", auth.Client.DefaultRequestHeaders.GetValues("User-Agent"))
            Dim accept As String = String.Join(", ", auth.Client.DefaultRequestHeaders.GetValues("Accept"))

            Dim parsedGuid As Guid
            Assert.IsTrue(Guid.TryParse(userAgent, parsedGuid), $"User-Agent is not a GUID: {userAgent}")
            StringAssert.Contains(accept, "application/json")
            StringAssert.Contains(accept, "text/html")
        End Using
    End Sub

    ''' <summary>
    ''' Verifies that all instances share the same (random) User-Agent during the application lifetime.
    ''' </summary>
    <TestMethod>
    Public Sub Constructor_SeveralInstances_ShareSameUserAgent()

        Using auth1 As New GoodFonAuth(), auth2 As New GoodFonAuth()
            Assert.AreEqual(auth1.Client.DefaultRequestHeaders.GetValues("User-Agent").Single(),
                            auth2.Client.DefaultRequestHeaders.GetValues("User-Agent").Single())
        End Using
    End Sub

    ''' <summary>
    ''' Verifies that the daily limit flag can be set and cleared.
    ''' </summary>
    <TestMethod>
    Public Sub DailyLimitReached_SetAndClear_RoundTrips()

        Using auth As New GoodFonAuth()
            auth.DailyLimitReached = True
            Assert.IsTrue(auth.DailyLimitReached)

            auth.DailyLimitReached = False
            Assert.IsFalse(auth.DailyLimitReached)
        End Using
    End Sub

    ''' <summary>
    ''' Verifies that disposing an instance several times does not throw.
    ''' </summary>
    <TestMethod>
    Public Sub Dispose_CalledTwice_DoesNotThrow()

        Dim auth As New GoodFonAuth()
        auth.Dispose()
        auth.Dispose()
    End Sub

#End Region

#Region " GoodFonAccountConfig "

    ''' <summary>
    ''' Verifies the default values of a new account configuration.
    ''' </summary>
    <TestMethod>
    Public Sub GoodFonAccountConfig_NewInstance_HasSafeDefaults()

        Dim config As New GoodFonAccountConfig()

        Assert.IsFalse(config.Enabled)
        Assert.AreEqual(String.Empty, config.Username)
        Assert.AreEqual(String.Empty, config.Password)
    End Sub

    ''' <summary>
    ''' Verifies that serialization uses the documented JSON property names ("Enabled", "Username", "Password"),
    ''' and that the configuration round-trips through the accounts parser.
    ''' </summary>
    <TestMethod>
    Public Sub GoodFonAccountConfig_Serialization_UsesDocumentedPropertyNamesAndRoundTrips()

        Dim config As New GoodFonAccountConfig() With {
            .Enabled = True,
            .Username = "alice",
            .Password = "p@ss"
        }

        Dim json As String = JsonSerializer.Serialize(New GoodFonAccountConfig() {config})

        StringAssert.Contains(json, """Enabled"":true")
        StringAssert.Contains(json, """Username"":""alice""")
        StringAssert.Contains(json, """Password"":")

        Dim roundTripped As GoodFonAccountConfig = ConfigFileParser.ParseAccountsJson(json).Single()
        Assert.IsTrue(roundTripped.Enabled)
        Assert.AreEqual("alice", roundTripped.Username)
        Assert.AreEqual("p@ss", roundTripped.Password)
    End Sub

#End Region

End Class

#End Region
