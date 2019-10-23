using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FileSystemClient.Models
{
    public class FileUploadModel
    {
        public string FileName { get; private set; }
        public byte[] Content { get; private set; }
        public FileUploadModel(string fileName, byte[] content)
        {
            FileName = fileName;
            Content = content;
        }
    }
}
