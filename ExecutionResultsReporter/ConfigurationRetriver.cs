using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExecutionResultsReporter
{
    public class ConfigurationRetriver
    {
        public Configuration ReturnConfiguration(String path)
        {
            var fileMap = new ExeConfigurationFileMap
            {
                ExeConfigFilename = path
            };

            return ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
        }
    }
}
