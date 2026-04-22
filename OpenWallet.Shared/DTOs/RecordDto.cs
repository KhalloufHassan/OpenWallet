using OpenWallet.Shared.Models;

namespace OpenWallet.Shared.DTOs;

public class RecordDto
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public string AccountCurrency { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string CategoryIcon { get; set; } = string.Empty;
    public string CategoryColor { get; set; } = string.Empty;
    public int? StoreId { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public RecordType Type { get; set; }
    public decimal Amount { get; set; }
    public DateTime DateTime { get; set; }
    public string Notes { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public int? LinkedTransferRecordId { get; set; }
    public List<TagDto> Tags { get; set; } = [];
    public List<AttachmentDto> Attachments { get; set; } = [];
}

public class CreateRecordDto
{
    public int AccountId { get; set; }
    public int CategoryId { get; set; }
    public int? StoreId { get; set; }
    public RecordType Type { get; set; }
    public decimal Amount { get; set; }
    public DateTime DateTime { get; set; }
    public string Notes { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public List<int> TagIds { get; set; } = [];
}

public class CreateTransferDto
{
    public int FromAccountId { get; set; }
    public int ToAccountId { get; set; }
    public int CategoryId { get; set; }
    public decimal Amount { get; set; }
    public DateTime DateTime { get; set; }
    public string Notes { get; set; } = string.Empty;
    public List<int> TagIds { get; set; } = [];
}

public class UpdateRecordDto
{
    public int AccountId { get; set; }
    public int CategoryId { get; set; }
    public int? StoreId { get; set; }
    public RecordType Type { get; set; }
    public decimal Amount { get; set; }
    public DateTime DateTime { get; set; }
    public string Notes { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public List<int> TagIds { get; set; } = [];
}

public class RecordFilterDto
{
    public int? AccountId { get; set; }
    public int? CategoryId { get; set; }
    public int? StoreId { get; set; }
    public RecordType? Type { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public List<int> TagIds { get; set; } = [];
    public string Search { get; set; } = string.Empty;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public class AttachmentDto
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
}
