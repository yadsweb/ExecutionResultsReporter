using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResultReporterExecutable
{
    public class jsonReport
    {
        public string project_id { get; set; }
        public string testrail_url { get; set; }
        public string testrail_username { get; set; }
        public string testrail_password { get; set; }
        public List<Suite> suites { get; set; }
    }
    public class Spec
    {
        public string status { get; set; }
        public string title { get; set; }
        public string duration { get; set; }
        public string stackTrace { get; set; }
    }

    public class Suite
    {
        public string title { get; set; }
        public string duration { get; set; }
        public List<Spec> specs { get; set; }
    }
}
