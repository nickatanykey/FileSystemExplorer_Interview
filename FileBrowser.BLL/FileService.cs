using FileBrowser.Domain;
using System.IO;
using System;
using System.IO.Abstractions; // Make sure to include this
using System.Linq;
using System.Reflection.Emit;

namespace FileBrowser.BLL
{
    public class FileService : IFileService
    {
        private readonly IFileSystem fileSystem;
        private readonly string homeDirectory;

        public FileService(string homeDirectory, IFileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
            this.homeDirectory = homeDirectory;
        }

        private string GetSafeFullPath(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath) || relativePath == "/")
                relativePath = string.Empty;
            else if (relativePath.StartsWith("/"))
                relativePath = relativePath.Substring(1);

            // Use IFileSystem.Path to combine and normalize the path
            return fileSystem.Path.GetFullPath(fileSystem.Path.Combine(homeDirectory, relativePath));
        }

        private void AddSubdirectories(DirectoryModel directoryContents, string fullPath)
        {
            var subdirectories = from dir in fileSystem.Directory.GetDirectories(fullPath)
                                 let dirInfo = fileSystem.DirectoryInfo.New(dir)
                                 select new DirectoryModel
                                 {
                                     Name = dirInfo.Name,
                                     Path = SanitizeOutputPath(dirInfo.FullName)
                                 };

            directoryContents.Subdirectories.AddRange(subdirectories);
        }

        private void AddFiles(DirectoryModel directoryContents, string fullPath)
        {
            var files = from file in fileSystem.Directory.GetFiles(fullPath)
                        let fileInfo = fileSystem.FileInfo.New(file)
                        select new FileModel
                        {
                            Name = fileInfo.Name,
                            Path = SanitizeOutputPath(fileInfo.FullName)
                        };

            directoryContents.Files.AddRange(files);
        }

        public string SanitizeOutputPath(string outputPath)
        {
            if (string.IsNullOrEmpty(outputPath))
                return "/";

            return outputPath.Replace(homeDirectory, string.Empty).Replace("\\", "/");
        }

        public virtual bool IsSecureDirectoryQuery(string fullPath)
        {
            return fullPath.StartsWith(homeDirectory, StringComparison.OrdinalIgnoreCase);
        }

        public virtual DirectoryModel GetDirectoryContents(string relativePath)
        { 
            var fullPath = GetSafeFullPath(relativePath);

            if (!IsSecureDirectoryQuery(fullPath))
            {
                throw new UnauthorizedAccessException("Access to the path is denied.");
            }

            if (!fileSystem.Directory.Exists(fullPath))
            {
                throw new DirectoryNotFoundException("Directory not found.");
            }

            var directoryInfo = fileSystem.DirectoryInfo.New(fullPath);
            var directoryContents = new DirectoryModel
            {
                Name = directoryInfo.Name,
                Path = SanitizeOutputPath(directoryInfo.FullName)
            };

            AddFiles(directoryContents, fullPath);
            AddSubdirectories(directoryContents, fullPath);

            return directoryContents;
        }

        public Stream GetFile(string relativePath)
        {
            // Assuming filePath is an absolute path or relative to a safe root
            var fullPath = GetSafeFullPath(relativePath);
            return new FileStream(fullPath, FileMode.Open, FileAccess.Read);
        }

        public bool FileExists(string relativePath)
        {
            var fullPath = GetSafeFullPath(relativePath);
            return fileSystem.File.Exists(fullPath);
        }

        public string GetMimeType(string relativePath)
        {
            var fullPath = GetSafeFullPath(relativePath);
            var extension = fileSystem.Path.GetExtension(fullPath).ToLowerInvariant();
            switch (extension)
            {
                case ".txt": 
                    return "text/plain";
                case ".pdf": 
                    return "application/pdf";
                default: 
                    return "application/octet-stream";
            }
        }

        public string GetFileName(string relativePath)
        {
            var fullPath = GetSafeFullPath(relativePath);
            return fileSystem.FileInfo.New(fullPath).Name;
        }
    }
}
