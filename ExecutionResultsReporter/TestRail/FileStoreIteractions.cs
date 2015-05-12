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
    class FileStoreIteractions
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
                File.WriteAllText(_path,info);
        }

        public TestPlan GetPlanFromFileStore()
        {
            return JsonConvert.DeserializeObject<TestPlan>(File.ReadAllText(_path));
        }

        public void DeleteFileStore()
        {
            if (File.Exists(_path))
            {
                File.Delete(_path);
            }
            else
            {
                Log.Warn("File with path '"+_path+"' didn't exist!");
            }
        }

    }
}
