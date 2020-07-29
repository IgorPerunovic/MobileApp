using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MobileApp.Services;
using MobileApp.Models;
using MobileApp.Models;
using Xamarin.Essentials;
using MobileApp.Factories;

#if WINDOWS_UWP
using Windows.System.Power;
using Windows.Foundation.Collections;
using Realms;
#elif __ANDROID__
using Xamarin.Essentials;
using Android.App;
using Android.Content;
#endif

namespace aucobo
{
    public static class RabbitMQService
    {
        static readonly SemaphoreSlim connectSema = new SemaphoreSlim(1);
        static readonly SemaphoreSlim toDoSema = new SemaphoreSlim(1);
        static bool pendingRabbitMessages = true;
        static IConnection rabbitConnection = null;
        static IModel rabbitChannel = null;

        private static bool isConnected;
        public static bool IsConnected => rabbitConnection?.IsOpen ?? false; //(rabbitConnection != null) && rabbitConnection.IsOpen;
//        {
//            get => isConnected;
//            private set
//            {
//                isConnected = value;
//                // todo: check if we need this in Xamarin.Forms
////#if WINDOWS_UWP
////                AppServiceBridge.SignalRabbitConnection();
////#endif
//            }
//        }

        public static void StopService() => rabbitConnection?.Abort(TimeSpan.FromSeconds(2)); // Abort() is like Close() but ignoring possible errors/exceptions

        public static bool RestartService(bool testConnection = false, bool checkForBatteryState = true)
        {
            // todo: check if we need this in Xamarin.Forms

//#if WINDOWS_UWP
//            if (Helper.IsForegroundApp && !testConnection) { return false; }
//#endif
            StopService();
            return StartService(testConnection, checkForBatteryState);
        }

        public static bool StartService(bool testConnection = false, bool checkForBatteryState = true)
        {
            connectSema.Wait();  //semaphore to block entry so we don't open multiple connections accidentally
            // ALWAYS RELEASE SEMAPHORE BEFORE EXITING CODE!!
            if (rabbitConnection.IsOpen) 
            {
                connectSema.Release(); 
                return true; 
            }
            
            // todo: check if battery state is a good condition for this?
            if ((!testConnection && checkForBatteryState && Battery.State != BatteryState.Discharging) || Settings.Smartwatch?.ID == null)
            {
                connectSema.Release();
                return false;
            }
          
            try
            {
                var queue = Settings.Smartwatch.ID;
                rabbitConnection = new ConnectionFactory()
                {
                    
                    Ssl = new SslOption()
                    {
                        Enabled = Settings.Configuration.RabbitPort == "5673", //todo: check if condition proper, move to constant
                        ServerName = Settings.Configuration.AucoboIP,
                    },
                    HostName = Settings.Configuration.RabbitIP,
                    Port = int.Parse(Settings.Configuration.RabbitPort),
                    UserName = Settings.Configuration.RabbitUserName,
                    Password = Settings.Configuration.RabbitPassword,
                    RequestedConnectionTimeout = TimeSpan.FromSeconds(3), //todo: check why 3 seconds?
                    AutomaticRecoveryEnabled = false, // we handle that by ourself, as it is less error-prone
                    RequestedHeartbeat = TimeSpan.FromSeconds(10)
                  
                    //todo: check if only on Android?
//#if __ANDROID__
//                    RequestedHeartbeat = TimeSpan.FromSeconds(10),
//#endif
                }.CreateConnection(queue);

                rabbitConnection.ConnectionShutdown += (s, e) =>
                {
                    if (!testConnection) 
                    { 
                        // todo: implement as needed
                    //    Logger.Warn("Rabbit connection shut down: " + e.Cause + ", " + e.ReplyText + ", " + e.ReplyCode); 
                    }
                    rabbitConnection = null;
                    rabbitChannel = null;
                    if (e.ReplyCode != RabbitMQ.Client.Constants.ReplySuccess) // 200
                    {
                            StartService();
                    }

                    // todo: clean up if above code works!
                    // original:
                    //if (e.ReplyCode != RabbitMQ.Client.Constants.ReplySuccess) // 200
                    //{
                    //    Task.Run(async () =>
                    //    {
                    //        await Task.Delay(1000); // todo: check why? what happens if this doesn't happen like this?
                    //        StartService();
                    //    });
                    //}
                    //rabbitConnection = null;
                    //rabbitChannel = null;
                };
                rabbitChannel = rabbitConnection.CreateModel();

                var consumer = new EventingBasicConsumer(rabbitChannel);
                if (!testConnection) {
                    // todo: check this and implement event
                    consumer.Received += (s, e) => MessageHandlerFactory.GetMessageHandler.HandleRabbitMessage(s, e);  //Consumer_Received; 
                }
                var consumerTag = string.Empty;
                // todo: figure out the logic to send appropriate tag (just check values)
                switch (Xamarin.Forms.Device.RuntimePlatform)
                {
                    case Xamarin.Forms.Device.iOS:
                        consumerTag = "windows smartwatch";
                        break;
                    case Xamarin.Forms.Device.Android:
                        consumerTag = "android ";// todo: implement helper + (Helper.IsSmartphone ? "phone" : "watch");
                        break;
                    case Xamarin.Forms.Device.UWP:
                        consumerTag = "iPhone"; // todo: check if it's iPhone or iOS, maybe?
                        break;
                }


                var tag = rabbitChannel.BasicConsume(queue, false, consumerTag + " " + queue, consumer);
                if (testConnection)
                {
                    rabbitChannel.BasicCancel(tag);
                    //  all went fine, so now stop this service again so unacked received notifications are rebroadcasted
                    StopService();
                    return true;
                }

                // todo: implement if needed
                //Logger.Debug("RabbitMQ connect success");
            }
            catch (Exception ex)
            {
                // todo: implement if needed
                //Logger.Debug("RabbitMQ connect error: " + ex.Message);
                StopService();
                return false;
            }
            finally { connectSema.Release(); }
            return true;
        }

        public static bool SendMessage(string exchange, string actionType, string json, string routingKey = null)
        {
            try
            {
                if (IsConnected) // TODO should this also work when charging / not connected?
                {
                    var props = rabbitChannel.CreateBasicProperties();
                    props.Expiration = TimeSpan.FromMinutes(5).TotalMilliseconds.ToString();
                    props.ContentType = "text/plain";
                    props.DeliveryMode = 1; // non-persistent
                    props.Headers = new Dictionary<string, object>()
                    {
                        { "senderId", Settings.Smartwatch.Owner.ID },
                        { "deviceId", Settings.Smartwatch.ID },
                    };

                    //todo: implement if needed
                   // Helper.LogDebug("Sending message to rabbit: " + json);
                    rabbitChannel.BasicPublish(exchange, routingKey ?? $"{actionType}.{Settings.Smartwatch.Owner.ID}", props, Encoding.UTF8.GetBytes(json));
                    return true;
                }
            }
            catch (Exception e)
            {
                //todo: implement if needed
                //Logger.Error("RabbitMQHelper.SendMessage: " + e);
            }
            return false;
        }

        public static void SendMedia(byte[] data, Device targetDevice)
        {
            try
            {
#if WINDOWS_UWP
                        if (Helper.IsForegroundApp)
                        {
                            AppServiceBridge.SendData(new ValueSet()
                            {
                                [AppServiceConstants.WalkieTalkieData] = data,
                                [AppServiceConstants.WalkieTalkieSenderDeviceId] = targetDevice.ID,
                            }).Forget();
                            return;
                        }
#endif
                if (IsConnected) // TODO should this also work when charging / not connected?
                {
                    var props = rabbitChannel.CreateBasicProperties();
                    props.Expiration = TimeSpan.FromMinutes(1).TotalMilliseconds.ToString();
                    props.ContentType = "audio/aac";
                    props.DeliveryMode = 1; // non-persistent
                    props.Headers = new Dictionary<string, object>()
                            {
                                { "senderDeviceId", Settings.Smartwatch.ID },
                                { "senderOwnerName", Settings.Smartwatch.Owner.FullName },
                                { "aucoboClass", ""/*Constants.WALKIE_TALKIE_BUTTON_ID*/ },
                            };

                    rabbitChannel.BasicPublish("", targetDevice.ID, props, data);
                }
                return;
            }
            catch (Exception e) 
            {
                //todo: implement if needed
                //Logger.Error("RabbitMQHelper.SendMedia: " + e); 
            }
            //todo: implement if needed
            //Helper.ShowToast("Sending audio message failed");
        }

        public static void SendPicture(byte[] data, Device targetDevice)
        {
            try
            {
                /*
                //todo: check if needed and implement?
#if WINDOWS_UWP
                        if (Helper.IsForegroundApp)
                        {
                            AppServiceBridge.SendData(new ValueSet()
                            {
                                [AppServiceConstants.PictureData] = data,
                                [AppServiceConstants.PictureSenderDeviceId] = targetDevice.ID,
                            }).Forget();
                            return;
                        }
#endif
                */
                if (IsConnected) // TODO should this also work when charging / not connected?
                {
                    var props = rabbitChannel.CreateBasicProperties();
                    props.Expiration = TimeSpan.FromMinutes(1).TotalMilliseconds.ToString();
                    props.ContentType = "image/jpeg";
                    props.DeliveryMode = 1; // non-persistent
                    props.Headers = new Dictionary<string, object>()
                            {
                                { "senderDeviceId", Settings.Smartwatch.ID },
                                { "senderOwnerName", Settings.Smartwatch.Owner.FullName },
                                { "aucoboClass", "" /*Constants.PICTURE_COMMUNICATION_BUTTON_ID*/ },
                            };

                    rabbitChannel.BasicPublish("", targetDevice.ID, props, data);
                }
                return;
            }
            catch (Exception e)
            {
                //    Logger.Error("RabbitMQHelper.SendMedia: " + e); 
            }
            //  Helper.ShowToast("Sending audio message failed");
        }

        static readonly SemaphoreSlim sendPendingMsgsSema = new SemaphoreSlim(1);

        /*
        public static async Task TrySendPendingMessages()
        {
            ////battery optimization in order to query database only when needed
            if (pendingRabbitMessages == false || sendPendingMsgsSema.Wait(0) == false) { return; }
            try
            {
                var messagesToProcess = DatabaseHelper.GetRealmItemsOfType<RabbitMessage>().ToList();

#if WINDOWS_UWP
                var toDoMessagesToProcess = messagesToProcess
                    .Where(x => x.GetHeadersDict().TryGetValue("aucoboClass", out var aucoboClass) && aucoboClass == "ToDoProcessed")
                    .ToList();
                var interactionsToProcess = messagesToProcess
                    .Where(x => x.GetHeadersDict().TryGetValue("aucoboClass", out var aucoboClass) && aucoboClass == "Interaction")
                    .ToList();
                if (toDoMessagesToProcess.Count > 0 || interactionsToProcess.Count > 0)
                {
                    var toDos = toDoMessagesToProcess.Select(x => JsonConvert.DeserializeObject<ToDo>(x.Message)).ToList();
                    var interactions = interactionsToProcess.Select(x => JsonConvert.DeserializeObject<Interaction>(x.Message)).ToList();
                    toDos.ForEach(x => x.SerializeRabbitEventHeader = true);
                    interactions.ForEach(x => x.SerializeRabbitEventHeader = true);
                    if (await AppServiceBridge.SendData(new ValueSet()
                    {
                        [AppServiceConstants.ToDos] = JsonConvert.SerializeObject(toDos),
                        [AppServiceConstants.Interactions] = JsonConvert.SerializeObject(interactions)
                    }) == null) { return; }

                    messagesToProcess = messagesToProcess.Except(toDoMessagesToProcess).Except(interactionsToProcess).ToList();
                    using var realm = Realm.GetInstance();
                    realm.Write(() =>
                    {
                        toDoMessagesToProcess.ForEach(x => realm.Remove(realm.Find<RabbitMessage>(x.ID)));
                        interactionsToProcess.ForEach(x => realm.Remove(realm.Find<RabbitMessage>(x.ID)));
                    });
                }
#endif

                foreach (var msg in messagesToProcess)
                {
                    var success = await ProcessRabbitMessage(msg);
                    if (success) { DatabaseHelper.RemoveRealmEventOfType<RabbitMessage>(msg.ID); }
                    else
                    {
                        Logger.Debug("TrySendPendingMessages failed");
                        return;
                    }
                }

                pendingRabbitMessages = false;
            }
            catch (Exception ex)
            {
                Logger.Error("TrySendPendingMessages: " + ex);
            }
            finally { sendPendingMsgsSema.Release(); }
        }
        */






        async static void Consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            MessageHandlerFactory.GetMessageHandler.HandleRabbitMessage(sender,e);
        }






        //todo: Implement this properly when you manage login and getting messages


        /*
        async static Task<bool> ProcessRabbitMessage(RabbitMessage msg)
        {
            var headers = msg.GetHeadersDict();
            Helper.LogDebug("Received message: " + string.Join(Environment.NewLine, headers.Select(kvp => kvp.Key + ": " + kvp.Value.ToString()))
                + Environment.NewLine + "message: " + msg.Message + Environment.NewLine);
            if (!headers.ContainsKey("aucoboClass")) { return true; }
            //Helper.LogDebug("Received message of type " + headers["aucoboClass"] + ": " + msg.Message);
            switch (headers["aucoboClass"])
            {
                case "ToDo":
                    await toDoSema.WaitAsync();
                    try
                    {
                        var toDo = JsonConvert.DeserializeObject<ToDo>(msg.Message);
                        if (headers.TryGetValue("event", out var ev)) { toDo.RabbitEventHeader = ev; }

                        if (ev != null && ev != "ASSIGNED" && ev != "CREATED" && ev != "ACCEPTED" && ev != "DONE") { return true; } // we dont care about other events
                        if (ev != null && ev != "ASSIGNED" && toDo.AssignedToCurrentUser) { return true; } // the current user initiated this update -> ignore

        #if WINDOWS_UWP
                                        var status = await Helper.GetDoNotDisturbStatusAsync();
                                        if (toDo.Assignee == null && status.BlockingTodoId != null && status.BlockingTodoId != toDo.ID)
                                        {
                                            await BackendHelper.DeclineTodoAsync(toDo, true, false);
                                            toDo.RabbitEventHeader = "DECLINED_BECAUSE_BLOCKED";
                                        }

                                        headers["aucoboClass"] = "ToDoProcessed";
                                        toDo.SerializeRabbitEventHeader = true;
                                        msg.Headers = JsonConvert.SerializeObject(headers);
                                        msg.Message = JsonConvert.SerializeObject(toDo);

                                        if (status.DoNotDisturb && status.BlockingTodoId != toDo.ID) { return false; }
                                        var displayOff1 = Helper.IsDisplayOff();
                                        if (Settings.NotificationDisplayOn || !displayOff1)
                                        {
                                            if (Settings.NotificationDisplayOn && displayOff1) { Helper.WakeupApp(); }
                                            return await AppServiceBridge.SendData(new ValueSet() { [AppServiceConstants.ToDo] = JsonConvert.SerializeObject(toDo) }) != null;
                                        }
                                        else
                                        {
                                            Helper.Vibrate(toDo);
                                            return false;
                                        }
        #else
                        if (toDo.Assignee == null && DatabaseHelper.Current.LocalTodos.Any(x => x != toDo && x.Blocking && !x.InteractionPending))
                        {
                            await BackendHelper.DeclineTodoAsync(toDo, true, false);
                            toDo.RabbitEventHeader = "DECLINED_BECAUSE_BLOCKED";
                        }
                        Helper.HandleTodo(toDo);
                        return true;
        #endif
                    }
                    finally { toDoSema.Release(); }
                case "PushNotification":
        #if WINDOWS_UWP
                                    if ((await Helper.GetDoNotDisturbStatusAsync()).DoNotDisturb) { return false; }
                                    var displayOff2 = Helper.IsDisplayOff();
                                    if (Settings.NotificationDisplayOn || !displayOff2)
                                    {
                                        if (Settings.NotificationDisplayOn && displayOff2) { Helper.WakeupApp(); }
                                        return await AppServiceBridge.SendData(new ValueSet() { [AppServiceConstants.PushNotification] = msg.Message }) != null;
                                    }
                                    else
                                    {
                                        var notification = JsonConvert.DeserializeObject<PushNotification>(msg.Message);
                                        Helper.Vibrate(notification);
                                        return false;
                                    }
        #else
                    //check blocking todo
                    if (DatabaseHelper.Current.LocalTodos.Any(x => x.Blocking && !x.InteractionPending)) { return false; }
                    Helper.ShowNotification(JsonConvert.DeserializeObject<PushNotification>(msg.Message));
                    return true;
        #endif
                case "Buttons":
        #if WINDOWS_UWP
                                    AppServiceBridge.SendData(new ValueSet() { [AppServiceConstants.Buttons] = msg.Message }).Forget();
        #else
                    Settings.UpdateButtons(JsonConvert.DeserializeObject<List<AucoboButton>>(msg.Message));
        #endif
                    return true; // even on failure, fire and forget
                case "User":
        #if WINDOWS_UWP
                                    var userSentResult = await AppServiceBridge.SendData(new ValueSet() { [AppServiceConstants.NewUser] = msg.Message });
                                    if (userSentResult != null) { Settings.UpdateOwner(JsonConvert.DeserializeObject<User>(msg.Message)); }
                                    return userSentResult != null;
        #else
                    Settings.UpdateOwner(JsonConvert.DeserializeObject<User>(msg.Message));
                    return true;
        #endif
                case "Configuration":
                    var newConfig = JsonConvert.DeserializeObject<Configuration>(msg.Message);
                    if (Settings.Configuration == null || JsonConvert.SerializeObject(newConfig) == JsonConvert.SerializeObject(Settings.Configuration)) { return true; }
                    Task.Run(async () => // offload in new task so rabbit message can be immediately ack'ed
                    {
                        await Task.Delay(1000); // give some time to ack rabbit message
                                var success = await Helper.TrySetNewWatch(SmartwatchDto.Create(Settings.Smartwatch, newConfig));
                        if (success) { Task.Run(() => RestartService()).Forget(); }
                    }).Forget();
                    return true;
                case "Interaction":
                    var interaction = JsonConvert.DeserializeObject<Interaction>(msg.Message);
                    if (headers.TryGetValue("event", out var ev2)) { interaction.RabbitEventHeader = ev2; }
        #if WINDOWS_UWP
                                    if ((await Helper.GetDoNotDisturbStatusAsync()).DoNotDisturb) { return false; }
                                    var displayOff3 = Helper.IsDisplayOff();
                                    if (Settings.NotificationDisplayOn || !displayOff3)
                                    {
                                        if (Settings.NotificationDisplayOn && displayOff3) { Helper.WakeupApp(); }
                                        interaction.SerializeRabbitEventHeader = true;
                                        return await AppServiceBridge.SendData(new ValueSet() { [AppServiceConstants.Interaction] = JsonConvert.SerializeObject(interaction) }) != null;
                                    }
                                    else
                                    {
                                        Helper.Vibrate();
                                        return false;
                                    }
        #else
                    Helper.HandleInteraction(interaction);
                    return true;
        #endif
                case "Tabs":
        #if WINDOWS_UWP
                                    AppServiceBridge.SendData(new ValueSet() { [AppServiceConstants.Tabs] = msg.Message }).Forget();
        #elif __ANDROID__
                                    Settings.Tabs = JsonConvert.DeserializeObject<List<string>>(msg.Message);
                                    Application.Context.StartActivity(new Intent(Application.Context, typeof(MainActivity))
                                        .SetFlags(ActivityFlags.ClearTask | ActivityFlags.NewTask));
        #endif
                    return true;
                case "Scandit":
        #if WINDOWS_UWP
                                    AppServiceBridge.SendData(new ValueSet() { [AppServiceConstants.ScanditLicense] = msg.Message }).Forget();
        #elif __ANDROID__
                                    Settings.ScanditLicense = JsonConvert.DeserializeObject<ScanningLicenseKey>(msg.Message).License;
                                    Logger.Info((Settings.ScanditLicense == null ? "removed" : "added") + " Scandit license key");
        #endif
                    return true;
                case "Honeywell":
        #if WINDOWS_UWP
                                    AppServiceBridge.SendData(new ValueSet() { [AppServiceConstants.HoneywellLicense] = msg.Message }).Forget();
        #elif __ANDROID__
                                    Settings.HoneywellLicense = JsonConvert.DeserializeObject<ScanningLicenseKey>(msg.Message).License;
                                    Logger.Info((Settings.HoneywellLicense == null ? "removed" : "added") + " Honeywell license key");
        #endif
                    return true;
                default:
                    Logger.Warn("Unknown Rabbit Message Class \"" + headers["aucoboClass"] + "\"");
                    //throw new Exception("Unknown Rabbit Message Class \"" + headers["aucoboClass"] + "\"");
                    return true; // ack message so it doesn't bother us any more
            }
        }
        */


        /*
    public static Task<bool> SendInteractionResult(Interaction interaction, double result)
        => SendInteractionResult(interaction, result.ToString("F" + interaction.Properties["comma"], CultureInfo.InvariantCulture));
        */

        /*   public static async Task<bool> SendInteractionResult(Interaction interaction, string result)
           {
               if (interaction.Type == Interaction.TYPE_PHOTO && result == Constants.MEDIA_SEND_PENDING)
               {
                   var media = DatabaseHelper.GetMedia(interaction.ID);
                   try { result = await BackendHelper.SendMediaAsync(media); }
                   catch (Exception e)
                   {
                       Logger.Error("SendMediaAsync: " + e);
                       Interaction.OnInteractionFinished(false);
                       return false;
                   }
               }

               interaction.Output = result;
               interaction.Initiator ??= Settings.Smartwatch?.Owner;
               interaction.MetaTags[interaction.Key] = interaction.Output;
               interaction.PreviousInteractions ??= new Dictionary<string, PreviousInteraction>();
               interaction.PreviousInteractions[interaction.ID] = new PreviousInteraction()
               {
                   Step = interaction.PreviousInteractions.Count + 1,
                   Key = interaction.Key,
                   Output = interaction.Output
               };

   #if WINDOWS_UWP
                       var success = await Helper.SendRabbitMessage("interaction.output", interaction.InteractionSet + "." + interaction.ID, interaction);
   #else
               await Task.CompletedTask;
               var success = SendMessage("interaction.output", interaction.InteractionSet + "." + interaction.ID, JsonConvert.SerializeObject(interaction));
   #endif

               Helper.ShowToast(Helper.GetString(success ? "InteractionResultSendingSuccess" : "InteractionResultSendingFailure"));
               if (success) { DatabaseHelper.Current.RemoveInteraction(interaction); }
               else { DatabaseHelper.Current.SaveInteraction(interaction, false); }
               Interaction.OnInteractionFinished(false);
               return success;
           }
       */
    }
}
