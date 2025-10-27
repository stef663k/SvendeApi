using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace SvendeApi.Utilities;

// Password hasher utility, bruger PBKDF2 til at hashe passwords med salt
public class PasswordHasher
{
    // Konstanter for hashing - salt størrelse, hash størrelse, iterations og algoritmer
    private const int SaltSize = 16;
    private const int HashSize = 20;
    private const int Iterations = 1000_000;
    private const string HashAlgorithm = "PBKDF2";
    private const string PseudoRandomFunction = "HMACSHA1";

    // Hasher et password med PBKDF2 og returnerer formateret string med salt og hash
    public static string HashPassword(string password)
    {
        if (password is null)
            throw new ArgumentNullException(nameof(password));

        // Genererer tilfældig salt
        Span<byte> salt = stackalloc byte[HashSize];
        RandomNumberGenerator.Fill(salt);

        // Hasher password med PBKDF2 algoritme
        byte[] subkey = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            HashSize);

        // Konverterer salt og hash til Base64 for lagring
        string saltB64 = Convert.ToBase64String(salt);
        string subkeyB64 = Convert.ToBase64String(subkey);
        
        // Returnerer formateret string med alle hash informationer, adskilt af '$' tegn
        return string.Join('$', HashAlgorithm, PseudoRandomFunction, Iterations, saltB64, subkeyB64);
    }

    // Tjekker om en hash string er korrekt formateret og indeholder alle nødvendige dele
    public static bool IsWellFormedHash(string? encode)
    {
        if (string.IsNullOrWhiteSpace(encode)) return false;
        string[] parts = encode.Split('$', StringSplitOptions.RemoveEmptyEntries);
        
        // Tjekker at hash har præcis 5 dele: algoritme, pseudo-random function, iterations, salt og hash
        if (parts.Length != 5)
            return false;
        if (!string.Equals(parts[0], HashAlgorithm, StringComparison.Ordinal)) return false;
        if (!string.Equals(parts[1], PseudoRandomFunction, StringComparison.Ordinal)) return false;
        if (!int.TryParse(parts[2], out _)) return false;
        return true;
    }

    // Verificerer om et password matcher en hashet streng ved at hashe passwordet og sammenligne hashes
    public static bool VerifyPassword(string password, string encoded)
    {
        if (password is null)
            throw new ArgumentNullException(nameof(password));

        // Tjekker at den encoded hash er korrekt formateret
        if (!IsWellFormedHash(encoded))
            return false;
        string[] parts = encoded.Split('$', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 5)
            return false;
        if (!string.Equals(parts[0], HashAlgorithm, StringComparison.Ordinal))
            return false;
        if (!int.TryParse(parts[2], out int iterations) || iterations < 10_000)
            return false;

        byte[] salt;
        byte[] expectedsSubkey;

        // Dekoder salt og forventet hash fra Base64
        try
        {
            salt = Convert.FromBase64String(parts[3]);
            expectedsSubkey = Convert.FromBase64String(parts[4]);
        }
        catch
        {
            return false;
        }

        // Hasher det indtastede password med samme salt og iterations
        byte[] actualSybKey = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            iterations,
            HashAlgorithmName.SHA256,
            HashSize
        );

        // Sammenligner hashes med timing-safe operationer for at forhindre timing attacks
        bool sameLength = actualSybKey.Length == expectedsSubkey.Length;
        int difference = actualSybKey.Length ^ expectedsSubkey.Length;
        int lenght = Math.Min(actualSybKey.Length, expectedsSubkey.Length);
        for (int i = 0; i < lenght; i++)
        {
            difference |= actualSybKey[i] ^ expectedsSubkey[i];
        }
        return sameLength && difference == 0;
    }
}