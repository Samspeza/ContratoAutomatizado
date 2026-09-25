using Contratos.Domain.Exceptions;

namespace Contratos.Domain.ValueObjects;

public sealed class Cnpj
{
    public string Numero { get; }

    private Cnpj(string numero)
    {
        Numero = numero;
    }

    public static Cnpj Criar(string valorInformado)
    {
        if (string.IsNullOrWhiteSpace(valorInformado))
            throw new CnpjInvalidoException("CNPJ não informado.");

        var apenasNumeros = new string(valorInformado.Where(char.IsDigit).ToArray());

        if (apenasNumeros.Length != 14)
            throw new CnpjInvalidoException("CNPJ inválido.");

        if (TodosDigitosIguais(apenasNumeros))
            throw new CnpjInvalidoException("CNPJ inválido.");

        if (!DigitosVerificadoresConferem(apenasNumeros))
            throw new CnpjInvalidoException("CNPJ inválido.");

        return new Cnpj(apenasNumeros);
    }

    private static bool TodosDigitosIguais(string numero) =>
        numero.Distinct().Count() == 1;

    private static bool DigitosVerificadoresConferem(string numero)
    {
        int[] multiplicadoresPrimeiroDigito = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        int[] multiplicadoresSegundoDigito = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

        var primeiroDigito = CalcularDigito(numero[..12], multiplicadoresPrimeiroDigito);
        var segundoDigito = CalcularDigito(numero[..12] + primeiroDigito, multiplicadoresSegundoDigito);

        return numero[12] - '0' == primeiroDigito && numero[13] - '0' == segundoDigito;
    }

    private static int CalcularDigito(string numeroParcial, int[] multiplicadores)
    {
        var soma = numeroParcial.Select((c, i) => (c - '0') * multiplicadores[i]).Sum();
        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }

    public string Formatado =>
        $"{Numero[..2]}.{Numero[2..5]}.{Numero[5..8]}/{Numero[8..12]}-{Numero[12..14]}";

    public override string ToString() => Numero;
}