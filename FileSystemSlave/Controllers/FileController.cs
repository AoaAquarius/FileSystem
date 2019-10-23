using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;

namespace FileSystemSlave.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get(string fileName)
        {
            var stream = System.IO.File.OpenRead(@"C:/Test/" + fileName);
            if (stream == null)
                return NotFound();
            return File(stream, "application/octet-stream");
        }

        [HttpPost]
        public string Post([FromBody] IFormCollection formCollection)
        {
            string result = "Success";
            FormFileCollection fileCollection = (FormFileCollection)formCollection.Files;
            try
            {
                foreach (IFormFile file in fileCollection)
                {
                    StreamReader reader = new StreamReader(file.OpenReadStream());
                    string content = reader.ReadToEnd();
                    string name = file.FileName;
                    string filename = @"C:/Test/" + name;
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

        [Route("SaveToStorage")]
        [HttpPost]
        public string SaveToStorage([FromBody] FileUploadModel fileUploadModel)
        {
            string result;
            try
            {
                string filename = @"C:/Test/" + fileUploadModel.FileName;
                if (System.IO.File.Exists(filename))
                {
                    System.IO.File.Delete(filename);
                }
                using (FileStream fs = System.IO.File.Create(filename))
                {
                    fs.Write(fileUploadModel.Content);
                    // Clear Cache
                    fs.Flush();
                }
                result = "Success";
            }
            catch
            {
                result = "Fail";
            }
            return result;
        }
    }
}
