using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OsuMapDownload.Exceptions;
using OsuMapDownload.Models;

namespace OsuMapDownload.Providers
{
    public class OsuDownloadProvider : BeatmapDownloadProvider
    {
        public const string BASE_URL = "https://osu.ppy.sh";
        public const string DOWNLOAD_URL = "/d/{0}n";
        public const string LOGIN_URL = "/forum/ucp.php?mode=login";

        private const string NOT_LOGGED_IN_CHECK = "action=\"https://osu.ppy.sh/forum/ucp.php?mode=login\"";
        private const string LOGIN_FAILED_CHECK = "class=\"error\"";

        private string _username;
        private string _password;

        public string CookieFilePath { get; set; }
        public CookieContainer Cookies { get; private set; } = new CookieContainer();
        public List<Action<bool>> LoginCallback = new List<Action<bool>>();
        public Task LoginTask;
        private bool _loggedIn;

        public bool LoggedIn {
            get { return _loggedIn; }
            private set {
                _loggedIn = value;
                foreach (var action in LoginCallback) {
                    action.Invoke(_loggedIn);
                }
                LoginCallback.Clear();
            }
        }

        public OsuDownloadProvider(string username, string password, CookieContainer cookies, string cookieFilePath = null) {
            _username = username;
            _password = password;
            CookieFilePath = cookieFilePath;
            if (cookies != null) {
                Cookies = cookies;
                return;
            }
            if (cookieFilePath != null) {
                LoadCookies();
            }
        }

        public OsuDownloadProvider(string username, string password, string cookiePath = null)
            : this(username, password, null, cookiePath) {}

        public override string GetUrl(MapsetDownload download) {
            return string.Format(BASE_URL + DOWNLOAD_URL, download.ID);
        }

        public override WebRequest PrepareRequest(MapsetDownload download) {
            var webRequest = (HttpWebRequest) WebRequest.Create(GetUrl(download));
            webRequest.CookieContainer = Cookies;
            return webRequest;
        }

        public override string GetFileName(WebResponse response) {
            return new ContentDisposition(response.Headers["Content-Disposition"]).FileName;
        }

        public override void StartDownloadTask(Task downloadTask, MapsetDownload download) {
            if (LoggedIn) {
                downloadTask.Start();
                return;
            }
            Debug.WriteLine("StartingDL");
            LoginCallback.Add(loggedIn => {
                Debug.WriteLine("LoginCB" + loggedIn);
                if (loggedIn) {
                    downloadTask.Start();
                } else {
                    download.Error = new DownloadProviderNotReadyException("Could not login into osu.");
                }
            });
            if (LoginTask == null) {
                CheckOrLogin();
            }
        }

        public void CheckOrLogin() {
            LoginTask = new Task(() => {
                var loggedIn = false;
                try {
                    loggedIn = CheckLoggedIn();
                    if (!loggedIn) {
                        loggedIn = Login(_username, _password);
                    }
                } catch (Exception) {
                    // throw;
                }
                LoggedIn = loggedIn;
                LoginTask = null;
            });
            LoginTask.Start();
        }

        protected bool CheckLoggedIn() {
            var webRequest = (HttpWebRequest) WebRequest.Create(BASE_URL);
            webRequest.CookieContainer = Cookies;

            var webResponse = (HttpWebResponse) webRequest.GetResponse();
            return !DownloadUtils.ResponseContains(webResponse, NOT_LOGGED_IN_CHECK);
        }

        protected bool Login(string username, string password) {
            if (username == null || password == null) return false;
            string poststring = $"login=login&password={password}&redirect=%2F&sid=&username={username}";
            var postdata = Encoding.UTF8.GetBytes(poststring);
            Cookies = new CookieContainer();

            var webRequest = (HttpWebRequest) WebRequest.Create(BASE_URL + LOGIN_URL);
            webRequest.Method = "POST";
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.ContentLength = postdata.Length;
            webRequest.CookieContainer = Cookies;
            using (var writer = webRequest.GetRequestStream()) {
                writer.Write(postdata, 0, postdata.Length);
            }
            var webResponse = (HttpWebResponse) webRequest.GetResponse();
            var ret = !DownloadUtils.ResponseContains(webResponse, LOGIN_FAILED_CHECK);
            if(ret && CookieFilePath != null) SaveCookies();
            return ret;
        }

        public void LoadCookies() {
            if(!File.Exists(CookieFilePath)) return;
            var uri = new Uri(BASE_URL);
            CookieContainer container;
            using (var fs = new FileStream(CookieFilePath, FileMode.Open, FileAccess.Read)) {
                container = DownloadUtils.DeserializeCookies(fs, uri);
            }
            if (container != null) Cookies = container;
        }

        public void SaveCookies() {
            var uri = new Uri(BASE_URL);
            using (var fs = new FileStream(CookieFilePath, FileMode.Create)) {
                DownloadUtils.SerializeCookies(Cookies.GetCookies(uri), uri, fs);
            }
        }
    }
}