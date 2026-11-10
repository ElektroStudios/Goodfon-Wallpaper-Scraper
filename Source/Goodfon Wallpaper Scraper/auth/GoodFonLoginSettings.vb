#Region " Option Statements "

Option Explicit On
Option Strict On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Diagnostics
Imports System.Text.Json.Serialization

#End Region

#Region " GoodFonLoginSettings "

''' <summary>
''' Represents the user-configurable settings of the GoodFon.com login process,
''' mainly used to handle the website login rate-limit ("Too many attempts. Please wait a few minutes").
''' </summary>
Public NotInheritable Class GoodFonLoginSettings ' NOSONAR

#Region " Constants "

    ''' <summary>
    ''' The default value for <see cref="GoodFonLoginSettings.RateLimitWaitSeconds"/> (3 minutes).
    ''' </summary>
    Public Const DefaultRateLimitWaitSeconds As Integer = 180

    ''' <summary>
    ''' The default value for <see cref="GoodFonLoginSettings.MaxLoginAttempts"/>.
    ''' </summary>
    Public Const DefaultMaxLoginAttempts As Integer = 3

    ''' <summary>
    ''' The default value for <see cref="GoodFonLoginSettings.DelayBetweenLoginsSeconds"/>.
    ''' </summary>
    Public Const DefaultDelayBetweenLoginsSeconds As Integer = 35

    ''' <summary>
    ''' The default value for <see cref="GoodFonLoginSettings.NetworkErrorWaitSeconds"/>.
    ''' </summary>
    Public Const DefaultNetworkErrorWaitSeconds As Integer = 30

#End Region

#Region " Properties "

    ''' <summary>
    ''' Gets or sets the time, in seconds, to wait before retrying a login attempt
    ''' that was rejected by the website login rate-limit.
    ''' <para></para>
    ''' If the website specifies a longer wait time (HTTP "Retry-After" header), the longer wait time is used.
    ''' </summary>
    <JsonPropertyName("RateLimitWaitSeconds")>
    Public Property RateLimitWaitSeconds As Integer = GoodFonLoginSettings.DefaultRateLimitWaitSeconds

    ''' <summary>
    ''' Gets or sets the maximum number of login attempts per account
    ''' (the first attempt included) when the website login rate-limit is hit.
    ''' </summary>
    <JsonPropertyName("MaxLoginAttempts")>
    Public Property MaxLoginAttempts As Integer = GoodFonLoginSettings.DefaultMaxLoginAttempts

    ''' <summary>
    ''' Gets or sets the time, in seconds, to wait between the logins of consecutive accounts,
    ''' to reduce the chance of triggering the website login rate-limit.
    ''' </summary>
    <JsonPropertyName("DelayBetweenLoginsSeconds")>
    Public Property DelayBetweenLoginsSeconds As Integer = GoodFonLoginSettings.DefaultDelayBetweenLoginsSeconds

    ''' <summary>
    ''' Gets or sets the time, in seconds, to wait before retrying a login attempt 
    ''' that failed due to a transient network error 
    ''' (e.g., "The underlying connection was closed: An unexpected error occurred on a send", or a request timeout).
    ''' </summary>
    <JsonPropertyName("NetworkErrorWaitSeconds")>
    Public Property NetworkErrorWaitSeconds As Integer = GoodFonLoginSettings.DefaultNetworkErrorWaitSeconds

#End Region

#Region " Public Methods "

    ''' <summary>
    ''' Validates the current settings values.
    ''' </summary>
    '''
    ''' <exception cref="InvalidOperationException">
    ''' Thrown when any setting has an invalid value.
    ''' </exception>
    <DebuggerStepThrough>
    Public Sub Validate()

        If Me.RateLimitWaitSeconds < 0 Then
            Throw New InvalidOperationException($"Login setting 'RateLimitWaitSeconds' cannot be negative (current value: {Me.RateLimitWaitSeconds}).")
        End If

        If Me.MaxLoginAttempts < 1 Then
            Throw New InvalidOperationException($"Login setting 'MaxLoginAttempts' must be greater than or equal to 1 (current value: {Me.MaxLoginAttempts}).")
        End If

        If Me.DelayBetweenLoginsSeconds < 0 Then
            Throw New InvalidOperationException($"Login setting 'DelayBetweenLoginsSeconds' cannot be negative (current value: {Me.DelayBetweenLoginsSeconds}).")
        End If

        If Me.NetworkErrorWaitSeconds < 0 Then
            Throw New InvalidOperationException($"Login setting 'NetworkErrorWaitSeconds' cannot be negative (current value: {Me.NetworkErrorWaitSeconds}).")
        End If
    End Sub

    ''' <summary>
    ''' Gets the time to wait before retrying a login attempt rejected by the website login rate-limit.
    ''' </summary>
    '''
    ''' <param name="retryAfter">
    ''' The wait time requested by the website (HTTP "Retry-After" header),
    ''' or <see langword="Nothing"/> if the website did not specify it.
    ''' </param>
    '''
    ''' <returns>
    ''' The greater value between <see cref="GoodFonLoginSettings.RateLimitWaitSeconds"/> and <paramref name="retryAfter"/>.
    ''' </returns>
    <DebuggerStepThrough>
    Public Function GetRateLimitWait(retryAfter As TimeSpan?) As TimeSpan

        Dim configuredWait As TimeSpan = TimeSpan.FromSeconds(Me.RateLimitWaitSeconds)

        Return If(retryAfter.HasValue AndAlso (retryAfter.Value > configuredWait), retryAfter.Value, configuredWait)
    End Function

#End Region

End Class

#End Region
