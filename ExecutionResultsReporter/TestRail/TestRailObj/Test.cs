using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExecutionResultsReporter.TestRail.TestRailObj
{
    public class Test
    {
        public string id { get; set; }
        public string case_id { get; set; }
        public string status_id { get; set; }
        public string assignedto_id { get; set; }
        public string run_id { get; set; }
        public string title { get; set; }
        public string type_id { get; set; }
        public string priority_id { get; set; }
        public string estimate { get; set; }
        public string estimate_forecast { get; set; }
        public string refs { get; set; }
        public string milestone_id { get; set; }
        public string custom_isreview { get; set; }
        public string custom_preconds { get; set; }
        public string custom_steps_separated { get; set; }
    }
}
