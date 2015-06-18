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

        ////Tests are not implemented yet commented method was just for development purposes
        //[Test]
        //public void SimpleTest()
        //{
        //    //var Data = new DataParser();
        //    //var trololo = new List<KeyValuePair<string, string>>();
        //    var dictionary = new List<KeyValuePair<string, string>>()
        //    {
        //        new KeyValuePair<string, string>( "ScenarioName", "Tickets are able to be purchased" ),
        //        new KeyValuePair<string, string>( "ScenarioCategories", "trololololololo" ), 
        //        new KeyValuePair<string, string>("StartDate","2015-04-27 00:00:10"),
        //        new KeyValuePair<string, string>("EndDate","2015-04-27 00:00:10"),
        //        new KeyValuePair<string, string>( "FileStore", @"c:\store.txt" ), 
        //        //new KeyValuePair<string, string>( "Configurations", "WIN8, Chrome 35 : Bingo Godz" ), 
        //        new KeyValuePair<string, string>( "Status", "passed" ), 
        //        new KeyValuePair<string, string>( "KnownIssues", "cms-12" ), 
        //        new KeyValuePair<string, string>( "scenariostep", "Given I create new unique player with 'default' data and save it to context under key 'newPlayer'" ), 
        //        new KeyValuePair<string, string>( "scenariostep", "And I create new bingo game with game code 'unique' and data 'default_template'" ), 
        //        new KeyValuePair<string, string>( "scenariostep", "And I create new bingo group with description 'unique' and data 'default_template'" ), 
        //        new KeyValuePair<string, string>( "scenariostep", "And I create new bingo prize configuration with description 'unique' and data 'default_template'" ), 
        //        new KeyValuePair<string, string>( "scenariostep", "And I create new bingo room with room code 'unique' and data 'default_template'" ), 
        //        new KeyValuePair<string, string>( "scenariostep", "And I add room with room code from 'store:globalContext' to group with description from 'store:globalContext'" ), 
        //        new KeyValuePair<string, string>( "scenariostep", "And I add site with site code 'dummy' to room with room code from 'store:globalContext'" ), 
        //        new KeyValuePair<string, string>( "scenariostep", "And I create new schedule for group with description from 'store:globalContext', game with game code from 'store:globalContext', prize configuration with description from 'store:globalContext', is special 'false', day 'today', start time 'default', end time 'default' and frequency '120'" ), 
        //        new KeyValuePair<string, string>( "scenariostep", "And I wait '65' seconds for scheduler to take the game" ), 
        //        new KeyValuePair<string, string>( "scenariostep", "And I launch game with room code 'store:globalContext', theme 'bingostars', site code 'dummy' for player from context under key 'newPlayer'" ), 
        //        new KeyValuePair<string, string>( "scenariostep", "When The game is loaded" ), 
        //        new KeyValuePair<string, string>( "scenariostep", "And I wait '1' seconds" ), 
        //        new KeyValuePair<string, string>( "scenariostep", "And I save current player balance to context under key 'initialBalance'" ), 
        //        new KeyValuePair<string, string>( "scenariostep", "And I select '3' tickets from purchase tickets form" ), 
        //        new KeyValuePair<string, string>( "scenariostep", "Then I see that selected tickets text is showing '3'" ), 
        //        new KeyValuePair<string, string>( "scenariostep", "When I click on element with locator 'buyTicketsButton'" ), 
        //        new KeyValuePair<string, string>( "scenariostep", "Then I see message which inform me that '3' tickets are purchased for total amount '£3.00'" ), 
        //        new KeyValuePair<string, string>( "scenariostep", "And I see that current player balance is equal to the sum of 'scenarioContext:initialBalance and -3.00'" ), 
        //    };
        //    var appConfig = CfgRetriver.ReturnConfiguration(@"C:\Users\USER\Documents\GitHub\AutomatedUItesting\AutomatedUITesting.Tests\App.config");
        //    var reporter = new TestRailReporter(41, dictionary);
        //    var apiClient = new ApiClient(appConfig.AppSettings.Settings["TestRail.url"].Value)
        //    {
        //        User = appConfig.AppSettings.Settings["TestRail.username"].Value,
        //        Password = appConfig.AppSettings.Settings["TestRail.password"].Value
        //    };
        //    reporter.SetApiClient(apiClient);
        //    reporter.Report();
        //}
    }
}
