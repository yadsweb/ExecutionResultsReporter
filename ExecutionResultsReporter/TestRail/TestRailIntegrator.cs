using System;
using System.Collections.Generic;
using System.Linq;
using ExecutionResultsReporter.TestRail.TestRailObj;
using log4net;

namespace ExecutionResultsReporter.TestRail
{
    static class TestRailIntegrator
    {
        private static readonly ILog Log = LogManager.GetLogger("TestPlanCreator");
        private static readonly ScenariosExtractor Extractor = new ScenariosExtractor();
        private static readonly ConfigurationRetriver CfgRetriver = new ConfigurationRetriver();
        private static void Main(string[] args)
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
                var appConfig = CfgRetriver.ReturnConfiguration(args[2]);
                var reporter = new TestRailReporter(Convert.ToInt32(args[3]));
                Log.Info("Trying to create new test rail api client!");
                var apiClient = new ApiClient(appConfig.AppSettings.Settings["TestRail.url"].Value)
                {
                    User = appConfig.AppSettings.Settings["TestRail.username"].Value,
                    Password = appConfig.AppSettings.Settings["TestRail.password"].Value
                };
                Log.Info("Creation of api client with test rail url '" + appConfig.AppSettings.Settings["TestRail.url"].Value + "', test rail user name '" + appConfig.AppSettings.Settings["TestRail.username"].Value + "' and test rail password '" + appConfig.AppSettings.Settings["TestRail.password"].Value + "' successful.");
                reporter.SetApiClient(apiClient);
                switch (args[0].ToLower())
                {
                    case "add_scenarios":
                        {
                            var scenarious = Extractor.RetriveScenarioInformation(args[4]);
                            var executionCategory = appConfig.AppSettings.Settings["Execution.tag"].Value;
                            Log.Info("Trying to add missing test cases to test rail.");
                            var relevantTestCase = reporter.CreatListWithRelevantTestCaseObjects(scenarious, executionCategory);
                            Log.Info("Test rail now include this test cases.");
                            foreach (var testCase in relevantTestCase)
                            {
                                Log.Info("\t " + testCase.title);
                            }
                            break;
                        }
                    case "create_complete_test_plan":
                        {
                            var scenarious = Extractor.RetriveScenarioInformation(args[4]);
                            var executionCategory = appConfig.AppSettings.Settings["Execution.tag"].Value;
                            Log.Info("Loading all test suites for project '" + args[3] + "'.");
                            reporter.LoadAllSuitesForProject();
                            Log.Info("Loading successful.");
                            Log.Info("Loading all test cases for project '" + args[3] + "'.");
                            reporter.LoadAllTestCasesForProject();
                            Log.Info("Loading successful.");
                            Log.Info("Creating a list with only relevant test cases (test cases which have category equals to the execution tag).");
                            var relevantTestCase = reporter.CreatListWithRelevantTestCaseObjects(scenarious, executionCategory);
                            if (!relevantTestCase.Any())
                            {
                                Log.Info("There were no test cases marked for execution.");
                                return;
                            }
                            Log.Info("List with '" + relevantTestCase.Count + "' test cases created successful.");
                            Log.Info("Creating a list with all suites for relevant test cases");
                            var suites = new List<string>();
                            foreach (var testcase in relevantTestCase.Where(testcase => !suites.Contains(testcase.suite_id)))
                            {
                                suites.Add(testcase.suite_id);
                            }
                            Log.Info("List with '" + suites.Count + "' suites created.");
                            Log.Info("Creating a list with grouped by suites test cases");
                            var groupedBySuiteTestCases = suites.Select(suite => relevantTestCase.Where(testcase => testcase.suite_id == suite).ToList()).ToList();
                            Log.Info("List with '" + groupedBySuiteTestCases.Count + "' groups created.");
                            var testPlanName = "Automation test plan from '" + DateTime.UtcNow.ToString("yyyy/MM/dd hh:mm:ss") +
                                               "(UTC)' for tests with category '" + appConfig.AppSettings.Settings["Execution.tag"].Value +
                                               "' and environment '" + appConfig.AppSettings.Settings["Execution.Environment"].Value + "'";
                            Log.Info("Creating test plan with name: " + testPlanName);
                            var testPlan = reporter.CreateTestPlan(testPlanName, "Test plan created by automation to store tests results");
                            Log.Info("Plan with id '" + testPlan.id + "' created successfully.");
                            Log.Info("Retrieving configurations from app.config");
                            var testConfigurationsAppConfig = new List<string>();
                            try
                            {
                                Log.Info("TestRail.Configurations element is present in appSetting trying to extract configurations delimited with ' : '.");
                                var configs = appConfig.AppSettings.Settings["TestRail.Configurations"].Value.Split(new[] { " : " }, StringSplitOptions.None);
                                testConfigurationsAppConfig.AddRange(configs);
                            }
                            catch (Exception)
                            {
                                Log.Warn("Exception appear when trying to read 'TestRail.Configurations' element from app.config, we assume it is not present and will continue.");
                            }
                            Log.Info(testConfigurationsAppConfig.Count + " configurations retrieved from app.config");
                            Log.Info("Retrieving configurations for project " + args[3] + " from test rail.");
                            var testConfigurations = reporter.RetriveConfigurationsForProject();
                            Log.Info(testConfigurations.Count + " configuration groups retrieved from test rail for project " + args[3]);
                            var relevantConfigurationIdsGrouped = new List<List<string>>();
                            if (testConfigurations.Any() && testConfigurationsAppConfig.Any())
                            {
                                Log.Info("Creating a list with only relevant configurations.");
                                foreach (var configurationGroup in testConfigurations)
                                {
                                    var tempList = new List<string>();
                                    foreach (var config in configurationGroup.configs)
                                    {
                                        foreach (var configName in testConfigurationsAppConfig)
                                        {
                                            if (configName.Equals(config.name))
                                            {
                                                Log.Debug("Match between test rail configuration name and configuration name in app.config ('" + config.name + "') found, adding its id '" + config.id + "' to temp list.");
                                                tempList.Add(config.id);
                                                continue;
                                            }
                                            Log.Debug("Name '" + configName + "' for configuration taken from app.config didn't match '" + config.name + "' taken from test rail.");
                                        }
                                    }
                                    relevantConfigurationIdsGrouped.Add(tempList);
                                }
                                Log.Info("List created with '" + relevantConfigurationIdsGrouped.Count + "' configuration groups.");
                            }
                            if (relevantConfigurationIdsGrouped.Any())
                            {
                                Log.Info("Creating partial combinations from relevant configurations list.");
                                var currentCombinations = new List<string>();
                                var allConfigIds = new List<string>();
                                var combinations = new List<List<string>>();

                                foreach (var configGroup in relevantConfigurationIdsGrouped)
                                {
                                    if (!currentCombinations.Any())
                                    {
                                        currentCombinations = configGroup;
                                    }
                                    else
                                    {
                                        foreach (var configId in configGroup)
                                        {
                                            foreach (var conbinationConfigId in currentCombinations)
                                            {
                                                Log.Debug("Adding row with ids '" + configId + " , " + conbinationConfigId + "' to the combination list.");
                                                combinations.Add(new List<string> { configId, conbinationConfigId });
                                                if (!allConfigIds.Contains(configId))
                                                {
                                                    allConfigIds.Add(configId);
                                                }
                                                if (!allConfigIds.Contains(conbinationConfigId))
                                                {
                                                    allConfigIds.Add(conbinationConfigId);
                                                }
                                            }
                                        }
                                    }
                                }

                                Log.Info("Combinations created successful.");

                                foreach (var group in groupedBySuiteTestCases)
                                {
                                    var caseIds = @group.Select(testRailTestCaseObj => testRailTestCaseObj.id).ToList();
                                    var runs = new List<TestRun>();
                                    foreach (var configIds in combinations)
                                    {
                                        var run = new TestRun
                                        {
                                            include_all = false,
                                            config_ids = configIds,
                                            case_ids = caseIds
                                        };
                                        runs.Add(run);
                                    }
                                    var newPlanEntry = new PlanEntry
                                    {
                                        id = null,
                                        suite_id = group.First().suite_id,
                                        assignedto_id = "1",
                                        include_all = false,
                                        case_ids = caseIds,
                                        config_ids = allConfigIds,
                                        runs = runs,

                                    };
                                    reporter.AddPlanEntryToTestPlan(testPlan.id, newPlanEntry);
                                }

                            }
                            else
                            {
                                foreach (var group in groupedBySuiteTestCases)
                                {
                                    var casIds = @group.Select(testRailTestCaseObj => testRailTestCaseObj.id).ToList();
                                    var newPlanEntry = new PlanEntry
                                    {
                                        id = null,
                                        suite_id = group.First().suite_id,
                                        assignedto_id = "1",
                                        include_all = false,
                                        case_ids = casIds,
                                    };
                                    reporter.AddPlanEntryToTestPlan(testPlan.id, newPlanEntry);
                                }
                            }
                            Log.Info("Reading updated test plan with id '" + testPlan.id + "' as a string.");
                            var planAsString = reporter.GetTestPlan(testPlan.id);
                            Log.Info("Plan retrieved successful");
                            new FileStoreIteractions(args[1]).WriteInfoToSotre(planAsString);
                            break;
                        }
                    case "close_test_plan":
                        {
                            var fileStore = new FileStoreIteractions(args[1]);
                            var planId = fileStore.GetPlanFromFileStore().id;
                            Log.Info("Trying to close plan with id '" + planId + "'");
                            var plan = reporter.CloseTestPlan(planId);
                            Log.Info("Plan with id '" + plan.id + "' closed successful.");
                            fileStore.DeleteFileStore();
                            break;
                        }
                    default:
                        {
                            throw new Exception("Argument '" + args[0] + "' didn't match any expected action ('add_scenarios','create_complete_test_plan','close_test_plan')!");
                        }
                }
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
