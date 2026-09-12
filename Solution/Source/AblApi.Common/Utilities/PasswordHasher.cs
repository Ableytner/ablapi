using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace AblApi.Common.Utilities;

public static class PasswordHasher
{
    private const KeyDerivationPrf Prf = KeyDerivationPrf.HMACSHA256;
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 10000;

    public static string Hash(string password)
    {
        byte[] salt = GenerateSalt();
        byte[] subkey = Pbkdf2(password, salt, Prf, Iterations, HashSize);
        return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(subkey)}";
    }

    public static bool Verify(string password, string hashedPassword)
    {
        try
        {
            var colonIndex = hashedPassword.IndexOf(':');
            if (colonIndex <= 0 || colonIndex == hashedPassword.Length - 1) return false;

            byte[] salt = Convert.FromBase64String(hashedPassword[..colonIndex]);
            byte[] expectedSubkey = Convert.FromBase64String(hashedPassword[(colonIndex + 1)..]);
            byte[] actualSubkey = Pbkdf2(password, salt, Prf, Iterations, HashSize);

            return System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(actualSubkey, expectedSubkey);
        }
        catch
        {
            return false;
        }
    }

    private static byte[] GenerateSalt()
    {
        byte[] salt = new byte[SaltSize];
        using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }
        return salt;
    }

    private static byte[] Pbkdf2(string password, byte[] salt, KeyDerivationPrf prf, int iterations, int outputLength)
    {
        return KeyDerivation.Pbkdf2(password, salt, prf, iterations, outputLength);
    }
}
