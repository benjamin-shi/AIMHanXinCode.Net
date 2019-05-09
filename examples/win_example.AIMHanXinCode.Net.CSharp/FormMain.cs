using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using renjuzhihui.shiyu.barcode;

namespace win_example.AIMHanXinCode.Net.CSharp
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            comboBoxECL.SelectedIndex = 1;

            comboBoxVersion.Items.Clear();
            for (int version = 1;version < 84;++version)
            {
                comboBoxVersion.Items.Add(version.ToString());
            }
            if (comboBoxVersion.Items.Count > 0)
            {
                comboBoxVersion.SelectedIndex = 0;
            }
        }

        private void buttonEncode_Click(object sender, EventArgs e)
        {
            //reset result view
            pictureBoxResult.Image = null;
            labelResult.Text = "";
            labelResult.ForeColor = Color.Black;
            labelResult.BackColor = Color.WhiteSmoke;

            try
            {
                int ecl = comboBoxECL.SelectedIndex + 1;
                int version = comboBoxVersion.SelectedIndex + 1;
                string strData = textBoxData.Text;

                //Get the transmit data from orignal data.
                //54936 is the codepage of GB18030.
                //Since Han Xin is more efficient in Chinese information compression, I use this Encoding to show the example.
                //You can change it to any other Encoding based on your application.
                //But, please note the encoding here must match the barcode scanner's encoding in actual situation.
                byte[] transmit_data = BarcodeTools.format_transmit_data(strData, Encoding.GetEncoding(54936));

                //Han Xin Encoding
                byte[,] symbol_matrix = HanXinCode.Encode(transmit_data, ref version, ref ecl, HanXinCode.DEFAULT_ECI, 0);

                if (null == symbol_matrix)
                {
                    throw new Exception("There is some error in Han Xin encoding process.");
                }

                //symbol matrix to bitmap
                Bitmap bitmap_result = BarcodeTools.barcode_bitmap(symbol_matrix, 5);

                if (null == bitmap_result)
                {
                    throw new Exception("There is some error in symbol matrix 2 bitmap process.");
                }

                //show result
                pictureBoxResult.Image = bitmap_result;
                if ((bitmap_result.Width >= pictureBoxResult.Width) || (bitmap_result.Height >= pictureBoxResult.Height))
                {
                    pictureBoxResult.SizeMode = PictureBoxSizeMode.Zoom;
                }
                else
                {
                    pictureBoxResult.SizeMode = PictureBoxSizeMode.CenterImage;
                }

                labelResult.Text = "Succeed.\r\n";
                labelResult.Text += "Version:" + version.ToString() + "\r\n";
                labelResult.Text += "ECL:" + ecl.ToString();
                labelResult.ForeColor = Color.White;
                labelResult.BackColor = Color.SeaGreen;
            }
            catch (Exception ex)
            {
                //Encoding Failed for some reason
                pictureBoxResult.Image = null;
                labelResult.Text = "Error\r\n" + ex.Message;
                labelResult.ForeColor = Color.White;
                labelResult.BackColor = Color.Red;
            }
        }

        private void contextMenuStripResult_Opening(object sender, CancelEventArgs e)
        {
            if (null == pictureBoxResult.Image)
            {
                e.Cancel = true;
            }
        }

        private void copyToClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (null != pictureBoxResult.Image)
            {
                Bitmap bitmap_copied = new Bitmap(pictureBoxResult.Image);

                Clipboard.SetImage(bitmap_copied);
            }
        }

        private void saveImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (null != pictureBoxResult.Image)
            {
                if (saveFileDialogResult.ShowDialog() == DialogResult.OK)
                {
                    String imagePath = saveFileDialogResult.FileName;

                    Bitmap bitmap_copied = new Bitmap(pictureBoxResult.Image);

                    bitmap_copied.Save(imagePath);

                    bitmap_copied.Dispose();
                    bitmap_copied = null;
                }
            }
        }
    }
}
