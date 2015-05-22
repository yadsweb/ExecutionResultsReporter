using System;
using System.Collections.Generic;

namespace ExecutionResultsReporter
{
    public class TestCaseExecutionData
    {
        public TestCaseExecutionData()
        {
            ScenarioCategories = new List<string>();
            AdditionalData = new List<string>();
            ScenarioSteps = new List<string>();
        }

        public String FeatureName { get; set; }
        public String FileStore { get; set; }
        public String KnownIssues { get; set; }
        public String Configurations { get; set; }
        public String ScenarioName { get; set; }
        public List<String> ScenarioCategories { get; private set; }
        public String StartDate { get; set; }
        public String EndDate { get; set; }
        public String Duration { get; set; }
        public String Status { get; set; }
        public String FailingStep { get; set; }
        public String FailureStackTrace { get; set; }
        public String ScreenShotLocation { get; set; }
        public String Browser { get; set; }
        public String Site { get; set; }
        public List<String> ScenarioSteps { get; private set; }
        public List<String> AdditionalData { get; private set; }
    }
}
