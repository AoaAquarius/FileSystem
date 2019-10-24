using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FileSystemClient;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;

namespace FileSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private FileService _fileService;
        public FileController(FileService fileService)
        {
            _fileService = fileService;
        }

        [Route("PostFile")]
        [HttpPost]
        public async Task<ActionResult<string>> PostFile([FromForm] IFormCollection formCollection)
        {
            FormFileCollection fileCollection = (FormFileCollection)formCollection.Files;
            IList<Task> tasks = new List<Task>();
            foreach (IFormFile file in fileCollection)
            {
                tasks.Add(_fileService.Upload(file.FileName, file.OpenReadStream()));
            }
            await Task.WhenAll(tasks.AsEnumerable());
            return "Success";
        }

        [Route("GetFile")]
        [HttpGet]
        public async Task<ActionResult> GetFile(string fileName)
        {
            string contentType = _fileService.GetContentType(fileName);
            Stream stream = await _fileService.DownLoad(fileName);
            stream.Seek(0, SeekOrigin.Begin);
            return File(stream, contentType, fileName);
        }
    }
}
