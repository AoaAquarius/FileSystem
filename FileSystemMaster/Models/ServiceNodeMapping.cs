using FileSystemMaster.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileSystemMaster.Models
{
    public class ServiceNodeMapping
    {
        private readonly int _virtualNodeCount = 1000;
        public SortedDictionary<long, int> GlobalMappings;
        public List<HashSet<long>> NodesMappings;
        public int NodeCount;
        public string[] ClientNodeURLs;

        public ServiceNodeMapping(string[] clientNodeURLs)
        {
            int nodeCount = clientNodeURLs.Length;
            GlobalMappings = new SortedDictionary<long, int>();
            NodesMappings = new List<HashSet<long>>();
            Random random = new Random();
            NodeCount = nodeCount;
            ClientNodeURLs = clientNodeURLs;
            while (nodeCount > 0)
            {
                HashSet<long> nodeMapping = new HashSet<long>();
                int temp = _virtualNodeCount;
                while (temp > 0)
                {
                    long num;
                    do
                    {
                        num = (long)(random.NextDouble() * Int64.MaxValue);
                    }
                    while (GlobalMappings.ContainsKey(num));
                    nodeMapping.Add(num);
                    GlobalMappings.Add(num, NodeCount - nodeCount);
                    temp--;
                }
                NodesMappings.Add(nodeMapping);
                nodeCount--;
            }
        }
    }
}
