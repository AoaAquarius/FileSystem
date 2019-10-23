using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileSystemMaster.Models
{
    public class InMemoryStorage
    {
        public Dictionary<string, FileMetaData> Files;
        public InMemoryStorage()
        {
            Files = new Dictionary<string, FileMetaData>();
        }
    }
}
