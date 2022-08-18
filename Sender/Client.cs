using System.Net.Sockets;
using izolabella.WebSocket.Unity.Shared.RequestHelpers;

#nullable enable

namespace izolabella.WebSocket.Unity.Sender
{
    public class Client
    {
        public Client(string Host, int Port, string? Token)
        {
            this.Host = Host;
            this.Port = Port;
            Socket Sock = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.Middle = new(Sock, Host, Port, Token, false);
        }

        public string Host { get; }

        public int Port { get; }

        public Middle Middle { get; }
    }
}
