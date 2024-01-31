using System.Collections.Generic;

namespace FileBrowser.Domain
{
    public class DirectoryModel
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public List<FileModel> Files { get; set; }
        public List<DirectoryModel> Subdirectories { get; set; }

        public DirectoryModel()
        {
            Files = new List<FileModel>();
            Subdirectories = new List<DirectoryModel>();
        }
    }
}