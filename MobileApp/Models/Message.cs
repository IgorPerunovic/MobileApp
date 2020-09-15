using aucobo;
using MobileApp.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace MobileApp.Models
{
    public class Message
    {
        [JsonProperty("initiator")]
        public User Initiator { get; set; }

        [JsonProperty("metaTags")]
        public Dictionary<string, string> MetaTags { get; set; } = new Dictionary<string, string>()
        {
            ["eventOriginId"] = Guid.NewGuid().ToString(),
            ["created"] = Helper.CurrentTimeMillis.ToString(), // still_todo: check this, should it be current time or what we get?
        };

        [JsonIgnore]
        public string EventOriginId
        {
            get => MetaTags["eventOriginId"];
            set => MetaTags["eventOriginId"] = value;
        }


        [JsonProperty("created")]
        public long CreatedUnix { get; set; }

        [JsonIgnore]
        public DateTimeOffset Created
        {
            get => DateTimeOffset.FromUnixTimeMilliseconds(CreatedUnix);
            set => CreatedUnix = value.ToUnixTimeMilliseconds();
        }
        // still_todo: check Created in meta tags or just value in JSON?
        //[JsonIgnore]
        //public DateTimeOffset Created
        //{
        //    get => DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(MetaTags["created"]));
        //    set => MetaTags["created"] = value.ToUnixTimeMilliseconds().ToString();
        //}

        [JsonProperty("_id")]
        public string ID { get; set; } = Guid.NewGuid().ToString();

        // Whereas 1 is more important than 2
        [JsonProperty("priority")]
        public int Priority { get; set; }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            if (MetaTags == null) { MetaTags = new Dictionary<string, string>(); }
            if (!MetaTags.ContainsKey("eventOriginId")) { EventOriginId = Guid.NewGuid().ToString(); }
            if (!MetaTags.ContainsKey("created")) { Created = DateTimeOffset.UtcNow; }
        }

        // used only in app
        public string RabbitEventHeader { get; set; }

        [JsonIgnore]
        public bool SerializeRabbitEventHeader { get; set; }

    }
}
