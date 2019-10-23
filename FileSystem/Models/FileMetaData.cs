using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileSystemClient.Models
{
    public class FileMetaData
    {
        public string FileName { get; set; }
        public int[] NodeIndexForPieces;
        public string[] FilePieceNames;
        public int ChunkSize { get; set; }
    }
}
