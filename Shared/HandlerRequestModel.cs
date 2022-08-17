using System;
using Newtonsoft.Json;
using UnityEngine;

namespace izolabella.WebSocket.Unity.Shared
{
    [Serializable]
    public class HandlerRequestModel
    {
        [JsonConstructor]
        public HandlerRequestModel(string Alias, object Entity)
        {
            this.Alias = Alias;
            this.Entity = Entity;
        }

        [JsonProperty("a")]
        public string Alias { get; }

        [JsonProperty("e")]
        public object Entity { get; }
    }
}
