namespace Contratos.Domain.ValueObjects;

public sealed class Cpf
{
    public string Numero { get; }

    private Cpf(string numero)
    {
        Numero = numero;
    }

    public static bool EhValido(string valorInformado)
    {
        var apenasNumeros = new string((valorInformado ?? string.Empty).Where(char.IsDigit).ToArray());

        if (apenasNumeros.Length != 11) return false;
        if (apenasNumeros.Distinct().Count() == 1) return false;

        return DigitosVerificadoresConferem(apenasNumeros);
    }

    public static Cpf Criar(string valorInformado)
    {
        if (!EhValido(valorInformado))
            throw new ArgumentException("CPF inválido.", nameof(valorInformado));

        return new Cpf(new string(valorInformado.Where(char.IsDigit).ToArray()));
    }

    private static bool DigitosVerificadoresConferem(string numero)
    {
        var primeiroDigito = CalcularDigito(numero[..9], 10);
        var segundoDigito = CalcularDigito(numero[..9] + primeiroDigito, 11);

        return numero[9] - '0' == primeiroDigito && numero[10] - '0' == segundoDigito;
    }

    private static int CalcularDigito(string baseNumero, int fatorInicial)
    {
        var soma = 0;
        for (var i = 0; i < baseNumero.Length; i++)
            soma += (baseNumero[i] - '0') * (fatorInicial - i);

        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }

    public override string ToString() => Numero;
}