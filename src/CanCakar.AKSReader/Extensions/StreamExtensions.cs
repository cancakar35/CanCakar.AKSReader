namespace CanCakar.AKSReader.Extensions
{
    internal static class StreamExtensions
    {
        internal static async Task ReadExactlyAsyncPolyfill(this Stream stream, byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            int totalRead = 0;
            while (totalRead < count)
            {
#if NET8_0_OR_GREATER
                int read = await stream.ReadAsync(buffer.AsMemory(offset + totalRead, count - totalRead), cancellationToken);
#else
                int read = await stream.ReadAsync(buffer, (offset + totalRead), (count - totalRead), cancellationToken);
#endif
                if (read == 0)
                {
                    throw new EndOfStreamException("Unable to read beyond the end of the stream.");
                }

                totalRead += read;
            }
        }

        internal static void ReadExactlyPolyfill(this Stream stream, byte[] buffer, int offset, int count)
        {
            int totalRead = 0;
            while (totalRead < count)
            {
                int read = stream.Read(buffer, (offset + totalRead), (count - totalRead));
                if (read == 0)
                {
                    throw new EndOfStreamException("Unable to read beyond the end of the stream.");
                }

                totalRead += read;
            }
        }
    }
}
