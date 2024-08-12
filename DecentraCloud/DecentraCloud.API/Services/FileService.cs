using DecentraCloud.API.DTOs;
using DecentraCloud.API.Helpers;
using DecentraCloud.API.Interfaces.RepositoryInterfaces;
using DecentraCloud.API.Interfaces.ServiceInterfaces;
using DecentraCloud.API.Models;
using HeyRed.Mime;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DecentraCloud.API.Services
{
    public class FileService : IFileService
    {
        private readonly INodeService _nodeService;
        private readonly IFileRepository _fileRepository;
        private readonly IUserRepository _userRepository;
        private readonly EncryptionHelper _encryptionHelper;

        public FileService(INodeService nodeService, IFileRepository fileRepository, IUserRepository userRepository, EncryptionHelper encryptionHelper)
        {
            _nodeService = nodeService;
            _fileRepository = fileRepository;
            _userRepository = userRepository;
            _encryptionHelper = encryptionHelper;
        }

        private string GetMimeType(string filename)
        {
            return MimeTypeHelper.GetMimeType(filename);
        }

        public async Task<FileOperationResult> UploadFile(FileUploadDto fileUploadDto)
        {
            // Ping the node before proceeding with upload
            var node = await _nodeService.GetNodeById(fileUploadDto.NodeId);
            if (node == null || !await _nodeService.EnsureNodeIsOnline(node.Id))
            {
                return new FileOperationResult { Success = false, Message = "Node is offline or unavailable." };
            }

            // Encrypt the file data before upload
            fileUploadDto.Data = _encryptionHelper.Encrypt(fileUploadDto.Data);

            // Add file record to the database first to generate the file ID
            var fileRecord = new FileRecord
            {
                UserId = fileUploadDto.UserId,
                Filename = fileUploadDto.Filename,
                NodeId = fileUploadDto.NodeId,
                Size = fileUploadDto.Data.Length,
                MimeType = MimeTypeHelper.GetMimeType(fileUploadDto.Filename),
                DateAdded = DateTime.UtcNow
            };
            await _fileRepository.AddFileRecord(fileRecord);

            // Use the generated file ID as the filename for the storage node
            var fileId = fileRecord.Id;

            var result = await _fileRepository.UploadFileToNode(new FileUploadDto
            {
                UserId = fileUploadDto.UserId,
                Filename = fileId,
                Data = fileUploadDto.Data,
                NodeId = fileUploadDto.NodeId
            }, node);

            if (result)
            {
                await _userRepository.UpdateUserStorageUsage(fileUploadDto.UserId, fileUploadDto.Data.Length);
                await _nodeService.UpdateNodeUptime(node.Id);
                return new FileOperationResult { Success = true, Message = "File uploaded successfully" };
            }
            else
            {
                await _fileRepository.DeleteFileRecord(fileUploadDto.UserId, fileRecord.Filename);
                return new FileOperationResult { Success = false, Message = "File upload failed and record deleted." };
            }
        }

        public async Task<bool> DeleteFile(string fileId, string userId)
        {
            var fileRecord = await _fileRepository.GetFileRecordById(fileId);
            if (fileRecord == null || fileRecord.UserId != userId)
            {
                return false;
            }

            // Ping the node before attempting deletion
            var node = await _nodeService.GetNodeById(fileRecord.NodeId);
            if (node == null || !await _nodeService.EnsureNodeIsOnline(node.Id))
            {
                return false;
            }

            // Attempt to delete the file from the storage node
            var deleteSuccess = await _fileRepository.DeleteFileFromNode(userId, fileId, node);
            if (!deleteSuccess)
            {
                return false;
            }

            // Delete file record from database
            var dbDeleteSuccess = await _fileRepository.DeleteFileRecord(userId, fileRecord.Filename);
            if (!dbDeleteSuccess)
            {
                return false;
            }

            // Update user's used storage
            var user = await _userRepository.GetUserById(userId);
            if (user == null)
            {
                return false;
            }

            user.UsedStorage -= fileRecord.Size;
            await _userRepository.UpdateUser(user);

            return true;
        }

        public async Task<IEnumerable<FileRecord>> GetAllFiles(string userId)
        {
            return await _fileRepository.GetFilesByUserId(userId);
        }

        public async Task<byte[]> ViewFile(string userId, string fileId)
        {
            var fileRecord = await _fileRepository.GetFileRecordById(fileId);
            if (fileRecord == null || fileRecord.UserId != userId)
            {
                return null;
            }

            var node = await _nodeService.GetNodeById(fileRecord.NodeId);
            if (node == null || !node.IsOnline || string.IsNullOrEmpty(node.Endpoint))
            {
                return null;
            }

            var encryptedContent = await _fileRepository.ViewFileOnNode(userId, fileId, node);
            return _encryptionHelper.Decrypt(encryptedContent);
        }

        public async Task<FileContentDto> DownloadFile(string userId, string fileId)
        {
            var fileRecord = await _fileRepository.GetFileRecordById(fileId);
            if (fileRecord == null || fileRecord.UserId != userId)
            {
                return null;
            }

            var node = await _nodeService.GetNodeById(fileRecord.NodeId);
            if (node == null || !node.IsOnline || string.IsNullOrEmpty(node.Endpoint))
            {
                return null;
            }

            var encryptedContent = await _fileRepository.DownloadFileFromNode(userId, fileId, node);
            var decryptedContent = _encryptionHelper.Decrypt(encryptedContent);
            return new FileContentDto
            {
                Filename = fileRecord.Filename,
                Content = decryptedContent
            };
        }

        public async Task<FileRecord> GetFile(string fileId)
        {
            return await _fileRepository.GetFileRecordById(fileId);
        }

        public async Task<IEnumerable<FileRecord>> SearchFiles(string userId, string query)
        {
            return await _fileRepository.SearchFileRecords(userId, query);
        }

        public async Task<FileRecordDto> GetFileDetails(string fileId, string userId)
        {
            var fileRecord = await _fileRepository.GetFileRecordById(fileId);
            if (fileRecord == null || (fileRecord.UserId != userId && !fileRecord.SharedWith.Contains(userId)))
            {
                return null;
            }

            var sharedWithEmails = new List<string>();
            foreach (var sharedUserId in fileRecord.SharedWith)
            {
                var user = await _userRepository.GetUserById(sharedUserId);
                if (user != null)
                {
                    sharedWithEmails.Add(user.Email);
                }
            }

            return new FileRecordDto
            {
                Id = fileRecord.Id,
                Filename = fileRecord.Filename,
                Size = fileRecord.Size,
                DateAdded = fileRecord.DateAdded,
                SharedWithEmails = sharedWithEmails
            };
        }

        public async Task<bool> IsFileOwner(string userId, string fileId)
        {
            var fileRecord = await _fileRepository.GetFileRecordById(fileId);
            return fileRecord != null && fileRecord.UserId == userId;
        }

        public async Task<bool> ShareFile(string fileId, string email)
        {
            var user = await _userRepository.GetUserByEmail(email);
            if (user == null)
            {
                return false;
            }

            return await _fileRepository.ShareFileWithUser(fileId, user.Id);
        }

        public async Task<IEnumerable<FileRecord>> GetFilesSharedWithUser(string userId)
        {
            return await _fileRepository.GetFilesSharedWithUser(userId);
        }

        public async Task<bool> RevokeShare(string fileId, string userEmail)
        {
            var fileRecord = await _fileRepository.GetFileRecordById(fileId);
            if (fileRecord == null)
            {
                return false; // File not found
            }

            var user = await _userRepository.GetUserByEmail(userEmail);
            if (user == null)
            {
                return false; // User with the provided email not found
            }

            if (!fileRecord.SharedWith.Contains(user.Id))
            {
                return false; // File is not shared with the user
            }

            return await _fileRepository.RevokeFileShare(fileId, user.Id);
        }

        public async Task<FileRecord> GetFileRecordById(string fileId)
        {
            return await _fileRepository.GetFileRecordById(fileId);
        }

        public async Task<bool> RenameFile(string userId, string fileId, string newFilename)
        {
            var fileRecord = await _fileRepository.GetFileRecordById(fileId);

            if (fileRecord == null || fileRecord.UserId != userId)
            {
                return false;
            }

            return await _fileRepository.RenameFile(fileId, newFilename);
        }

    }
}
