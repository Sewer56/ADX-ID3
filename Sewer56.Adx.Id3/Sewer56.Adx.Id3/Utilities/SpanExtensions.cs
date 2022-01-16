using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Sewer56.Adx.Id3.Utilities;

/// <summary>
/// Provides extensions related to spans.
/// </summary>
public static class SpanExtensions
{
    /// <summary/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<byte> AsSpanFast(this byte[] data)
    {
#if NET5_0 || NET5_0_OR_GREATER
        return MemoryMarshal.CreateSpan(ref MemoryMarshal.GetArrayDataReference(data), data.Length);
#else
        return data.AsSpan();
#endif
    }
}