namespace PXResources.Shared.Connection;


public class Credentials
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required TokenInfo TokenInfo { get; init; }
}