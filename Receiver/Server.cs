using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using izolabella.WebSocket.Unity.Shared;
using izolabella.WebSocket.Unity.Shared.Requisites;

#nullable enable

namespace izolabella.WebSocket.Unity.Receiver
{
    public class Server
    {
        const int maxWillingReadBytes = 5000000;

        public Server(int Port, bool OverrideReceiverLookup = false)
        {
            this.AcceptedListenerRequests = new();
            this.Listener = new(System.Net.IPAddress.Any, Port);
            if(!OverrideReceiverLookup)
            {
                this.RequestHandlers = BaseImpUtil.GetItems<RequestHandler>();
            }
        }

        public delegate Task<IEnumerable<Requisite>> OnRequisiteRequestHandler(TcpClient InnerClient);
        public event OnRequisiteRequestHandler? OnRequisiteRequest;

        public List<RequestHandler> RequestHandlers { get; } = new();

        public TcpListener Listener { get; }

        public List<Task> AcceptedListenerRequests { get; }

        public async Task ForgetAndFireAsync(TcpClient Client)
        {
            Client.ReceiveBufferSize = maxWillingReadBytes;
            Client.SendBufferSize = maxWillingReadBytes;
            SocketClient SClient = new(Client, this.RequestHandlers, await (this.OnRequisiteRequest?.Invoke(Client) ?? Task.FromResult<IEnumerable<Requisite>>(Array.Empty<Requisite>())));
            await SClient.ProcessRequests();
        }

        public Task StartListener()
        {
            this.Listener.Start();
            new Task(async () =>
            {
                while(true)
                {
                    TcpClient ClientTask = await this.Listener.AcceptTcpClientAsync();
                    new Task(() => this.AcceptedListenerRequests.Add(this.ForgetAndFireAsync(ClientTask))).Start();
                }
            }).Start();
            return Task.CompletedTask;
        }

        public async Task StopAsync()
        {
            await Task.WhenAll(this.AcceptedListenerRequests);
            this.Listener.Stop();
        }
    }
}
