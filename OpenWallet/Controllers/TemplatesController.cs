using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenWallet.Managers;
using OpenWallet.Shared.DTOs;

namespace OpenWallet.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class TemplatesController(TemplatesManager manager) : ControllerBase
{
    /// <summary>Returns all templates.</summary>
    [HttpGet]
    public async Task<ActionResult<List<TemplateDto>>> GetAll() =>
        Ok(await manager.GetAllAsync());

    /// <summary>Returns a single template by ID.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<TemplateDto>> GetById(int id)
    {
        try { return Ok(await manager.GetByIdAsync(id)); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    /// <summary>Creates a new template.</summary>
    [HttpPost]
    public async Task<ActionResult<TemplateDto>> Create(CreateTemplateDto dto)
    {
        TemplateDto created = await manager.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Updates an existing template.</summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<TemplateDto>> Update(int id, UpdateTemplateDto dto)
    {
        try { return Ok(await manager.UpdateAsync(id, dto)); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    /// <summary>Deletes a template.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try { await manager.DeleteAsync(id); return NoContent(); }
        catch (KeyNotFoundException) { return NotFound(); }
    }
}
