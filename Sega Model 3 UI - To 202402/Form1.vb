﻿Option Strict Off
Imports System.ComponentModel
Imports System.IO
Imports System.Net
Imports System.Reflection.Emit
Imports System.Runtime.InteropServices
Imports System.Diagnostics
Imports System.Text
Imports System.Windows.Forms.VisualStyles.VisualStyleElement
Imports System.Xml
Imports SharpDX.XInput
Imports System.Threading
Imports System.Net.Http
Imports System.Threading.Tasks
Imports System.Runtime.InteropServices.ComTypes
Imports System.Timers

Public Class Form1
    Inherits Form
    Private x_timer As System.Threading.Timer
    Private x2_timer As System.Threading.Timer
    Private rep_timer As System.Threading.Timer

    Dim IgnoreClose As Boolean = False
    Dim Load_comp_F As Boolean = False
    Dim brdy = 0

    <System.Runtime.InteropServices.DllImport("winmm.dll", CharSet:=System.Runtime.InteropServices.CharSet.Auto)>
    Private Shared Function mciSendString(ByVal command As String,
    ByVal buffer As System.Text.StringBuilder,
    ByVal bufferSize As Integer, ByVal hwndCallback As IntPtr) As Integer
    End Function

    Private mysound As String = "mysound3"
    Private aliasName As String = "MediaFile"

    Private mouseDevices As New Dictionary(Of IntPtr, String)
    'Private WithEvents wm As New Wiimote()

    <DllImport("user32.dll")>
    Private Shared Function GetWindowRect(ByVal hWnd As IntPtr, ByRef rect As RECT) As Boolean
    End Function

    <StructLayout(LayoutKind.Sequential)>
    Private Structure RECT
        Public Left As Integer
        Public Top As Integer
        Public Right As Integer
        Public Bottom As Integer
    End Structure

    Private Function GetSupermodelWidth() As Integer?
        Dim processes() As Process = Process.GetProcessesByName("supermodel") ' Ensure this name is accurate
        If processes.Length > 0 Then
            Dim hWnd As IntPtr = processes(0).MainWindowHandle
            If hWnd <> IntPtr.Zero Then
                Dim rect As RECT
                If GetWindowRect(hWnd, rect) Then
                    Dim width As Integer = rect.Right - rect.Left
                    Return width
                End If
            End If
        End If
        Return Nothing
    End Function

    Private Function GetSupermodelHeight() As Integer?
        Dim processes() As Process = Process.GetProcessesByName("supermodel") ' Ensure this name is accurate
        If processes.Length > 0 Then
            Dim hWnd As IntPtr = processes(0).MainWindowHandle
            If hWnd <> IntPtr.Zero Then
                Dim rect As RECT
                If GetWindowRect(hWnd, rect) Then
                    Dim Height As Integer = rect.Bottom - rect.Top
                    Return Height
                End If
            End If
        End If
        Return Nothing
    End Function

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        Surround1.Interval = interval
        Surround2.Interval = interval
        DemoTimer.Interval = interval
        Dim appPath As String = System.Windows.Forms.Application.StartupPath
        Dim fileName As String = appPath & "\Supermodel.exe"
        If System.IO.File.Exists(fileName) Then
            Label37.Text = System.IO.File.GetLastWriteTime(fileName).ToString
        Else
            Dim result As DialogResult = MessageBox.Show("Supermodel.exe not found." & vbCrLf & "Want you download PonMi version of Supermodel3?",
                                             "質問",
                                             MessageBoxButtons.YesNo,
                                             MessageBoxIcon.Exclamation,
                                             MessageBoxDefaultButton.Button2)
            If result = DialogResult.Yes Then
                download()
                IgnoreClose = True
                Me.Close()
            ElseIf result = DialogResult.No Then

            End If
            Me.Close()
        End If

        fileName = appPath & "\Config"
        If System.IO.Directory.Exists(fileName) Then
            'Initialize DataTable
            GameData.Columns.Add("Games", GetType(String))
            GameData.Columns.Add("Version", GetType(String))
            GameData.Columns.Add("Roms", GetType(String))
            GameData.Columns.Add("Step", GetType(String))
            GameData.Columns.Add("A-E", GetType(String))
            GameData.Columns.Add("Inputs", GetType(String))
            DT_Roms.Columns.Add("name", GetType(String))

            LoadGameXml()
            Load_initialfile()
            If Forecolor_s = "White" Then
                WhiteToolStripMenuItem.PerformClick()
            Else
                BlackToolStripMenuItem.PerformClick()
            End If

            If Bgcolor_R = 147 And Bgcolor_G = 0 And Bgcolor_B = 80 Then
                Bgcolor_R = 0
                Bgcolor_G = 0
                Bgcolor_B = 255
            End If


            Me.BackColor = Color.FromArgb(255, Bgcolor_R, Bgcolor_G, Bgcolor_B)
            Label_path.BackColor = Color.FromArgb(255, Bgcolor_R, Bgcolor_G, Bgcolor_B)
            DataGridView_Setting()
            Select Case Last_Sort
                Case 0
                    C0_Sort_F = Not (C0_Sort_F)
                    Header0.PerformClick()
                Case 1
                    C1_Sort_F = Not (C1_Sort_F)
                    Header1.PerformClick()
                Case 2
                    C2_Sort_F = Not (C2_Sort_F)
                    Header2.PerformClick()
                Case 3
                    C3_Sort_F = Not (C3_Sort_F)
                    Header3.PerformClick()
                Case 4
                    C4_Sort_F = Not (C4_Sort_F)
                    Header4.PerformClick()
                Case 5
                    C5_Sort_F = Not (C5_Sort_F)
                    Header5.PerformClick()
                Case Else
            End Select
            LoadResolution()
            LastSelectRow()
            GetAllControls(Me, FontSize_bin)
        Else
            MessageBox.Show("Config folder not found.")
            Me.Close()
        End If
        RegisterRawInputDevices()
        InitializeMouseDevices()
        If Favorite.ToString = "Show All" Then
            ShowFavoriteToolStripMenuItem.PerformClick()
        End If
        UpdateDataGridView()
        Load_comp_F = True

        Timer3.Enabled = False
        Dim cmd As String
        cmd = "stop " + mysound
        mciSendString(cmd, Nothing, 0, IntPtr.Zero)
        cmd = "close " + mysound
        mciSendString(cmd, Nothing, 0, IntPtr.Zero)
        Dim Rnd As String = "up"
        Dim leng As Integer = 2000
        Dim sond As String = "sound\" & Rnd & ".mp3"
        Dim fileNames As String = sond
        cmd = "open """ + fileNames + """ type mpegvideo alias " + mysound

        If mciSendString(cmd, Nothing, 0, IntPtr.Zero) <> 0 Then
            Return
        End If
        cmd = "play " + mysound
        mciSendString(cmd, Nothing, 0, IntPtr.Zero)
        Timer3.Interval = leng
        Timer3.Enabled = True
        'ComboBox1.SelectedIndex = 0
        'ComboBox2.SelectedIndex = 0
        brdy = 1
    End Sub


    Private Sub download()
        System.Diagnostics.Process.Start("https://github.com/BackPonBeauty/Supermodel3-PonMi/releases")
    End Sub


    Private Sub DataGridView1_MouseDown(sender As Object, e As MouseEventArgs) Handles DataGridView1.MouseDown
        If e.Button = MouseButtons.Right Then
            ' 右クリック時のコンテキストメニューを表示
            Dim hit As DataGridView.HitTestInfo = DataGridView1.HitTest(e.X, e.Y)
            If hit.Type = DataGridViewHitTestType.Cell Then
                DataGridView1.CurrentCell = DataGridView1.Rows(hit.RowIndex).Cells(hit.ColumnIndex)
                Dim selectedCellValue As String = DataGridView1.CurrentRow.Cells(2).Value.ToString()
                Dim filePath As String = "favorite.txt"
                Dim isFavorite As Boolean = File.ReadAllLines(filePath).Contains(selectedCellValue)
                If isFavorite Then
                    ContextMenuStrip1.Items(0).Text = "Remove from Favorite"
                Else
                    ContextMenuStrip1.Items(0).Text = "Add Favorite"
                End If
                ContextMenuStrip1.Show(DataGridView1, e.Location)
            End If
        End If
    End Sub

    ' コンテキストメニューのアイテムクリックイベントハンドラ
    Private Sub contextMenuStrip1_ItemClicked(sender As Object, e As ToolStripItemClickedEventArgs) Handles ContextMenuStrip1.ItemClicked
        Dim selectedCellValue As String = DataGridView1.CurrentRow.Cells(2).Value.ToString()
        ' ファイルパスを設定
        Dim filePath As String = "favorite.txt"
        If ContextMenuStrip1.Items(0).Text = "Add Favorite" Then
            File.AppendAllText(filePath, selectedCellValue & Environment.NewLine)
        Else
            Dim lines As List(Of String) = File.ReadAllLines(filePath).ToList()
            lines.Remove(selectedCellValue)
            File.WriteAllLines(filePath, lines)
        End If
        If ShowFavoriteToolStripMenuItem.Text = "Show All" Then
            ShowFavoriteToolStripMenuItem.Text = "Show Favorites"
            ShowFavoriteToolStripMenuItem.PerformClick()
        Else
            UpdateDataGridView()
        End If
        Console.WriteLine(Favorite_n)
    End Sub

    Private Sub UpdateDataGridView()
        Dim filePath As String = "favorite.txt"
        Dim favoriteItems As New HashSet(Of String)(File.ReadAllLines(filePath))
        Favorite_n = 0
        For Each row As DataGridViewRow In DataGridView1.Rows
            Dim cellValue As String = row.Cells(2).Value.ToString()
            If favoriteItems.Contains(cellValue) Then
                row.DefaultCellStyle.ForeColor = Color.Pink ' 任意の色を設定
                Favorite_n += 1
            Else
                row.DefaultCellStyle.ForeColor = Color.White ' 元の色を設定
            End If
        Next
    End Sub


    Private Sub InitializeMouseDevices()
        Dim deviceCount As UInteger = 0
        Dim deviceListSize As UInteger = CUInt(Marshal.SizeOf(GetType(RawInput.RAWINPUTDEVICELIST)))

        ' 最初にデバイスの数を取得
        RawInput.GetRawInputDeviceList(IntPtr.Zero, deviceCount, deviceListSize)
        Console.WriteLine($"Number of devices: {deviceCount}")
        ' デバイスリストのバッファを作成
        Dim pRawInputDeviceList As IntPtr = Marshal.AllocHGlobal(CInt(deviceCount) * CInt(deviceListSize))

        ' デバイスリストを取得
        RawInput.GetRawInputDeviceList(pRawInputDeviceList, deviceCount, deviceListSize)

        Dim mouseIndex As Integer = 1

        For i = deviceCount - 1 To 0 Step -1

            Dim rid As RawInput.RAWINPUTDEVICELIST = CType(Marshal.PtrToStructure(New IntPtr(pRawInputDeviceList.ToInt64() + (i * deviceListSize)), GetType(RawInput.RAWINPUTDEVICELIST)), RawInput.RAWINPUTDEVICELIST)
            'Console.WriteLine(rid.hDevice)
            ' マウスデバイスのみ処理Or deviceName = "\\?\Microsoft HID RID\000D_0002\1"
            If rid.dwType = RawInput.RIM_TYPEMOUSE Then
                Dim size As UInteger = 0
                RawInput.GetRawInputDeviceInfo(rid.hDevice, RawInput.RIDI_DEVICENAME, IntPtr.Zero, size)
                If size > 0 Then
                    Dim nameBuilder As New StringBuilder(CInt(size))
                    RawInput.GetRawInputDeviceInfo(rid.hDevice, RawInput.RIDI_DEVICENAME, nameBuilder, size)
                    Dim deviceName As String = nameBuilder.ToString()
                    Console.WriteLine(deviceName)

                    mouseDevices(rid.hDevice) = $"MOUSE{mouseIndex}" '{deviceName}
                    Console.WriteLine($"{mouseIndex}{rid.hDevice}")
                    mouseIndex += 1
                End If
            End If
        Next

        Marshal.FreeHGlobal(pRawInputDeviceList)
    End Sub

    Protected Overrides Sub WndProc(ByRef m As Message)
        Const WM_INPUT As Integer = &HFF

        If m.Msg = WM_INPUT Then
            Dim dwSize As UInteger = 0
            RawInput.GetRawInputData(m.LParam, RawInput.RID_INPUT, IntPtr.Zero, dwSize, CUInt(Marshal.SizeOf(GetType(RawInput.RAWINPUTHEADER))))

            If dwSize > 0 Then
                Dim buffer As IntPtr = Marshal.AllocHGlobal(CInt(dwSize))
                Try
                    If RawInput.GetRawInputData(m.LParam, RawInput.RID_INPUT, buffer, dwSize, CUInt(Marshal.SizeOf(GetType(RawInput.RAWINPUTHEADER)))) = dwSize Then
                        Dim raw As RawInput.RAWINPUT = Marshal.PtrToStructure(Of RawInput.RAWINPUT)(buffer)

                        If raw.header.dwType = RawInput.RIM_TYPEMOUSE Then
                            Dim mouse As RawInput.RAWMOUSE = raw.mouse
                            Dim deviceName As String = If(mouseDevices.ContainsKey(raw.header.hDevice), mouseDevices(raw.header.hDevice), "Unknown Mouse")
                            'Dim wheelDelta As Short = CType(mouse.usButtonData, Short)
                            'Console.WriteLine($"Device Name: {raw.header.hDevice}")
                            'Console.WriteLine($"Button Flags: {mouse.usButtonFlags}")
                            'Console.WriteLine($"Button Data: {mouse.usButtonData}")
                            ' ボタンの状態をチェック
                            Dim data1 As String = ""
                            If mouse.usButtonData = 1 Or mouse.usButtonData = 2 Then
                                data1 = "LEFT_BUTTON"
                            End If
                            If mouse.usButtonData = 4 Or mouse.usButtonData = 8 Then
                                data1 = "RIGHT_BUTTON"
                            End If
                            If mouse.usButtonData = 16 Or mouse.usButtonData = 32 Then
                                data1 = "MIDDLE_BUTTON"
                            End If
                            If mouse.usButtonData = 7865344 Then
                                data1 = "ZAXIS_POS"
                            End If
                            If mouse.usButtonData = -7863296 Then
                                data1 = "ZAXIS_NEG"
                            End If
                            Dim deltaX As Integer = mouse.lLastX
                            Dim deltaY As Integer = mouse.lLastY

                            If deltaY > 0 Then
                                data1 = "YAXIS_POS"
                            End If
                            If deltaY < 0 Then
                                data1 = "YAXIS_NEG"
                            End If
                            If deltaX > 0 Then
                                data1 = "XAXIS_POS"
                            End If
                            If deltaX < 0 Then
                                data1 = "XAXIS_NEG"
                            End If


                            'If (mouse.usButtonFlags And RawInput.RI_MOUSE_LEFT_BUTTON_UP) <> 0 Then
                            '    Console.WriteLine($"{deviceName}: Left button up")
                            'End If
                            'If (mouse.usButtonFlags And RawInput.RI_MOUSE_RIGHT_BUTTON_DOWN) <> 0 Then
                            '    Console.WriteLine($"{deviceName}: Right button down")
                            'End If
                            'If (mouse.usButtonFlags And RawInput.RI_MOUSE_RIGHT_BUTTON_UP) <> 0 Then
                            '    Console.WriteLine($"{deviceName}: Right button up")
                            'End If
                            'If (mouse.usButtonFlags And RawInput.RI_MOUSE_MIDDLE_BUTTON_DOWN) <> 0 Then
                            '    Console.WriteLine($"{deviceName}: Middle button down")
                            'End If
                            'If (mouse.usButtonFlags And RawInput.RI_MOUSE_MIDDLE_BUTTON_UP) <> 0 Then
                            '    Console.WriteLine($"{deviceName}: Middle button up")
                            'End If
                            'If (mouse.usButtonFlags And RawInput.RI_MOUSE_BUTTON_4_DOWN) <> 0 Then
                            '    Console.WriteLine($"{deviceName}: Button 4 down")
                            'End If
                            'If (mouse.usButtonFlags And RawInput.RI_MOUSE_BUTTON_4_UP) <> 0 Then
                            '    Console.WriteLine($"{deviceName}: Button 4 up")
                            'End If
                            'If (mouse.usButtonFlags And RawInput.RI_MOUSE_BUTTON_5_DOWN) <> 0 Then
                            '    Console.WriteLine($"{deviceName}: Button 5 down")
                            'End If
                            'If (mouse.usButtonFlags And RawInput.RI_MOUSE_BUTTON_5_UP) <> 0 Then
                            '    Console.WriteLine($"{deviceName}: Button 5 up")
                            'End If

                            ' マウスの移動量を取得する場合

                            'Dim wheelDelta As Short = BitConverter.ToInt16(BitConverter.GetBytes(mouse.usButtonData), 0)

                            'If wheelDelta > 0 Then
                            'Console.WriteLine($"{deviceName}: Wheel  {wheelDelta}")
                            'ElseIf wheelDelta < 0 Then
                            '    Console.WriteLine($"{deviceName}: Wheel down {wheelDelta}")
                            'End If
                            If RawInput_Enabled = True Then
                                Label1.Text = ($"{deviceName}_{data1}")
                            End If
                            'Console.WriteLine($"{deviceName} ButtonsState : {mouse.usButtonData} ,Delta X: {deltaX}, Delta Y: {deltaY}" & vbCrLf)
                        End If
                    End If
                Finally
                    Marshal.FreeHGlobal(buffer)
                End Try
            End If
        End If
        MyBase.WndProc(m)
    End Sub

    Private Sub RegisterRawInputDevices()
        Dim rid As New RawInputDevice()
        rid.usUsagePage = &H1
        rid.usUsage = &H2
        rid.dwFlags = RawInput.RIDEV_INPUTSINK
        rid.hwndTarget = Me.Handle

        If Not RegisterRawInputDevices(New RawInputDevice() {rid}, 1, CUInt(Marshal.SizeOf(rid))) Then
            Throw New ApplicationException("Failed to register raw input devices.")
            Console.WriteLine("Failed to register raw input devices.")
        Else
            Console.WriteLine("Success to register raw input devices.")
        End If
    End Sub

    <DllImport("user32.dll", SetLastError:=True)>
    Public Shared Function RegisterRawInputDevices(ByVal pRawInputDevices() As RawInputDevice, ByVal uiNumDevices As UInteger, ByVal cbSize As UInteger) As Boolean
    End Function

    <StructLayout(LayoutKind.Sequential)>
    Public Structure RawInputDevice
        Public usUsagePage As UShort
        Public usUsage As UShort
        Public dwFlags As UInteger
        Public hwndTarget As IntPtr
    End Structure

    <DllImport("KERNEL32.DLL", CharSet:=CharSet.Auto)>
    Public Shared Function GetPrivateProfileString(
                              lpAppName As String,
                              lpKeyName As String,
                              lpDefault As String,
                              lpReturnedString As StringBuilder,
                              nSize As Integer,
                              lpFileName As String) As Integer
    End Function
    <DllImport("KERNEL32.DLL", CharSet:=CharSet.Auto)>
    Public Shared Function WritePrivateProfileString(
                              ByVal lpApplicationName As String,
                              ByVal lpKeyName As String,
                              ByVal lpString As String,
                              ByVal lpFileName As StringBuilder) As Integer
    End Function

    Public Structure ListInfo
        Public Property Lever_s As Integer

    End Structure
    Public Structure ListInfo2
        Public Property Lever_s As Integer

    End Structure



    Dim GameData As New DataTable
    Public Roms As String
    Dim Inputs As String
    Dim DT_Roms As New DataTable
    Dim List_Rec As New List(Of ListInfo)
    Dim List_dammy As New List(Of ListInfo2)

    Dim DT_Rep As New DataTable
    Dim keys As New DataTable
    Public ScreenN(3) As String
    Public Bx(3) As Integer
    Public By(3) As Integer
    Public Bw(3) As Integer
    Public Bh(3) As Integer

    Dim Columns0Width As StringBuilder = New StringBuilder(300)
    Dim Columns1Width As StringBuilder = New StringBuilder(300)
    Dim Columns2Width As StringBuilder = New StringBuilder(300)
    Dim Columns3Width As StringBuilder = New StringBuilder(300)
    Dim Columns4Width As StringBuilder = New StringBuilder(300)
    Dim Columns5Width As StringBuilder = New StringBuilder(300)

    Dim Columns0_i As Integer
    Dim Columns1_i As Integer
    Dim Columns2_i As Integer
    Dim Columns3_i As Integer
    Dim Columns4_i As Integer
    Dim Columns5_i As Integer

    Dim C0_Sort_F As Boolean = False
    Dim C1_Sort_F As Boolean = False
    Dim C2_Sort_F As Boolean = False
    Dim C3_Sort_F As Boolean = False
    Dim C4_Sort_F As Boolean = False
    Dim C5_Sort_F As Boolean = False

    Dim Last_Sort As Integer = 0
    Dim Onece As Boolean = False
    Dim Last_SelectedItem As String = ""
    Dim Last_SelectedRow As Integer = 0
    Dim Last_SelectedRow_bin As Integer = 0
    Public FontSize_bin As Integer = 10
    Dim Resolution_index_bin As Integer = 0
    Public Bgcolor_R As Integer = 147
    Public Bgcolor_G As Integer = 0
    Public Bgcolor_B As Integer = 80
    Public Pub_Forecolor_s As Color = Color.White
    Public Forecolor_s As String = "White"
    Public Outputs_F As Boolean
    Public ScanLine_F As Boolean = False
    Public Capture_F As Boolean = False
    Dim Front_F As Boolean = True
    Dim Scanline_Enabled As Boolean = False
    Dim RawInput_Enabled As Boolean = False
    Public Opacity_D As Double = 0.5
    'Dim vGen As New vGen
    Dim interval As Integer = 1
    Dim Favorite_n As Integer = 0

    Dim N As Integer = 0
    Dim Rec As Boolean = False
    Dim XTimer_F As Boolean = False

    WithEvents KeyboardHooker1 As New Key
    Public Sub New()
        InitializeComponent()
        'x_timer = New System.Threading.Timer(AddressOf SurroundingSub1)
        x2_timer = New System.Threading.Timer(AddressOf SurroundingSub2)
        rep_timer = New System.Threading.Timer(AddressOf DemoPlay)

    End Sub



    'DragMove
    Private Sub Form1_ResizeEnd(sender As Object, e As EventArgs) Handles MyBase.ResizeEnd

        'Label1.Text = "( " & Me.Left & " , " & Me.Top & " )"
        Dim s As System.Windows.Forms.Screen = System.Windows.Forms.Screen.FromControl(Me)
        'ディスプレイの高さと幅を取得
        Dim x As Integer = s.Bounds.X
        Dim y As Integer = s.Bounds.Y
        Label_hScreenRes.Text = CStr(s.Bounds.Height)
        Label_wScreenRes.Text = CStr(s.Bounds.Width)

        Dim N As Integer
        For i = 0 To ScreenN.Length - 1
            If Me.Top >= By(i) And Me.Top <= (By(i) + Bh(i)) And Me.Left >= Bx(i) And Me.Left <= (Bx(i) + Bw(i)) Then
                Label2.Text = ScreenN(i).ToString
                N = i
                Exit For

            End If
        Next

    End Sub




    Private Sub LoadResolution()
        Dim line As String = ""
        Dim al As New ArrayList

        Using sr As StreamReader = New StreamReader(
          "Resolution.txt", Encoding.GetEncoding("UTF-8"))

            line = sr.ReadLine()
            Do Until line Is Nothing
                ComboBox_resolution.Items.Add(line)
                line = sr.ReadLine()
            Loop
            ComboBox_resolution.SelectedIndex = Resolution_index_bin
        End Using
    End Sub

    Private Sub LastSelectRow()
        DataGridView1.CurrentCell = DataGridView1(0, Last_SelectedRow_bin)
        'Debug("LastSelect = " & Last_SelectedRow.ToString)
    End Sub

    Private Sub LoadGameXml()
        Dim xmlDoc As New XmlDocument()
        Dim appPath As String = System.Windows.Forms.Application.StartupPath
        Dim fileName As String = $"{appPath}\Config\Games.xml"

        If Not System.IO.File.Exists(fileName) Then
            MessageBox.Show("Games.xml not found.")
            Me.Close()
            Exit Sub
        End If

        xmlDoc.Load(fileName)
        Dim games = xmlDoc.SelectNodes("//game")

        For Each game As XmlNode In games
            Dim title = game.SelectSingleNode("identity/title")?.InnerText
            Dim version = game.SelectSingleNode("identity/version")?.InnerText
            Dim romName = game.Attributes("name")?.Value
            Dim stepping = game.SelectSingleNode("hardware/stepping")?.InnerText

            Dim inputs = game.SelectNodes("hardware/inputs/input")
            Dim inputTypes_bin = String.Join(", ", inputs.Cast(Of XmlNode)().Select(Function(input) input.Attributes("type")?.Value))
            Dim inputTypes() As String = inputTypes_bin.Split(",")
            GameData.Rows.Add(title, version, romName, stepping, "", inputTypes(1).Trim)
            'ComboBox1.Items.Add(title)
        Next
    End Sub
    'Private Sub Load_gamexml()
    '    XmlReader
    '    Dim xmlDoc As New XmlDocument()
    '    Dim xroot As XmlNode
    '    Dim xfolder As XmlNodeList
    '    Dim xnode As XmlNode
    '    StringBuilder
    '    Dim xname(1000) As String
    '    Dim xVersion(1000) As String
    '    Dim xRoms(1000) As String
    '    Dim xStep(1000) As String
    '    Dim xInputType(1000) As String
    '    Dim appPath As String = System.Windows.Forms.Application.StartupPath
    '    Dim fileName As String = appPath & "\Config\Games.xml"
    '    If System.IO.File.Exists(fileName) Then
    '        xmlDoc.Load(appPath & "\Config\Games.xml")
    '        xroot = xmlDoc.DocumentElement
    '        xfolder = xroot.SelectNodes("//game")
    '        Dim i As Integer = 1
    '        For Each xnode In xfolder
    '            xname(i) = xnode.SelectSingleNode("//game[" & i & "]/identity/title").InnerText
    '            xVersion(i) = xnode.SelectSingleNode("//game[" & i & "]/identity/version").InnerText
    '            xRoms(i) = xnode.SelectSingleNode("//game[" & i & "]/@name").Value
    '            xStep(i) = xnode.SelectSingleNode("//game[" & i & "]/hardware/stepping").InnerText
    '            Dim j As Integer = 2
    '            Dim InputNodes = xnode.SelectNodes("//game[" & i & "]/hardware/inputs/input[" & j & "]")
    '            xInputType(i) = String.Join(", ", InputNodes.Cast(Of XmlNode).Select(Function(inputNode) inputNode.Attributes("type").Value))
    '            GameData.Rows.Add(xname(i), xVersion(i), xRoms(i), xStep(i), " ", xInputType(i))
    '            i += 1
    '        Next
    '    Else
    '        MessageBox.Show("Games.xml not found.")
    '        Me.Close()
    '    End If


    'End Sub

    Private Sub DataGridView_Setting()

        DataGridView1.DataSource = GameData
        DataGridView1.RowHeadersVisible = False
        DataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        DataGridView1.AllowUserToResizeColumns = True
        DataGridView1.AllowUserToResizeRows = False
        DataGridView1.MultiSelect = False
        DataGridView1.ReadOnly = True
        DataGridView1.AllowUserToAddRows = False
        DataGridView1.Columns(3).DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight
        DataGridView1.DefaultCellStyle.BackColor = Color.FromArgb(54, 57, 63)
        DataGridView1.DefaultCellStyle.ForeColor = Color.White
        Console.WriteLine(Columns0_i)
        DataGridView1.Columns(0).Width = Columns0_i
        DataGridView1.Columns(1).Width = Columns1_i
        DataGridView1.Columns(2).Width = Columns2_i
        DataGridView1.Columns(3).Width = Columns3_i
        DataGridView1.Columns(4).Width = Columns4_i
        DataGridView1.Columns(5).Width = Columns5_i
        DataGridView1.Columns(4).DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
        DataGridView1.Columns(4).HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter
        For Each c As DataGridViewColumn In DataGridView1.Columns
            c.SortMode = DataGridViewColumnSortMode.Programmatic
        Next c

        Try
            DataGridView1.CurrentCell = DataGridView1.Rows(0).Cells(0)
        Catch ex As Exception
        End Try

    End Sub


    Private Sub DataGridView1_ColumnWidthChanged(ByVal sender As Object, ByVal e As DataGridViewColumnEventArgs) Handles DataGridView1.ColumnWidthChanged
        If Load_comp_F = True Then
            Columns0_i = DataGridView1.Columns(0).Width
            Columns1_i = DataGridView1.Columns(1).Width
            Columns2_i = DataGridView1.Columns(2).Width
            Columns3_i = DataGridView1.Columns(3).Width
            Columns4_i = DataGridView1.Columns(4).Width
            Columns5_i = DataGridView1.Columns(5).Width
        End If

    End Sub

    Dim Favorite As StringBuilder = New StringBuilder(300)
    Private Sub Load_initialfile()
        Dim appPath As String = System.Windows.Forms.Application.StartupPath
        Dim fileName As String = appPath & "\Config\Supermodel.ini"
        If System.IO.File.Exists(fileName) Then


            Dim iniFileName = "Config\Supermodel.ini"
            Dim RefreshRate As StringBuilder = New StringBuilder(300)
            Dim TrueAR As StringBuilder = New StringBuilder(300)
            Dim XResolution As StringBuilder = New StringBuilder(300)
            Dim YResolution As StringBuilder = New StringBuilder(300)
            Dim WindowXPosition As StringBuilder = New StringBuilder(300)
            Dim WindowYPosition As StringBuilder = New StringBuilder(300)
            Dim BorderlessWindow As StringBuilder = New StringBuilder(300)

            Dim New3DEngine As StringBuilder = New StringBuilder(300)
            Dim QuadRendering As StringBuilder = New StringBuilder(300)
            Dim WideScreen As StringBuilder = New StringBuilder(300)
            Dim Stretch As StringBuilder = New StringBuilder(300)
            Dim WideBackground As StringBuilder = New StringBuilder(300)
            Dim Crosshairs As StringBuilder = New StringBuilder(300)
            Dim GPUMultiThreaded As StringBuilder = New StringBuilder(300)
            Dim MultiThreaded As StringBuilder = New StringBuilder(300)
            Dim MultiTexture As StringBuilder = New StringBuilder(300)
            Dim VSync As StringBuilder = New StringBuilder(300)
            Dim FullScreen As StringBuilder = New StringBuilder(300)
            Dim Throttle As StringBuilder = New StringBuilder(300)
            Dim ShowFrameRate As StringBuilder = New StringBuilder(300)

            Dim PowerPCFrequency As StringBuilder = New StringBuilder(300)
            Dim Supersampling As StringBuilder = New StringBuilder(300)
            Dim CRTcolors As StringBuilder = New StringBuilder(300)
            Dim UpscaleMode As StringBuilder = New StringBuilder(300)

            Dim EmulateSound As StringBuilder = New StringBuilder(300)
            Dim EmulateDSB As StringBuilder = New StringBuilder(300)
            Dim FlipStereo As StringBuilder = New StringBuilder(300)
            Dim LegacySoundDSP As StringBuilder = New StringBuilder(300)
            Dim MusicVolume As StringBuilder = New StringBuilder(300)
            Dim SoundVolume As StringBuilder = New StringBuilder(300)
            Dim Balance As StringBuilder = New StringBuilder(300)

            Dim ForceFeedback As StringBuilder = New StringBuilder(300)

            Dim Network As StringBuilder = New StringBuilder(300)
            Dim SimulateNet As StringBuilder = New StringBuilder(300)
            Dim PortIn As StringBuilder = New StringBuilder(300)
            Dim PortOut As StringBuilder = New StringBuilder(300)
            Dim AddressOut As StringBuilder = New StringBuilder(300)

            Dim DirectInputConstForceLeftMax As StringBuilder = New StringBuilder(300)
            Dim DirectInputConstForceRightMax As StringBuilder = New StringBuilder(300)
            Dim DirectInputSelfCenterMax As StringBuilder = New StringBuilder(300)
            Dim DirectInputFrictionMax As StringBuilder = New StringBuilder(300)
            Dim DirectInputVibrateMax As StringBuilder = New StringBuilder(300)

            Dim XInputConstForceThreshold As StringBuilder = New StringBuilder(300)
            Dim XInputConstForceMax As StringBuilder = New StringBuilder(300)
            Dim XInputVibrateMax As StringBuilder = New StringBuilder(300)

            Dim InputSystem As StringBuilder = New StringBuilder(300)

            Dim InputAutoTrigger As StringBuilder = New StringBuilder(300)
            Dim InputAutoTrigger2 As StringBuilder = New StringBuilder(300)
            Dim HideCMD As StringBuilder = New StringBuilder(300)
            Dim Dir As StringBuilder = New StringBuilder(300)
            Dim CrosshairStyle As StringBuilder = New StringBuilder(300)

            Dim C0_F As StringBuilder = New StringBuilder(300)
            Dim C1_F As StringBuilder = New StringBuilder(300)
            Dim C2_F As StringBuilder = New StringBuilder(300)
            Dim C3_F As StringBuilder = New StringBuilder(300)
            Dim C4_F As StringBuilder = New StringBuilder(300)
            Dim C5_F As StringBuilder = New StringBuilder(300)

            Dim Last_Sort_s As StringBuilder = New StringBuilder(300)
            Dim Last_Selected_s As StringBuilder = New StringBuilder(300)

            Dim FontSize As StringBuilder = New StringBuilder(300)
            Dim Resolution_index As StringBuilder = New StringBuilder(300)
            Dim BgcolorR As StringBuilder = New StringBuilder(300)
            Dim BgcolorG As StringBuilder = New StringBuilder(300)
            Dim BgcolorB As StringBuilder = New StringBuilder(300)
            Dim Forecolor As StringBuilder = New StringBuilder(300)
            Dim Title_SB As StringBuilder = New StringBuilder(300)
            Dim Outputs As StringBuilder = New StringBuilder(300)
            Dim Scanline As StringBuilder = New StringBuilder(300)
            Dim Gamepad As StringBuilder = New StringBuilder(300)
            Dim Opacity As StringBuilder = New StringBuilder(30000)
            Dim SS As StringBuilder = New StringBuilder(300)


            GetPrivateProfileString(" Global ", "RefreshRate", "57.524160", RefreshRate, 15, iniFileName)
            GetPrivateProfileString(" Global ", "true-ar", "False", TrueAR, 15, iniFileName)
            GetPrivateProfileString(" Global ", "Supersampling", "1", Supersampling, 15, iniFileName)

            GetPrivateProfileString(" Global ", "CRTcolors", "0", CRTcolors, 15, iniFileName)
            GetPrivateProfileString(" Global ", "UpscaleMode", "2", UpscaleMode, 15, iniFileName)

            GetPrivateProfileString(" Global ", "XResolution", "496", XResolution, 15, iniFileName)
            GetPrivateProfileString(" Global ", "YResolution", "384", YResolution, 15, iniFileName)
            GetPrivateProfileString(" Global ", "WindowXPosition", "50", WindowXPosition, 15, iniFileName)
            GetPrivateProfileString(" Global ", "WindowYPosition", "50", WindowYPosition, 15, iniFileName)
            GetPrivateProfileString(" Global ", "BorderlessWindow", "False", BorderlessWindow, 15, iniFileName)
            GetPrivateProfileString(" Global ", "New3DEngine", "True", New3DEngine, 15, iniFileName)
            GetPrivateProfileString(" Global ", "QuadRendering", "True", QuadRendering, 15, iniFileName)
            GetPrivateProfileString(" Global ", "WideScreen", "True", WideScreen, 15, iniFileName)
            GetPrivateProfileString(" Global ", "Stretch", "False", Stretch, 15, iniFileName)
            GetPrivateProfileString(" Global ", "WideBackground", "False", WideBackground, 15, iniFileName)

            GetPrivateProfileString(" Global ", "Crosshairs", "3", Crosshairs, 15, iniFileName)

            GetPrivateProfileString(" Global ", "GPUMultiThreaded", "True", GPUMultiThreaded, 15, iniFileName)
            GetPrivateProfileString(" Global ", "MultiThreaded", "True", MultiThreaded, 15, iniFileName)
            GetPrivateProfileString(" Global ", "MultiTexture", "True", MultiTexture, 15, iniFileName)
            GetPrivateProfileString(" Global ", "VSync", "False", VSync, 15, iniFileName)

            GetPrivateProfileString(" Global ", "FullScreen", "False", FullScreen, 15, iniFileName)
            GetPrivateProfileString(" Global ", "Throttle", "True", Throttle, 15, iniFileName)
            GetPrivateProfileString(" Global ", "ShowFrameRate", "False", ShowFrameRate, 15, iniFileName)
            GetPrivateProfileString(" Global ", "PowerPCFrequency", "58", PowerPCFrequency, 15, iniFileName)

            GetPrivateProfileString(" Global ", "EmulateSound", "True", EmulateSound, 15, iniFileName)
            GetPrivateProfileString(" Global ", "EmulateDSB", "True", EmulateDSB, 15, iniFileName)
            GetPrivateProfileString(" Global ", "FlipStereo", "False", FlipStereo, 15, iniFileName)
            GetPrivateProfileString(" Global ", "LegacySoundDSP", "False", LegacySoundDSP, 15, iniFileName)
            GetPrivateProfileString(" Global ", "MusicVolume", "100", MusicVolume, 15, iniFileName)
            GetPrivateProfileString(" Global ", "SoundVolume", "100", SoundVolume, 15, iniFileName)
            GetPrivateProfileString(" Global ", "Balance", "0", Balance, 15, iniFileName)

            GetPrivateProfileString(" Global ", "ForceFeedback", "True", ForceFeedback, 15, iniFileName)
            GetPrivateProfileString(" Supermodel3 UI ", "Outputs", "False", Outputs, 15, iniFileName)

            GetPrivateProfileString(" Global ", "Network", "True", Network, 15, iniFileName)
            GetPrivateProfileString(" Global ", "SimulateNet", "True", SimulateNet, 15, iniFileName)
            GetPrivateProfileString(" Global ", "PortIn", "1971", PortIn, 15, iniFileName)
            GetPrivateProfileString(" Global ", "PortOut", "1972", PortOut, 15, iniFileName)
            GetPrivateProfileString(" Global ", "AddressOut", "127.0.0.1", AddressOut, 15, iniFileName)

            GetPrivateProfileString(" Global ", "DirectInputConstForceLeftMax", "100", DirectInputConstForceLeftMax, 15, iniFileName)
            GetPrivateProfileString(" Global ", "DirectInputConstForceRightMax", "100", DirectInputConstForceRightMax, 15, iniFileName)
            GetPrivateProfileString(" Global ", "DirectInputSelfCenterMax", "100", DirectInputSelfCenterMax, 15, iniFileName)
            GetPrivateProfileString(" Global ", "DirectInputFrictionMax", "100", DirectInputFrictionMax, 15, iniFileName)
            GetPrivateProfileString(" Global ", "DirectInputVibrateMax", "100", DirectInputVibrateMax, 15, iniFileName)

            GetPrivateProfileString(" Global ", "XInputConstForceThreshold", "100", XInputConstForceThreshold, 15, iniFileName)
            GetPrivateProfileString(" Global ", "XInputConstForceMax", "100", XInputConstForceMax, 15, iniFileName)
            GetPrivateProfileString(" Global ", "XInputVibrateMax", "100", XInputVibrateMax, 15, iniFileName)

            GetPrivateProfileString(" Global ", "InputSystem", "xinput", InputSystem, 15, iniFileName)

            'GetPrivateProfileString(" Global ", "InputAutoTrigger", "Error", InputAutoTrigger, 15, iniFileName)
            'GetPrivateProfileString(" Global ", "InputAutoTrigger2", "Error", InputAutoTrigger2, 15, iniFileName)

            GetPrivateProfileString(" Supermodel3 UI ", "HideCMD", "False", HideCMD, 15, iniFileName)

            GetPrivateProfileString(" Supermodel3 UI ", "Dir", "C:\天上天下唯我独尊\Roms", Dir, 150, iniFileName)
            GetPrivateProfileString(" Global ", "Title", "Supermodel", Title_SB, 150, iniFileName)

            GetPrivateProfileString(" Global ", "CrosshairStyle", "vector", CrosshairStyle, 15, iniFileName)

            GetPrivateProfileString(" Supermodel3 UI ", "Columns0Width", CStr(200), Columns0Width, 15, iniFileName)
            GetPrivateProfileString(" Supermodel3 UI ", "Columns1Width", CStr(150), Columns1Width, 15, iniFileName)
            GetPrivateProfileString(" Supermodel3 UI ", "Columns2Width", CStr(120), Columns2Width, 15, iniFileName)
            GetPrivateProfileString(" Supermodel3 UI ", "Columns3Width", CStr(50), Columns3Width, 15, iniFileName)
            GetPrivateProfileString(" Supermodel3 UI ", "Columns4Width", CStr(50), Columns4Width, 15, iniFileName)
            GetPrivateProfileString(" Supermodel3 UI ", "Columns5Width", CStr(120), Columns5Width, 15, iniFileName)

            GetPrivateProfileString(" Supermodel3 UI ", "Columns0Sort", "False", C0_F, 15, iniFileName)
            GetPrivateProfileString(" Supermodel3 UI ", "Columns1Sort", "False", C1_F, 15, iniFileName)
            GetPrivateProfileString(" Supermodel3 UI ", "Columns2Sort", "False", C2_F, 15, iniFileName)
            GetPrivateProfileString(" Supermodel3 UI ", "Columns3Sort", "False", C3_F, 15, iniFileName)
            GetPrivateProfileString(" Supermodel3 UI ", "Columns4Sort", "False", C4_F, 15, iniFileName)
            GetPrivateProfileString(" Supermodel3 UI ", "Columns5Sort", "False", C5_F, 15, iniFileName)

            GetPrivateProfileString(" Supermodel3 UI ", "LastSort", CStr(0), Last_Sort_s, 15, iniFileName)
            GetPrivateProfileString(" Supermodel3 UI ", "LastSelectedRow", CStr(0), Last_Selected_s, 15, iniFileName)
            GetPrivateProfileString(" Supermodel3 UI ", "FontSize", CStr(10), FontSize, 15, iniFileName)
            GetPrivateProfileString(" Supermodel3 UI ", "Resolution_index", CStr(10), Resolution_index, 15, iniFileName)
            GetPrivateProfileString(" Supermodel3 UI ", "BackColor_R", "147", BgcolorR, 15, iniFileName)
            GetPrivateProfileString(" Supermodel3 UI ", "BackColor_G", "0", BgcolorG, 15, iniFileName)
            GetPrivateProfileString(" Supermodel3 UI ", "BackColor_B", "80", BgcolorB, 15, iniFileName)
            GetPrivateProfileString(" Supermodel3 UI ", "ForeColor", "White", Forecolor, 15, iniFileName)
            'GetPrivateProfileString(" Supermodel3 UI ", "BackColor_B", "80", BgcolorB, 15, iniFileName)
            'GetPrivateProfileString(" Supermodel3 UI ", "ForeColor", "White", Forecolor, 15, iniFileName)
            GetPrivateProfileString(" Supermodel3 UI ", "Scanline", "False", Scanline, 15, iniFileName)
            GetPrivateProfileString(" Supermodel3 UI ", "Gamepad", "False", Gamepad, 15, iniFileName)
            GetPrivateProfileString(" Supermodel3 UI ", "Opacity", "5", Opacity, 15, iniFileName)
            GetPrivateProfileString(" Supermodel3 UI ", "SS", "False", SS, 15, iniFileName)
            GetPrivateProfileString(" Supermodel3 UI ", "Favorite", "False", Favorite, 15, iniFileName)



            'SuperSampling
            If SS.ToString = True Then
                CheckBox_ss.Checked = True
            Else
                CheckBox_ss.Checked = False
            End If

            'CRTcolor
            ComboBox1.SelectedIndex = Integer.Parse(CRTcolors.ToString)

            'UpscaleMode
            ComboBox2.SelectedIndex = Integer.Parse(UpscaleMode.ToString)



            'Opacity
            Dim result As Integer
            If Integer.TryParse(Opacity.ToString, result) = False Then
                Opacity_D = 5
            Else
                Opacity_D = Integer.Parse(Opacity.ToString())
            End If
            'Scanline
            If Scanline.ToString() = "True" Or VSync.ToString() = "1" Then
                Button_hook.PerformClick()
            End If

            'Gamepad
            If Gamepad.ToString() = "True" Or VSync.ToString() = "1" Then
                Button_X.PerformClick()
            End If

            'BackColor
            Bgcolor_R = CInt(BgcolorR.ToString)
            Bgcolor_G = CInt(BgcolorG.ToString)
            Bgcolor_B = CInt(BgcolorB.ToString)

            'ForeColor
            Forecolor_s = Forecolor.ToString

            'Columuns
            Columns0_i = Integer.Parse(Columns0Width.ToString)
            Columns1_i = Integer.Parse(Columns1Width.ToString)
            Columns2_i = Integer.Parse(Columns2Width.ToString)
            Columns3_i = Integer.Parse(Columns3Width.ToString)
            Columns4_i = Integer.Parse(Columns4Width.ToString)
            Columns5_i = Integer.Parse(Columns5Width.ToString)

            'Columuns Sort Flag
            If C0_F.ToString() = "True" Then
                C0_Sort_F = True
            Else
                C0_Sort_F = False
            End If
            If C1_F.ToString() = "True" Then
                C1_Sort_F = True
            Else
                C1_Sort_F = False
            End If
            If C2_F.ToString() = "True" Then
                C2_Sort_F = True
            Else
                C2_Sort_F = False
            End If
            If C3_F.ToString() = "True" Then
                C3_Sort_F = True
            Else
                C3_Sort_F = False
            End If
            If C4_F.ToString() = "True" Then
                C4_Sort_F = True
            Else
                C4_Sort_F = False
            End If
            If C5_F.ToString() = "True" Then
                C5_Sort_F = True
            Else
                C5_Sort_F = False
            End If

            'Last_Sort
            Last_Sort = Integer.Parse(Last_Sort_s.ToString)

            'Last_Selected
            Last_SelectedRow = Integer.Parse(Last_Selected_s.ToString)
            Last_SelectedRow_bin = Integer.Parse(Last_Selected_s.ToString)

            'FontSize
            FontSize_bin = Integer.Parse(FontSize.ToString)

            'Resolution_index
            Resolution_index_bin = Integer.Parse(Resolution_index.ToString)

            'New3DEngine
            If New3DEngine.ToString() = "True" Or New3DEngine.ToString() = "1" Then
                RadioButton_new3d.Checked = True
                RadioButton_legacy.Checked = False
            Else
                RadioButton_new3d.Checked = False
                RadioButton_legacy.Checked = True
            End If

            'VSync
            If VSync.ToString() = "True" Or VSync.ToString() = "1" Then
                CheckBox_vsync.Checked = True
            Else
                CheckBox_vsync.Checked = False
            End If

            'QuadRendering
            If QuadRendering.ToString() = "True" Or QuadRendering.ToString() = "1" Then
                CheckBox_quadrender.Checked = True
            Else
                CheckBox_quadrender.Checked = False
            End If

            'GPUMultiThreaded
            If GPUMultiThreaded.ToString() = "True" Or GPUMultiThreaded.ToString() = "1" Then
                CheckBox_gpumulti.Checked = True
            Else
                CheckBox_gpumulti.Checked = False
            End If

            'MultiThreaded
            If MultiThreaded.ToString() = "True" Or MultiThreaded.ToString() = "1" Then
                CheckBox_multishread.Checked = True
            Else
                CheckBox_multishread.Checked = False
            End If

            'MultiTexture
            If MultiTexture.ToString() = "True" Or MultiTexture.ToString() = "1" Then
                CheckBox_multitexture.Checked = True
            Else
                CheckBox_multitexture.Checked = False
            End If

            'Borderless
            If BorderlessWindow.ToString() = "True" Or BorderlessWindow.ToString() = "1" Then
                CheckBox_borderless.Checked = True
            Else
                CheckBox_borderless.Checked = False
            End If

            'Borderless
            If BorderlessWindow.ToString() = "True" Or BorderlessWindow.ToString() = "1" Then
                CheckBox_borderless.Checked = True
            Else
                CheckBox_borderless.Checked = False
            End If

            'FullScreen
            If FullScreen.ToString() = "True" Or FullScreen.ToString() = "1" Then
                CheckBox_fullscreen.Checked = True
            Else
                CheckBox_fullscreen.Checked = False
            End If

            'WideScreen
            If WideScreen.ToString() = "True" Or WideScreen.ToString() = "1" Then
                CheckBox_widescreen.Checked = True
            Else
                CheckBox_widescreen.Checked = False
            End If

            'WideBackground
            If WideBackground.ToString() = "True" Or WideBackground.ToString() = "1" Then
                CheckBox_widebg.Checked = True
            Else
                CheckBox_widebg.Checked = False
            End If

            'Stretch
            If Stretch.ToString() = "True" Or Stretch.ToString() = "1" Then
                CheckBox_stretch.Checked = True
            Else
                CheckBox_stretch.Checked = False
            End If

            'ShowFrameRate
            If ShowFrameRate.ToString() = "True" Or ShowFrameRate.ToString() = "1" Then
                CheckBox_showfrmerate.Checked = True
            Else
                CheckBox_showfrmerate.Checked = False
            End If

            'Throttle
            If Throttle.ToString() = "True" Or Throttle.ToString() = "1" Then
                CheckBox_throttle.Checked = True
            Else
                CheckBox_throttle.Checked = False
            End If

            'EmulateSound
            If EmulateSound.ToString() = "True" Or EmulateSound.ToString() = "1" Then
                CheckBox_emulatesound.Checked = True
            Else
                CheckBox_emulatesound.Checked = False
            End If

            'FlipStereo
            If FlipStereo.ToString() = "True" Or FlipStereo.ToString() = "1" Then
                CheckBox_flipstereo.Checked = True
            Else
                CheckBox_flipstereo.Checked = False
            End If

            'EmulateDSB
            If EmulateDSB.ToString() = "True" Or EmulateDSB.ToString() = "1" Then
                CheckBox_emuDSB.Checked = True
            Else
                CheckBox_emuDSB.Checked = False
            End If

            'LegacySoundDSP
            If LegacySoundDSP.ToString() = "True" Or LegacySoundDSP.ToString() = "1" Then
                CheckBox_legacyDSP.Checked = True
            Else
                CheckBox_legacyDSP.Checked = False
            End If

            'MusicVolume
            Label_Music.Text = MusicVolume.ToString()
            MusicBar.Value = CInt(MusicVolume.ToString())

            'MusicVolume
            Label_Sound.Text = SoundVolume.ToString()
            SoundBar.Value = CInt(SoundVolume.ToString())

            'Balance
            Label_Balance.Text = Balance.ToString()
            BalanceBar.Value = CInt(Balance.ToString())

            'RefreshRate
            Dim RR As String = RefreshRate.ToString
            If RR = "57.524160" Then
                CheckBox_TrueHz.Checked = True
            Else
                CheckBox_TrueHz.Checked = False
            End If
            Label_refreshrate.Text = RefreshRate.ToString()

            'true-ar
            If TrueAR.ToString() = "True" Or TrueAR.ToString() = "1" Then
                CheckBox_truear.Checked = True
            Else
                CheckBox_truear.Checked = False
            End If

            'PowerPCFrequency
            Dim PPC As String = PowerPCFrequency.ToString()
            If PPC = "0" Then
                PPC = "Auto"
            End If
            Label_PPC.Text = PPC
            PPC_Bar.Value = CInt(PowerPCFrequency.ToString())

            'Supersampling
            Label_SS.Text = Supersampling.ToString()
            SS_Bar.Value = CInt(Supersampling.ToString())

            'WindowsPosition
            Label_xPos.Text = WindowXPosition.ToString()
            Label_yPos.Text = WindowYPosition.ToString()

            'Resolution
            Label_xRes.Text = XResolution.ToString
            Label_yRes.Text = YResolution.ToString

            'HideCMD
            If HideCMD.ToString() = "True" Or HideCMD.ToString() = "1" Then
                CheckBox_hidecmd.Checked = True
            Else
                CheckBox_hidecmd.Checked = False
            End If
            'Debug("HideCMD" & HideCMD.ToString)

            'Dir
            Label_path.Text = Dir.ToString
            Roms_count(Dir.ToString)
            'Debug("Dir" & Dir.ToString)

            'Title
            TextBox_Title.Text = Title_SB.ToString

            'InputSystem
            ComboBox_input.Text = InputSystem.ToString
            'Crosshairs
            Select Case Crosshairs.ToString
                Case "0"
                    ComboBox_crosshair.Text = "Disable"
                Case "1"
                    ComboBox_crosshair.Text = "Player1"
                Case "2"
                    ComboBox_crosshair.Text = "Player2"
                Case "3"
                    ComboBox_crosshair.Text = "2Players"
            End Select

            'CrosshairStyle
            ComboBox_style.Text = CrosshairStyle.ToString

            'ForceFeedback
            If ForceFeedback.ToString() = "True" Or ForceFeedback.ToString() = "1" Then
                CheckBox18.Checked = True
            Else
                CheckBox18.Checked = False
            End If

            'Outputs
            If Outputs.ToString() = "True" Or Outputs.ToString() = "1" Then
                CheckBox_outputs.Checked = True
            Else
                CheckBox_outputs.Checked = False
            End If

            'Network
            If Network.ToString() = "True" Or Network.ToString() = "1" Then
                CheckBox_network.Checked = True
            Else
                CheckBox_network.Checked = False
            End If

            'SimulateNet
            If SimulateNet.ToString() = "True" Or SimulateNet.ToString() = "1" Then
                CheckBox_simnetwork.Checked = True
            Else
                CheckBox_simnetwork.Checked = False
            End If

            'PortIn
            TextBox_Portin.Text = PortIn.ToString

            'PortOut
            TextBox_Portout.Text = PortOut.ToString

            'AddressOut
            TextBox_Addressout.Text = AddressOut.ToString

            'DirectInputConstForceLeftMax
            DConstLeft.Text = DirectInputConstForceLeftMax.ToString

            'DirectInputConstForceRightMax
            DConstRight.Text = DirectInputConstForceRightMax.ToString

            'DirectInputSelfCenterMax
            DCenter.Text = DirectInputSelfCenterMax.ToString

            'DirectInputFrictionMax
            DFriction.Text = DirectInputFrictionMax.ToString

            'DirectInputVibrateMax
            DViblate.Text = DirectInputVibrateMax.ToString

            'XInputConstForceThreshold
            XThreshold.Text = XInputConstForceThreshold.ToString

            'XInputConstForceMax
            XConst.Text = XInputConstForceMax.ToString

            'XInputVibrateMax
            XViblate.Text = XInputVibrateMax.ToString

            'Resolution
            ComboBox_resolution.Text = XResolution.ToString & "x" & YResolution.ToString

            'Get Display Size 
            Dim i As Integer = 0

            For Each s In System.Windows.Forms.Screen.AllScreens
                'display device name
                ScreenN(i) = s.DeviceName
                'top left of display
                Bx(i) = s.Bounds.X
                By(i) = s.Bounds.Y
                'Display size
                Bw(i) = s.Bounds.Width
                Label_wScreenRes.Text = CStr(s.Bounds.Width)
                Bh(i) = s.Bounds.Height
                Label_hScreenRes.Text = CStr(s.Bounds.Height)
                'Debug(i & "::" & ScreenN(i))
                i += 1
            Next

            Label_wScreenRes.Text = CStr(Screen.GetBounds(Me).Width)
            Label_hScreenRes.Text = CStr(Screen.GetBounds(Me).Height)
        Else
            MessageBox.Show("supermodel.ini not found.")
            Me.Close()
        End If
    End Sub

    'Where is the debug window? i hate it
    Public Sub Debug(S As String)
        Debugtext.AppendText(S & vbCrLf)
    End Sub


    Private Sub Button_ScreenPos_Click(sender As Object, e As EventArgs) Handles Button_screenpos.Click
        If My.Application.OpenForms("PosResWindow") IsNot Nothing Then

        Else
            If Integer.Parse(Label_wScreenRes.Text) < Integer.Parse(Label_xRes.Text) Or Integer.Parse(Label_hScreenRes.Text) < Integer.Parse(Label_yRes.Text) Then
                Dim result As DialogResult = MessageBox.Show("It's bigger than the screen size. Are you sure Lunch PosResWindow?",
                                             "confirmation",
                                             MessageBoxButtons.YesNo,
                                             MessageBoxIcon.Exclamation,
                                            MessageBoxDefaultButton.Button1)
                If result = DialogResult.No Then
                    Exit Sub
                End If
                Dim f As PosResWindow = New PosResWindow()
                f.StartPosition = FormStartPosition.CenterScreen
                If f.ShowDialog(Me) = DialogResult.OK Then
                End If
                f.Dispose()
            Else
                Dim f As PosResWindow = New PosResWindow()
                f.StartPosition = FormStartPosition.CenterScreen
                If f.ShowDialog(Me) = DialogResult.OK Then
                End If
                f.Dispose()
            End If
        End If
    End Sub

    Private Sub DataGridView1_SelectCellChanged(sender As Object, e As DataGridViewCellEventArgs) Handles DataGridView1.CellEnter
        Roms = CStr(DataGridView1.CurrentRow.Cells(2).Value)
        Inputs = CStr(DataGridView1.CurrentRow.Cells(5).Value)
        PictureBox1.ImageLocation = "Snaps\" & Roms & ".jpg"
        Last_SelectedRow = DataGridView1.CurrentRow.Index
    End Sub

    Private Sub DataGridView1_SelectCellClick(sender As Object, e As DataGridViewCellEventArgs) Handles DataGridView1.CellClick
        Console.WriteLine(e.RowIndex)
        If e.RowIndex <> -1 Then
            Last_SelectedItem = CStr(DataGridView1.CurrentCell.Value)
            Last_SelectedRow = DataGridView1.CurrentRow.Index
        End If

        'Debug(Last_SelectedRow)
    End Sub

    Private Sub PPC_Bar_Scroll(sender As Object, e As EventArgs) Handles PPC_Bar.Scroll
        If PPC_Bar.Value = 0 Then
            Label_PPC.Text = "Auto"
        Else
            Label_PPC.Text = CStr(PPC_Bar.Value)
        End If
    End Sub

    Private Sub SS_Bar_Scroll(sender As Object, e As EventArgs) Handles SS_Bar.Scroll
        Label_SS.Text = CStr(SS_Bar.Value)
    End Sub

    Private Sub ComboBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox_resolution.SelectedIndexChanged
        Dim S_Select As String = CStr(ComboBox_resolution.SelectedItem)
        Dim S_Split() As String = Split(S_Select, "x")
        Label_xRes.Text = S_Split(0)
        Label_yRes.Text = S_Split(1)
    End Sub

    Private Sub Button_LoadRom_Click(sender As Object, e As EventArgs) Handles Button_loadrom.Click
        Load_Roms()
    End Sub
    Private Sub Load_Roms()
        If ShowFavoriteToolStripMenuItem.Text = "Show All" And Favorite_n = 0 Then
            MessageBox.Show("Boo", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Exit Sub
        End If
        Dim flag As Boolean = False
        If ComboBox_input.SelectedItem = "rawinput" And Inputs = "analog_gun1" Then
            If My.Application.OpenForms("Gun") IsNot Nothing Then

            Else
                Dim f As Gun = New Gun()
                'f.Label_ConstLeft.Text = DConstLeft.Text
                'f.ConstLeftBar.Value = CInt(DConstLeft.Text)
                'f.Label_ConstRight.Text = DConstRight.Text
                'f.ConsRightBar.Value = CInt(DConstRight.Text)
                'f.Label_Center.Text = DCenter.Text
                'f.SelfBar.Value = CInt(DCenter.Text)
                'f.Label_Friction.Text = DFriction.Text
                'f.FrictionBar.Value = CInt(DFriction.Text)
                'f.Label_Viblate.Text = DViblate.Text
                'f.ViblateBar.Value = CInt(DViblate.Text)
                RawInput_hook.Text = "Disabled"
                RawInput_Enabled = False
                If (f.ShowDialog(Me) = DialogResult.OK) Then
                End If
                If (f.DialogResult = DialogResult.OK) Then
                    WriteGunIni()
                ElseIf (f.DialogResult = DialogResult.Cancel) Then
                    flag = True
                ElseIf (f.DialogResult = DialogResult.Ignore) Then
                    flag = False
                End If
                f.Dispose()
            End If

        End If

        If flag = True Then
            Exit Sub
        End If



        WriteIni()
        Dim ffb As String = ""
        If CheckBox18.Checked = True Then
            ffb = " -outputs=win"
        End If
        Try
            Dim appPath As String = System.Windows.Forms.Application.StartupPath
            Dim startInfo As New ProcessStartInfo(appPath & "\Supermodel.exe ", " """ & Label_path.Text & "\" & Roms & ".zip""" & ffb) ')
            startInfo.CreateNoWindow = CheckBox_hidecmd.Checked
            startInfo.UseShellExecute = False
            Process.Start(startInfo)
            loading.Show()
        Catch ex As Exception
            MessageBox.Show(ex.Message.ToString, "Error",
MessageBoxButtons.OK,
MessageBoxIcon.Error)
        End Try
        'End If

    End Sub


    Private Sub AboutToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles AboutToolStripMenuItem.Click
        If My.Application.OpenForms("About") IsNot Nothing Then
        Else
            Dim f As About = New About()
            If (f.ShowDialog(Me) = DialogResult.OK) Then
            End If
            f.Dispose()
        End If
    End Sub

    Private Sub MusicBar_Scroll(sender As Object, e As EventArgs) Handles MusicBar.Scroll
        Label_Music.Text = CStr(MusicBar.Value)
    End Sub

    Private Sub BalanceBar_Scroll(sender As Object, e As EventArgs) Handles BalanceBar.Scroll
        Label_Balance.Text = CStr(BalanceBar.Value)
    End Sub

    Private Sub SoundBar_Scroll(sender As Object, e As EventArgs) Handles SoundBar.Scroll
        Label_Sound.Text = CStr(SoundBar.Value)
    End Sub

    Private Sub Button10_Click(sender As Object, e As EventArgs) Handles Button10.Click
        Dim appPath As String = System.Windows.Forms.Application.StartupPath
        Process.Start(appPath & "\Supermodel.exe ", " -config-inputs")
    End Sub

    Private Sub Button_Folder_Click(sender As Object, e As EventArgs) Handles Button_folder.Click
        Dim fbd As New FolderBrowserDialog
        fbd.Description = "Select Roms folder."
        fbd.RootFolder = Environment.SpecialFolder.Desktop
        fbd.SelectedPath = Label_path.Text
        fbd.ShowNewFolderButton = True
        If fbd.ShowDialog(Me) = DialogResult.OK Then
            Label_path.Text = fbd.SelectedPath
            Roms_count(fbd.SelectedPath)
        End If
    End Sub


    Private Sub Roms_count(Dir As String)
        Try
            If DT_Roms.Rows.Count > 0 Then
                DT_Roms.Rows.Clear()
            End If
            Dim Roms_count As Integer = 0
            Dim FileCount As Integer = Directory.GetFiles(Dir, "*.zip", SearchOption.TopDirectoryOnly).Length
            If FileCount > 0 Then
                Dim files As IEnumerable(Of String) = System.IO.Directory.EnumerateFiles(Dir, "*.zip", System.IO.SearchOption.TopDirectoryOnly)
                'ファイルを列挙する

                For Each f As String In files
                    Dim f_split() As String
                    f_split = Split(f, "\")
                    Dim fl As Integer = f_split.Length - 1
                    Dim f_replace As String = f_split(fl).Replace(".zip", "")
                    DT_Roms.Rows.Add(f_replace)
                Next

                For i As Integer = 0 To GameData.Rows.Count - 1
                    GameData.Rows(i).Item("A-E") = " "
                Next



                Dim Roms_bin As String

                For i As Integer = 0 To GameData.Rows.Count - 1
                    Roms_bin = GameData.Rows(i).Item("Roms").ToString
                    For j As Integer = 0 To FileCount - 1
                        If Roms_bin = DT_Roms.Rows(j).Item(0).ToString Then
                            GameData.Rows(i).Item("A-E") = "o"
                            Roms_count += 1
                            Exit For
                        End If
                    Next
                Next
            Else
                For i As Integer = 0 To GameData.Rows.Count - 1
                    GameData.Rows(i).Item("A-E") = " "
                Next
            End If
            DataGridView1.DataSource = GameData
            Label_Roms.Text = Roms_count & " rom(s) available."
            Label_listed.Text = GameData.Rows.Count & " game(s) listed." 'DataGridView1.CurrentRow.Index + 1 & " / " &
        Catch ex As Exception

        End Try

    End Sub
    Private Sub TextBox1and2_KeyPress(sender As Object, e As System.Windows.Forms.KeyPressEventArgs) Handles TextBox_Portin.KeyPress, TextBox_Portout.KeyPress
        If (e.KeyChar < "0"c OrElse "9"c < e.KeyChar) AndAlso e.KeyChar <> ControlChars.Back Then
            e.Handled = True
        End If
    End Sub

    Private Sub TextBox3_KeyPress(sender As Object, e As System.Windows.Forms.KeyPressEventArgs) Handles TextBox_Addressout.KeyPress
        If (e.KeyChar < "0"c OrElse "9"c < e.KeyChar) AndAlso e.KeyChar <> ControlChars.Back AndAlso e.KeyChar <> "."c Then
            e.Handled = True
        End If
    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        Panel_Video.Left = 612
        Panel_Video.Top = 336
        Panel_Sound.Left = 2000
        Panel_Sound.Top = 336
        Panel_Input.Left = 2000
        Panel_Input.Top = 336
        Panel_Network.Left = 2000
        Panel_Network.Top = 336
        Panel_ponmi.Left = 2000
        Panel_ponmi.Top = 336
    End Sub

    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click
        Panel_Video.Left = 2000
        Panel_Video.Top = 336
        Panel_Sound.Left = 612
        Panel_Sound.Top = 336
        Panel_Input.Left = 2000
        Panel_Input.Top = 336
        Panel_Network.Left = 2000
        Panel_Network.Top = 336
        Panel_ponmi.Left = 2000
        Panel_ponmi.Top = 336
    End Sub

    Private Sub Button6_Click(sender As Object, e As EventArgs) Handles Button6.Click
        Panel_Video.Left = 2000
        Panel_Video.Top = 336
        Panel_Sound.Left = 2000
        Panel_Sound.Top = 336
        Panel_Input.Left = 612
        Panel_Input.Top = 336
        Panel_Network.Left = 2000
        Panel_Network.Top = 336
        Panel_ponmi.Left = 2000
        Panel_ponmi.Top = 336
    End Sub

    Private Sub Button7_Click(sender As Object, e As EventArgs) Handles Button7.Click
        Panel_Video.Left = 2000
        Panel_Video.Top = 336
        Panel_Sound.Left = 2000
        Panel_Sound.Top = 336
        Panel_Input.Left = 2000
        Panel_Input.Top = 336
        Panel_Network.Left = 612
        Panel_Network.Top = 336
        Panel_ponmi.Left = 2000
        Panel_ponmi.Top = 336
    End Sub

    Private Sub Button11_Click(sender As Object, e As EventArgs)
        Dim result As DialogResult = MessageBox.Show("Do you want to overwrite the ini file?",
                                             "confirmation",
                                             MessageBoxButtons.OKCancel,
                                             MessageBoxIcon.Exclamation,
                                            MessageBoxDefaultButton.Button2)
        If result = DialogResult.OK Then
            WriteIni()
        End If
    End Sub

    Private Sub WriteGunIni()
        Dim iniFileName As New StringBuilder(300)
        iniFileName.Append("Config\Supermodel.ini")
        Dim Section As String = " Global "
        WritePrivateProfileString(Section, "InputAnalogJoyX", Label39.Text & "_XAXIS", iniFileName)
        WritePrivateProfileString(Section, "InputAnalogJoyY", Label39.Text & "_YAXIS", iniFileName)
        WritePrivateProfileString(Section, "InputAnalogJoyTrigger", Label39.Text & "_LEFT_BUTTON", iniFileName)
        WritePrivateProfileString(Section, "InputAnalogJoyEvent", Label39.Text & "_RIGHT_BUTTON", iniFileName)
        WritePrivateProfileString(Section, "InputAnalogJoyEvent2", Label39.Text & "_MIDDLE_BUTTON", iniFileName)
        WritePrivateProfileString(Section, "InputGunX", Label39.Text & "_XAXIS", iniFileName)
        WritePrivateProfileString(Section, "InputGunY", Label39.Text & "_YAXIS", iniFileName)
        WritePrivateProfileString(Section, "InputTrigger", Label39.Text & "_LEFT_BUTTON", iniFileName)
        WritePrivateProfileString(Section, "InputOffscreen", Label39.Text & "_RIGHT_BUTTON", iniFileName)
        WritePrivateProfileString(Section, "InputAnalogGunX", Label39.Text & "_XAXIS", iniFileName)
        WritePrivateProfileString(Section, "InputAnalogGunY", Label39.Text & "_YAXIS", iniFileName)
        WritePrivateProfileString(Section, "InputAnalogTriggerLeft", Label39.Text & "_LEFT_BUTTON", iniFileName)
        WritePrivateProfileString(Section, "InputAnalogTriggerRight", Label39.Text & "_RIGHT_BUTTON", iniFileName)

        WritePrivateProfileString(Section, "InputGunX2", Label40.Text & "_XAXIS", iniFileName)
        WritePrivateProfileString(Section, "InputGunY2", Label40.Text & "_YAXIS", iniFileName)
        WritePrivateProfileString(Section, "InputTrigger2", Label40.Text & "_LEFT_BUTTON", iniFileName)
        WritePrivateProfileString(Section, "InputOffscreen2", Label40.Text & "_RIGHT_BUTTON", iniFileName)
        WritePrivateProfileString(Section, "InputAnalogGunX2", Label40.Text & "_XAXIS", iniFileName)
        WritePrivateProfileString(Section, "InputAnalogGunY2", Label40.Text & "_YAXIS", iniFileName)
        WritePrivateProfileString(Section, "InputAnalogTriggerLeft2", Label40.Text & "_LEFT_BUTTON", iniFileName)
        WritePrivateProfileString(Section, "InputAnalogTriggerRight2", Label40.Text & "_RIGHT_BUTTON", iniFileName)
    End Sub


    Private Sub WriteIni()
        Dim iniFileName As New StringBuilder(300)
        iniFileName.Append("Config\Supermodel.ini")
        Dim Section As String = " Global "
        WritePrivateProfileString(Section, "RefreshRate", Label_refreshrate.Text, iniFileName)
        WritePrivateProfileString(Section, "XResolution", Label_xRes.Text, iniFileName)
        WritePrivateProfileString(Section, "YResolution", Label_yRes.Text, iniFileName)
        WritePrivateProfileString(Section, "WindowXPosition", Label_xPos.Text, iniFileName)
        WritePrivateProfileString(Section, "WindowYPosition", Label_yPos.Text, iniFileName)
        WritePrivateProfileString(Section, "BorderlessWindow", CheckBox_borderless.Checked.ToString, iniFileName)
        WritePrivateProfileString(Section, "New3DEngine", RadioButton_new3d.Checked.ToString, iniFileName)
        WritePrivateProfileString(Section, "QuadRendering", CheckBox_quadrender.Checked.ToString, iniFileName)
        WritePrivateProfileString(Section, "WideScreen", CheckBox_widescreen.Checked.ToString, iniFileName)
        WritePrivateProfileString(Section, "Stretch", CheckBox_stretch.Checked.ToString, iniFileName)
        WritePrivateProfileString(Section, "WideBackground", CheckBox_widebg.Checked.ToString, iniFileName)

        WritePrivateProfileString(Section, "Crosshairs", CStr(ComboBox_crosshair.SelectedIndex), iniFileName)

        WritePrivateProfileString(Section, "GPUMultiThreaded", CheckBox_gpumulti.Checked.ToString, iniFileName)
        WritePrivateProfileString(Section, "MultiThreaded", CheckBox_multishread.Checked.ToString, iniFileName)
        WritePrivateProfileString(Section, "MultiTexture", CheckBox_multitexture.Checked.ToString, iniFileName)
        WritePrivateProfileString(Section, "VSync", CheckBox_vsync.Checked.ToString, iniFileName)
        WritePrivateProfileString(Section, "FullScreen", CheckBox_fullscreen.Checked.ToString, iniFileName)
        WritePrivateProfileString(Section, "Throttle", CheckBox_throttle.Checked.ToString, iniFileName)
        WritePrivateProfileString(Section, "ShowFrameRate", CheckBox_showfrmerate.Checked.ToString, iniFileName)
        If Label_PPC.Text = "Auto" Then
            WritePrivateProfileString(Section, "PowerPCFrequency", "0", iniFileName)
        Else
            WritePrivateProfileString(Section, "PowerPCFrequency", Label_PPC.Text, iniFileName)
        End If
        If RadioButton_new3d.Checked = True Then
            WritePrivateProfileString(Section, "Supersampling", Label_SS.Text, iniFileName)
            WritePrivateProfileString(Section, "CRTcolors", ComboBox1.SelectedIndex.ToString, iniFileName)
        Else
            WritePrivateProfileString(Section, "Supersampling", "1", iniFileName)
            WritePrivateProfileString(Section, "CRTcolors", "0", iniFileName)
        End If

        WritePrivateProfileString(Section, "UpscaleMode ", ComboBox2.SelectedIndex.ToString, iniFileName)

        WritePrivateProfileString(Section, "EmulateSound", CheckBox_emulatesound.Checked.ToString, iniFileName)
        WritePrivateProfileString(Section, "EmulateDSB", CheckBox_emuDSB.Checked.ToString, iniFileName)
        WritePrivateProfileString(Section, "FlipStereo", CheckBox_flipstereo.Checked.ToString, iniFileName)
        WritePrivateProfileString(Section, "LegacySoundDSP", CheckBox_legacyDSP.Checked.ToString, iniFileName)
        WritePrivateProfileString(Section, "MusicVolume", Label_Music.Text, iniFileName)
        WritePrivateProfileString(Section, "SoundVolume", Label_Sound.Text, iniFileName)
        WritePrivateProfileString(Section, "Balance", Label_Balance.Text, iniFileName)

        WritePrivateProfileString(Section, "ForceFeedback", CheckBox18.Checked.ToString, iniFileName)
        WritePrivateProfileString(" Supermodel3 UI ", "Outputs", CheckBox_outputs.Checked.ToString, iniFileName)

        WritePrivateProfileString(Section, "Network", CheckBox_network.Checked.ToString, iniFileName)
        WritePrivateProfileString(Section, "SimulateNet", CheckBox_simnetwork.Checked.ToString, iniFileName)
        WritePrivateProfileString(Section, "PortIn", TextBox_Portin.Text, iniFileName)
        WritePrivateProfileString(Section, "PortOut", TextBox_Portout.Text, iniFileName)
        WritePrivateProfileString(Section, "AddressOut", TextBox_Addressout.Text, iniFileName)

        WritePrivateProfileString(Section, "InputSystem", CStr(ComboBox_input.SelectedItem), iniFileName)
        WritePrivateProfileString(" Supermodel3 UI ", "HideCMD", CheckBox_hidecmd.Checked.ToString, iniFileName)
        WritePrivateProfileString(" Supermodel3 UI ", "Dir", Label_path.Text, iniFileName)
        If TextBox_Title.Text = "" Then
            TextBox_Title.Text = "Supermodel - PonMi"
        End If
        WritePrivateProfileString(Section, "Title", TextBox_Title.Text, iniFileName)
        WritePrivateProfileString(Section, "true-ar", CheckBox_truear.Checked.ToString, iniFileName)

        WritePrivateProfileString(" Supermodel3 UI ", "Columns0Width", CStr(DataGridView1.Columns(0).Width), iniFileName)
        WritePrivateProfileString(" Supermodel3 UI ", "Columns1Width", CStr(DataGridView1.Columns(1).Width), iniFileName)
        WritePrivateProfileString(" Supermodel3 UI ", "Columns2Width", CStr(DataGridView1.Columns(2).Width), iniFileName)
        WritePrivateProfileString(" Supermodel3 UI ", "Columns3Width", CStr(DataGridView1.Columns(3).Width), iniFileName)
        WritePrivateProfileString(" Supermodel3 UI ", "Columns4Width", CStr(DataGridView1.Columns(4).Width), iniFileName)
        WritePrivateProfileString(" Supermodel3 UI ", "Columns5Width", CStr(DataGridView1.Columns(5).Width), iniFileName)

        WritePrivateProfileString(" Supermodel3 UI ", "Columns0Sort", CStr(C0_Sort_F), iniFileName)
        WritePrivateProfileString(" Supermodel3 UI ", "Columns1Sort", CStr(C1_Sort_F), iniFileName)
        WritePrivateProfileString(" Supermodel3 UI ", "Columns2Sort", CStr(C2_Sort_F), iniFileName)
        WritePrivateProfileString(" Supermodel3 UI ", "Columns3Sort", CStr(C3_Sort_F), iniFileName)
        WritePrivateProfileString(" Supermodel3 UI ", "Columns4Sort", CStr(C4_Sort_F), iniFileName)
        WritePrivateProfileString(" Supermodel3 UI ", "Columns5Sort", CStr(C5_Sort_F), iniFileName)

        WritePrivateProfileString(" Supermodel3 UI ", "LastSort", CStr(Last_Sort), iniFileName)
        WritePrivateProfileString(" Supermodel3 UI ", "LastSelectedRow", CStr(Last_SelectedRow), iniFileName)
        WritePrivateProfileString(" Supermodel3 UI ", "FontSize", CStr(FontSize_bin), iniFileName)
        WritePrivateProfileString(" Supermodel3 UI ", "Resolution_index", CStr(ComboBox_resolution.SelectedIndex), iniFileName)
        WritePrivateProfileString(" Supermodel3 UI ", "BackColor_R", CStr(Bgcolor_R), iniFileName)
        WritePrivateProfileString(" Supermodel3 UI ", "BackColor_G", CStr(Bgcolor_G), iniFileName)
        WritePrivateProfileString(" Supermodel3 UI ", "BackColor_B", CStr(Bgcolor_B), iniFileName)
        WritePrivateProfileString(" Supermodel3 UI ", "ForeColor", Forecolor_s, iniFileName)
        WritePrivateProfileString(" Supermodel3 UI ", "Scanline", CStr(Scanline_Enabled), iniFileName)
        WritePrivateProfileString(" Supermodel3 UI ", "Gamepad", CStr(Surround1.Enabled), iniFileName)
        WritePrivateProfileString(" Supermodel3 UI ", "Opacity", CStr(Opacity_D), iniFileName)
        WritePrivateProfileString(" Supermodel3 UI ", "SS", CheckBox_ss.Checked.ToString, iniFileName)
        WritePrivateProfileString(" Supermodel3 UI ", "Favorite", ShowFavoriteToolStripMenuItem.Text.ToString, iniFileName)

        WritePrivateProfileString(Section, "DirectInputConstForceLeftMax", DConstLeft.Text, iniFileName)
        WritePrivateProfileString(Section, "DirectInputConstForceRightMax", DConstRight.Text, iniFileName)
        WritePrivateProfileString(Section, "DirectInputSelfCenterMax", DCenter.Text, iniFileName)
        WritePrivateProfileString(Section, "DirectInputFrictionMax", DFriction.Text, iniFileName)
        WritePrivateProfileString(Section, "DirectInputVibrateMax", DViblate.Text, iniFileName)

        WritePrivateProfileString(Section, "XInputConstForceThreshold", XThreshold.Text, iniFileName)
        WritePrivateProfileString(Section, "XInputConstForceMax", XConst.Text, iniFileName)
        WritePrivateProfileString(Section, "XInputVibrateMax", XViblate.Text, iniFileName)

        WritePrivateProfileString(Section, "CrosshairStyle", CStr(ComboBox_style.SelectedItem), iniFileName)
    End Sub

    Private Sub Me_Closing(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles MyBase.Closing
        If IgnoreClose = False Then
            Try
                WriteIni()
            Catch

            End Try
        End If
    End Sub

    Private Sub Button8_Click(sender As Object, e As EventArgs) Handles Button8.Click
        If My.Application.OpenForms("DinputForce") IsNot Nothing Then

        Else
            Dim f As DinputForce = New DinputForce()
            f.Label_ConstLeft.Text = DConstLeft.Text
            f.ConstLeftBar.Value = CInt(DConstLeft.Text)
            f.Label_ConstRight.Text = DConstRight.Text
            f.ConsRightBar.Value = CInt(DConstRight.Text)
            f.Label_Center.Text = DCenter.Text
            f.SelfBar.Value = CInt(DCenter.Text)
            f.Label_Friction.Text = DFriction.Text
            f.FrictionBar.Value = CInt(DFriction.Text)
            f.Label_Viblate.Text = DViblate.Text
            f.ViblateBar.Value = CInt(DViblate.Text)
            If (f.ShowDialog(Me) = DialogResult.OK) Then
            End If
            f.Dispose()
        End If
    End Sub

    Private Sub Button9_Click(sender As Object, e As EventArgs) Handles Button9.Click
        If My.Application.OpenForms("XinputForce") IsNot Nothing Then

        Else
            Dim f As XinputForce = New XinputForce()
            f.Label_XThreshold.Text = XThreshold.Text
            f.ThresholdBar.Value = CInt(XThreshold.Text)
            f.Label_XConstMax.Text = XConst.Text
            f.ConstMaxBar.Value = CInt(XConst.Text)
            f.Label_XViblate.Text = XViblate.Text
            f.ViblateBar.Value = CInt(XViblate.Text)

            If (f.ShowDialog(Me) = DialogResult.OK) Then
            End If
            f.Dispose()
        End If
    End Sub
    Private Sub DataGridView1_HeaderClick(sender As Object, e As DataGridViewCellMouseEventArgs) Handles DataGridView1.CellMouseClick
        If e.RowIndex < 0 Then
            Dim N As Integer = e.ColumnIndex
            Last_Sort = N
            Select Case N
                Case 0
                    If C0_Sort_F = False Then
                        DataGridView1.Sort(DataGridView1.Columns(0), System.ComponentModel.ListSortDirection.Ascending)
                        GameData.DefaultView.Sort = "Games ASC"
                    Else
                        DataGridView1.Sort(DataGridView1.Columns(0), System.ComponentModel.ListSortDirection.Descending)
                        GameData.DefaultView.Sort = "Games DESC"
                    End If
                    C0_Sort_F = Not C0_Sort_F
                    Last_Sort = 0
                Case 1
                    If C1_Sort_F = False Then
                        DataGridView1.Sort(DataGridView1.Columns(1), System.ComponentModel.ListSortDirection.Ascending)
                        GameData.DefaultView.Sort = "Version ASC"
                    Else
                        DataGridView1.Sort(DataGridView1.Columns(1), System.ComponentModel.ListSortDirection.Descending)
                        GameData.DefaultView.Sort = "Version DESC"
                    End If
                    C1_Sort_F = Not C1_Sort_F
                    Last_Sort = 1
                Case 2
                    If C2_Sort_F = False Then
                        DataGridView1.Sort(DataGridView1.Columns(2), System.ComponentModel.ListSortDirection.Ascending)
                        GameData.DefaultView.Sort = "Roms ASC"
                    Else
                        DataGridView1.Sort(DataGridView1.Columns(2), System.ComponentModel.ListSortDirection.Descending)
                        GameData.DefaultView.Sort = "Roms DESC"
                    End If
                    C2_Sort_F = Not C2_Sort_F
                    Last_Sort = 2
                Case 3
                    If C3_Sort_F = False Then
                        DataGridView1.Sort(DataGridView1.Columns(3), System.ComponentModel.ListSortDirection.Ascending)
                        GameData.DefaultView.Sort = "Step ASC"
                    Else
                        DataGridView1.Sort(DataGridView1.Columns(3), System.ComponentModel.ListSortDirection.Descending)
                        GameData.DefaultView.Sort = "Step DESC"
                    End If
                    Last_Sort = 3
                    C3_Sort_F = Not C3_Sort_F
                Case 4
                    If C4_Sort_F = False Then
                        DataGridView1.Sort(DataGridView1.Columns(4), System.ComponentModel.ListSortDirection.Ascending)
                        GameData.DefaultView.Sort = "A-E ASC"
                    Else
                        DataGridView1.Sort(DataGridView1.Columns(4), System.ComponentModel.ListSortDirection.Descending)
                        GameData.DefaultView.Sort = "A-E DESC"
                    End If
                    Last_Sort = 4
                    C4_Sort_F = Not C4_Sort_F
                Case 5
                    If C5_Sort_F = False Then
                        DataGridView1.Sort(DataGridView1.Columns(5), System.ComponentModel.ListSortDirection.Ascending)
                        GameData.DefaultView.Sort = "Inputs ASC"
                    Else
                        DataGridView1.Sort(DataGridView1.Columns(5), System.ComponentModel.ListSortDirection.Descending)
                        GameData.DefaultView.Sort = "Inputs DESC"
                    End If
                    Last_Sort = 5
                    C5_Sort_F = Not C5_Sort_F
                Case Else
            End Select
            UpdateDataGridView()
        End If

    End Sub


    Private Sub DataGridView1_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles DataGridView1.CellContentDoubleClick
        'Console.WriteLine(e.RowIndex)
        If e.RowIndex >= 0 Then
            Roms = CStr(DataGridView1.CurrentRow.Cells(2).Value)
            Inputs = CStr(DataGridView1.CurrentRow.Cells(5).Value)
            PictureBox1.ImageLocation = "Snaps\" & Roms & ".jpg"
            Load_Roms()
        End If
    End Sub

    Private Sub CheckBox_TrueHz_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox_TrueHz.CheckedChanged
        If CheckBox_TrueHz.Checked = False Then
            Label_refreshrate.Text = "60"
        Else
            Label_refreshrate.Text = "57.524160"
        End If
    End Sub

    Private Sub Get_Local_IPAddress(sender As Object, e As EventArgs) Handles Button_Get_Local_IPAddress.Click
        Dim hostname As String = Dns.GetHostName()
        Dim adrList As IPAddress() = Dns.GetHostAddresses(hostname)
        Dim adrLength = adrList.Count - 1
        Label_Local_IPaddress.Text = adrList(adrLength).ToString()
    End Sub

    Private Function Get_Global_IPaddress() As String
        Try
            Dim cmd As New System.Diagnostics.Process()
            cmd.StartInfo.FileName = System.Environment.GetEnvironmentVariable("ComSpec")
            cmd.StartInfo.UseShellExecute = False
            cmd.StartInfo.RedirectStandardOutput = True
            cmd.StartInfo.RedirectStandardInput = False
            cmd.StartInfo.CreateNoWindow = True
            cmd.StartInfo.Arguments = "/c curl inet-ip.info/ip"
            cmd.Start()
            Dim results As String = cmd.StandardOutput.ReadToEnd()
            cmd.WaitForExit()
            cmd.Close()
            Return results '.Trim() ※1
        Catch ex As Exception
            Return String.Empty
        End Try
    End Function

    Private Sub Header0_Click(sender As Object, e As EventArgs) Handles Header0.Click
        If C0_Sort_F = False Then
            DataGridView1.Sort(DataGridView1.Columns(0), System.ComponentModel.ListSortDirection.Ascending)
            GameData.DefaultView.Sort = "Games ASC"
            C0_Sort_F = True
        Else
            DataGridView1.Sort(DataGridView1.Columns(0), System.ComponentModel.ListSortDirection.Descending)
            GameData.DefaultView.Sort = "Games DESC"
            C0_Sort_F = False
        End If
        Last_Sort = 0
    End Sub
    Private Sub Header1_Click(sender As Object, e As EventArgs) Handles Header1.Click
        DataGridView1.ClearSelection()
        If C1_Sort_F = False Then
            DataGridView1.Sort(DataGridView1.Columns(1), System.ComponentModel.ListSortDirection.Ascending)
            GameData.DefaultView.Sort = "Version ASC"
            C1_Sort_F = True
        Else
            DataGridView1.Sort(DataGridView1.Columns(1), System.ComponentModel.ListSortDirection.Descending)
            GameData.DefaultView.Sort = "Version DESC"
            C1_Sort_F = False
        End If
        Last_Sort = 1

    End Sub
    Private Sub Header2_Click(sender As Object, e As EventArgs) Handles Header2.Click
        If C2_Sort_F = False Then
            DataGridView1.Sort(DataGridView1.Columns(2), System.ComponentModel.ListSortDirection.Ascending)
            GameData.DefaultView.Sort = "Roms ASC"
            C2_Sort_F = True
        Else
            DataGridView1.Sort(DataGridView1.Columns(2), System.ComponentModel.ListSortDirection.Descending)
            GameData.DefaultView.Sort = "Roms DESC"
            C2_Sort_F = False
        End If
        Last_Sort = 2
    End Sub
    Private Sub Header3_Click(sender As Object, e As EventArgs) Handles Header3.Click
        If C3_Sort_F = False Then
            DataGridView1.Sort(DataGridView1.Columns(3), System.ComponentModel.ListSortDirection.Ascending)
            GameData.DefaultView.Sort = "Step ASC"
            C3_Sort_F = True
        Else
            DataGridView1.Sort(DataGridView1.Columns(3), System.ComponentModel.ListSortDirection.Descending)
            GameData.DefaultView.Sort = "Step DESC"
            C3_Sort_F = False
        End If
        Last_Sort = 3
    End Sub

    Private Sub Header4_Click(sender As Object, e As EventArgs) Handles Header4.Click
        If C4_Sort_F = False Then
            DataGridView1.Sort(DataGridView1.Columns(4), System.ComponentModel.ListSortDirection.Ascending)
            GameData.DefaultView.Sort = "A-E ASC"
            C4_Sort_F = True
        Else
            DataGridView1.Sort(DataGridView1.Columns(4), System.ComponentModel.ListSortDirection.Descending)
            GameData.DefaultView.Sort = "A-E DESC"
            C4_Sort_F = False
        End If
        Last_Sort = 4
    End Sub

    Private Sub Header5_Click(sender As Object, e As EventArgs) Handles Header5.Click
        If C5_Sort_F = False Then
            DataGridView1.Sort(DataGridView1.Columns(5), System.ComponentModel.ListSortDirection.Ascending)
            GameData.DefaultView.Sort = "Inputs ASC"
            C5_Sort_F = True
        Else
            DataGridView1.Sort(DataGridView1.Columns(5), System.ComponentModel.ListSortDirection.Descending)
            GameData.DefaultView.Sort = "Inputs DESC"
            C5_Sort_F = False
        End If
        Last_Sort = 5
    End Sub

    Private Sub ToolStripMenuItem8_Click(sender As Object, e As EventArgs) Handles ToolStripMenuItem8.Click
        FontSize_bin = 8
        GetAllControls(Me, Integer.Parse(CStr(FontSize_bin)))
    End Sub

    Private Sub ToolStripMenuItem10_Click(sender As Object, e As EventArgs) Handles ToolStripMenuItem10.Click
        FontSize_bin = 10
        GetAllControls(Me, Integer.Parse(CStr(FontSize_bin)))
    End Sub



    Private Sub GetAllControls(ByVal control As Control, size As Integer)
        If control.HasChildren Then
            For Each childControl As Control In control.Controls
                GetAllControls(childControl, size)
                childControl.Font = New Font("Arial", size, FontStyle.Regular)
            Next childControl
        End If
    End Sub

    Private Sub BGColorToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ChooseToolStripMenuItem.Click
        Dim cd As New ColorDialog()
        cd.Color = Me.BackColor
        cd.AllowFullOpen = True
        cd.SolidColorOnly = False
        If cd.ShowDialog() = DialogResult.OK Then
            Me.BackColor = cd.Color
            Label_path.BackColor = cd.Color
            Bgcolor_R = CInt(cd.Color.R.ToString)
            Bgcolor_G = CInt(cd.Color.G.ToString)
            Bgcolor_B = CInt(cd.Color.B.ToString)
        End If
    End Sub

    Private Sub WhiteToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles WhiteToolStripMenuItem.Click
        Dim c As Object
        For Each c In Controls
            If TypeOf c Is IButtonControl Or TypeOf c Is MenuStrip Or TypeOf c Is TextBoxBase Then
                c.ForeColor = Color.Black
            Else
                c.ForeColor = Color.White
            End If
        Next
        For Each c In Panel_Video.Controls
            c.ForeColor = Color.White
            If TypeOf c Is IButtonControl Then
                c.ForeColor = Color.Black
            Else
                c.ForeColor = Color.White
            End If
            If c.name = "ComboBox1" Or c.name = "ComboBox2" Then
                c.ForeColor = Color.Black
            End If
        Next
        For Each c In Panel_Sound.Controls
            c.ForeColor = Color.White
        Next
        For Each c In Panel_Input.Controls
            c.ForeColor = Color.White
            If TypeOf c Is ButtonBase Then
                c.ForeColor = Color.Black
            End If
            If c.name = "CheckBox18" Or c.name = "CheckBox_outputs" Then
                c.ForeColor = Color.White
            End If

            If c.name = "ComboBox_input" Then
                c.Forecolor = Color.Black
            End If
        Next
        For Each c In Panel_Network.Controls
            c.ForeColor = Color.White
            If TypeOf c Is IButtonControl Then
                c.ForeColor = Color.Black
            Else
                c.ForeColor = Color.White
            End If
        Next
        For Each c In Panel_ponmi.Controls
            c.ForeColor = Color.White
            If TypeOf c Is IButtonControl Then
                c.ForeColor = Color.Black
            Else
                c.ForeColor = Color.White
            End If
        Next
        Pub_Forecolor_s = Color.White
        Forecolor_s = "White"
    End Sub

    Private Sub BlackToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles BlackToolStripMenuItem.Click
        Dim c As Object
        For Each c In Me.Controls
            If TypeOf c Is DataGridView Then
                c.ForeColor = Color.White
            Else
                c.ForeColor = Color.Black
            End If
        Next
        For Each c In Panel_Video.Controls
            c.ForeColor = Color.Black
        Next
        For Each c In Panel_Sound.Controls
            c.ForeColor = Color.Black
        Next
        For Each c In Panel_Input.Controls
            c.ForeColor = Color.Black
        Next
        For Each c In Panel_Network.Controls
            c.ForeColor = Color.Black
            If TypeOf c Is TextBoxBase Then
                c.ForeColor = Color.White
            End If
        Next
        For Each c In Panel_ponmi.Controls
            c.ForeColor = Color.Black
        Next
        Pub_Forecolor_s = Color.Black
        Forecolor_s = "Black"
    End Sub

    Private Sub DefoultToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles DefoultToolStripMenuItem.Click
        Bgcolor_R = 147
        Bgcolor_G = 0
        Bgcolor_B = 80
        Me.BackColor = Color.FromArgb(255, Bgcolor_R, Bgcolor_G, Bgcolor_B)
        Label_path.BackColor = Color.FromArgb(255, Bgcolor_R, Bgcolor_G, Bgcolor_B)
        WhiteToolStripMenuItem.PerformClick()
    End Sub

    Private Sub Button_Ponmi_Click(sender As Object, e As EventArgs) Handles Button_Ponmi.Click
        Panel_Video.Left = 2000
        Panel_Video.Top = 336
        Panel_Sound.Left = 2000
        Panel_Sound.Top = 336
        Panel_Input.Left = 2000
        Panel_Input.Top = 336
        Panel_Network.Left = 2000
        Panel_Network.Top = 336
        Panel_ponmi.Left = 612
        Panel_ponmi.Top = 336
    End Sub

    Private Sub LinkLabel1_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles LinkLabel1.LinkClicked
        System.Diagnostics.Process.Start("https://github.com/BackPonBeauty/Supermodel3-PonMi?tab=readme-ov-file#ponmi")
    End Sub

    Private Sub Surround1_Tick(sender As Object, e As EventArgs) Handles Surround1.Tick
        SurroundingSub1()
    End Sub
    Private Sub Surround2_Tick(sender As Object, e As EventArgs) Handles Surround2.Tick
        SurroundingSub2()
    End Sub

    Sub Sounds(Rnd As String, leng As Integer)
        brdy = 1
        Timer3.Enabled = False
        Dim cmd As String
        cmd = "stop " + mysound
        mciSendString(cmd, Nothing, 0, IntPtr.Zero)
        cmd = "close " + mysound
        mciSendString(cmd, Nothing, 0, IntPtr.Zero)
        'Dim Rnd As String = "up"
        'Dim leng As Integer = 200
        Dim sond As String = "sound\" & Rnd & ".mp3"
        Dim fileNames As String = sond
        cmd = "open """ + fileNames + """ type mpegvideo alias " + mysound

        If mciSendString(cmd, Nothing, 0, IntPtr.Zero) <> 0 Then
            Return
        End If
        cmd = "play " + mysound
        mciSendString(cmd, Nothing, 0, IntPtr.Zero)
        Timer3.Interval = leng
        Timer3.Enabled = True
    End Sub


    Dim Center_i As Integer = 0
    Dim lever32 As Integer
    Dim Button32_1 As Integer
    Dim Button32_2 As Integer
    Dim Button32_3 As Integer
    Dim rec_i As Integer = 0
    Public Sub SurroundingSub1()
        'Me.SuspendLayout()
        'rec_i += 1
        Dim controller = New SharpDX.XInput.Controller(SharpDX.XInput.UserIndex.One)
        If controller.IsConnected Then
            Dim state = controller.GetState()
            Dim a = state.Gamepad.Buttons
            If Rec Then
                Dim OneItem As New ListInfo
                OneItem.Lever_s = a

                List_Rec.Add(OneItem)
                Exit Sub
            End If

            Dim n As String = Convert.ToString((a), 2).PadLeft(16, CChar("0"))

            Dim lever As String = Strings.Right(n, 4)
            Dim n1 = "0"
            Dim n2 = "0"
            Dim n3 = "0"
            Dim n4 = "0"
            'Dim x = state.Gamepad.LeftThumbX
            'Dim y = state.Gamepad.LeftThumbY

            'If (y <> Center_i Or x <> Center_i) And lever = "0000" Then
            '    If y > Center_i * -1 Then
            '        n1 = "1"
            '    End If
            '    If y < Center_i Then
            '        n2 = "1"
            '    End If
            '    If x < Center_i Then
            '        n3 = "1"
            '    End If
            '    If x > Center_i * -1 Then
            '        n4 = "1"
            '    End If
            '    lever = n4 & n3 & n2 & n1
            'End If

            joybox1.Image = CType(My.Resources.ResourceManager.GetObject("_" & lever.ToString), Image)

            If lever = "0001" Then
                If brdy = 0 Then
                    Sounds("up", 200)
                End If
            End If
            If lever = "0010" Then
                If brdy = 0 Then
                    Sounds("down", 300)
                End If
            End If
            If lever = "0100" Then
                If brdy = 0 Then
                    Sounds("left", 300)
                End If
            End If
            If lever = "1000" Then
                If brdy = 0 Then
                    Sounds("right", 250)
                End If
            End If




            'Shift
            Dim ss As String = n.Substring(3, 1)
            If ss = "0" Then
                shiftbox1.Image = Nothing

            Else
                shiftbox1.Image = My.Resources.sd
                If brdy = 0 Then
                    Sounds("chu", 300)
                End If
            End If


            'Beat
            Dim bb As String = n.Substring(1, 1)
            If bb = "0" Then
                beatbox1.Image = Nothing

            Else
                beatbox1.Image = My.Resources.bd
                If brdy = 0 Then
                    Sounds("chu", 300)
                End If
            End If

            'Charge
            Dim cc As String = n.Substring(0, 1)
            If cc = "0" Then
                chargebox1.Image = Nothing

            Else
                chargebox1.Image = My.Resources.cd
                If brdy = 0 Then
                    Sounds("nya", 200)
                End If
            End If


            'Jump
            Dim jj As String = n.Substring(6, 1)
            If jj = "0" Then
                jumpbox1.Image = Nothing

            Else
                jumpbox1.Image = My.Resources.jd
                If brdy = 0 Then
                    Sounds("one", 200)
                End If
            End If


            'Start
            Dim st As String = n.Substring(11, 1)
            If st = "0" Then
                Startbox1.Image = Nothing

            Else
                Startbox1.Image = My.Resources.cd
                If brdy = 0 Then
                    Sounds("suzuki", 400)
                End If
            End If

            'Home
            Dim hm As String = n.Substring(10, 1)
            If hm = "0" Then
                Homebox1.Image = Nothing

            Else
                Homebox1.Image = My.Resources.cd
                If brdy = 0 Then
                    Sounds("tanaka", 400)
                End If
            End If

            'B
            Dim b As String = n.Substring(2, 1)
            If b = "0" Then
                Bbox1.Image = Nothing

            Else
                Bbox1.Image = My.Resources.cd
                If brdy = 0 Then
                    Sounds("nya", 200)
                End If
            End If

            'L_Sholder
            Dim ls As String = n.Substring(7, 1)
            If ls = "0" Then
                LSbox1.Image = Nothing

            Else
                LSbox1.Image = My.Resources.cd
                If brdy = 0 Then
                    Sounds("one", 200)
                End If
            End If

            'RSB
            Dim rsb As String = n.Substring(8, 1)
            If rsb = "0" Then
                Rbox1.Image = Nothing

            Else
                Rbox1.Image = My.Resources.cd
                If brdy = 0 Then
                    Sounds("suzuki", 400)
                End If
            End If

            'LSB
            Dim lsb As String = n.Substring(9, 1)
            If lsb = "0" Then
                Lbox1.Image = Nothing

            Else
                Lbox1.Image = My.Resources.cd
                If brdy = 0 Then
                    Sounds("tanaka", 400)
                End If
            End If
            'Dim rrr As String = n.Substring(9, 1)
            'If rrr = "1" And Rec = False Then
            '    Me.WindowState = FormWindowState.Minimized
            '    List_Rec.Clear()
            '    Button13.Text = "REC"
            '    Button13.ForeColor = Color.Red
            'End If
            'Dim sss As String = n.Substring(8, 1)
            'If sss = "1" And Rec = True Then
            '    Rec = False
            '    Button13.Text = "STOP"
            '    Button13.ForeColor = Color.Blue
            '    Me.WindowState = FormWindowState.Normal

            'End If

        Else
            joybox1.Image = Nothing
        End If
        'Me.ResumeLayout(True)
    End Sub

    Private Sub SurroundingSub2()
        Me.SuspendLayout()
        Dim controller = New SharpDX.XInput.Controller(SharpDX.XInput.UserIndex.Two)
        If controller.IsConnected Then
            Dim state = controller.GetState()
            Dim a = state.Gamepad.Buttons

            Dim n As String = Convert.ToString((a), 2).PadLeft(16, CChar("0"))

            Dim lever As String = Strings.Right(n, 4)
            Dim n1 = "0"
            Dim n2 = "0"
            Dim n3 = "0"
            Dim n4 = "0"
            Dim x = state.Gamepad.LeftThumbX
            Dim y = state.Gamepad.LeftThumbY

            If (y <> Center_i Or x <> Center_i) And lever = "0000" Then
                If y > Center_i Then
                    n1 = "1"
                End If
                If y < Center_i Then
                    n2 = "1"
                End If
                If x < Center_i Then
                    n3 = "1"
                End If
                If x > Center_i Then
                    n4 = "1"
                End If
                lever = n4 & n3 & n2 & n1
            End If

            joybox2.Image = CType(My.Resources.ResourceManager.GetObject("_" & lever.ToString), Image)

            'Shift
            Dim ss As String = n.Substring(3, 1)
            If ss = "0" Then
                shiftbox2.Image = Nothing

            Else
                shiftbox2.Image = My.Resources.sd

            End If


            'Beat
            Dim bb As String = n.Substring(1, 1)
            If bb = "0" Then
                beatbox2.Image = Nothing

            Else
                beatbox2.Image = My.Resources.bd

            End If

            'Charge
            Dim cc As String = n.Substring(0, 1)
            If cc = "0" Then
                chargebox2.Image = Nothing

            Else
                chargebox2.Image = My.Resources.cd

            End If


            'Jump
            Dim jj As String = n.Substring(6, 1)
            If jj = "0" Then
                jumpbox2.Image = Nothing

            Else
                jumpbox2.Image = My.Resources.jd

            End If


            'Start
            Dim st As String = n.Substring(11, 1)
            If st = "0" Then
                Startbox2.Image = Nothing

            Else
                Startbox2.Image = My.Resources.cd

            End If

            'Home
            Dim hm As String = n.Substring(10, 1)
            If hm = "0" Then
                Homebox2.Image = Nothing

            Else
                Homebox2.Image = My.Resources.cd

            End If

            'B
            Dim b As String = n.Substring(2, 1)
            If b = "0" Then
                Bbox2.Image = Nothing

            Else
                Bbox2.Image = My.Resources.cd

            End If

            'L_Sholder
            Dim ls As String = n.Substring(7, 1)
            If ls = "0" Then
                LSbox2.Image = Nothing

            Else
                LSbox2.Image = My.Resources.cd

            End If

            'RSB
            Dim rsb As String = n.Substring(8, 1)
            If rsb = "0" Then
                Rbox2.Image = Nothing

            Else
                Rbox2.Image = My.Resources.cd
            End If

            'LSB
            Dim lsb As String = n.Substring(9, 1)
            If lsb = "0" Then
                Lbox2.Image = Nothing

            Else
                Lbox2.Image = My.Resources.cd
            End If

        Else
            joybox2.Image = Nothing
        End If
        Me.ResumeLayout(True)
    End Sub

    'Private Function TentoTwo(ByVal value As String) As String
    '    If CBool(Math.Floor(CInt(value) / 2)) Then
    '        Return (CStr(Math.Floor(CInt(value) / 2))) + CStr(CInt(value) Mod 2)
    '    End If
    '    Return CStr(CInt(value) Mod 2)
    'End Function

    Private Sub ControlerX4_Click(sender As Object, e As EventArgs) Handles Button_X.Click
        If XTimer_F = False Then
            'x_timer.Change(0, interval)
            Surround1.Enabled = True
            Button_X.Text = "Enabled"
            XTimer_F = True
            Panel7.Left = -240
        Else
            'x_timer.Change(Timeout.Infinite, Timeout.Infinite)
            Surround1.Enabled = False
            Button_X.Text = "Disabled"
            joybox1.Image = Nothing
            XTimer_F = False
        End If
    End Sub


    Public scanline_type As String = "PICTURE" 'Or "PICTURE"
    Dim control_F As Boolean = False
    Sub KeybordHooker1_KeyDown(sender As Object, e As KeyBoardHookerEventArgs) Handles KeyboardHooker1.KeyDown1
        Dim Tabb As String = CStr(e.vkCode)
        Label2.Text = Tabb
        If Tabb = "162" Then
            control_F = True
        End If

        If control_F Then
            If Tabb = "73" Then
                If Front_F = False Then
                    Front_F = True
                    Me.BringToFront()
                Else
                    Front_F = False
                    Me.SendToBack()
                End If

            End If
            If Tabb = "83" Then
                'Debug(Process.GetProcessesByName("Supermodel").Count)
                'If Process.GetProcessesByName("Supermodel").Count <> 0 Then
                If ScanLine_F = False Then
                    If scanline_type = "PICTURE" Then
                        scanline_type = "LINE1"
                    ElseIf scanline_type = "LINE1" Then
                        scanline_type = "LINE2"
                    ElseIf scanline_type = "LINE2" Then
                        scanline_type = "PICTURE"
                        ScanLine_F = True
                    End If
                    ScanLine.Width = Integer.Parse(Label_xRes.Text.ToString)
                    ScanLine.Height = Integer.Parse(Label_yRes.Text.ToString)
                    ScanLine.Top = Integer.Parse(Label_yPos.Text.ToString)
                    ScanLine.Left = Integer.Parse(Label_xPos.Text.ToString)
                    ScanLine.PictureBox1.Top = 0
                    ScanLine.PictureBox1.Left = 0
                    ScanLine.PictureBox1.Width = Integer.Parse(Label_xRes.Text.ToString)
                    ScanLine.PictureBox1.Height = Integer.Parse(Label_yRes.Text.ToString)

                    ScanLine.Top = Integer.Parse(Label_yPos.Text.ToString)
                    ScanLine.Left = Integer.Parse(Label_xPos.Text.ToString)
                    ScanLine.Draw_Scanline(scanline_type)
                    ScanLine.Show()
                Else
                    ScanLine_F = False
                    ScanLine.Close()
                    ScanLine.Dispose()
                End If

            End If

            If Tabb = "79" Then
                If Opacity_D > 1 Then
                    Opacity_D -= 1
                    ScanLine.Opacity -= 0.1
                End If
            End If
            If Tabb = "80" Then
                If Opacity_D < 10 Then
                    Opacity_D += 1
                    ScanLine.Opacity += 0.1
                End If
            End If
        End If
    End Sub

    Sub KeybordHooker1_Keyup(sender As Object, e As KeyBoardHookerEventArgs) Handles KeyboardHooker1.KeyUp1
        Dim Tabb As String = CStr(e.vkCode)
        Label2.Text = Tabb
        If Tabb = "162" Then
            control_F = False
        End If
    End Sub

    Private Sub Button1_Click_1(sender As Object, e As EventArgs) Handles Button_hook.Click
        If KeyboardHooker1.Hooked = False Then
            If KeyboardHooker1.MouseHookStart() = True Then
                Button_hook.Text = "Enabled"
                Scanline_Enabled = True
            End If
        Else
            If KeyboardHooker1.MouseHookEnd() = True Then
                Button_hook.Text = "Disabled"
                Scanline_Enabled = False
            End If
        End If
    End Sub

    Private Sub Button1_Click_2(sender As Object, e As EventArgs) Handles Button_Get_Global_IPAddress.Click
        Label_Global_IPaddress.Text = Get_Global_IPaddress()
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim ofd As New OpenFileDialog()

        'はじめのファイル名を指定する
        'はじめに「ファイル名」で表示される文字列を指定する
        ofd.FileName = ".rep"
        'はじめに表示されるフォルダを指定する
        '指定しない（空の文字列）の時は、現在のディレクトリが表示される
        Dim appPath As String = System.Reflection.Assembly.GetExecutingAssembly().Location
        ofd.InitialDirectory = appPath
        '[ファイルの種類]に表示される選択肢を指定する
        '指定しないとすべてのファイルが表示される
        ofd.Filter = "replayfile(*.rep)|*.rep*|すべてのファイル(*.*)|*.*"
        '[ファイルの種類]ではじめに選択されるものを指定する
        '2番目の「すべてのファイル」が選択されているようにする
        ofd.FilterIndex = 1
        'タイトルを設定する
        ofd.Title = "select file"
        'ダイアログボックスを閉じる前に現在のディレクトリを復元するようにする
        ofd.RestoreDirectory = True
        '存在しないファイルの名前が指定されたとき警告を表示する
        'デフォルトでTrueなので指定する必要はない
        ofd.CheckFileExists = True
        '存在しないパスが指定されたとき警告を表示する
        'デフォルトでTrueなので指定する必要はない
        ofd.CheckPathExists = True

        'ダイアログを表示する
        If ofd.ShowDialog() = DialogResult.OK Then
            'OKボタンがクリックされたとき、選択されたファイル名を表示する
            Console.WriteLine(ofd.FileName)
            Dim line As String

            Dim al As New ArrayList

            Using sr As StreamReader = New StreamReader(ofd.FileName, Encoding.GetEncoding("UTF-8"))

                line = sr.ReadLine()
                Do Until line Is Nothing
                    Dim OneItem As New ListInfo
                    OneItem.Lever_s = line
                    List_Rec.Add(OneItem)
                    line = sr.ReadLine()
                Loop
            End Using
        End If
    End Sub

    Dim Cn As Integer = 2
    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        'Dim P1 = vGen.PlugIn(CUInt(Cn))

    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        Dim enc As Encoding = Encoding.GetEncoding("UTF-8")
        Dim Fname As String = DateTime.Now.ToString("yy_MM_dd_HHmmss") & ".rep"
        Using writer As StreamWriter = New StreamWriter(Fname, False, enc)
            For i As Integer = 0 To List_Rec.Count - 1
                writer.WriteLine(List_Rec(i).Lever_s)
            Next
        End Using

    End Sub

    Private Sub Button11_Click_1(sender As Object, e As EventArgs) Handles Button11.Click
        'Dim U1 = vGen.UnPlugForce(CUInt(Cn))
        If List_Rec.Count > 0 Then
            N = 0

            XTimer_F = False
            x_timer.Change(Timeout.Infinite, Timeout.Infinite)
            'x2_timer.Change(0, interval)
            rep_timer.Change(0, interval)

            Panel7.Left = 53
            Button_X.Text = "Disabled"

        End If



    End Sub

    Private Sub Button12_Click(sender As Object, e As EventArgs) Handles Button12.Click
        rep_timer.Change(Timeout.Infinite, Timeout.Infinite)
        x2_timer.Change(Timeout.Infinite, Timeout.Infinite)
        'Dim U1 = vGen.UnPlugForce(CUInt(Cn))
    End Sub

    Public Sub DemoPlay()

        Dim Len As Integer = List_Rec.Count - 1
        If N < Len Then
            N += 1
        Else
            N = 0
            List_dammy.Clear()
        End If
        Keys_Demo_Play()

    End Sub

    Dim nn As Integer = 0
    Private Sub Keys_Demo_Play()
        Dim lever As Integer = CUShort(CInt(List_Rec(N).Lever_s)) Mod 16
        Dim button As Integer = CUShort(CInt(List_Rec(N).Lever_s)) - lever

        'vGen.SetDpad(CUInt(Cn), CByte(lever))
        'vGen.SetButton(CUInt(Cn), CUShort(button), True)
        'vGen.SetButton(CUInt(Cn), CUShort(62448 - button), False)
        'Dim OneItem As New ListInfo2
        'OneItem.Lever_s = lever
        'List_dammy.Add(OneItem)

    End Sub

    Private Sub Button13_Click(sender As Object, e As EventArgs) Handles Button13.Click
        If Rec = False Then
            If XTimer_F = False Then
                Exit Sub
            End If
            List_Rec.Clear()
            Button13.Text = "REC"
            Button13.ForeColor = Color.Red
            Rec = True
        Else

            Button13.Text = "STOP"
            Button13.ForeColor = Color.Gray
            Rec = False
        End If
    End Sub

    'Private Sub CheckBox_ss_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox_ss.CheckedChanged
    '    If CheckBox_ss.Checked = True Then
    '        Write_ss_Ini()
    '    Else
    '        Delete_ss_Ini()
    '    End If
    'End Sub

    'Private Sub Write_ss_Ini()
    '    Dim iniFileName As New StringBuilder(300)
    '    iniFileName.Append("Config\Supermodel.ini")
    '    Dim Section As String = " Global "
    '    WritePrivateProfileString(Section, "Supersampling", Label_SS.Text, iniFileName)
    'End Sub

    'Private Sub Delete_ss_Ini()
    '    Dim iniFileName As New StringBuilder(300)
    '    iniFileName.Append("Config\Supermodel.ini")
    '    Dim Section As String = " Global "
    '    WritePrivateProfileString(Section, "Supersampling", Nothing, iniFileName)
    'End Sub

    Private Sub ComboBox_input_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox_input.SelectedIndexChanged
        Dim N As String = ComboBox_input.SelectedItem
        Console.WriteLine(N)
        Dim iniFileName As New StringBuilder(300)
        iniFileName.Append("Config\Supermodel.ini")
        Dim Section As String = " Global "
        WritePrivateProfileString(Section, "InputSystem", N, iniFileName)
    End Sub

    Private Sub RawInput_hook_Click(sender As Object, e As EventArgs) Handles RawInput_hook.Click
        RawInput_Enabled = Not RawInput_Enabled
        If RawInput_Enabled = True Then
            RegisterRawInputDevices()
            InitializeMouseDevices()
            RawInput_hook.Text = "Enabled"
        Else
            RawInput_hook.Text = "Disabled"
        End If
    End Sub

    Private Sub ShowFavoriteToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ShowFavoriteToolStripMenuItem.Click
        If ShowFavoriteToolStripMenuItem.Text = "Show Favorites" Then
            ShowFavoriteToolStripMenuItem.Text = "Show All"
            ' favorite.txtの内容を読み込む
            Dim filePath As String = "favorite.txt"
            Dim favoriteItems As New HashSet(Of String)(File.ReadAllLines(filePath))
            Favorite_n = 0
            ' DataGridView1の現在のデータソースを取得
            Dim originalData As DataTable = CType(DataGridView1.DataSource, DataTable)

            ' フィルタリングされたデータを格納するための新しいDataTableを作成
            Dim filteredData As New DataTable
            For Each column As DataColumn In originalData.Columns
                filteredData.Columns.Add(column.ColumnName, column.DataType)
            Next

            ' favorite.txtに含まれるカラム1の値を持つ行を追加
            For Each row As DataRow In originalData.Rows
                If favoriteItems.Contains(row(2).ToString()) Then
                    Favorite_n += 1
                    filteredData.ImportRow(row)
                End If
            Next

            ' DataGridView1のデータソースをフィルタリングされたデータに更新
            DataGridView1.DataSource = filteredData
        Else
            ShowFavoriteToolStripMenuItem.Text = "Show Favorites"
            DataGridView_Setting()
            UpdateDataGridView()
        End If

    End Sub

    Private Sub Button14_Click(sender As Object, e As EventArgs) Handles Button14.Click
        'Button14.Top = -100
        If Capture_F = True Then
            captureForm.StopCapture()
            Dim result As DialogResult = MessageBox.Show("Close Window?",
                                             "Question",
                                             MessageBoxButtons.OKCancel,
                                             MessageBoxIcon.Exclamation,
                                             MessageBoxDefaultButton.Button2)
            If result = DialogResult.OK Then
                Capture_F = False
                captureForm.Close()
                captureForm.Dispose()
            ElseIf result = DialogResult.Cancel Then
                captureForm.StartCapture()
                captureForm.BringToFront()
            End If
        Else
            If Process.GetProcessesByName("supermodel").Count <> 0 Then

                Dim w As Integer = GetSupermodelWidth()
                Dim h As Integer = GetSupermodelHeight()
                Console.WriteLine("w:" & w)
                If w <> 640 Or h <> 360 Then

                Else
                    Capture_F = True
                    captureForm.Show()
                End If
            End If
        End If



        ''Console.WriteLine(Process.GetProcessesByName("supermodel").Count)
        'If Process.GetProcessesByName("supermodel").Count <> 0 Then

        '    Dim w As Integer = GetSupermodelWidth()
        '    Dim h As Integer = GetSupermodelHeight()
        '    Console.WriteLine("w:" & w)
        '    If w <> 640 Or h <> 360 Then

        '    Else
        '        Capture_F = True
        '        captureForm.Show()
        '    End If
        'Else
        'End If

    End Sub

    Private Sub timer_buttonProcess_Tick(sender As Object, e As EventArgs) Handles timer_buttonProcess.Tick
        timer_buttonProcess.Enabled = False
        Button14.Enabled = True
        Button14.Top = 32
    End Sub

    Private Sub Timer3_Tick(sender As Object, e As EventArgs) Handles Timer3.Tick
        Timer3.Enabled = False
        'If Tabb = 96 Then
        Dim cmd As String
        '再生しているWAVEを停止する
        cmd = "stop " + mysound
        mciSendString(cmd, Nothing, 0, IntPtr.Zero)
        '  閉じる
        cmd = "close " + mysound
        mciSendString(cmd, Nothing, 0, IntPtr.Zero)
        'End If
        brdy = 0
        Console.WriteLine("ttt:::" & brdy)

    End Sub
End Class


