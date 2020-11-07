using System;
using System.Linq;
using System.Net;

namespace AstroModLoader
{
    public static class GitHubAPI
    {
        public static string CombineURI(params string[] uris)
        {
            string output = "";
            foreach (string uriBit in uris)
            {
                output += uriBit.Trim('/') + "/";
            }
            return output.TrimEnd('/');
        }

        public static string GetLatestVersionURL(string repo)
        {
            return CombineURI("https://github.com", repo, "releases", "latest");
        }

        public static Version GetLatestVersionFromGitHub(string repo)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(GetLatestVersionURL(repo));
                request.Method = "GET";
                request.AllowAutoRedirect = false;
                request.ContentType = "application/json; charset=utf-8";
                request.UserAgent = AMLUtils.UserAgent;

                string newURL = null;
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    newURL = response.Headers["location"];
                }

                if (string.IsNullOrEmpty(newURL)) return null;
                string[] splitURL = newURL.Split('/');
                string kosherVersion = splitURL[splitURL.Length - 1];
                if (kosherVersion[0] == 'v') kosherVersion = kosherVersion.Substring(1);

                Version.TryParse(kosherVersion, out Version foundVersion);
                return foundVersion;
            }
            catch (Exception ex)
            {
                if (ex is WebException || ex is FormatException) return null;
                throw;
            }
        }
    }
}
