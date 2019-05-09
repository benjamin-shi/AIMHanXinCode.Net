<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FormMain
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(FormMain))
        Me.tableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel()
        Me.tableLayoutPanel2 = New System.Windows.Forms.TableLayoutPanel()
        Me.comboBoxVersion = New System.Windows.Forms.ComboBox()
        Me.label4 = New System.Windows.Forms.Label()
        Me.label3 = New System.Windows.Forms.Label()
        Me.label1 = New System.Windows.Forms.Label()
        Me.textBoxData = New System.Windows.Forms.TextBox()
        Me.comboBoxECL = New System.Windows.Forms.ComboBox()
        Me.buttonEncode = New System.Windows.Forms.Button()
        Me.tableLayoutPanel3 = New System.Windows.Forms.TableLayoutPanel()
        Me.pictureBoxResult = New System.Windows.Forms.PictureBox()
        Me.labelResult = New System.Windows.Forms.Label()
        Me.contextMenuStripResult = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.saveImageToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.copyToClipboardToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.saveFileDialogResult = New System.Windows.Forms.SaveFileDialog()
        Me.tableLayoutPanel1.SuspendLayout()
        Me.tableLayoutPanel2.SuspendLayout()
        Me.tableLayoutPanel3.SuspendLayout()
        CType(Me.pictureBoxResult, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.contextMenuStripResult.SuspendLayout()
        Me.SuspendLayout()
        '
        'tableLayoutPanel1
        '
        Me.tableLayoutPanel1.ColumnCount = 2
        Me.tableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.tableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.tableLayoutPanel1.Controls.Add(Me.tableLayoutPanel3, 0, 0)
        Me.tableLayoutPanel1.Controls.Add(Me.tableLayoutPanel2, 0, 0)
        Me.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.tableLayoutPanel1.Location = New System.Drawing.Point(0, 0)
        Me.tableLayoutPanel1.Name = "tableLayoutPanel1"
        Me.tableLayoutPanel1.RowCount = 1
        Me.tableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.tableLayoutPanel1.Size = New System.Drawing.Size(748, 469)
        Me.tableLayoutPanel1.TabIndex = 1
        '
        'tableLayoutPanel2
        '
        Me.tableLayoutPanel2.ColumnCount = 1
        Me.tableLayoutPanel2.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.tableLayoutPanel2.Controls.Add(Me.comboBoxVersion, 0, 5)
        Me.tableLayoutPanel2.Controls.Add(Me.label4, 0, 4)
        Me.tableLayoutPanel2.Controls.Add(Me.label3, 0, 2)
        Me.tableLayoutPanel2.Controls.Add(Me.label1, 0, 0)
        Me.tableLayoutPanel2.Controls.Add(Me.textBoxData, 0, 1)
        Me.tableLayoutPanel2.Controls.Add(Me.comboBoxECL, 0, 3)
        Me.tableLayoutPanel2.Controls.Add(Me.buttonEncode, 0, 6)
        Me.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill
        Me.tableLayoutPanel2.Location = New System.Drawing.Point(3, 3)
        Me.tableLayoutPanel2.Name = "tableLayoutPanel2"
        Me.tableLayoutPanel2.RowCount = 7
        Me.tableLayoutPanel2.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 41.0!))
        Me.tableLayoutPanel2.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.tableLayoutPanel2.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 41.0!))
        Me.tableLayoutPanel2.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 41.0!))
        Me.tableLayoutPanel2.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 41.0!))
        Me.tableLayoutPanel2.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 41.0!))
        Me.tableLayoutPanel2.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 93.0!))
        Me.tableLayoutPanel2.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23.0!))
        Me.tableLayoutPanel2.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23.0!))
        Me.tableLayoutPanel2.Size = New System.Drawing.Size(368, 463)
        Me.tableLayoutPanel2.TabIndex = 1
        '
        'comboBoxVersion
        '
        Me.comboBoxVersion.Dock = System.Windows.Forms.DockStyle.Fill
        Me.comboBoxVersion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.comboBoxVersion.FormattingEnabled = True
        Me.comboBoxVersion.Location = New System.Drawing.Point(3, 332)
        Me.comboBoxVersion.Name = "comboBoxVersion"
        Me.comboBoxVersion.Size = New System.Drawing.Size(362, 29)
        Me.comboBoxVersion.TabIndex = 7
        '
        'label4
        '
        Me.label4.Dock = System.Windows.Forms.DockStyle.Fill
        Me.label4.Location = New System.Drawing.Point(3, 291)
        Me.label4.Margin = New System.Windows.Forms.Padding(3)
        Me.label4.Name = "label4"
        Me.label4.Size = New System.Drawing.Size(362, 35)
        Me.label4.TabIndex = 5
        Me.label4.Text = "Version:"
        '
        'label3
        '
        Me.label3.Dock = System.Windows.Forms.DockStyle.Fill
        Me.label3.Location = New System.Drawing.Point(3, 209)
        Me.label3.Margin = New System.Windows.Forms.Padding(3)
        Me.label3.Name = "label3"
        Me.label3.Size = New System.Drawing.Size(362, 35)
        Me.label3.TabIndex = 4
        Me.label3.Text = "Error Correction Level:"
        '
        'label1
        '
        Me.label1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.label1.Location = New System.Drawing.Point(3, 3)
        Me.label1.Margin = New System.Windows.Forms.Padding(3)
        Me.label1.Name = "label1"
        Me.label1.Size = New System.Drawing.Size(362, 35)
        Me.label1.TabIndex = 0
        Me.label1.Text = "Data:"
        '
        'textBoxData
        '
        Me.textBoxData.Dock = System.Windows.Forms.DockStyle.Fill
        Me.textBoxData.Location = New System.Drawing.Point(3, 44)
        Me.textBoxData.Multiline = True
        Me.textBoxData.Name = "textBoxData"
        Me.textBoxData.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.textBoxData.Size = New System.Drawing.Size(362, 159)
        Me.textBoxData.TabIndex = 1
        '
        'comboBoxECL
        '
        Me.comboBoxECL.Dock = System.Windows.Forms.DockStyle.Fill
        Me.comboBoxECL.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.comboBoxECL.FormattingEnabled = True
        Me.comboBoxECL.Items.AddRange(New Object() {"L1", "L2", "L3", "L4"})
        Me.comboBoxECL.Location = New System.Drawing.Point(3, 250)
        Me.comboBoxECL.Name = "comboBoxECL"
        Me.comboBoxECL.Size = New System.Drawing.Size(362, 29)
        Me.comboBoxECL.TabIndex = 6
        '
        'buttonEncode
        '
        Me.buttonEncode.Dock = System.Windows.Forms.DockStyle.Fill
        Me.buttonEncode.Location = New System.Drawing.Point(3, 373)
        Me.buttonEncode.Name = "buttonEncode"
        Me.buttonEncode.Size = New System.Drawing.Size(362, 87)
        Me.buttonEncode.TabIndex = 8
        Me.buttonEncode.Text = "Encode"
        Me.buttonEncode.UseVisualStyleBackColor = True
        '
        'tableLayoutPanel3
        '
        Me.tableLayoutPanel3.ColumnCount = 1
        Me.tableLayoutPanel3.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.tableLayoutPanel3.Controls.Add(Me.pictureBoxResult, 0, 0)
        Me.tableLayoutPanel3.Controls.Add(Me.labelResult, 0, 1)
        Me.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill
        Me.tableLayoutPanel3.Location = New System.Drawing.Point(377, 3)
        Me.tableLayoutPanel3.Name = "tableLayoutPanel3"
        Me.tableLayoutPanel3.RowCount = 2
        Me.tableLayoutPanel3.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.tableLayoutPanel3.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 120.0!))
        Me.tableLayoutPanel3.Size = New System.Drawing.Size(368, 463)
        Me.tableLayoutPanel3.TabIndex = 2
        '
        'pictureBoxResult
        '
        Me.pictureBoxResult.BackColor = System.Drawing.Color.WhiteSmoke
        Me.pictureBoxResult.ContextMenuStrip = Me.contextMenuStripResult
        Me.pictureBoxResult.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pictureBoxResult.Location = New System.Drawing.Point(3, 3)
        Me.pictureBoxResult.Name = "pictureBoxResult"
        Me.pictureBoxResult.Size = New System.Drawing.Size(362, 337)
        Me.pictureBoxResult.TabIndex = 0
        Me.pictureBoxResult.TabStop = False
        '
        'labelResult
        '
        Me.labelResult.BackColor = System.Drawing.Color.WhiteSmoke
        Me.labelResult.Dock = System.Windows.Forms.DockStyle.Fill
        Me.labelResult.Location = New System.Drawing.Point(3, 346)
        Me.labelResult.Margin = New System.Windows.Forms.Padding(3)
        Me.labelResult.Name = "labelResult"
        Me.labelResult.Size = New System.Drawing.Size(362, 114)
        Me.labelResult.TabIndex = 1
        Me.labelResult.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'contextMenuStripResult
        '
        Me.contextMenuStripResult.ImageScalingSize = New System.Drawing.Size(24, 24)
        Me.contextMenuStripResult.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.saveImageToolStripMenuItem, Me.copyToClipboardToolStripMenuItem})
        Me.contextMenuStripResult.Name = "contextMenuStripResult"
        Me.contextMenuStripResult.Size = New System.Drawing.Size(199, 97)
        '
        'saveImageToolStripMenuItem
        '
        Me.saveImageToolStripMenuItem.Name = "saveImageToolStripMenuItem"
        Me.saveImageToolStripMenuItem.Size = New System.Drawing.Size(198, 30)
        Me.saveImageToolStripMenuItem.Text = "Save"
        '
        'copyToClipboardToolStripMenuItem
        '
        Me.copyToClipboardToolStripMenuItem.Name = "copyToClipboardToolStripMenuItem"
        Me.copyToClipboardToolStripMenuItem.Size = New System.Drawing.Size(198, 30)
        Me.copyToClipboardToolStripMenuItem.Text = "Copy"
        '
        'saveFileDialogResult
        '
        Me.saveFileDialogResult.Filter = "PNG file|*.png|Bitmap File|*.bmp|Jpeg File|*.jpg"
        Me.saveFileDialogResult.RestoreDirectory = True
        Me.saveFileDialogResult.Title = "Please choose the image file you wish to save"
        '
        'FormMain
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(10.0!, 21.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(748, 469)
        Me.Controls.Add(Me.tableLayoutPanel1)
        Me.Font = New System.Drawing.Font("Arial", 9.0!)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "FormMain"
        Me.Text = "Han Xin Code"
        Me.tableLayoutPanel1.ResumeLayout(False)
        Me.tableLayoutPanel2.ResumeLayout(False)
        Me.tableLayoutPanel2.PerformLayout()
        Me.tableLayoutPanel3.ResumeLayout(False)
        CType(Me.pictureBoxResult, System.ComponentModel.ISupportInitialize).EndInit()
        Me.contextMenuStripResult.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

    Private WithEvents tableLayoutPanel1 As TableLayoutPanel
    Private WithEvents tableLayoutPanel2 As TableLayoutPanel
    Private WithEvents comboBoxVersion As ComboBox
    Private WithEvents label4 As Label
    Private WithEvents label3 As Label
    Private WithEvents label1 As Label
    Private WithEvents textBoxData As TextBox
    Private WithEvents comboBoxECL As ComboBox
    Private WithEvents buttonEncode As Button
    Private WithEvents tableLayoutPanel3 As TableLayoutPanel
    Private WithEvents pictureBoxResult As PictureBox
    Private WithEvents labelResult As Label
    Private WithEvents contextMenuStripResult As ContextMenuStrip
    Private WithEvents saveImageToolStripMenuItem As ToolStripMenuItem
    Private WithEvents copyToClipboardToolStripMenuItem As ToolStripMenuItem
    Private WithEvents saveFileDialogResult As SaveFileDialog
End Class
