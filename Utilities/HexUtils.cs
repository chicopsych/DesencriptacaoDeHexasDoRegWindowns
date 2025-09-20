using System;
using System.Text;

namespace DesencriptacaoDeHexasDoRegWindowns.Utilities
{
    public static class HexUtils
    {
        public static string CleanHex(string hex)
        {
            if (string.IsNullOrWhiteSpace(hex)) return string.Empty;
            return hex.Replace("-", "")
                      .Replace(" ", "")
                      .Replace(":", "")
                      .Replace("0x", "", StringComparison.OrdinalIgnoreCase)
                      .Trim();
        }

        public static byte[] HexStringToByteArray(string hex)
        {
            hex = CleanHex(hex);
            if (hex.Length % 2 != 0)
            {
                throw new FormatException("A string hexadecimal deve ter um número par de dígitos.");
            }

            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return bytes;
        }

        public static bool TryParseHex(string? hex, out byte[] bytes)
        {
            bytes = Array.Empty<byte>();
            if (string.IsNullOrWhiteSpace(hex)) return false;
            try
            {
                bytes = HexStringToByteArray(hex);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static string ToHex(byte[] data) => BitConverter.ToString(data).Replace("-", "");

        public static string ToSpacedHex(byte[] data) => BitConverter.ToString(data).Replace("-", " ");
    }
}
