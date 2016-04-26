using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;

namespace Computator.NET.Charting.Printing
{
    internal class ImagePrinter
    {
        private readonly PrintDialog printdlg = new PrintDialog();
        private readonly PrintDocument printDocument = new PrintDocument();
        private readonly PrintPreviewDialog printPrvDlg = new PrintPreviewDialog();
        private Image imageToPrint;

        public ImagePrinter()
        {
            // printDocument.DefaultPageSettings.PrinterSettings.PrinterName = "Printer Name";
            // printDocument.DefaultPageSettings.Landscape = true; //or false!
            printDocument.PrintPage += (sender, args) =>
            {
                if (imageToPrint == null)
                    return;

                var m = args.MarginBounds;

                if (imageToPrint.Width/(double) imageToPrint.Height > m.Width/(double) m.Height) // image is wider
                {
                    m.Height = (int) (imageToPrint.Height/(double) imageToPrint.Width*m.Width);
                }
                else
                {
                    m.Width = (int) (imageToPrint.Width/(double) imageToPrint.Height*m.Height);
                }
                args.Graphics.DrawImage(imageToPrint, m);
            };


            // preview the assigned document or you can create a different previewButton for it
            printPrvDlg.Document = printDocument;
            //printPrvDlg.

            printdlg.Document = printDocument;
        }

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