using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GUI
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            label1.Text = "NewFile";
            label2.Text += "0";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NewFile(richTextBox1);
        }

        private void NewFile(RichTextBox richTextBox1)
        {
            if(label1.Text == "NewFile")
            {
                var message = MessageBox.Show("Save current change?", "", MessageBoxButtons.OKCancel);
                if (message == DialogResult.OK)
                {
                    Save(richTextBox1, true);
                }
            }
            else if(label1.Text != "NewFile" && label1.ForeColor == Color.Red)
            {
                // save current change
                var message = MessageBox.Show("Save current change?", "", MessageBoxButtons.OKCancel);
                if(message == DialogResult.OK)
                {
                    Save(richTextBox1, false);
                }
            }
            richTextBox1.Clear();
            label1.Text = "NewFile";
        }

        private void newToolStripButton_Click(object sender, EventArgs e)
        {
            NewFile(richTextBox1);
        }

        private void openToolStripButton_Click(object sender, EventArgs e)
        {
            OpenFile(richTextBox1);
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            if (label1.Text.Length == 0) label1.Text = "NewFile";
            label1.ForeColor = Color.Red;
            label2.Text = "Line: " + richTextBox1.Lines.Length;
        }

        private void OpenFile(RichTextBox richTextBox1)
        {
            // check if we need to save
            if (label1.Text == "NewFile")
            {
                var message = MessageBox.Show("Want save currently file?", "", MessageBoxButtons.OKCancel);
                if (message == DialogResult.OK)
                {
                    SaveFileDialog saveFileDialog = new SaveFileDialog();
                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        StreamWriter writer = new StreamWriter(saveFileDialog.OpenFile());
                        foreach (string l in richTextBox1.Lines)
                        {
                            writer.WriteLine(l);
                        }
                        writer.Dispose();
                        writer.Close();
                    }
                }
                
            }
            else
            {
                var message = MessageBox.Show("Want save currently change?", "", MessageBoxButtons.OKCancel);
                if (message == DialogResult.OK)
                {         
                        StreamWriter writer = new StreamWriter(label1.Text);
                        foreach (string l in richTextBox1.Lines)
                        {
                            writer.WriteLine(l);
                        }
                        writer.Dispose();
                        writer.Close();
                }
                
            }
            richTextBox1.Clear();

            // open file
            OpenFileDialog openFile = new OpenFileDialog();
            if (openFile.ShowDialog() == DialogResult.OK)
            {
                StreamReader reader = new StreamReader(openFile.FileName);
                string text = reader.ReadToEnd();
                richTextBox1.Text = text;
                label1.Text = openFile.FileName;
                label1.ForeColor = Color.Black;
                reader.Dispose(); reader.Close();
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFile(richTextBox1);
        }

        private void saveToolStripButton_Click(object sender, EventArgs e)
        {
            if (label1.Text == "NewFile")
            {
                Save(richTextBox1, true);
            }
            else
            {
                Save(richTextBox1, false);
            }
           
        }
        private void Save(RichTextBox richTextBox1, bool saveDialog)
        {
            if (saveDialog)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                if(saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    StreamWriter writer = new StreamWriter(saveFileDialog.FileName);
                    foreach (string l in richTextBox1.Lines)
                    {
                        writer.WriteLine(l);
                    }
                    writer.Dispose();
                    writer.Close();
                    label1.Text = saveFileDialog.FileName;
                }
            }
            else
            {
                StreamWriter writer = new StreamWriter(label1.Text);
                foreach (string l in richTextBox1.Lines)
                {
                    writer.WriteLine(l);
                }
                writer.Dispose();
                writer.Close();
            }
            MessageBox.Show("File saved");

            label1.ForeColor = Color.Black;
        }

        private void cutToolStripButton_Click(object sender, EventArgs e)
        {
            richTextBox1.Cut();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (label1.Text == "NewFile")
            {
                Save(richTextBox1, true);
            }
            else
            {
                Save(richTextBox1, false);
            }
        }

        private void copyToolStripButton_Click(object sender, EventArgs e)
        {
            richTextBox1.Copy();
        }

        private void pasteToolStripButton_Click(object sender, EventArgs e)
        {
            richTextBox1.Paste();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            if (label1.Text == "NewFile")
            {
                MessageBox.Show("Previously save current file");
                return;
            }
            else
            {
                if(label1.ForeColor == Color.Red)
                {
                    MessageBox.Show("Save current file");
                    return;
                }
                string extension = Path.GetExtension(label1.Text);
                if (extension != ".as")
                {
                    MessageBox.Show("File extension must be .as", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                else
                {
                    string outFileNname = "";
                    try
                    {
                        AK_Course_C_Sharp.ASOL.Exec(label1.Text, ref outFileNname);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    if(outFileNname != "")
                    {
                        var message = MessageBox.Show($"machine code saved to: {outFileNname}  open it?", "", MessageBoxButtons.OKCancel);
                        if(message == DialogResult.OK)
                        {
                            richTextBox1.Clear();
                            StreamReader streamReader = new StreamReader(outFileNname);
                            richTextBox1.Text = streamReader.ReadToEnd();
                            streamReader.Close();
                            label1.Text = outFileNname;
                        }
                        
                    }
                }
            }
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            if (label1.Text == "NewFile")
            {
                MessageBox.Show("Previously save current file");
                return;
            }
            else
            {
                if (label1.ForeColor == Color.Red)
                {
                    MessageBox.Show("Save current file");
                    return;
                }
                string extension = Path.GetExtension(label1.Text);
                if (extension != ".mc")
                {
                    MessageBox.Show("File extension must be .mc", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                else
                {
                    string outFileNname = "";
                    try
                    {
                        AK_Course_C_Sharp.SSOL.Exec(label1.Text, ref outFileNname);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        return;
                    }
                    if (outFileNname != "")
                    {
                        var message = MessageBox.Show($"report saved to: {outFileNname}  open it?", "", MessageBoxButtons.OKCancel);
                        if (message == DialogResult.OK)
                        {
                            richTextBox1.Clear();
                            StreamReader streamReader = new StreamReader(outFileNname);
                            richTextBox1.Text = streamReader.ReadToEnd();
                            streamReader.Close();
                            label1.Text = outFileNname;
                            
                        }

                    }
                }
            }
        }
    }
}
