using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace adlinkClient
{

    public class CDSHelper
    {        
        string _APIURL, _IoTDeviceID, _IoTDevicePW, _AccessToken, _IoTHubName, _IoTHubProtocol, _UserName, _Password;
        string _IoTHubAuthenticationType, _IoTDeviceKey, _ContainerName, _CertificateFileName, _CertificatePassword;

        public static DeviceClient _CDSClient;
        private object response;

        public CDSHelper(string apiURL, string iotDeviceID, string iotDevicePassword, string username, string password)
        {
            if (!apiURL.StartsWith("http"))
                throw new Exception("invalid URL");
            if (!apiURL.EndsWith("/"))
                apiURL = apiURL + "/";

            _APIURL = apiURL;
            _IoTDeviceID = iotDeviceID;
            _IoTDevicePW = iotDevicePassword;
            _UserName = username;
            _Password = password;
        }

        public async Task<bool> ApplyMessage()
        {
            string endPointURI = _APIURL + "admin-api/IoTDevice/" + Client.GatewayID + "/Message";
            string ioTDeviceMessageString = await callAPIService("GET", endPointURI, null);
            dynamic messages = JsonConvert.DeserializeObject(ioTDeviceMessageString);
            bool applied = false;
            string contentType = "application/json";

            foreach (dynamic message in messages)
            {
                if (message.MessageCatalogName == Client.MessageName)
                {
                    applied = true;
                    break;
                }
            }

            if (applied == false)
            {
                //apply message
                endPointURI = _APIURL + "admin-api/IoTDevice/" + Client.GatewayID + "/Message";

                var applyMessage = new
                {
                    CompanyId = Client.CompanyId,
                    RequesterEmail = Client.Username,
                    MessageCatalogIdList = new int[] { Client.MessageCatalogId }
                };

                string postData = JsonConvert.SerializeObject(applyMessage);
                string message = await callAPIService("PUT", endPointURI, postData, contentType);
            }

            return true;
        }

        public async Task<bool> GetMessaegCatalogId()
        {
            string endPointURI = _APIURL + "admin-api/MessageCatalog/company/" + Client.CompanyId.ToString();
            string IoTDeviceString = await callAPIService("GET", endPointURI, null);
            dynamic IoTDeviceJSONObj = JsonConvert.DeserializeObject(IoTDeviceString);
            foreach(dynamic obj in IoTDeviceJSONObj)
            {
                Console.WriteLine(obj.Name);
                if(obj.Name==Client.MessageName)
                {
                    Client.MessageCatalogId = obj.Id;
                    break;
                }
            }

            return true;
        }
        public async Task<bool> GetMessaegEquipmemtId()
        {
            string endPointURI = _APIURL + "admin-api/Equipment/company/" + Client.CompanyId.ToString();
            string IoTDeviceString = await callAPIService("GET", endPointURI, null);
            dynamic IoTDeviceJSONObj = JsonConvert.DeserializeObject(IoTDeviceString);
            
            foreach (dynamic obj in IoTDeviceJSONObj)
            {               
                Client.eqId.Add(Convert.ToString(obj.Name));
            }

            return true;
        }
        public async Task<bool> Connect()
        {
            string endPointURI = _APIURL + "device-api/DeviceAuth/" + _IoTDeviceID;
            string IoTDeviceString = await callAPIService("GET", endPointURI, null);
            dynamic IoTDeviceJSONObj = JObject.Parse(IoTDeviceString);
            _IoTHubName = IoTDeviceJSONObj.IoTHubName;
            _IoTHubProtocol = IoTDeviceJSONObj.IoTHubProtocol;
            _IoTHubAuthenticationType = IoTDeviceJSONObj.IoTHubAuthenticationType;
            _IoTDeviceKey = IoTDeviceJSONObj.DeviceKey;
            _ContainerName = IoTDeviceJSONObj.ContainerName;
            _CertificateFileName = IoTDeviceJSONObj.CertificateFileName;
            _CertificatePassword = IoTDeviceJSONObj.CertificatePassword;
            try
            {
                Microsoft.Azure.Devices.Client.TransportType protocol = Microsoft.Azure.Devices.Client.TransportType.Http1;
                switch (_IoTHubProtocol.ToLower())
                {
                    case "amqp":
                        protocol = Microsoft.Azure.Devices.Client.TransportType.Amqp;
                        break;
                    case "mqtt":
                        protocol = Microsoft.Azure.Devices.Client.TransportType.Mqtt;
                        break;
                    case "https":
                        protocol = Microsoft.Azure.Devices.Client.TransportType.Http1;
                        break;
                }
                if (_CDSClient == null)
                {
                    _CDSClient = DeviceClient.Create(_IoTHubName, new DeviceAuthenticationWithRegistrySymmetricKey(_IoTDeviceID, _IoTDeviceKey), protocol);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return true;
        }

        private async Task<string> callAPIService(string method, string endPointURI, string postData, string contentType= "application/x-www-form-urlencoded")
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(endPointURI);
            request.Method = method;
            HttpWebResponse response = null;

            try
            {
                request.ContentType = contentType;
                request.Headers.Add("Authorization", "Bearer " + _AccessToken);

                switch (method.ToLower())
                {
                    case "get":
                    case "delete":
                        response = request.GetResponse() as HttpWebResponse;
                        break;
                    case "post":
                    case "put":
                        using (Stream requestStream = request.GetRequestStream())
                        using (StreamWriter writer = new StreamWriter(requestStream, Encoding.ASCII))
                        {
                            writer.Write(postData);
                        }
                        response = (HttpWebResponse)request.GetResponse();
                        break;
                    default:
                        throw new Exception("Method:" + method + " Not Support");
                }
                using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                {
                    return sr.ReadToEnd();
                }
            }
            catch (WebException ex)
            {
                var httpResponse = (HttpWebResponse)ex.Response;

                if (httpResponse.StatusCode == HttpStatusCode.Unauthorized)
                {
                    if (await getAPIToken())
                        return await callAPIService(method, endPointURI, postData);
                }
                else
                    throw new Exception(httpResponse.StatusCode.ToString());
            }
            catch (Exception)
            {                
                throw;
            }

            return null;
        }


        private async Task<bool> getAPIToken()
        {
            string tokenRole = "admin";           

            var content = new FormUrlEncodedContent(new Dictionary<string, string>()
            {
                { "grant_type", "password" },
                { "email", _UserName },
                { "password", _Password },
                { "role", tokenRole }
            });

            string uri = _APIURL + "token";
            string postdata = "grant_type=password&email=" + _UserName;
            postdata += "&password=" + _Password;
            postdata += "&role=" + tokenRole;
            //response = await client.PostAsync(uri, content);
            try
            {
                string result = await callAPIService("POST", uri, postdata);
                dynamic access_result = JObject.Parse(result);
                _AccessToken = access_result.access_token;
                Client.CompanyId = access_result.CompanyId;
                return true;
            }
            catch(Exception e)
            {
                Console.WriteLine("Exception " + e.ToString());
                return false;
            }
        }
    }
}
