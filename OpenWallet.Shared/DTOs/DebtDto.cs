namespace OpenWallet.Shared.DTOs;

public class DebtDto
{
    public int Id { get; set; }
    public string PartyName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public bool IsLending { get; set; }
    public string Notes { get; set; } = string.Empty;
    public bool IsResolved { get; set; }
    public DateTime CreatedAt { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal AmountRemaining { get; set; }
    public List<RecordDto> RelatedRecords { get; set; } = [];
}

public class CreateDebtDto
{
    public string PartyName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public bool IsLending { get; set; }
    public string Notes { get; set; } = string.Empty;
}

public class UpdateDebtDto
{
    public string PartyName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public bool IsLending { get; set; }
    public string Notes { get; set; } = string.Empty;
    public bool IsResolved { get; set; }
}
