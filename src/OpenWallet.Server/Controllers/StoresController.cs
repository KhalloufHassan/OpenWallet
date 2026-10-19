using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenWallet.Managers;
using OpenWallet.Shared.DTOs;

namespace OpenWallet.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class StoresController(StoresManager manager) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<StoreDto>>> GetAll() =>
        Ok(await manager.GetAllAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<StoreDto>> GetById(int id)
    {
        try { return Ok(await manager.GetByIdAsync(id)); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpPost]
    public async Task<ActionResult<StoreDto>> Create(CreateStoreDto dto)
    {
        StoreDto created = await manager.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<StoreDto>> Update(int id, CreateStoreDto dto)
    {
        try { return Ok(await manager.UpdateAsync(id, dto)); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try { await manager.DeleteAsync(id); return NoContent(); }
        catch (KeyNotFoundException) { return NotFound(); }
    }
}
