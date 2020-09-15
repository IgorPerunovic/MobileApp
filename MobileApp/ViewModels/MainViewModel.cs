using MobileApp.Factories;
using MobileApp.Interfaces;
using MobileApp.Models;
using MobileApp.Services;
using MobileApp.Views;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using Plugin.Toast;
using Plugin.Toasts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace MobileApp.ViewModels
{
    public class MainViewModel : MvxViewModel
    {
        public List<Message> Interactables { get; /*maybe private?*/ set; }

        private void AddToInteractables(Message msg)
        {
            Interactables.Add(msg);
            //Settings.CurrentInteractables = Interactables;
        }

        private List<AucoboTab> tabs;
        public List<AucoboTab> Tabs
        {
            get { return tabs; }
            set { SetProperty(ref tabs, value); }
        }

        public MainViewModel()
        {
            Prepare();
            var changeTabsButton = new Button() { MinimumHeightRequest =150, MinimumWidthRequest = 150, Text="change tabs", BackgroundColor = Color.WhiteSmoke};
            changeTabsButton.Clicked += (s, e) => {
                ChangeTabsCommand.Execute();
            };

            var testToastButton = new Button() { MinimumHeightRequest = 150, MinimumWidthRequest = 150, Text = "test toast!", BackgroundColor = Color.WhiteSmoke };
            testToastButton.Clicked += (s, e) => {
                CrossToastPopUp.Current.ShowToastMessage("Message");

            };

            var testNotificationButton = new Button() { MinimumHeightRequest = 150, MinimumWidthRequest = 150, Text = "test notification!", BackgroundColor = Color.WhiteSmoke };
            testNotificationButton.Clicked += async (s, e) =>
            {
                var notificator = DependencyService.Get<IToastNotificator>();
                var result = await notificator.Notify(new NotificationOptions() { Title = "some title", Description = "My description!", IsClickable = false, AllowTapInNotificationCenter = false });
            };

            //var tabModel1 = new TabModel() { Title = "TAB 1", Id = "1" };
            //var tab1 = new AucoboTab(tabModel1) {BackgroundColor = Color.Red};
            //var sl = new StackLayout() { Children = { testToastButton, testNotificationButton, changeTabsButton } };

            //sl.HorizontalOptions = LayoutOptions.FillAndExpand;
            //sl.VerticalOptions= LayoutOptions.FillAndExpand;
            //tab1.Content = sl;
            // this.Tabs = new List<AucoboTab>() { tab1, new AucoboTab() { Title = "TAB 2", BackgroundColor = Color.Green }, new AucoboTab() { Title = "TAB 3", BackgroundColor = Color.Yellow } };
            PopulateTabs();

        }

        private void PopulateTabs()
        {
            this.Tabs = new List<AucoboTab>();
            foreach (TabModel t in Settings.Tabs)
            {
                Tabs.Add(new AucoboTab(t));
            }
        }

        private void ClearInteratables() 
        {
            this.Interactables = new List<Message>();
            //Settings.ClearInteractables();
        }

        public async override void Prepare()
        {
            //var config = await Helper.TryGetNewConfiguration(Constants.QR);
            //RabbitMQService.StartService();
            Console.WriteLine("Initializing, subscribing to message handler event");
            IMessageHandler mh = MessageHandlerFactory.GetMessageHandler;
            mh.TodoCreated += Mh_TodoCreated;

            // This is the first method to be called after construction
        }

        public override Task Initialize()
        {
            // Async initialization, YEY!
         
            return base.Initialize();

        }

        // still_todo: perhaps change to message received? Or handle separately?
        private void Mh_TodoCreated(object sender, TodoCreatedEventArgs e)
        {
            Console.WriteLine("MESSAGE ACCEPTED IN VM!");
            // filter interatables, do logic
            var toDo = e.Todo;

            // still_todo: ask for this logic, why? Resolve in AM-2683
            // still_todo: implement backend helper or something like that
            //if (toDo.Assignee == null && Interactables.Any(x => x != toDo && x.Blocking && !x.InteractionPending))
            //{
            //    //await BackendHelper.DeclineTodoAsync(toDo, true, false);
            //    toDo.RabbitEventHeader = "DECLINED_BECAUSE_BLOCKED";
            //}


            //if we added a new interactable
            Interactables.Add(e.Todo);
            Settings.Interactables = Interactables;
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
            Tabs = new List<AucoboTab>() { new AucoboTab() { Title = "TAB 4", BackgroundColor = Color.Red }, new AucoboTab() { Title = "TAB 5", BackgroundColor = Color.Green }, new AucoboTab() { Title = "TAB 6", BackgroundColor = Color.Yellow }, new AucoboTab() { Title = "TAB 7", BackgroundColor = Color.Gray } };
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
