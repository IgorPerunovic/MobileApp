using aucobo;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MobileApp.Models
{
    public class Device
    {
        [JsonProperty("_id")]
        public string ID { get; set; }

        [JsonProperty("owner")]
        public User Owner { get; set; }

        [JsonProperty("configurationId")]
        public string ConfigurationId { get; set; }

        public static Device FromWalkieTalkieMessage(WalkieTalkieMessage message) => new Device()
        {
            ID = message.SenderDeviceId,
            Owner = new User() { Name = message.SenderOwnerName }
        };

        //TODO: implement this as needed
        public static Device FromPictureMessage(PictureMessage message) => new Device()
        {
            ID = message.SenderDeviceId,
            Owner = new User() { Name = message.SenderOwnerName }
        };
    }

    public class Smartwatch : Device
    {
        //TODO: implement this model to represent button data
        [JsonProperty("buttons")]
        public List<AucoboButtonModel> Buttons { get; set; }

        public Smartwatch GetCopy() => JsonConvert.DeserializeObject<Smartwatch>(JsonConvert.SerializeObject(this));
    }

    public class SmartwatchDto : Smartwatch
    {
        public static SmartwatchDto Create(Smartwatch smartwatch, ServerConfiguration config)
        {
            var newConfig = JsonConvert.DeserializeObject<SmartwatchDto>(JsonConvert.SerializeObject(smartwatch));
            newConfig.Configuration = config;
            return newConfig;
        }

        [JsonProperty("configuration")]
        public ServerConfiguration Configuration { get; set; }

        [JsonIgnore]
        public Smartwatch Smartwatch => GetCopy();
    }
}
