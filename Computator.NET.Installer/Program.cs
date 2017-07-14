namespace Computator.NET.Setup
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