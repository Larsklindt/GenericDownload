using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericDownload.ServiceConfiguration
{
    /// <summary>
    /// Configuration element collection for <see cref="OISServiceConfigurationElement"/>
    /// </summary>
    public class OISServiceConfigurationElementCollection : ConfigurationElementCollection
    {
        /// <summary>
        /// Get the new element.
        /// </summary>
        /// <returns>A <see cref="OISServiceConfigurationElement"/> instance.</returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return new OISServiceConfigurationElement();
        }

        /// <summary>
        /// Get the element key.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>A key of the element or empty string.</returns>
        protected override object GetElementKey(ConfigurationElement element)
        {
            OISServiceConfigurationElement customElement = element as OISServiceConfigurationElement;

            if (customElement != null)
            {
                return customElement.Key;
            }

            return String.Empty;
        }

        /// <summary>
        /// Gets or sets <see cref="OISServiceConfigurationElement"/> at specified index.
        /// </summary>
        public OISServiceConfigurationElement this[int index]
        {
            get { return base.BaseGet(index) as OISServiceConfigurationElement; }
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
