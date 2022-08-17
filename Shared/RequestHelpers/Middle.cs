using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using izolabella.WebSocket.Unity.Receiver;
using izolabella.WebSocket.Unity.Sender;
using izolabella.WebSocket.Unity.Shared.Frames;
using UnityEngine;

#nullable enable

namespace izolabella.WebSocket.Unity.Shared.RequestHelpers
{
    public class Middle
    {
        public Middle(Client Client)
        {
            this.Sock = Client.TcpClient.Client;
        }

        public Middle(SocketClient FromServer)
        {
            this.Sock = FromServer.Inner.Client;
        }

        public delegate Task OnRequestReceivedH(HandlerRequestModel Model);
        public event OnRequestReceivedH? OnRequestReceived;

        public delegate Task SocketDisconnectedH();
        public event SocketDisconnectedH? OnSocketDisconnected;

        public Socket Sock { get; }

        /// <summary>
        /// Retrieves a valid <see cref="NetworkStream"/> if the socket is connected.
        /// </summary>
        /// <returns></returns>
        public async Task<NetworkStream?> EnsureConnectionAsync()
        {
            if (this.Sock != null)
            {
                if (!this.Sock.Connected && this.OnSocketDisconnected != null)
                {
                    await this.OnSocketDisconnected.Invoke();
                }
                NetworkStream NetStream = new(this.Sock, true);
                if (NetStream.CanRead && NetStream.CanWrite)
                {
                    return NetStream;
                }
            }
            return null;
        }

        public Task StartProcessingIncoming()
        {
            new Task(async () =>
            {
                try
                {
                    using NetworkStream? NetStr = await this.EnsureConnectionAsync();
                    while (this.Sock.Connected && NetStr != null && NetStr.CanRead && NetStr.CanWrite/* && this.AppliedRequisites.All(R => R.EnsureValidity())*/)
                    {
                        if (NetStr.DataAvailable)
                        {
                            byte[] B = new byte[4];
                            NetStr.Read(B, 0, B.Length);
                            int DataSize = BitConverter.ToInt32(B, 0) - 4;
                            if (this.Sock.Available >= DataSize)
                            {
                                byte[] Data = new byte[DataSize];
                                NetStr.Read(Data, 0, Data.Length);
                                Frame? F = Frame.FromBytes(Data, false);
                                if (F is not null and Frame Fa)
                                {
                                    this.OnRequestReceived?.Invoke(Fa.Model);
                                    ////handler count = 0 fix
                                    //RequestHandler? Target = this.Handlers.FirstOrDefault(H => H.Alias.ToLower() == Fa.Model.Alias.ToLower());
                                    //if (Target != null)
                                    //{
                                    //    object SendBack = await Target.HandleRequest(Fa.Model);
                                    //}
                                }
                            }
                        }
                    }
                }
                catch (Exception Ex)
                {
                    Console.WriteLine(Ex);
                }
            }).Start();
            return Task.CompletedTask;
        }

        public async Task SendRequestAsync(HandlerRequestModel Model)
        {
            using NetworkStream? NetStr = await this.EnsureConnectionAsync();
            if(NetStr != null)
            {
                Frame Fr = new(Model);
                byte[] FrameBytes = Frame.ToBytes(Fr);
                NetStr.Write(FrameBytes, 0, FrameBytes.Length);
            }
        }
    }
}
