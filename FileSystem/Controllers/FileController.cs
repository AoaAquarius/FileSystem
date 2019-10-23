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

        [Route("Test")]
        [HttpPost]
        public async Task<ActionResult<string>> Test([FromForm] IFormCollection formCollection)
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

        [Route("PostFile")]
        [HttpPost]
        public String PostFile([FromForm] IFormCollection formCollection)
        {
            String result = "Success";
            FormFileCollection fileCollection = (FormFileCollection)formCollection.Files;
            try
            {
                foreach (IFormFile file in fileCollection)
                {
                    StreamReader reader = new StreamReader(file.OpenReadStream());
                    String content = reader.ReadToEnd();
                    String name = file.FileName;
                    String filename = @"C:/Test/" + name;
                    if (System.IO.File.Exists(filename))
                    {
                        System.IO.File.Delete(filename);
                    }
                    using (FileStream fs = System.IO.File.Create(filename))
                    {
                        // Copy to file
                        file.CopyTo(fs);
                        // Clear Cache
                        fs.Flush();
                    }
                    result = "Success";
                }
            }
            catch
            {
                result = "Fail";
            }
            return result;
        }

        [HttpPut("MergeFile")]
        public String MergeFile()
        {
            _fileService.MergeFile("garrett-parker-DlkF4-dbCOU-unsplash.jpg", 20 * 1024, @"C:/Test/");
            return "OK";
        }
    }
}
