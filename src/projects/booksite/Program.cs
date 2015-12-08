namespace Vurdalakov.Ngonb
{
    using System;

    class Program
    {
        static void Main(string[] args)
        {
            var directoryName = Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\personal\gazeta\krasev");
            //var directoryName = @"D:\gazeta\krasev";

            var webCrawler = new WebCrawler(directoryName);


            const String baseUrl = "http://www.booksite.ru/krassever/";

            var years = webCrawler.ExtractAll(baseUrl + "index.htm", @" href=""(\d\d\d\d\..+?)""");

            foreach (var year in years)
            {
                var yearUrl = baseUrl + year;

                var issues = webCrawler.ExtractAll(yearUrl, @"<a href=""(\d\d\d\d/(?:\w+/)?\d\d\d\d_\d+\.pdf)");

                foreach (var issue in issues)
                {
                    var pdfUrl = baseUrl + issue;

                    var directory = webCrawler.Parse(pdfUrl, @"/(\d\d\d\d)/");
                    var number = webCrawler.Parse(pdfUrl, @"_(\d+)\.").PadLeft(3, '0');

                    var name = webCrawler.Parse(pdfUrl, @"/\d\d\d\d/(?:(\w+)/)\d\d\d\d_");
                    if (!String.IsNullOrEmpty(name))
                    {
                        if (name.Equals("izvestya"))
                        {
                            name = "A_";
                        }
                        else if (name.Equals("krassever"))
                        {
                            name = "B_";
                        }
                        else
                        {
                            throw new Exception();
                        }
                    }

                    var fileName = String.Format(@"{0}\{0}_{2}{1}.pdf", directory, number, name);

                    Console.WriteLine("{0} => {1}", pdfUrl, fileName);

                    webCrawler.AddFile(pdfUrl, fileName);
                }
            }
        }
    }
}
