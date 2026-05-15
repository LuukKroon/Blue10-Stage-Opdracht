namespace PasswordManager.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    
    public string MasterPasswordHash { get; set; } = string.Empty;
    
    public byte[] PasswordSalt { get; set; } = Array.Empty<byte>(); 
    
    public byte[] EncryptionSalt { get; set; } = Array.Empty<byte>();
    public string? TotpSecret { get; set; } 
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}