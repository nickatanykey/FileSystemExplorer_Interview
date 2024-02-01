using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileBrowser.Domain
{

    public class SearchResult
    {
        public bool IsFile { get; set; }

        public string RelativePath { get; set; }

        public string Name { get; set; }
    }
}
