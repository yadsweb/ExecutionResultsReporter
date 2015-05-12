using System.Collections.Generic;

namespace ExecutionResultsReporter.TestRail.TestRailObj
{
    public class ConfigurationGroup
    {
        public string id { get; set; }
        public string name { get; set; }
        public string project_id { get; set; }
        public List<Configuration> configs { get; set; }
    }
}
