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
        private GlobalVariables _globalVariables;
        public FileController(GlobalVariables globalVariables)
        {
            _globalVariables = globalVariables;
        }
        [HttpGet]
        [Route("get")]
        public IActionResult Get(string fileName)
        {
            var stream = System.IO.File.OpenRead(_globalVariables.RootDirectory + fileName);
            if (stream == null)
                return NotFound();
            return File(stream, "application/octet-stream");
        }

        [Route("SaveToStorage")]
        [HttpPost]
        public string SaveToStorage([FromBody] FileUploadModel fileUploadModel)
        {
            string result;
            try
            {
                string filename = _globalVariables.RootDirectory + fileUploadModel.FileName;
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
            catch (Exception e)
            {
                result = e.Message;
            }
            return result;
        }
    }
}
