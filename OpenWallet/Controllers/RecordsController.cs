using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenWallet.Managers;
using OpenWallet.Shared.DTOs;

namespace OpenWallet.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class RecordsController(RecordsManager manager, AttachmentsManager attachmentsManager) : ControllerBase
{
    /// <summary>Returns a filtered, paginated list of records.</summary>
    [HttpGet]
    public async Task<ActionResult> GetAll([FromQuery] RecordFilterDto filter)
    {
        (List<RecordDto> records, int total) = await manager.GetAllAsync(filter);
        return Ok(new { Records = records, Total = total });
    }

    /// <summary>Returns a single record by ID.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<RecordDto>> GetById(int id)
    {
        try { return Ok(await manager.GetByIdAsync(id)); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    /// <summary>Creates a new expense or income record.</summary>
    [HttpPost]
    public async Task<ActionResult<RecordDto>> Create(CreateRecordDto dto)
    {
        RecordDto created = await manager.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Creates a transfer between two accounts.</summary>
    [HttpPost("transfer")]
    public async Task<ActionResult> CreateTransfer(CreateTransferDto dto)
    {
        (RecordDto outgoing, RecordDto incoming) = await manager.CreateTransferAsync(dto);
        return Ok(new { Outgoing = outgoing, Incoming = incoming });
    }

    /// <summary>Updates an existing record.</summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<RecordDto>> Update(int id, UpdateRecordDto dto)
    {
        try { return Ok(await manager.UpdateAsync(id, dto)); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    /// <summary>Deletes a record. If it's part of a transfer, both sides are deleted.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try { await manager.DeleteAsync(id); return NoContent(); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    /// <summary>Uploads a file attachment to a record.</summary>
    [HttpPost("{id:int}/attachments")]
    public async Task<ActionResult<AttachmentDto>> UploadAttachment(int id, IFormFile file)
    {
        AttachmentDto dto = await attachmentsManager.SaveAsync(id, file);
        return Ok(dto);
    }

    /// <summary>Downloads a file attachment.</summary>
    [HttpGet("{id:int}/attachments/{attachmentId:int}")]
    public async Task<IActionResult> DownloadAttachment(int id, int attachmentId)
    {
        try
        {
            (byte[] data, string contentType, string fileName) = await attachmentsManager.GetFileAsync(attachmentId);
            return File(data, contentType, fileName);
        }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    /// <summary>Deletes a file attachment.</summary>
    [HttpDelete("{id:int}/attachments/{attachmentId:int}")]
    public async Task<IActionResult> DeleteAttachment(int id, int attachmentId)
    {
        try { await attachmentsManager.DeleteAsync(attachmentId); return NoContent(); }
        catch (KeyNotFoundException) { return NotFound(); }
    }
}
