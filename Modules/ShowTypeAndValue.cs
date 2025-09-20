namespace DesencriptacaoDeHexasDoRegWindowns.Modules
{
	internal class ShowTypeAndValue
	{
		// Mostra o tipo e valor do dado retornado de "AskWhichKeyMethod"
		public static void ShowTypeAndValueMethod(object valor)
		{
			if (valor == null)
			{
				Console.WriteLine("O valor retornado � nulo.");
				return;
			}

			Type tipoValor = valor.GetType();
			Console.WriteLine($"Tipo do valor: {tipoValor}");

			if (tipoValor == typeof(string))
			{
				Console.WriteLine($"Valor (string): {valor}");
			}
			else if (tipoValor == typeof(int))
			{
				Console.WriteLine($"Valor (int): {valor}");
			}
			else if (tipoValor == typeof(byte[]))
			{
				byte[] byteArray = (byte[])valor;
				string hexString = BitConverter.ToString(byteArray).Replace("-", " ");
				Console.WriteLine($"Valor (byte[]): {hexString}");
			}
			else if (tipoValor == typeof(long))
			{
				Console.WriteLine($"Valor (long): {valor}");
			}
			else if (tipoValor == typeof(string[]))
			{
				string[] stringArray = (string[])valor;
				Console.WriteLine("Valor (string[]):");
				foreach (string str in stringArray)
				{
					Console.WriteLine($"- {str}");
				}
			}
			else
			{
				Console.WriteLine($"Tipo de valor n�o tratado: {tipoValor}. Valor: {valor}");
			}
		}

		// Sanitiza o valor para exibi��o
		public static string SanitizeValueForDisplay(object valor)
		{
			if (valor == null)
			{
				return "null";
			}
			Type tipoValor = valor.GetType();
			if (tipoValor == typeof(string))
			{
				return (string)valor;
			}
			else if (tipoValor == typeof(int) || tipoValor == typeof(long))
			{
				return valor?.ToString() ?? string.Empty;
			}
			else if (tipoValor == typeof(byte[]))
			{
				byte[] byteArray = (byte[])valor;
				return BitConverter.ToString(byteArray).Replace("-", " ");
			}
			else if (tipoValor == typeof(string[]))
			{
				string[] stringArray = (string[])valor;
				return string.Join(", ", stringArray);
			}
			else
			{
				return $"Tipo de valor n�o tratado: {tipoValor}. Valor: {valor}";
			}
		}

	}
}
