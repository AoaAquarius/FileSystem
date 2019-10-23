using FileSystemMaster.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileSystemMaster.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceMappingController : ControllerBase
    {
        private ServiceNodeMapping _serviceNodeMapping;
        private InMemoryStorage _inMemoryStorage;
        public ServiceMappingController(ServiceNodeMapping serviceNodeMapping, InMemoryStorage inMemoryStorage)
        {
            _serviceNodeMapping = serviceNodeMapping;
            _inMemoryStorage = inMemoryStorage;
        }

        [Route("GetClientNodesURL")]
        [HttpGet]
        public string[] GetClientNodesURL()
        {
            return _serviceNodeMapping.ClientNodeURLs;
        }
    }
}
