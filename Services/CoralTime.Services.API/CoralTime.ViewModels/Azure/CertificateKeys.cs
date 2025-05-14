using Newtonsoft.Json;
using System.Collections.Generic;

namespace CoralTime.ViewModels.Azure
{
    public class Key
    {
        [JsonProperty("kty")]
        public string Kty { get; set; }

        [JsonProperty("use")]
        public string Use { get; set; }

        [JsonProperty("kid")]
        public string Kid { get; set; }

        [JsonProperty("x5t")]
        public string X5t { get; set; }

        [JsonProperty("n")]
        public string N { get; set; }

        [JsonProperty("e")]
        public string E { get; set; }

        [JsonProperty("x5c")]
        public List<string> X5c { get; set; } 

        [JsonProperty("cloud_instance_name")]
        public string CloudInstanceName { get; set; }
    }

    public class CertificateKeys
    {
        [JsonProperty("keys")]
        public List<Key> Keys { get; set; }
    }
}