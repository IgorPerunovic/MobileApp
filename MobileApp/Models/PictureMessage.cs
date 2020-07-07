using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace MobileApp.Models
{
    public class PictureMessage : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyChanged(string name = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public byte[] Data { get; set; }

        public string SenderOwnerName { get; set; }

        public string SenderDeviceId { get; set; }

        public bool isReplyable => SenderDeviceId != null ? true : false;

        public static PictureMessage FromRabbitMessage(byte[] body, string senderDeviceId, string senderOwnerName)
        {
            return new PictureMessage()
            {
                Data = body,
                SenderDeviceId = senderDeviceId,
                SenderOwnerName = senderOwnerName
            };
        }

        // todo: see if we should implement this and how?
//#if WINDOWS_UWP
//        bool isShowing;
//        public ImageSource source;
//        public string ShowIcon => isShowing ? "" : "";

//        public async Task SetSource()
//        {
//            if (Data != null && Data.Length > 0)
//            {
//                var stream = new MemoryStream(Data).AsRandomAccessStream();
//                var bitmap = new BitmapImage();
//                await bitmap.SetSourceAsync(stream);
//                source = bitmap;
//            }
//        }
//        public bool CanShow { get; set; }
//#endif
    }
}
