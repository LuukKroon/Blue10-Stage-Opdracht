using System.Security.Cryptography;
using System.Text;
using PasswordManager.Domain.Interfaces;

namespace PasswordManager.Infrastructure.Services;

public class AesGcmEncryptionService : IEncryptionService
{
    public byte[] DeriveKeyFromMasterPassword(string masterPassword, byte[] encryptionSalt)
    {
        using var pbkdf2 = new Rfc2898DeriveBytes(masterPassword, encryptionSalt, 600_000, HashAlgorithmName.SHA256);
        return pbkdf2.GetBytes(32); 
    }

    public (byte[] cipherText, byte[] nonce, byte[] tag) Encrypt(string plainText, byte[] key)
    {
        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
        
        byte[] nonce = new byte[AesGcm.NonceByteSizes.MaxSize];
        RandomNumberGenerator.Fill(nonce);

        byte[] cipherText = new byte[plainBytes.Length];
        byte[] tag = new byte[AesGcm.TagByteSizes.MaxSize];

        using (var aesGcm = new AesGcm(key, AesGcm.TagByteSizes.MaxSize))
        {
            aesGcm.Encrypt(nonce, plainBytes, cipherText, tag);
        }

        return (cipherText, nonce, tag);
    }

    public string Decrypt(byte[] cipherText, byte[] nonce, byte[] tag, byte[] key)
    {
        byte[] plainBytes = new byte[cipherText.Length];

        using (var aesGcm = new AesGcm(key, AesGcm.TagByteSizes.MaxSize))
        {
            aesGcm.Decrypt(nonce, cipherText, tag, plainBytes);
        }

        return Encoding.UTF8.GetString(plainBytes);
    }
}