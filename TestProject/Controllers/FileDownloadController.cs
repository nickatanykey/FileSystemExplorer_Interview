using System.Web.Http;
using FileBrowser.BLL;
using System;
using System.IO;
using System.Net.Http;
using System.Net;
using System.Net.Http.Headers;

public class FileDownloadController : ApiController
{
    private IFileService fileService;

    public FileDownloadController(IFileService fileService)
    {
        this.fileService = fileService;
    }

    [HttpGet]
    [Route("api/file/download")]
    public IHttpActionResult GetFile(string filePath)
    {
        if (!fileService.FileExists(filePath))
        {
            return NotFound();
        }

        try
        {
            Stream stream = fileService.GetFile(filePath);
            string fileName = fileService.GetFileName(filePath);
            string contentType = fileService.GetMimeType(filePath);

            var result = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(stream)
            };

            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = fileName
            };
            result.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);

            var response = ResponseMessage(result);
            
            return response;
        }
        catch (Exception ex)
        {
            return InternalServerError(ex);
        }
    }
}