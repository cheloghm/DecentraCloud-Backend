using DecentraCloud.API.Data;
using DecentraCloud.API.DTOs;
using DecentraCloud.API.Interfaces.RepositoryInterfaces;
using DecentraCloud.API.Models;
using MongoDB.Bson.IO;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DecentraCloud.API.Repositories
{
    public class FileRepository : IFileRepository
    {
        private readonly DecentraCloudContext _context;

        public FileRepository(DecentraCloudContext context)
        {
            _context = context;
        }

        private HttpClient CreateHttpClient()
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true
            };

            return new HttpClient(handler);
        }

        public async Task AddFileRecord(FileRecord fileRecord)
        {
            await _context.Files.InsertOneAsync(fileRecord);
        }

        public async Task<FileRecord> GetFileRecord(string userId, string filename)
        {
            var filter = Builders<FileRecord>.Filter.And(
                Builders<FileRecord>.Filter.Eq(f => f.UserId, userId),
                Builders<FileRecord>.Filter.Eq(f => f.Filename, filename)
            );
            return await _context.Files.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<FileRecord> GetFileRecordById(string fileId)
        {
            return await _context.Files.Find(f => f.Id == fileId).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<FileRecord>> GetFilesByUserId(string userId)
        {
            return await _context.Files.Find(f => f.UserId == userId).ToListAsync();
        }

        public async Task<bool> DeleteFileRecord(string userId, string filename)
        {
            var filter = Builders<FileRecord>.Filter.And(
                Builders<FileRecord>.Filter.Eq(f => f.UserId, userId),
                Builders<FileRecord>.Filter.Eq(f => f.Filename, filename)
            );
            var result = await _context.Files.DeleteOneAsync(filter);
            return result.DeletedCount > 0;
        }

        public async Task<bool> UploadFileToNode(FileUploadDto fileUploadDto, Node node)
        {
            try
            {
                var httpClient = CreateHttpClient();
                var url = $"{node.Endpoint}/storage/upload";
                var content = new MultipartFormDataContent
                {
                    { new ByteArrayContent(fileUploadDto.Data), "file", fileUploadDto.Filename },
                    { new StringContent(fileUploadDto.UserId), "userId" },
                    { new StringContent(fileUploadDto.Filename), "filename" }
                };

                var request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = content
                };
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", node.Token);

                var response = await httpClient.SendAsync(request);
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                // Log and handle the exception
                Console.WriteLine($"HTTP Request failed: {ex.Message}");
                return false;
            }
        }

        public async Task<byte[]> DownloadFileFromNode(string userId, string fileId, Node node)
        {
            var httpClient = CreateHttpClient();
            var url = $"{node.Endpoint}/storage/download/{userId}/{fileId}";
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", node.Token);
            var response = await httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadAsByteArrayAsync();
        }

        public async Task<byte[]> ViewFileOnNode(string userId, string fileId, Node node)
        {
            var httpClient = CreateHttpClient();
            var url = $"{node.Endpoint}/storage/view/{userId}/{fileId}";
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", node.Token);
            var response = await httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadAsByteArrayAsync();
        }

        public async Task<IEnumerable<FileRecord>> SearchFileRecords(string userId, string query)
        {
            var filter = Builders<FileRecord>.Filter.And(
                Builders<FileRecord>.Filter.Eq(f => f.UserId, userId),
                Builders<FileRecord>.Filter.Regex(f => f.Filename, new MongoDB.Bson.BsonRegularExpression(query, "i"))
            );
            return await _context.Files.Find(filter).ToListAsync();
        }

        public async Task<bool> DeleteFileRecordById(string fileId)
        {
            var filter = Builders<FileRecord>.Filter.Eq(f => f.Id, fileId);
            var result = await _context.Files.DeleteOneAsync(filter);
            return result.DeletedCount > 0;
        }

        public async Task<bool> DeleteFileFromNode(string userId, string fileId, Node node)
        {
            var httpClient = CreateHttpClient();
            var url = $"{node.Endpoint}/storage/delete";
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", node.Token);

            var content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(new { userId, filename = fileId }), Encoding.UTF8, "application/json");
            var response = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Delete, url) { Content = content });

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateFileRecord(FileRecord fileRecord)
        {
            var filter = Builders<FileRecord>.Filter.Eq(f => f.Id, fileRecord.Id);
            var result = await _context.Files.ReplaceOneAsync(filter, fileRecord);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> ShareFileWithUser(string fileId, string userId)
        {
            var filter = Builders<FileRecord>.Filter.Eq(f => f.Id, fileId);
            var update = Builders<FileRecord>.Update.AddToSet(f => f.SharedWith, userId);
            var result = await _context.Files.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        public async Task<IEnumerable<FileRecord>> GetFilesSharedWithUser(string userId)
        {
            var filter = Builders<FileRecord>.Filter.AnyEq(f => f.SharedWith, userId);
            return await _context.Files.Find(filter).ToListAsync();
        }

        public async Task<bool> RevokeFileShare(string fileId, string userId)
        {
            var filter = Builders<FileRecord>.Filter.Eq(f => f.Id, fileId);
            var update = Builders<FileRecord>.Update.Pull(f => f.SharedWith, userId);
            var result = await _context.Files.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> RenameFile(string fileId, string newFilename)
        {
            var filter = Builders<FileRecord>.Filter.Eq(f => f.Id, fileId);
            var update = Builders<FileRecord>.Update.Set(f => f.Filename, newFilename);
            var result = await _context.Files.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

    }
}
