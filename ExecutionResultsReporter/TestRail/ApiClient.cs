using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using log4net;
using Newtonsoft.Json;
using RestSharp;

namespace ExecutionResultsReporter.TestRail
{
    public class ApiClient
    {
        private readonly string _mUrl;
        private readonly ILog _log = LogManager.GetLogger("ApiClient");
        public ApiClient(string baseUrl)
        {
            if (!baseUrl.EndsWith("/"))
            {
                baseUrl += "/";
            }
            _mUrl = baseUrl + "index.php?/api/v2/";
        }

        public string User { get; set; }

        public string Password { get; set; }

        public string SendGet(string uri)
        {
            _log.Debug("Sending get request to url: " + _mUrl + uri);
            return SendRequest("GET", uri, null);
        }

        public string SendPost(string uri, object data)
        {
            _log.Debug("Sending post request to url: " + _mUrl + uri);
            return SendRequest("POST", uri, data);
        }

        private string SendRequest(string method, string uri, object data)
        {
            var auth = Convert.ToBase64String(
                Encoding.ASCII.GetBytes(
                    String.Format(
                        "{0}:{1}",
                        User,
                        Password
                    )
                )
            );
            var client = new RestClient(_mUrl);
            RestRequest request;
            switch (method)
            {
                case "GET":
                    request = new RestRequest(uri, Method.GET);
                    break;
                case "POST":
                    request = new RestRequest(uri, Method.POST);
                    break;
                default:
                    throw new Exception("Unsupported method '" + method + "'!");
            }
            request.AddHeader("Authorization", "Basic " + auth);
            request.AddHeader("Content-Type", "application/json");
            if (data != null)
            {
                var serializedData = JsonConvert.SerializeObject(data, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                _log.Debug("Request body is: " + serializedData);
                request.AddParameter("application/json", serializedData, ParameterType.RequestBody);
            }
            var response = client.Execute(request);
            if ((int)response.StatusCode == 429)
            {
                var timeToSleep = 5000;
                if (response.Headers.ToList().Any(element => element.Name == "Retry-After"))
                {
                    timeToSleep = Convert.ToInt32(response.Headers.ToList().Single(element => element.Name == "Retry-After").Value) * 1000;
                    _log.Debug("Response headers contains 'Retry-After' so the time to sleep before retrying will be set to '" + timeToSleep + "'");
                }
                else
                {
                    _log.Debug("Response headers: ");
                    foreach (var header in response.Headers)
                    {
                        _log.Debug("\t\t" + header);
                    }
                    _log.Debug("Didn't contains 'Retry-After' we will sleep the default time interval of 5 seconds.");
                    Thread.Sleep(timeToSleep);
                }
                response = client.Execute(request);
            }
            _log.Debug("Response body is: " + response.Content);
            return response.Content;
        }
    }
}
