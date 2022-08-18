using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace izolabella.WebSocket.Unity.Shared.Requisites
{
    public abstract class Requisite
    {
        public Requisite(TcpClient Client)
        {
            this.Client = Client;
        }

        public TcpClient Client { get; }

        public DateTime LastEnsured { get; private set; }

        public TimeSpan TimeSinceLastEnsured => DateTime.UtcNow.Subtract(this.LastEnsured);

        /// <summary>
        /// Ensures the client is still valid under the provided requisite model.
        /// </summary>
        /// <returns>True if the inner client passes the model.</returns>
        public bool EnsureValidity()
        {
            this.LastEnsured = DateTime.UtcNow;
            bool Passed = this.ProtectedEnsureValidity();
            if (!Passed)
            {
                this.KillClientAsync();
            }
            return Passed;
        }

        /// <summary>
        /// Ensures the client is still valid under the provided requisite model.
        /// </summary>
        /// <returns>True if the inner client passes the model.</returns>
        protected abstract bool ProtectedEnsureValidity();

        public Task KillClientAsync()
        {
            this.Client.Dispose();
            this.Client.Close();
            return this.OnClientDeath();
        }

        protected virtual Task OnClientDeath()
        {
            return Task.CompletedTask;
        }
    }
}
