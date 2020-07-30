using MobileApp.Interfaces;
using MobileApp.Models;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MobileApp.Services
{
    // this is a singleton class
    public class MessageHandler : IMessageHandler
    {
        private static readonly MessageHandler instance = new MessageHandler();

        private MessageHandler() { }

        public static MessageHandler Instance {
            get {
                return instance;
            }
        }

        public void HandleRabbitMessage(object sender, BasicDeliverEventArgs e)
        {
//            //todo: implement this properly, with specialized classes creating interactable models etc
//            try
//            {
                
//                var headers = e.BasicProperties.Headers.Where(x => x.Value is byte[]).ToDictionary(x => x.Key, y => Encoding.UTF8.GetString(y.Value as byte[]));
//                if (headers.TryGetValue(Constants.AUCOBO_CLASS_HEADER, out var aucoboClass))
//                {
//                    // todo: check if we should do things differently. How will we handle tabs, deleting, creating, etc?
//                    if (aucoboClass == Constants.WALKIE_TALKIE_BUTTON_ID && headers.TryGetValue("senderDeviceId", out var senderDeviceId) && headers.TryGetValue("senderOwnerName", out var senderOwnerName))
//                    {
                        
//#if __ANDROID__
//                                    Helper.RunOnUiThread(() => MainActivity.WalkieTalkieMessages.Add(WalkieTalkieMessage.FromRabbitMessage(e.Body.ToArray(), senderDeviceId, senderOwnerName)));
//#elif WINDOWS_UWP
//                                        var status = await Helper.GetDoNotDisturbStatusAsync();
//                                        if (status.DoNotDisturb) { return; }
//                                        if (Helper.IsDisplayOff()) { Helper.WakeupApp(); }
//                                        await AppServiceBridge.SendData(new ValueSet()
//                                        {
//                                            [AppServiceConstants.WalkieTalkieData] = e.Body.ToArray(),
//                                            [AppServiceConstants.WalkieTalkieSenderDeviceId] = senderDeviceId,
//                                            [AppServiceConstants.WalkieTalkieSenderOwnerName] = senderOwnerName,
//                                        });
//#elif __IOS__
//                                    // TODO
//#endif
//                        //Helper.Vibrate();
//                        return;
//                    }
//                    else if (aucoboClass == Constants.PICTURE_COMMUNICATION_BUTTON_ID)
//                    {
//                        headers.TryGetValue("senderDeviceId", out var pictureSenderDeviceId);
//                        headers.TryGetValue("senderOwnerName", out var pictureSenderOwnerName);

//#if __ANDROID__
//                                        Helper.RunOnUiThread(() => MainActivity.PictureMessages.Add(PictureMessage.FromRabbitMessage(e.Body.ToArray(), pictureSenderDeviceId, pictureSenderOwnerName)));
//#elif WINDOWS_UWP
//                                        var status = await Helper.GetDoNotDisturbStatusAsync();
//                                        if (status.DoNotDisturb) { return; }
//                                        if (Helper.IsDisplayOff()) { Helper.WakeupApp(); }
//                                        await AppServiceBridge.SendData(new ValueSet()
//                                        {
//                                            [AppServiceConstants.PictureData] = e.Body.ToArray(),
//                                            [AppServiceConstants.PictureSenderDeviceId] = pictureSenderDeviceId,
//                                            [AppServiceConstants.PictureSenderOwnerName] = pictureSenderOwnerName,
//                                        });
//#elif __IOS__
//                                    // TODO
//#endif
//                        Helper.Vibrate();
//                        return;
//                    }
//                }


//                // todo: Realm, database, save rabbit messages, handle everything and anything without aucoboClass
//                //var msg = new RabbitMessage()
//                //{
//                //    ID = Guid.NewGuid().ToString(),
//                //    Message = Encoding.UTF8.GetString(e.Body.ToArray()),
//                //    Headers = JsonConvert.SerializeObject(headers)
//                //};

//                //var processed = true;
//                //// handle exception while processing as "successul" processing to prevent repeated unsuccessful processing
//                //try { processed = await ProcessRabbitMessage(msg); } catch (Exception ex) { Logger.Error("ProcessRabbitMessage: " + ex); }

//                //if (!processed)
//                //{
//                //    DatabaseHelper.PushRealmItem(msg);
//                //    pendingRabbitMessages = true;
//                //}
//                //rabbitChannel.BasicAck(e.DeliveryTag, false);
//            }
//            catch (Exception ex) { Logger.Error("Consumer_Received: " + ex); }
        
        }
    }
}
