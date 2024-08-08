using DecentraCloud.API.DTOs;
using DecentraCloud.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DecentraCloud.API.Interfaces.RepositoryInterfaces
{
    public interface IFileRepository
    {
        Task AddFileRecord(FileRecord fileRecord);
        Task<FileRecord> GetFileRecord(string userId, string filename);
        Task<FileRecord> GetFileRecordById(string fileId);
        Task<IEnumerable<FileRecord>> GetFilesByUserId(string userId);
        Task<bool> DeleteFileRecord(string userId, string filename);
        Task<bool> ShareFileWithUser(string fileId, string userId);
        Task<IEnumerable<FileRecord>> GetFilesSharedWithUser(string userId);
        Task<bool> UploadFileToNode(FileUploadDto fileUploadDto, Node node);
        Task<byte[]> DownloadFileFromNode(string userId, string fileId, Node node);
        Task<byte[]> ViewFileOnNode(string userId, string fileId, Node node);
        Task<IEnumerable<FileRecord>> SearchFileRecords(string userId, string query);
        Task<bool> DeleteFileFromNode(string userId, string fileId, Node node);
        Task<bool> UpdateFileRecord(FileRecord fileRecord);
        Task<bool> RevokeFileShare(string fileId, string userId);
        Task<bool> RenameFile(string fileId, string newFilename);


    }
}
