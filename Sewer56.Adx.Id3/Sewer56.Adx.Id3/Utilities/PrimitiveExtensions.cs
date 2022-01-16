using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace Sewer56.Adx.Id3.Utilities;

/// <summary>
/// Extensions to binary primitives.
/// </summary>
public static class PrimitiveExtensions
{
    /// <summary>
    /// If necessary, converts value from big to little endian.
    /// </summary>
    /// <param name="value">The value to return.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort AsBigEndian(this ushort value)
    {
        return BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(value) : value;
    }

    /// <summary>
    /// If necessary, converts value from big to little endian.
    /// </summary>
    /// <param name="value">The value to return.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int AsBigEndian(this int value)
    {
        return BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(value) : value;
    }

    /// <summary>
    /// If necessary, converts value from big to little endian.
    /// </summary>
    /// <param name="value">The value to return.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint AsBigEndian(this uint value)
    {
        return BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(value) : value;
    }
}