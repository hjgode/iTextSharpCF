#define TEST
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using iTextSharp;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace iTextSharpCF_test
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void mnuExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            openFileDialog1.InitialDirectory = "\\My Documents\\My Pictures";
            openFileDialog1.Filter = "JPEG (*.jpg)|*.jpg|PNG (*.png)|*.png|GIF (*.gif)|*.gif|TIF (*.tif)|*.tif|all (*.*)|*.*";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
                listBox1.Items.Add(openFileDialog1.FileName);

        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex != -1)
                listBox1.Items.RemoveAt(listBox1.SelectedIndex);
        }

        private bool addImage(string sFilename, iTextSharp.text.Document doc)
        {
            bool bReturn = false;
            iTextSharp.text.Image img;
            try
            {
                Paragraph p1 = new Paragraph(new Chunk(sFilename, FontFactory.GetFont(FontFactory.HELVETICA, 12)));
                doc.Add(p1);

#if !TEST
                Bitmap myBitmap = new Bitmap(sFilename);
                if (sFilename.ToLower().EndsWith("jpg"))
                    img = iTextSharp.text.Image.GetInstance(myBitmap, System.Drawing.Imaging.ImageFormat.Jpeg);
                else if (sFilename.ToLower().EndsWith("gif"))
                    img = iTextSharp.text.Image.GetInstance(myBitmap, System.Drawing.Imaging.ImageFormat.Gif);
                else if (sFilename.ToLower().EndsWith("bmp"))
                    img = iTextSharp.text.Image.GetInstance(myBitmap, System.Drawing.Imaging.ImageFormat.Bmp);
                else if (sFilename.ToLower().EndsWith("png"))
                    img = iTextSharp.text.Image.GetInstance(myBitmap, System.Drawing.Imaging.ImageFormat.Png);
                else
                    throw new NotSupportedException("Unsupported image format");
                //is the image to wide or to high?
                float fWidth=doc.Right - doc.Left - doc.RightMargin - doc.LeftMargin;
                float fHeight=doc.Top - doc.Bottom - doc.TopMargin - doc.BottomMargin;
                if ((myBitmap.Width > fWidth) || (myBitmap.Height>fHeight))
                    img.ScaleToFit(fWidth, fHeight);

#else
                img = iTextSharp.text.Image.GetInstance(sFilename);
#endif
                doc.Add(img);
                doc.NewPage(); //used to create a new page for every image
                bReturn = true;
            }
            catch (iTextSharp.text.BadElementException bx)
            {
                System.Diagnostics.Debug.WriteLine("BadElementException in doc.add() for '" + sFilename + "': " + bx.Message);
            }
            catch (iTextSharp.text.DocumentException dx)
            {
                System.Diagnostics.Debug.WriteLine("DocumentException in doc.add() for '" + sFilename + "': " + dx.Message);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception in doc.add() for '" + sFilename + "': " + ex.Message);
            }
            return bReturn;
        }
        private void btnCreatePDF_Click(object sender, EventArgs e)
        {
            string myPDFfile = "";
            if (listBox1.Items.Count > 0)
            {
                saveFileDialog1.FileName = "\\My Documents\\MyPDF.pdf";
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    myPDFfile = saveFileDialog1.FileName;
                    if (!myPDFfile.ToLower().EndsWith(".pdf"))
                        if(myPDFfile.EndsWith("."))
                            myPDFfile += "pdf";
                        else
                            myPDFfile += ".pdf";
                }
                else
                    return;
                textBox1.Text = "Saving to " + myPDFfile + "\r\n";
                iTextSharp.text.Document doc = new Document();
                PdfWriter writer = PdfWriter.GetInstance(doc, new System.IO.FileStream(myPDFfile, System.IO.FileMode.Create));
                
                doc.Open();
                string currentImageName;
                //Uri currentURI; //will not work locally
                for (int i = 0; i < listBox1.Items.Count; i++)
                {
                    currentImageName = listBox1.Items[i].ToString();
                    //accessing local file using WebRequest does not work in CF!
                    //currentURI = new Uri(currentImageName,UriKind.Relative);
                    textBox1.Text += currentImageName;
                    if (addImage(currentImageName, doc))
                        textBox1.Text += " inserted OK\r\n";
                    else
                        textBox1.Text += " insertion failed\r\n";
                }
                doc.Close();
                textBox1.Text += "finished";
            }
        }
    }
}