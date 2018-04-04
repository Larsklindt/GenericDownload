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
    public class OISServiceConfigurationElement : ConfigurationSection
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
        /// Gets the configuration service userid.
        /// </summary>
        [ConfigurationProperty("uid", IsRequired = true)]
        public string Uid
        {
            get
            {
                string value = base["uid"] as String;

                return value ?? String.Empty;
            }
        }

        /// <summary>
        /// Gets the configuration service password.
        /// </summary>
        [ConfigurationProperty("pwd", IsRequired = true)]
        public string Pwd
        {
            get
            {
                string value = base["pwd"] as String;

                return value ?? String.Empty;
            }
        }

        /// <summary>
        /// Gets the configuration service sql for fetching data.
        /// </summary>
        [ConfigurationProperty("sql", IsRequired = true)]
        public string Sql
        {
            get
            {
                string value = base["sql"] as String;

                return value ?? String.Empty;
            }
        }

        /// <summary>
        /// Gets the configuration service meta.
        /// </summary>
        [ConfigurationProperty("meta", IsRequired = true)]
        public string Meta
        {
            get
            {
                string value = base["meta"] as String;

                return value ?? String.Empty;
            }
        }

        /// <summary>
        /// Gets the configuration database sql for storing data.
        /// </summary>
        [ConfigurationProperty("databaseSQL", IsRequired = true)]
        public string DatabaseSQL
        {
            get
            {
                string value = base["databaseSQL"] as String;

                return value ?? String.Empty;
            }
        }

    }
}
