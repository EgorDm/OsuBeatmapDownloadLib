using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTestExperimental
    {
        public const int TEST_SONG_ID = 138554;
        public const string OSU_MAIN_URL = "https://osu.ppy.sh/";
        public const string OSU_DOWNLOAD_URL = "https://osu.ppy.sh/d/{0}n";
        public const string OSU_LOGIN_URL = "https://osu.ppy.sh/forum/ucp.php?mode=login";

        private const string USERNAME = "test";
        private const string PASSWORD = "test!!";

        private const string NOT_LOGGED_IN_CHECK = "action=\"https://osu.ppy.sh/forum/ucp.php?mode=login\"";
        private const string LOGIN_FAILED_CHECK = "class=\"error\"";
        private static readonly CookieContainer COOKIE_CONTAINER = new CookieContainer();


        [TestMethod]
        public void TestDownloadFromOsu()
        {
           Debug.WriteLine("Checking login");
            if (CheckLoggedIn(true))
            {
                Debug.WriteLine("We are logged in!");
            }
            else
            {
                Debug.WriteLine("Not logged in");
            }
        }

        private static bool CheckLoggedIn(bool login = false)
        {
            var webRequest = (HttpWebRequest)WebRequest.Create(OSU_MAIN_URL);
            webRequest.CookieContainer = COOKIE_CONTAINER;

            var webResponse = (HttpWebResponse)webRequest.GetResponse();
            if (ResponseContains(webResponse, NOT_LOGGED_IN_CHECK))
            {
                return (login) && Login();
            }
            return true;
        }

        private static bool Login()
        {
            string poststring = $"login=login&password={PASSWORD}&redirect=%2F&sid=&username={USERNAME}";
            var postdata = Encoding.UTF8.GetBytes(poststring);
            var webRequest = (HttpWebRequest)WebRequest.Create(OSU_LOGIN_URL);
            webRequest.Method = "POST";
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.ContentLength = postdata.Length;
            webRequest.CookieContainer = COOKIE_CONTAINER;
            using (var writer = webRequest.GetRequestStream())
            {
                writer.Write(postdata, 0, postdata.Length);
            }
            var webResponse = (HttpWebResponse)webRequest.GetResponse();
            return !ResponseContains(webResponse, LOGIN_FAILED_CHECK);
        }

        private static bool ResponseContains(WebResponse haystack, string needle)
        {
            using (var responseStream = haystack.GetResponseStream())
            {
                if (responseStream == null) return false;
                using (var reader = new StreamReader(responseStream))
                {
                    var responseText = reader.ReadToEnd();
                    return responseText.Contains(needle);
                }
            }
        }
        
    }
}