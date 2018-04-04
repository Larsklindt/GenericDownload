using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericDownload.ServiceConfiguration
{
    /// <summary>
    /// Custom configuration section to contain the configuration element collection.
    /// </summary>
    class OISServiceConfigurationSection : ConfigurationSection
    {
        /// <summary>
        /// Gets the collection of custom configuration elements.
        /// </summary>
        [ConfigurationProperty("serviceConfigurations")]
        public OISServiceConfigurationElementCollection CustomConfigurations
        {
            get
            {
                OISServiceConfigurationElementCollection collection = base["serviceConfigurations"] as OISServiceConfigurationElementCollection;

                return collection ?? new OISServiceConfigurationElementCollection();
            }
        }

    }
}
