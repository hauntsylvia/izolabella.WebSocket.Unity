using System;
using System.Net.Sockets;

namespace izolabella.WebSocket.Unity.Shared.Requisites
{
    public class HeartbeatRequisite : Requisite
    {
        public HeartbeatRequisite(TimeSpan MaximumGrace, TcpClient Client) : base(Client)
        {
            this.MaximumGrace = MaximumGrace;
        }

        public TimeSpan MaximumGrace { get; }

        protected override bool ProtectedEnsureValidity()
        {
            return this.TimeSinceLastEnsured < this.MaximumGrace;
        }
    }
}
