namespace Vurdalakov.Ngonb
{
    using System;

    class Program
    {
        static void Main(string[] args)
        {
            //var directoryName = Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\personal\gazeta\sovsib");
            var directoryName = @"D:\gazeta\sovsib";

            var webCrawler = new WebCrawler(directoryName);


            const String baseUrl = "http://elib.ngonb.ru";

            var years = webCrawler.ExtractAll("http://elib.ngonb.ru/jspui/handle/NGONB/32", @"<option value=""NGONB/(\d+)"">\d\d\d\d</option>");

            foreach (var year in years)
            {
                var url = String.Format("http://elib.ngonb.ru/jspui/handle/NGONB/{0}/browse?type=dateissued&submit_browse=Issue+Date", year);

                while (true)
                {
                    var issues = webCrawler.ExtractAll(url, @"<a href=""/jspui/handle/NGONB/(\d+)"">(.*?)</a></td>");

                    foreach (var issue in issues)
                    {
                        var issueUrl = "http://elib.ngonb.ru/jspui/handle/NGONB/" + issue;
                        var pdfUrl = webCrawler.Extract(issueUrl, @"""(/jspui/bitstream/NGONB/" + issue + @"(?:.+?).pdf)""");
                        if (!String.IsNullOrEmpty(pdfUrl))
                        {
                            pdfUrl = baseUrl + pdfUrl;

                            var date = webCrawler.Extract(issueUrl, @">(\d\d\d\d-\d\d-\d\d)<");
                            var directory = date.Substring(0, 4);
                            var number = webCrawler.Extract(issueUrl, @">(\d+).pdf<");

                            var fileName = String.Format(@"{0}\{1}_{2}.pdf", directory, date.Replace('-', '_'), number);

                            Console.WriteLine("{0} => {1}", pdfUrl, fileName);

                            webCrawler.AddFile(pdfUrl, fileName);
                        }
                    }

                    // next page
                    url = webCrawler.Extract(url, @"href=""(.+?)"">next");
                    if (String.IsNullOrEmpty(url))
                    {
                        break;
                    }
                    url = baseUrl + url.XmlUnescape();
                }
            }
        }
    }
}
