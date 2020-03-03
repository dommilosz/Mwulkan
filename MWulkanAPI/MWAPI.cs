using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Windows.Forms;

namespace MWulkanAPI
{
    public static class MWAPI
    {
        public static string token;
        public static List<RootingRule> RootingRules;
        public static void Test()
        {
            GetRoutingRules();
            ResolveCode("3S1VBFJ");
        }

        public static string GetPFX(string code, string pin, string symbol)
        {
            RestClient client = new RestClient(new Uri($"{ResolveCode(code)}/{symbol}/mobile-api/Uczen.v3.UczenStart/Certyfikat"));
            var request = new RestRequest();

            int unixTimestamp = (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            string devman = "Mwulkan";
            string devmod = Environment.MachineName;
            string uuid1 = Guid.NewGuid().ToString();
            string uuid2 = Guid.NewGuid().ToString();
            string content = $"{{ \"PIN\": \"{pin}\", \"TokenKey\": \"{code}\", \"AppVersion\": \"18.4.1.388\", \"DeviceId\": \"{uuid1}\", \"DeviceName\": \"{devman}#{devmod}\", \"DeviceNameUser\": \"\", \"DeviceDescription\": \"\", \"DeviceSystemType\": \"Android\", \"DeviceSystemVersion\": \"6.0.1\", \"RemoteMobileTimeKey\": {unixTimestamp}, \"TimeKey\": {unixTimestamp - 1}, \"RequestId\": \"{uuid2}\", \"RemoteMobileAppVersion\": \"18.4.1.388\", \"RemoteMobileAppName\": \"VULCAN-Android-ModulUcznia\" }} ";

            var body = new RequestBody("application/json", "", content);
            request.Body = body;
            request.AddHeader("RequestMobileType", "RegisterDevice");
            request.AddHeader("User-Agent", "MobileUserAgent");
            request.AddHeader("Content-Type", "application/json");
            var response = client.Post(request);
            var json = response.Content;
            try
            {
                var data = (JObject)JsonConvert.DeserializeObject(json);
                var token2 = data["TokenCert"];
                token = token2.ToString();
                File.WriteAllText($"{Application.StartupPath}/token.tk",token.ToString());
            }
            catch { }
            return "";
        }
        public static void GetRoutingRules()
        {
            RootingRules = new List<RootingRule>();
            WebClient w = new WebClient();
            var tmp = w.DownloadString("http://komponenty.vulcan.net.pl/UonetPlusMobile/RoutingRules.txt");
            var tables = tmp.Split('\n').ToList();
            for (int i = 0; i < tables.Count; i++)
            {
                tables[i] = tables[i].TrimEnd('\r');
            }
            foreach (var item in tables)
            {
                if (item.Split(',').Length > 1)
                    RootingRules.Add(new RootingRule(item.Split(',')[0], item.Split(',')[1]));
            }
        }
        public static string ResolveCode(string code)
        {
            GetRoutingRules();
            string symbol = code[0].ToString() + code[1].ToString() + code[2].ToString();
            foreach (var item in MWAPI.RootingRules)
            {
                if (item.code.Contains(symbol)) return item.url;
            }
            return null;
        }
    }
    public class RootingRule
    {
        public string code;
        public string url;
        public RootingRule(string code, string url)
        {
            this.code = code;
            this.url = url;
        }
    }
}
