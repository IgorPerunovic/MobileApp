using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace MobileApp.Services
{
    public enum LogLevel { Start, Trace, Debug, Info, Warn, Error }

    public class Logger
    {
        // todo: check if we need this
        //public static PushNotification LastShownNotification;
        public static DateTimeOffset PushedPendingLogsTimestamp = DateTimeOffset.Now.AddDays(-7);
        static bool PendingLogs = true;

        public static async Task HandleEvent(LogLevel level, string message)
        {
            if (Settings.Smartwatch?.ID == null && level == LogLevel.Start) { return; }

            try
            {
                var logId = Guid.NewGuid().ToString();
                var profiles = Connectivity.ConnectionProfiles;
                if (profiles.Contains(ConnectionProfile.WiFi))
                {

                    var current = Connectivity.NetworkAccess;

                    
                    if (current == NetworkAccess.Internet)
                    {
                        // Connection to internet is available
                    }
                    // Active Wi-Fi connection.
                }


#if WINDOWS_UWP
                if (!Helper.IsForegroundApp)
                {
                    var data = await AppServiceBridge.SendData(new ValueSet() { [AppServiceConstants.RequestWifiStatus] = true });
                    if (data != null && data.TryGetValue(AppServiceConstants.WifiSsid, out var ssid) && ssid != null &&
                        data.TryGetValue(AppServiceConstants.WifiStrength, out var strength) && strength != null)
                    {
                        Helper.WifiSsid = ssid.ToString();
                        Helper.WifiStrength = strength.ToString();
                    }
                }
#elif __ANDROID__
                var wifiInfo = ((WifiManager)Application.Context.GetSystemService(Context.WifiService)).ConnectionInfo;
#endif

                var logObj = new LogEntry()
                {
                    AdditionalInfo = new Dictionary<string, string>()
                    {
#if WINDOWS_UWP
                        ["battery"] = PowerManager.RemainingChargePercent + " / " + Helper.AdjustedBatteryLevel,
                        ["uptime"] = Environment.TickCount.ToString(),
                        ["ip"] = Helper.IpAddress ?? NetworkInformation.GetHostNames()?.FirstOrDefault(x => x.IPInformation != null && x.Type == HostNameType.Ipv4)?.ToString() ?? "---",
                        ["wifiSsid"] = Helper.WifiSsid ?? "---",
                        ["wifiStrength"] = Helper.WifiStrength ?? "---",
#elif __ANDROID__
                        ["battery"] = ((BatteryManager)Application.Context.GetSystemService(Context.BatteryService)).GetIntProperty((int)BatteryProperty.Capacity).ToString(),
                        ["uptime"] = SystemClock.ElapsedRealtime().ToString(),
                        ["ip"] = Helper.IpAddress ?? "---",
                        ["proglovestate"] = ProGloveBroadcastReceiver.ScannerState,
                        ["wifiSsid"] = wifiInfo.SSID,
                        ["wifiStrength"] = WifiManager.CalculateSignalLevel(wifiInfo.Rssi, 6).ToString(), // range 0 - 5
#elif __IOS__
                        ["battery"] = UIDevice.CurrentDevice.BatteryLevel.ToString(), 
                        ["uptime"] = (NSProcessInfo.ProcessInfo.SystemUptime * 1000).ToString(), 
#endif
                    },
                    Level = level,
                    Message = message
                };

                if (level == LogLevel.Start)
                {
                    logObj.AdditionalInfo["apnToken"] = Settings.PushDeviceToken;
                    logObj.AdditionalInfo["appVersion"] = AppInfo.VersionString;
                    logObj.AdditionalInfo["manufacturer"] = DeviceInfo.Manufacturer;
                    logObj.AdditionalInfo["OSVersion"] = DeviceInfo.VersionString;
                    logObj.AdditionalInfo["deviceName"] = DeviceInfo.Name;
                    logObj.AdditionalInfo["deviceModel"] = DeviceInfo.Model;
#if WINDOWS_UWP
                    logObj.AdditionalInfo["os"] = "Windows";
                    logObj.AdditionalInfo["languageCode"] = Settings.DisplayLanguage.Replace('-', '_');
                    logObj.AdditionalInfo["objectClass"] = "Smartwatch";
                    logObj.AdditionalInfo["serialNumber"] = Helper.SerialNumber ?? "---";
#elif __ANDROID__
                    logObj.AdditionalInfo["os"] = "Android";
                    logObj.AdditionalInfo["languageCode"] = System.Globalization.CultureInfo.CurrentCulture.Name.Replace('-', '_');
                    logObj.AdditionalInfo["objectClass"] = Helper.IsSmartphone ? "Smartphone" : "Smartwatch";
                    logObj.AdditionalInfo["serialNumber"] = Build.Serial;
#elif __IOS__
                    logObj.AdditionalInfo["os"] = "iOS";
                    logObj.AdditionalInfo["languageCode"] = System.Globalization.CultureInfo.CurrentCulture.Name.Replace('-', '_');
                    logObj.AdditionalInfo["objectClass"] = "Smartphone";
#endif
                }

                if (LastShownNotification?.EventOriginId != null)
                {
                    logObj.AdditionalInfo["eventOriginId"] = LastShownNotification.EventOriginId;
                }

                var logJson = JsonConvert.SerializeObject(logObj);
#if DEBUG
                Helper.LogDebug("Log: " + message);
#endif
                var logEvent = new RealmLogEntry() { ID = logId, Json = logJson };
                if (level == LogLevel.Error) { DatabaseHelper.PushRealmItem(logEvent); }
                var success = await TrySendEvent(logJson);
                if (!success && level != LogLevel.Debug)
                {
                    if (level != LogLevel.Error) { DatabaseHelper.PushRealmItem(logEvent); }
                    PendingLogs = true;
                }
                else
                {
                    if (level == LogLevel.Error) { DatabaseHelper.RemoveRealmEventOfType<RealmLogEntry>(logId); }
                    await TrySendPendingEvents();
                }
            }
            catch { }
        }

        public static void Started(string message = null) => HandleEvent(LogLevel.Start, message).Forget();
        public static void Trace(string message = null) => HandleEvent(LogLevel.Trace, message).Forget();
        public static void Debug(string message = null) => HandleEvent(LogLevel.Debug, message).Forget();
        public static void Info(string message = null) => HandleEvent(LogLevel.Info, message).Forget();
        public static void Warn(string message = null) => HandleEvent(LogLevel.Warn, message).Forget();
        public static void Error(string message = null) => HandleEvent(LogLevel.Error, message).Forget();

        public static async Task TrySendPendingEvents()
        {
            // battery optimization in order to query database only when needed
            if (!PendingLogs || DateTimeOffset.Now - PushedPendingLogsTimestamp < TimeSpan.FromMinutes(5)) { return; }

            PushedPendingLogsTimestamp = DateTimeOffset.Now;
            var pendingEvents = DatabaseHelper.GetRealmItemsOfType<RealmLogEntry>()
                .Where(x => Helper.CurrentTimeMillis - x.ToObject().Timestamp > 5000) // log must be at least 5 seconds old
                .OrderBy(x => x.ToObject().Level); // high prio logs first

            var logsToSend = pendingEvents.Take(10); // send out max 10 items at once
            foreach (var pendingEvent in logsToSend)
            {
                var success = await TrySendEvent(pendingEvent.Json);
                if (!success) { return; }
                DatabaseHelper.RemoveRealmEventOfType<RealmLogEntry>(pendingEvent.ID);
            }

            PendingLogs = pendingEvents.Count() != logsToSend.Count();
        }

        public static async Task<bool> TrySendEvent(string json)
        {
            var resultMessage = await BackendHelper.SendLogEventAsync(json);
            if (resultMessage.IsSuccessStatusCode) { return true; }
            Helper.LogDebug("Error while sending log: " + resultMessage.ReasonPhrase + "  " + json);
            if (!resultMessage.ReasonPhrase.StartsWith("{")) { return false; }

            var logResponse = JsonConvert.DeserializeObject<ApiResponse>(resultMessage.ReasonPhrase,
                new JsonSerializerSettings { Error = (se, ev) => ev.ErrorContext.Handled = true });
            if (logResponse != null)
            {
                if (logResponse.Exception?.Contains("DeviceLogStartedNotReceivedException") == true)
                {
                    Started("After DeviceLogStartedNotReceivedException");
                }
            }
            return false;
        }
    }

}
