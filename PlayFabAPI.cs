using AstroModIntegrator;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace AstroModLoader
{
    public class PlayFabException : WebException
    {
        public PlayFabException() : base()
        {

        }

        public PlayFabException(string message) : base("Invalid response from PlayFab: " + message)
        {

        }
    }

    public static class PlayFabAPI
    {
        public static string CustomID = "";
        public static string Token = "";
        public static bool Dirty = false;

        public static void Auth()
        {
            if (string.IsNullOrEmpty(CustomID))
            {
                Random random = new Random();
                byte[] randBuf = new byte[16]; random.NextBytes(randBuf);
                CustomID = string.Concat(randBuf.Select(x => x.ToString("X2")).ToArray());
                Dirty = true;
            }

            using (var wb = new WebClient())
            {
                wb.Encoding = Encoding.UTF8;
                wb.Headers[HttpRequestHeader.ContentType] = "application/json; charset=utf-8";
                wb.Headers[HttpRequestHeader.UserAgent] = "game=Astro, engine=UE4, version=4.18.2-0+++UE4+Release-4.18, platform=Windows, osver=6.2.9200.1.256.64bit";
                wb.Headers.Add("X-PlayFabSDK", "UE4MKPL-1.25.190916");

                string response = wb.UploadString("https://5EA1.playfabapi.com/Client/LoginWithCustomID?sdk=UE4MKPL-1.25.190916", "POST", "{\"CreateAccount\":true, \"CustomId\":\"" + CustomID + "\", \"TitleId\":\"5EA1\"}");
                JObject jObj = JObject.Parse(response);

                int decidedCode = 0;
                try
                {
                    decidedCode = jObj.GetValue("code").ToObject<int>();
                }
                catch
                {
                    throw new PlayFabException(response);
                }
                if (decidedCode != 200) throw new WebException(jObj.GetValue("status").ToObject<string>());

                try
                {
                    Token = ((JObject)jObj.GetValue("data")).GetValue("SessionTicket").ToObject<string>();
                    Dirty = true;
                }
                catch
                {
                    throw new PlayFabException(response);
                }
            }
        }

        public static JObject SimpleReq(string operation, string data, bool allowReauth = true)
        {
            using (var wb = new WebClient())
            {
                wb.Headers[HttpRequestHeader.ContentType] = "application/json; charset=utf-8";
                wb.Headers[HttpRequestHeader.UserAgent] = "game=Astro, engine=UE4, version=4.18.2-0+++UE4+Release-4.18, platform=Windows, osver=6.2.9200.1.256.64bit";
                wb.Headers.Add("X-PlayFabSDK", "UE4MKPL-1.25.190916");
                wb.Headers.Add("X-Authorization", Token);

                try
                {
                    string response = wb.UploadString("https://5EA1.playfabapi.com/Client/" + operation + "?sdk=UE4MKPL-1.25.190916", "POST", data);
                    return JObject.Parse(response);
                }
                catch (WebException ex)
                {
                    string responseContent = "";
                    HttpStatusCode code = HttpStatusCode.Unused;
                    try
                    {
                        code = ((HttpWebResponse)ex.Response).StatusCode;
                        using (StreamReader r = new StreamReader(ex.Response.GetResponseStream()))
                        {
                            responseContent = r.ReadToEnd();
                            JObject erroredJObj = JObject.Parse(responseContent);
                            if (allowReauth && erroredJObj.GetValue("status").ToObject<string>() == "Unauthorized")
                            {
                                Auth();
                                return SimpleReq(operation, data, false);
                            }
                        }
                    }
                    catch { }

                    throw new PlayFabException(code + ": " + responseContent);
                }
            }
        }

        public static AstroLauncherServerInfo GetAstroLauncherData(string address)
        {
            JObject serverInfo = SimpleReq("GetCurrentGames", "{\"TagFilter\":{\"Includes\":[{\"Data\":{\"gameId\":\"" + address + "\"}}]}}");
            List<Server> allGames;

            try
            {
                allGames = ((JArray)((JObject)serverInfo.GetValue("data")).GetValue("Games")).ToObject<List<Server>>();
            }
            catch
            {
                throw new PlayFabException(serverInfo.ToString());
            }

            if (allGames.Count == 0) return null;

            Server selectedServer = allGames[0];
            if (selectedServer.DetailedServerInfo == null) return null;

            string serverData = selectedServer.DetailedServerInfo.ServerData;

            try
            {
                return ((JObject)JObject.Parse(serverData).GetValue("customdata")).ToObject<AstroLauncherServerInfo>();
            }
            catch
            {
                return null; // Probably a vanilla server
            }
        }
    }
}
