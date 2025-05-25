namespace DiscordChannelReader.Utils;

public static class CaesarCipher
{
    public static string Decrypt(string text, int shift = 3)
    {
        return new string(text.Select(c =>
        {
            if (!char.IsLetter(c)) return c;

            char offset = char.IsUpper(c) ? 'A' : 'a';
            return (char)(((c - offset - shift + 26) % 26) + offset);
        }).ToArray());
    }

    public static string Encrypt(string text, int shift = 3)
    {
        return new string(text.Select(c =>
        {
            if (!char.IsLetter(c)) return c;

            char offset = char.IsUpper(c) ? 'A' : 'a';
            return (char)(((c - offset + shift) % 26) + offset);
        }).ToArray());
    }
}
