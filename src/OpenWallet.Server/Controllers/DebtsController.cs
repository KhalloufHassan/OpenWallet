using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenWallet.Managers;
using OpenWallet.Shared.DTOs;

namespace OpenWallet.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class DebtsController(DebtsManager manager) : ControllerBase
{
    /// <summary>Returns all debts.</summary>
    [HttpGet]
    public async Task<ActionResult<List<DebtDto>>> GetAll() =>
        Ok(await manager.GetAllAsync());

    /// <summary>Returns a single debt by ID.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<DebtDto>> GetById(int id)
    {
        try { return Ok(await manager.GetByIdAsync(id)); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    /// <summary>Creates a new debt.</summary>
    [HttpPost]
    public async Task<ActionResult<DebtDto>> Create(CreateDebtDto dto)
    {
        DebtDto created = await manager.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Updates an existing debt.</summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<DebtDto>> Update(int id, UpdateDebtDto dto)
    {
        try { return Ok(await manager.UpdateAsync(id, dto)); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    /// <summary>Deletes a debt.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try { await manager.DeleteAsync(id); return NoContent(); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    /// <summary>Links an existing record to a debt.</summary>
    [HttpPost("{id:int}/records/{recordId:int}")]
    public async Task<IActionResult> LinkRecord(int id, int recordId)
    {
        await manager.LinkRecordAsync(id, recordId);
        return NoContent();
    }

    /// <summary>Removes a record link from a debt.</summary>
    [HttpDelete("{id:int}/records/{recordId:int}")]
    public async Task<IActionResult> UnlinkRecord(int id, int recordId)
    {
        try { await manager.UnlinkRecordAsync(id, recordId); return NoContent(); }
        catch (KeyNotFoundException) { return NotFound(); }
    }
}
