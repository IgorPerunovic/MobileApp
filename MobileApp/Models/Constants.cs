using System;
using System.Collections.Generic;
using System.Text;

namespace MobileApp.Models
{
    public static class Constants
    {
        public static string QR = "https://develop.aucobo-global.de/api/devices/device-qr-config/31ec9530-20c7-4a20-8153-348b3fc110b5";

        public const int RESET_PIN = 3697;
        public const int DEFAULT_INFONOTIFICATION_DURATIONMILLIS = 2000;
        public const int INFONOTIFICATION_PERMANENT = 0; // no timeout
        public const string MEDIA_SEND_PENDING = "MEDIA_SEND_PENDING";

        public const string WALKIE_TALKIE_BUTTON_ID = "walkie_talkie";
        public const string PICTURE_COMMUNICATION_BUTTON_ID = "picture_btn";

        #region message aucobo class identifiers
        public const string AUCOBO_CLASS_HEADER = "aucoboClass";
        public const string WALKIE_TALKIE_MESSAGE_AUCOBO_CLASS = "walkie_talkie";
        public const string PICTURE_COMMUNICATION_MESSAGE_AUCOBO_CLASS = "picture_btn";
        public const string TO_DO_MESSAGE_AUCOBO_CLASS = "ToDo";
        #endregion

        public const string AUDIORECORDFORMAT = "mm':'ss'.'ff";


        
    }
}
