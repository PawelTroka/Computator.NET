using System;
using System.Windows.Forms;

namespace Computator.NET.Desktop.Controls.AutocompleteMenu
{
    // MSHTML loads DocumentText asynchronously. Starting another navigation before
    // it consumes the pending stream can discard that stream and display a blank
    // page. Serialize loads, retaining only the latest autocomplete selection.
    internal sealed class ReferenceDocumentLoader
    {
        private readonly WebBrowser browser;
        private bool loading;
        private string pendingDocument;
        private readonly Timer loader = new Timer { Interval = 20 };

        public event EventHandler DocumentReady;

        public ReferenceDocumentLoader(WebBrowser browser)
        {
            this.browser = browser;
            browser.DocumentCompleted += DocumentCompleted;
            browser.Disposed += (_, _) => loader.Dispose();
            browser.VisibleChanged += (_, _) => QueuePendingDocument();
            browser.HandleCreated += (_, _) => QueuePendingDocument();
            loader.Tick += (_, _) =>
            {
                loader.Stop();
                if (!loading && CanLoad && pendingDocument != null) LoadPendingDocument();
            };
        }

        public void ShowDocument(string html)
        {
            pendingDocument = html;
            QueuePendingDocument();
        }

        private bool CanLoad => !browser.IsDisposed && !browser.Disposing && browser.IsHandleCreated && browser.Visible;

        private void QueuePendingDocument()
        {
            if (!loading && CanLoad && pendingDocument != null) loader.Start();
            else loader.Stop();
        }

        private void LoadPendingDocument()
        {
            var html = pendingDocument;
            pendingDocument = null;
            loading = true;
            try { browser.DocumentText = html; }
            catch { loading = false; throw; }
        }

        private void DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (browser.ReadyState != WebBrowserReadyState.Complete || e.Url != browser.Url) return;
            loading = false;
            QueuePendingDocument();
            // A completed navigation can belong to an earlier selection. Only
            // size the tooltip once the newest queued document is ready.
            if (pendingDocument == null) DocumentReady?.Invoke(this, EventArgs.Empty);
        }
    }
}
