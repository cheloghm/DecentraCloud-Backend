﻿using DecentraCloud.API.DTOs;
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
            var node = await _nodeService.GetRandomOnlineNode();
            if (node == null || !await _nodeService.EnsureNodeIsOnline(node.Id))
            {
                return new FileOperationResult { Success = false, Message = "No available nodes or node is offline." };
            }

            if (node.AllocatedFileStorage.AvailableStorage < fileUploadDto.Data.Length)
            {
                return new FileOperationResult { Success = false, Message = "Not enough available storage on node." };
            }

            fileUploadDto.Data = _encryptionHelper.Encrypt(fileUploadDto.Data);

            var fileRecord = new FileRecord
            {
                UserId = fileUploadDto.UserId,
                Filename = fileUploadDto.Filename,
                NodeId = node.Id,
                Size = fileUploadDto.Data.Length,
                MimeType = MimeTypeHelper.GetMimeType(fileUploadDto.Filename),
                DateAdded = DateTime.UtcNow
            };
            await _fileRepository.AddFileRecord(fileRecord);

            var fileId = fileRecord.Id;

            var result = await _fileRepository.UploadFileToNode(new FileUploadDto
            {
                UserId = fileUploadDto.UserId,
                Filename = fileId,
                Data = fileUploadDto.Data,
                NodeId = node.Id
            }, node);

            if (result)
            {
                // Update user storage
                await _userRepository.UpdateUserStorageUsage(fileUploadDto.UserId, fileUploadDto.Data.Length);

                // Update node storage
                node.AllocatedFileStorage.UsedStorage += fileUploadDto.Data.Length;
                node.AllocatedFileStorage.AvailableStorage -= fileUploadDto.Data.Length;

                // The StorageStats will automatically update based on the above operations
                await _nodeService.UpdateNode(node);

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

            var node = await _nodeService.GetNodeById(fileRecord.NodeId);
            if (node == null || !await _nodeService.EnsureNodeIsOnline(node.Id))
            {
                return false;
            }

            var deleteSuccess = await _fileRepository.DeleteFileFromNode(userId, fileId, node);
            if (!deleteSuccess)
            {
                return false;
            }

            var dbDeleteSuccess = await _fileRepository.DeleteFileRecord(userId, fileRecord.Filename);
            if (!dbDeleteSuccess)
            {
                return false;
            }

            // Update user storage
            var user = await _userRepository.GetUserById(userId);
            if (user == null)
            {
                return false;
            }

            user.UsedStorage -= fileRecord.Size;
            await _userRepository.UpdateUser(user);

            // Update node storage
            node.AllocatedFileStorage.UsedStorage -= fileRecord.Size;
            node.AllocatedFileStorage.AvailableStorage += fileRecord.Size;

            await _nodeService.UpdateNode(node);

            return true;
        }

        public async Task<IEnumerable<FileRecord>> GetAllFiles(string userId, int pageNumber, int pageSize)
        {
            var files = await _fileRepository.GetFilesByUserId(userId);

            // Reverse the order for LIFO and apply pagination
            var paginatedFiles = files.OrderByDescending(f => f.DateAdded)
                                      .Skip((pageNumber - 1) * pageSize)
                                      .Take(pageSize)
                                      .ToList();

            foreach (var file in paginatedFiles)
            {
                var node = await _nodeService.GetNodeById(file.NodeId);
                if (node == null || !await _nodeService.EnsureNodeIsOnline(node.Id))
                {
                    continue;
                }
            }

            return paginatedFiles;
        }

        public async Task<byte[]> ViewFile(string userId, string fileId)
        {
            var fileRecord = await _fileRepository.GetFileRecordById(fileId);
            if (fileRecord == null || fileRecord.UserId != userId)
            {
                return null;
            }

            var node = await _nodeService.GetNodeById(fileRecord.NodeId);
            if (node == null || !await _nodeService.EnsureNodeIsOnline(node.Id))
            {
                return null;
            }

            if (!node.IsOnline)
            {
                // If the node was previously offline but now online, update its status
                node.IsOnline = true;
                node.Uptime.Add(DateTime.UtcNow); // Record uptime without overwriting existing values
                await _nodeService.UpdateNode(node);
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
            if (node == null || !await _nodeService.EnsureNodeIsOnline(node.Id))
            {
                return null;
            }

            if (!node.IsOnline)
            {
                // If the node was previously offline but now online, update its status
                node.IsOnline = true;
                node.Uptime.Add(DateTime.UtcNow); // Record uptime without overwriting existing values
                await _nodeService.UpdateNode(node);
            }

            var encryptedContent = await _fileRepository.DownloadFileFromNode(userId, fileId, node);
            var decryptedContent = _encryptionHelper.Decrypt(encryptedContent);
            return new FileContentDto
            {
                Filename = fileRecord.Filename,
                Content = decryptedContent
            };
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

        public async Task<IEnumerable<FileRecord>> GetFilesSharedWithUser(string userId, int pageNumber, int pageSize)
        {
            var files = await _fileRepository.GetFilesSharedWithUser(userId);

            // Reverse the order for LIFO and apply pagination
            return files.OrderByDescending(f => f.DateAdded)
                        .Skip((pageNumber - 1) * pageSize)
                        .Take(pageSize)
                        .ToList();
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

        public async Task<FileRecord> GetFile(string fileId)
        {
            return await _fileRepository.GetFileRecordById(fileId);
        }

    }
}
