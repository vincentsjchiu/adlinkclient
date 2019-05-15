using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using adlinkClient;
using Newtonsoft.Json.Linq;
namespace DataConnectProClientSampleForMultiEqID
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        bool initialSuccess = false;
        int sendDataSuccess = 0;
        bool getEquipmentIDSuccess = false;
        ushort cardNumber = 0;//MCM-100 is always 0, if you connect to USB-2405,it will depends on the hardware configuration
        string username;
        string password;
        string messageName = "VCM";//Tell the DataConnectPro those data come from which edge software 
        int companyId;
        string[] Equipment;
        Client client = new Client();
        string deviceId;
        JObject topicName;
        List<List<string>> EqIds;
        List<string> Chs;
        string EquipmentID0 = "", EquipmentID1 = "", EquipmentID2 = "", EquipmentID3 = "";
        string[] datainfo = new string[4];
        double[] channeldata = new double[4];
        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void comboBoxCH0EquipmentID_MouseDown(object sender, MouseEventArgs e)
        {
            this.comboBoxCH0EquipmentID.Items.Clear();
            getEquipmentIDSuccess = client.GetEquipmentID(out Equipment);
            if (getEquipmentIDSuccess == true)
            {
                comboBoxCH0EquipmentID.Items.AddRange(Equipment);
            }
            else
            {
                MessageBox.Show("Get EquipmentID Fail");
            }
        }

        private void comboBoxCH1EquipmentID_MouseDown(object sender, MouseEventArgs e)
        {
            this.comboBoxCH1EquipmentID.Items.Clear();
            getEquipmentIDSuccess = client.GetEquipmentID(out Equipment);
            if (getEquipmentIDSuccess == true)
            {
                comboBoxCH1EquipmentID.Items.AddRange(Equipment);
            }
            else
            {
                MessageBox.Show("Get EquipmentID Fail");
            }
        }

        private void comboBoxCH2EquipmentID_MouseDown(object sender, MouseEventArgs e)
        {
            this.comboBoxCH2EquipmentID.Items.Clear();
            getEquipmentIDSuccess = client.GetEquipmentID(out Equipment);
            if (getEquipmentIDSuccess == true)
            {
                comboBoxCH2EquipmentID.Items.AddRange(Equipment);
            }
            else
            {
                MessageBox.Show("Get EquipmentID Fail");
            }
        }

        private void comboBoxCH3EquipmentID_MouseDown(object sender, MouseEventArgs e)
        {
            this.comboBoxCH3EquipmentID.Items.Clear();
            getEquipmentIDSuccess = client.GetEquipmentID(out Equipment);
            if (getEquipmentIDSuccess == true)
            {
                comboBoxCH3EquipmentID.Items.AddRange(Equipment);
            }
            else
            {
                MessageBox.Show("Get EquipmentID Fail");
            }
        }

        private void comboBoxCH0EquipmentID_SelectedIndexChanged(object sender, EventArgs e)
        {
            EquipmentID0 = Convert.ToString(comboBoxCH0EquipmentID.SelectedItem);
        }

        private void comboBoxCH1EquipmentID_SelectedIndexChanged(object sender, EventArgs e)
        {
            EquipmentID1 = Convert.ToString(comboBoxCH1EquipmentID.SelectedItem);
        }

        private void comboBoxCH2EquipmentID_SelectedIndexChanged(object sender, EventArgs e)
        {
            EquipmentID2 = Convert.ToString(comboBoxCH2EquipmentID.SelectedItem);
        }

        private void comboBoxCH3EquipmentID_SelectedIndexChanged(object sender, EventArgs e)
        {
            EquipmentID3 = Convert.ToString(comboBoxCH3EquipmentID.SelectedItem);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            combindatainfo();
            GetEqidToCHlist();
            for(int i=0; i<4;i++)
            {
                channeldata[i] = (double)randomNumber(i, 100);//Vitrual channel value from ch0~ch3
            }
            for (int i = 0; i < EqIds.Count; i++)
            {

                if (EqIds[i][0].ToString() != "")
                {
                    sendDataSuccess = client.SendData(databyeqid(datainfo, EqIds[i][0].ToString(), EqIds[i].ToArray()));
                    if (sendDataSuccess != 0)
                    {
                        MessageBox.Show("SendData Fail");
                    }
                }
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            username = this.textBoxUsername.Text;
            password = this.textBoxPassword.Text;
            initialSuccess = client.Initial(cardNumber, username, password, messageName, out deviceId);
            this.textBoxDeviceID.Text = deviceId;
            if (initialSuccess == true)
            {
                MessageBox.Show("Initial Success");
                client.MessageReached += Client_MessageReached;
            }
            else
            {
                MessageBox.Show("Initial Fail");
            }
        }
        public string databyeqid(string[] datainfo, string equipmentid, string[] chname)
        {
            topicName = new JObject();          
            topicName["equipmentId"] = equipmentid;
            topicName["equipmentRunStatus"] = 1;
            topicName["MessageName"] = messageName;
            for (int i = 0; i < datainfo.GetLongLength(0); i++)
            {
                for (int j = 1; j < chname.Length; j++)
                {
                    if (datainfo[i].Substring(0, datainfo[i].IndexOf("_")) == chname[j])//searching "CH0" "CH1" "CH2 "CH3" from "CH0_PIB0", "CH1_PIB0", "CH2_PIB0", "CH3_PIB0" 
                    {
                        topicName[datainfo[i]] = channeldata[i];
                        topicName[datainfo[i] + "_I"] = "10 Hz to 1000 Hz";
                    }
                }
            }

            return topicName.ToString();
        }
        private void GetEqidToCHlist()
        {
            //Create Channel mapping string to match the "CH0" "CH1" "CH2 "CH3"
            EqIds = new List<List<string>>();
            Chs = new List<string>();
            Chs.Add(EquipmentID0);
            Chs.Add(textBoxCh0name.Text);
            EqIds.Add(Chs);
            Chs = new List<string>();
            Chs.Add(EquipmentID1);
            Chs.Add(textBoxCh1name.Text);
            EqIds.Add(Chs);
            Chs = new List<string>();
            Chs.Add(EquipmentID2);
            Chs.Add(textBoxCh2name.Text);
            EqIds.Add(Chs);
            Chs = new List<string>();
            Chs.Add(EquipmentID3);
            Chs.Add(textBoxCh3name.Text);
            EqIds.Add(Chs);
            Chs = new List<string>();
            for (int i = 0; i < EqIds.Count - 1; i++)
            {
                for (int j = 1; j < EqIds.Count - i; j++)
                    if (EqIds[i][0] == EqIds[i + j][0])
                    {
                        EqIds[i].Add(EqIds[i + j][1]);
                        EqIds.RemoveAt(i + j);
                        j = 0;
                    }
            }
        }
        private void combindatainfo()
        {
            datainfo[0] = textBoxCh0name.Text + "_" + textBoxch0valuename.Text;
            datainfo[1] = textBoxCh1name.Text + "_" + textBoxch1valuename.Text;
            datainfo[2] = textBoxCh2name.Text + "_" + textBoxch2valuename.Text;
            datainfo[3] = textBoxCh3name.Text + "_" + textBoxch3valuename.Text;
        }

        private int randomNumber(int boundMin, int boundMax)
        {

            Random Rnd = new Random(); //加入Random，產生的數字不會重覆

            int x = Rnd.Next(boundMin, boundMax);

            return x;
        }
        private void Client_MessageReached(object sender, ReceiveEventArgs e)
        {
            richTextBoxMessagefromDataConnectPro.Text = e.Msg.ToString();
            //Console.WriteLine(e.Msg.ToString());
        }
    }
}
