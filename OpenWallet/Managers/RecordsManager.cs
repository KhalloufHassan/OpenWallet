using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;
using OpenWallet.Database;
using OpenWallet.Database.Models;
using OpenWallet.Shared.DTOs;
using OpenWallet.Shared.Models;

namespace OpenWallet.Managers;

public class RecordsManager(AppDbContext db)
{
    public async Task<(List<RecordDto> Records, int Total)> GetAllAsync(RecordFilterDto filter)
    {
        IQueryable<Record> query = db.Records
            .Include(r => r.Account)
            .Include(r => r.Category)
            .Include(r => r.Store)
            .Include(r => r.RecordTags).ThenInclude(rt => rt.Tag)
            .Include(r => r.Attachments)
            .Include(r => r.LinkedTransferRecord)
            .AsQueryable();

        if (filter.AccountId.HasValue)
            query = query.Where(r => r.AccountId == filter.AccountId.Value);

        if (filter.CategoryId.HasValue)
            query = query.Where(r => r.CategoryId == filter.CategoryId.Value
                                  || r.Category.ParentCategoryId == filter.CategoryId.Value);

        if (filter.StoreId.HasValue)
            query = query.Where(r => r.StoreId == filter.StoreId.Value);

        if (filter.Type.HasValue)
            query = query.Where(r => r.Type == filter.Type.Value);

        if (filter.FromDate.HasValue)
            query = query.Where(r => r.DateTime >= Utc(filter.FromDate.Value));

        if (filter.ToDate.HasValue)
            query = query.Where(r => r.DateTime <= Utc(filter.ToDate.Value));

        if (filter.TagIds.Count > 0)
            query = query.Where(r => r.RecordTags.Any(rt => filter.TagIds.Contains(rt.TagId)));

        if (!string.IsNullOrWhiteSpace(filter.Search))
            query = query.Where(r => r.Notes.Contains(filter.Search));

        int total = await query.CountAsync();

        List<Record> records = await query
            .OrderByDescending(r => r.DateTime)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return (records.Select(MapToDto).ToList(), total);
    }

    public async Task<RecordDto> GetByIdAsync(int id)
    {
        Record record = await db.Records
            .Include(r => r.Account)
            .Include(r => r.Category)
            .Include(r => r.Store)
            .Include(r => r.RecordTags).ThenInclude(rt => rt.Tag)
            .Include(r => r.Attachments)
            .Include(r => r.LinkedTransferRecord)
            .FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new KeyNotFoundException($"Record {id} not found.");

        return MapToDto(record);
    }

    public async Task<List<RecordDto>> GetRecentAsync(int count = 10)
    {
        List<Record> records = await db.Records
            .Include(r => r.Account)
            .Include(r => r.Category)
            .Include(r => r.Store)
            .Include(r => r.RecordTags).ThenInclude(rt => rt.Tag)
            .Include(r => r.Attachments)
            .Include(r => r.LinkedTransferRecord)
            .OrderByDescending(r => r.DateTime)
            .Take(count)
            .ToListAsync();

        return records.Select(MapToDto).ToList();
    }

    public async Task<RecordDto> CreateAsync(CreateRecordDto dto)
    {
        Record record = new()
        {
            AccountId = dto.AccountId,
            CategoryId = dto.CategoryId,
            StoreId = dto.StoreId,
            Type = dto.Type,
            Amount = dto.Type == RecordType.Expense ? -Math.Abs(dto.Amount) : Math.Abs(dto.Amount),
            DateTime = Utc(dto.DateTime),
            Notes = dto.Notes,
            Location = ToPoint(dto.Latitude, dto.Longitude)
        };

        db.Records.Add(record);
        await db.SaveChangesAsync();

        foreach (int tagId in dto.TagIds)
            db.RecordTags.Add(new RecordTag { RecordId = record.Id, TagId = tagId });

        await db.SaveChangesAsync();
        return await GetByIdAsync(record.Id);
    }

    public async Task<(RecordDto Outgoing, RecordDto Incoming)> CreateTransferAsync(CreateTransferDto dto)
    {
        Record outgoing = new()
        {
            AccountId = dto.FromAccountId,
            CategoryId = dto.CategoryId,
            Type = RecordType.Transfer,
            Amount = -Math.Abs(dto.Amount),
            DateTime = Utc(dto.DateTime),
            Notes = dto.Notes
        };

        Record incoming = new()
        {
            AccountId = dto.ToAccountId,
            CategoryId = dto.CategoryId,
            Type = RecordType.Transfer,
            Amount = Math.Abs(dto.Amount),
            DateTime = Utc(dto.DateTime),
            Notes = dto.Notes
        };

        db.Records.Add(outgoing);
        db.Records.Add(incoming);
        await db.SaveChangesAsync();

        outgoing.LinkedTransferRecordId = incoming.Id;
        incoming.LinkedTransferRecordId = outgoing.Id;

        foreach (int tagId in dto.TagIds)
        {
            db.RecordTags.Add(new RecordTag { RecordId = outgoing.Id, TagId = tagId });
            db.RecordTags.Add(new RecordTag { RecordId = incoming.Id, TagId = tagId });
        }

        await db.SaveChangesAsync();

        return (await GetByIdAsync(outgoing.Id), await GetByIdAsync(incoming.Id));
    }

    public async Task<(RecordDto Outgoing, RecordDto Incoming)> UpdateTransferAsync(int id, CreateTransferDto dto)
    {
        Record record = await db.Records
            .Include(r => r.RecordTags)
            .FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new KeyNotFoundException($"Record {id} not found.");

        Record linked = await db.Records
            .Include(r => r.RecordTags)
            .FirstOrDefaultAsync(r => r.Id == record.LinkedTransferRecordId)
            ?? throw new KeyNotFoundException($"Linked transfer record not found.");

        Record outgoing = record.Amount < 0 ? record : linked;
        Record incoming = record.Amount < 0 ? linked : record;

        outgoing.AccountId  = dto.FromAccountId;
        outgoing.CategoryId = dto.CategoryId;
        outgoing.Amount     = -Math.Abs(dto.Amount);
        outgoing.DateTime   = Utc(dto.DateTime);
        outgoing.Notes      = dto.Notes;

        incoming.AccountId  = dto.ToAccountId;
        incoming.CategoryId = dto.CategoryId;
        incoming.Amount     = Math.Abs(dto.Amount);
        incoming.DateTime   = Utc(dto.DateTime);
        incoming.Notes      = dto.Notes;

        db.RecordTags.RemoveRange(outgoing.RecordTags);
        db.RecordTags.RemoveRange(incoming.RecordTags);
        foreach (int tagId in dto.TagIds)
        {
            db.RecordTags.Add(new RecordTag { RecordId = outgoing.Id, TagId = tagId });
            db.RecordTags.Add(new RecordTag { RecordId = incoming.Id, TagId = tagId });
        }

        await db.SaveChangesAsync();
        return (await GetByIdAsync(outgoing.Id), await GetByIdAsync(incoming.Id));
    }

    public async Task<RecordDto> UpdateAsync(int id, UpdateRecordDto dto)
    {
        Record record = await db.Records
            .Include(r => r.RecordTags)
            .FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new KeyNotFoundException($"Record {id} not found.");

        record.AccountId = dto.AccountId;
        record.CategoryId = dto.Type == RecordType.Transfer ? null : dto.CategoryId;
        record.StoreId = dto.StoreId;
        record.Type = dto.Type;
        record.Amount = dto.Type == RecordType.Expense ? -Math.Abs(dto.Amount) : Math.Abs(dto.Amount);
        record.DateTime = Utc(dto.DateTime);
        record.Notes = dto.Notes;
        record.Location = ToPoint(dto.Latitude, dto.Longitude);

        db.RecordTags.RemoveRange(record.RecordTags);
        foreach (int tagId in dto.TagIds)
            db.RecordTags.Add(new RecordTag { RecordId = record.Id, TagId = tagId });

        await db.SaveChangesAsync();
        return await GetByIdAsync(record.Id);
    }

    public async Task DeleteAsync(int id)
    {
        Record record = await db.Records
            .Include(r => r.RecordTags)
            .Include(r => r.Attachments)
            .FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new KeyNotFoundException($"Record {id} not found.");

        if (record.LinkedTransferRecordId.HasValue)
        {
            Record linked = await db.Records
                .Include(r => r.RecordTags)
                .Include(r => r.Attachments)
                .FirstOrDefaultAsync(r => r.Id == record.LinkedTransferRecordId.Value)
                ?? throw new KeyNotFoundException();

            record.LinkedTransferRecordId = null;
            linked.LinkedTransferRecordId = null;
            await db.SaveChangesAsync();

            db.RecordTags.RemoveRange(linked.RecordTags);
            db.Attachments.RemoveRange(linked.Attachments);
            db.Records.Remove(linked);
        }

        db.RecordTags.RemoveRange(record.RecordTags);
        db.Attachments.RemoveRange(record.Attachments);
        db.Records.Remove(record);
        await db.SaveChangesAsync();
    }

    static DateTime Utc(DateTime dt) => DateTime.SpecifyKind(dt, DateTimeKind.Utc);

    // NpgsqlPoint stores (X=longitude, Y=latitude) matching the geometric convention
    static NpgsqlPoint? ToPoint(double? lat, double? lon) =>
        lat.HasValue && lon.HasValue ? new NpgsqlPoint(lon.Value, lat.Value) : null;

    public static RecordDto MapToDto(Record r) => new()
    {
        Id = r.Id,
        AccountId = r.AccountId,
        AccountName = r.Account?.Name ?? string.Empty,
        AccountCurrency = r.Account?.Currency ?? string.Empty,
        AccountColor = r.Account?.Color ?? string.Empty,
        CategoryId = r.CategoryId ?? 0,
        CategoryName = r.Category?.Name ?? string.Empty,
        CategoryIcon = r.Category?.Icon ?? string.Empty,
        CategoryColor = r.Category?.Color ?? string.Empty,
        StoreId = r.StoreId,
        StoreName = r.Store?.Name ?? string.Empty,
        Type = r.Type,
        Amount = r.Amount,
        DateTime = r.DateTime,
        Notes = r.Notes,
        Latitude = r.Location?.Y,
        Longitude = r.Location?.X,
        LinkedTransferRecordId = r.LinkedTransferRecordId,
        LinkedAccountId = r.LinkedTransferRecord?.AccountId,
        Tags = r.RecordTags?.Select(rt => new TagDto { Id = rt.Tag.Id, Name = rt.Tag.Name }).ToList() ?? [],
        Attachments = r.Attachments?.Select(a => new AttachmentDto
        {
            Id = a.Id,
            FileName = a.FileName,
            ContentType = a.ContentType,
            FileSize = a.FileSize
        }).ToList() ?? []
    };
}
