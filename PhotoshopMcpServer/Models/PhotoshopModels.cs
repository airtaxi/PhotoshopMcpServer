namespace PhotoshopMcpServer.Models;

// Represents the result of a Photoshop JavaScript execution.
public record PhotoshopScriptResult(
    bool Success,
    string Result,
    string ErrorMessage
);

// Represents information about an open Photoshop document.
public record PhotoshopDocumentInfo(
    string Name,
    string FilePath,
    int Width,
    int Height,
    string ColorMode,
    double Resolution
);

// Represents the result of a Photoshop COM operation.
public record PhotoshopOperationResult(
    bool Success,
    string Message,
    string Data
);
