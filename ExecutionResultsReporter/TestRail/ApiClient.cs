/**
 * TestRail API binding for .NET (API v2, available since TestRail 3.0)
 *
 * Learn more:
 *
 * http://docs.gurock.com/testrail-api2/start
 * http://docs.gurock.com/testrail-api2/accessing
 *
 * Copyright Gurock Software GmbH. See license.md for details.
 */

using System;
using System.Net;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ExecutionResultsReporter.TestRail
{
	public class ApiClient
	{
		private string _mUser;
		private string _mPassword;
		private string _mUrl;

		public ApiClient(string baseUrl)
		{
			if (!baseUrl.EndsWith("/"))
			{
				baseUrl += "/";
			}
			_mUrl = baseUrl + "index.php?/api/v2/";
		}
	    
        /**
		 * Get/Set User
		 *
		 * Returns/sets the user used for authenticating the API requests.
		 */
		public string User
		{
			get { return _mUser; }
			set { _mUser = value; }
		}

		/**
		 * Get/Set Password
		 *
		 * Returns/sets the password used for authenticating the API requests.
		 */
		public string Password
		{
			get { return _mPassword; }
			set { _mPassword = value; }
		}

		/**
		 * Send Get
		 *
		 * Issues a GET request (read) against the API and returns the result
		 * (as JSON object, i.e. JObject or JArray instance).
		 *
		 * Arguments:
		 *
		 * uri                  The API method to call including parameters
		 *                      (e.g. get_case/1)
		 */
		public object SendGet(string uri)
		{
			return SendRequest("GET", uri, null);
		}

		/**
		 * Send POST
		 *
		 * Issues a POST request (write) against the API and returns the result
		 * (as JSON object, i.e. JObject or JArray instance).
		 *
		 * Arguments:
		 *
		 * uri                  The API method to call including parameters
		 *                      (e.g. add_case/1)
		 * data                 The data to submit as part of the request (as
		 *                      serializable object, e.g. a dictionary)
		 */
		public object SendPost(string uri, object data)
		{
			return SendRequest("POST", uri, data);
		}

		private object SendRequest(string method, string uri, object data)
		{
			var url = _mUrl + uri;

			// Create the request object and set the required HTTP method
			// (GET/POST) and headers (content type and basic auth).
			var request = (HttpWebRequest)WebRequest.Create(url);
			request.ContentType = "application/json";
			request.Method = method;

			string auth = Convert.ToBase64String(
				Encoding.ASCII.GetBytes(
					String.Format(
						"{0}:{1}",
						_mUser,
						_mPassword
					)
				)
			);

			request.Headers.Add("Authorization", "Basic " + auth);

			if (method == "POST")
			{
				// Add the POST arguments, if any. We just serialize the passed
				// data object (i.e. a dictionary) and then add it to the request
				// body.
				if (data != null)
				{
					byte[] block = Encoding.UTF8.GetBytes( 
						JsonConvert.SerializeObject(data)
					);
					request.GetRequestStream().Write(block, 0, block.Length);
                    //Console.WriteLine(JsonConvert.SerializeObject(data));
				}
			}

			// Execute the actual web request (GET or POST) and record any
			// occurred errors.
			Exception ex = null;
			HttpWebResponse response;
			try
			{
				response = (HttpWebResponse)request.GetResponse();
			}
			catch (WebException e)
			{
				if (e.Response == null)
				{
					throw;
				}

				response = (HttpWebResponse)e.Response;
				ex = e;
			}

			// Read the response body, if any, and deserialize it from JSON.
			var text = "";
			if (response != null)
			{
				var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);

				using (reader)
				{
					text = reader.ReadToEnd();
				}
			}

			JContainer result;
			if (text != "")
			{
				if (text.StartsWith("["))
				{
					result = JArray.Parse(text);
				}
				else 
				{
					result = JObject.Parse(text);
				}
			}
			else 
			{
				result = new JObject();
			}

			// Check for any occurred errors and add additional details to
			// the exception message, if any (e.g. the error message returned
			// by TestRail).
			if (ex != null)
			{
				string error = (string) result["error"];
				if (error != null)
				{
					error = '"' + error + '"';
				}
				else
				{
					error = "No additional error message received";
				}

				throw new APIException(
					String.Format(
						"TestRail API returned HTTP {0} ({1})",
						(int)response.StatusCode,
						error
					)
				);
			}

			return result;
		}
	}
}

