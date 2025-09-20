using System;
using DesencriptacaoDeHexasDoRegWindowns.Cli;
using DesencriptacaoDeHexasDoRegWindowns.Models;
using DesencriptacaoDeHexasDoRegWindowns.Services;
using DesencriptacaoDeHexasDoRegWindowns.Utilities;

public class Program
{
  public static void Main(string[] args)
  {
    Console.WriteLine("=== Ferramenta Avançada de Descriptografia DPAPI ===");
    Console.WriteLine("Versão 2.0 - Suporte flexível para valores hexadecimais e registro do Windows");
    Console.WriteLine();

    var options = ArgumentParser.Parse(args);

    var dpapi = new DpapiDecryptionService();
    var registry = new RegistryService();

    switch (options.Mode)
    {
      case AppMode.Help:
        ShowUsage();
        break;
      case AppMode.Interactive:
        new InteractiveCli(dpapi, registry).Run();
        break;
      case AppMode.Hex:
        if (!string.IsNullOrWhiteSpace(options.HexValue))
        {
          Console.WriteLine("🔧 Modo: Descriptografar valor hexadecimal fornecido");
          if (HexUtils.TryParseHex(options.HexValue, out var bytes))
          {
            dpapi.DecryptAndShow(bytes, options.Verbose);
          }
          else
          {
            Console.WriteLine("❌ Valor hexadecimal inválido.");
          }
        }
        else
        {
          Console.WriteLine("❌ Nenhum valor hexadecimal informado.");
          ShowUsage();
        }
        break;
      case AppMode.Registry:
        if (!string.IsNullOrWhiteSpace(options.RegistryPath) && !string.IsNullOrWhiteSpace(options.RegistryValueName))
        {
          Console.WriteLine("🔧 Modo: Ler e descriptografar valor do Registro do Windows");
          var data = registry.ReadRegistryValue(options.RegistryPath!, options.RegistryValueName!, options.Verbose);
          if (data != null)
          {
            dpapi.DecryptAndShow(data, options.Verbose);
          }
        }
        else
        {
          Console.WriteLine("❌ Argumentos insuficientes para ler o registro.");
          ShowUsage();
        }
        break;
      default:
        ShowUsage();
        new InteractiveCli(dpapi, registry).Run();
        break;
    }
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