using System;
using System.Security.Cryptography;
using System.Text;

namespace DesencriptacaoDeHexasDoRegWindowns.Services
{
    public interface IDpapiDecryptionService
    {
        void DecryptAndShow(byte[] encryptedBytes, bool verbose = false);
    }

    public sealed class DpapiDecryptionService : IDpapiDecryptionService
    {
        public void DecryptAndShow(byte[] encryptedBytes, bool verbose = false)
        {
            if (!OperatingSystem.IsWindows())
            {
                Console.WriteLine("⚠️  A descriptografia DPAPI só é suportada no Windows.");
                Console.WriteLine($"📄 Valor em Hex: {BitConverter.ToString(encryptedBytes)}");
                Console.WriteLine($"📄 Valor em Base64: {Convert.ToBase64String(encryptedBytes)}");
                return;
            }

            try
            {
                if (verbose)
                {
                    Console.WriteLine($"🔐 Valor criptografado (Hex): {BitConverter.ToString(encryptedBytes).Replace("-", " ")}");
                    Console.WriteLine($"🔐 Valor criptografado (Base64): {Convert.ToBase64String(encryptedBytes)}");
                }

                byte[] decryptedBytes = ProtectedData.Unprotect(encryptedBytes, null, DataProtectionScope.CurrentUser);

                string[] encodings = { "Unicode", "UTF8", "ASCII" };

                Console.WriteLine("\n🎉 SUCESSO NA DESCRIPTOGRAFIA!");
                Console.WriteLine("📋 Resultados em diferentes encodings:");

                foreach (string encodingName in encodings)
                {
                    try
                    {
                        string result = encodingName switch
                        {
                            "Unicode" => Encoding.Unicode.GetString(decryptedBytes),
                            "UTF8" => Encoding.UTF8.GetString(decryptedBytes),
                            "ASCII" => Encoding.ASCII.GetString(decryptedBytes),
                            _ => string.Empty
                        };

                        Console.WriteLine($"  {encodingName}: {result}");
                    }
                    catch
                    {
                        Console.WriteLine($"  {encodingName}: [Erro na decodificação]");
                    }
                }
            }
            catch (CryptographicException)
            {
                Console.WriteLine("\n❌ FALHA NA DESCRIPTOGRAFIA");
                Console.WriteLine("💡 Motivo: Este programa precisa ser executado no mesmo computador");
                Console.WriteLine("   e com a mesma conta de usuário que criptografou os dados.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ ERRO INESPERADO: {ex.Message}");
            }
        }
    }
}
