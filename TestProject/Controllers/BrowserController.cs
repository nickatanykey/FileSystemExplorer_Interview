using System.Web.Http;
using FileBrowser.BLL;
using System;
using FileBrowser.Domain;

public class BrowserController : ApiController
{
    private readonly IFileService fileService;

    public BrowserController(IFileService fileService)
    {
        this.fileService = fileService;
    }

    [HttpGet]
    [Route("api/browser/isappsetup")]
    public IHttpActionResult IsAppSetUp()
    {
        string homePath = this.fileService.GetHomeDirectoryPath();

        bool isApplicationSetUp = !string.IsNullOrEmpty(homePath);

        return Ok(new { isApplicationSetUp });
    }   

    [HttpGet]
    [Route("api/browser/getcontents")]
    public IHttpActionResult GetDirectoryContents(string path = "")
    {
        try
        {
            var directoryContents = fileService.GetDirectoryContents(path ?? string.Empty);

            if (directoryContents == null)
            { 
                return NotFound();
            }   

            return Ok(directoryContents);
        }
        catch (UnauthorizedAccessException)
        {
            //add logging here for unauthorized access
            return Unauthorized();
        }
        catch (Exception ex) 
        {
            //add logging here for exception
            return InternalServerError();
        }
    }

    [HttpGet]
    [Route("api/browser/search")]
    public IHttpActionResult Search(string searchText)
    {
        SearchResults searchResults = fileService.Search(searchText);

        return Ok(searchResults);
    }
}
