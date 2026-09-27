using System.Globalization;

namespace Contratos.Infrastructure.Documents;

public static class NumeroPorExtenso
{
    private static readonly string[] Unidades =
        { "zero", "um", "dois", "três", "quatro", "cinco", "seis", "sete", "oito", "nove" };

    private static readonly string[] DezANoventaENove =
    {
        "dez", "onze", "doze", "treze", "quatorze", "quinze", "dezesseis",
        "dezessete", "dezoito", "dezenove"
    };

    private static readonly string[] Dezenas =
        { "", "", "vinte", "trinta", "quarenta", "cinquenta", "sessenta", "setenta", "oitenta", "noventa" };

    private static readonly string[] Centenas =
    {
        "", "cem", "duzentos", "trezentos", "quatrocentos", "quinhentos",
        "seiscentos", "setecentos", "oitocentos", "novecentos"
    };

    public static string ExtensoInteiro(int numero)
    {
        if (numero == 0) return Unidades[0];
        if (numero < 0 || numero > 999) throw new ArgumentOutOfRangeException(nameof(numero));

        if (numero == 100) return "cem";

        var partes = new List<string>();

        if (numero >= 100)
        {
            partes.Add(Centenas[numero / 100]);
            numero %= 100;
        }

        if (numero >= 20)
        {
            partes.Add(Dezenas[numero / 10]);
            numero %= 10;
            if (numero > 0) partes.Add(Unidades[numero]);
        }
        else if (numero >= 10)
        {
            partes.Add(DezANoventaENove[numero - 10]);
        }
        else if (numero > 0)
        {
            partes.Add(Unidades[numero]);
        }

        return string.Join(" e ", partes);
    }

    public static string ExtensoPercentual(decimal percentual)
    {
        var parteInteira = (int)Math.Truncate(percentual);
        var parteDecimal = Math.Round((percentual - parteInteira) * 100, 0);

        if (parteDecimal == 0)
            return ExtensoInteiro(parteInteira) + " por cento";

        return $"{ExtensoInteiro(parteInteira)} vírgula {ExtensoInteiro((int)parteDecimal)} por cento";
    }

    public static string FormatarPercentual(decimal percentual)
    {
        var valorFormatado = percentual.ToString("0.##", CultureInfo.InvariantCulture);
        return $"{valorFormatado}% ({ExtensoPercentual(percentual)})";
    }

    public static string FormatarParcelas(int quantidade)
    {
        return $"{quantidade} ({ExtensoInteiro(quantidade)})";
    }
}