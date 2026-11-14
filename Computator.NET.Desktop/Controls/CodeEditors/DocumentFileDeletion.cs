using System;
using System.IO;

namespace Computator.NET.Desktop.Controls.CodeEditors
{
    internal static class DocumentFileDeletion
    {
        // Approval may display modal dialogs or save a copy. Keep the original
        // disk version and open buffer until approval and deletion both succeed.
        internal static bool TryDelete(string path, Func<bool> approve,
            Func<bool> stillApproved, Action commit)
        {
            var expected = DocumentFileSafety.Capture(path);
            if (!approve() || !stillApproved()) return false;
            if (!expected.Matches(DocumentFileSafety.Capture(path)))
                throw new IOException("The file changed while choosing how to delete it. The current file and open document have been kept.");
            File.Delete(path);
            commit();
            return true;
        }
    }
}
