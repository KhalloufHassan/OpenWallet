namespace OpenWallet.Database.Models;

public class TemplateTag
{
    public int TemplateId { get; set; }
    public Template Template { get; set; } = default!;
    public int TagId { get; set; }
    public Tag Tag { get; set; } = default!;
}
