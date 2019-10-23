using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FileSystemMaster.Models
{
    public class FileMetaData
    {
        public string FileName { get; private set; }
        public int[] NodeIndexForPieces;
        public string[] FilePieceNames;
        public int ChunkSize { get; private set; }
        public FileMetaData(string fileName, int pieceCount, int chunkSize)
        {
            FileName = fileName;
            NodeIndexForPieces = new int[pieceCount];
            FilePieceNames = new string[pieceCount];
            ChunkSize = chunkSize;
        }
    }
}
