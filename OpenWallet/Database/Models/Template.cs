using OpenWallet.Shared.Models;

namespace OpenWallet.Database.Models;

public class Template
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int AccountId { get; set; }
    public Account Account { get; set; } = default!;
    public int CategoryId { get; set; }
    public Category Category { get; set; } = default!;
    public RecordType Type { get; set; }
    public decimal Amount { get; set; }
    public string Notes { get; set; } = string.Empty;
    public List<TemplateTag> TemplateTags { get; set; } = [];
}
