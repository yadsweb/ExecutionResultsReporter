using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExecutionResultsReporter.TestRail.TestRailObj;
using log4net;
using Newtonsoft.Json;

namespace ExecutionResultsReporter.TestRail
{
    public class FileStoreIteractions
    {
        private readonly string _path;
        private static readonly ILog Log = LogManager.GetLogger("TestPlanCreator");

        public FileStoreIteractions(string path)
        {
            _path = path;
        }

        public void WriteInfoToSotre(String info)
        {
            if (File.Exists(_path))
            {
                Log.Warn("File with sore already exist its content will be overwritten.");
            }
            File.WriteAllText(_path, info);
        }

        public TestPlan GetPlanFromFileStore()
        {
            var testPlan = new StringBuilder();
            Log.Debug("Current store.txt path: " + _path);
            using (var propertyFile = new FileStream(_path, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var reader = new StreamReader(propertyFile, Encoding.Unicode))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    testPlan.Append(line);
                }
            }
            return JsonConvert.DeserializeObject<TestPlan>(testPlan.ToString());
        }

        public void DeleteFileStore()
        {
            if (File.Exists(_path))
            {
                File.Delete(_path);
            }
            else
            {
                Log.Warn("File with path '" + _path + "' didn't exist!");
            }
        }

    }
}
