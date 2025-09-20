using System;
using DesencriptacaoDeHexasDoRegWindowns.Services;
using DesencriptacaoDeHexasDoRegWindowns.Utilities;

namespace DesencriptacaoDeHexasDoRegWindowns.Cli
{
    public sealed class InteractiveCli
    {
        private readonly IDpapiDecryptionService _dpapi;
        private readonly IRegistryService _registry;

        public InteractiveCli(IDpapiDecryptionService dpapi, IRegistryService registry)
        {
            _dpapi = dpapi;
            _registry = registry;
        }

        public void Run()
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
                    Run();
                    break;
            }
        }

        private void InteractiveHexDecryption()
        {
            Console.WriteLine("\n--- Descriptografia de Valor Hexadecimal ---");
            Console.WriteLine("Cole o valor hexadecimal (pode conter hífens, espaços ou prefixos):");
            Console.Write("Valor: ");

            string? hexValue = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(hexValue))
            {
                if (HexUtils.TryParseHex(hexValue, out var bytes))
                {
                    _dpapi.DecryptAndShow(bytes, true);
                }
                else
                {
                    Console.WriteLine("❌ Valor hexadecimal inválido.");
                }
            }
            else
            {
                Console.WriteLine("❌ Valor vazio fornecido.");
            }

            Console.WriteLine("\nPressione qualquer tecla para continuar...");
            Console.ReadKey();
            Run();
        }

        private void InteractiveRegistryDecryption()
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
                var registryBytes = _registry.ReadRegistryValue(regPath!, regValueName!, true);
                if (registryBytes != null)
                {
                    _dpapi.DecryptAndShow(registryBytes, true);
                }
            }
            else
            {
                Console.WriteLine("❌ Caminho ou nome do valor vazio.");
            }

            Console.WriteLine("\nPressione qualquer tecla para continuar...");
            Console.ReadKey();
            Run();
        }
    }
}
