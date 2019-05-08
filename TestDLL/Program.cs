using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using adlinkClient;
//using System.Threading;

namespace TestDLL
{
    class Program
    {
        static void Main(string[] args)
        {
            bool initialSuccess = false;
            bool sendDataSuccess = false;
            bool receiveSuccess=false;
            string jsonMessage = "{\"companyId\":28,\"msgTimestamp\":\"2019 - 03 - 21T19: 12:59.10Z\",\"equipmentId\":\"Compressor01\",\"equipmentRunStatus\":1,\"MessageName\":\"GMLite\",\"CH0_OA\":6,\"CH0_OA_I\":\"10 Hz to 1000 Hz\"}";

            string username = "test19";
            string password = "garage";
            string messageName = "VCM";
            Client client = new Client();
           
            int companyId;
            string deviceId;
            ushort cardNumber = 0;
            string[] Equipment;
            
            initialSuccess = client.Initial(cardNumber, username, password, messageName, out deviceId);
            
            Console.WriteLine("Wait.....");
            client.MessageReached += Client_MessageReached;
           
            Console.ReadKey();
            if (initialSuccess == true)
            {
                for (int i = 0; i < 5; i++)
                {
                    try
                    {
                        sendDataSuccess = client.SendData(jsonMessage);
                        
                        //sendDataSuccess=client.GetEquipmentID(out Equipment);
                        //sendDataSuccess=client.ge
                        if (sendDataSuccess == true)
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
                    //Thread.Sleep(5000);
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
