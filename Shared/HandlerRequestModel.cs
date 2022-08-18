using System;
using Newtonsoft.Json;

#nullable enable

namespace izolabella.WebSocket.Unity.Shared
{
    [Serializable]
    public class HandlerRequestModel
    {
        [JsonConstructor]
        private HandlerRequestModel(string Alias, string Entity, string? Token)
        {
            this.Alias = Alias;
            this.Entity = Entity;
            this.Token = Token ?? string.Empty;
        }

        public HandlerRequestModel(string Alias, object Entity, string? Token)
        {
            this.Alias = Alias;
            this.Entity = JsonConvert.SerializeObject(Entity);
            this.Token = Token ?? string.Empty;
        }

        [JsonProperty("a")]
        public string Alias { get; }

        [JsonProperty("e")]
        private string Entity { get; }

        [JsonProperty("t")]
        public string Token { get; }

        public bool TryParse<T>(out T Value)
        {
            try
            {
#pragma warning disable CS8601 // Possible null reference assignment.
                Value = JsonConvert.DeserializeObject<T>(this.Entity);
                return Value != null;
            }
            catch(Exception Ex)
            {
                Console.WriteLine(Ex);
                Value = default;
#pragma warning restore CS8601 // Possible null reference assignment.
                return false;
            }
        }
    }
}
