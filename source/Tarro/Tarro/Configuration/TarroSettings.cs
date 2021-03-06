﻿using System.Configuration;

namespace Tarro.Configuration
{
    internal class TarroSettings : ConfigurationSection
    {
        public static TarroSettings Settings { get; } = ConfigurationManager.GetSection("Tarro") as TarroSettings;

        [ConfigurationProperty("instanceName", IsRequired = false)]
        public string InstanceName
        {
            get { return (string)this["instanceName"]; }
            set { this["instanceName"] = value; }
        }
        [ConfigurationProperty("managementPort",DefaultValue = 2250, IsRequired = false)]
        public int ManagementPort
        {
            get { return (int)this["managementPort"]; }
            set { this["managementPort"] = value; }
        }
        [ConfigurationProperty("applications", IsRequired = false)]
        public ApplicationCollection Applications
        {
            get { return (ApplicationCollection)this["applications"]; }
        }
    }
}
