using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using adlinkClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading;

namespace TestDLL
{
    class Program
    {
        static void Main(string[] args)
        {
            int initialSuccess = 0;
            int sendDataSuccess = 0;
            int receiveSuccess=0;
            //string jsonMessage = "{\"companyId\":28,\"msgTimestamp\":\"2019 - 05 - 15T19: 12:59.10Z\",\"equipmentId\":\"Compressor01\",\"equipmentRunStatus\":1,\"MessageName\":\"GMLite\",\"CH0_OA\":6,\"CH0_OA_I\":\"10 Hz to 1000 Hz\"}";

            string username = "test19";
            string password = "garag";
            string messageName = "VCM";
            Client client = new Client();
            string equipmentid="Compressor03";
            int companyId;
            string deviceId;
            ushort cardNumber = 0;
            string[] Equipment;
            JObject topicName;
            initialSuccess = client.Initial(cardNumber, username, password, messageName, out deviceId);
            
            Console.WriteLine("Wait.....");
            topicName = new JObject();
            topicName["equipmentId"] = equipmentid;
            topicName["equipmentRunStatus"] = 1;
            topicName["MessageName"] = messageName;
            topicName["CH0_OA"] = 4.5;
            topicName["CH0_OA_I"] = "10 Hz to 1000 Hz";
            topicName["CH1_OA"] = 2.2;
            topicName["CH1_OA_I"] = "10 Hz to 1000 Hz";
            topicName["CH2_OA"] = 3.6;
            topicName["CH2_OA_I"] = "10 Hz to 1000 Hz";
            topicName["CH3_OA"] = 7.5;
            topicName["CH3_OA_I"] = "10 Hz to 1000 Hz";
            
            
            client.MessageReached += Client_MessageReached;
           
            Console.ReadKey();
            if (initialSuccess == 0)
            {
                for (int i = 0; i < 5; i++)
                {
                    try
                    {
                        sendDataSuccess = client.SendData(topicName.ToString());

                        //sendDataSuccess=client.GetEquipmentID(out Equipment);
                        //sendDataSuccess=client.ge
                        if (sendDataSuccess == 0)
                        {
                            Console.WriteLine("Send Success");                           
                            //Console.WriteLine(Equipment[0]);
                        }
                        else
                        {
                            Console.WriteLine("Send Fail");
                        }
                    }catch(Exception e)
                    {
                        
                        Console.WriteLine(e.ToString());
                    }
                    Thread.Sleep(1000);
                }
            }
            else
            {
                Console.WriteLine("Initial Fail");
            }
            Console.ReadKey();

        }

        private static void Client_MessageReached(object sender, ReceiveEventArgs e)
        {
            Console.WriteLine(e.Msg.ToString());
            
        }

        static void Reached(object sender, ReceiveEventArgs e)
        {
            Console.WriteLine(e.Msg.ToString());

        }
    }
}
