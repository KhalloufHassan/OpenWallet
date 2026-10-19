using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenWallet.Managers;
using OpenWallet.Shared.DTOs;

namespace OpenWallet.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class TagsController(TagsManager manager) : ControllerBase
{
    /// <summary>Returns all tags.</summary>
    [HttpGet]
    public async Task<ActionResult<List<TagDto>>> GetAll() =>
        Ok(await manager.GetAllAsync());

    /// <summary>Returns a single tag by ID.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<TagDto>> GetById(int id)
    {
        try { return Ok(await manager.GetByIdAsync(id)); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    /// <summary>Creates a new tag.</summary>
    [HttpPost]
    public async Task<ActionResult<TagDto>> Create(CreateTagDto dto)
    {
        TagDto created = await manager.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Updates an existing tag.</summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<TagDto>> Update(int id, CreateTagDto dto)
    {
        try { return Ok(await manager.UpdateAsync(id, dto)); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    /// <summary>Deletes a tag.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try { await manager.DeleteAsync(id); return NoContent(); }
        catch (KeyNotFoundException) { return NotFound(); }
    }
}
