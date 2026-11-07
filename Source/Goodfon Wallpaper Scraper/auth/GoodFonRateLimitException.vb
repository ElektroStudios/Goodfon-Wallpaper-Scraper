#Region " Option Statements "

Option Explicit On
Option Strict On
Option Infer Off

#End Region

#Region " GoodFonRateLimitException "

''' <summary>
''' The exception that is thrown when the GoodFon.com website rejects a login attempt due to its login rate-limit
''' ("Too many attempts. Please wait a few minutes").
''' </summary>
Public NotInheritable Class GoodFonRateLimitException : Inherits Exception

    ''' <summary>
    ''' Gets the wait time requested by the website (HTTP "Retry-After" header),
    ''' or <see langword="Nothing"/> if the website did not specify it.
    ''' </summary>
    Public ReadOnly Property RetryAfter As TimeSpan?

    ''' <summary>
    ''' Initializes a new instance of the <see cref="GoodFonRateLimitException"/> class.
    ''' </summary>
    Public Sub New()

        Me.New("Too many login attempts. Please wait a few minutes.", Nothing)
    End Sub

    ''' <summary>
    ''' Initializes a new instance of the <see cref="GoodFonRateLimitException"/> class.
    ''' </summary>
    '''
    ''' <param name="message">
    ''' The message that describes the error.
    ''' </param>
    '''
    ''' <param name="retryAfter">
    ''' The wait time requested by the website, or <see langword="Nothing"/> if the website did not specify it.
    ''' </param>
    Public Sub New(message As String, retryAfter As TimeSpan?)

        MyBase.New(message)
        Me.RetryAfter = retryAfter
    End Sub

End Class

#End Region
