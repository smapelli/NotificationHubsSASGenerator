using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NotificationHubsSASGenerator
{
    internal static class ConfigurationManagerHelper
    {
        public static bool Exists()
        {
            return Exists(Assembly.GetEntryAssembly());
        }

        public static bool Exists(Assembly assembly)
        {
            return File.Exists(assembly.Location + ".config");
        }

        public static bool CheckConfigFileIsPresent()
        {
            return File.Exists(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
        }
    }
}
