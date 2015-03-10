using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using System.Configuration;
using System.Net;
using System.IO;
using WorkerRole1;
using System.Diagnostics;

namespace WebRole1
{
    /// <summary>
    /// Summary description for WebService1
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line.
    [System.Web.Script.Services.ScriptService]
    public class WebService1 : System.Web.Services.WebService
    {
        public static CloudTable table;
        public static CloudQueue visitedURLs;
        public static CloudQueue HTMLs;
        public static CloudQueue messages;
        private PerformanceCounter CPU;

        private PerformanceCounter RAM;

        private Dictionary<string, List<string>> cache;

        [WebMethod]
        public string initializeCloud()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            visitedURLs = queueClient.GetQueueReference("visited");
            visitedURLs.CreateIfNotExists();
            HTMLs = queueClient.GetQueueReference("urls");
            HTMLs.CreateIfNotExists();
            messages = queueClient.GetQueueReference("status");
            messages.CreateIfNotExists();
            table = tableClient.GetTableReference("htmlURLs");
            table.CreateIfNotExists();
            return "success";
        }

        [WebMethod]
        public string getCPU()
        {
            CPU = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            String s = this.CPU.NextValue().ToString();
            return s;
        }

        [WebMethod]
        public string getRAM()
        {
            RAM = new PerformanceCounter("Memory", "Available MBytes");
            String s = this.RAM.NextValue().ToString();
            return s;
        }

        [WebMethod]
        public string start()
        {
            initializeCloud();
            if (messages.Exists())
            {
                CloudQueueMessage message = messages.GetMessage(TimeSpan.FromMinutes(5));
                if (message != null)
                {
                    messages.DeleteMessage(message);
                }
                messages.AddMessage(new CloudQueueMessage("Start"));
                return "Start";
            }
            return "failed to start";
        }

        [WebMethod]
        public string ClearEverything()
        {
            HTMLs.Clear();
            messages.Clear();
            messages.AddMessage(new CloudQueueMessage("Stop"));
            table.DeleteIfExists();
            return "Everything has been deleted";
        }

        [WebMethod]
        public List<string> GetResults(string input)
        {
            List<string> s = new List<string>();
            input = input.ToLower();
            char[] spliters = new char[] { ' ', ',', '-', '_', '"', ':', ';', '/', '.', '!', '&', '?' };
            string[] split = input.Split(spliters, StringSplitOptions.RemoveEmptyEntries);
            if (table != null)
            {
                foreach (string word in split)
                {
                    List<string> temp = new List<string>();
                    if (cache != null && cache.ContainsKey(word))
                    {
                        s = cache[word];
                    }
                    else
                    {
                        TableQuery<URL> query = new
                            TableQuery<URL>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, word));
                        foreach (URL entity in table.ExecuteQuery(query))
                        {
                            s.Add(HttpUtility.UrlDecode(entity.RowKey));
                            temp.Add(HttpUtility.UrlDecode(entity.RowKey));
                        }
                        if (cache == null)
                        {
                            cache = new Dictionary<string, List<string>>();
                        }
                        else
                        {
                            cache.Add(word, temp);
                        }
                    }
                }
            }
            var inOrder = s
                .GroupBy(i => i)
                .OrderByDescending(i => i.Count())
                .Select(g => g.Key).Take(10);
            s = inOrder.ToList();
            return s;
        }

        [WebMethod]
        public string GetTitle(string input)
        {
            string s = "URL not found";
            if (table != null)
            {
                TableQuery<URL> query = new
                    TableQuery<URL>().Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, HttpUtility.UrlEncode(input)));
                foreach (URL entity in table.ExecuteQuery(query))
                {
                    return entity.Title;
                }
            }
            return s;
        }


        [WebMethod]
        public List<string> Errors()
        {
            List<string> s = new List<string>();
            if (table != null)
            {
                TableQuery<URL> query = new
                    TableQuery<URL>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "Error"));
                foreach (URL entity in table.ExecuteQuery(query))
                {
                    s.Add(HttpUtility.UrlDecode(entity.RowKey));
                }
            }
            return s;
        }

        [WebMethod]
        public List<string> LastTenParsed()
        {
            List<string> s = new List<string>();
            if (table != null)
            {
                TableQuery<URL> query = new
                    TableQuery<URL>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "LastTen"));
                foreach (URL entity in table.ExecuteQuery(query))
                {
                    s.Add(entity.Title);
                }
            }
            return s;
        }

        [WebMethod]
        public string GetIndexCount()
        {
            string s = "";
            if (table != null)
            {
                TableQuery<URL> query = new
                    TableQuery<URL>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "IndexCount"));
                foreach (URL entity in table.ExecuteQuery(query))
                {
                    s = entity.Title;
                }
            }
            return s;
        }

        [WebMethod]
        public string GetURLsCrawledNum()
        {
            string s = "";
            if (table != null)
            {
                TableQuery<URL> query = new
                    TableQuery<URL>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "URLs"));
                foreach (URL entity in table.ExecuteQuery(query))
                {
                    s = entity.Title;
                }
            }
            return s;
        }

        [WebMethod]
        public string queueSize()
        {
            if (HTMLs.Exists())
            {
                HTMLs.FetchAttributes();
                return HTMLs.ApproximateMessageCount.ToString();
            }
            return "Queue is empty";
        }

        [WebMethod]
        public string statusMessage()
        {
            if (messages.Exists() && messages != null)
            {
                CloudQueueMessage message = messages.PeekMessage();
                if (message != null)
                {
                    if (message.AsString == "Crawling")
                    {
                        return "Crawling";
                    }
                    else if (message.AsString == "Start")
                    {
                        return "Loading - This may take a few minutes";
                    }
                    else
                    {
                        return "Idle";
                    }
                }
            }
            return "Idle";
        }

        [WebMethod]
        public List<string> removeMessage()
        {
            List<string> messages = new List<string>();
            if (HTMLs != null)
            {
                //CloudQueueMessage message = HTMLs.GetMessage(TimeSpan.FromMinutes(5));
                foreach (CloudQueueMessage message in HTMLs.GetMessages(10))
                {
                    if (message != null)
                    {
                        HTMLs.DeleteMessage(message);
                        messages.Add(message.AsString);
                    }
                }
            }
            return messages;
        }

        [WebMethod]
        public List<string> testHTMLParse()
        {
            WebRequest req = HttpWebRequest.Create("http://cnn.com/travel/index.html");
            WebResponse res = req.GetResponse();
            Stream streamRes = res.GetResponseStream();
            StreamReader sr = new StreamReader(streamRes);
            List<string> temp = new List<string>();
            string line = "";
            while ((line = sr.ReadLine()) != null)
            {
                temp.Add(line);
            }
            return temp;
        }

        [WebMethod]
        public List<string> test()
        {
            List<string> siteMaps = new List<String>();
            List<string> siteMaps2 = new List<String>();
            int count = siteMaps.Count;
            string url;
            WebRequest req = HttpWebRequest.Create("http://cnn.com/robots.txt");
            WebResponse res = req.GetResponse();
            Stream streamRes = res.GetResponseStream();
            StreamReader sr = new StreamReader(streamRes);
            html = new List<string>();
            List<string> disallows = new List<String>();
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                if (line.StartsWith("Disallow:"))
                {
                    string[] temp = line.Split(' ');
                    disallows.Add(temp[1]);
                }
                else if (line.StartsWith("Sitemap:"))
                {
                    string[] temp = line.Split(' ');
                    siteMaps.Add(temp[1]);
                }
            }
            return test2(siteMaps);
        }

        public static List<string> html;

        public List<string> test2(List<string> siteMaps)
        {
            string url = siteMaps[siteMaps.Count - 1];
            siteMaps.Remove(siteMaps[siteMaps.Count - 1]);
            //List<string> html = new List<String>();
            WebRequest req = HttpWebRequest.Create(url);
            WebResponse res = req.GetResponse();
            Stream streamRes = res.GetResponseStream();
            StreamReader sr = new StreamReader(streamRes);
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                string[] spliters = new string[] { "<url><loc>", "<loc>", "</loc>" };
                string[] temp = line.Split(spliters, StringSplitOptions.RemoveEmptyEntries);
                if (line.Contains(".xml"))
                {
                    if (temp[1].Contains("2015"))
                    {
                        siteMaps.Add(temp[1]);
                        test2(siteMaps);
                    }
                }
                else if (line.Contains("<loc>") && line.Contains("cnn.com") && !line.Contains("/2014/") && !line.Contains("/2013/") && !line.Contains("/2012/") && !line.Contains("/2011/"))
                {
                    string t = temp[0];
                    if (!t.Contains("cnn"))
                    {
                        html.Add(temp[1]);
                    }
                    else
                    {
                        string[] splitTemp = t.Split('/');
                        if (splitTemp[splitTemp.Length - 1].Contains('.') && !splitTemp[splitTemp.Length - 1].Contains("html"))
                        {

                        }
                        else
                        {
                            html.Add(t);
                        }
                    }
                }
            }
            if (siteMaps.Count != 0)
            {
                test2(siteMaps);
            }
            return html;
        }
    }
}