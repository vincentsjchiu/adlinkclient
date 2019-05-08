using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace adlinkClient

{
    class Gateway
    {
        DeviceClient _CDSClient;
        Client _Client;
        JObject json, topicName;
        public Gateway(DeviceClient cdsClient, Client client)
        {
            _CDSClient = cdsClient;
            _Client = client;          
            ReceiveCloudToDeviceMessageAsync();
        }

        public void SendMessage(string jsonMessage)
        {

            try
            {
                /* Send Telemetry */
                SendTelemetry(jsonMessage).ConfigureAwait(false);                 
                /* Receive Cloud To Devce Message */

            }
            catch (AggregateException ex)
            {

                throw new Exception(ex.InnerException.ToString());

            }


        }
        
        public async Task SendTelemetry(string jsonMessage)
        {
            List<Message> messages = getDeviceMessages(jsonMessage);
            await _CDSClient.SendEventBatchAsync(messages);
            //await Task.Run(()=>_CDSClient.SendEventBatchAsync(messages));           
        }

        private List<Message> getDeviceMessages(string jsonMessage)
        {
            string device01Msg;           
            json = new JObject();
            topicName = new JObject();
            json = (JObject)JsonConvert.DeserializeObject(jsonMessage);
            topicName["companyId"] = Client.CompanyId;
            topicName["msgTimestamp"] = this._Client.currenttime.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'");
            topicName.Merge(json);
            device01Msg = topicName.ToString();
            var message01 = new Message(Encoding.ASCII.GetBytes(device01Msg));
            message01.Properties.Add("MessageCatalogId", Client.MessageCatalogId.ToString());
            message01.Properties.Add("Type", "Message");
            List<Message> messageList = new List<Message>();
            messageList.Add(message01);
            return messageList;
        }

        private string getEquipmentTelemetry(string equipmentId)
        {

            var telemetry = new
            {

            };

            return JsonConvert.SerializeObject(telemetry);
        }


        public async void ReceiveCloudToDeviceMessageAsync()
        {
            while (true)
            {
                try
                {
                    Message receivedMessage = await _CDSClient.ReceiveAsync();

                    if (receivedMessage == null)
                        continue;// It returns null after a specifiable timeout period (in this case, the default of one minute is used)
                    ReceiveEventArgs args = new ReceiveEventArgs();
                    string msg = Encoding.ASCII.GetString(receivedMessage.GetBytes());
                    args.Msg = msg;
                    this._Client.Controlstr = msg;                    
                    await _CDSClient.CompleteAsync(receivedMessage);
                    this._Client.OnReveived(args);
                }
                catch (Exception ex)
                {
                    //Console.WriteLine(ex.ToString());
                }
            }
        }

    }

}
