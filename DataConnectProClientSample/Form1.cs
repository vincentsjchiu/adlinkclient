﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;
using adlinkClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Threading;
namespace DataConnectProClientSample
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        int initialSuccess = 0;
        int sendDataSuccess = 0;
        int getEquipmentIDSuccess = 0;
        ushort cardNumber = 0;//MCM-100 is always 0, if you connect to USB-2405,it will depends on the hardware configuration
        string username;
        string password;
        string messageName = "VCM";//Tell the DataConnectPro those data come from which edge software 
        int companyId;
        string[] Equipment;
        string equipmentid;
        Client client = new Client();
        string deviceId;
        JObject topicName;
        JObject json;
        bool flage = true;
        Thread sddata;
        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            //messageName = this.textBoxMessageName.Text;
            username = this.textBoxUsername.Text;
            password = this.textBoxPassword.Text;
            initialSuccess = client.Initial(cardNumber, username, password, messageName, out deviceId);
            this.textBoxDeviceID.Text = deviceId;
            if (initialSuccess == 0)
            {
                MessageBox.Show("Initial Success");
                client.MessageReached += Client_MessageReached;
            }
            else
            {
                MessageBox.Show("Initial Fail");
            }
        }

        private void comboBox1_MouseDown(object sender, MouseEventArgs e)
        {
            this.comboBox1.Items.Clear();
            getEquipmentIDSuccess = client.GetEquipmentID(out Equipment);
            if (getEquipmentIDSuccess == 0)
            {
                comboBox1.Items.AddRange(Equipment);
            }
            else
            {
                MessageBox.Show("Get EquipmentID Fail");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            sddata = new Thread(data);
            sddata.Start();
            /*topicName = new JObject();
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
            //Task.Run(() => sendDataSuccess = client.SendData(topicName.ToString()));
            sendDataSuccess = client.SendData(topicName.ToString());
            if (sendDataSuccess != 0)
            {
                //MessageBox.Show("SendData Fail");
                Console.WriteLine("SendData Fail");
            }
            else
            {
                Console.WriteLine("SendData Done");
            }*/
        }
        private void data()
        {
            while (true)
            {
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
                //Task.Run(() => sendDataSuccess = client.SendData(topicName.ToString()));
                sendDataSuccess = client.SendData(topicName.ToString());
                if (sendDataSuccess != 0)
                {
                    //MessageBox.Show("SendData Fail");
                    Console.WriteLine("SendData Fail");
                }
                else
                {
                    Console.WriteLine("SendData Done");
                }
                //Application.DoEvents();
                Thread.Sleep(10000);
            }
        }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            equipmentid = Convert.ToString(comboBox1.SelectedItem);
        }
        private void Client_MessageReached(object sender, ReceiveEventArgs e)
        {
            string dio = client.Controlstr;//e.Msg.ToString();
            richTextBoxMessagefromDataConnectPro.Text = dio;//e.Msg.ToString();
            /*JsonReader reader = new JsonTextReader(new StringReader(dio));
           
            while (reader.Read())
            {
                dio = reader.Value.ToString();
            }*/
            json = (JObject)JsonConvert.DeserializeObject(dio);
            Console.WriteLine(json["D0"]);
            Console.WriteLine(json["D1"]);
            //Console.WriteLine(e.Msg.ToString());
        }

    }
}
