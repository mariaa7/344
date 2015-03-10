using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;


namespace ClassLibrary1
{
    public class Crawler
    {
        public static HashSet<string> visitedURLs { get; set; }
        public static List<string> html { get; set; }
        public static List<string> disallows { get; set; }
        public static int URLsCrawled { get; set; }

        public Crawler()
        {
            //ClassLibrary1.Crawler.visitedURLs = new List<string>();
            visitedURLs = new HashSet<string>();
        }

        public List<string> crawlRobots()
        {
            List<string> siteMaps = new List<String>();
            //List<string> siteMaps2 = new List<String>();
            //siteMaps.Add("http://cnn.com/robots.txt");
            int count = siteMaps.Count;
            WebRequest req = HttpWebRequest.Create("http://bleacherreport.com/robots.txt");
            WebResponse res = req.GetResponse();
            Stream streamRes = res.GetResponseStream();
            List<string> urls = new List<string>();
            string line;
            if (streamRes != null)
            {
                StreamReader sr = new StreamReader(streamRes);

                disallows = new List<String>();

                while ((line = sr.ReadLine()) != null)
                {
                    if (line.StartsWith("Disallow:"))
                    {
                        string[] temp = line.Split(' ');
                        disallows.Add(temp[1]);
                    }
                    else if (line.StartsWith("Sitemap:") && line.Contains("nba"))
                    {

                        string[] temp = line.Split(' ');
                        siteMaps.Add(temp[1]);
                    }
                }
                sr.Close();
            }
            req = HttpWebRequest.Create(siteMaps[0]);
            res = req.GetResponse();
            streamRes = res.GetResponseStream();
            if (streamRes != null)
            {
                StreamReader sr = new StreamReader(streamRes);
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.Contains("<loc>"))
                    {
                        string[] spliters = new string[] { "<loc>", "</loc>" };
                        string[] temp = line.Split(spliters, StringSplitOptions.RemoveEmptyEntries);
                        urls.Add(temp[1]);
                    }
                }
                sr.Close();
            }
            return urls;
        }

        public static List<string> cnnhtml;

        public List<string> cnnRobotsCrawl()
        {
            List<string> siteMaps = new List<String>();
            List<string> siteMaps2 = new List<String>();
            int count = siteMaps.Count;
            string url;
            WebRequest req = HttpWebRequest.Create("http://cnn.com/robots.txt");
            WebResponse res = req.GetResponse();
            Stream streamRes = res.GetResponseStream();
            if (streamRes != null)
            {
                StreamReader sr = new StreamReader(streamRes);
                cnnhtml = new List<string>();
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
                sr.Close();
            }
            return test2(siteMaps);
        }

        public List<string> test2(List<string> siteMaps)
        {
            string url = siteMaps[siteMaps.Count - 1];
            siteMaps.Remove(siteMaps[siteMaps.Count - 1]);
            //List<string> html = new List<String>();
            WebRequest req = HttpWebRequest.Create(url);
            WebResponse res = req.GetResponse();
            Stream streamRes = res.GetResponseStream();
            if (streamRes != null)
            {
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
                            cnnhtml.Add(temp[1]);
                        }
                        else
                        {
                            string[] splitTemp = t.Split('/');
                            if (splitTemp[splitTemp.Length - 1].Contains('.') && !splitTemp[splitTemp.Length - 1].Contains("html"))
                            {

                            }
                            else
                            {
                                cnnhtml.Add(t);
                            }
                        }
                    }
                }
                sr.Close();
            }
            if (siteMaps.Count != 0)
            {
                test2(siteMaps);
            }
            return cnnhtml;
        }

        public List<string> crawlLink(string html)
        {
            List<string> links = new List<string>();
            try
            {
                HtmlWeb hw = new HtmlWeb();

                HtmlDocument doc = hw.Load(html);
                visitedURLs.Add(html);
                string title = doc.DocumentNode.SelectSingleNode("//head/title").InnerText;
                //string meta = doc.DocumentNode.SelectSingleNode("//meta[@name='pubdate']").GetAttributeValue("co‌​ntent", String.Empty);
                links.Add(title);
                // if (meta != null)
                // {
                // 	links.Add(meta);
                // }
                // else
                // {
                // 	links.Add("No date");
                // }
                foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//a[@href]"))
                {
                    HtmlAttribute att = link.Attributes["href"];
                    string url = att.Value;
                    if (html.Contains("bleacherreport.com/"))
                    {
                        if (url.StartsWith("//"))
                        {
                            url = "http:" + att.Value;
                            if (!visitedURLs.Contains(url) && !links.Contains(url))
                            {
                                //queue.AddMessage(new CloudQueueMessage(url));
                                links.Add(url);

                            }

                        }
                        else if (url.StartsWith("/"))
                        {
                            url = "http://bleacherreport.com" + att.Value;

                            if (!visitedURLs.Contains(url) && !links.Contains(url))
                            {
                                //queue.AddMessage(new CloudQueueMessage(url));
                                links.Add(url);
                            }
                        }
                        else if ((url.Contains('/') && url.Contains(".bleacherreport.com/")) || (url.Contains('/') && url.Contains("//bleacherreport.com")))
                        {
                            if (!visitedURLs.Contains(url) && !links.Contains(url))
                            {
                                //queue.AddMessage(new CloudQueueMessage(url));
                                links.Add(url);
                            }
                        }
                    }
                    else
                    {
                        if (url.StartsWith("//"))
                        {
                            url = "http:" + att.Value;
                            if (!visitedURLs.Contains(url) && !links.Contains(url))
                            {
                                //queue.AddMessage(new CloudQueueMessage(url));
                                links.Add(url);

                            }

                        }
                        else if (url.StartsWith("/"))
                        {
                            url = "http://cnn.com" + att.Value;

                            if (!visitedURLs.Contains(url) && !links.Contains(url))
                            {
                                //queue.AddMessage(new CloudQueueMessage(url));
                                links.Add(url);
                            }
                        }
                        else if ((url.Contains('/') && url.Contains(".cnn.com/")) || (url.Contains('/') && url.Contains("//cnn.com")))
                        {
                            if (!visitedURLs.Contains(url) && !links.Contains(url))
                            {
                                //queue.AddMessage(new CloudQueueMessage(url));
                                links.Add(url);
                            }
                        }
                    }
                }
                return links;
            }
            catch (Exception e)
            {
                links.Add(e.ToString());
                // links.Add("No date");
                return links;
            }
        }

        //  	public string visited(string html)
        // 	{
        //     	visitedURLs.Add(html);
        //     	return html;
        //  	}

        public bool isAllowed(string html)
        {
            foreach (string disallow in disallows)
            {
                if (html.Contains(disallow))
                {
                    return false;
                }
            }
            return true;
        }

        public HashSet<string> visited()
        {
            return visitedURLs;
        }
    }
}