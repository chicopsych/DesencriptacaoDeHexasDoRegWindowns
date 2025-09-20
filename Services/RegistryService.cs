using System;
using Microsoft.Win32;

namespace DesencriptacaoDeHexasDoRegWindowns.Services
{
    public interface IRegistryService
    {
        byte[]? ReadRegistryValue(string path, string name, bool verbose = false);
    }

    public sealed class RegistryService : IRegistryService
    {
        public byte[]? ReadRegistryValue(string path, string name, bool verbose = false)
        {
            if (!OperatingSystem.IsWindows())
            {
                Console.WriteLine("❌ A leitura do registro só é suportada no Windows.");
                return null;
            }

            try
            {
                if (verbose)
                {
                    Console.WriteLine($"📂 Caminho: {path}");
                    Console.WriteLine($"🏷️  Nome do valor: {name}");
                }

                object? value = Registry.GetValue(path, name, null);

                if (value is byte[] byteValue)
                {
                    if (verbose)
                    {
                        Console.WriteLine($"✅ Valor lido com sucesso ({byteValue.Length} bytes)");
                    }
                    return byteValue;
                }

                if (value != null)
                {
                    Console.WriteLine($"⚠️  O valor '{name}' não é do tipo binário (byte[]), mas sim '{value.GetType().Name}'.");
                    Console.WriteLine($"   Valor: {value}");
                }
                else
                {
                    Console.WriteLine($"❌ O valor '{name}' não foi encontrado no caminho '{path}'.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erro ao ler o registro: {ex.Message}");
            }

            return null;
        }
    }
}
