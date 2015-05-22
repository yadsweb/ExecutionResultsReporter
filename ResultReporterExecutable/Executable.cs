using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using log4net;
using ExecutionResultsReporter.TestRail;
using Newtonsoft.Json;

namespace ResultReporterExecutable
{
    static class Executable
    {
        private static readonly ILog Log = LogManager.GetLogger("TestRailIntegrator");
        static void Main(string[] args)
        {
            try
            {
                if (args[0].ToLower().Equals("report_results_from_json"))
                {
                    if (args.Count() < 3)
                    {
                        throw new ArgumentException("For this action the tool require few additional parameter! \n " +
                                                    "- path to json file \n " +
                                                    "- name of test plan (the date in utc will be automatically added to it) \n ");
                    }
                    var jsonReport = JsonConvert.DeserializeObject<jsonReport>(File.ReadAllText(args[1]));
                    var scenarios = new List<ScenarioObj>();
                    foreach (var suite in jsonReport.suites)
                    {
                        scenarios.AddRange(suite.specs.Select(spec => new ScenarioObj
                        {
                            Name = spec.title,
                            FeatureName = suite.title,
                        }));
                    }
                    if (!scenarios.Any())
                    {
                        throw new Exception("No scenarios were retrieved!");
                    }
                    var reporter = new TestRailReporter(Convert.ToInt32(jsonReport.project_id));
                    Log.Info("Trying to create new test rail api client.");
                    var user = jsonReport.testrail_username;
                    Log.Info("With test rail url '" + jsonReport.testrail_url + "', test rail user name '" + user + "' and test rail password '" + jsonReport.testrail_password + "' successful.");
                    var apiClient = new ApiClient(jsonReport.testrail_url)
                    {
                        User = user,
                        Password = jsonReport.testrail_password
                    };
                    Log.Info("Creation of api client successful.");
                    reporter.SetApiClient(apiClient);
                    Log.Info("Loading all test suites and cases for project '" + jsonReport.project_id + "'.");
                    reporter.LoadAllSuitesForProject();
                    reporter.LoadAllTestCasesForProject();
                    Log.Info("Loading successful.");
                    var testPlan = TestRailIntegrator.DoAction("create_complete_test_plan", null, reporter, jsonReport.project_id, user, null, "", "", "", scenarios, args[2] + " " + DateTime.UtcNow.ToString("yyyy/MM/dd hh:mm:ss"));
                    Log.Info("Trying to add the results to plan with id '"+testPlan.id+"'.");
                    foreach (var suite in jsonReport.suites)
                    {
                        foreach (var spec in suite.specs)
                        {
                            var data = new List<KeyValuePair<string, string>>{
                            new KeyValuePair<string, string>("ScenarioName", spec.title),
                            new KeyValuePair<string, string>("Status", spec.status),
                            new KeyValuePair<string, string>("failurestacktrace",spec.stackTrace),
                            new KeyValuePair<string, string>("duration",spec.duration.ToString()),
                        };
                            reporter.SetData(data);
                            reporter.Report(testPlan);
                        }
                    }
                    TestRailIntegrator.DoAction("close_test_plan", null, reporter, null, null, testPlan.id, null, null, null, null, null);
                    return;
                }
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
