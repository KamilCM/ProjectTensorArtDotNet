using System;
using System.IO;
using System.Text;

namespace DiscordChannelReader.Utils;

public static class TokenObfuscator
{
    private static string GetSecret()
    {
        var path = @"C:\temp\some_stuff_sea_related\some.txt";

        if (!File.Exists(path))
            throw new FileNotFoundException("Secret key file not found.", path);

        return File.ReadAllText(path).Trim();
    }

    public static string Encrypt(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var key = Encoding.UTF8.GetBytes(GetSecret());
        for (int i = 0; i < bytes.Length; i++)
            bytes[i] ^= key[i % key.Length];
        return Convert.ToBase64String(bytes);
    }

    public static string Decrypt(string encrypted)
    {
        var bytes = Convert.FromBase64String(encrypted);
        var key = Encoding.UTF8.GetBytes(GetSecret());
        for (int i = 0; i < bytes.Length; i++)
            bytes[i] ^= key[i % key.Length];
        return Encoding.UTF8.GetString(bytes);
    }
}
