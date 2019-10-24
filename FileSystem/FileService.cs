using FileSystemClient.Models;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace FileSystemClient
{
    public class FileService
    {
        private GlobalVariables _globalVariables;
        public FileService(GlobalVariables globalVariables)
        {
            _globalVariables = globalVariables;
        }

        public async Task<Stream> DownLoad(string fileName)
        {
            FileMetaData fileMetaData = await GetFileMetaData(fileName);
            Stream output = new MemoryStream();
            for (int i = 0; i < fileMetaData.FilePieceNames.Length; i++)
            {
                int nodeIndex = fileMetaData.NodeIndexForPieces[i];
                string partailFileName = fileMetaData.FilePieceNames[i];
                byte[] buffer = await RequestPartialFile(partailFileName, nodeIndex);
                output.Write(buffer);
            }
            return output;
        }

        private async Task<byte[]> RequestPartialFile(string partailFileName, int nodeIndex)
        {
            var client = new RestClient(_globalVariables.SlaveNodeEndpoints[nodeIndex] + _globalVariables.RequestPartialFileRoute + $"?fileName={partailFileName}");
            var request = new RestRequest(Method.GET);
            request.AddHeader("cache-control", "no-cache");
            IRestResponse response = await client.ExecuteTaskAsync(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return response.RawBytes;
            }
            else
            {
                throw new Exception($"Cannot get partial file {partailFileName} on node {nodeIndex}.");
            }
        }

        public async Task<FileMetaData> GetFileMetaData(string fileName)
        {
            var client = new RestClient(_globalVariables.GetFileMatedataURL + $"?fileName={fileName}");
            var request = new RestRequest(Method.GET);
            request.AddHeader("cache-control", "no-cache");
            IRestResponse response = await client.ExecuteTaskAsync(request);
            FileMetaData fileMetaData = null;
            if (response.StatusCode == HttpStatusCode.OK)
            {
                fileMetaData = JsonConvert.DeserializeObject<FileMetaData>(response.Content);
            }
            if (fileMetaData == null)
            {
                throw new Exception("Cannot get split meta data");
            }
            return fileMetaData;
        }

        public void MergeFile(string fileName, int chunkSize, string path)
        {
            const int BUFFER_SIZE = 20 * 1024;
            byte[] buffer = new byte[BUFFER_SIZE];
            int index = 0;
            using (Stream output = File.Create(path + "\\" + fileName))
            {
                while (File.Exists(path + "\\" + index))
                {
                    using (Stream input = File.OpenRead(path + "\\" + index))
                    {
                        input.Read(buffer);
                    }
                    output.Write(buffer);
                    index++;
                }
            }

            //Delete Pieces
            index = 0;
            while (File.Exists(path + "\\" + index))
            {
                File.Delete(path + "\\" + index);
                index++;
            }
        }

        public async Task Upload(string fileName, Stream input)
        {
            //Get split pieces
            var client = new RestClient(_globalVariables.RequestForSplitURL + $"?fileName={fileName}&fileSize={input.Length}");
            var request = new RestRequest(Method.GET);
            request.AddHeader("cache-control", "no-cache");
            IRestResponse response = await client.ExecuteTaskAsync(request);
            FileMetaData fileMetaData = null;
            if (response.StatusCode == HttpStatusCode.OK)
            {
                fileMetaData = JsonConvert.DeserializeObject<FileMetaData>(response.Content);
            }
            if (fileMetaData == null)
            {
                throw new Exception("Cannot get split meta data");
            }
            //Send pieces to slaves
            await SplitFileAsync(input, fileMetaData);
            //SplitFile(input, fileMetaData);
        }

        public async Task SplitFileAsync(Stream input, FileMetaData fileMetaData)
        {
            int position = 0;
            IList<Task> tasks = new List<Task>();
            while (input.Position < input.Length)
            {
                for (int i = 0; i < fileMetaData.FilePieceNames.Length; i++)
                {
                    try
                    {
                        string fileName = fileMetaData.FilePieceNames[i];
                        int nodeIndex = fileMetaData.NodeIndexForPieces[i];
                        tasks.Add(SendPartialFileAsync(input, position, fileName, nodeIndex, fileMetaData.ChunkSize));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
                await Task.WhenAll(tasks.AsEnumerable());
            }
        }

        private void SplitFile(Stream input, FileMetaData fileMetaData)
        {
            int position = 0;
            while (input.Position < input.Length)
            {
                for (int i = 0; i < fileMetaData.FilePieceNames.Length; i++)
                {
                    try
                    {
                        string fileName = fileMetaData.FilePieceNames[i];
                        int nodeIndex = fileMetaData.NodeIndexForPieces[i];
                        SendPartialFile(input, position, fileName, nodeIndex, fileMetaData.ChunkSize);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            }
        }

        private async Task SendPartialFileAsync(Stream input, int position, string partialFileName, int nodeIndex, int bufferSize)
        {
            byte[] buffer = new byte[bufferSize];
            input.Read(buffer, position, bufferSize);
            var client = new RestClient(_globalVariables.SlaveNodeEndpoints[nodeIndex] + _globalVariables.SaveFileAPIRoute);
            FileUploadModel fileUploadModel = new FileUploadModel(partialFileName, buffer);
            var request = new RestRequest(Method.POST);
            request.AddJsonBody(fileUploadModel);
            request.AddHeader("cache-control", "no-cache");
            IRestResponse response = await client.ExecuteTaskAsync(request);
            if (response.StatusCode == HttpStatusCode.OK && JsonConvert.DeserializeObject<string>(response.Content) == "Success")
            {
                Console.WriteLine($"File piece {partialFileName} upload to slave node {nodeIndex} successfully!");
            }
            else
            {
                throw new Exception($"File piece {partialFileName} did not upload to slave node {nodeIndex} successfully!");
            }
        }

        private void SendPartialFile(Stream input, int position, string partialFileName, int nodeIndex, int bufferSize)
        {
            byte[] buffer = new byte[bufferSize];
            input.Read(buffer, position, bufferSize);
            var client = new RestClient(_globalVariables.SlaveNodeEndpoints[nodeIndex] + _globalVariables.SaveFileAPIRoute);
            FileUploadModel fileUploadModel = new FileUploadModel(partialFileName, buffer);
            var request = new RestRequest(Method.POST);
            request.AddJsonBody(fileUploadModel);
            request.AddHeader("cache-control", "no-cache");
            IRestResponse response = client.Execute(request);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"File piece {partialFileName} did not upload to slave node {nodeIndex} successfully!");
            }
        }

        public string GetContentType(string path)
        {
            var types = GetMimeTypes();
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return types[ext];
        }

        private Dictionary<string, string> GetMimeTypes()
        {
            return new Dictionary<string, string>
            {
                {".txt", "text/plain"},
                {".pdf", "application/pdf"},
                {".doc", "application/vnd.ms-word"},
                {".docx", "application/vnd.ms-word"},
                {".xls", "application/vnd.ms-excel"},
                {".xlsx", "application/vnd.openxmlformats"},
                {".png", "image/png"},
                {".jpg", "image/jpeg"},
                {".jpeg", "image/jpeg"},
                {".gif", "image/gif"},
                {".csv", "text/csv"},
                {".exe", "application/vnd.microsoft.portable-executable"}
            };
        }
    }
}
