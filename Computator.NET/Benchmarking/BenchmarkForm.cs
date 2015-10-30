namespace Computator.NET.Benchmarking
{
    public partial class BenchmarkForm : System.Windows.Forms.Form
    {
        private readonly Benchmark benchmark;

        public BenchmarkForm()
        {
            InitializeComponent();

            benchmark = new Benchmark();


            functionsTestBackgroundWorker.DoWork += benchmark.mathFunctionsCalculationSpeedTest;
            functionsTestBackgroundWorker.ProgressChanged += FunctionsTestBackgroundWorker_ProgressChanged;
            functionsTestBackgroundWorker.RunWorkerCompleted += FunctionsTestBackgroundWorker_RunWorkerCompleted;

            memoryTestBackgroundWorker.DoWork += benchmark.memoryAllocationSpeedTest;
            memoryTestBackgroundWorker.ProgressChanged += memoryTestBackgroundWorker_ProgressChanged;
            memoryTestBackgroundWorker.RunWorkerCompleted += memoryTestBackgroundWorker_RunWorkerCompleted;
        }

        private void FunctionsTestBackgroundWorker_RunWorkerCompleted(object sender,
            System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                System.Windows.Forms.MessageBox.Show(
                    Localization.Strings.MathFunctionsCalculationSpeedTestCancelledByUser, Localization.Strings.Canceled);
            }

            else if (!(e.Error == null))
            {
                System.Windows.Forms.MessageBox.Show(Localization.Strings.Error + ": " + e.Error.Message,
                    Localization.Strings.Error + "!");
            }

            else
            {
                System.Windows.Forms.MessageBox.Show(
                    Localization.Strings.MathFunctionsCalculationSpeedTestDoneSuccesfullyCheckOutYourPointsResults,
                    Localization.Strings.Done);
                functionsTestRichTextBox.Text = System.DateTime.Now.ToShortDateString() + " " +
                                                System.DateTime.Now.ToShortTimeString() + Localization.Strings.Result +
                                                benchmark.Points +
                                                Localization.Strings.Points + functionsTestRichTextBox.Text;
            }
        }

        private void FunctionsTestBackgroundWorker_ProgressChanged(object sender,
            System.ComponentModel.ProgressChangedEventArgs e)
        {
            progressBar2.Value = e.ProgressPercentage;
        }

        private void memoryTestBackgroundWorker_RunWorkerCompleted(object sender,
            System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                System.Windows.Forms.MessageBox.Show(
                    Localization.Strings.memoryTestBackgroundWorker_RunWorkerCompleted, Localization.Strings.Canceled);
            }

            else if (!(e.Error == null))
            {
                System.Windows.Forms.MessageBox.Show(Localization.Strings.Error + ": " + e.Error.Message,
                    Localization.Strings.Error + "!");
            }

            else
            {
                System.Windows.Forms.MessageBox.Show(
                    Localization.Strings.MemoryAllocationSpeedTestDoneSuccesfullyCheckOutYourPointsResult,
                    Localization.Strings.Done);
                memoryTestRichTextBox.Text = System.DateTime.Now.ToShortDateString() + " " +
                                             System.DateTime.Now.ToShortTimeString() +
                                             Localization.Strings.Result + benchmark.Points +
                                             Localization.Strings.Points +
                                             memoryTestRichTextBox.Text;
            }
        }

        private void memoryTestBackgroundWorker_ProgressChanged(object sender,
            System.ComponentModel.ProgressChangedEventArgs e)
        {
            memoryTestProgressBar.Value = e.ProgressPercentage;
        }

        private void startMemoryTestButton_Click(object sender, System.EventArgs e)
        {
            if (!memoryTestBackgroundWorker.IsBusy)
                memoryTestBackgroundWorker.RunWorkerAsync();
        }

        // private Benchmark functionsBenchmark;

        private void cancelMemoryTestButton_Click(object sender, System.EventArgs e)
        {
            if (memoryTestBackgroundWorker.IsBusy)
                memoryTestBackgroundWorker.CancelAsync();
        }

        private void startFunctionsTestButton_Click(object sender, System.EventArgs e)
        {
            if (!functionsTestBackgroundWorker.IsBusy)
                functionsTestBackgroundWorker.RunWorkerAsync();
        }

        private void cancelFunctionsTestButton_Click(object sender, System.EventArgs e)
        {
            if (!functionsTestBackgroundWorker.IsBusy)
                functionsTestBackgroundWorker.CancelAsync();
        }
    }
}