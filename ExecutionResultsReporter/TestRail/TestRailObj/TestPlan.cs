using System.Collections.Generic;

namespace ExecutionResultsReporter.TestRail.TestRailObj
{
    public class TestPlan
    {
        public string id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string milestone_id { get; set; }
        public string assignedto_id { get; set; }
        public bool is_completed { get; set; }
        public string completed_on { get; set; }
        public string passed_count { get; set; }
        public string blocked_count { get; set; }
        public string untested_count { get; set; }
        public string retest_count { get; set; }
        public string failed_count { get; set; }
        public string custom_status1_count { get; set; }
        public string custom_status2_count { get; set; }
        public string custom_status3_count { get; set; }
        public string custom_status4_count { get; set; }
        public string custom_status5_count { get; set; }
        public string custom_status6_count { get; set; }
        public string custom_status7_count { get; set; }
        public string project_id { get; set; }
        public string created_on { get; set; }
        public string created_by { get; set; }
        public string url { get; set; }
        public List<PlanEntry> entries { get; set; }
    }
}
