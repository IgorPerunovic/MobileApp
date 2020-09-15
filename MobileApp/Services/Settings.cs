using aucobo;
using MobileApp.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Xamarin.Essentials;

namespace MobileApp.Services
{
    public static class Settings
    {
        // these internal methods are never used directly from outside the app, they deal with saving various available data types
        #region internal methods

        #region saveValues
        private static void saveString(string key, string value)
        {
            Preferences.Set(key, value);

        }

        private static void saveBool(string key, bool value)
        {
            Preferences.Set(key, value);
        }

        private static void saveDouble(string key, double value)
        {
            Preferences.Set(key, value);
        }

        private static void saveInt(string key, int value)
        {
            Preferences.Set(key, value);
        }

        private static void saveFloat(string key, float value)
        {
            Preferences.Set(key, value);
        }

        private static void saveLong(string key, long value)
        {
            Preferences.Set(key, value);
        }

        private static void saveDateTime(string key, DateTime value) 
        {
            Preferences.Set(key, value);
        }

        #endregion

        #region getValues
        private static string getString(string key)
        {
            return Preferences.Get(key, string.Empty);
        }

        private static bool getBool(string key)
        {
            return Preferences.Get(key, false);
        }

        private static double getDouble(string key) 
        {
            return Preferences.Get(key, 0.0);
        }

        private static int getInt(string key)
        {
            return Preferences.Get(key, 0);
        }

        private static float getFloat(string key)
        {
            return Preferences.Get(key, (float)0);
        }

        private static long getLong(string key)
        {
            return Preferences.Get(key, 0);
        }

        private static DateTime getDateTime(string key)
        {
            return Preferences.Get(key, DateTime.MinValue);
        }

        #endregion

        #endregion

        // constants and keys used for saving and getting values
        #region constants
        const string smartwatchKey = "Smart_Watch_Key";
        const string configurationKey = "Configuration_Key";
        const string interactablesKey = "Interactables_Key"; 
        #endregion

        // these methods are used as presented to get and save specific values that we use in the app
        #region externally available methods



        public static Smartwatch Smartwatch 
        { 
            get 
            {
                var smartWatchJSON = getString(smartwatchKey);
                var result = JsonConvert.DeserializeObject<Smartwatch>(smartWatchJSON);
                return result;
            }
            set 
            {
                if (!(value is Smartwatch)) { return; }
                saveString(smartwatchKey, JsonConvert.SerializeObject(value));
                
            }
            
        }
        
    
        public static ServerConfiguration Configuration 
        {
            get
            {
                var configurationJson = getString(configurationKey);
                var result = JsonConvert.DeserializeObject<ServerConfiguration>(configurationJson);
                return result;
            }
            set
            {
                if (!(value is ServerConfiguration)) { return; }
                saveString(configurationKey, JsonConvert.SerializeObject(value));
            }
        }

        public static List<TabModel> Tabs => Smartwatch.Owner.Tabs?? DefaultTabs;
        // still_todo: implement JWT class
        //public static JWT OauthToken { get; set; }

        private static List<TabModel> DefaultTabs => new List<TabModel>() { new TabModel() { Id = "1", Title = "Tab1!!" }, new TabModel() { Id = "2", Title = "Tab2!!" } };


        public static List<Message> Interactables
        {
            get
            {
                var interactables = getString(interactablesKey);
                var result = JsonConvert.DeserializeObject<List<Message>>(interactables);
                return result;
            }
            set
            {
                if (!(value is List<Message>)) { return; }
                saveString(interactablesKey, JsonConvert.SerializeObject(value));
            }

        }
        #endregion
    }
}