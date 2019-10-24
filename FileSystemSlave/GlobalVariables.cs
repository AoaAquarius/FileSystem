using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileSystemSlave
{
    public class GlobalVariables
    {
        public string RootDirectory { get; private set; }
        public GlobalVariables(IConfiguration configuration)
        {
            RootDirectory = configuration["RootDirectory"];
        }
    }
}
