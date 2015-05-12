using System.Collections.Generic;
using log4net;

namespace ExecutionResultsReporter
{
    public class DataParser
    {
        private readonly ILog _log = LogManager.GetLogger("DataParser");
        public TestCaseExecutionData Parse(IEnumerable<KeyValuePair<string, string>> data)
        {
            _log.Info("Parsing data from data object.");
            var parseData = new TestCaseExecutionData();
            foreach (var row in data)
            {
                switch (row.Key.ToLower())
                {
                    case "featurename":
                        _log.Debug("Matching key from dictionary to 'FeatureName' so value '" + row.Value + "' will be added to the result.");
                        parseData.FeatureName = row.Value;
                        break;
                    case "filestore":
                        _log.Debug("Matching key from dictionary to 'FileStore' so value '" + row.Value + "' will be added to the result.");
                        parseData.FileStore = row.Value;
                        break;
                    case "scenarioname":
                        _log.Debug("Matching key from dictionary to 'ScenarioName' so value '" + row.Value + "' will be added to the result.");
                        parseData.ScenarioName = row.Value;
                        break;
                    case "scenariocategories":
                        _log.Debug("Matching key from dictionary to 'ScenarioCategories' so value '" + row.Value + "' will be added list of categories in the result.");
                        parseData.ScenarioCategories.Add(row.Value);
                        break;
                    case "startdate":
                        _log.Debug("Matching key from dictionary to 'StartDate' so value '" + row.Value + "' will be added to the result.");
                        parseData.StartDate = row.Value;
                        break;
                    case "enddate":
                        _log.Debug("Matching key from dictionary to 'EndDate' so value '" + row.Value + "' will be added to the result.");
                        parseData.EndDate = row.Value;
                        break;
                    case "status":
                        _log.Debug("Matching key from dictionary to 'Status' so value '" + row.Value + "' will be added to the result.");
                        parseData.Status = row.Value;
                        break;
                    case "failingstep":
                        _log.Debug("Matching key from dictionary to 'FailingStep' so value '" + row.Value + "' will be added to the result.");
                        parseData.FailingStep = row.Value;
                        break;
                    case "failurestacktrace":
                        _log.Debug("Matching key from dictionary to 'FailureStackTrace' so value '" + row.Value + "' will be added to the result.");
                        parseData.FailureStackTrace = row.Value;
                        break;
                    case "screenshotlocation":
                        _log.Debug("Matching key from dictionary to 'ScreenShotLocation' so value '" + row.Value + "' will be added to the result.");
                        parseData.ScreenShotLocation = row.Value;
                        break;
                    case "browser":
                        _log.Debug("Matching key from dictionary to 'Browser' so value '" + row.Value + "' will be added to the result.");
                        parseData.Browser = row.Value;
                        break;
                    case "site":
                        _log.Debug("Matching key from dictionary to 'Site' so value '" + row.Value + "' will be added to the result.");
                        parseData.Site = row.Value;
                        break;
                    case "scenariostep":
                        _log.Debug("Matching key from dictionary to 'ScenarioSteps' so value '" + row.Value + "' will be added to the result.");
                        parseData.ScenarioSteps.Add(row.Value);
                        break;
                    case "configurations":
                        _log.Debug("Matching key from dictionary to 'Configurations' so value '" + row.Value + "' will be added to the result.");
                        parseData.Configurations = row.Value;
                        break;
                    case "knownissue":
                        _log.Debug("Matching key from dictionary to 'KnownIssue' so value '" + row.Value + "' will be added to the result.");
                        parseData.KnownIssues = row.Value;
                        break;
                    default:
                        _log.Debug("Provided key '"+row.Key+"' didn't match any pattern so it will be added to 'additional data' list property of the result");
                        parseData.AdditionalData.Add(row.Value);
                        break;
                }
            }
            _log.Info("Parsing complete.");
            return parseData;
        }
    }
}
