using System.Collections.Generic;

namespace ExecutionResultsReporter
{
    public class Reporter
    {
        private readonly List<IReporter> _specificReporters;

        private Reporter(List<IReporter> specificReporters)
        {
            _specificReporters = specificReporters;
        }

        public void Report()
        {
            foreach (var specificReporter in _specificReporters)
            {
                specificReporter.Report();
            }
        }
    }
}
