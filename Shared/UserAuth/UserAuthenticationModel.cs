using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable

namespace izolabella.WebSocket.Unity.Shared.UserAuth
{
    public abstract class UserAuthenticationModel
    {
        public UserAuthenticationModel()
        {

        }

        public abstract AUser? AuthUser(HandlerRequestModel Model);
    }
}
