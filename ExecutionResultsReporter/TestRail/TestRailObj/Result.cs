using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExecutionResultsReporter.TestRail.TestRailObj
{
    public class Result
    {
        public string case_id { get; set; }
        public string status_id { get; set; }
        public string comment { get; set; }
        public string defects { get; set; }
        public string elapsed { get; set; }
        public string version { get; set; }
        public string assignedto_id { get; set; }
    }
}
