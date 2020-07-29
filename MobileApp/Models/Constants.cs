using System;
using System.Collections.Generic;
using System.Text;

namespace MobileApp.Models
{
    public static class Constants
    {
        public static string QR = "https://develop.aucobo-global.de/api/devices/device-qr-config/8d7f4df4-38bb-46ff-8493-7c7dc08a48e9";

        public const int RESET_PIN = 3697;
        public const int DEFAULT_INFONOTIFICATION_DURATIONMILLIS = 2000;
        public const int INFONOTIFICATION_PERMANENT = 0; // no timeout
        public const string MEDIA_SEND_PENDING = "MEDIA_SEND_PENDING";

        public const string WALKIE_TALKIE_BUTTON_ID = "walkie_talkie";
        public const string PICTURE_COMMUNICATION_BUTTON_ID = "picture_btn";
        public const string AUDIORECORDFORMAT = "mm':'ss'.'ff";

        public const string AUCOBO_CLASS_HEADER = "aucoboClass";
        
    }
}
