using System.ComponentModel;

// This file is to support 'init' setter feature which is only available from .NET 5.0, refer: https://www.mking.net/blog/error-cs0518-isexternalinit-not-defined
namespace System.Runtime.CompilerServices;

[EditorBrowsable(EditorBrowsableState.Never)]
internal static class IsExternalInit { }