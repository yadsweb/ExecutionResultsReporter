﻿using System.Collections.Generic;

namespace ExecutionResultsReporter.TestRail.TestRailObj
{
    public class TestCase
    {
       public string id { get; set; }
       public string title { get; set; }
       public string section_id { get; set; }
       public string type_id { get; set; }
       public string priority_id { get; set; }
       public string milestone_id { get; set; }
       public string refs { get; set; }
       public string created_by { get; set; }
       public string created_on { get; set; }
       public string updated_by { get; set; }
       public string updated_on { get; set; }
       public string estimate { get; set; }
       public string estimate_forecast { get; set; }
       public string suite_id { get; set; }
       public bool custom_isreview { get; set; }
       public string custom_preconds { get; set; }
       public List<TestCaseStep> custom_steps_separated { get; set; }

        public TestCase()
        {
            custom_steps_separated = new List<TestCaseStep>();
        }
    }
}
