using Microsoft.EntityFrameworkCore;
using OpenWallet.Database;
using OpenWallet.Database.Models;
using OpenWallet.Shared.DTOs;

namespace OpenWallet.Managers;

public class TagsManager(AppDbContext db)
{
    public async Task<List<TagDto>> GetAllAsync()
    {
        return await db.Tags
            .Select(t => new TagDto { Id = t.Id, Name = t.Name })
            .ToListAsync();
    }

    public async Task<TagDto> GetByIdAsync(int id)
    {
        Tag tag = await db.Tags.FindAsync(id)
            ?? throw new KeyNotFoundException($"Tag {id} not found.");

        return new TagDto { Id = tag.Id, Name = tag.Name };
    }

    public async Task<TagDto> CreateAsync(CreateTagDto dto)
    {
        Tag tag = new() { Name = dto.Name };
        db.Tags.Add(tag);
        await db.SaveChangesAsync();
        return new TagDto { Id = tag.Id, Name = tag.Name };
    }

    public async Task<TagDto> UpdateAsync(int id, CreateTagDto dto)
    {
        Tag tag = await db.Tags.FindAsync(id)
            ?? throw new KeyNotFoundException($"Tag {id} not found.");

        tag.Name = dto.Name;
        await db.SaveChangesAsync();
        return new TagDto { Id = tag.Id, Name = tag.Name };
    }

    public async Task DeleteAsync(int id)
    {
        Tag tag = await db.Tags.FindAsync(id)
            ?? throw new KeyNotFoundException($"Tag {id} not found.");

        db.Tags.Remove(tag);
        await db.SaveChangesAsync();
    }
}
