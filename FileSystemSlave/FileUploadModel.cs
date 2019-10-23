using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileSystemSlave
{
    public class FileUploadModel
    {
        public string FileName { get; set; }
        public byte[] Content { get; set; }
    }
}
