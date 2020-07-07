using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MobileApp.Models
{
    public class AucoboButtonModel : Message
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonIgnore]
        public string FormattedString => Text == Constants.WALKIE_TALKIE_BUTTON_ID 
            ? "Walkie Talkie" 
            : 
                (Text == Constants.PICTURE_COMMUNICATION_BUTTON_ID 
                ? "Localized String here"// todo: implement localization Helper.GetString("SendPictureMessage") 
                : Text);
        
    }
}
