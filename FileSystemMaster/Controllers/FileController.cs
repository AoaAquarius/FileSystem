using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;
using FileSystemMaster.Models;
using Microsoft.AspNetCore.Mvc;

namespace FileSystemMaster.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private ServiceNodeMapping _serviceNodeMapping;
        private InMemoryStorage _inMemoryStorage;
        public FileController(ServiceNodeMapping serviceNodeMapping, InMemoryStorage inMemoryStorage)
        {
            _serviceNodeMapping = serviceNodeMapping;
            _inMemoryStorage = inMemoryStorage;
        }

        [Route("Get")]
        [HttpGet]
        public FileMetaData Get(string fileName)
        {
            if (!_inMemoryStorage.Files.ContainsKey(fileName))
                return null;
            FileMetaData fileMetaData = _inMemoryStorage.Files.GetValueOrDefault(fileName);
            return fileMetaData;
        }

        [Route("Save")]
        [HttpGet]
        public FileMetaData Save(string fileName, string fileSize)
        {
            if (_inMemoryStorage.Files.ContainsKey(fileName))
            {
                return _inMemoryStorage.Files.GetValueOrDefault(fileName);
            }
            int chunkSize = 20 * 1024;
            long fileTotalSize = Convert.ToInt64(fileSize);
            int totalPieceCount = (int)(fileTotalSize / chunkSize);
            if (fileTotalSize % chunkSize != 0)
                totalPieceCount++;
            FileMetaData fileMetaData = new FileMetaData(fileName, totalPieceCount, chunkSize);
            int curCount = totalPieceCount;
            while (curCount > 0)
            {
                int pieceIndex = totalPieceCount - curCount;
                string filePieceName = Helper.GetFilePieceName(fileName, pieceIndex);
                long hashCode = Helper.GetInt64HashCode(filePieceName);
                //Look for node
                int node = _serviceNodeMapping.GlobalMappings.SkipWhile(pair => pair.Key < hashCode).First().Value;
                fileMetaData.NodeIndexForPieces[pieceIndex] = node;
                fileMetaData.FilePieceNames[pieceIndex] = filePieceName;
                curCount--;
            }
            _inMemoryStorage.Files.Add(fileName, fileMetaData);
            return fileMetaData;
        }
    }
}
