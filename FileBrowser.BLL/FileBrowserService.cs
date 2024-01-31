using FileBrowser.Domain;
using System.IO;
using System;
using System.IO.Abstractions; // Make sure to include this
using System.Linq;

namespace FileBrowser.BLL
{
    public class FileBrowserService : IFileBrowserService
    {
        private readonly IFileSystem fileSystem;
        private readonly string homeDirectory;

        public FileBrowserService(string homeDirectory, IFileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
            this.homeDirectory = homeDirectory;
        }

        private string GetSafeFullPath(string relativePath)
        {
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
                                     Path = dirInfo.FullName
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
                            Path = fileInfo.FullName
                        };

            directoryContents.Files.AddRange(files);
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
                Path = directoryInfo.FullName
            };

            AddFiles(directoryContents, fullPath);
            AddSubdirectories(directoryContents, fullPath);

            return directoryContents;
        }
    }
}
