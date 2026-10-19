namespace OpenWallet.Shared.DTOs;

public class TagDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class CreateTagDto
{
    public string Name { get; set; } = string.Empty;
}
