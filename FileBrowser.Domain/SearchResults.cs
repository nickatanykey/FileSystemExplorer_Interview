using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileBrowser.Domain
{
    public class SearchResults
    {
        public SearchResult[] Directories { get; set; }
        public SearchResult[] Files { get; set; }
    }
}
