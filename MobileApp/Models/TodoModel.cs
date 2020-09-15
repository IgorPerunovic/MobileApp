using aucobo;
using MobileApp.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace MobileApp.Models
{
    public class TodoModel : Message
    {
        [JsonProperty("body")]
        public string Body { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("color")]
        public string ColorCode { get; set; }

        [JsonProperty("displayDurationMillis")]
        public int DisplayDurationMilliseconds { get; set; }

        [JsonProperty("vibration")]
        public int Vibration { get; set; } = 1;

        [JsonProperty("sound")]
        public string Sound { get; set; }

        [JsonProperty("assignee")]
        public User Assignee { get; set; }

        [JsonIgnore]
        public bool AssignedToCurrentUser => Assignee?.ID == Settings.Smartwatch.Owner.ID;
        
        [JsonIgnore]
        public Color Color => Color.FromHex(ColorCode); // still_todo: check if it's hex or other format? Also, should we trim # from start?

    }
}
