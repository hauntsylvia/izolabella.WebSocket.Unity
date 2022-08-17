using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using izolabella.WebSocket.Unity.Shared;
using izolabella.WebSocket.Unity.Shared.Frames;
using Newtonsoft.Json;
using UnityEngine;

#nullable enable

namespace izolabella.WebSocket.Unity.Sender
{
    public class Client
    {
        public Client(string Host, int Port)
        {
            this.Host = Host;
            this.Port = Port;
            this.TcpClient = new(Host, Port);
            this.ProcessIncoming();
        }

        public delegate Task OnClientDisconnectedHander();
        public event OnClientDisconnectedHander? OnClientDisconnected;

        public delegate Task OnClientReconnectedHander();
        public event OnClientReconnectedHander? OnClientReconnected;

        public delegate Task OnDataReceivedHandler(HandlerRequestModel Model);
        public event OnDataReceivedHandler? OnDataReceived;

        public bool IsDisconnected { get; private set; } = true;

        public string Host { get; }

        public int Port { get; }

        public TcpClient TcpClient { get; }

        public async Task<NetworkStream?> EnsureConnectionAsync()
        {
            if(this.TcpClient != null)
            {
                if(!this.TcpClient.Connected)
                {
                    Debug.Log("connecting . .");
                    await this.TcpClient.ConnectAsync(this.Host, this.Port).ConfigureAwait(false);
                    Debug.Log("connected");
                }
                NetworkStream NetStream = this.TcpClient.GetStream();
                if(NetStream.CanRead && NetStream.CanWrite)
                {
                    if (this.IsDisconnected)
                    {
                        this.IsDisconnected = false;
                        this.OnClientReconnected?.Invoke();
                    }
                    return NetStream;
                }
            }
            this.IsDisconnected = true;
            this.OnClientDisconnected?.Invoke();
            return null;
        }

        public void ProcessIncoming()
        {
            new Task(async () =>
            {
                NetworkStream? Stream = await this.EnsureConnectionAsync().ConfigureAwait(false);
                if (Stream != null)
                {
                    while (true)
                    {
                        if(Stream.DataAvailable && Stream.CanRead)
                        {
                            string FinalRepresentation = string.Empty;
                            byte[] Chunk = new byte[2048];
                            while(Stream.Read(Chunk, 0, Chunk.Length) > 0)
                            {
                                FinalRepresentation += Encoding.UTF8.GetString(Chunk);
                            }
                            HandlerRequestModel? Model = JsonConvert.DeserializeObject<HandlerRequestModel>(FinalRepresentation);
                            if(Model != null)
                            {
                                this.OnDataReceived?.Invoke(Model);
                            }
                        }
                    }
                }
            }).Start();
        }

        public async Task SendRequestAsync(HandlerRequestModel Payload)
        {
            try
            {
                NetworkStream? Stream = await this.EnsureConnectionAsync();
                Debug.Log(Stream == null ? "connection bad" : "connection good");
                if (Stream != null)
                {
                    byte[] Raw = Frame.ToBytes(new(Payload));
                    Debug.Log(Payload.Entity + $" - sent, {Raw.Length} bytes");
                    Stream.Write(Raw, 0, Raw.Length);
                }
                else
                {
                    Debug.Log(Payload.Alias + " - not sent, stream null");
                }
            }
            catch(Exception Ex)
            {
                Debug.Log(Payload.Alias + $" - not sent, exception: {Ex}");
                this.OnClientDisconnected?.Invoke();
            }
        }
    }
}
