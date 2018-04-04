using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericDownload.ServiceConfiguration
{
    /// <summary>
    /// Custom configuration element.
    /// </summary>
    public class ScrapingServiceConfigurationElement : ConfigurationSection
    {
        /// <summary>
        /// Gets the configuration service key.
        /// </summary>
        [ConfigurationProperty("key", IsRequired = true)]
        public string Key
        {
            get
            {
                string value = base["key"] as String;

                return value ?? String.Empty;
            }
        }

        /// <summary>
        /// Gets the configuration service type.
        /// </summary>
        [ConfigurationProperty("type", IsRequired = true)]
        public string Type
        {
            get
            {
                string value = base["type"] as String;

                return value ?? String.Empty;
            }
        }


        /// <summary>
        /// Gets the configuration url to retrieve historic real property value data
        /// </summary>
        [ConfigurationProperty("svurUrl", IsRequired = true)]
        public string SvurUrl
        {
            get
            {
                string value = base["svurUrl"] as String;

                return value ?? String.Empty;
            }
        }

        /// <summary>
        /// Gets the configuration database to store data
        /// </summary>
        [ConfigurationProperty("databaseInsertSQL", IsRequired = true)]
        public string DatabaseInsertSQL
        {
            get
            {
                string value = base["databaseInsertSQL"] as String;

                return value ?? String.Empty;
            }
        }

        /// <summary>
        /// Gets the configuration database to update data
        /// </summary>
        [ConfigurationProperty("databaseUpdateSQL", IsRequired = true)]
        public string DatabaseUpdateSQL
        {
            get
            {
                string value = base["databaseUpdateSQL"] as String;

                return value ?? String.Empty;
            }
        }
    }
}
