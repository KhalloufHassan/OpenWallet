namespace OpenWallet.Database.Models;

public class PasskeyCredential
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public byte[] CredentialId { get; set; } = [];
    public byte[] PublicKey { get; set; } = [];
    public uint SignCount { get; set; }
    public string Transports { get; set; } = string.Empty;
    public bool IsBackupEligible { get; set; }
    public bool IsBackedUp { get; set; }
    public Guid AaGuid { get; set; }
    public DateTime CreatedAt { get; set; }
}
