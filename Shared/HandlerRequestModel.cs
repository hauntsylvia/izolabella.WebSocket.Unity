using System;
using Newtonsoft.Json;
using UnityEngine;

#nullable enable

namespace izolabella.WebSocket.Unity.Shared
{
    [Serializable]
    public class HandlerRequestModel
    {
        [JsonConstructor]
        public HandlerRequestModel(string Alias, object Entity, string? Token)
        {
            this.Alias = Alias;
            this.Entity = Entity;
            this.Token = Token ?? string.Empty;
        }

        [JsonProperty("a")]
        public string Alias { get; }

        [JsonProperty("e")]
        public object Entity { get; }

        [JsonProperty("t")]
        public string Token { get; }
    }
}
