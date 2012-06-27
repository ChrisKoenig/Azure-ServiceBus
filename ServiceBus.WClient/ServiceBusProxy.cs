using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBus.WClient
{
    class ServiceBusProxy
    {

        #region Constants and String Formatters

        // {0} = service namespace
        private const string UT_SERVICE_BUS_ROOT = "https://{0}.servicebus.windows.net";

        // {0} = service namespace
        // {1} = topic path
        private const string UT_SEND_MESSAGE_TO_TOPIC = "https://{0}.servicebus.windows.net/{1}/messages";

        // {0} = service namespace        
        private const string UT_GET_ACCESS_TOKEN = "https://{0}-sb.accesscontrol.windows.net/WRAPv0.9/";

        private const string FMT_ATOM_MESSAGE = "<entry xmlns='http://www.w3.org/2005/Atom'><content type='application/xml'>{0}</content></entry>";
        #endregion

        #region Properties

        public string AccountName { get; set; }
        public string DefaultIssuer { get; set; }
        public string DefaultKey { get; set; }
        #endregion

        /// <summary>
        /// Initializes a new instance of the ServiceBusProxy class.
        /// </summary>
        public ServiceBusProxy(string accountName, string defaultIssuer, string defaultKey)
        {
            AccountName = accountName;
            DefaultIssuer = defaultIssuer;
            DefaultKey = defaultKey;
        }


        public async void SendMessageToTopic(string topicName, string message)
        {

            // Headers:
            //  Authorization: Specifies a WRAPv0.9.7.2 token containing a SimpleWebToken acquired from ACS. Set to WRAP access_token="{swt}".
            //  Content-Type: Set to application/atom+xml;type=entry;charset=utf-8.

            var requestUriString = String.Format(UT_SEND_MESSAGE_TO_TOPIC, AccountName, topicName);
            var accessToken = await GetAccessToken();
            var authHeader = new AuthenticationHeaderValue("SimpleWebToken", accessToken);
            var contentTypeHeader = new MediaTypeHeaderValue("application/atom+xml;type=entry;charset=utf-8");
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = authHeader;

            var content = new StringContent(String.Format(FMT_ATOM_MESSAGE, message));
            content.Headers.ContentType = contentTypeHeader;

            var response = await client.PostAsync(requestUriString, content);

        }


        public async Task<string> GetAccessToken()
        {

            var requestStringUri = String.Format(UT_GET_ACCESS_TOKEN, AccountName);
            var contentTypeHeader = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            var client = new HttpClient();

            var wrap_name = DefaultIssuer;
            var wrap_password = DefaultKey;
            var wrap_scope = String.Format(UT_SERVICE_BUS_ROOT, AccountName);
            var wrap_scope_enc = WebUtility.UrlEncode(wrap_scope);
            var content = new StringContent(String.Format("{0}&{1}&{2}", wrap_name, wrap_password, wrap_scope_enc));
            content.Headers.ContentType = contentTypeHeader;
            var response = await client.PostAsync(requestStringUri, content);

            return response.ToString();
        }

    }
}
