Public Class frmMain
    Sub DoRequest()
        If InvokeRequired Then
            Invoke(New MethodInvoker(AddressOf DoRequest))
        Else
            If (TextBox1.Text = Nothing) Then
                DisplayMessage("Please input a Request URL.")
                Exit Sub
            End If
            If Not (TextBox3.Text = Nothing) Then
                HTTP.UserAgent = TextBox3.Text
            End If
            Dim Success As String = Nothing
            If Not (TextBox5.Text = Nothing) Then
                Success = TextBox5.Text
            End If
            If RadioButton1.Checked Then
                HTTP.Method = "get"
            Else
                HTTP.Method = "post"
            End If
            HTTP.AllowAutoRedirect = CheckBox1.Checked
            HTTP.KeepAlive = CheckBox2.Checked
            Try
                DisplayMessage("Alow Redirects: " & HTTP.AllowAutoRedirect)
                DisplayMessage("Keep Alive: " & HTTP.KeepAlive)
                DisplayMessage("UserAgent: " & HTTP.UserAgent)
                DisplayMessage("Method: " & HTTP.Method)
                DisplayMessage("Making Request...")
                RichTextBox1.Text = HTTP.Request(TextBox1.Text, TextBox2.Text, TextBox4.Text, Success)
                DisplayMessage("Request Complete.")
                IO.File.WriteAllText(Application.StartupPath & "\Request.html", RichTextBox1.Text)
                DisplayMessage("HTML saved to : " & Application.StartupPath & "\Request.html")
            Catch ex As Exception
                DisplayMessage("Error!")
                DisplayMessage(ex.Message)
            End Try
        End If
    End Sub
    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        Dim thrdRequest As New Threading.Thread(AddressOf DoRequest)
        thrdRequest.IsBackground = True
        thrdRequest.Start()
    End Sub
    Delegate Sub _DisplayMessage(ByVal Text As String)
    Sub DisplayMessage(ByVal Text As String)
        If InvokeRequired Then
            Invoke(New _DisplayMessage(AddressOf DisplayMessage), Text)
        Else
            Dim L As New ListViewItem(Now.ToString("hh:mm:ss tt"))
            L.SubItems.Add(Text)
            ListView1.Items.Add(L)
        End If
    End Sub
End Class
Public Class HTTP

    Public Shared Cookies As New Net.CookieContainer

#Region " UserAgent."
    Private Shared _UserAgent As String = "Mozilla/5.0 (Windows NT 6.1; rv:11.0) Gecko/20100101 Firefox/11.0" ' Default
    Public Shared Property UserAgent() As String
        Get
            Return _UserAgent
        End Get
        Set(ByVal UserAgent_ As String)
            _UserAgent = UserAgent_
        End Set
    End Property
#End Region

#Region " Method."
    Private Shared _Method As String = "GET" ' Default
    Public Shared Property Method() As String
        Get
            Return _Method
        End Get
        Set(ByVal Method_ As String)
            _Method = Method_.ToUpper
        End Set
    End Property
#End Region

#Region " AllowAutoRedirect."
    Private Shared _AllowAutoRedirect As Boolean = True ' Default
    Public Shared Property AllowAutoRedirect() As Boolean
        Get
            Return _AllowAutoRedirect
        End Get
        Set(ByVal AllowAutoRedirect_ As Boolean)
            _AllowAutoRedirect = AllowAutoRedirect_
        End Set
    End Property
#End Region

#Region " KeepAlive."
    Private Shared _KeepAlive As Boolean = True ' Default
    Public Shared Property KeepAlive() As Boolean
        Get
            Return _KeepAlive
        End Get
        Set(ByVal KeepAlive_ As Boolean)
            _KeepAlive = KeepAlive_
        End Set
    End Property
#End Region

    Public Shared Function Request(ByVal Host As String, Optional ByVal Referer As String = Nothing, Optional ByVal POSTData As String = Nothing, Optional ByVal SuccessString As String = Nothing) As String
        Try
            Dim Cookies_ As New Net.CookieContainer
            Dim WebR As Net.HttpWebRequest = DirectCast(Net.WebRequest.Create(Host), Net.HttpWebRequest)

            WebR.Method = _Method
            WebR.CookieContainer = Cookies_
            WebR.AllowAutoRedirect = _AllowAutoRedirect
            WebR.KeepAlive = _KeepAlive
            WebR.UserAgent = _UserAgent
            WebR.ContentType = "application/x-www-form-urlencoded"
            WebR.Referer = Referer

            If (_Method = "POST") Then
                Dim _PostData As Byte()
                _PostData = System.Text.Encoding.Default.GetBytes(POSTData)
                WebR.ContentLength = _PostData.Length

                Dim StreamWriter As System.IO.Stream = WebR.GetRequestStream()
                StreamWriter.Write(_PostData, 0, POSTData.Length)
                StreamWriter.Dispose()
            End If

            Dim WebResponse As Net.HttpWebResponse = DirectCast(WebR.GetResponse, Net.HttpWebResponse)
            Cookies_.Add(WebResponse.Cookies)
            Cookies = Cookies_
            Dim StreamReader As New System.IO.StreamReader(WebResponse.GetResponseStream)
            Dim PageHTML As String = StreamReader.ReadToEnd()
            If (SuccessString IsNot Nothing) Then
                If PageHTML.Contains(SuccessString) Then
                    Return "Success!"
                Else
                    Return "Failure!"
                End If
            Else
                Return PageHTML
            End If

        Catch ex As Exception
            Return ex.Message
        End Try
    End Function

End Class