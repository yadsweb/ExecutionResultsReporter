using System.Collections.Generic;

namespace ExecutionResultsReporter.TestRail.TestRailObj
{
    public class PlanEntry
    {
        public string id { get; set; }
        public string suite_id { get; set; }
        public string assignedto_id { get; set; }
        public bool include_all { get; set; }
        public List<string> config_ids { get; set; }
        public List<string> case_ids { get; set; }
        public List<TestRun> runs { get; set; }

    }
}
