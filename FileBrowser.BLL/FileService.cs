using FileBrowser.Domain;
using System.IO;
using System;
using System.IO.Abstractions; // Make sure to include this
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Collections.Generic;
using System.IO.Pipes;

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
                            Path = SanitizeOutputPath(fileInfo.FullName),
                            Length = fileInfo.Length
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

        public string GetHomeDirectoryPath()
        {
            return homeDirectory;
        }

        public static string SanitizeFileName(string fileName)
        {
            var invalidChars = new HashSet<char>(Path.GetInvalidFileNameChars());

            // Replace invalid characters with an underscore (or remove them)
            var cleanName = new string(fileName
                .Select(ch => invalidChars.Contains(ch) ? '_' : ch)
                .ToArray())
                .Trim(); // Trim leading and trailing whitespace

            if (Constants.ReservedWindowsFilenames.Contains(cleanName.ToUpperInvariant()))
            {
                cleanName = "_" + cleanName;
            }

            return cleanName;
        }


        public async Task UploadFileAsync(string relativePath, Stream fileStream, string fileName)
        {
            string filename = SanitizeFileName(fileName);

            string safeDirPath = GetSafeFullPath(relativePath);
            if (!fileSystem.Directory.Exists(safeDirPath))
                return;

            string filePath = Path.Combine(safeDirPath, filename);

            if (!File.Exists(filePath))
            {
                // Ensure the directory exists
                var directory = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Write the stream to the file
                using (var file = new FileStream(filePath, FileMode.Create))
                {
                    await fileStream.CopyToAsync(file);
                }
            }
        }

        public void DeleteDirectory(string relativePath)
        {
            string safeDirPath = GetSafeFullPath(relativePath);
            if (fileSystem.Directory.Exists(safeDirPath) 
                && !fileSystem.Directory.GetFiles(safeDirPath).Any())
            {
                fileSystem.Directory.Delete(safeDirPath);
            }
        }

        public void DeleteFile(string relativePath, string fileName)
        {
            string filename = SanitizeFileName(fileName);
            string safeDirPath = GetSafeFullPath(relativePath);

            if (fileSystem.Directory.Exists(safeDirPath))
            {
                string fullPath = Path.Combine(safeDirPath, filename);
                if (fileSystem.File.Exists(fullPath))
                {
                    fileSystem.File.Delete(fullPath);
                }
            }
        }

        //todo: in memory (redis maybe) cache to increase file system reads, especially after updating requiring OS fs re-index
        public SearchResults Search(string searchText)
        {
            var searchResults = new SearchResults();

            var files = fileSystem.Directory.EnumerateFiles(homeDirectory, searchText, SearchOption.AllDirectories);

            var directories = fileSystem.Directory.EnumerateDirectories(homeDirectory, searchText, SearchOption.AllDirectories);

            if (files != null && files.Any())
            {
                searchResults.Files = 
                    files
                        .Select(
                            file => new SearchResult 
                            { 
                                Name = SanitizeOutputPath(file), 
                                RelativePath = Path.GetDirectoryName(
                                    SanitizeOutputPath(file)
                                ).Replace("\\", "/")
                            }).ToArray();
            }

            if (directories != null && directories.Any())
            {
                searchResults.Directories = directories.Select(directory => new SearchResult { Name = SanitizeOutputPath(directory), RelativePath = SanitizeOutputPath(directory) }).ToArray();
            }

            return searchResults;
        }
    }
}
