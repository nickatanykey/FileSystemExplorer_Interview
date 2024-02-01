using System.Web.Http;
using FileBrowser.BLL;
using System;
using System.IO;
using System.Net.Http;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Linq;

public class FileController : ApiController
{
    private IFileService fileService;

    public FileController(IFileService fileService)
    {
        this.fileService = fileService;
    }

    [HttpGet]
    [Route("api/file/download")]
    //demonstrating routing precedence 
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

    [HttpPost]
    public async Task<IHttpActionResult> Upload(string relativePath)
    {
        try
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                return BadRequest("Unsupported media type");
            }

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);

            var file = provider.Contents.First();
            //trim quotes that some browsers might send
            var filename = file.Headers.ContentDisposition.FileName.Trim('"');

            using (Stream stream = await file.ReadAsStreamAsync())
            {
                await fileService.UploadFileAsync(relativePath, stream, filename);
            }
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch //general exception
        {
            //todo: log here
            return InternalServerError();
        }

        return Ok();
    }

    [HttpDelete]
    [Route("api/file")]
    public IHttpActionResult DeleteFile(string fullFilePath)
    {
        try
        {
            string fileName = Path.GetFileName(fullFilePath);
            string relativePath = Path.GetDirectoryName(fullFilePath).Replace("\\", "/");
            fileService.DeleteFile(relativePath, fileName);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch
        {
            return InternalServerError();
        }

        return Ok();
    }

    [HttpDelete]
    [Route("api/directory")]
    public IHttpActionResult DeleteDirectory(string relativePath)
    {
        try
        {
            fileService.DeleteDirectory(relativePath);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch
        {
            return InternalServerError();
        }
        return Ok();
    }
}