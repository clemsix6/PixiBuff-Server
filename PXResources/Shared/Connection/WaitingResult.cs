namespace PXResources.Shared.Connection;


public class WaitingResult
{
    public required int MaxTryCount { get; set; }
    public required DateTime Expiration { get; set; }
}