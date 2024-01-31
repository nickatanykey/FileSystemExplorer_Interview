using NUnit.Framework;
using System.IO.Abstractions;
using System;
using FileBrowser.BLL;
using System.IO;

[TestFixture]
public class FileBrowserServiceTests
{
    private FileService fileBrowserService;

    [SetUp]
    public void Setup()
    {  
        fileBrowserService = new FileService(
            TestContext.CurrentContext.TestDirectory, 
            new FileSystem()
        );
    }

    [Test]
    public void GetDirectoryContents_WhenPathIsOutsideHomeDirectory_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        string outsidePath = @"C:\";

        // Act & Assert
        var ex = Assert.Throws<UnauthorizedAccessException>(() =>
            fileBrowserService.GetDirectoryContents(outsidePath));

        Assert.That(ex.Message, Is.EqualTo("Access to the path is denied."));
    }


    [Test]
    public void GetDirectoryContents_WhenPathIsInHomeDirectory_ThrowsNoException()
    {
        // Arrange
        string correctPath = TestContext.CurrentContext.TestDirectory + @"\TestFolder\TestSubFolder";

        // Act & Assert
        bool isExceptionThrownExpected = false;
        bool isExceptionThrown = false;

        try
        {
            var directoryContents = fileBrowserService.GetDirectoryContents(correctPath);
        }
        catch
        {
            isExceptionThrown = true;
        }

        Assert.That(isExceptionThrownExpected, Is.EqualTo(isExceptionThrown));  
    }
}