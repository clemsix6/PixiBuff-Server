namespace PXResources.Shared.Connection;


public class TokenInfo
{
    public required string Token { get; init; }
    public required DateTime Expiration { get; init; }
}