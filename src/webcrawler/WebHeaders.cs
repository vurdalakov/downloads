namespace Vurdalakov
{
    using System;
    using System.Collections.Generic;

    public class WebHeaders
    {
        public Int64 ContentLength { get; private set; }
        public String ContentType { get; private set; }
        public DateTime LastModified { get; private set; }

        public WebHeaders(Dictionary<String, String> httpHeaders)
        {
            ContentLength = -1;
            ContentType = "";
            LastModified = new DateTime(1900, 1, 1, 0, 0, 0);

            foreach (var header in httpHeaders)
            {
                switch (header.Key.ToLower())
                {
                    case "content-length":
                        ContentLength = Convert.ToInt64(header.Value);
                        break;
                    case "content-type":
                        ContentType = header.Value;
                        break;
                    case "last-modified":
                        LastModified = DateTime.Parse(header.Value).ToUniversalTime();
                        break;
                }
            }
        }
    }
}
