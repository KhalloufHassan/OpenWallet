using Microsoft.EntityFrameworkCore;
using OpenWallet.Database;
using OpenWallet.Database.Models;
using OpenWallet.Shared.DTOs;

namespace OpenWallet.Managers;

public class TemplatesManager(AppDbContext db)
{
    public async Task<List<TemplateDto>> GetAllAsync()
    {
        List<Template> templates = await db.Templates
            .Include(t => t.Account)
            .Include(t => t.Category)
            .Include(t => t.TemplateTags).ThenInclude(tt => tt.Tag)
            .ToListAsync();

        return templates.Select(MapToDto).ToList();
    }

    public async Task<TemplateDto> GetByIdAsync(int id)
    {
        Template template = await db.Templates
            .Include(t => t.Account)
            .Include(t => t.Category)
            .Include(t => t.TemplateTags).ThenInclude(tt => tt.Tag)
            .FirstOrDefaultAsync(t => t.Id == id)
            ?? throw new KeyNotFoundException($"Template {id} not found.");

        return MapToDto(template);
    }

    public async Task<TemplateDto> CreateAsync(CreateTemplateDto dto)
    {
        Template template = new()
        {
            Name = dto.Name,
            AccountId = dto.AccountId,
            CategoryId = dto.CategoryId,
            Type = dto.Type,
            Amount = dto.Amount,
            Notes = dto.Notes
        };

        db.Templates.Add(template);
        await db.SaveChangesAsync();

        foreach (int tagId in dto.TagIds)
            db.TemplateTags.Add(new TemplateTag { TemplateId = template.Id, TagId = tagId });

        await db.SaveChangesAsync();
        return await GetByIdAsync(template.Id);
    }

    public async Task<TemplateDto> UpdateAsync(int id, UpdateTemplateDto dto)
    {
        Template template = await db.Templates
            .Include(t => t.TemplateTags)
            .FirstOrDefaultAsync(t => t.Id == id)
            ?? throw new KeyNotFoundException($"Template {id} not found.");

        template.Name = dto.Name;
        template.AccountId = dto.AccountId;
        template.CategoryId = dto.CategoryId;
        template.Type = dto.Type;
        template.Amount = dto.Amount;
        template.Notes = dto.Notes;

        db.TemplateTags.RemoveRange(template.TemplateTags);
        foreach (int tagId in dto.TagIds)
            db.TemplateTags.Add(new TemplateTag { TemplateId = template.Id, TagId = tagId });

        await db.SaveChangesAsync();
        return await GetByIdAsync(template.Id);
    }

    public async Task DeleteAsync(int id)
    {
        Template template = await db.Templates
            .Include(t => t.TemplateTags)
            .FirstOrDefaultAsync(t => t.Id == id)
            ?? throw new KeyNotFoundException($"Template {id} not found.");

        db.TemplateTags.RemoveRange(template.TemplateTags);
        db.Templates.Remove(template);
        await db.SaveChangesAsync();
    }

    private static TemplateDto MapToDto(Template t) => new()
    {
        Id = t.Id,
        Name = t.Name,
        AccountId = t.AccountId,
        AccountName = t.Account?.Name ?? string.Empty,
        CategoryId = t.CategoryId,
        CategoryName = t.Category?.Name ?? string.Empty,
        CategoryIcon = t.Category?.Icon ?? string.Empty,
        Type = t.Type,
        Amount = t.Amount,
        Notes = t.Notes,
        Tags = t.TemplateTags?.Select(tt => new TagDto { Id = tt.Tag.Id, Name = tt.Tag.Name }).ToList() ?? []
    };
}
