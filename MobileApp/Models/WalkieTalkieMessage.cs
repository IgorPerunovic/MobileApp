using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace MobileApp.Models
{
    public class WalkieTalkieMessage : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyChanged(string name = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public byte[] Data { get; set; }

        public string SenderOwnerName { get; set; }

        public string SenderDeviceId { get; set; }

        public static WalkieTalkieMessage FromRabbitMessage(byte[] body, string senderDeviceId, string senderOwnerName)
        {
            return new WalkieTalkieMessage()
            {
                Data = body,
                SenderDeviceId = senderDeviceId,
                SenderOwnerName = senderOwnerName
            };
        }

        //still_todo: see if we should implement this and how
        //#if WINDOWS_UWP
        //        bool isPlaying;
        //        public MediaElement MediaElement;
        //        public string PlayIcon => isPlaying ? "" : "";
        //        public bool CanPlay { get; set; }

        //        public void MediaEnded(object sender, RoutedEventArgs e)
        //        {
        //            isPlaying = false;
        //            NotifyChanged(nameof(PlayIcon));
        //        }

        //        public void PlayMedia(object sender, RoutedEventArgs e)
        //        {
        //            if (isPlaying) { MediaElement.Stop(); }
        //            else { MediaElement.Play(); }
        //            isPlaying = !isPlaying;
        //            NotifyChanged(nameof(PlayIcon));
        //        }

        //        public void MediaOpened(object sender, RoutedEventArgs e)
        //        {
        //            CanPlay = true;
        //            NotifyChanged(nameof(CanPlay));
        //        }
        //#endif
    }
}
