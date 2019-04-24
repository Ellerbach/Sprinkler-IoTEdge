using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SprinklerNetCore.Models;

namespace SprinklerNetCore
{
    public class Program
    {
        //public static SiteInformation SiteInformation { get; set; }

        public static void Main(string[] args)
        {
            //SiteInformation = SiteInformation.LoadConfiguration();
            //SiteInformation.SaveConfiguration();
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
