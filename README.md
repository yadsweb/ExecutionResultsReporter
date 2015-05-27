# ExecutionResultsReporter
Simple library which can be used to report test execution results to different sources.
 
Purpouse
==================

The main idea of this reporter is to: 

1. Report results from test execution to custom data base.

2. Report results from test execution to test rail. 

How to use? 
==================

The tool require some configuration, which should be provided by the app.config of exexuting assembly. Here is an example of needed parameters: 

```<add key="DBReporting.Host" value="" />
    <add key="DBReporting.Username" value="" />
    <add key="DBReporting.Password" value="" />
    <add key="DBReporting.DataBase" value="" />
    <add key="TestRail.url" value="" />
    <add key="TestRail.username" value="" />
    <add key="TestRail.password" value="" />```  

For the db part you need 'DBReporting.Host', 'DBReporting.Username', 'DBReporting.Password' and 'DBReporting.DataBase' which are pretty self explanatory. And for the test rail part you need test rail url and credentials. 

For the db if tables needed for storing the information are not created the tool will try to create them so only empty data base and user which have correct credentials should be needed. 

Both data base and test rail reporters are using simple data object which contains information that will be reported to mentioned systems. The data object look like this: 

``` public class TestCaseExecutionData
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
    } ```

This data object is created and filled by a parser which accept  ``` IEnumerable<KeyValuePair<string, string>> data ``` so the keys in mentioned enumerable will be check and if it match some property then its value will be populated in the data object. If the key didn't match any property then its value should be added to “AdditionalData” list of the data object.

Also there is a interface (Reporter) with only one method (Report()) which can be used for creating different implementations. Available implementations are: 

``` public class DbReporter : Ireporter ```
and 
``` public class TestRailReporter : Ireporter ```

The data base reporter require only the ``` List<KeyValuePair<string, string>> data ``` which will be parsed and send to the db and the Test rail reporter except this list require test rail project id.

The easy way to use DB reporter is just in your test after scenario finished you collect all needed information for the data object then construct it, create instance of the db reporter and call report method: 

```                     var List = new List<KeyValuePair<string, string>>{
                    new KeyValuePair<string, string>("ScenarioName", ScenarioContext.Current.ScenarioInfo.Title),
                    new KeyValuePair<string, string>("ScenarioCategories", tags),
                    new KeyValuePair<string, string>("StartDate", startDate.ToString()),
                    new KeyValuePair<string, string>("EndDate", DateTime.Now.ToString()),
                    new KeyValuePair<string, string>("KnownIssues", "CMS-123"),
                    new KeyValuePair<string, string>("FileStore", @"c:\store.txt"), 
                    new KeyValuePair<string, string>( "Configurations", "WIN8, Chrome 35 : Bingo Godz" ), 
                    new KeyValuePair<string, string>("Status", "passed"), 
...
};

                    var reporter = new DbReporter(List);
                    reporter.Report();

```

Have in mind that some of the data will not be used by the db reporter but most of it which will be send to relevant columns in the db is needed: 

Currently the db should have 2 tables: ``` CREATE TABLE IF NOT EXISTS `testcasearchive` (`ID` bigint(20) NOT NULL AUTO_INCREMENT,`TestCaseName` varchar(10000) DEFAULT NULL,`KnownIssues` varchar(10000) DEFAULT NULL,`Categories` text,PRIMARY KEY (`ID`)) ENGINE=InnoDB DEFAULT CHARSET=utf8; ```

and 

``` CREATE TABLE IF NOT EXISTS `testcasesresults` (`ID` bigint(20) NOT NULL AUTO_INCREMENT,`CaseID` bigint(20) NOT NULL,`StartDate` datetime DEFAULT NULL,`EndDate` datetime DEFAULT NULL,`Status` char(50) DEFAULT NULL,`FailingStep` varchar(1000) DEFAULT NULL,`ReasonForFailure` varchar(10000) DEFAULT NULL,`ScreenShotLocation` varchar(5000) DEFAULT NULL,`Site` varchar(50) DEFAULT NULL,`Browser` varchar(50) DEFAULT NULL,`AdditionalData` text,PRIMARY KEY (`ID`),KEY `CaseID` (`CaseID`),CONSTRAINT `CaseID` FOREIGN KEY (`CaseID`) REFERENCES `testcasearchive` (`ID`)) ENGINE=InnoDB DEFAULT CHARSET=utf8; ```

For the test rail the functionality is more complicated: 

1. There is a executable which can be used to add test suites, test sections, test cases and 
test plan with runs in test rail. After creating them it will write all relevant information to a file which can be read to retrieve run id's.

The executable can be run this way:

ResultReporterExecutable.exe create_complete_test_plan c:\store.txt "C:\Users\USER\Documents\GitHub\AutomatedUItesting\AutomatedUITesting.Tests\App.config" 24 "C:\Users\USER\Documents\GitHub\AutomatedUITests\GrosvenorAutomatedUiTests\bin\Debug\GrosvenorAutomatedUiTests.dll"

first parameter is action which can have values: 'add_scenarios' , 'create_complete_test_plan' , 'close_test_plan' or 'report_results_from_json'

report_results_from_json needs a json file with data like: 

``` {
  "testrail_url":"https://bedegaming.testrail.com/",
  "testrail_username":"sos+AutomatedUITests@bedegaming.com",
  "testrail_password":"Cgsye2hyS5Ubl0T3Dr2w",
  "project_id": 33,
  "suites": [
    {
      "title": "SUITE: Deposits Widget - Players Filter MultipleDeposits|",
      "duration": "60",
      "specs": [
        {
          "status": "passed",
          "title": "Given I deposit twice and When I create a deposits widget with configuration SinceMidnight/MinDeposit equal to the deposit amount done/Filter - No Filter, Then I should see ALL my deposits listed",
          "duration": "14s",
          "stackTrace": ""
        },
        {
          "status": "failed",
          "title": "Given I deposit twice and When I create a deposits widget with configuration SinceMidnight/MinDeposit equal to the deposit amount done/Filter - Has deposited more than once, Then I should see ALL my deposits listed",
          "duration": "14s",
          "stackTrace": "message: Expected 2 to equal 3."
        },
        {
          "status": "passed",
          "title": "Given I deposit more than once and When I create a deposits widget with configuration SinceMidnight/MinDeposit equal to the deposit amount done/Filter - Has deposited exactly once, Then I should NOT see any of my deposits",
          "duration": "14s",
          "stackTrace": ""
        }
      ]
    }
  ]
}
```

and a name of the test plan which will be created in test rail. Current data will be added to the name to make it unique. 

close_test_plan this will close already created and save to file test plan so you need to provide the name of file where the test plan is stored
