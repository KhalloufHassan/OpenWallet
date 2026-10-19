using Microsoft.EntityFrameworkCore;
using OpenWallet.Database;
using OpenWallet.Database.Models;
using OpenWallet.Shared.DTOs;
using OpenWallet.Shared.Models;

namespace OpenWallet.Managers;

public class DebtsManager(AppDbContext db)
{
    public async Task<List<DebtDto>> GetAllAsync()
    {
        List<Debt> debts = await db.Debts
            .Include(d => d.DebtRecords).ThenInclude(dr => dr.Record).ThenInclude(r => r.Account)
            .Include(d => d.DebtRecords).ThenInclude(dr => dr.Record).ThenInclude(r => r.Category)
            .Include(d => d.DebtRecords).ThenInclude(dr => dr.Record).ThenInclude(r => r.RecordTags).ThenInclude(rt => rt.Tag)
            .ToListAsync();

        return debts.Select(MapToDto).ToList();
    }

    public async Task<DebtDto> GetByIdAsync(int id)
    {
        Debt debt = await db.Debts
            .Include(d => d.DebtRecords).ThenInclude(dr => dr.Record).ThenInclude(r => r.Account)
            .Include(d => d.DebtRecords).ThenInclude(dr => dr.Record).ThenInclude(r => r.Category)
            .Include(d => d.DebtRecords).ThenInclude(dr => dr.Record).ThenInclude(r => r.RecordTags).ThenInclude(rt => rt.Tag)
            .FirstOrDefaultAsync(d => d.Id == id)
            ?? throw new KeyNotFoundException($"Debt {id} not found.");

        return MapToDto(debt);
    }

    public async Task<DebtDto> CreateAsync(CreateDebtDto dto)
    {
        Debt debt = new()
        {
            PartyName = dto.PartyName,
            Amount = dto.Amount,
            IsLending = dto.IsLending,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow,
            IsResolved = false
        };

        db.Debts.Add(debt);
        await db.SaveChangesAsync();
        return await GetByIdAsync(debt.Id);
    }

    public async Task<DebtDto> UpdateAsync(int id, UpdateDebtDto dto)
    {
        Debt debt = await db.Debts.FindAsync(id)
            ?? throw new KeyNotFoundException($"Debt {id} not found.");

        debt.PartyName = dto.PartyName;
        debt.Amount = dto.Amount;
        debt.IsLending = dto.IsLending;
        debt.Notes = dto.Notes;
        debt.IsResolved = dto.IsResolved;

        await db.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    public async Task DeleteAsync(int id)
    {
        Debt debt = await db.Debts
            .Include(d => d.DebtRecords)
            .FirstOrDefaultAsync(d => d.Id == id)
            ?? throw new KeyNotFoundException($"Debt {id} not found.");

        db.DebtRecords.RemoveRange(debt.DebtRecords);
        db.Debts.Remove(debt);
        await db.SaveChangesAsync();
    }

    public async Task LinkRecordAsync(int debtId, int recordId)
    {
        bool exists = await db.DebtRecords.AnyAsync(dr => dr.DebtId == debtId && dr.RecordId == recordId);
        if (exists) return;

        db.DebtRecords.Add(new DebtRecord { DebtId = debtId, RecordId = recordId });
        await db.SaveChangesAsync();
    }

    public async Task UnlinkRecordAsync(int debtId, int recordId)
    {
        DebtRecord debtRecord = await db.DebtRecords
            .FirstOrDefaultAsync(dr => dr.DebtId == debtId && dr.RecordId == recordId)
            ?? throw new KeyNotFoundException();

        db.DebtRecords.Remove(debtRecord);
        await db.SaveChangesAsync();
    }

    private static DebtDto MapToDto(Debt d)
    {
        List<RecordDto> records = d.DebtRecords
            .Select(dr => RecordsManager.MapToDto(dr.Record))
            .ToList();

        decimal paid = d.IsLending
            ? records.Where(r => r.Type == RecordType.Income).Sum(r => r.Amount)
            : records.Where(r => r.Type == RecordType.Expense).Sum(r => Math.Abs(r.Amount));
        return new DebtDto
        {
            Id = d.Id,
            PartyName = d.PartyName,
            Amount = d.Amount,
            IsLending = d.IsLending,
            Notes = d.Notes,
            IsResolved = d.IsResolved,
            CreatedAt = d.CreatedAt,
            AmountPaid = paid,
            AmountRemaining = Math.Max(0, d.Amount - paid),
            RelatedRecords = records
        };
    }
}
