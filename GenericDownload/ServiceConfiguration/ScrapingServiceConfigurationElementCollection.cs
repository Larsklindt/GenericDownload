using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericDownload.ServiceConfiguration
{
    /// <summary>
    /// Configuration element collection for <see cref="ScrapingServiceConfigurationElement"/>
    /// </summary>
    public class ScrapingServiceConfigurationElementCollection : ConfigurationElementCollection
    {
        /// <summary>
        /// Get the new element.
        /// </summary>
        /// <returns>A <see cref="ScrapingServiceConfigurationElement"/> instance.</returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return new ScrapingServiceConfigurationElement();
        }

        /// <summary>
        /// Get the element key.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>A key of the element or empty string.</returns>
        protected override object GetElementKey(ConfigurationElement element)
        {
            ScrapingServiceConfigurationElement customElement = element as ScrapingServiceConfigurationElement;

            if (customElement != null)
            {
                return customElement.Key;
            }

            return String.Empty;
        }

        /// <summary>
        /// Gets or sets <see cref="ScrapingServiceConfigurationElement"/> at specified index.
        /// </summary>
        public ScrapingServiceConfigurationElement this[int index]
        {
            get { return base.BaseGet(index) as ScrapingServiceConfigurationElement; }
            set
            {
                if (base.BaseGet(index) != null)
                {
                    base.BaseRemoveAt(index);
                }

                base.BaseAdd(index, value);
            }
        }

    }
}
