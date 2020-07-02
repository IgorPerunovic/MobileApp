using Newtonsoft.Json;
using System.Collections.Generic;

namespace aucobo
{
    public class User
    {
        [JsonProperty("_id")]
        public string ID { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("firstname")]
        public string Firstname { get; set; }

        [JsonProperty("lastname")]
        public string Lastname { get; set; }

        [JsonProperty("mail")]
        public string Mail { get; set; }

        [JsonProperty("metaTags")]
        public Dictionary<string, string> MetaTags { get; set; }

        [JsonIgnore]
        public string FullName
        {
            get
            {
                var name = string.Empty;
                if (!string.IsNullOrWhiteSpace(Firstname)) { name += Firstname; }
                if (!string.IsNullOrWhiteSpace(Firstname) && !string.IsNullOrWhiteSpace(Lastname)) { name += " "; }
                if (!string.IsNullOrWhiteSpace(Lastname)) { name += Lastname; }
                if (string.IsNullOrWhiteSpace(name)) { name = Name; }
                return name;
            }
        }
    }
}
