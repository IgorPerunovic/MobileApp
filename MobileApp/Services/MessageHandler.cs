using MobileApp.Interfaces;
using MobileApp.Models;
using Newtonsoft.Json;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace MobileApp.Services
{
    // this is a singleton class
    public class MessageHandler : IMessageHandler
    {
        private MessageHandler() { }
        
        
        static readonly SemaphoreSlim toDoSema = new SemaphoreSlim(1);
        public event EventHandler<TodoCreatedEventArgs> TodoCreated;


        public static MessageHandler Instance { get; } = new MessageHandler();

        public void HandleRabbitMessage(object sender, BasicDeliverEventArgs e)
        {
            //still_todo: implement this properly, with specialized classes creating interactable models etc
            try
            {

                var headers = e.BasicProperties.Headers.Where(x => x.Value is byte[]).ToDictionary(x => x.Key, y => Encoding.UTF8.GetString(y.Value as byte[]));
                if (headers.TryGetValue(Constants.AUCOBO_CLASS_HEADER, out var aucoboClass))
                {
                    switch (aucoboClass)
                    {
                        case Constants.WALKIE_TALKIE_MESSAGE_AUCOBO_CLASS:  // STILL_TODO: Implement this properly, check logic
                            {
                                if (headers.TryGetValue("senderDeviceId", out var senderDeviceId) && headers.TryGetValue("senderOwnerName", out var senderOwnerName))
                                { handleWalkieTalkieMessage(e); }
                                break;
                            }

                        case Constants.PICTURE_COMMUNICATION_MESSAGE_AUCOBO_CLASS: 
                            {
                                handlePictureMessage(e, headers);
                                break;
                            }
                        case Constants.TO_DO_MESSAGE_AUCOBO_CLASS:
                            {
                                handleToDoMessage(e);
                                break;
                            }
                        default: break;

                    }

                    var Message = Encoding.UTF8.GetString(e.Body.ToArray());

                }


                // still_todo: save rabbit messages, handle everything and anything without aucoboClass
                //var msg = new RabbitMessage()
                //{
                //    ID = Guid.NewGuid().ToString(),
                //    Message = Encoding.UTF8.GetString(e.Body.ToArray()),
                //    Headers = JsonConvert.SerializeObject(headers)
                //};



                //var processed = true;
                //// handle exception while processing as "successul" processing to prevent repeated unsuccessful processing
                //try { processed = await ProcessRabbitMessage(msg); } catch (Exception ex) { Logger.Error("ProcessRabbitMessage: " + ex); }

                //if (!processed)
                //{
                //    DatabaseHelper.PushRealmItem(msg);
                //    pendingRabbitMessages = true;
                //}
                //rabbitChannel.BasicAck(e.DeliveryTag, false);
            }
            catch (Exception ex) {
            //    Logger.Error("Consumer_Received: " + ex); 
            }

        }

        void handlePictureMessage(BasicDeliverEventArgs e, Dictionary<string, string> headers) {
            headers.TryGetValue("senderDeviceId", out var pictureSenderDeviceId);
            headers.TryGetValue("senderOwnerName", out var pictureSenderOwnerName);
            return;
            // still_todo: implement this on VM with event raising
#if __ANDROID__
                                        Helper.RunOnUiThread(() => MainActivity.PictureMessages.Add(PictureMessage.FromRabbitMessage(e.Body.ToArray(), pictureSenderDeviceId, pictureSenderOwnerName)));
#elif WINDOWS_UWP
                                        var status = await Helper.GetDoNotDisturbStatusAsync();
                                        if (status.DoNotDisturb) { return; }
                                        if (Helper.IsDisplayOff()) { Helper.WakeupApp(); }
                                        await AppServiceBridge.SendData(new ValueSet()
                                        {
                                            [AppServiceConstants.PictureData] = e.Body.ToArray(),
                                            [AppServiceConstants.PictureSenderDeviceId] = pictureSenderDeviceId,
                                            [AppServiceConstants.PictureSenderOwnerName] = pictureSenderOwnerName,
                                        });
#elif __IOS__
                                    // still_TODO
#endif
            //Helper.Vibrate();
            return;
        }

        void handleWalkieTalkieMessage(BasicDeliverEventArgs e)
        {

            return;
            // still_todo: implement this on VM with event raising
#if __ANDROID__
                                    Helper.RunOnUiThread(() => MainActivity.WalkieTalkieMessages.Add(WalkieTalkieMessage.FromRabbitMessage(e.Body.ToArray(), senderDeviceId, senderOwnerName)));
#elif WINDOWS_UWP
                                        var status = await Helper.GetDoNotDisturbStatusAsync();
                                        if (status.DoNotDisturb) { return; }
                                        if (Helper.IsDisplayOff()) { Helper.WakeupApp(); }
                                        await AppServiceBridge.SendData(new ValueSet()
                                        {
                                            [AppServiceConstants.WalkieTalkieData] = e.Body.ToArray(),
                                            [AppServiceConstants.WalkieTalkieSenderDeviceId] = senderDeviceId,
                                            [AppServiceConstants.WalkieTalkieSenderOwnerName] = senderOwnerName,
                                        });
#elif __IOS__
                                    // still_TODO
#endif
            //Helper.Vibrate();
            return;
        }

        async void handleToDoMessage(BasicDeliverEventArgs msg)
        {

            await toDoSema.WaitAsync();
            try
            {
                var message = Encoding.UTF8.GetString(msg.Body.ToArray());
                var toDo = JsonConvert.DeserializeObject<TodoModel>(message);
                var headers = msg.BasicProperties.Headers.Where(x => x.Value is byte[]).ToDictionary(x => x.Key, y => Encoding.UTF8.GetString(y.Value as byte[]));


                if (headers.TryGetValue("event", out var ev)) // still_todo: add as constants, not as hardcoded values
                { 
                    toDo.RabbitEventHeader = ev;
                    if (ev != "ASSIGNED" && ev != "CREATED" && ev != "ACCEPTED" && ev != "DONE") { return; } // we dont care about other events 
                    if (ev != "ASSIGNED" && toDo.AssignedToCurrentUser) { return; } // the current user initiated this update -> ignore
                }

                // IMPORTANT STILL_TODO: raise event with ToDo for the VM to handle
                // this goes into the VM:
                //if (toDo.Assignee == null && DatabaseHelper.Current.LocalTodos.Any(x => x != toDo && x.Blocking && !x.InteractionPending))
                //{
                //    await BackendHelper.DeclineTodoAsync(toDo, true, false);
                //    toDo.RabbitEventHeader = "DECLINED_BECAUSE_BLOCKED";
                //}
                TodoCreated.Invoke(this, new TodoCreatedEventArgs(toDo));
                
            }
            catch(Exception ex) { }
            finally { toDoSema.Release(); }

        
        }


    }


    public class TodoCreatedEventArgs : EventArgs
    {
        public TodoCreatedEventArgs(TodoModel todo)
        {
            this.Todo = todo;
        }

        public TodoModel Todo { get; set; }
    }
}
