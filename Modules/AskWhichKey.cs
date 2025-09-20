namespace DesencriptacaoDeHexasDoRegWindowns.Modules
{
	internal class AskWhichKey
	{
		// Pergunta e ao usu�rio o caminho completo da chave/valor que deseja descripitografar
		public class AskWhichKeyClass
		{
			public static string AskWhichKeyMethod(string caminhoValor)
			{
				Console.WriteLine("Digite o caminho completo da chave/valor que deseja descripitografar:");
				var input = Console.ReadLine();

				// Verifica se o caminho � v�lido
				if (string.IsNullOrWhiteSpace(input))
				{
					Int16 contador = 0;
					while (contador < 3 && string.IsNullOrWhiteSpace(input))
					{
						Console.WriteLine("O caminho n�o pode ser vazio. Deseja tentar novamente? S/N");
						string? resposta = Console.ReadLine()?.Trim().ToUpperInvariant();

						if (resposta == "S")
						{
							Console.WriteLine("Digite o caminho completo da chave/valor que deseja descripitografar:");
							input = Console.ReadLine();
						}
						else if (resposta == "N")
						{
							Console.WriteLine("Encerrando o programa.");
							Environment.Exit(0);
						}
						else
						{
							Console.WriteLine("Resposta inv�lida. Digite S para sim ou N para n�o.");
						}

						contador++;
					}

					if (string.IsNullOrWhiteSpace(input))
					{
						Console.WriteLine("N�mero m�ximo de tentativas atingido. Encerrando o programa.");
						Environment.Exit(1);
					}
				}

				return input!;
			}
		}

	}
}
