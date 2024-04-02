namespace PXResources.Source.NetworkShared.Client;


public class TokenInfoDTO
{
    public required string Token { get; init; }
    public required DateTime Expiration { get; init; }
}