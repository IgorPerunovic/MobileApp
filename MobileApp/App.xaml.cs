﻿using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using MobileApp.Services;
using MobileApp.Views;
using System.Diagnostics;
using MobileApp.Models;

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
            //var config = await Helper.TryGetNewConfiguration(Constants.QR);
            var config = true;

            Settings.SaveConfig(config);
            var configAgain = Settings.GetConfig();

            Debug.WriteLine("configuration is: " + configAgain);

        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}