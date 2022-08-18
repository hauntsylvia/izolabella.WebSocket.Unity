using System;
using System.Threading.Tasks;
using AttritionalFear.Util;
using izolabella.WebSocket.Unity.Shared.RequestHelpers;
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

        public virtual RateLimiter Limiter { get; } = new(TimeSpan.Zero);

        public delegate void OnThisRequestH(object Entity);
        public virtual event OnThisRequestH? OnCustomCallback;

        public Task<object?> HandleRequest(HandlerRequestModel SentObject, Middle Caller, IUser? User)
        {
            this.LastReceivedRequest = DateTime.UtcNow;
            return this.ProtectedHandleRequest(SentObject, Caller, User);
        }

        protected abstract Task<object?> ProtectedHandleRequest(HandlerRequestModel SentObject, Middle Caller, IUser? User);
    }
}
