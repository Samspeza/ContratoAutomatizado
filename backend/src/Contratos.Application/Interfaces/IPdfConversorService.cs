namespace Contratos.Application.Interfaces;

public interface IPdfConversorService
{
    byte[] ConverterDocxParaPdf(byte[] conteudoDocx);
}