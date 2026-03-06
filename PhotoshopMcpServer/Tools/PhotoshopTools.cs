using System.ComponentModel;
using ModelContextProtocol.Server;
using PhotoshopMcpServer.Models;
using PhotoshopMcpServer.Services;

namespace PhotoshopMcpServer.Tools;

// MCP tools for controlling Adobe Photoshop via COM automation.
// The primary tool is ExecuteJavaScript which allows full Photoshop scripting.
// Additional convenience tools provide structured access to common operations.
[McpServerToolType]
public class PhotoshopTools(IPhotoshopService photoshopService)
{
    [McpServerTool]
    [Description(
        "Executes arbitrary JavaScript in Adobe Photoshop's scripting engine. " +
        "This is the primary tool for all Photoshop automation. " +
        "The script has access to the full Photoshop DOM via 'app' (the Application object). " +
        "Example: 'app.activeDocument.flatten(); app.activeDocument.save();' " +
        "Returns the string result of the last evaluated expression, or empty string.")]
    public string ExecuteJavaScript(
        [Description(
            "The JavaScript code to execute in Photoshop. " +
            "Use 'app' to access the Photoshop Application object. " +
            "Use 'app.activeDocument' for the current document. " +
            "You can return values by making the last expression evaluate to a string. " +
            "Example scripts:\n" +
            "  - Get document name: 'app.activeDocument.name'\n" +
            "  - Create new doc: 'app.documents.add(800, 600, 72, \"New Doc\")'\n" +
            "  - Save as PNG: 'var opts = new ExportOptionsSaveForWeb(); opts.format = SaveDocumentType.PNG; app.activeDocument.exportDocument(new File(\"/path/out.png\"), ExportType.SAVEFORWEB, opts);'\n" +
            "  - Apply filter: 'app.activeDocument.activeLayer.applySharpen()'\n" +
            "  - Get layer list: 'var names=[]; for(var i=0;i<app.activeDocument.layers.length;i++) names.push(app.activeDocument.layers[i].name); names.join(\",\")'")]
        string script)
    {
        var result = photoshopService.ExecuteJavaScriptWithResult(script);
        if (!result.Success)
            return $"Error: {result.ErrorMessage}";
        return result.Result;
    }

    [McpServerTool]
    [Description(
        "Checks whether Adobe Photoshop is currently running and accessible via COM. " +
        "Returns 'true' if Photoshop is running, 'false' otherwise.")]
    public string IsPhotoshopRunning()
        => photoshopService.IsPhotoshopRunning().ToString().ToLowerInvariant();

    [McpServerTool]
    [Description(
        "Launches Adobe Photoshop if it is not already running, " +
        "or connects to the running instance. " +
        "Must be called before executing JavaScript if Photoshop is not open.")]
    public string LaunchPhotoshop()
    {
        try
        {
            photoshopService.LaunchPhotoshop();
            return "Photoshop launched and connected successfully.";
        }
        catch (Exception exception)
        {
            return $"Failed to launch Photoshop: {exception.Message}";
        }
    }

    [McpServerTool]
    [Description(
        "Gets the version string of the running Adobe Photoshop instance. " +
        "Useful for verifying connectivity and checking Photoshop version compatibility.")]
    public string GetPhotoshopVersion()
    {
        try
        {
            return photoshopService.GetPhotoshopVersion();
        }
        catch (Exception exception)
        {
            return $"Error: {exception.Message}";
        }
    }

    [McpServerTool]
    [Description(
        "Gets information about the currently active (frontmost) Photoshop document. " +
        "Returns document name, file path, dimensions, color mode, and resolution. " +
        "Returns an error message if no document is open.")]
    public string GetActiveDocumentInfo()
    {
        try
        {
            var documentInfo = photoshopService.GetActiveDocumentInfo();
            return
                $"Name: {documentInfo.Name}\n" +
                $"Path: {documentInfo.FilePath}\n" +
                $"Size: {documentInfo.Width} x {documentInfo.Height} px\n" +
                $"Color Mode: {documentInfo.ColorMode}\n" +
                $"Resolution: {documentInfo.Resolution} ppi";
        }
        catch (Exception exception)
        {
            return $"Error: {exception.Message}";
        }
    }

    [McpServerTool]
    [Description(
        "Gets a list of all currently open Photoshop document names. " +
        "Returns a comma-separated list of document names, or 'No documents open' if none.")]
    public string GetOpenDocuments()
    {
        try
        {
            var documentNames = photoshopService.GetOpenDocumentNames();
            if (documentNames.Count == 0)
                return "No documents open.";
            return string.Join(", ", documentNames);
        }
        catch (Exception exception)
        {
            return $"Error: {exception.Message}";
        }
    }

    [McpServerTool]
    [Description(
        "Opens an image file in Photoshop. " +
        "Provide the full absolute path to the image file. " +
        "Supports PSD, JPEG, PNG, TIFF, BMP, GIF, and other formats Photoshop can open.")]
    public string OpenDocument(
        [Description("The full absolute path to the image file to open. Example: C:\\Images\\photo.jpg")]
        string filePath)
    {
        var script = $"app.open(new File(\"{filePath.Replace("\\", "/")}\"));";
        var result = photoshopService.ExecuteJavaScriptWithResult(script);
        if (!result.Success)
            return $"Failed to open file: {result.ErrorMessage}";
        return $"Opened: {filePath}";
    }

    [McpServerTool]
    [Description(
        "Saves the currently active Photoshop document in its current format. " +
        "For PSD files, saves as PSD. " +
        "Use ExecuteJavaScript for advanced save options (Save As, Export, etc.).")]
    public string SaveActiveDocument()
    {
        var result = photoshopService.ExecuteJavaScriptWithResult("app.activeDocument.save();");
        if (!result.Success)
            return $"Failed to save document: {result.ErrorMessage}";
        return "Document saved successfully.";
    }

    [McpServerTool]
    [Description(
        "Creates a new Photoshop document with the specified dimensions. " +
        "Returns confirmation with the document name.")]
    public string CreateNewDocument(
        [Description("Width of the new document in pixels.")]
        int width,
        [Description("Height of the new document in pixels.")]
        int height,
        [Description("Resolution in pixels per inch (PPI). Common values: 72 (screen), 300 (print).")]
        double resolution,
        [Description("Name for the new document.")]
        string documentName)
    {
        var script =
            $"var doc = app.documents.add({width}, {height}, {resolution}, \"{documentName}\"); doc.name;";
        var result = photoshopService.ExecuteJavaScriptWithResult(script);
        if (!result.Success)
            return $"Failed to create document: {result.ErrorMessage}";
        return $"Created document '{documentName}' ({width}x{height} px, {resolution} ppi).";
    }

    [McpServerTool]
    [Description(
        "Exports the active Photoshop document as a PNG file to the specified path. " +
        "Uses Save for Web with PNG-24 settings (lossless, supports transparency).")]
    public string ExportAsPng(
        [Description("The full absolute path where the PNG file should be saved. Example: C:\\Output\\result.png")]
        string outputPath)
    {
        var normalizedPath = outputPath.Replace("\\", "/");
        var script =
            $"var exportOptions = new ExportOptionsSaveForWeb();" +
            $"exportOptions.format = SaveDocumentType.PNG;" +
            $"exportOptions.PNG8 = false;" +
            $"exportOptions.transparency = true;" +
            $"app.activeDocument.exportDocument(new File(\"{normalizedPath}\"), ExportType.SAVEFORWEB, exportOptions);" +
            $"\"{outputPath}\"";
        var result = photoshopService.ExecuteJavaScriptWithResult(script);
        if (!result.Success)
            return $"Failed to export PNG: {result.ErrorMessage}";
        return $"Exported PNG to: {outputPath}";
    }

    [McpServerTool]
    [Description(
        "Exports the active Photoshop document as a JPEG file to the specified path. " +
        "Quality ranges from 0 (lowest) to 100 (highest).")]
    public string ExportAsJpeg(
        [Description("The full absolute path where the JPEG file should be saved.")]
        string outputPath,
        [Description("JPEG quality from 0 (lowest) to 100 (highest). Default is 80.")]
        int quality)
    {
        var normalizedPath = outputPath.Replace("\\", "/");
        var script =
            $"var exportOptions = new ExportOptionsSaveForWeb();" +
            $"exportOptions.format = SaveDocumentType.JPEG;" +
            $"exportOptions.quality = {quality};" +
            $"app.activeDocument.exportDocument(new File(\"{normalizedPath}\"), ExportType.SAVEFORWEB, exportOptions);" +
            $"\"{outputPath}\"";
        var result = photoshopService.ExecuteJavaScriptWithResult(script);
        if (!result.Success)
            return $"Failed to export JPEG: {result.ErrorMessage}";
        return $"Exported JPEG (quality={quality}) to: {outputPath}";
    }

    [McpServerTool]
    [Description(
        "Gets a JSON-like summary of all layers in the active Photoshop document. " +
        "Returns layer names, types, and visibility for up to the top-level layers.")]
    public string GetLayerInfo()
    {
        var script =
            "(function() {" +
            "  var doc = app.activeDocument;" +
            "  var result = [];" +
            "  for (var i = 0; i < doc.layers.length; i++) {" +
            "    var layer = doc.layers[i];" +
            "    result.push(layer.name + ' [' + layer.typename + ', visible=' + layer.visible + ']');" +
            "  }" +
            "  return result.join('\\n');" +
            "})();";
        var operationResult = photoshopService.ExecuteJavaScriptWithResult(script);
        if (!operationResult.Success)
            return $"Error: {operationResult.ErrorMessage}";
        return operationResult.Result;
    }
}
