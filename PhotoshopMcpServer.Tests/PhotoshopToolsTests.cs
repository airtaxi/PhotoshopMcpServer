using FluentAssertions;
using Moq;
using PhotoshopMcpServer.Models;
using PhotoshopMcpServer.Services;
using PhotoshopMcpServer.Tools;
using Xunit;

namespace PhotoshopMcpServer.Tests;

public class PhotoshopToolsTests
{
    private readonly Mock<IPhotoshopService> _mockService;
    private readonly PhotoshopTools _tools;

    public PhotoshopToolsTests()
    {
        _mockService = new Mock<IPhotoshopService>();
        _tools = new PhotoshopTools(_mockService.Object);
    }

    [Fact]
    public void ExecuteJavaScript_WhenSuccessful_ReturnsResult()
    {
        var script = "app.activeDocument.name";
        _mockService.Setup(service => service.ExecuteJavaScriptWithResult(script))
            .Returns(new PhotoshopScriptResult(true, "MyDocument.psd", string.Empty));

        var result = _tools.ExecuteJavaScript(script);

        result.Should().Be("MyDocument.psd");
    }

    [Fact]
    public void ExecuteJavaScript_WhenFailed_ReturnsErrorMessage()
    {
        var script = "invalid.script()";
        _mockService.Setup(service => service.ExecuteJavaScriptWithResult(script))
            .Returns(new PhotoshopScriptResult(false, string.Empty, "Script error"));

        var result = _tools.ExecuteJavaScript(script);

        result.Should().Be("Error: Script error");
    }

    [Fact]
    public void IsPhotoshopRunning_WhenRunning_ReturnsTrue()
    {
        _mockService.Setup(service => service.IsPhotoshopRunning()).Returns(true);

        var result = _tools.IsPhotoshopRunning();

        result.Should().Be("true");
    }

    [Fact]
    public void IsPhotoshopRunning_WhenNotRunning_ReturnsFalse()
    {
        _mockService.Setup(service => service.IsPhotoshopRunning()).Returns(false);

        var result = _tools.IsPhotoshopRunning();

        result.Should().Be("false");
    }

    [Fact]
    public void LaunchPhotoshop_WhenSuccessful_ReturnsSuccessMessage()
    {
        _mockService.Setup(service => service.LaunchPhotoshop());

        var result = _tools.LaunchPhotoshop();

        result.Should().Be("Photoshop launched and connected successfully.");
    }

    [Fact]
    public void LaunchPhotoshop_WhenExceptionThrown_ReturnsFailureMessage()
    {
        _mockService.Setup(service => service.LaunchPhotoshop())
            .Throws(new InvalidOperationException("Photoshop not found"));

        var result = _tools.LaunchPhotoshop();

        result.Should().StartWith("Failed to launch Photoshop:");
        result.Should().Contain("Photoshop not found");
    }

    [Fact]
    public void GetPhotoshopVersion_WhenSuccessful_ReturnsVersion()
    {
        _mockService.Setup(service => service.GetPhotoshopVersion()).Returns("24.0.0");

        var result = _tools.GetPhotoshopVersion();

        result.Should().Be("24.0.0");
    }

    [Fact]
    public void GetPhotoshopVersion_WhenExceptionThrown_ReturnsErrorMessage()
    {
        _mockService.Setup(service => service.GetPhotoshopVersion())
            .Throws(new InvalidOperationException("COM error"));

        var result = _tools.GetPhotoshopVersion();

        result.Should().Be("Error: COM error");
    }

    [Fact]
    public void GetActiveDocumentInfo_WhenSuccessful_ReturnsFormattedInfo()
    {
        var documentInfo = new PhotoshopDocumentInfo(
            Name: "photo.psd",
            FilePath: "C:/Images/photo.psd",
            Width: 1920,
            Height: 1080,
            ColorMode: "RGBColor",
            Resolution: 72.0
        );
        _mockService.Setup(service => service.GetActiveDocumentInfo()).Returns(documentInfo);

        var result = _tools.GetActiveDocumentInfo();

        result.Should().Contain("photo.psd");
        result.Should().Contain("1920 x 1080 px");
        result.Should().Contain("RGBColor");
        result.Should().Contain("72");
    }

    [Fact]
    public void GetActiveDocumentInfo_WhenExceptionThrown_ReturnsErrorMessage()
    {
        _mockService.Setup(service => service.GetActiveDocumentInfo())
            .Throws(new InvalidOperationException("No active document"));

        var result = _tools.GetActiveDocumentInfo();

        result.Should().Be("Error: No active document");
    }

    [Fact]
    public void GetOpenDocuments_WhenDocumentsExist_ReturnsCommaSeparatedNames()
    {
        _mockService.Setup(service => service.GetOpenDocumentNames())
            .Returns(["doc1.psd", "doc2.psd", "doc3.psd"]);

        var result = _tools.GetOpenDocuments();

        result.Should().Be("doc1.psd, doc2.psd, doc3.psd");
    }

    [Fact]
    public void GetOpenDocuments_WhenNoDocumentsOpen_ReturnsNoDocumentsMessage()
    {
        _mockService.Setup(service => service.GetOpenDocumentNames())
            .Returns([]);

        var result = _tools.GetOpenDocuments();

        result.Should().Be("No documents open.");
    }

    [Fact]
    public void GetOpenDocuments_WhenExceptionThrown_ReturnsErrorMessage()
    {
        _mockService.Setup(service => service.GetOpenDocumentNames())
            .Throws(new InvalidOperationException("COM disconnected"));

        var result = _tools.GetOpenDocuments();

        result.Should().Be("Error: COM disconnected");
    }

    [Fact]
    public void OpenDocument_WhenSuccessful_ReturnsOpenedMessage()
    {
        var filePath = @"C:\Images\photo.jpg";
        _mockService.Setup(service => service.ExecuteJavaScriptWithResult(It.IsAny<string>()))
            .Returns(new PhotoshopScriptResult(true, string.Empty, string.Empty));

        var result = _tools.OpenDocument(filePath);

        result.Should().Be($"Opened: {filePath}");
    }

    [Fact]
    public void OpenDocument_WhenFailed_ReturnsFailureMessage()
    {
        var filePath = @"C:\Images\missing.jpg";
        _mockService.Setup(service => service.ExecuteJavaScriptWithResult(It.IsAny<string>()))
            .Returns(new PhotoshopScriptResult(false, string.Empty, "File not found"));

        var result = _tools.OpenDocument(filePath);

        result.Should().StartWith("Failed to open file:");
        result.Should().Contain("File not found");
    }

    [Fact]
    public void SaveActiveDocument_WhenSuccessful_ReturnsSavedMessage()
    {
        _mockService.Setup(service => service.ExecuteJavaScriptWithResult(It.IsAny<string>()))
            .Returns(new PhotoshopScriptResult(true, string.Empty, string.Empty));

        var result = _tools.SaveActiveDocument();

        result.Should().Be("Document saved successfully.");
    }

    [Fact]
    public void SaveActiveDocument_WhenFailed_ReturnsFailureMessage()
    {
        _mockService.Setup(service => service.ExecuteJavaScriptWithResult(It.IsAny<string>()))
            .Returns(new PhotoshopScriptResult(false, string.Empty, "Save failed"));

        var result = _tools.SaveActiveDocument();

        result.Should().StartWith("Failed to save document:");
    }

    [Fact]
    public void CreateNewDocument_WhenSuccessful_ReturnsFormattedMessage()
    {
        _mockService.Setup(service => service.ExecuteJavaScriptWithResult(It.IsAny<string>()))
            .Returns(new PhotoshopScriptResult(true, "MyNewDoc", string.Empty));

        var result = _tools.CreateNewDocument(800, 600, 72.0, "MyNewDoc");

        result.Should().Contain("MyNewDoc");
        result.Should().Contain("800");
        result.Should().Contain("600");
        result.Should().Contain("72");
    }

    [Fact]
    public void CreateNewDocument_WhenFailed_ReturnsFailureMessage()
    {
        _mockService.Setup(service => service.ExecuteJavaScriptWithResult(It.IsAny<string>()))
            .Returns(new PhotoshopScriptResult(false, string.Empty, "Creation error"));

        var result = _tools.CreateNewDocument(800, 600, 72.0, "TestDoc");

        result.Should().StartWith("Failed to create document:");
    }

    [Fact]
    public void ExportAsPng_WhenSuccessful_ReturnsExportedMessage()
    {
        var outputPath = @"C:\Output\result.png";
        _mockService.Setup(service => service.ExecuteJavaScriptWithResult(It.IsAny<string>()))
            .Returns(new PhotoshopScriptResult(true, outputPath, string.Empty));

        var result = _tools.ExportAsPng(outputPath);

        result.Should().Be($"Exported PNG to: {outputPath}");
    }

    [Fact]
    public void ExportAsPng_WhenFailed_ReturnsFailureMessage()
    {
        _mockService.Setup(service => service.ExecuteJavaScriptWithResult(It.IsAny<string>()))
            .Returns(new PhotoshopScriptResult(false, string.Empty, "Export error"));

        var result = _tools.ExportAsPng(@"C:\Output\result.png");

        result.Should().StartWith("Failed to export PNG:");
    }

    [Theory]
    [InlineData(80)]
    [InlineData(100)]
    [InlineData(50)]
    public void ExportAsJpeg_WhenSuccessful_ReturnsFormattedMessage(int quality)
    {
        var outputPath = @"C:\Output\result.jpg";
        _mockService.Setup(service => service.ExecuteJavaScriptWithResult(It.IsAny<string>()))
            .Returns(new PhotoshopScriptResult(true, outputPath, string.Empty));

        var result = _tools.ExportAsJpeg(outputPath, quality);

        result.Should().Contain($"quality={quality}");
        result.Should().Contain(outputPath);
    }

    [Fact]
    public void ExportAsJpeg_WhenFailed_ReturnsFailureMessage()
    {
        _mockService.Setup(service => service.ExecuteJavaScriptWithResult(It.IsAny<string>()))
            .Returns(new PhotoshopScriptResult(false, string.Empty, "JPEG export failed"));

        var result = _tools.ExportAsJpeg(@"C:\Output\result.jpg", 80);

        result.Should().StartWith("Failed to export JPEG:");
    }

    [Fact]
    public void GetLayerInfo_WhenSuccessful_ReturnsLayerList()
    {
        var layerInfo = "Background [ArtLayer, visible=true]\nLayer 1 [ArtLayer, visible=false]";
        _mockService.Setup(service => service.ExecuteJavaScriptWithResult(It.IsAny<string>()))
            .Returns(new PhotoshopScriptResult(true, layerInfo, string.Empty));

        var result = _tools.GetLayerInfo();

        result.Should().Contain("Background");
        result.Should().Contain("Layer 1");
    }

    [Fact]
    public void GetLayerInfo_WhenFailed_ReturnsErrorMessage()
    {
        _mockService.Setup(service => service.ExecuteJavaScriptWithResult(It.IsAny<string>()))
            .Returns(new PhotoshopScriptResult(false, string.Empty, "No document open"));

        var result = _tools.GetLayerInfo();

        result.Should().Be("Error: No document open");
    }
}
