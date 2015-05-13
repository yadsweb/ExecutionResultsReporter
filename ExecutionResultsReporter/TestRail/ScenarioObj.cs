using System.Collections.Generic;

namespace ExecutionResultsReporter.TestRail
{
    public class ScenarioObj
    {
        public ScenarioObj()
        {
            TestCaseAttributes = new List<string>();
            CategoryAttribute = new List<string>();
        }

        public string Name { get; set; }
        public string FeatureName { get; set; }
        public List<string> TestCaseAttributes { get; set; }
        public List<string> CategoryAttribute { get; set; }
    }
}
