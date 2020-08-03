using MobileApp.ViewModels;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MobileApp.Views
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : TabbedPage
    {
        public MainPage()
        {
            InitializeComponent();
            var bc = new MainViewModel();
            this.BindingContext = bc;
            //this.ItemsSource = bc.Tabs;

            changeTabs();
        }

        private async void changeTabs() {
            await Task.Delay(10000);
            var bc = (MainViewModel)this.BindingContext;
            bc.ChangeTabsCommand.Execute();
        }
    }
}