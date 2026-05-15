namespace PasswordManager.Domain.Interfaces;

public interface IEncryptionService
{
    byte[] DeriveKeyFromMasterPassword(string masterPassword, byte[] encryptionSalt);

    (byte[] cipherText, byte[] nonce, byte[] tag) Encrypt(string plainText, byte[] key);

    string Decrypt(byte[] cipherText, byte[] nonce, byte[] tag, byte[] key);
}