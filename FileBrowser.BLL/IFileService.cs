using FileBrowser.Domain;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace FileBrowser.BLL
{
    public interface IFileService
    {
        bool IsSecureDirectoryQuery(string fullPath);
        DirectoryModel GetDirectoryContents(string relativePath);
        Stream GetFile(string filePath);
        Task UploadFileAsync(string relativePath, Stream fileStream, string fileName);
        bool FileExists(string filePath);
        string GetMimeType(string filePath);
        string GetFileName(string filePath);
        string GetHomeDirectoryPath();
        void DeleteFile(string relativePath, string fileName);
        void DeleteDirectory(string relativePath);

        SearchResults Search(string searchText);
    }
}