﻿Imports System.Reflection.Emit
Imports System.Runtime.InteropServices

Public Class captureForm
    Dim FPS As Integer = 0
    Dim Hwnd_bin As IntPtr = Nothing
    Dim MyhWnd As IntPtr = Nothing
    'Dim left_i As Integer = 320
    'Dim top_i As Integer = 180
    Public targetBitmap As Bitmap
    Dim targetProcessName As String = "supermodel" ' ターゲットのプロセス名を指定してください
    Dim targetHwnd As IntPtr = IntPtr.Zero

    <DllImport("gdi32.dll")>
    Private Shared Function BitBlt(ByVal hDestDC As IntPtr,
        ByVal x As Integer, ByVal y As Integer,
        ByVal nWidth As Integer, ByVal nHeight As Integer,
        ByVal hSrcDC As IntPtr,
        ByVal xSrc As Integer, ByVal ySrc As Integer,
        ByVal dwRop As Integer) As Integer
    End Function

    <DllImport("user32.dll")>
    Private Shared Function ReleaseDC(ByVal hwnd As IntPtr,
        ByVal hdc As IntPtr) As IntPtr
    End Function


    <DllImport("user32.dll")>
    Private Shared Function GetWindowDC(ByVal hwnd As IntPtr) As IntPtr
    End Function

    <DllImport("user32.dll")>
    Private Shared Function GetForegroundWindow() As IntPtr
    End Function


    <DllImport("user32.dll")>
    Private Shared Function SetForegroundWindow(hWnd As IntPtr) As <MarshalAs(UnmanagedType.Bool)> Boolean
    End Function

    'PrivateFontCollectionオブジェクトを作成する
    Dim pfc As New System.Drawing.Text.PrivateFontCollection()

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        'リソース(wlmaru2004p4u)をバイト配列に読み込む
        Dim fontBuf As Byte() = My.Resources.HOOG0557

        'または、次のようにしてリソースを読み込む
        'Dim asm As System.Reflection.Assembly = _
        '    System.Reflection.Assembly.GetExecutingAssembly()
        'Dim strm As System.IO.Stream = _
        '    asm.GetManifestResourceStream("Project1.wlmaru2004p4u.ttf")
        'Dim fontBuf As Byte() = New Byte(strm.Length - 1) {}
        'strm.Read(fontBuf, 0, fontBuf.Length)
        'strm.Close()

        'バイト配列のポインターを取得する
        Dim fontBufPtr As IntPtr = System.Runtime.InteropServices.
    Marshal.AllocCoTaskMem(fontBuf.Length)
        System.Runtime.InteropServices.
    Marshal.Copy(fontBuf, 0, fontBufPtr, fontBuf.Length)
        'PrivateFontCollectionにフォントを追加する
        pfc.AddMemoryFont(fontBufPtr, fontBuf.Length)
        System.Runtime.InteropServices.
    Marshal.FreeCoTaskMem(fontBufPtr)

        targetBitmap = New Bitmap(640, 360)
        MyhWnd = Me.Handle
        Dim N = 0
        For Each p As System.Diagnostics.Process In System.Diagnostics.Process.GetProcesses()
            Console.WriteLine(p.ProcessName)
            Console.WriteLine(p.MainWindowHandle)
            If p.ProcessName = targetProcessName Then
                N += 1
                If N = 1 Then
                    targetHwnd = p.MainWindowHandle
                    Console.WriteLine(N)
                    Exit For
                End If

            End If
        Next

        StartCapture()
        FPS_Timer.Enabled = True

    End Sub

    Private Sub Center(By As Integer, Bh As Integer, Bx As Integer, Bw As Integer)
        Me.Top = CInt(((Bh / 2) + By) - (Me.Height / 2))
        Me.Left = CInt(((Bw / 2) + Bx) - (Me.Width / 2))
    End Sub

    Public taskRunning As Boolean = False

    Public Async Sub StartCapture()
        If taskRunning Then Return ' 前回のタスクが完了していない場合は何もしない
        taskRunning = True


        Await Task.Run(Async Function()
                           While taskRunning
                               If FPS_Timer.Enabled Then
                                   FPS += 1
                               End If

                               Dim winDC As IntPtr = GetWindowDC(targetHwnd)
                               Dim g As Graphics = Graphics.FromImage(targetBitmap)
                               Dim hDC As IntPtr = g.GetHdc()

                               ' Capture the screen content
                               BitBlt(hDC, 0, 0, 640, 360, winDC, Nothing, Nothing, SRCCOPY)

                               g.ReleaseHdc(hDC)
                               g.Dispose()
                               ReleaseDC(IntPtr.Zero, winDC)


                               ' 更新後の画像を保持する
                               PictureBox.Invoke(Sub()
                                                     If PictureBox.Image IsNot Nothing Then
                                                         PictureBox.Image.Dispose()
                                                     End If
                                                     PictureBox.Image = New Bitmap(targetBitmap)
                                                 End Sub)

                               ' 繰り返し間隔を調整するための遅延
                               Await Task.Delay(2) ' 適切な遅延時間を設定

                           End While
                       End Function)
    End Sub



    Public Sub StopCapture()
        taskRunning = False
    End Sub

    Const SRCCOPY As Integer = 13369376

    Private mousePoint As Point

    Private Sub Form1_MouseDown(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Me.MouseDown, PictureBox.MouseDown, Me.MouseClick, PictureBox.MouseClick
        If (e.Button And MouseButtons.Left) = MouseButtons.Left Then
            mousePoint = New Point(e.X, e.Y)
        End If
    End Sub
    Dim N As Integer

    Private Sub Form1_MouseMove(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Me.MouseMove, PictureBox.MouseMove
        If (e.Button And MouseButtons.Left) = MouseButtons.Left Then
            Me.Left += e.X - mousePoint.X
            Me.Top += e.Y - mousePoint.Y
        End If

    End Sub

    Dim mode As Integer = 1

    Private Sub PictureBox1_Click(sender As Object, e As EventArgs) Handles PictureBox.DoubleClick
        If mode = 3 Then
            Me.WindowState = FormWindowState.Maximized
            PictureBox.Top = 0
            PictureBox.Left = 0
            PictureBox.Width = Me.Width
            PictureBox.Height = Me.Height
            PictureBox.Top = 0
            mode = 0
        ElseIf mode = 2 Then
            Me.WindowState = FormWindowState.Normal
            Me.Width = 1600
            Me.Height = 900
            Me.Top = Me.Top - 90
            Me.Left = Me.Left - 160
            PictureBox.Top = 0
            PictureBox.Left = 0
            PictureBox.Width = Me.Width
            PictureBox.Height = Me.Height
            PictureBox.Top = 0
            mode = 3
        ElseIf mode = 1 Then
            Me.WindowState = FormWindowState.Normal
            Me.Width = 1280
            Me.Height = 720
            PictureBox.Top = 0
            PictureBox.Left = 0
            PictureBox.Width = Me.Width
            PictureBox.Height = Me.Height
            PictureBox.Top = 0
            Me.Top = Me.Top - 90
            Me.Left = Me.Left - 160
            mode = 2
        ElseIf mode = 0 Then
            Me.WindowState = FormWindowState.Normal
            Me.Width = 960
            Me.Height = 540
            PictureBox.Top = 0
            PictureBox.Left = 0
            PictureBox.Width = Me.Width
            PictureBox.Height = Me.Height
            PictureBox.Top = 0
            Me.Top = Me.Top + 180
            Me.Left = Me.Left + 320
            mode = 1
        Else
        End If
        If mode = 1 Or mode = 2 Or mode = 3 Then
            Dim s As System.Windows.Forms.Screen = System.Windows.Forms.Screen.FromControl(Me)
            Dim x As Integer = s.Bounds.X
            Dim y As Integer = s.Bounds.Y
            Dim h As Integer = s.Bounds.Height
            Dim w As Integer = s.Bounds.Width
            Center(y, h, x, w)
        End If
        FPS_Timer.Enabled = True

    End Sub

    Private Sub FPS_Timer_Tick(sender As Object, e As EventArgs) Handles FPS_Timer.Tick
        Dim f As New System.Drawing.Font(pfc.Families(0), 12)
        FPS_Label.UseCompatibleTextRendering = True
        FPS_Label.Font = f
        FPS_Label.Text = FPS.ToString
        FPS = 0
    End Sub

    Private Sub FPS_Click(sender As Object, e As EventArgs) Handles FPS_Label.Click
        If FPS_Timer.Enabled = True Then
            FPS_Timer.Enabled = False
            FPS_Label.Text = ""
        Else
            FPS_Timer.Enabled = True
        End If
    End Sub


    Private Sub pic_MouseDown(sender As Object, e As MouseEventArgs) Handles PictureBox.MouseDown
        If e.Button = MouseButtons.Right Then
            StopCapture()
            Dim result As DialogResult = MessageBox.Show("Close Window?",
                                             "Question",
                                             MessageBoxButtons.OKCancel,
                                             MessageBoxIcon.Exclamation,
                                             MessageBoxDefaultButton.Button2)
            If result = DialogResult.OK Then
                Form1.Capture_F = False
                Me.Close()
            ElseIf result = DialogResult.Cancel Then
                StartCapture()
            End If
        End If
    End Sub
    Private Sub Form1_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        Form1.Capture_F = False
        taskRunning = False
        targetBitmap.Dispose()
    End Sub


End Class
