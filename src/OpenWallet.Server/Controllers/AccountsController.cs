using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenWallet.Managers;
using OpenWallet.Shared.DTOs;

namespace OpenWallet.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class AccountsController(AccountsManager manager) : ControllerBase
{
    /// <summary>Returns all accounts with their current balance.</summary>
    [HttpGet]
    public async Task<ActionResult<List<AccountDto>>> GetAll() =>
        Ok(await manager.GetAllAsync());

    /// <summary>Returns a single account by ID.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<AccountDto>> GetById(int id)
    {
        try { return Ok(await manager.GetByIdAsync(id)); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    /// <summary>Creates a new account.</summary>
    [HttpPost]
    public async Task<ActionResult<AccountDto>> Create(CreateAccountDto dto)
    {
        AccountDto created = await manager.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Updates an existing account.</summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<AccountDto>> Update(int id, UpdateAccountDto dto)
    {
        try { return Ok(await manager.UpdateAsync(id, dto)); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    /// <summary>Deletes an account.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try { await manager.DeleteAsync(id); return NoContent(); }
        catch (KeyNotFoundException) { return NotFound(); }
    }
}
