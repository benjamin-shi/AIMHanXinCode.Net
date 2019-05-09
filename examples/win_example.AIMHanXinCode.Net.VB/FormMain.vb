Imports System
Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Data
Imports System.Drawing
Imports System.Text
Imports System.Windows.Forms

Imports renjuzhihui.shiyu.barcode

Public Class FormMain
    Private Sub FormMain_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        comboBoxECL.SelectedIndex = 1

        comboBoxVersion.Items.Clear()
        For version As Integer = 1 To 84
            comboBoxVersion.Items.Add(version.ToString())
        Next
        If comboBoxVersion.Items.Count > 0 Then
            comboBoxVersion.SelectedIndex = 0
        End If
    End Sub

    Private Sub buttonEncode_Click(sender As Object, e As EventArgs) Handles buttonEncode.Click
        'reset result view
        pictureBoxResult.Image = Nothing
        labelResult.Text = ""
        labelResult.ForeColor = Color.Black
        labelResult.BackColor = Color.WhiteSmoke

        Try
            Dim ecl As Integer = comboBoxECL.SelectedIndex + 1
            Dim version As Integer = comboBoxVersion.SelectedIndex + 1
            Dim strData As String = textBoxData.Text

            'Get the transmit data from orignal data.
            '54936 Is the codepage of GB18030.
            'Since Han Xin Is more efficient in Chinese information compression, I use this Encoding to show the example.
            'You can change it to any other Encoding based on your application.
            'But, please note the encoding here must match the barcode scanner's encoding in actual situation.
            Dim transmit_data() As Byte = BarcodeTools.format_transmit_data(strData, Encoding.GetEncoding(54936))

            'Han Xin Encoding
            Dim symbol_matrix As Byte(,) = HanXinCode.Encode(transmit_data, version, ecl, HanXinCode.DEFAULT_ECI, 0)

            If symbol_matrix Is Nothing Then
                Throw New Exception("There is some error in Han Xin encoding process.")
            End If

            'symbol matrix to bitmap
            Dim bitmap_result As Bitmap = BarcodeTools.barcode_bitmap(symbol_matrix, 5)

            If bitmap_result Is Nothing Then
                Throw New Exception("There is some error in symbol matrix 2 bitmap process.")
            End If

            'show result
            pictureBoxResult.Image = bitmap_result
            If (bitmap_result.Width >= pictureBoxResult.Width) OrElse (bitmap_result.Height >= pictureBoxResult.Height) Then
                pictureBoxResult.SizeMode = PictureBoxSizeMode.Zoom
            Else
                pictureBoxResult.SizeMode = PictureBoxSizeMode.CenterImage
            End If

            labelResult.Text = "Succeed." & vbCrLf
            labelResult.Text &= "Version:" & version.ToString() & vbCrLf
            labelResult.Text &= "ECL:" & ecl.ToString()
            labelResult.ForeColor = Color.White
            labelResult.BackColor = Color.SeaGreen
        Catch ex As Exception
            'Encoding Failed for some reason
            pictureBoxResult.Image = Nothing
            labelResult.Text = "Error" & vbCrLf & ex.Message
            labelResult.ForeColor = Color.White
            labelResult.BackColor = Color.Red
        End Try
    End Sub

    Private Sub contextMenuStripResult_Opening(sender As Object, e As CancelEventArgs) Handles contextMenuStripResult.Opening
        If pictureBoxResult.Image Is Nothing Then
            e.Cancel = True
        End If
    End Sub

    Private Sub copyToClipboardToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles copyToClipboardToolStripMenuItem.Click
        If pictureBoxResult.Image IsNot Nothing Then
            Dim bitmap_copied As Bitmap = New Bitmap(pictureBoxResult.Image)

            Clipboard.SetImage(bitmap_copied)
        End If
    End Sub

    Private Sub saveImageToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles saveImageToolStripMenuItem.Click
        If pictureBoxResult.Image IsNot Nothing Then
            If saveFileDialogResult.ShowDialog() = DialogResult.OK Then
                Dim imagePath As String = saveFileDialogResult.FileName

                Dim bitmap_copied As Bitmap = New Bitmap(pictureBoxResult.Image)

                bitmap_copied.Save(imagePath)

                bitmap_copied.Dispose()
                bitmap_copied = Nothing
            End If
        End If
    End Sub
End Class
