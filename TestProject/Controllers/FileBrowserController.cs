using System.Web.Http;
using System.Web.Configuration;
using FileBrowser.BLL;
using System;

public class FileBrowserController : ApiController
{
    private readonly string homeDirectory;

    private readonly IFileBrowserService fileBrowserService;

    public FileBrowserController(IFileBrowserService fileBrowserService)
    {
        this.fileBrowserService = fileBrowserService;
    }

    [HttpGet]
    [Route("api/filebrowser/getcontents")]
    public IHttpActionResult GetDirectoryContents(string path = "")
    {
        try
        {
            string homePath = WebConfigurationManager.AppSettings["HomeDirectoryPath"];

            var directoryContents = fileBrowserService.GetDirectoryContents(path);

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
