using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Computator.NET.Charting.Printing
{
    class ImagePrinter
    {
        Image imageToPrint;
        readonly PrintDocument printDocument = new PrintDocument();
        public ImagePrinter()
        {
           // printDocument.DefaultPageSettings.PrinterSettings.PrinterName = "Printer Name";
           // printDocument.DefaultPageSettings.Landscape = true; //or false!
            printDocument.PrintPage += (sender, args) =>
            {
                if (imageToPrint == null)
                    return;

                Rectangle m = args.MarginBounds;

                if ((double)imageToPrint.Width / (double)imageToPrint.Height > (double)m.Width / (double)m.Height) // image is wider
                {
                    m.Height = (int)((double)imageToPrint.Height / (double)imageToPrint.Width * (double)m.Width);
                }
                else
                {
                    m.Width = (int)((double)imageToPrint.Width / (double)imageToPrint.Height * (double)m.Height);
                }
                args.Graphics.DrawImage(imageToPrint, m);
            };



            // preview the assigned document or you can create a different previewButton for it
            printPrvDlg.Document = printDocument;
            //printPrvDlg.

            printdlg.Document = printDocument;
        }

        PrintDialog printdlg = new PrintDialog();
        PrintPreviewDialog printPrvDlg = new PrintPreviewDialog();

        public void Print(Image image)
        {

            imageToPrint = image;




            if (printdlg.ShowDialog() == DialogResult.OK)
            {
                printDocument.Print();
            }



        }


        public void PrintPreview(Image image)
        {
            imageToPrint = image;

            printPrvDlg.ShowDialog(); // this shows the preview and then show the Printer Dlg below
        }

    }
}
