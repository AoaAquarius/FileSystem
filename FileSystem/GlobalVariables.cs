using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace FileSystemClient
{
    public class GlobalVariables
    {
        public string RequestForSplitURL { get; private set; }
        public string ServiceMappingURL { get; private set; }
        public string SaveFileAPIRoute { get; private set; }
        public string GetFileMatedataURL { get; private set; }
        public string RequestPartialFileRoute { get; private set; }
        public string[] SlaveNodeEndpoints { get; set; }
        public GlobalVariables(IConfiguration configuration)
        {
            RequestForSplitURL = configuration["RequestForSplitURL"];
            ServiceMappingURL = configuration["ServiceMappingURL"];
            SaveFileAPIRoute = configuration["SaveFileAPIRoute"];
            GetFileMatedataURL = configuration["GetFileMatedataURL"];
            RequestPartialFileRoute = configuration["RequestPartialFileRoute"];
            //Get ServiceMapping
            var client = new RestClient(ServiceMappingURL);
            var request = new RestRequest(Method.GET);
            request.AddHeader("cache-control", "no-cache");
            IRestResponse response = client.Execute(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                JArray jArray = JArray.Parse(response.Content);
                SlaveNodeEndpoints = new string[jArray.Count];
                for (int i = 0; i < jArray.Count; i++)
                {
                    SlaveNodeEndpoints[i] = jArray[i].ToString();
                }
            }
            else
            {
                throw new Exception("Cannot load slave node URLs");
            }
        }
    }
}
