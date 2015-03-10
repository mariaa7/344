using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Services;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Configuration;
using Microsoft.WindowsAzure.ServiceRuntime;
using System.IO;
namespace WebRole1
{
    /// <summary>
    /// Summary description for WebService2
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line.
    [System.Web.Script.Services.ScriptService]
    public class WebService2 : System.Web.Services.WebService
    {
        private PerformanceCounter memProcess = new PerformanceCounter("Memory", "Available MBytes");
        [WebMethod]
        public float GetAvailableMBytes()
        {
            float memUsage = memProcess.NextValue();
            return memUsage;
        }
        private CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString2"]);
        private CloudBlobClient blobClient;
        CloudBlobContainer container;
        private string lastWord;
        private int number = 999999;
        [WebMethod]
        public string storage()
        {
            blobClient = storageAccount.CreateCloudBlobClient();
            string filePath = "fail";
            container = blobClient.GetContainerReference("pa2");
            if (container.Exists())
            {
                foreach (IListBlobItem item in container.ListBlobs(null, false))
                {
                    if (item.GetType() == typeof(CloudBlockBlob))
                    {
                        CloudBlockBlob blob = container.GetBlockBlobReference("enwiki.txt");
                        filePath = System.IO.Path.GetTempPath() + "\\wiki.txt";
                        using (var fileStream = System.IO.File.OpenWrite(filePath))
                        {
                            blob.DownloadToStream(fileStream);
                        }
                    }
                }
            }
            return filePath;
        }
        public class TrieNode
        {
            public bool isWord = false;
            public Dictionary<char, TrieNode> vertices = new Dictionary<char, TrieNode>();
        }
        public static TrieNode root;
        public WebService2()
        {
            root = new TrieNode();
            addFileToTrie();
        }
        [WebMethod]
        public string addFileToTrie()
        {
            string s = "";
            try
            {
                using (StreamReader sr = new StreamReader(System.IO.Path.GetTempPath() + "\\wiki.txt"))
                {
                    Console.WriteLine(sr.ReadLine());
                    for (int i = 0; i < 1000000; i++)
                    {
                        setNode(sr.ReadLine());
                    }
                    s = sr.ReadLine();
                }
            }
            catch (Exception e)
            {
                s = e.ToString();
            }
            lastWord = s;
            return s;
        }

        [WebMethod]
        public string getLastWord()
        {
            return lastWord;
        }

        [WebMethod]
        public int getCount()
        {
            return number;
        }

        [WebMethod]
        public void setNode(String word)
        {
            var node = root;
            char[] array = word.ToLower().ToCharArray();
            TrieNode temp;
            for (int i = 0; i < array.Length; i++)
            {
                var character = array[i];
                if (character == '_')
                {
                    character = ' ';
                    array[i] = ' ';
                    word = new string(array);
                }
                if (!node.vertices.ContainsKey(character))
                {
                    temp = new TrieNode();
                    if (i == array.Length - 1)
                    {
                        temp.isWord = true;
                    }
                    node.vertices.Add(character, temp);
                }
                else
                {
                    temp = node.vertices[character];
                }
                node = temp;
            }
        }
        [WebMethod]
        public List<string> getChildrenS(string input)
        {
            TrieNode temp = root;
            string s = "";
            char[] array = input.ToLower().ToCharArray();
            foreach (var letter in array)
            {
                if (temp.vertices.ContainsKey(letter))
                {
                    temp = temp.vertices[letter];
                    s = s + letter.ToString();
                }
                else
                {
                    List<String> l = new List<string>();
                    l.Add("failed");
                    return l;
                }
            }
            List<string> children = new List<string>();
            String wordTemp = s;
            return getChildren(temp, wordTemp, children);
        }
        public List<String> getChildren(TrieNode node, string s, List<string> children)
        {
            if (children.Count == 10)
            {
                return children;
            }
            string wordTemp = s;
            foreach (var vertex in node.vertices)
            {
                if (children.Count < 10)
                {
                    s = wordTemp + vertex.Key;
                    if (vertex.Value.isWord)
                    {
                        children.Add(s);
                    }
                    getChildren(vertex.Value, s, children);
                }
                else
                {
                    return children;
                }
            }
            return children;
        }
    }
}