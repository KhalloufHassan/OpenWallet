using OpenWallet.Shared.Models;

namespace OpenWallet.Shared.DTOs;

public class TemplateDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int AccountId { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string CategoryIcon { get; set; } = string.Empty;
    public RecordType Type { get; set; }
    public decimal Amount { get; set; }
    public string Notes { get; set; } = string.Empty;
    public List<TagDto> Tags { get; set; } = [];
}

public class CreateTemplateDto
{
    public string Name { get; set; } = string.Empty;
    public int AccountId { get; set; }
    public int CategoryId { get; set; }
    public RecordType Type { get; set; }
    public decimal Amount { get; set; }
    public string Notes { get; set; } = string.Empty;
    public List<int> TagIds { get; set; } = [];
}

public class UpdateTemplateDto
{
    public string Name { get; set; } = string.Empty;
    public int AccountId { get; set; }
    public int CategoryId { get; set; }
    public RecordType Type { get; set; }
    public decimal Amount { get; set; }
    public string Notes { get; set; } = string.Empty;
    public List<int> TagIds { get; set; } = [];
}
