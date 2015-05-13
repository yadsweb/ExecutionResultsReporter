using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using ExecutionResultsReporter.TestRail;

namespace ResultReporterExecutable
{
    static class Executable
    {
        private static readonly ILog Log = LogManager.GetLogger("TestRailIntegrator");
        static void Main(string[] args)
        {
            try
            {
                if (args.Count() < 5)
                {
                    throw new ArgumentException("The tool require some parameters! \n " +
                                                "- action, this should have value 'add_scenarios' , 'create_complete_test_plan' or 'close_test_plan' \n " +
                                                "- path to file store \n " +
                                                "- path to app.config \n " +
                                                "- test rail project id" +
                                                "- path to dll which contains tests");
                }
                TestRailIntegrator.RunIntegration(args[0], args[1], args[2], args[3], args[4], null);

            }
            catch (Exception e)
            {
                Log.Error("Exception appear during execution!");
                Log.Error(e.Message);
                Log.Error(e.StackTrace);
            }
        }
    }
}
