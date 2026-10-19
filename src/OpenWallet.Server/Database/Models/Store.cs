namespace OpenWallet.Database.Models;

public class Store
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<Record> Records { get; set; } = [];
}
