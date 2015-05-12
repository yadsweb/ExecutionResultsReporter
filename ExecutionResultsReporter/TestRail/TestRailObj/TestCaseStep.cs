using System;

namespace ExecutionResultsReporter.TestRail.TestRailObj
{
    class TestCaseStep
    {
        public TestCaseStep(string Content, string Expected)
        {
            content = Content;
            expected = Expected;
        }

        public String content { get; set; }
        public String expected { get; set; }
    }
}
