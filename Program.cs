using System;
using System.Text;
using System.Security.Cryptography;
using Microsoft.Win32;
using System.Text.RegularExpressions;

public class Program
{
  public static void Main(string[] args)
  {
    Console.WriteLine("=== Ferramenta Avançada de Descriptografia DPAPI ===");
    Console.WriteLine("Versão 2.0 - Suporte flexível para valores hexadecimais e registro do Windows");
    Console.WriteLine();

    if (args.Length == 0)
    {
      ShowUsage();
      RunInteractiveMode();
      return;
    }

    ProcessCommandLineArgs(args);
  }

  private static void ProcessCommandLineArgs(string[] args)
  {
    string? hexValue = null;
    string? regPath = null;
    string? regValueName = null;
    bool showHelp = false;
    bool verbose = false;

    // Parsear argumentos
    for (int i = 0; i < args.Length; i++)
    {
      switch (args[i].ToLower())
      {
        case "--value":
        case "-v":
          if (i + 1 < args.Length)
            hexValue = args[++i];
          break;
        case "--path":
        case "-p":
          if (i + 1 < args.Length)
            regPath = args[++i];
          break;
        case "--name":
        case "-n":
          if (i + 1 < args.Length)
            regValueName = args[++i];
          break;
        case "--help":
        case "-h":
          showHelp = true;
          break;
        case "--verbose":
          verbose = true;
          break;
        case "--interactive":
        case "-i":
          RunInteractiveMode();
          return;
        default:
          Console.WriteLine($"⚠️  Argumento desconhecido: {args[i]}");
          ShowUsage();
          return;
      }
    }

    if (showHelp)
    {
      ShowUsage();
      return;
    }
    // Executar ação baseada nos argumentos
    if (hexValue != null)
    {
      Console.WriteLine("🔧 Modo: Descriptografar valor hexadecimal fornecido");
      DecryptHexValue(hexValue, verbose);
    }
    else if (regPath != null && regValueName != null)
    {
      Console.WriteLine("🔧 Modo: Ler e descriptografar valor do Registro do Windows");
      DecryptRegistryValue(regPath, regValueName, verbose);
    }
    else
    {
      Console.WriteLine("❌ Argumentos insuficientes ou inválidos.");
      ShowUsage();
    }

  }

  private static void RunInteractiveMode()
  {
    Console.WriteLine("🎯 MODO INTERATIVO");
    Console.WriteLine("Escolha uma opção:");
    Console.WriteLine("1. Descriptografar valor hexadecimal");
    Console.WriteLine("2. Ler valor do registro do Windows");
    Console.WriteLine("3. Sair");
    Console.Write("\nOpção: ");

    string? choice = Console.ReadLine();

    switch (choice)
    {
      case "1":
        InteractiveHexDecryption();
        break;
      case "2":
        InteractiveRegistryDecryption();
        break;
      case "3":
        Console.WriteLine("👋 Saindo...");
        break;
      default:
        Console.WriteLine("❌ Opção inválida.");
        RunInteractiveMode();
        break;
    }
  }

  private static void InteractiveHexDecryption()
  {
    Console.WriteLine("\n--- Descriptografia de Valor Hexadecimal ---");
    Console.WriteLine("Cole o valor hexadecimal (pode conter hífens, espaços ou prefixos):");
    Console.Write("Valor: ");

    string? hexValue = Console.ReadLine();
    if (!string.IsNullOrWhiteSpace(hexValue))
    {
      DecryptHexValue(hexValue, true);
    }
    else
    {
      Console.WriteLine("❌ Valor vazio fornecido.");
    }

    Console.WriteLine("\nPressione qualquer tecla para continuar...");
    Console.ReadKey();
    RunInteractiveMode();
  }

  private static void InteractiveRegistryDecryption()
  {
    Console.WriteLine("\n--- Descriptografia de Valor do Registro ---");
    Console.WriteLine("Digite o caminho completo da chave do registro:");
    Console.WriteLine("Exemplo: HKEY_CURRENT_USER\\Software\\MyApp\\Settings");
    Console.Write("Caminho: ");

    string? regPath = Console.ReadLine();

    Console.WriteLine("\nDigite o nome do valor:");
    Console.Write("Nome: ");

    string? regValueName = Console.ReadLine();

    if (!string.IsNullOrWhiteSpace(regPath) && !string.IsNullOrWhiteSpace(regValueName))
    {
      DecryptRegistryValue(regPath, regValueName, true);
    }
    else
    {
      Console.WriteLine("❌ Caminho ou nome do valor vazio.");
    }

    Console.WriteLine("\nPressione qualquer tecla para continuar...");
    Console.ReadKey();
    RunInteractiveMode();
  }

  private static void DecryptRegistryValue(string regPath, string regValueName, bool verbose = false)
  {
    if (!OperatingSystem.IsWindows())
    {
      Console.WriteLine("❌ A leitura do registro só é suportada no Windows.");
      return;
    }

    try
    {
      if (verbose)
      {
        Console.WriteLine($"📂 Caminho: {regPath}");
        Console.WriteLine($"🏷️  Nome do valor: {regValueName}");
      }

      byte[]? registryBytes = ReadRegistryValue(regPath, regValueName);
      if (registryBytes != null)
      {
        if (verbose)
        {
          Console.WriteLine($"✅ Valor lido com sucesso ({registryBytes.Length} bytes)");
        }
        DecryptAndShow(registryBytes, verbose);
      }
      else
      {
        Console.WriteLine("❌ Não foi possível ler o valor do registro.");
      }
    }
    catch (Exception ex)
    {
      Console.WriteLine($"❌ Erro ao ler o registro: {ex.Message}");
    }
  }

  private static void DecryptAndShow(byte[] encryptedBytes, bool verbose = false)
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

      // Tentar descriptografar usando DPAPI
      byte[] decryptedBytes = ProtectedData.Unprotect(encryptedBytes, null, DataProtectionScope.CurrentUser);

      // Tentar diferentes encodings
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
            _ => ""
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

  private static byte[] HexStringToByteArray(string hex)
  {
    // Limpar a string hex
    hex = hex.Replace("-", "")
             .Replace(" ", "")
             .Replace("0x", "")
             .Replace(":", "")
             .Trim();

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

  private static byte[]? ReadRegistryValue(string path, string name)
  {
    if (!OperatingSystem.IsWindows())
    {
      return null;
    }

    object? value = Registry.GetValue(path, name, null);

    if (value is byte[] byteValue)
    {
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

    return null;
  }

  private static string AnalyzeEncryptionType(byte[] data)
  {
    if (data.Length < 4) return "Dados muito pequenos para análise";

    // Verificar assinatura DPAPI
    if (data[0] == 0x01 && data[1] == 0x00 && data[2] == 0x00 && data[3] == 0x00)
    {
      return "DPAPI (Data Protection API) - Assinatura detectada";
    }

    // Verificar outros padrões comuns
    if (data.Length == 16) return "Possível MD5 hash ou AES-128 key";
    if (data.Length == 20) return "Possível SHA1 hash";
    if (data.Length == 32) return "Possível SHA256 hash ou AES-256 key";

    return $"Tipo desconhecido ({data.Length} bytes)";
  }

  private static void ShowUsage()
  {
    Console.WriteLine("📖 USO:");
    Console.WriteLine("  dotnet run --value <valor_hexadecimal>");
    Console.WriteLine("    Descriptografa um valor hexadecimal (RegBinary) diretamente.");
    Console.WriteLine();
    Console.WriteLine("  dotnet run --path \"<caminho_do_registro>\" --name <nome_do_valor>");
    Console.WriteLine("    Lê um valor binário do registro do Windows e o descriptografa.");
    Console.WriteLine();
    Console.WriteLine("  dotnet run --interactive");
    Console.WriteLine("    Executa em modo interativo com menu.");
    Console.WriteLine();
    Console.WriteLine("🔧 OPÇÕES:");
    Console.WriteLine("  --verbose     Exibe informações detalhadas");
    Console.WriteLine("  --help, -h    Exibe esta ajuda");
    Console.WriteLine();
    Console.WriteLine("📝 EXEMPLOS:");
    Console.WriteLine("  dotnet run -v \"01-00-00-00-D0-8C-9D...\"");
    Console.WriteLine("  dotnet run -p \"HKEY_CURRENT_USER\\Software\\MyApp\\Credentials\" -n Password");
    Console.WriteLine("  dotnet run --interactive");
    Console.WriteLine();
  }

}