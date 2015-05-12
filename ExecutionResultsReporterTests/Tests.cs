using System;
using System.Collections.Generic;
using NUnit.Framework;
using ExecutionResultsReporter.DataBase;
using ExecutionResultsReporter;
using ExecutionResultsReporter.TestRail;

namespace ExecutionResultsReporterTests
{
    public class Tests
    {
        private readonly ConfigurationRetriver CfgRetriver = new ConfigurationRetriver();

        [Test]
        public void SimpleTest()
        {
            var Data = new DataParser();
            var trololo = new List<KeyValuePair<string, string>>();
            var dictionary = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>( "ScenarioName", "I want to see base page elements for bet history page" ),
                new KeyValuePair<string, string>( "ScenarioCategories", "trololololololo" ), 
                new KeyValuePair<string, string>("StartDate","2015-04-27 00:00:10"),
                new KeyValuePair<string, string>("EndDate","2015-04-27 00:00:10"),
                new KeyValuePair<string, string>( "FileStore", @"c:\store.txt" ), 
                new KeyValuePair<string, string>( "Configurations", "WIN8, Chrome 35 : Bingo Godz" ), 
                new KeyValuePair<string, string>( "Status", "passed" ), 
                new KeyValuePair<string, string>( "KnownIssues", "cms-12" ), 
                new KeyValuePair<string, string>( "scenariostep", "Log in " ), 
                new KeyValuePair<string, string>( "scenariostep", "Sing trololololo" ), 
            };
            var appConfig = CfgRetriver.ReturnConfiguration(@"C:\Users\USER\Documents\GitHub\AutomatedUItesting\AutomatedUITesting.Tests\App.config");
            var reporter = new TestRailReporter(24,dictionary);
            var apiClient = new ApiClient(appConfig.AppSettings.Settings["TestRail.url"].Value)
            {
                User = appConfig.AppSettings.Settings["TestRail.username"].Value,
                Password = appConfig.AppSettings.Settings["TestRail.password"].Value
            };
            reporter.SetApiClient(apiClient);
            reporter.Report();
        }
    }
}
