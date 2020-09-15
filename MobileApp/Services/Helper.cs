using MobileApp.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MobileApp.Services
{
    public class Helper
    {

        public readonly static TimeSpan HttpTimeout = TimeSpan.FromSeconds(5);
        public static HttpClient HttpClient = new HttpClient(new HttpClientHandler()) { Timeout = HttpTimeout };

        public static async Task<bool> TryGetNewConfiguration(string text)
        {
            try
            {
                // ShowToast(GetString("FetchingConfiguration"));
                var response = await HttpClient.GetAsync(text);
                Debug.WriteLine("response is: " + response);
                if (response.IsSuccessStatusCode)
                {
                    text = await response.Content.ReadAsStringAsync();
                    var result = await TrySetNewWatch(JsonConvert.DeserializeObject<SmartwatchDto>(text));
                    
                    return result;
                }
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    Debug.WriteLine("404, not found");
                   // ShowToast(GetString("ConfigurationFetchError"));
                   // await Task.Delay(2000); // let notification fire
                }
                else
                {
                    //ShowToast($"{GetString("UnexpectedError")}! Error code {response.StatusCode}: {response.ReasonPhrase}");
                    await Task.Delay(2000); // let notification fire
                }
            }
            catch(Exception e) { Debug.WriteLine("exception: " + e.Message); }
            // still_TODO: handle these errors in a user-friendly way
            //#if __ANDROID__
            //            catch (Java.Net.UnknownHostException) { ShowToast(GetString("HttpError")); }
            //#endif
            //            catch (HttpRequestException) { 
            //                //ShowToast(GetString("HttpError")); 
            //            }
            //            catch (TaskCanceledException) { ShowToast(GetString("TaskCanceledError")); }
            //            catch (Exception) when (!text.StartsWith("https") && !text.StartsWith("http")) { ShowToast(GetString("ConfigScannedWrongCode")); }
            //            catch (Exception ex) { ShowToast(GetString("UnexpectedError") + ": " + ex.Message); }
            return false;
        }

        public static async Task<bool> TrySetNewWatch(SmartwatchDto smartwatchDto)
        {
            Debug.WriteLine("ID is: " + smartwatchDto.ID);

            // still_TODO: decipher this and reuse the code where posible
            var oldConfig = Settings.Configuration;
            var oldWatch = Settings.Smartwatch;
            Settings.Smartwatch = smartwatchDto; // Set smartwatch so watch can be registered
            Debug.WriteLine("smartwatch: " + Settings.Smartwatch.Owner + " " + Settings.Smartwatch.ID);

            //still_todo: check if this is a good place to do this?
            Settings.Configuration = smartwatchDto.Configuration;

            return true;


            //            ShowToast(GetString("RegisteringSmartwatch"));
            //            if (await BackendHelper.RegisterWatchAsync() is Smartwatch newWatch)
            //            {
            //                ShowToast(GetString("ApplyingSmartwatchConfig"));
            //                Settings.SetSmartwatch(newWatch);
            //                if (RabbitMQService.RestartService(true))
            //                {
            //                    var sentSuccessfully = true;
            //#if WINDOWS_UWP
            //                    sentSuccessfully = await AppServiceBridge.SendData(new ValueSet()
            //                    {
            //                        [AppServiceConstants.Smartwatch] = JsonConvert.SerializeObject(newWatch),
            //                        [AppServiceConstants.Configuration] = JsonConvert.SerializeObject(Settings.Configuration),
            //                    }) != null; // only accept new config if other task receives it too
            //#endif
            //                    if (sentSuccessfully)
            //                    {
            //                        DatabaseHelper.Current.ResetDatabase(false);
            //                        Settings.SetSmartwatchConfig(SmartwatchDto.Create(newWatch, smartwatchDto.Configuration));
            //#if !WINDOWS_UWP
            //                        Task.Run(() => RabbitMQService.StartService()).Forget();
            //#endif
            //                        Logger.Started("New config");
            //                        ShowToast(GetString("ConfigureWatchSuccess"));
            //                        return true;
            //                    }
            //                    else
            //                    {
            //                        ShowToast(GetString("ErrorSendingConfigToBackgroundTask"));
            //                        await Task.Delay(3000); // delay so notification stays visible for at least 3 seconds
            //                    }
            //                }
            //                else
            //                {
            //                    ShowToast(GetString("RabbitConnectionError"));
            //                    await Task.Delay(3000); // delay so notification stays visible for at least 3 seconds
            //                }
            //            }

            //            Settings.SetSmartwatch(oldWatch);
            //            Settings.SetConfiguration(oldConfig);
            //            Task.Run(() => RabbitMQService.RestartService()).Forget();
            //            ShowToast(GetString("ConfigureWatchNoSuccess"));
            //            return false;
        }


        public static long CurrentTimeMillis => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    }
}
