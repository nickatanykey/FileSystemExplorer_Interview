using System.Web.Http;
using FileBrowser.BLL;
using System;

public class FileBrowserController : ApiController
{
    private readonly IFileService fileBrowserService;

    public FileBrowserController(IFileService fileBrowserService)
    {
        this.fileBrowserService = fileBrowserService;
    }

    [HttpGet]
    [Route("api/filebrowser/getcontents")]
    public IHttpActionResult GetDirectoryContents(string path = "")
    {
        try
        {
            var directoryContents = fileBrowserService.GetDirectoryContents(path ?? string.Empty);

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
        catch
        {
            //add logging here for exception
            return InternalServerError();
        }
    }
}
