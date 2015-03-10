using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using System.Configuration;
using Microsoft.WindowsAzure.Storage.Table;
using System.IO;
using ClassLibrary1;

namespace WorkerRole1
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);

        public static CloudQueue HTMLs;
        public static CloudQueue visitedURLs;
        public static List<String> lastTen;
        public static Crawler spider;
        public static CloudTable table;
        public static CloudQueue messages;
        public static int index;
        public static int urlsCrawled;

        public override void Run()
        {
            Trace.TraceInformation("WorkerRole1 is running");
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            table = tableClient.GetTableReference("htmlURLs");
            HTMLs = queueClient.GetQueueReference("urls");
            messages = queueClient.GetQueueReference("status");
            visitedURLs = queueClient.GetQueueReference("visited");
            bool robotsparsed = false;
            CloudQueueMessage status = null;
            lastTen = new List<String>();
            while (true)
            {
                Thread.Sleep(10);
                if (messages.Exists())
                {
                    status = messages.PeekMessage();
                }
                if (status != null && status.AsString.Equals("Start") && HTMLs != null && !robotsparsed)
                {

                    spider = new Crawler();
                    List<string> urls = spider.crawlRobots();
                    List<string> urls2 = spider.cnnRobotsCrawl();
                    index = 0;
                    urlsCrawled = 0;
                    foreach (string url in urls)
                    {
                        HTMLs.AddMessage(new CloudQueueMessage(url));
                    }
                    foreach (string url in urls2)
                    {
                        HTMLs.AddMessage(new CloudQueueMessage(url));
                    }
                    robotsparsed = true;
                    messages.DeleteMessage(messages.GetMessage(TimeSpan.FromMinutes(5)));
                    messages.AddMessage(new CloudQueueMessage("Crawling"));
                }
                else if (status != null && !status.AsString.Equals("Stop") && HTMLs != null && robotsparsed)
                {
                    CloudQueueMessage message = HTMLs.GetMessage(TimeSpan.FromMinutes(5));
                    if (message != null)
                    {
                        HTMLs.DeleteMessage(message);
                        string messageString = message.AsString;
                        if (spider.isAllowed(messageString))
                        {
                            List<string> newLinks = spider.crawlLink(messageString);
                            URL entry = null;
                            List<URL> urlEntries = new List<URL>();
                            if (newLinks[0] == "Error")
                            {
                                urlEntries.Add(new URL(messageString, newLinks[0], newLinks[0]));
                            }
                            else
                            {
                                char[] spliters = new char[] { ' ', ',', '-', '_', '"', ':', ';', '/', '.', '!', '&', '?', '|', '$', '%', '(', ')' };
                                string[] split = newLinks[0].Split(spliters, StringSplitOptions.RemoveEmptyEntries);

                                foreach (string word in split)
                                {
                                    string temp = word.ToLower();
                                    urlEntries.Add(new URL(messageString, temp, newLinks[0]));
                                }
                                //entry = new URL(messageString, newLinks[0]);
                            }
                            if (lastTen.Count < 10)
                            {
                                lastTen.Add(messageString);
                            }
                            else
                            {
                                lastTen.Remove(lastTen[0]);
                                lastTen.Add(messageString);
                            }
                            if (spider.visited().Contains(messageString))
                            {
                                index++;
                            }
                            urlsCrawled++;
                            foreach (URL urlEntry in urlEntries)
                            {
                                TableOperation insertOperation = TableOperation.InsertOrReplace(urlEntry);
                                table.ExecuteAsync(insertOperation);
                            }
                            //var result = table.BeginExecute(insertOperation,
                            //new AsyncCallback(onTableExecuteComplete), entity);
                            //result.AsyncWaitHandle.WaitOne();
                            if (newLinks.Count > 1)
                            {
                                for (int i = 2; i < newLinks.Count - 1; i++)
                                {
                                    HTMLs.AddMessageAsync(new CloudQueueMessage(newLinks[i]));
                                }
                            }
                        }
                        URL tableEntry = new URL("index", "IndexCount", index.ToString());
                        TableOperation indexOperation = TableOperation.InsertOrReplace(tableEntry);
                        table.Execute(indexOperation);
                        URL tableEntry2 = new URL("crawled", "URLs", urlsCrawled.ToString());
                        TableOperation urlCountOperation = TableOperation.InsertOrReplace(tableEntry2);
                        table.Execute(urlCountOperation);
                        URL lastTenEntry;
                        TableOperation lastTenOp;
                        for (int i = 0; i < lastTen.Count; i++)
                        {
                            lastTenEntry = new URL(i.ToString(), "LastTen", lastTen[i]);
                            lastTenOp = TableOperation.InsertOrReplace(lastTenEntry);
                            table.Execute(lastTenOp);
                        }
                    }
                    else
                    {
                        messages.DeleteMessage(status);
                    }

                }

            }
            // 	try
            //   	{
            //       	this.RunAsync(this.cancellationTokenSource.Token).Wait();
            //   	}
            //   	finally
            //   	{
            //       	this.runCompleteEvent.Set();
            //   	}
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            bool result = base.OnStart();

            Trace.TraceInformation("WorkerRole1 has been started");

            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("WorkerRole1 is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("WorkerRole1 has stopped");
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following with your own logic.
            while (!cancellationToken.IsCancellationRequested)
            {
                Trace.TraceInformation("Working");
                await Task.Delay(1000);
            }
        }
    }
}