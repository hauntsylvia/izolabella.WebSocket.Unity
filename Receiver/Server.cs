using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using izolabella.WebSocket.Unity.Shared;
using izolabella.WebSocket.Unity.Shared.RequestHelpers;
using izolabella.WebSocket.Unity.Shared.Requisites;

#nullable enable

namespace izolabella.WebSocket.Unity.Receiver
{
    public class Server
    {
        public Server(int Port, bool OverrideReceiverLookup = false)
        {
            this.AcceptedListenerRequests = new();
            this.Listener = new(System.Net.IPAddress.Any, Port);
            if(!OverrideReceiverLookup)
            {
                this.RequestHandlers = BaseImpUtil.GetItems<RequestHandler>();
            }
        }

        public delegate Task OnSocketConnectedH(Middle M);
        public event OnSocketConnectedH? OnSocketConnected;

        public List<RequestHandler> RequestHandlers { get; } = new();

        public TcpListener Listener { get; }

        public List<Task> AcceptedListenerRequests { get; }
        public Task ForgetAndFireAsync(Socket Client)
        {
            Middle SClient = new(Client, string.Empty/*, this.RequestHandlers, await (this.OnRequisiteRequest?.Invoke(Client) ?? Task.FromResult<IEnumerable<Requisite>>(Array.Empty<Requisite>()))*/);
            this.OnSocketConnected?.Invoke(SClient);
            return Task.CompletedTask;
        }

        public Task StartListener()
        {
            this.Listener.Start();
            new Task(async () =>
            {
                while(true)
                {
                    Socket ClientTask = await this.Listener.AcceptSocketAsync();
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
