namespace Vurdalakov
{
    using System;
    using System.Collections.Generic;
    using System.Net;

    public class WebClientEx : WebClient
    {
        public String Method { get; set; }

        public HttpWebRequest Request { get; private set; }

        public HttpWebResponse Response { get; private set; }

        public Dictionary<String, String> DownloadHeaders(String url)
        {
            var headers = new Dictionary<String, String>();

            var method = Method;
            Method = "HEAD";

            DownloadData(url);

            foreach (String header in Response.Headers)
            {
                headers.Add(header, Response.Headers[header]);
            }

            Method = method;

            return headers;
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            var webRequest = base.GetWebRequest(address);

            Request = webRequest as HttpWebRequest;

            if (!String.IsNullOrEmpty(Method))
            {
                webRequest.Method = Method;
            }

            return webRequest;
        }

        protected override WebResponse GetWebResponse(WebRequest request)
        {
            var webResponse = base.GetWebResponse(request);

            Response = webResponse as HttpWebResponse;

            return webResponse;
        }
    }
}
