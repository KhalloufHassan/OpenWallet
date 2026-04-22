using Microsoft.EntityFrameworkCore;
using OpenWallet.Database;
using OpenWallet.Database.Models;
using OpenWallet.Shared.DTOs;

namespace OpenWallet.Managers;

public class StoresManager(AppDbContext db)
{
    public async Task<List<StoreDto>> GetAllAsync() =>
        await db.Stores
            .OrderBy(s => s.Name)
            .Select(s => new StoreDto { Id = s.Id, Name = s.Name })
            .ToListAsync();

    public async Task<StoreDto> GetByIdAsync(int id)
    {
        Store store = await db.Stores.FindAsync(id)
            ?? throw new KeyNotFoundException($"Store {id} not found.");
        return new StoreDto { Id = store.Id, Name = store.Name };
    }

    public async Task<StoreDto> CreateAsync(CreateStoreDto dto)
    {
        Store store = new() { Name = dto.Name };
        db.Stores.Add(store);
        await db.SaveChangesAsync();
        return new StoreDto { Id = store.Id, Name = store.Name };
    }

    public async Task<StoreDto> UpdateAsync(int id, CreateStoreDto dto)
    {
        Store store = await db.Stores.FindAsync(id)
            ?? throw new KeyNotFoundException($"Store {id} not found.");
        store.Name = dto.Name;
        await db.SaveChangesAsync();
        return new StoreDto { Id = store.Id, Name = store.Name };
    }

    public async Task DeleteAsync(int id)
    {
        Store store = await db.Stores.FindAsync(id)
            ?? throw new KeyNotFoundException($"Store {id} not found.");
        db.Stores.Remove(store);
        await db.SaveChangesAsync();
    }
}
