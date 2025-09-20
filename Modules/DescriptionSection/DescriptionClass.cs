using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using DesencriptacaoDeHexasDoRegWindowns.Modules;

namespace DesencriptacaoDeHexasDoRegWindowns.Modules.DescriptionSection
{
    public enum EncryptionType
    {
        Unknown,
        DPAPI,
        MD5,
        SHA1,
        SHA256,
        SHA512,
        BCRYPT,
        MD5_Crypt,
        SHA256_Crypt,
        SHA512_Crypt,
        NTLM,
        LM
    }

    public sealed class DescriptionClass
    {
        // Usa o valor sanitizado de "ShowTypeAndValueMethod" para descrever o tipo de encriptação
        public void DescribeEncryptionType(object valor)
        {
            string sanitizedValue;

            try
            {
                // Classe externa não definida aqui; manter chamada e proteger contra falhas.
                sanitizedValue = ShowTypeAndValue.SanitizeValueForDisplay(valor);
            }
            catch
            {
                sanitizedValue = valor?.ToString() ?? string.Empty;
            }

            Console.WriteLine($"Descrição do valor: {sanitizedValue}");
            var tipo = IdentifyEncryptionType(sanitizedValue);
            Console.WriteLine($"Tipo identificado: {tipo}");
        }

        public EncryptionAnalysis AnalyzeEncryption(string input)
        {
            input ??= string.Empty;
            string trimmed = input.Trim();

            bool isHex = IsHexString(trimmed);
            var type = IdentifyEncryptionType(trimmed);

            return new EncryptionAnalysis(type, isHex, trimmed.Length);
        }

        private EncryptionType IdentifyEncryptionType(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return EncryptionType.Unknown;

            string s = value.Trim();

            // 1) BCRYPT ($2a$, $2b$, $2y$)
            if (Regex.IsMatch(s, @"^\$2[aby]?\$\d{2}\$[./A-Za-z0-9]{53}$"))
                return EncryptionType.BCRYPT;

            // 2) Unix crypt flavors
            if (Regex.IsMatch(s, @"^\$1\$.+"))
                return EncryptionType.MD5_Crypt;
            if (Regex.IsMatch(s, @"^\$5\$.+"))
                return EncryptionType.SHA256_Crypt;
            if (Regex.IsMatch(s, @"^\$6\$.+"))
                return EncryptionType.SHA512_Crypt;

            // 3) DPAPI heurísticas (hex ou base64 com 0x01000000 no começo)
            if (IsHexString(s) && s.StartsWith("01000000", StringComparison.OrdinalIgnoreCase))
                return EncryptionType.DPAPI;
            if (IsBase64(s))
            {
                if (s.StartsWith("AQAA", StringComparison.Ordinal)) // 0x01000000 em Base64
                    return EncryptionType.DPAPI;

                // Decodificar e checar assinatura/tamanhos
                if (TryBase64Decode(s, out var bytes))
                {
                    if (bytes.Length >= 4 && bytes[0] == 0x01 && bytes[1] == 0x00 && bytes[2] == 0x00 && bytes[3] == 0x00)
                        return EncryptionType.DPAPI;

                    // Tamanhos canônicos de hash
                    return bytes.Length switch
                    {
                        16 => EncryptionType.MD5,
                        20 => EncryptionType.SHA1,
                        32 => EncryptionType.SHA256,
                        64 => EncryptionType.SHA512,
                        _ => EncryptionType.Unknown
                    };
                }
            }

            // 4) HEX puro com tamanhos conhecidos
            if (IsHexString(s))
            {
                var hexLen = s.Length;

                if (hexLen == 32)
                {
                    // Checar LM vazio
                    if (s.Equals("AAD3B435B51404EEAAD3B435B51404EE", StringComparison.OrdinalIgnoreCase))
                        return EncryptionType.LM;

                    // Heurística simples: se todo uppercase e [0-9A-F] => NTLM, senão MD5
                    bool allUpperHex = Regex.IsMatch(s, @"^[0-9A-F]{32}$");
                    return allUpperHex ? EncryptionType.NTLM : EncryptionType.MD5;
                }

                if (hexLen == 40) return EncryptionType.SHA1;
                if (hexLen == 64) return EncryptionType.SHA256;
                if (hexLen == 128) return EncryptionType.SHA512;
            }

            return EncryptionType.Unknown;
        }

        public static bool IsHexString(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return false;

            s = CleanHex(s);

            if (s.Length == 0 || (s.Length % 2) != 0)
                return false;

            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                bool isHex = (c >= '0' && c <= '9') ||
                             (c >= 'a' && c <= 'f') ||
                             (c >= 'A' && c <= 'F');
                if (!isHex) return false;
            }
            return true;
        }

        public static bool IsBase64(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return false;

            s = s.Trim();

            // Comprimento deve ser múltiplo de 4 (pode vir com padding).
            if ((s.Length % 4) != 0)
                return false;

            return TryBase64Decode(s, out _);
        }

        public static string HexToBase64(string hex)
        {
            if (string.IsNullOrWhiteSpace(hex))
                throw new FormatException("Entrada vazia para conversão Hex->Base64.");

            hex = CleanHex(hex);

            if ((hex.Length % 2) != 0)
                throw new FormatException("Comprimento hexadecimal inválido (ímpar).");

            int len = hex.Length / 2;
            byte[] bytes = new byte[len];

            for (int i = 0; i < len; i++)
            {
                int hi = ParseNybble(hex[2 * i]) << 4;
                int lo = ParseNybble(hex[2 * i + 1]);
                int val = hi | lo;
                if (val < 0) throw new FormatException($"Hex inválido na posição {2 * i}.");
                bytes[i] = (byte)val;
            }

            return Convert.ToBase64String(bytes);
        }

        public static string Base64ToHex(string base64)
        {
            if (!TryBase64Decode(base64?.Trim() ?? string.Empty, out var bytes))
                throw new FormatException("String Base64 inválida.");

            var sb = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes)
                sb.AppendFormat("{0:X2}", b);

            return sb.ToString();
        }

        public void RunSampleTests(IEnumerable<string> testCases)
        {
            if (testCases is null) return;

            foreach (var testCase in testCases)
            {
                string preview = testCase is null ? "<null>" :
                    testCase.Substring(0, Math.Min(testCase.Length, 50));
                Console.WriteLine($"Input: {preview}...");

                var result = AnalyzeEncryption(testCase ?? string.Empty);
                Console.WriteLine($"  -> Tipo Identificado: {result.Type}");
                Console.WriteLine($"  -> É Hexadecimal: {result.IsHexadecimal}");
                Console.WriteLine($"  -> Tamanho: {result.Length} caracteres");

                if (result.IsHexadecimal && result.Type != EncryptionType.Unknown)
                {
                    try
                    {
                        string base64 = HexToBase64(testCase!);
                        string base64Preview = base64.Substring(0, Math.Min(base64.Length, 50));
                        Console.WriteLine($"  -> Convertido para Base64: {base64Preview}...");
                    }
                    catch (FormatException e)
                    {
                        Console.WriteLine($"  -> Erro na conversão Hex->Base64: {e.Message}");
                    }
                }

                if (!result.IsHexadecimal && IsBase64(testCase ?? string.Empty))
                {
                    try
                    {
                        string hex = Base64ToHex(testCase!);
                        string hexPreview = hex.Substring(0, Math.Min(hex.Length, 50));
                        Console.WriteLine($"  -> Convertido para Hexadecimal: {hexPreview}...");
                    }
                    catch (FormatException e)
                    {
                        Console.WriteLine($"  -> Erro na conversão Base64->Hex: {e.Message}");
                    }
                }

                Console.WriteLine();
            }
        }

        public void RunInteractiveDemo()
        {
            Console.WriteLine("=== Modo Interativo ===");
            Console.WriteLine("Digite um hash ou valor encriptado para análise (ou 'sair' para terminar):");

            string? input;
            while ((input = Console.ReadLine()) != null && input.Trim().ToLowerInvariant() != "sair")
            {
                if (string.IsNullOrWhiteSpace(input))
                    continue;

                var result = AnalyzeEncryption(input);
                Console.WriteLine($"Tipo: {result.Type}");
                Console.WriteLine($"Hexadecimal: {result.IsHexadecimal}");
                Console.WriteLine($"Tamanho: {result.Length}");

                if (result.IsHexadecimal)
                {
                    try
                    {
                        Console.WriteLine($"Hex->Base64: {HexToBase64(input)}");
                    }
                    catch (FormatException e)
                    {
                        Console.WriteLine($"Erro Hex->Base64: {e.Message}");
                    }
                }
                else if (IsBase64(input))
                {
                    try
                    {
                        Console.WriteLine($"Base64->Hex: {Base64ToHex(input)}");
                    }
                    catch (FormatException e)
                    {
                        Console.WriteLine($"Erro Base64->Hex: {e.Message}");
                    }
                }

                Console.WriteLine();
            }
        }

        private static string CleanHex(string s)
        {
            s = s.Trim();
            // Remover prefixos e separadores comuns
            s = s.Replace("0x", "", StringComparison.OrdinalIgnoreCase)
                 .Replace(" ", "", StringComparison.Ordinal)
                 .Replace(":", "", StringComparison.Ordinal);
            return s;
        }

        private static bool TryBase64Decode(string s, out byte[] bytes)
        {
            s = s.Trim();

            try
            {
                // Usar Convert.TryFromBase64String (mais eficiente, evita alocação extra)
                bytes = Convert.FromBase64String(s);
                return true;
            }
            catch
            {
                bytes = Array.Empty<byte>();
                return false;
            }
        }

        private static int ParseNybble(char c)
        {
            if (c >= '0' && c <= '9') return c - '0';
            if (c >= 'a' && c <= 'f') return 10 + (c - 'a');
            if (c >= 'A' && c <= 'F') return 10 + (c - 'A');
            return -1;
        }
    }

    public readonly struct EncryptionAnalysis
    {
        public EncryptionType Type { get; }
        public bool IsHexadecimal { get; }
        public int Length { get; }

        public EncryptionAnalysis(EncryptionType type, bool isHexadecimal, int length)
        {
            Type = type;
            IsHexadecimal = isHexadecimal;
            Length = length;
        }
    }
}
