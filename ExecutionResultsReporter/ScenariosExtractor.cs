using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ExecutionResultsReporter.TestRail;
using log4net;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace ExecutionResultsReporter
{
    public class ScenariosExtractor
    {
        private readonly ILog _log = LogManager.GetLogger("ScenariosExtractor");

        public IEnumerable<ScenarioObj> RetriveScenarioInformation(String dllPath)
        {
            var result = new List<ScenarioObj>();
            _log.Info("Loading assembly with path: " + dllPath);
            var assemblyContainingTests = Assembly.LoadFrom(dllPath);
            _log.Info("Assembly loaded successful.");
            foreach (var type in assemblyContainingTests.GetTypes())
            {
                var classAttributes = type.GetCustomAttributes() as Attribute[] ?? type.GetCustomAttributes().ToArray();
                var firstOrDefault = (DescriptionAttribute)classAttributes.FirstOrDefault(attribute => attribute.GetType() == typeof(DescriptionAttribute));
                var featureName = "Default test suite for test case with no feature";
                if (firstOrDefault != null)
                {
                    featureName = firstOrDefault.Description;
                }
                MethodInfo[] methods = type.GetMethods();
                foreach (var methodInfo in methods)
                {
                    var tempScenario = new ScenarioObj();
                    var attributes = methodInfo.GetCustomAttributes() as Attribute[] ?? methodInfo.GetCustomAttributes().ToArray();
                    var isValidTest = attributes.Any(attribute => attribute.GetType() == typeof(TestAttribute));
                    if (isValidTest)
                    {
                        foreach (var attribute in attributes.Where(attribute => attribute.GetType() == typeof(DescriptionAttribute)))
                        {
                            var scenarioName = ((DescriptionAttribute)attribute).Description;
                            _log.Info("Found scenario with name: " + scenarioName);
                            tempScenario.Name = scenarioName;
                            break;
                        }
                        foreach (var attribute in attributes.Where(attribute => attribute.GetType() == typeof(CategoryAttribute)))
                        {
                            var category = ((CategoryAttribute)attribute);
                            _log.Info("Found category with name: " + category.Name);
                            tempScenario.CategoryAttribute.Add(category.Name);
                        }
                        tempScenario.FeatureName = featureName;
                        if (attributes.Any(attribute => attribute.GetType() == typeof(TestCaseAttribute)))
                        {
                            
                            foreach (var attribute in attributes.Where(attribute => attribute.GetType() == typeof(TestCaseAttribute)))
                            {
                                var newName = tempScenario.Name;
                                var testCaseAttribute = ((TestCaseAttribute)attribute);
                                _log.Info("Found test case attribute with '" + testCaseAttribute.Arguments.Count() + "' argument's.");
                                _log.Info("Adding the attributes arguments to the scenario name in format (\"argument1\",\"argument2\".....).");
                                var tmpString = "";
                                foreach (var argument in testCaseAttribute.Arguments)
                                {
                                    if (argument == null)
                                    {
                                        continue;
                                    }
                                    _log.Info("\t " + argument);
                                    tmpString = tmpString + "\""+ argument + "\",";
                                    tempScenario.TestCaseAttributes.Add(argument.ToString());
                                }
                                newName = newName + "(" + tmpString.Substring(0, (tmpString.Length - 1)) + ")";
                                if (newName.Length > 250)
                                {
                                    var caseIdString = "";
                                    if (tempScenario.CategoryAttribute.Any())
                                    {
                                        caseIdString = (from caseId in tempScenario.CategoryAttribute where caseId.ToLower().StartsWith("caseid:") select caseId.ToLower().Replace("caseid:", "").Trim()).FirstOrDefault() ?? "";
                                    }
                                    if (!string.IsNullOrEmpty(caseIdString))
                                    {
                                        newName = tempScenario.Name.Substring(0, tempScenario.Name.Length - (newName.Length - 246 + caseIdString.Length)) + "..." + caseIdString + "(" + tmpString.Substring(0, (tmpString.Length - 1)) + ")";
                                    }
                                    else
                                    {
                                        newName = tempScenario.Name.Substring(0, newName.Length - (newName.Length - 246)) +"..."+ "(" + tmpString.Substring(0, (tmpString.Length - 1)) + ")";
                                    }
                                }
                                if (result.Any(scenario => scenario.Name == newName)) continue;
                                _log.Info("Adding scenario with name : " + newName);
                                result.Add(new ScenarioObj
                                {
                                    Name = newName,
                                    FeatureName = tempScenario.FeatureName,
                                    CategoryAttribute = tempScenario.CategoryAttribute,
                                    TestCaseAttributes = tempScenario.TestCaseAttributes
                                });
                            }
                        }
                        else
                        {
                            result.Add(tempScenario);
                        }
                    }
                }
            }
            return result;
        }
    }
}
