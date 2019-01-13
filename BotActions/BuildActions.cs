using LuisBot.Entity;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;
using System.Xml;

namespace LuisBot.BotActions
{
    public class BuildActions
    {
        static string url = "https://jenkinsmaster25.azurewebsites.net/";
        static string _errorMessage = "Sorry, I couldn't retrive information for you now due to some issues. Please try again later..";
        static List<string> getTagValue(string xmlFile, string tagName)
        {
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.LoadXml(xmlFile);
            List<string> nodeName = new List<string>();
            XmlNodeList nodeList = xmldoc.GetElementsByTagName(tagName);
            string Short_Fall = string.Empty;
            foreach (XmlNode node in nodeList)
            {
                Short_Fall = node.InnerText;
                nodeName.Add(Short_Fall);
                //  Console.WriteLine(Short_Fall);
            }
            return nodeName;
        }
        static HttpClient addAuthToHeader(HttpClient client)
        {
            string userid = "admin", password = "e914edab16564c64a117d422cd92bc44";//"463006786cdf345c7b595a65d2aa8e98";
            string basicAuthToken = Convert.ToBase64String(Encoding.Default.GetBytes(userid + ":" + password));
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", basicAuthToken);
            return client;
        }
        public async static void TriggerBuild(String job)
        {
            using (HttpClient client = new HttpClient())
            {
                addAuthToHeader(client);
                using (HttpResponseMessage response = await client.GetAsync(url + "/job/" + job + "/build?token=nekottoken"))
                {
                    using (HttpContent content = response.Content)
                    {
                        string myContent = await content.ReadAsStringAsync();
                        Console.WriteLine(myContent);
                    }
                }
            }
        }

        public static async Task<string> CreateJob(String jobName)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    addAuthToHeader(client);
                    string xmlString = System.IO.File.ReadAllText($"D:\\consoleapp\\config.xml");
                    var httpContent = new StringContent(xmlString, Encoding.UTF8, "application/xml");

                    using (HttpResponseMessage response = await client.PostAsync(url + "/createItem?name=" + jobName, httpContent))
                    {
                        HttpContent content = response.Content;

                        string myContent = await content.ReadAsStringAsync();
                        Console.WriteLine(myContent);
                    }

                }
                return String.Format("New job {0} has been created for you.", jobName);
            }
            catch (Exception ex)
            {
                return "Could not create job right now.\nPlease try again later";
            }
        }

        public async static Task<string> LastBuildInfo(string jobName)
        {
            string allBuildInfo = await AllBuildInfo();
            string requiredBuildInfo = "No such job/build found!!! Please provide correct job/build name";
            string splitChar = ")";
            char[] c = splitChar.ToCharArray();
            string[] buildInfo = allBuildInfo.Split(c);
            foreach (string build in buildInfo)
            {
                try
                {
                    if (build.Contains(jobName))
                    {
                        requiredBuildInfo = build.Substring(1, build.Length - 2);
                        break;
                    }
                }
                catch (Exception e) { }
            }
            return requiredBuildInfo;

        }

        public async static Task<string> LastBuildStatus(string jobName)
        {
            string BuildInfo = await LastBuildInfo(jobName);
            string requiredBuildStatus = "No such job/build found!!! Please provide correct job/build name";
            string splitChar = ",";
            char[] c = splitChar.ToCharArray();
            string[] buildInfo = BuildInfo.Split(c);
            foreach (string build in buildInfo)
            {
                if (build.Contains("Build result"))
                {
                    requiredBuildStatus = build.Substring(1, build.Length - 3);
                    break;
                }
            }
            return requiredBuildStatus;

        }

        public async static Task<string> AllBuildInfo()
        {
            using (HttpClient client = new HttpClient())
            {
                addAuthToHeader(client);
                using (HttpResponseMessage response = await client.GetAsync(url + "/api/json?depth=2&pretty=true*&tree=jobs[name,lastBuild[number,duration,timestamp,result,changeSet[items[msg,author[fullName]]]]]"))
                {
                    using (HttpContent content = response.Content)
                    {
                        string myContent = await content.ReadAsStringAsync();
                        string json_content = "";

                        JToken token = JObject.Parse(myContent);
                        JObject jObject = JObject.Parse(myContent);
                        string name = "", result = "", number = "", Timestamp = "", duration = "";
                        JArray Jobs = (JArray)jObject["jobs"];
                        int i = 1;
                        foreach (var variablename in Jobs)

                        {
                            try
                            {
                                name = variablename["name"].ToString();
                                JToken lastBuild = variablename["lastBuild"];
                                result = lastBuild["result"].ToString();
                                number = lastBuild["number"].ToString();
                                //Timestamp = lastBuild["timestamp"].ToString();
                                duration = lastBuild["duration"].ToString();

                                json_content += i++.ToString() + ") JOB NAME-" + name + " - Last build details-" + " Duration - " + duration + "ms, Build number - " + number + ", Build result - " + result + "\n\n";
                            }
                            catch (Exception e) { }
                        }


                        return json_content;
                    }
                }
            }


        }

        public async static Task<string> GetBuildNumber(String job)
        {
            string myContent = "";
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    addAuthToHeader(client);
                    using (HttpResponseMessage response = await client.GetAsync(url + "/job/" + job + "/1/buildNumber"))
                    {
                        using (HttpContent content = response.Content)
                        {
                            myContent = await content.ReadAsStringAsync();
                            Console.WriteLine(myContent);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return _errorMessage;
            }
            return String.Format("Build number for the job {1} is {0}", job, myContent);
        }
        public async static Task<string> BuildTimeStamp(string job)
        {
            string myContent = "";
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    addAuthToHeader(client);
                    using (HttpResponseMessage response = await client.GetAsync(url + "/job/" + job + "/1/buildTimestamp"))
                    {
                        using (HttpContent content = response.Content)
                        {
                            myContent = await content.ReadAsStringAsync();
                            Console.WriteLine(myContent);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return _errorMessage;
            }
            return String.Format("{0}, is the last build time stamp for {1}", myContent, job);
        }

        public async static Task<string> ForceRestart()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    addAuthToHeader(client);
                    using (HttpResponseMessage response = await client.GetAsync(url + "/restart"))
                    {
                        using (HttpContent content = response.Content)
                        {
                            var myContent = await content.ReadAsStringAsync();
                            Console.WriteLine(myContent);
                        }
                    }
                }
                return "I have successfully force restarted the server";
            }
            catch (Exception ex)
            {
                return "I cannot force restart now due to issues.. Please try again later.";
            }
        }

        public async static Task<string> SafeRestart()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    addAuthToHeader(client);
                    using (HttpResponseMessage response = await client.GetAsync(url + "/safeRestart"))
                    {
                        using (HttpContent content = response.Content)
                        {
                            string myContent = await content.ReadAsStringAsync();
                            Console.WriteLine(myContent);
                        }
                    }
                }
                return "I have successfully safe restarted the server";
            }
            catch (Exception ex)
            {
                return "I cannot Safe restart now due to issues.. Please try again later.";
            }
        }

        public async static Task<string> GetAvailablePlugin()
        {

            string myContent = "";
            List<string> pluginsList;
            string pluginname = "";
            using (HttpClient client = new HttpClient())
            {
                addAuthToHeader(client);
                using (HttpResponseMessage response = await client.GetAsync(url + "/pluginManager/api/xml?depth=1&xpath=/*/*/shortName|/*/*/version&wrapper=plugins"))
                {
                    using (HttpContent content = response.Content)
                    {
                        myContent = await content.ReadAsStringAsync();
                        // Console.WriteLine(myContent);
                        pluginsList = getTagValue(myContent, "shortName");
                    }
                }
                pluginname = "These are the available plugins you asked" + Environment.NewLine;
                int i = 0;
                foreach (var plugindata in pluginsList)
                {
                    pluginname += " " + Convert.ToString(++i) + ") " + plugindata;

                }
            }
            return pluginname;

        }

        public async static Task<string> DeleteJob(String jobName)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    addAuthToHeader(client);
                    string xmlString = System.IO.File.ReadAllText($"D:\\consoleapp\\config.xml");
                    var httpContent = new StringContent(xmlString, Encoding.UTF8, "application/xml");
                    using (HttpResponseMessage response = await client.PostAsync(url + "/job/" + jobName + "/doDelete", httpContent))
                    {
                        HttpContent content = response.Content;

                        string myContent = await content.ReadAsStringAsync();
                        Console.WriteLine(myContent);
                    }
                }
                return String.Format("I have deleted the job {0} successfully", jobName);
            }
            catch (Exception ex)
            {
                return _errorMessage;
            }
        }
    }
}