using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using izolabella.WebSocket.Unity.Shared;
using izolabella.WebSocket.Unity.Shared.RequestHelpers;
using izolabella.WebSocket.Unity.Shared.UserAuth;

#nullable enable

namespace izolabella.WebSocket.Unity.Receiver
{
    public class Server
    {
        public Server(int Port, UserAuthenticationModel UserAuthModel, bool OverrideReceiverLookup = false)
        {
            this.AcceptedListenerRequests = new();
            this.Listener = new(System.Net.IPAddress.Any, Port);
            if (!OverrideReceiverLookup)
            {
                this.RequestHandlers = BaseImpUtil.GetItems<RequestHandler>();
            }

            this.UserAuthModel = UserAuthModel;
        }

        public delegate Task OnSocketConnectedH(Middle M);
        public event OnSocketConnectedH? OnSocketConnected;

        public delegate Task OnUserAuthFailureH(Middle M);
        public event OnUserAuthFailureH? OnUserAuthFailure;

        public List<RequestHandler> RequestHandlers { get; } = new();

        public TcpListener Listener { get; }

        public List<Task> AcceptedListenerRequests { get; }

        public UserAuthenticationModel UserAuthModel { get; }

        public Task ForgetAndFireAsync(Socket Client)
        {
            Middle SClient = new(Client, string.Empty, true/*, this.RequestHandlers, await (this.OnRequisiteRequest?.Invoke(Client) ?? Task.FromResult<IEnumerable<Requisite>>(Array.Empty<Requisite>()))*/);
            SClient.UserNeedsAuth += this.AttemptToAuthUser;
            this.OnSocketConnected?.Invoke(SClient);
            return Task.CompletedTask;
        }

        private async Task<IUser?> AttemptToAuthUser(HandlerRequestModel Model, Middle Instance)
        {
            IUser? U = await this.UserAuthModel.AuthUserAsync(Model);
            if (U == null)
            {
                this.OnUserAuthFailure?.Invoke(Instance);
            }
            return U;
        }

        public Task StartListener()
        {
            this.Listener.Start();
            new Task(async () =>
            {
                while (true)
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
