using DecentraCloud.API.DTOs;
using DecentraCloud.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DecentraCloud.API.Interfaces.ServiceInterfaces
{
    public interface IFileService
    {
        Task<FileOperationResult> UploadFile(FileUploadDto fileUploadDto);
        Task<IEnumerable<FileRecord>> GetAllFiles(string userId);
        Task<byte[]> ViewFile(string userId, string fileId);
        Task<FileContentDto> DownloadFile(string userId, string fileId);
        Task<FileRecord> GetFile(string fileId);
        Task<IEnumerable<FileRecord>> SearchFiles(string userId, string query);
        Task<bool> DeleteFile(string userId, string fileId);
        Task<bool> ShareFile(string fileId, string emailToShareWith);
        Task<IEnumerable<FileRecord>> GetFilesSharedWithUser(string userId);
        Task<FileRecordDto> GetFileDetails(string fileId, string userId);
        Task<bool> IsFileOwner(string userId, string fileId);
        Task<bool> RevokeShare(string fileId, string userEmail);
        Task<FileRecord> GetFileRecordById(string fileId);
        Task<bool> RenameFile(string userId, string fileId, string newFilename);

    }
}
