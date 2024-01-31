using FileBrowser.Domain;

namespace FileBrowser.BLL
{
    public interface IFileBrowserService
    {
        bool IsSecureDirectoryQuery(string fullPath);
        DirectoryModel GetDirectoryContents(string relativePath);
        string GetHomeDirectoryPath();
    }
}