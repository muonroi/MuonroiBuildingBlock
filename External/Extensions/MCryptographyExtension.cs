﻿namespace Muonroi.BuildingBlock.External.Extensions;

public static class MCryptographyExtension
{
    public static string Decrypt(string key, string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText) || string.IsNullOrEmpty(key))
        {
            throw new ArgumentException("Key and cipherText must not be null or empty.");
        }

        byte[] buffer = global::System.Convert.FromBase64String(cipherText);
        byte[] iv = new byte[16];

        byte[] keyBytes = GetValidKey(key, 256);

        using Aes aesAlg = Aes.Create();
        aesAlg.Padding = PaddingMode.PKCS7;
        aesAlg.KeySize = 256;
        aesAlg.Key = keyBytes;
        aesAlg.IV = iv;

        ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

        using MemoryStream msDecrypt = new(buffer);
        using CryptoStream csDecrypt = new(msDecrypt, decryptor, CryptoStreamMode.Read);
        using StreamReader srDecrypt = new(csDecrypt);

        return srDecrypt.ReadToEnd();
    }

    private static byte[] GetValidKey(string key, int keySize)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(key);
        byte[] hashBytes = SHA256.HashData(keyBytes);
        byte[] validKey = new byte[keySize / 8];
        Buffer.BlockCopy(hashBytes, 0, validKey, 0, validKey.Length);
        return validKey;
    }

    public static string GenerateSha256String(string inputString)
    {
        byte[] hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(inputString));
        return BitConverter.ToString(hashBytes).Replace("-", string.Empty).ToLower();
    }

    public static string GenerateHmacSha512(string key, string inputData)
    {
        StringBuilder hash = new();
        byte[] keyBytes = Encoding.UTF8.GetBytes(key);
        byte[] inputBytes = Encoding.UTF8.GetBytes(inputData);
        using (HMACSHA512 hmac = new(keyBytes))
        {
            foreach (byte theByte in hmac.ComputeHash(inputBytes))
            {
                _ = hash.Append(theByte.ToString("x2"));
            }
        }

        return hash.ToString();
    }

    public static string MD5Hash(string input)
    {
        byte[] hashBytes = MD5.HashData(Encoding.UTF8.GetBytes(input));
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    }

    public static string EncryptMd5(string text)
    {
        byte[] byteValues = Encoding.Unicode.GetBytes(text);
        byte[] byteHash = MD5.HashData(byteValues);
        return global::System.Convert.ToBase64String(byteHash);
    }

    public static string Sha256(string value)
    {
        StringBuilder Sb = new();
        Encoding enc = Encoding.UTF8;
        foreach (byte b in SHA256.HashData(enc.GetBytes(value)))
        {
            _ = Sb.Append(b.ToString("x2"));
        }

        return Sb.ToString();
    }

    public static string EncryptMd5Sha256WithSalt(string data, string salt)
    {
        return Sha256(EncryptMd5(data.Trim()) + salt);
    }
}