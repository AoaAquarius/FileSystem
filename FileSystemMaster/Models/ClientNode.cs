using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileSystemMaster.Models
{
    public class ClientNode
    {
        public int Id { get; private set; }
        public string URL { get; private set; }
        public ClientNode(int id, string url)
        {
            Id = id;
            URL = url;
        }
    }
}
