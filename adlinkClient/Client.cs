using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace adlinkClient
{
    public class Client
    {
        public const string APIURL = "https://apiservicetrafficmanager.adlinktech.com/";      // Change to Your API URL
        public static string GatewayID;                         // Change to Your IoTDeviceID
        public static string GatewayPW;
        public static string Username;
        public static int MessageCatalogId;
        public static int CompanyId;
        public static string MessageName;
        public byte[] Read_SN_char = new byte[16];
        int error;
        public string SN_str;
        public string Controlstr;
        public string currenteqId, lasteqId;
        public static List<string> eqId = new List<string>();
        public DateTime currenttime;
        CDSHelper cdsHelper;
        Gateway gateway;
        JObject js;
        public Dictionary<string, DateTime> equipmentSendTime;
        public bool Initial(ushort cardNumber, string username, string password, string messageName, out string deviceId)
        {
            error = USBDASK.UD_Register_Card(USBDASK.USB_2405, cardNumber);
            if (error != USBDASK.NoError)
            {
                System.Windows.Forms.MessageBox.Show("Please Connect ADLINK Device!");
                
                deviceId = null;
                return false;
            }
            else
            {
                //string sr = "HAN2NC1003";
                //USBDASK.UD_Custom_Serial_Number_Write(cardNumber, Encoding.ASCII.GetBytes(sr));
                USBDASK.UD_Custom_Serial_Number_Read(cardNumber, Read_SN_char);
                if(Read_SN_char.All(singleByte => singleByte == 0))
                {
                    USBDASK.UD_Serial_Number_Read(cardNumber, Read_SN_char);
                }                                
                GatewayID = System.Text.Encoding.ASCII.GetString(Read_SN_char).Substring(0, 10);               
                deviceId = GatewayID;
                USBDASK.UD_Release_Card(cardNumber);
                Username = username;
                MessageName = messageName;
                try
                {
                    cdsHelper = new CDSHelper(APIURL, GatewayID, GatewayPW, username, password);
                    cdsHelper.Connect().Wait();
                    cdsHelper.GetMessaegCatalogId().Wait();
                    cdsHelper.ApplyMessage().Wait();                   
                    gateway = new Gateway(CDSHelper._CDSClient, this);
                    equipmentSendTime = new Dictionary<string, DateTime>();                   
                    currenteqId = "";
                }
                catch (Exception e)
                {
                    return false;
                }

                return true;
            }
        }
        public bool GetEquipmentID(out string[] equipmemtId)
        {
            try
            {
                eqId.Clear();
                cdsHelper.GetMessaegEquipmemtId().Wait();
                equipmemtId = eqId.ToArray();
                equipmentSendTime.Clear();
            }
            catch
            {
                equipmemtId = null;
                return false;
            }
            return true;
        }
        
        public bool SendData(string jsonMessage)
        {           
            if (CDSHelper._CDSClient != null)
            {
                try
                {
                    currenttime = DateTime.UtcNow;
                    js = new JObject();                   
                    js = (JObject)JsonConvert.DeserializeObject(jsonMessage);
                    currenteqId = js["equipmentId"].ToString();
                    if (equipmentSendTime.ContainsKey(currenteqId) == false)
                    {
                        //send
                        equipmentSendTime.Add(currenteqId, currenttime);
                        gateway.SendMessage(jsonMessage);
                        return true;
                    }
                    else
                    {
                        if ((currenttime - equipmentSendTime[currenteqId]).TotalSeconds > 8)
                        {
                            //send
                            equipmentSendTime[currenteqId] = currenttime;
                            gateway.SendMessage(jsonMessage);
                            return true;
                        }
                        else
                        {
                            System.Windows.Forms.MessageBox.Show("The time interval between two data need to be >= 10 seconds!");
                            return false;
                        }
                    }                                    
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("System.Security.Authentication.AuthenticationException"))
                    {
                        System.Windows.Forms.MessageBox.Show("Please Check Your System Date Time!");
                    }
                    return false;
                }

            }
            else
            {
                return false;
            }


        }
        public virtual void OnReveived(ReceiveEventArgs e)
        {
            EventHandler<ReceiveEventArgs> handler = MessageReached;
            if (handler != null)
            {
                handler(this, e);
            }
            
        }
        public event EventHandler<ReceiveEventArgs> MessageReached;
    }
    public class ReceiveEventArgs : EventArgs
    {
        public string Msg { get; set; }

    }

}
