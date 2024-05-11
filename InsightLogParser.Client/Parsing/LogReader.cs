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
                    line = await _sr.ReadLineAsync(token).ConfigureAwait(false);
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
