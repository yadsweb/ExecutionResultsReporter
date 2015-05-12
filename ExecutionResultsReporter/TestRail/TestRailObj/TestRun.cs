using System.Collections.Generic;

namespace ExecutionResultsReporter.TestRail.TestRailObj
{
    public class TestRun
    {
        public string id { get; set; }
        public string suite_id { get; set; }
        public bool include_all { get; set; }
        public List<string> case_ids { get; set; }
        public List<string> config_ids { get; set; }
    }
}
