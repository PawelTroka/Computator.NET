namespace Computator.NET.Installer
{
    internal class Program
    {
        private static void Main()
        {
            var bootstrapperBuilder = new BootstrapperBuilder();
            bootstrapperBuilder.Build();
        }
    }
}