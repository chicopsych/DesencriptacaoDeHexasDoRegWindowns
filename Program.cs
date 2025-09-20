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



}