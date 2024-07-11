using System.Runtime.CompilerServices;
using System.Text;

namespace InsightLogParser.Client.Parsing
{
    internal class LogReader : IDisposable
    {
        private const int DelayTime = 250;

        private readonly FileStream _fs;
        private readonly StreamReader _sr;

        public LogReader(string logPath)
        {
            _fs = File.Open(logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            _sr = new StreamReader(_fs, Encoding.UTF8);
        }

        /// <summary>
        /// This code is ripped from the non-async version of .NET's ReadLine but if it runs out of characters keeps going until a full line has been read
        /// </summary>
        private async Task<string?> ReadFullLine(StreamReader reader, CancellationToken token)
        {
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                if (token.IsCancellationRequested)
                {
                    return null;
                }
                int ch = reader.Read();
                //if we reach the end of stream, wait a bit and try again
                if (ch == -1)
                {
                    try
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(DelayTime), token).ConfigureAwait(ConfigureAwaitOptions.None);
                    }
                    catch (OperationCanceledException) { }
                    continue;
                }
                if (ch == '\r' || ch == '\n')
                {
                    if (ch == '\r' && reader.Peek() == '\n')
                    {
                        reader.Read();
                    }

                    return sb.ToString();
                }
                sb.Append((char)ch);
            }
        }

        public async IAsyncEnumerable<string> ReadLinesAsync([EnumeratorCancellation] CancellationToken token = default)
        {
            while (!token.IsCancellationRequested)
            {
                if (_sr.EndOfStream)
                {
                    try
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(DelayTime), token).ConfigureAwait(ConfigureAwaitOptions.None);
                    }
                    catch (OperationCanceledException)
                    {
                        yield break;
                    }
                    continue;
                }

                string? line;
                try
                {
                    //line = await _sr.ReadLineAsync(token).ConfigureAwait(false);
                    line = await ReadFullLine(_sr, token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    yield break;
                }
                if (line == null) continue;
                yield return line;
            }
        }

        public void Dispose()
        {
            _sr.Dispose();
            _fs.Dispose();
        }
    }
}
