namespace Ice.ViewModels;

public class ErrorViewModel
{
    public string? RequestId { get; set; }

    public int StatusCode { get; set; }

    public string? Message { get; set; }

    public string? DetailedMessage { get; set; }

    public string? Suggestion { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}