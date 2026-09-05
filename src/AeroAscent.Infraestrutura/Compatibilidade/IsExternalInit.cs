#if !NET5_0_OR_GREATER || NETSTANDARD2_0 || NETSTANDARD2_1
namespace System.Runtime.CompilerServices
{
    using System.ComponentModel;

    /// <summary>
    /// Classe interna de compatibilidade para permitir o uso de records e propriedades init em .NET Standard 2.1.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static class IsExternalInit
    {
    }
}
#endif
