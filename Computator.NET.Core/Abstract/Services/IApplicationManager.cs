namespace Computator.NET.Core.Abstract.Services
{
    public interface IApplicationManager
    {
        void SendStringAsKey(string key);
        void Restart();
    }
}