using System;
using System.Collections.Generic;
using System.Linq;
using ExecutionResultsReporter.TestRail.TestRailObj;
using log4net;
using Newtonsoft.Json;

namespace ExecutionResultsReporter.TestRail
{
    public static class TestRailIntegrator
    {
        private static readonly ILog Log = LogManager.GetLogger("TestRailIntegrator");
        private static readonly ScenariosExtractor Extractor = new ScenariosExtractor();
        private static readonly ConfigurationRetriver CfgRetriver = new ConfigurationRetriver();

        public static void RunIntegration(string action, string fileStore, string appConfigPath, string testRailProject, string testDllPath, string planId)
        {
            if (action == "create_complete_test_plan")
            {
                var testPlan = RunIntegration(action, appConfigPath, testRailProject, testDllPath, planId);
                Log.Info("Converting test plan object with id '" + testPlan.id + "' to string.");
                var testPlanAsString = JsonConvert.SerializeObject(testPlan);
                Log.Info("Plan retrieved successful");
                new FileStoreIteractions(fileStore).WriteInfoToSotre(testPlanAsString);
                return;
            }
            if (action == "close_test_plan")
            {
                if (string.IsNullOrEmpty(planId))
                {
                    planId = new FileStoreIteractions(fileStore).GetPlanFromFileStore().id;
                }
                RunIntegration(action, appConfigPath, testRailProject, testDllPath, planId);
            }
            else
            {
                Log.Info("Chosen action '"+action+"' didn't require file interactions.");
                RunIntegration(action, appConfigPath, testRailProject, testDllPath, planId);
            }

        }

        public static TestPlan RunIntegration(string action, string appConfigPath, string testRailProject, string testDllPath, string planId)
        {
            var appConfig = CfgRetriver.ReturnConfiguration(appConfigPath);
            var reporter = new TestRailReporter(Convert.ToInt32(testRailProject));
            Log.Info("Trying to create new test rail api client!");
            var user = appConfig.AppSettings.Settings["TestRail.Username"].Value;
            var apiClient = new ApiClient(appConfig.AppSettings.Settings["TestRail.Url"].Value)
            {
                User = user,
                Password = appConfig.AppSettings.Settings["TestRail.Password"].Value
            };
            Log.Info("Creation of api client with test rail url '" + appConfig.AppSettings.Settings["TestRail.Url"].Value + "', test rail user name '" + appConfig.AppSettings.Settings["TestRail.Username"].Value + "' and test rail password '" + appConfig.AppSettings.Settings["TestRail.Password"].Value + "' successful.");
            reporter.SetApiClient(apiClient);
            
            Log.Info("Loading all test suites and cases for project '" + testRailProject + "'.");
            reporter.LoadAllSuitesForProject();
            reporter.LoadAllTestCasesForProject();
            Log.Info("Loading successful.");
            switch (action.ToLower())
            {
                case "add_scenarios":
                    {
                        var scenarious = Extractor.RetriveScenarioInformation(testDllPath);
                        var executionCategory = appConfig.AppSettings.Settings["Execution.Tag"].Value;
                        Log.Info("Trying to add missing test cases to test rail.");
                        var relevantTestCase = reporter.CreatListWithRelevantTestCaseObjects(scenarious, executionCategory);
                        Log.Info("Test rail now include this test cases.");
                        foreach (var testCase in relevantTestCase)
                        {
                            Log.Info("\t " + testCase.title);
                        }
                        return null;
                    }
                case "create_complete_test_plan":
                    {
                        var scenarious = Extractor.RetriveScenarioInformation(testDllPath);
                        var executionCategory = appConfig.AppSettings.Settings["Execution.Tag"].Value;
                        Log.Info("Trying to add missing test cases to test rail.");
                        var relevantTestCase = reporter.CreatListWithRelevantTestCaseObjects(scenarious, executionCategory);
                        Log.Info("Reloading all test suites and cases for project '" + testRailProject + "'.");
                        reporter.LoadAllSuitesForProject();
                        reporter.LoadAllTestCasesForProject();
                        Log.Info("Loading successful.");
                        Log.Info("Test rail now include this test cases.");
                        foreach (var testCase in relevantTestCase)
                        {
                            Log.Info("\t " + testCase.title);
                        }
                        
                        if (!relevantTestCase.Any())
                        {
                            Log.Info("There were no test cases marked for execution.");
                            return null;
                        }
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
                        var executionTag = "";
                        var environment = "";
                        try
                        {
                            executionTag = appConfig.AppSettings.Settings["Execution.Tag"].Value;
                        }
                        catch (Exception)
                        {
                            Log.Warn("Execution tag (Execution.tag) from app.config can not be retrieved");
                        }
                        try
                        {
                            environment = appConfig.AppSettings.Settings["Execution.Environment"].Value;
                        }
                        catch (Exception)
                        {
                            Log.Warn("Execution environment (Execution.Environment) from app.config can not be retrieved");
                        }
                        var testPlanName = "Automation test plan from '" + DateTime.UtcNow.ToString("yyyy/MM/dd hh:mm:ss") +
                                           "(UTC)' for tests with category '" + executionTag +
                                           "' and environment '" + environment + "'";
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
                        Log.Info("Retrieving configurations for project " + testRailProject + " from test rail.");
                        var testConfigurations = reporter.RetriveConfigurationsForProject();
                        Log.Info(testConfigurations.Count + " configuration groups retrieved from test rail for project " + testRailProject);
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
                        Log.Info("Trying to find user id of user with e-mail: " + user);
                        var userId = reporter.ReturnUserId(user);
                        if (string.IsNullOrEmpty(userId))
                        {
                            Log.Warn("User id is empty so we set the it to default value of 1!");
                            userId = "1";
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
                                    assignedto_id = userId,
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
                                    assignedto_id = userId,
                                    include_all = false,
                                    case_ids = casIds,
                                };
                                reporter.AddPlanEntryToTestPlan(testPlan.id, newPlanEntry);
                            }
                        }
                        return JsonConvert.DeserializeObject<TestPlan>(reporter.GetTestPlanAsString(testPlan.id));
                    }
                case "close_test_plan":
                    {
                        Log.Info("Trying to close plan with id '" + planId + "'");
                        var plan = reporter.CloseTestPlan(planId);
                        Log.Info("Plan with id '" + plan.id + "' closed successful.");
                        return null;
                    }
                default:
                    {
                        throw new Exception("Argument '" + action + "' didn't match any expected action ('add_scenarios','create_complete_test_plan','close_test_plan')!");
                    }
            }
        }
    }
}
