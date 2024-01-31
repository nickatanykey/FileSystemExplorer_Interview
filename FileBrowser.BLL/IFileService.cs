using FileBrowser.Domain;
using System.IO;

namespace FileBrowser.BLL
{
    public interface IFileService
    {
        bool IsSecureDirectoryQuery(string fullPath);
        DirectoryModel GetDirectoryContents(string relativePath);
        Stream GetFile(string filePath);
        bool FileExists(string filePath);
        string GetMimeType(string filePath);
        string GetFileName(string filePath);
    }
}