using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace aucobo
{
    public class ServerConfiguration
    {
        //maybe not needed? still_todo: check
        //[JsonProperty("settings")]
        //public List<Setting> Settings { get; set; }

        [JsonProperty("_id")]
        public string ID { get; set; }

        [JsonProperty("aucoboIP")]
        public string AucoboIP { get; set; }

        [JsonProperty("aucoboPort")]
        public string AucoboPort { get; set; }

        [JsonProperty("aucoboUser")]
        public string AucoboUser { get; set; }

        [JsonProperty("aucoboPw")]
        public string AucoboPassword { get; set; }

        [JsonProperty("rabbitIP")]
        public string RabbitIP { get; set; }

        [JsonProperty("rabbitPort")]
        public string RabbitPort { get; set; }

        [JsonProperty("rabbitUserName")]
        public string RabbitUserName { get; set; }

        [JsonProperty("rabbitPassword")]
        public string RabbitPassword { get; set; }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            ID = ID.Trim();
            AucoboIP = AucoboIP.Trim();
            AucoboPort = AucoboPort.Trim();
            AucoboUser = AucoboUser.Trim();
            AucoboPassword = AucoboPassword.Trim();
            RabbitIP = RabbitIP.Trim();
            RabbitPort = RabbitPort.Trim();
            RabbitUserName = RabbitUserName.Trim();
            RabbitPassword = RabbitPassword.Trim();
        }
    }
}
