using Microsoft.EntityFrameworkCore;
using OpenWallet.Database;
using OpenWallet.Database.Models;
using OpenWallet.Shared.DTOs;

namespace OpenWallet.Managers;

public class AttachmentsManager(AppDbContext db, IWebHostEnvironment env)
{
    private string UploadsPath => Path.Combine(env.ContentRootPath, "uploads");

    public async Task<AttachmentDto> SaveAsync(int recordId, IFormFile file)
    {
        Directory.CreateDirectory(UploadsPath);

        string uniqueName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        string filePath = Path.Combine(UploadsPath, uniqueName);

        using (FileStream stream = new(filePath, FileMode.Create))
            await file.CopyToAsync(stream);

        Attachment attachment = new()
        {
            RecordId = recordId,
            FileName = file.FileName,
            FilePath = uniqueName,
            ContentType = file.ContentType,
            FileSize = file.Length
        };

        db.Attachments.Add(attachment);
        await db.SaveChangesAsync();

        return new AttachmentDto
        {
            Id = attachment.Id,
            FileName = attachment.FileName,
            ContentType = attachment.ContentType,
            FileSize = attachment.FileSize
        };
    }

    public async Task DeleteAsync(int attachmentId)
    {
        Attachment attachment = await db.Attachments.FindAsync(attachmentId)
            ?? throw new KeyNotFoundException($"Attachment {attachmentId} not found.");

        string fullPath = Path.Combine(UploadsPath, attachment.FilePath);
        if (File.Exists(fullPath))
            File.Delete(fullPath);

        db.Attachments.Remove(attachment);
        await db.SaveChangesAsync();
    }

    public async Task<(byte[] Data, string ContentType, string FileName)> GetFileAsync(int attachmentId)
    {
        Attachment attachment = await db.Attachments.FindAsync(attachmentId)
            ?? throw new KeyNotFoundException($"Attachment {attachmentId} not found.");

        string fullPath = Path.Combine(UploadsPath, attachment.FilePath);
        byte[] data = await File.ReadAllBytesAsync(fullPath);
        return (data, attachment.ContentType, attachment.FileName);
    }
}
