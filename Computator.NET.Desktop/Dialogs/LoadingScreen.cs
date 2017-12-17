using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Computator.NET.Core.Properties;
using Computator.NET.DataTypes.Properties;
using Computator.NET.Desktop.Services;

namespace Computator.NET.Desktop.Dialogs
{
    internal class LoadingScreen : Form
    {
        //The type of form to be displayed as the splash screen.
        private static LoadingScreen loadingScreen;

        public LoadingScreen()
        {
            AutoScaleMode = AutoScaleMode.Font;
            AutoSize = true;
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterScreen;
            this.Icon = GraphicsResources.computator_net_icon;
            //  this.BackColor = Color.White;
            //this.TransparencyKey = Color.White;
            var img = Resources.computator_net_logo;
            // this.BackgroundImage = Properties.Resources.computator_net_icon.ToBitmap();

            var pictureBox = new PictureBox
            {
                Image = img,
                Dock = DockStyle.Fill,
                Width = 256,
                Height = 256,
                SizeMode = PictureBoxSizeMode.StretchImage
            };

            pictureBox.Size = pictureBox.Size.DpiScale();

            var progressBar = new ProgressBar
            {
                Style = ProgressBarStyle.Marquee,
                MarqueeAnimationSpeed = 10,
                Dock = DockStyle.Bottom
            };
            progressBar.Size = progressBar.Size.DpiScale();

            Size = new Size(pictureBox.Width, pictureBox.Height + progressBar.Height);

            // this.Controls.Add(label);
            Controls.Add(pictureBox);
            Controls.Add(progressBar);
        }

        protected override bool ShowWithoutActivation
        {
            get { return true; }
        }

        public static void ShowSplashScreen()
        {
            // Make sure it is only launched once.

            if (loadingScreen != null)
                return;
            var thread = new Thread(ShowForm) {IsBackground = true};
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        private static void ShowForm()
        {
            loadingScreen = new LoadingScreen();
            Application.Run(loadingScreen);
        }

        public static void CloseForm()
        {
            loadingScreen.Invoke(new CloseDelegate(CloseFormInternal));
        }

        private static void CloseFormInternal()
        {
            loadingScreen.Close();
        }

        //Delegate for cross thread call to close

        private delegate void CloseDelegate();
    }
}