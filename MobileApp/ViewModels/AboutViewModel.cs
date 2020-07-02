using System;
using System.Diagnostics;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace MobileApp.ViewModels
{
    public class AboutViewModel : BaseViewModel
    {
        public AboutViewModel()
        {
            Title = "About";
            OpenWebCommand = new Command(async () => await Browser.OpenAsync("https://xamarin.com"));
            TestCommand = new Command(() => {
                Debug.WriteLine("test command from VM");
            });
        }

        public ICommand OpenWebCommand { get; }

        public ICommand TestCommand { get; }
    }
}