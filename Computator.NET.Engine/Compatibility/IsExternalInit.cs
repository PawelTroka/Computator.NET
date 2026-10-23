#if NETFRAMEWORK
// Compiler marker required by records on Framework; it adds no runtime behavior.
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}
#endif
