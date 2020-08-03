using MvvmCross.Commands;
using MvvmCross.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace MobileApp.ViewModels
{
    public class MainViewModel : MvxViewModel
    {
        public MainViewModel()
        {
            this.Tabs = new List<NavigationPage>() { new NavigationPage() { Title = "TAB 1", BackgroundColor = Color.Red }, new NavigationPage() { Title = "TAB 2", BackgroundColor = Color.Green }, new NavigationPage() { Title = "TAB 3", BackgroundColor = Color.Yellow } };
        }

        public override void Prepare()
        {
            // This is the first method to be called after construction
        }

        public override Task Initialize()
        {
            // Async initialization, YEY!

            return base.Initialize();
        }

        public override void ViewAppeared()
        {
            base.ViewAppeared();
            Task.Delay(10000);
            ResetTextCommand.Execute();
        }

        public IMvxCommand ResetTextCommand => new MvxCommand(ResetText);

        private void ResetText()
        {
            Text = "Some text that's been changed";
        }


        public IMvxCommand ChangeTabsCommand => new MvxCommand(ChangeTabs);

        private void ChangeTabs()
        {

            Tabs.Clear();
            Tabs = new List<NavigationPage>() { new NavigationPage() { Title = "TAB 4", BackgroundColor = Color.Red }, new NavigationPage() { Title = "TAB 5", BackgroundColor = Color.Green }, new NavigationPage() { Title = "TAB 6", BackgroundColor = Color.Yellow }, new NavigationPage() { Title = "TAB 7", BackgroundColor = Color.Gray } };
        }
        private List<NavigationPage> tabs;
        public List<NavigationPage> Tabs 
        {
            get { return tabs; }
            set { SetProperty(ref tabs, value); }
        } 
        
       //=> new List<NavigationPage>() { new NavigationPage() { Title = "TAB 1", BackgroundColor = Color.Red }, new NavigationPage() { Title = "TAB 2", BackgroundColor = Color.Green }, new NavigationPage() { Title = "TAB 3", BackgroundColor = Color.Yellow } };

       

        private string _text = "Hello MvvmCross";
        public string Text
        {
            get { return _text; }
            set { SetProperty(ref _text, value); }
        }
    }

}
