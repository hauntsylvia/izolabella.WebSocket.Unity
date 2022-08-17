using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using izolabella.WebSocket.Unity.Shared;
using izolabella.WebSocket.Unity.Shared.Frames;
using izolabella.WebSocket.Unity.Shared.RequestHelpers;
using Newtonsoft.Json;
using UnityEngine;

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
