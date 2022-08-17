using System;
using System.Threading.Tasks;

namespace izolabella.WebSocket.Unity.Shared
{
    public abstract class RequestHandler
    {
        /// <summary>
        /// The time this handler was last invoked.
        /// </summary>
        public DateTime LastReceivedRequest { get; private set; }

        public abstract string Alias { get; }

        public abstract bool MustBeAuthorized { get; }

        public Task<object> HandleRequest(HandlerRequestModel SentObject)
        {
            this.LastReceivedRequest = DateTime.UtcNow;
            return this.ProtectedHandleRequest(SentObject);
        }

        protected abstract Task<object> ProtectedHandleRequest(HandlerRequestModel SentObject);
    }
}
