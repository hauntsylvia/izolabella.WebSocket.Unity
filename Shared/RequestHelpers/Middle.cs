using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
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
        public Middle(Socket Sock, string? Token)
        {
            this.Sock = Sock;
            this.Token = Token;
            this.Handlers = BaseImpUtil.GetItems<RequestHandler>(AppDomain.CurrentDomain.GetAssemblies());
            this.StartProcessingIncoming();
        }

        public Middle(Socket Sock, string Hostname, int Port, string? Token)
        {
            this.Sock = Sock;
            this.Token = Token;
            this.Sock.Connect(Hostname, Port);
            this.Handlers = BaseImpUtil.GetItems<RequestHandler>(AppDomain.CurrentDomain.GetAssemblies());
            this.StartProcessingIncoming();
        }

        public delegate Task OnRequestReceivedH(HandlerRequestModel Model);
        public event OnRequestReceivedH? OnRequestReceived;

        public delegate Task SocketDisconnectedH();
        public event SocketDisconnectedH? OnSocketDisconnected;

        public Socket Sock { get; }

        public string? Token { get; }

        public List<RequestHandler> Handlers { get; }

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
                else
                {
                    NetworkStream NetStream = new(this.Sock, true);
                    if (NetStream.CanRead && NetStream.CanWrite)
                    {
                        return NetStream;
                    }
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
                    NetworkStream? NetStr = await this.EnsureConnectionAsync();
                    while (this.Sock.Connected && NetStr != null && NetStr.CanRead && NetStr.CanWrite/* && this.AppliedRequisites.All(R => R.EnsureValidity())*/)
                    {
                        if (NetStr.DataAvailable)
                        {
                            byte[] B = new byte[4];
                            NetStr.Read(B, 0, B.Length);
                            int DataSize = BitConverter.ToInt32(B, 0) - 4;
                            if (this.Sock.Available >= DataSize)
                            {
                                byte[] Data = B.Concat(new byte[DataSize]).ToArray();
                                NetStr.Read(Data, 0, Data.Length);
                                Frame? F = Frame.FromBytes(Data);
                                if (F is not null and Frame Fa)
                                {
                                    this.OnRequestReceived?.Invoke(Fa.Model);
                                    RequestHandler? Target = this.Handlers.FirstOrDefault(H => H.Alias.ToLower() == Fa.Model.Alias.ToLower());
                                    if (Target != null)
                                    {
                                        object SendBack = await Target.HandleRequest(Fa.Model);
                                    }
                                }
                            }
                        }
                        else
                        {
                            await Task.Delay(TimeSpan.FromSeconds(0.1));
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
            NetworkStream? NetStr = await this.EnsureConnectionAsync();
            if(NetStr != null)
            {
                Frame Fr = new(Model);
                byte[] FrameBytes = Frame.ToBytes(Fr);
                NetStr.Write(FrameBytes, 0, FrameBytes.Length);
            }
        }
    }
}
