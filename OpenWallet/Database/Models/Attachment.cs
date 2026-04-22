namespace OpenWallet.Database.Models;

public class Attachment
{
    public int Id { get; set; }
    public int RecordId { get; set; }
    public Record Record { get; set; } = default!;
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
}
