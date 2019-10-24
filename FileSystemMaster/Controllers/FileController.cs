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
        public IActionResult Get(string fileName)
        {
            try
            {
                if (!_inMemoryStorage.Files.ContainsKey(fileName))
                    return null;
                FileMetaData fileMetaData = _inMemoryStorage.Files.GetValueOrDefault(fileName);
                return Ok(fileMetaData);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [Route("Save")]
        [HttpGet]
        public IActionResult Save(string fileName, string fileSize)
        {
            int curCount = 0;
            string curfilePieceName;
            long curHashCode;
            try
            {
                if (_inMemoryStorage.Files.ContainsKey(fileName))
                {
                    return Ok(_inMemoryStorage.Files.GetValueOrDefault(fileName));
                }
                int chunkSize = 20 * 1024;
                long fileTotalSize = Convert.ToInt64(fileSize);
                int totalPieceCount = (int)(fileTotalSize / chunkSize);
                if (fileTotalSize % chunkSize != 0)
                    totalPieceCount++;
                FileMetaData fileMetaData = new FileMetaData(fileName, totalPieceCount, chunkSize);
                curCount = totalPieceCount;
                while (curCount > 0)
                {
                    int pieceIndex = totalPieceCount - curCount;
                    string filePieceName = Helper.GetFilePieceName(fileName, pieceIndex);
                    long hashCode = Helper.GetInt64HashCode(filePieceName);

                    curHashCode = hashCode;
                    curfilePieceName = filePieceName;
                    //Look for node
                    var keyValuePairs = _serviceNodeMapping.GlobalMappings.SkipWhile(pair => pair.Key < hashCode);
                    if (keyValuePairs.Count() == 0)
                        hashCode = 0;
                    int node = _serviceNodeMapping.GlobalMappings.SkipWhile(pair => pair.Key < hashCode).First().Value;
                    fileMetaData.NodeIndexForPieces[pieceIndex] = node;
                    fileMetaData.FilePieceNames[pieceIndex] = filePieceName;
                    curCount--;
                }
                _inMemoryStorage.Files.Add(fileName, fileMetaData);
                return Ok(fileMetaData);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
