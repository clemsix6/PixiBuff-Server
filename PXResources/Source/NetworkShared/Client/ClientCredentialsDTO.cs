namespace PXResources.Source.NetworkShared.Client;


public class ClientCredentialsDTO
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required TokenInfoDTO TokenInfo { get; init; }
}