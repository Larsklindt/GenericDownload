using GenericDownload.download;
using GenericDownload.ServiceConfiguration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericDownload
{
    class Program
    {

        static void Main(string[] args)
        {
            // indicates if firstrun action is to be performed
            Boolean firstRunner = false;
            // set specified configuration file
            ExeConfigurationFileMap configFileMap = new ExeConfigurationFileMap();
            if (args.Length > 0)
                configFileMap.ExeConfigFilename = args[0]; // full path to the config file
            if (args.Length > 1)
                firstRunner = Boolean.Parse(args[1]); // firstrun action, if not specified -> False
            // Get the mapped configuration file
            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);

            //now on use config object

            AppSettingsSection section = (AppSettingsSection)config.GetSection("appSettings");
            
            //// read custom configuration for download OIS Settings
            OISServiceConfigurationSection oisSection = config.GetSection("oisdata") as OISServiceConfigurationSection;

            if (oisSection != null)
            {
                OISServiceConfigurationElementCollection collection = oisSection.CustomConfigurations;

                foreach (OISServiceConfigurationElement element in collection)
                {
                    IDownload id = DownloadFactory.createDownloader(element.Type);
                    id.setImportSettings(element);
                    Console.WriteLine(config.ConnectionStrings.ConnectionStrings["database"].ConnectionString);
                    id.setExportSettings(new SqlConnection(config.ConnectionStrings.ConnectionStrings["database"].ConnectionString));
                    id.startDownload(firstRunner);
                }
            }
            
            //// read custom configuration for scraping Web Settings
            ScrapingServiceConfigurationSection scrapingSection = config.GetSection("scraping") as ScrapingServiceConfigurationSection;

            if (scrapingSection != null)
            {
                ScrapingServiceConfigurationElementCollection collection = scrapingSection.CustomConfigurations;

                foreach (ScrapingServiceConfigurationElement element in collection)
                {
                    IDownload id = DownloadFactory.createDownloader(element.Type);
                    id.setImportSettings(element);
                    Console.WriteLine(config.ConnectionStrings.ConnectionStrings["database"].ConnectionString);
                    id.setExportSettings(new SqlConnection(config.ConnectionStrings.ConnectionStrings["database"].ConnectionString));
                    id.startDownload(firstRunner);
                }
            }
        }
    }
}
