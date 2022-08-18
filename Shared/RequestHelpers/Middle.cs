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
using izolabella.WebSocket.Unity.Shared.UserAuth;
using static izolabella.WebSocket.Unity.Shared.RequestHelpers.Middle;

#nullable enable

namespace izolabella.WebSocket.Unity.Shared.RequestHelpers
{
    public class Middle
    {
        public Middle(Socket Sock, string? Token, bool IsServer)
        {
            this.Sock = Sock;
            this.Token = Token;
            this.IsServer = IsServer;
            this.Handlers = BaseImpUtil.GetItems<RequestHandler>(AppDomain.CurrentDomain.GetAssemblies());
            this.StartProcessingIncoming();
        }

        public Middle(Socket Sock, string Hostname, int Port, string? Token, bool IsServer)
        {
            this.Sock = Sock;
            this.Token = Token;
            this.IsServer = IsServer;
            this.Sock.Connect(Hostname, Port);
            this.Handlers = BaseImpUtil.GetItems<RequestHandler>(AppDomain.CurrentDomain.GetAssemblies());
            this.StartProcessingIncoming();
        }

        public delegate Task OnRequestReceivedH(HandlerRequestModel Model, Middle Instance);
        public event OnRequestReceivedH? OnRequestReceived;

        public delegate Task SocketDisconnectedH(Middle Instance);
        public event SocketDisconnectedH? OnSocketDisconnected;

        public delegate Task<IUser?> UserNeedsAuthH(HandlerRequestModel Model, Middle Instance);
        public event UserNeedsAuthH? UserNeedsAuth;

        public delegate Task RateLimitedH(HandlerRequestModel Model, RequestHandler Target, Middle Instance);
        public event RateLimitedH? RateLimited;

        public delegate void DebugMessageH(string M);
        public event DebugMessageH? DebugMessage;

        public Socket Sock { get; }

        public string? Token { get; }

        public bool IsServer { get; }

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
                    await this.OnSocketDisconnected.Invoke(this);
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
                            this.DebugMessage?.Invoke($"Received frame, self-size report: {DataSize} bytes");
                            if (this.Sock.Available >= DataSize)
                            {
                                byte[] Data = new byte[DataSize];
                                NetStr.Read(Data, 0, Data.Length);
                                byte[] BData = B.Concat(Data).ToArray();
                                Frame? F = Frame.FromBytes(BData);
                                if (F is not null and Frame Fa)
                                {
                                    this.OnRequestReceived?.Invoke(Fa.Model, this);
                                    RequestHandler? Target = this.Handlers.FirstOrDefault(H => H.Alias.ToLower() == Fa.Model.Alias.ToLower());
                                    if (Target != null && Target.Limiter.Passes(this.Sock.RemoteEndPoint))
                                    {
                                        IUser? A = null;
                                        if(Target.MustBeAuthorized && this.IsServer && this.UserNeedsAuth != null)
                                        {
                                            A = await this.UserNeedsAuth.Invoke(Fa.Model, this);
                                        }
                                        if((Target.MustBeAuthorized && A != null) || !Target.MustBeAuthorized)
                                        {
                                            object? SendBack = await Target.HandleRequest(Fa.Model, this, A);
                                            if(SendBack != null && Target.CallbackAlias != null)
                                            {
                                                await this.SendRequestAsync(new(Target.CallbackAlias, SendBack, this.Token));
                                            }
                                        }
                                    }
                                    else if(Target != null && !Target.Limiter.Passes(this.Sock.RemoteEndPoint))
                                    {
                                        this.RateLimited?.Invoke(Fa.Model, Target, this);
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

        public bool LinkCallbackByType<T>(Action<object> E) where T : RequestHandler
        {
            RequestHandler? H = this.Handlers.FirstOrDefault(H => H.GetType() == typeof(T));
            if(H != null)
            {
                H.OnCustomCallback += (A) => E.Invoke(A);
                return true;
            }
            return false;
        }
    }
}
