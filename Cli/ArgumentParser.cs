using System;
using DesencriptacaoDeHexasDoRegWindowns.Models;

namespace DesencriptacaoDeHexasDoRegWindowns.Cli
{
    public static class ArgumentParser
    {
        public static AppOptions Parse(string[] args)
        {
            var options = new AppOptions();

            if (args.Length == 0)
            {
                options.Mode = AppMode.Interactive;
                return options;
            }

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i].ToLowerInvariant())
                {
                    case "--value":
                    case "-v":
                        if (i + 1 < args.Length) options.HexValue = args[++i];
                        options.Mode = AppMode.Hex;
                        break;
                    case "--path":
                    case "-p":
                        if (i + 1 < args.Length) options.RegistryPath = args[++i];
                        options.Mode = AppMode.Registry;
                        break;
                    case "--name":
                    case "-n":
                        if (i + 1 < args.Length) options.RegistryValueName = args[++i];
                        options.Mode = AppMode.Registry;
                        break;
                    case "--interactive":
                    case "-i":
                        options.Mode = AppMode.Interactive;
                        break;
                    case "--help":
                    case "-h":
                        options.Mode = AppMode.Help;
                        break;
                    case "--verbose":
                        options.Verbose = true;
                        break;
                    default:
                        Console.WriteLine($"⚠️  Argumento desconhecido: {args[i]}");
                        options.Mode = AppMode.Help;
                        break;
                }
            }

            return options;
        }
    }
}
