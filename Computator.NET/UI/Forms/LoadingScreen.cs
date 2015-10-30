namespace Computator.NET.UI.Forms
{
    internal class LoadingScreen : System.Windows.Forms.Form
    {
        //The type of form to be displayed as the splash screen.
        private static LoadingScreen loadingScreen;

        public LoadingScreen()
        {
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Icon = Properties.Resources.computator_net_icon;
            // this.BackColor = Color.White;
            var img = Properties.Resources.computator_net_logo;
            // this.BackgroundImage = Properties.Resources.computator_net_icon.ToBitmap();

            var pictureBox = new System.Windows.Forms.PictureBox
            {
                Image = img,
                Dock = System.Windows.Forms.DockStyle.Fill,
                Width = 256,
                Height = 256,
                SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage
            };

            var progressBar = new System.Windows.Forms.ProgressBar
            {
                Style = System.Windows.Forms.ProgressBarStyle.Marquee,
                MarqueeAnimationSpeed = 10,
                Dock = System.Windows.Forms.DockStyle.Bottom
            };

            var label = new System.Windows.Forms.Label
            {
                Text = "by Pawel Troka",
                Font = new System.Drawing.Font("Consolas", 10)
            };

            Size = new System.Drawing.Size(pictureBox.Width, pictureBox.Height + progressBar.Height);

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
            var thread = new System.Threading.Thread(ShowForm);
            thread.IsBackground = true;
            thread.SetApartmentState(System.Threading.ApartmentState.STA);
            thread.Start();
        }

        private static void ShowForm()
        {
            loadingScreen = new LoadingScreen();
            System.Windows.Forms.Application.Run(loadingScreen);
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