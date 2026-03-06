using PhotoshopMcpServer.Models;

namespace PhotoshopMcpServer.Services;

// Interface for interacting with Adobe Photoshop via COM automation.
public interface IPhotoshopService
{
    // Checks whether Photoshop is currently running and accessible.
    bool IsPhotoshopRunning();

    // Launches Photoshop if it is not already running.
    void LaunchPhotoshop();

    // Executes a JavaScript string in Photoshop's scripting engine.
    // Returns the string result from Photoshop's DoJavaScript call.
    string ExecuteJavaScript(string script);

    // Executes a JavaScript string and returns the result along with metadata.
    PhotoshopScriptResult ExecuteJavaScriptWithResult(string script);

    // Gets basic info about the currently active Photoshop document.
    PhotoshopDocumentInfo GetActiveDocumentInfo();

    // Gets a list of open document names.
    IReadOnlyList<string> GetOpenDocumentNames();

    // Gets the Photoshop application version string.
    string GetPhotoshopVersion();

    // Closes the COM connection and releases resources.
    void Disconnect();
}
