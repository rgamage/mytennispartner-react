using Microsoft.Extensions.Logging;
using RestSharp;
using System;
using System.Net;
using Newtonsoft.Json;

namespace MyTennisPartner.Web.Services
{
    public class RestClientService
    {
        private readonly ILogger logger;
        private readonly RestClient client;
        private string host;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="restClient"></param>
        /// <param name="logger"></param>
        public RestClientService(RestClient restClient, ILogger logger)
        {
            this.logger = logger;
            client = restClient;
        }

        /// <summary>
        /// setter/getter for base url
        /// </summary>
        public Uri BaseUrl {
            get {
                return client.BaseUrl;
            }
            set {
                client.BaseUrl = value;
            }
        }

        /// <summary>
        /// set host of target provider
        /// </summary>
        /// <param name="hostname"></param>
        public void SetHost(string hostname)
        {
            client.BaseUrl = new Uri($"https://{hostname}/");
            client.AddDefaultHeader("Host", hostname);
            host = hostname;
        }

        /// <summary>
        /// add default header for requests
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void AddHeader(string name, string value)
        {
            client.AddDefaultHeader(name, value);
        }

        /// <summary>
        /// clear all cookies
        /// </summary>
        public void ClearCookies()
        {
            client.CookieContainer = new CookieContainer();
        }

        /// <summary>
        /// get from a url
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public IRestResponse Get(string url)
        {
            logger.LogInformation($"RestClientService: getting url {url}");
            var request = new RestRequest(url, Method.GET);
            //request.AddHeader("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3");
            request.AddHeader("Host", host);
            var response = client.Execute(request);
            return response;
        }

        /// <summary>
        /// post data to a url
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public IRestResponse Post(string url, object data)
        {
            logger.LogInformation($"RestClientService: posting to url {url}, data = {JsonConvert.SerializeObject(data)}");
            var request = new RestRequest(url, Method.POST);           
            request.AddJsonBody(data);
            request.AddHeader("Host", host);
            var response = client.Execute(request);
            return response;
        }
    }
}
