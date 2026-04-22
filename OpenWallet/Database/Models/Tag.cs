namespace OpenWallet.Database.Models;

public class Tag
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<RecordTag> RecordTags { get; set; } = [];
}
