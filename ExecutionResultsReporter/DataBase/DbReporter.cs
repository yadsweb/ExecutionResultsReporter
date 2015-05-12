using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using MySql.Data.MySqlClient;
using log4net;

namespace ExecutionResultsReporter.DataBase
{
    /*
     * This class should be used to report results from test execution to custom DB.
     * In our case the DB is MySQL, the tool will try to create all relevant tables which it needs if they don't exist ine provided 
     * data base so only permissions for creating tables should be needed. 
     */

    public class DbReporter : IReporter
    {
        private readonly ILog _log = LogManager.GetLogger("DbReporter");
        private MySqlConnection _connection;
        private readonly List<KeyValuePair<string, string>> _data;

        public DbReporter(List<KeyValuePair<string, string>> data)
        {
            _data = data;
            CreateConnection();
        }

        private void CreateConnection()
        {
            var myConnectionString = "server=" + ConfigurationManager.AppSettings["DBReporting.Host"] + ";uid=" + ConfigurationManager.AppSettings["DBReporting.Username"] + ";pwd=" + ConfigurationManager.AppSettings["DBReporting.Password"] + ";database=" + ConfigurationManager.AppSettings["DBReporting.DataBase"] + ";";
            try
            {
                _connection = new MySqlConnection(myConnectionString);
            }
            catch (Exception)
            {
                _log.Error("Error when trying to create a connection to db with connection string: \n   " + myConnectionString);
                throw;
            }
        }

        private void CreatTables()
        {
            _log.Info("Trying to create tables if they don't exist.");
            var archiveCreateCommand = new MySqlCommand("CREATE TABLE IF NOT EXISTS `testcasearchive` (`ID` bigint(20) NOT NULL AUTO_INCREMENT,`TestCaseName` varchar(10000) DEFAULT NULL,`KnownIssues` varchar(10000) DEFAULT NULL,`Categories` text,PRIMARY KEY (`ID`)) ENGINE=InnoDB DEFAULT CHARSET=utf8;", _connection);
            var resultsCreateCommand = new MySqlCommand("CREATE TABLE IF NOT EXISTS `testcasesresults` (`ID` bigint(20) NOT NULL AUTO_INCREMENT,`CaseID` bigint(20) NOT NULL,`StartDate` datetime DEFAULT NULL,`EndDate` datetime DEFAULT NULL,`Status` char(50) DEFAULT NULL,`FailingStep` varchar(1000) DEFAULT NULL,`ReasonForFailure` varchar(10000) DEFAULT NULL,`ScreenShotLocation` varchar(5000) DEFAULT NULL,`Site` varchar(50) DEFAULT NULL,`Browser` varchar(50) DEFAULT NULL,`AdditionalData` text,PRIMARY KEY (`ID`),KEY `CaseID` (`CaseID`),CONSTRAINT `CaseID` FOREIGN KEY (`CaseID`) REFERENCES `testcasearchive` (`ID`)) ENGINE=InnoDB DEFAULT CHARSET=utf8;",_connection);
            _connection.Open();

            try
            {
                archiveCreateCommand.ExecuteNonQuery();
                resultsCreateCommand.ExecuteNonQuery();
            }
            catch (Exception)
            {
                _log.Error("When trying to create tables needed for reporting with! \n      Queries are:" +
                           "\n          CREATE TABLE IF NOT EXISTS `testcasearchive` (`ID` bigint(20) NOT NULL AUTO_INCREMENT,`TestCaseName` varchar(10000) DEFAULT NULL,`KnownIssues` varchar(10000) DEFAULT NULL,`Categories` text,PRIMARY KEY (`ID`)) ENGINE=InnoDB DEFAULT CHARSET=utf8;"
                           + "\n          CREATE TABLE IF NOT EXISTS `testcasesresults` (`ID` bigint(20) NOT NULL AUTO_INCREMENT,`CaseID` bigint(20) NOT NULL,`StartDate` datetime DEFAULT NULL,`EndDate` datetime DEFAULT NULL,`Status` char(50) DEFAULT NULL,`FailingStep` varchar(1000) DEFAULT NULL,`ReasonForFailure` varchar(10000) DEFAULT NULL,`ScreenShotLocation` varchar(5000) DEFAULT NULL,`Site` varchar(50) DEFAULT NULL,`Browser` varchar(50) DEFAULT NULL,`AdditionalData` text,PRIMARY KEY (`ID`),KEY `CaseID` (`CaseID`),CONSTRAINT `CaseID` FOREIGN KEY (`CaseID`) REFERENCES `testcasearchive` (`ID`)) ENGINE=InnoDB DEFAULT CHARSET=utf8;");
                throw;
            }
            finally
            {
                _connection.Close();
            }
            _log.Info("Tables Created or already exist.");
        }

        private bool IsTestExistInArchive(string testCaseName)
        {
            _log.Debug(
                "Query which will be execute is: \n select count(*) as totalTests from testcasearchive where TestCaseName=\"" +
                testCaseName.Replace("\"", "'") + "\";");
            var command =
                new MySqlCommand(
                    "select count(*) as totalTests from testcasearchive where TestCaseName=\"" +
                    testCaseName.Replace("\"", "'") + "\";",
                    _connection);
            _connection.Open();
            var reader = command.ExecuteReader();
            try
            {
                //The count query should return only one row, so we done only one read
                reader.Read();
                _log.Info("Method for checking if test exist in archive will return '" +
                          (reader.GetInt32(0) > 0) +
                          "'.");
                return reader.GetInt32(0) > 0;
            }
            catch (Exception e)
            {
                throw new Exception("Error when trying to check if test case with name " + testCaseName +
                                    " already exist in archive! \n Exception caught: \n" + e.StackTrace);
            }
            finally
            {
                reader.Close();
                _connection.Close();
            }
        }

        private void AddTestCaseToArchive(string testCaseName)
        {
            _log.Debug("Trying to add test case with scenario name '" + testCaseName + "' to test archive table!");
            if (!IsTestExistInArchive(testCaseName))
            {
                try
                {
                    _log.Debug(
                        "Query which will be execute is: \n Insert into testcasearchive (TestCaseName) values (\"" +
                        testCaseName.Replace("\"", "'") + "\");");
                    var command =
                        new MySqlCommand(
                            "Insert into testcasearchive (TestCaseName) values (\"" +
                            testCaseName.Replace("\"", "'") + "\");",
                            _connection);
                    _connection.Open();
                    command.ExecuteNonQuery();
                }
                catch (Exception)
                {
                    _log.Error("Error when trying to insert test case with name '" + testCaseName +
                               "' in to table testcasearchive!");
                    throw;
                }
                finally
                {
                    _connection.Close();
                }
            }
            _log.Info("Test case with name '" + testCaseName +
                      "' already exist in archive so no operations will be performed.");
        }

        public int ReturnCaseIdFromArchive(string testCaseName)
        {
            _log.Debug(
                "Query which will be execute is: \n select id from testcasearchive where TestCaseName=\"" +
                testCaseName.Replace("\"", "'") + "\";");
            var command =
                new MySqlCommand(
                    "select id from testcasearchive where TestCaseName=\"" +
                    testCaseName.Replace("\"", "'") + "\";",
                    _connection);
            _connection.Open();
            var reader = command.ExecuteReader();
            if (!reader.HasRows)
            {
                reader.Close();
                _connection.Close();
                throw new Exception("Error when trying to find id of test case with name " + testCaseName +
                                    " it looks like the query 'select id from testcasearchive where TestCaseName='" +
                                    testCaseName + "';' did not return any results!");
            }
            try
            {
                //The count query should return only one row so we done only one read
                //Even if by mistake we have duplicated entries in the db since i assume the 
                //use of this id will be just to map the test case name and id it should not be a problem to use the first one
                reader.Read();
                _log.Info("Method for selecting the id of test case '" + testCaseName +
                          "' will return id '" +
                          reader.GetInt32(0) + "'.");
                return reader.GetInt32(0);
            }
            catch (Exception e)
            {
                throw new Exception(
                    "Error when trying to take case id from archive for test case with name " +
                    testCaseName + "\n" + e.StackTrace);
            }
            finally
            {
                reader.Close();
                _connection.Close();
            }
        }

        public void ReportExecutionResult(int caseId, string startDate, string endDate, string status,
            string failingStep, string reasonForFailure, string screenshotLocation, string site,
            string browser, IEnumerable<string> additionalData)
        {
            startDate = !string.IsNullOrEmpty(startDate) ? "\""+startDate+"\"" : "null";
            endDate = !string.IsNullOrEmpty(endDate) ? "\"" + endDate + "\"" : "null";

            var additionaDataString = "";
            foreach (var row in additionalData)
            {
                _log.Debug("Adding '"+row+"' to additional data string.");
                additionaDataString = additionaDataString + row + " \r\n";
            }
            failingStep = ReplaceDoubleQuotesWithSinge(failingStep);
            reasonForFailure = ReplaceDoubleQuotesWithSinge(reasonForFailure);
            site = ReplaceDoubleQuotesWithSinge(site);
            browser = ReplaceDoubleQuotesWithSinge(browser);
            try
            {
                _log.Debug(
                    "Query which will be execute is: \n Insert into testcasesresults (CaseID, StartDate, EndDate, Status, FailingStep, ReasonForFailure, ScreenShotLocation, Site, Browser, AdditionalData) values (\"" +
                        caseId + "\"," + startDate + "," + endDate + ",\"" + status + "\",\"" +
                        failingStep + "\",\"" + reasonForFailure + "\",\"" + screenshotLocation + "\",\"" +
                        site + "\",\"" + browser + "\",\"" + additionaDataString + "\");");
                var command =
                    new MySqlCommand(
                        "Insert into testcasesresults (CaseID, StartDate, EndDate, Status, FailingStep, ReasonForFailure, ScreenShotLocation, Site, Browser, AdditionalData) values (\"" +
                        caseId + "\"," + startDate + "," + endDate + ",\"" + status + "\",\"" +
                        failingStep + "\",\"" + reasonForFailure + "\",\"" + screenshotLocation + "\",\"" +
                        site + "\",\"" + browser + "\",\"" + additionaDataString + "\");",
                        _connection);
                _connection.Open();
                command.ExecuteNonQuery();
            }
            catch (Exception)
            {
                _log.Error("Error when trying to insert test case execution results for test with id '" +
                           caseId +
                           "' in to table testcasesresults!");
                throw;
            }
            finally
            {
                _connection.Close();
            }
        }

        private string ReplaceDoubleQuotesWithSinge(string param)
        {
            return param != null ? param.Replace("\"", "'") : null;
        }

        private string RemovePrefixAndReplaceChar(string rawKnownIssues)
        {
            var result = "";
            rawKnownIssues = rawKnownIssues.Trim().ToLower();

            if (!rawKnownIssues.StartsWith("knownissues:")) return result;
            result = rawKnownIssues.Replace("knownissues:", "");
            if (!result.Contains("_")) return result;
            _log.Debug("Replacing '_' with '-' to match exactly the keys from jira.");
            result = result.Replace("_", "-");
            return result;
        }

        public void UpdateKnownIssuesForScenario(int scenarioId, string knownIssues)
        {

            knownIssues = RemovePrefixAndReplaceChar(knownIssues);

            try
            {
                _log.Debug("Query which will be execute is: \n UPDATE testcasearchive SET KnownIssues='" +
                           knownIssues + "' WHERE id='" + scenarioId + "';");
                var command =
                    new MySqlCommand(
                        "UPDATE testcasearchive SET KnownIssues='" + knownIssues + "' WHERE id='" +
                        scenarioId + "';",
                        _connection);
                _connection.Open();
                command.ExecuteNonQuery();
            }
            catch (Exception)
            {
                _log.Error("Error when trying to update known issues for scenario with id '" + scenarioId +
                           "' in to table testcasearchive!");
                throw;
            }
            finally
            {
                _connection.Close();
            }
        }

        private void UpdateCategoriesForScenario(int scenarioId, List<string> categories)
        {
            _log.Debug("Trying to update categories for scenario with db id '" + scenarioId + "'.");
            var allCategories = "";
            foreach (var category in categories)
            {
                if (category == categories.Last())
                {
                    allCategories = allCategories + category;
                    continue;
                }
                allCategories = allCategories + category +" , ";
            }
            try
            {
                _log.Debug("Query which will be execute is: \n UPDATE testcasearchive SET Categories='" +
                           allCategories + "' WHERE id='" + scenarioId + "';");
                var command =
                    new MySqlCommand(
                        "UPDATE testcasearchive SET Categories='" + allCategories + "' WHERE id='" +
                        scenarioId + "';",
                        _connection);
                _connection.Open();
                command.ExecuteNonQuery();
            }
            catch (Exception)
            {
                _log.Error("Error when trying to update categories for scenario with id '" + scenarioId +
                           "' in to table testcasearchive!");
                throw;
            }
            finally
            {
                _connection.Close();
            }
            _log.Debug("Update successful.");
        }

        public void Report()
        {
            if (_data == null)
            {
                _log.Error("Incorrect arguments constructor of the object received data object null!");
                throw new Exception("Provided for reporting data is null!");
            }
            var parsedData = new DataParser().Parse(_data);
            CreatTables();
            AddTestCaseToArchive(parsedData.ScenarioName);
            var dbCaseId = ReturnCaseIdFromArchive(parsedData.ScenarioName);

            var knownIssues = "";

            foreach (var tag in parsedData.ScenarioCategories.Where(tag => tag.ToLower().Contains("knownissues:")))
            {
                knownIssues = tag;
                _log.Info("Known issues tag found with value '" + knownIssues + "'.");
            }

            if (!String.IsNullOrEmpty(knownIssues))
            {
                UpdateKnownIssuesForScenario(dbCaseId, knownIssues);
            }
            UpdateCategoriesForScenario(dbCaseId, parsedData.ScenarioCategories);
            ReportExecutionResult(dbCaseId,parsedData.StartDate,parsedData.EndDate,parsedData.Status,parsedData.FailingStep,parsedData.FailureStackTrace,parsedData.ScreenShotLocation,parsedData.Site,parsedData.Browser, parsedData.AdditionalData);
        }
    }
}