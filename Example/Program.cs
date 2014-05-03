using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace Example
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Check source code for examples. Press any key to exit.");
            Console.ReadKey();
        }

        // Simple page scraping
        public static void Example1()
        {
            Muka.HttpRequest request;

            request = new Muka.HttpRequest("http://example.com");
            // or using Uri object
            Uri uri = new Uri("http://example.com");
            request = new Muka.HttpRequest(uri);

            string source = request.DownloadString();
            Console.WriteLine(source);
        }

        // Download files from web (e.g, images, documents)
        public static void Example2()
        {
            var request = new Muka.HttpRequest("http://upload.wikimedia.org/wikipedia/commons/7/70/Example.png");
            int fileSize = request.DownloadFile(@"C:\Example.png");
            Console.WriteLine("Download complete. Downloaded {0} bytes.", fileSize);
        }

        // Page download with WebException suppressed 
        // If WebException is thrown DownloadString() returns empty string
        public static void Example3()
        {
            var request = new Muka.HttpRequest("http://this.page.doesnt.exists");
            request.SupressWebExceptions = true;
            string source = request.DownloadString();
            Console.WriteLine("Source length: {0} bytes.", source.Length);
        }

        // Send post data and use cookies
        public static void Example4()
        {
            var request = new Muka.HttpRequest("http://example.com/login");
            // you can use dictionary
            request.PostData.Add("login", "darthvader");
            request.PostData.Add("password", "deathstar");
            // or raw string (it will be encoded internally) - you can use both methods simultaneously if you want to
            request.RawPostData = "var1=smth1&var2=smth2";

            string source = request.DownloadString();
            // you can check source to deteremine if you're logged in or not
            if (source.Contains("logout"))
            {
                var request2 = new Muka.HttpRequest("http://example.com/messages");
                // set cookies
                request2.Cookies = request.Cookies;
                string source2 = request.DownloadString();
                // process user's messages
                // [...]
            }
        }

        // Set headers and customize HttpWebRequest
        public static void Example5()
        {
            var request = new Muka.HttpRequest("http://example.com/");
            // you can work with underlying HttpWebRequest if you need more customization
            // it's read-only so you can't set your own HttpWebRequest
            request.Request.Headers.Add(HttpRequestHeader.ContentType, "application/pdf");
            request.Request.UserAgent = "customUA";

            string source = request.DownloadString();
        }
    }
}
