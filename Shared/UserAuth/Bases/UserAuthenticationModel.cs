using System.Threading.Tasks;

#nullable enable

namespace izolabella.WebSocket.Unity.Shared.UserAuth
{
    public abstract class UserAuthenticationModel
    {
        public UserAuthenticationModel()
        {

        }

        public abstract Task<IUser?> AuthUserAsync(HandlerRequestModel Model);
    }
}
