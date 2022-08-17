using System;
using System.Threading.Tasks;
using izolabella.WebSocket.Unity.Shared.UserAuth;

#nullable enable

namespace izolabella.WebSocket.Unity.Shared
{
    public abstract class RequestHandler
    {
        /// <summary>
        /// The time this handler was last invoked.
        /// </summary>
        public DateTime LastReceivedRequest { get; private set; }

        public abstract string Alias { get; }

        public virtual string? CallbackAlias { get; } = null;

        public abstract bool MustBeAuthorized { get; }

        public Task<object?> HandleRequest(HandlerRequestModel SentObject, IUser? User)
        {
            this.LastReceivedRequest = DateTime.UtcNow;
            return this.ProtectedHandleRequest(SentObject, User);
        }

        protected abstract Task<object?> ProtectedHandleRequest(HandlerRequestModel SentObject, IUser? User);
    }
}
