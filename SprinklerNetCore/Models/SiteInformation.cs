using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Linq;
using DarkSkyApi;
using System.Threading;
using System.Text;
using System.Diagnostics;
using Microsoft.Azure.Devices.Client;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System.ComponentModel.DataAnnotations;
using System.Device.Gpio;

namespace SprinklerNetCore.Models
{
    public class SiteInformation : ISiteInformation
    {

        #region Variables

        private const string LocalPath = "Files";
        private const string SitePath = "SiteInformation.json";
        private const string PastProgramPath = "PastProgram.json";
        private const int PinRelayOn = 20;
        private static string FileSitePath => _deviceId + "." + SitePath;
        private static string FilePactProgramPath => _deviceId + "." + PastProgramPath;
        private static Timer _timerForecastCallBack;
        private static Timer _timerProgramCallBack;
        private static bool ConfigLoaded = false;
        private static AzureBlobSetings _azureBlobSetings;
        private static string _deviceId;

        private static int _maxSprinklers { get { return 5; } }
        private static List<Sprinkler> _sprinklers { get; set; }
        private static Settings _settings { get; set; }
        private static List<FuzzySprinkler> _fuzzySprinklers { get; set; }
        private static List<SprinklerProgram> _sprinklerPrograms { get; set; }
        private static SoilHumidity _soilHumidity { get; set; }
        private static NeedToSrpinkle _needToSprinkle { get; set; }

        [Display(Name = nameof(Resources.Text.MaxSprinklers), ResourceType = typeof(Resources.Text))]
        public int MaxSprinklers => _maxSprinklers;
        [Display(Name = nameof(Resources.Text.Sprinklers), ResourceType = typeof(Resources.Text))]
        public List<Sprinkler> Sprinklers { get { return _sprinklers; } set { _sprinklers = value; } }
        [Display(Name = nameof(Resources.Text.ForecastIOSettings), ResourceType = typeof(Resources.Text))]
        public Settings Settings { get { return _settings; } set { _settings = value; } }
        [Display(Name = nameof(Resources.Text.FuzzySprinklers), ResourceType = typeof(Resources.Text))]
        public List<FuzzySprinkler> FuzzySprinklers { get { return _fuzzySprinklers; } set { _fuzzySprinklers = value; } }
        [Display(Name = nameof(Resources.Text.SprinklerPrograms), ResourceType = typeof(Resources.Text))]
        public List<SprinklerProgram> SprinklerPrograms { get { return _sprinklerPrograms; } set { _sprinklerPrograms = value; } }
        [Display(Name = nameof(Resources.Text.SoilHumidity), ResourceType = typeof(Resources.Text))]
        public SoilHumidity SoilHumidity { get { return _soilHumidity; } set { _soilHumidity = value; } }
        [Display(Name = nameof(Resources.Text.NeedToSprinkle), ResourceType = typeof(Resources.Text))]
        public NeedToSrpinkle NeedToSprinkle { get { return _needToSprinkle; } set { _needToSprinkle = value; } }
        [JsonIgnore]
        public HistorySprinkling HistorySprinkling => GetHistorySprinkling();

        #endregion

        #region Configuraiton and timers

        public SiteInformation()
        {
            InitData();
        }

        public SiteInformation(AzureBlobSetings azureBlobSetings)
        {
            _azureBlobSetings = azureBlobSetings;
            InitData();
        }

        private void InitData()
        {
            if (!ConfigLoaded)
            {
                ConfigLoaded = true;
                LoadConfiguration();
                //initialize the relay output
#if DEBUG
                Debug.WriteLine($"Initialing relay output");
#else
                GpioController controller = new GpioController();
                controller.OpenPin(PinRelayOn, PinMode.Output);
                controller.Write(PinRelayOn, PinValue.High);
#endif
            }
        }

        public static void LoadConfiguration()
        {
            SiteInformation retConf;

            try
            {
                _deviceId = Environment.GetEnvironmentVariable("IOTHUB_DEVICE_CONN_STRING");
                _deviceId = _deviceId.Substring(_deviceId.IndexOf("DeviceId=", StringComparison.CurrentCultureIgnoreCase) + 9);
                _deviceId = _deviceId.Substring(0, _deviceId.IndexOf(';'));
                var stream = DownloadAsync(FileSitePath).GetAwaiter().GetResult();
                retConf = JsonConvert.DeserializeObject<SiteInformation>(Encoding.Default.GetString(stream.ToArray()));
            }
            catch (Exception)
            {

                try
                {
                    StreamReader file = new StreamReader(Path.Combine(LocalPath, SitePath));
                    var jsonSite = file.ReadToEnd();
                    file.Close();
                    retConf = JsonConvert.DeserializeObject<SiteInformation>(jsonSite);
                }
                catch (Exception)
                {
                    retConf = new SiteInformation();
                    //default settings                    
                    retConf.Sprinklers = new List<Sprinkler>();
                    for (int i = 0; i < 3; i++)
                    {
                        Sprinkler spr = new Sprinkler();
                        spr.Name = $"Sprinkler {i}";
                        spr.Number = i;
                        spr.TypicalProgram = new TypicalProgram();
                        spr.TypicalProgram.StartTime = new TimeSpan(0, i, 0);
                        spr.TypicalProgram.Duration = new TimeSpan(0, 20, 0);
                        retConf.Sprinklers.Add(spr);
                    }
                    retConf.Settings = new Settings();
                    retConf.Settings.Name = "My super Sprinkler";
                    retConf.Settings.ApiKey = "123";
                    retConf.Settings.City = "Antony";
                    retConf.Settings.Language = Language.French;
                    retConf.Settings.Latitude = (float)1.2;
                    retConf.Settings.Longitude = (float)2.4;
                    retConf.Settings.Unit = Unit.SI;
                    retConf.Settings.TimeZoneInfoId = TimeZoneInfo.Local.Id;
                    retConf.FuzzySprinklers = new List<FuzzySprinkler>();
                    for (int i = 0; i < 3; i++)
                    {
                        FuzzySprinkler fz = new FuzzySprinkler();
                        fz.TempMin = i * 10;
                        fz.TempMax = (i + 1) * 10;
                        fz.SprinklingMax = (float)(100.0 * i / 3);
                        fz.RainMax = (i + 1) * 2;
                        retConf.FuzzySprinklers.Add(fz);
                    }
                    retConf.SprinklerPrograms = new List<SprinklerProgram>();
                }
            }

            // Set the timer for automated forecast 
            if (string.IsNullOrEmpty(retConf.Settings.TimeZoneInfoId))
                retConf.Settings.TimeZoneInfoId = TimeZoneInfo.Local.Id;
            DateTime dt = DateTime.Now;
            try
            {
                dt = dt.Add(TimeZoneInfo.FindSystemTimeZoneById(retConf.Settings.TimeZoneInfoId).GetUtcOffset(dt));
            }
            catch (Exception)
            {
                retConf.Settings.TimeZoneInfoId = TimeZoneInfo.Local.Id;
                dt = dt.Add(TimeZoneInfo.FindSystemTimeZoneById(retConf.Settings.TimeZoneInfoId).GetUtcOffset(dt));
            }            
            TimeSpan dueDate;
            TimeSpan period = new TimeSpan(24, 0, 0);
            if (dt.TimeOfDay > retConf.Settings.TimeToCheck)
            {
                dueDate = new TimeSpan(24, 0, 0) - dt.TimeOfDay + retConf.Settings.TimeToCheck;
            }
            else
            {
                dueDate = retConf.Settings.TimeToCheck - dt.TimeOfDay;
            }

            _timerForecastCallBack = new Timer(TimerForecastCheck, null, dueDate, period);
            _timerProgramCallBack = new Timer(TimerProgramCheck, null, new TimeSpan(0, 1, 0), new TimeSpan(0, 1, 0));
        }

        private static void TimerProgramCheck(object state)
        {
            // Check if anything needs to get launch as program
            List<SprinklerProgram> toRemove = new List<SprinklerProgram>();
            DateTime dt = DateTime.Now;
            dt = dt.Add(TimeZoneInfo.FindSystemTimeZoneById(_settings.TimeZoneInfoId).GetUtcOffset(dt));
            foreach (var prg in _sprinklerPrograms)
            {
                if (prg.DateTimeStart < dt)
                {
                    var spr = _sprinklers.Where(m => m.Number == prg.Number).FirstOrDefault();
                    if (spr != null)
                    {
                        spr.CloseAfter = prg.Duration;
                        spr.Open = true;
                        // Save it into the srpinkling file
                        SavePastPrograms(prg);
                    }
                    toRemove.Add(prg);
                }
            }
            if (toRemove.Count > 0)
            {
                foreach (var rm in toRemove)
                    _sprinklerPrograms.Remove(rm);
                SaveConfigurationStatic();
            }
        }

        private static void TimerForecastCheck(object state)
        {
            // Get the forecast first
            var needToSprinkle = GetForecastStatic();
            if (needToSprinkle.NeedTo)
            {
                // Create the typical programs adjusted
                foreach (var spr in _sprinklers)
                {
                    var prg = new SprinklerProgram();
                    prg.Duration = spr.TypicalProgram.Duration * needToSprinkle.PercentageCorrection;
                    prg.Number = spr.Number;
                    var dtnow = DateTime.Now;
                    if (spr.TypicalProgram.StartTime > dtnow.TimeOfDay)
                    {
                        dtnow = dtnow.AddDays(1);
                    }
                    prg.DateTimeStart = new DateTime(dtnow.Year, dtnow.Month, dtnow.Day, spr.TypicalProgram.StartTime.Hours, spr.TypicalProgram.StartTime.Minutes, spr.TypicalProgram.StartTime.Seconds);
                    _sprinklerPrograms.Add(prg);
                    SaveConfigurationStatic();
                }

            }

        }

        private static void SavePastPrograms(SprinklerProgram sprinklerProgram)
        {
            string allfile = "";
            try
            {
                var stream = DownloadAsync(FilePactProgramPath).GetAwaiter().GetResult();
                allfile = Encoding.Default.GetString(stream.ToArray());
            }
            catch (Exception)
            {
                // Most likely the file does not exist or we don't have access
                // So read the local file then
                try
                {
                    StreamReader fileR = new StreamReader(Path.Combine(LocalPath, PastProgramPath));
                    allfile = fileR.ReadToEnd();
                    fileR.Close();
                }
                catch (Exception)
                {

                    //So it's really the first time the file is created
                }
            }

            var jsonSer = JsonConvert.SerializeObject(sprinklerProgram);
            if (allfile.Length > 2)
            {
                allfile = allfile.Substring(0, allfile.Length - 1);
                allfile += "," + jsonSer + "]";
            }
            else
            {
                allfile = "[" + jsonSer + "]";
            }
            StreamWriter file = new StreamWriter(Path.Combine(LocalPath, PastProgramPath));
            file.Write(allfile);
            file.Close();
            // If multiple programs are at the same time, we will have to wait otherwise
            // we will get an error while uploading the stream
            try
            {
                UploadAsync(FilePactProgramPath, Path.Combine(LocalPath, PastProgramPath)).Wait();
            }
            catch (Exception)
            {

                // well, maybe we are offline, then it won't be uploaded
            }
            
        }

        public void SaveConfiguration()
        {
            SaveConfigurationStatic();
        }

        public static void SaveConfigurationStatic()
        {

            StreamWriter file = new StreamWriter(Path.Combine(LocalPath, SitePath));
            var jsonSer = JsonConvert.SerializeObject(new SiteInformation());
            file.Write(jsonSer);
            file.Close();
            try
            {
                UploadAsync(FileSitePath, Path.Combine(LocalPath, SitePath)).Wait();
            }
            catch (Exception)
            {

                // most likely offline mode
            }
            
        }

        #endregion

        #region Forecast

        public NeedToSrpinkle GetForecast()
        {
            return GetForecastStatic();
        }

        public static NeedToSrpinkle GetForecastStatic()
        {
            NeedToSrpinkle needToSrpinkle = new NeedToSrpinkle();
            try
            {
                // Get the forecast
                var client = new DarkSkyService(_settings.ApiKey);
                var tsk = client.GetWeatherDataAsync(_settings.Latitude, _settings.Longitude, _settings.Unit, _settings.Language);
                tsk.Wait();
                var forecast = tsk.Result;
                needToSrpinkle.Forecast = forecast;
                // Get Forcast max temperature for the next 24h and same for precipitations
                float ForecastMaxTemp = 0;
                float ForecastTotalPrecipitation = 0;
                float ForecastProbabilityPrecipitation = 0;
                if (forecast.Daily.Days != null)
                    if (forecast.Daily.Days.Count > 0)
                    {
                        if (forecast.Daily.Days[0].HighTemperature >= ForecastMaxTemp)
                            ForecastMaxTemp = forecast.Daily.Days[0].HighTemperature;
                        if ((forecast.Daily.Days[0].PrecipitationIntensity * 24) >= ForecastTotalPrecipitation)
                            ForecastTotalPrecipitation = forecast.Daily.Days[0].PrecipitationIntensity * 24;
                        if (forecast.Daily.Days[0].PrecipitationProbability >= ForecastProbabilityPrecipitation)
                            ForecastProbabilityPrecipitation = forecast.Daily.Days[0].PrecipitationProbability * 100;

                    }
                // Get historical temperature of the day before
                tsk = client.GetTimeMachineWeatherAsync(_settings.Latitude, _settings.Longitude, DateTime.Now.AddDays(-1), _settings.Unit, _settings.Language);
                tsk.Wait();
                var history = tsk.Result;
                // find the al up precipitation and max temperature
                float HistMaxTemp = 0;
                float HistTotalPrecipitation = 0;
                if (history.Daily.Days != null)
                    if (history.Daily.Days.Count > 0)
                    {
                        if (history.Daily.Days[0].HighTemperature >= HistMaxTemp)
                            HistMaxTemp = history.Daily.Days[0].HighTemperature;
                        if (history.Daily.Days[0].PrecipitationAccumulation >= HistTotalPrecipitation)
                            HistTotalPrecipitation = history.Daily.Days[0].PrecipitationAccumulation;
                    }


                needToSrpinkle.NeedTo = false;
                //Do all math with fuzzy logic
                foreach (var objective in _fuzzySprinklers)
                {
                    //Found the righ range
                    if ((ForecastMaxTemp >= objective.TempMin) && (ForecastMaxTemp < objective.TempMax))
                    {
                        // How much it rained ?
                        if (HistTotalPrecipitation >= objective.RainMax)
                        {
                            needToSrpinkle.NeedTo = false;
                        }
                        // Will it rain for sure? and will it rain enough?
                        else if ((ForecastProbabilityPrecipitation >= _settings.PrecipitationPercentForecast) && (ForecastTotalPrecipitation >= objective.RainMax))
                        {
                            needToSrpinkle.NeedTo = false;
                        }
                        else
                        {   // so we need to sprinkler. Make the math how long with the correction factor
                            // first calculate proportion of time vs the theoritical maximum
                            needToSrpinkle.PercentageCorrection = (float)(((objective.RainMax - HistTotalPrecipitation) / objective.RainMax) * objective.SprinklingMax / 100.0);
                            needToSrpinkle.NeedTo = true;
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                return null;
            }
            return needToSrpinkle;
        }

        #endregion

        private HistorySprinkling GetHistorySprinkling()
        {
            try
            {
                var stream = DownloadAsync(FilePactProgramPath).GetAwaiter().GetResult();
                return new HistorySprinkling() { SprinklerPrograms = JsonConvert.DeserializeObject<List<SprinklerProgram>>(Encoding.Default.GetString(stream.ToArray())) };
            }
            catch (Exception)
            {
                StreamReader streamReader = new StreamReader(Path.Combine(LocalPath, PastProgramPath));
                try
                {
                    return new HistorySprinkling() { SprinklerPrograms = JsonConvert.DeserializeObject<List<SprinklerProgram>>(streamReader.ReadToEnd()) };
                }
                catch (Exception)
                {
                    return null;
                }                
            }
        }

        private static async Task<CloudBlobContainer> GetContainerAsync()

        {
            //Account
            CloudStorageAccount storageAccount = new CloudStorageAccount(
                new StorageCredentials(_azureBlobSetings.AccountName, _azureBlobSetings.AccountKey), true);
            //Client
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            //Container
            CloudBlobContainer blobContainer = blobClient.GetContainerReference(_azureBlobSetings.ContainerName);
            await blobContainer.CreateIfNotExistsAsync();
            //await blobContainer.SetPermissionsAsync(new BlobContainerPermissions() { PublicAccess = BlobContainerPublicAccessType.Blob });
            return blobContainer;
        }

        private static async Task<CloudBlockBlob> GetBlockBlobAsync(string blobName)
        {
            //Container
            CloudBlobContainer blobContainer = await GetContainerAsync();
            //Blob
            CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(blobName);
            return blockBlob;
        }

        public static async Task<MemoryStream> DownloadAsync(string blobName)

        {
            //Blob
            CloudBlockBlob blockBlob = await GetBlockBlobAsync(blobName);

            //Download
            using (var stream = new MemoryStream())
            {
                await blockBlob.DownloadToStreamAsync(stream);
                return stream;
            }
        }


        public static async Task UploadAsync(string blobName, string filePath)
        {
            //Blob
            CloudBlockBlob blockBlob = await GetBlockBlobAsync(blobName);
            //Upload
            using (var fileStream = new StreamReader(filePath))
            {
                await blockBlob.UploadFromStreamAsync(fileStream.BaseStream);
            }
        }
    }
}
