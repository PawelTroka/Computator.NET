using System.Diagnostics;

namespace Computator.NET.Core.Services
{
    public interface IProcessRunnerService
    {
        void Run(string path);
    }

    public class ProcessRunnerService : IProcessRunnerService
    {
        public void Run(string path)
        {
            Process.Start(path);
        }
    }
}