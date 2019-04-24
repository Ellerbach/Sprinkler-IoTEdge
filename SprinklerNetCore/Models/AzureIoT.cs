
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SprinklerNetCore.Models
{
    public class AzureIoT : IAzureIoT
    {
        private static string _strconn = null;
        private static string _deviceId = "";
        private static SiteInformation _site;
        private static ModuleClient _deviceClient;

        public AzureIoT(ISiteInformation siteInformation)
        {
            _site = (SiteInformation)siteInformation;
            InitIoTHub();
        }

        private static void InitIoTHub()
        {
            try
            {
                // Find a way to get the connection string
                //IOTEDGE_GATEWAYHOSTNAME = rpimoscowgarden
                //IOTEDGE_WORKLOADURI = unix:///var/run/iotedge/workload.sock
                //HOSTNAME = 346d1c5b3ed1
                //PATH = /usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin
                //IOTEDGE_IOTHUBHOSTNAME = EllerbachIOT.azure-devices.net
                //IOTEDGE_MODULEGENERATIONID = 636893565275753486
                //ASPNETCORE_ENVIRONMENT = Development
                //DOTNET_VERSION = 2.2.3
                //IOTEDGE_MODULEID = sprinkler
                //IOTEDGE_AUTHSCHEME = sasToken
                //IOTEDGE_APIVERSION = 2018-06-28
                //DOTNET_RUNNING_IN_CONTAINER = true
                //IOTEDGE_DEVICEID = MoscowGarden
                //HOME = /root
                //ASPNETCORE_URLS = http://+:80
                //RuntimeLogLevel = Information

                _strconn = Environment.GetEnvironmentVariable("IOTHUB_DEVICE_CONN_STRING");
                // Either IoT Edge  either IoT Hub
                if (_strconn == null)
                {
                    _deviceClient = ModuleClient.CreateFromEnvironmentAsync().GetAwaiter().GetResult();
                    _deviceClient.OpenAsync().Wait();
                    _deviceClient.SetMessageHandlerAsync(MessageHandlerIoT, _site);
                    _deviceId = Environment.GetEnvironmentVariable("IOTEDGE_DEVICEID");
                }
                else
                {
                    _deviceId = _strconn;
                    _deviceId = _deviceId.Substring(_deviceId.IndexOf("DeviceId=", StringComparison.CurrentCultureIgnoreCase) + 9);
                    _deviceId = _deviceId.Substring(0, _deviceId.IndexOf(';'));
                    ReceiveDataFromAzure();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error initializing Azure Iot Hub connection string: {ex.Message}");
            }

        }

        private static async Task<MessageResponse> MessageHandlerIoT(Message message, object userContext)
        {
            var ret = await ProcessMessage(message);
            return ret ? MessageResponse.Completed : MessageResponse.Abandoned;
        }

        private static async Task<bool> ProcessMessage(Message receivedMessage)
        {
            bool ballOK = true;
            try
            {
                string messageData;
                if (receivedMessage != null)
                {
                    // {"command":"addprogram","message":"{\"DateTimeStart\":\"2016-06-02T03:04:05+00:00\",\"Duration\":\"00:02:05\",\"SprinklerNumber\":3}"}
                    //MessageIoT temp = new MessageIoT();
                    //temp.command = "test";
                    //temp.message = JsonConvert.SerializeObject(new SprinklerProgram(new DateTimeOffset(2016, 6, 2, 3, 4, 5, new TimeSpan(0, 0, 0)), new TimeSpan(0, 2, 5), 3));
                    //var ret = JsonConvert.SerializeObject(temp);
                    //SendDataToAzure(ret);
                    messageData = Encoding.ASCII.GetString(receivedMessage.GetBytes());
                    MessageIoT cmdmsg = null;
                    try
                    {
                        cmdmsg = JsonConvert.DeserializeObject<MessageIoT>(messageData);
                    }
                    catch (Exception)
                    {
                        return false;

                    }

                    if (cmdmsg.command.ToLower() == "sprinklername")
                    {
                        cmdmsg.message = JsonConvert.SerializeObject(_site.Sprinklers);
                        // await Task.Delay(500);
                        SendDataToAzure(JsonConvert.SerializeObject(cmdmsg));
                    }
                    else if (cmdmsg.command.ToLower() == "programs")
                    {
                        cmdmsg.message = JsonConvert.SerializeObject(_site.SprinklerPrograms);
                        // await Task.Delay(500);
                        SendDataToAzure(JsonConvert.SerializeObject(cmdmsg));
                    }
                    else if (cmdmsg.command.ToLower() == "addprogram")
                    {
                        if (cmdmsg.message != null)
                        {
                            try
                            {
                                _site.SprinklerPrograms.Add(JsonConvert.DeserializeObject<SprinklerProgram>(cmdmsg.message));
                            }
                            catch (Exception)
                            {
                                ballOK = false;
                            }
                        }
                    }
                    else if (cmdmsg.command.ToLower() == "removeprogram")
                    {
                        if (cmdmsg.message != null)
                        {
                            try
                            {
                                //need to be smart how to remove a program
                                //so loop and check the elements
                                for (int i = 0; i < _site.SprinklerPrograms.Count; i++)
                                {
                                    SprinklerProgram MySpr = (SprinklerProgram)_site.SprinklerPrograms[i];
                                    SprinklerProgram spr = JsonConvert.DeserializeObject<SprinklerProgram>(cmdmsg.message);
                                    if ((MySpr.Number == spr.Number) &&
                                        (MySpr.Duration.CompareTo(spr.Duration) == 0) &&
                                        (MySpr.DateTimeStart.CompareTo(spr.DateTimeStart) == 0))
                                        _site.SprinklerPrograms.RemoveAt(i);
                                }
                            }
                            catch (Exception)
                            {
                                ballOK = false;
                            }

                        }
                    }
                    else if ((cmdmsg.command.ToLower() == "pumpstart") || (cmdmsg.command.ToLower() == "pumpstop"))
                    {
                        int sprNum = -1;
                        try
                        {
                            sprNum = Convert.ToInt32(cmdmsg.message);
                        }
                        catch { }
                        if ((sprNum >= 0) && (sprNum < _site.MaxSprinklers))
                        {
                            if (cmdmsg.command.ToLower() == "pumpstart")
                                _site.Sprinklers[sprNum].Open = true;
                            else
                                _site.Sprinklers[sprNum].Open = false;
                        }
                    }
                }
                else
                    ballOK = false;
            }
            catch (Exception)
            {
                return false;
            }
            return ballOK;
        }

        private static async Task ReceiveDataFromAzure()
        {
            if (_strconn == null)
                return;
            DeviceClient deviceClient = DeviceClient.CreateFromConnectionString(_strconn, TransportType.Http1);
            Message receivedMessage = null;
            try
            {
                while (true)
                {
                    try
                    {
                        receivedMessage = await deviceClient.ReceiveAsync();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error receiving from Azure Iot Hub: {ex.Message}");
                    }
                    if (receivedMessage != null)
                    {
                        var ret = await ProcessMessage(receivedMessage);
                        try
                        {
                            if (ret)
                                await deviceClient.CompleteAsync(receivedMessage);
                            else
                                await deviceClient.RejectAsync(receivedMessage);
                        }
                        catch (Exception)
                        {
                            try
                            {
                                await deviceClient.RejectAsync(receivedMessage);
                            }
                            catch (Exception)
                            {

                            }
                        }
                    }

                }
            }
            catch (Exception)
            {
                ReceiveDataFromAzure();
            }
        }

        public static async Task SendDataToAzure(string text)
        {
            //var text = "{\"info\":\"RPI SerreManagment Working\"}";
            var msg = new Message(Encoding.UTF8.GetBytes(text));
            try
            {
                if (_strconn == null)
                {
                    await _deviceClient.SendEventAsync(msg);
                }
                else
                {

                    DeviceClient deviceClient = DeviceClient.CreateFromConnectionString(_strconn, TransportType.Http1);
                    await deviceClient.SendEventAsync(msg);

                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error posting on Azure Iot Hub: {ex.Message}");
            }

        }

        public static async Task LogToAzure(string info, object obj = null)
        {
            try
            {
                AzureLog azureLog = new AzureLog()
                {
                    Info = obj,
                    DeviceId = _deviceId
                };
                SendDataToAzure(JsonConvert.SerializeObject(azureLog));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error posting on Azure Iot Hub: {ex.Message}");
            }
        }
    }
}
