using FluentAssertions;
using PhotoshopMcpServer.Models;
using Xunit;

namespace PhotoshopMcpServer.Tests;

// Tests for model records and data structures.
// PhotoshopService itself requires a live Photoshop COM instance,
// so COM-dependent behavior is tested via integration tests (not unit tests).
public class PhotoshopModelsTests
{
    [Fact]
    public void PhotoshopScriptResult_Success_HasCorrectProperties()
    {
        var result = new PhotoshopScriptResult(true, "output", string.Empty);

        result.Success.Should().BeTrue();
        result.Result.Should().Be("output");
        result.ErrorMessage.Should().BeEmpty();
    }

    [Fact]
    public void PhotoshopScriptResult_Failure_HasCorrectProperties()
    {
        var result = new PhotoshopScriptResult(false, string.Empty, "Script error occurred");

        result.Success.Should().BeFalse();
        result.Result.Should().BeEmpty();
        result.ErrorMessage.Should().Be("Script error occurred");
    }

    [Fact]
    public void PhotoshopDocumentInfo_HasCorrectProperties()
    {
        var documentInfo = new PhotoshopDocumentInfo(
            Name: "test.psd",
            FilePath: "C:/test.psd",
            Width: 1920,
            Height: 1080,
            ColorMode: "RGBColor",
            Resolution: 300.0
        );

        documentInfo.Name.Should().Be("test.psd");
        documentInfo.FilePath.Should().Be("C:/test.psd");
        documentInfo.Width.Should().Be(1920);
        documentInfo.Height.Should().Be(1080);
        documentInfo.ColorMode.Should().Be("RGBColor");
        documentInfo.Resolution.Should().Be(300.0);
    }

    [Fact]
    public void PhotoshopDocumentInfo_RecordEquality_WorksCorrectly()
    {
        var firstDocumentInfo = new PhotoshopDocumentInfo("doc.psd", "/path", 100, 200, "RGB", 72.0);
        var secondDocumentInfo = new PhotoshopDocumentInfo("doc.psd", "/path", 100, 200, "RGB", 72.0);

        firstDocumentInfo.Should().Be(secondDocumentInfo);
    }

    [Fact]
    public void PhotoshopOperationResult_HasCorrectProperties()
    {
        var operationResult = new PhotoshopOperationResult(true, "Success", "result data");

        operationResult.Success.Should().BeTrue();
        operationResult.Message.Should().Be("Success");
        operationResult.Data.Should().Be("result data");
    }

    [Fact]
    public void PhotoshopScriptResult_RecordEquality_WorksCorrectly()
    {
        var firstResult = new PhotoshopScriptResult(true, "data", string.Empty);
        var secondResult = new PhotoshopScriptResult(true, "data", string.Empty);

        firstResult.Should().Be(secondResult);
    }

    [Theory]
    [InlineData(true, "output", "", true)]
    [InlineData(false, "", "error", false)]
    public void PhotoshopScriptResult_SuccessFlag_CorrectlyRepresentsState(
        bool success, string resultText, string errorMessage, bool expectedSuccess)
    {
        var scriptResult = new PhotoshopScriptResult(success, resultText, errorMessage);

        scriptResult.Success.Should().Be(expectedSuccess);
    }
}
