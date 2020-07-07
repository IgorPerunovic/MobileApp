using System;
using System.Collections.Generic;
using System.Text;

namespace MobileApp.Models
{
    public static class Constants
    {
        public static string QR = "http://develop.aucobo-global.de/api/devices/device-qr-config/0bb531a1-856b-4ec3-9bfc-1cac04b5dab6";

        public const int RESET_PIN = 3697;
        public const int DEFAULT_INFONOTIFICATION_DURATIONMILLIS = 2000;
        public const int INFONOTIFICATION_PERMANENT = 0; // no timeout
        public const string MEDIA_SEND_PENDING = "MEDIA_SEND_PENDING";

        public const string WALKIE_TALKIE_BUTTON_ID = "walkie_talkie";
        public const string PICTURE_COMMUNICATION_BUTTON_ID = "picture_btn";
        public const string AUDIORECORDFORMAT = "mm':'ss'.'ff";

    }
}
