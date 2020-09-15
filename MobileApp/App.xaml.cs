using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using MobileApp.Services;
using MobileApp.Views;
using System.Diagnostics;
using MobileApp.Models;
using aucobo;
using System.Threading.Tasks;
using Plugin.Toasts;
using Plugin.Toast;

namespace MobileApp
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();

            DependencyService.Register<MockDataStore>();
            MainPage = new MainPage();
        }

        protected async override void OnStart()
        {
            Debug.WriteLine("app started");

           // var config = await Helper.TryGetNewConfiguration(Constants.QR);
            RabbitMQService.StartService();

            //await Task.Delay(5000);
            //CrossToastPopUp.Current.ShowToastMessage("Message");
            //var notificator = DependencyService.Get<IToastNotificator>();
            //var result = await notificator.Notify(new NotificationOptions() { Title = "some title", Description = "My description!", IsClickable = false, AllowTapInNotificationCenter = false });
            //Debug.WriteLine("result is: " + result.ToString());
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
