using Microsoft.EntityFrameworkCore;
using OpenWallet.Database;
using OpenWallet.Database.Models;
using OpenWallet.Shared.DTOs;

namespace OpenWallet.Managers;

public class CategoriesManager(AppDbContext db)
{
    public async Task<List<CategoryDto>> GetAllAsync()
    {
        List<Category> categories = await db.Categories
            .Include(c => c.SubCategories)
            .Where(c => c.ParentCategoryId == null)
            .ToListAsync();

        return categories.Select(MapToDto).ToList();
    }

    public async Task<List<CategoryDto>> GetFlatAsync()
    {
        List<Category> categories = await db.Categories
            .Include(c => c.ParentCategory)
            .ToListAsync();

        return categories.Select(c => new CategoryDto
        {
            Id = c.Id,
            Name = c.Name,
            Icon = c.Icon,
            Color = c.Color,
            ParentCategoryId = c.ParentCategoryId,
            ParentCategoryName = c.ParentCategory?.Name ?? string.Empty
        }).ToList();
    }

    public async Task<CategoryDto> GetByIdAsync(int id)
    {
        Category category = await db.Categories
            .Include(c => c.SubCategories)
            .Include(c => c.ParentCategory)
            .FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new KeyNotFoundException($"Category {id} not found.");

        return MapToDto(category);
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto)
    {
        Category category = new()
        {
            Name = dto.Name,
            Icon = dto.Icon,
            Color = dto.Color,
            ParentCategoryId = dto.ParentCategoryId
        };

        db.Categories.Add(category);
        await db.SaveChangesAsync();
        return MapToDto(category);
    }

    public async Task<CategoryDto> UpdateAsync(int id, UpdateCategoryDto dto)
    {
        Category category = await db.Categories.FindAsync(id)
            ?? throw new KeyNotFoundException($"Category {id} not found.");

        category.Name = dto.Name;
        category.Icon = dto.Icon;
        category.Color = dto.Color;
        category.ParentCategoryId = dto.ParentCategoryId;

        await db.SaveChangesAsync();
        return MapToDto(category);
    }

    public async Task DeleteAsync(int id)
    {
        Category category = await db.Categories.FindAsync(id)
            ?? throw new KeyNotFoundException($"Category {id} not found.");

        db.Categories.Remove(category);
        await db.SaveChangesAsync();
    }

    private static CategoryDto MapToDto(Category c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        Icon = c.Icon,
        Color = c.Color,
        ParentCategoryId = c.ParentCategoryId,
        ParentCategoryName = c.ParentCategory?.Name ?? string.Empty,
        SubCategories = c.SubCategories.Select(MapToDto).ToList()
    };
}
