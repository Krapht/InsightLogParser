using System.Linq;

namespace InsightLogParser.Client
{
    internal class ServerTracker
    {
        private readonly MessageWriter _writer;
        private readonly List<string> _previousServers = new List<string>();

        public ServerTracker(MessageWriter writer)
        {
            _writer = writer;
        }

        public void Connect(string serverAddress)
        {
            _writer.WriteServerConnection(_previousServers, serverAddress);
        }

        public void Connected(string serverAddress)
        {
            if (!_previousServers.Contains(serverAddress))
            {
                _previousServers.Add(serverAddress);
            }
        }
    }
}
