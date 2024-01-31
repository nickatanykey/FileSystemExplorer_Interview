using NUnit.Framework;
using Moq;
using System.Web.Http.Results;
using FileBrowser.BLL;
using FileBrowser.Domain;
using System.IO.Abstractions;

[TestFixture]
public class FileBrowserControllerTests
{
    private Mock<IFileSystem> mockFileSystem;

    private Mock<FileService> mockService;
    private FileBrowserController controller;

    [SetUp]
    public void Setup()
    {
        mockFileSystem = new Mock<IFileSystem>();
        mockService = new Mock<FileService>("valid", mockFileSystem.Object);
        controller = new FileBrowserController(mockService.Object);
    }

    [Test]
    public void GetDirectoryContents_ReturnsOk_WithValidPath()
    {
        // Arrange
        mockService.Setup(s => s.GetDirectoryContents(It.IsAny<string>()))
                   .Returns(new DirectoryModel()); // Return a valid DirectoryModel

        // Act
        var result = controller.GetDirectoryContents("valid/path");

        // Assert
        Assert.IsInstanceOf<OkNegotiatedContentResult<DirectoryModel>>(result);
    }

    [Test]
    public void GetDirectoryContents_ReturnsNotFound_WhenServiceReturnsNull()
    {
        // Arrange
        mockService.Setup(s => s.GetDirectoryContents(It.IsAny<string>()))
                   .Returns((DirectoryModel)null); // Return null

        // Act
        var result = controller.GetDirectoryContents("nonexistent/path");

        // Assert
        Assert.IsInstanceOf<NotFoundResult>(result);
    }

    [Test]
    public void GetDirectoryContents_ReturnsInternalServerError_OnException()
    {
        // Arrange
        mockService.Setup(s => s.GetDirectoryContents(It.IsAny<string>()))
                   .Throws(new System.Exception()); // Throw an exception

        // Act
        var result = controller.GetDirectoryContents("exception/path");

        // Assert
        Assert.IsInstanceOf<InternalServerErrorResult>(result);
    }
}