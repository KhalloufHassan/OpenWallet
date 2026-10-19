namespace OpenWallet.Shared.DTOs;

public class StoreDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class CreateStoreDto
{
    public string Name { get; set; } = string.Empty;
}
