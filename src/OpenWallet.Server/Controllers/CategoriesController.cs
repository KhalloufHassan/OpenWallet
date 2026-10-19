using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenWallet.Managers;
using OpenWallet.Shared.DTOs;

namespace OpenWallet.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class CategoriesController(CategoriesManager manager) : ControllerBase
{
    /// <summary>Returns all root categories with their sub-categories.</summary>
    [HttpGet]
    public async Task<ActionResult<List<CategoryDto>>> GetAll() =>
        Ok(await manager.GetAllAsync());

    /// <summary>Returns a flat list of all categories.</summary>
    [HttpGet("flat")]
    public async Task<ActionResult<List<CategoryDto>>> GetFlat() =>
        Ok(await manager.GetFlatAsync());

    /// <summary>Returns a single category by ID.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoryDto>> GetById(int id)
    {
        try { return Ok(await manager.GetByIdAsync(id)); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    /// <summary>Creates a new category.</summary>
    [HttpPost]
    public async Task<ActionResult<CategoryDto>> Create(CreateCategoryDto dto)
    {
        CategoryDto created = await manager.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Updates an existing category.</summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<CategoryDto>> Update(int id, UpdateCategoryDto dto)
    {
        try { return Ok(await manager.UpdateAsync(id, dto)); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    /// <summary>Deletes a category.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try { await manager.DeleteAsync(id); return NoContent(); }
        catch (KeyNotFoundException) { return NotFound(); }
    }
}
