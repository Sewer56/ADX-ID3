using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace Sewer56.Adx.Id3.Utilities;

/// <summary>
/// Extensions to streams.
/// </summary>
public static class StreamExtensions
{
    /// <summary>
    /// Reads an unmanaged, generic type from the stream.
    /// </summary>
    /// <param name="stream">The stream to read the value from.</param>
    /// <param name="value">The value to return.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe bool TryReadSafe<T>(this Stream stream, out T value) where T : unmanaged
    {
        value = default;
        var valueSpan = new Span<byte>(Unsafe.AsPointer(ref value), sizeof(T));
        return TryReadSafe(stream, valueSpan);
    }

    /// <summary>
    /// Reads a given number of bytes from a stream.
    /// </summary>
    /// <param name="stream">The stream to read the value from.</param>
    /// <param name="result">The buffer to receive the bytes.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe bool TryReadSafe(this Stream stream, Span<byte> result)
    {
        int numBytesRead = 0;
        int numBytesToRead = result.Length;

        while (numBytesToRead > 0)
        {
            int bytesRead = stream.Read(result.Slice(numBytesRead));
            if (bytesRead <= 0)
                return false;

            numBytesRead += bytesRead;
            numBytesToRead -= bytesRead;
        }

        return true;
    }

    /// <summary>
    /// Writes a number of padding bytes to the stream.
    /// </summary>
    /// <param name="target">The stream to read the value from.</param>
    /// <param name="numBytesToWrite">Number of bytes to write.</param>
    /// <param name="padByte">The byte to perform padding with.</param>
    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void AddPadding(this Stream target, int numBytesToWrite, byte padByte)
    {
        const int maxBufferSize = 4096;

        // Init buffer to copy data.
        var bufferSize = Math.Min(maxBufferSize, numBytesToWrite);
        byte* buffer   = stackalloc byte[bufferSize];
        var bufferSpan = new Span<byte>(buffer, bufferSize);
        Unsafe.InitBlockUnaligned(buffer, padByte, (uint)bufferSize);
        
        // Loop while more than 0 left.
        int numBytesLeft = numBytesToWrite;

        do
        {
            var numBytesWrite = Math.Min(numBytesLeft, bufferSize);
            target.Write(bufferSpan.Slice(0, numBytesWrite));
            numBytesLeft -= numBytesWrite;
        }
        while (numBytesLeft > 0);
    }
}