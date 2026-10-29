using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Contratos.Infrastructure.Persistence;

// O SQLite guarda datas sem fuso e as devolve com Kind "Unspecified". Sem este conversor,
// a API serializaria "2026-10-28T22:00:00" (sem Z) e o navegador interpretaria como horário local.
public sealed class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
{
    public UtcDateTimeConverter()
        : base(
            valor => valor.ToUniversalTime(),
            valor => DateTime.SpecifyKind(valor, DateTimeKind.Utc))
    {
    }
}

public sealed class UtcNullableDateTimeConverter : ValueConverter<DateTime?, DateTime?>
{
    public UtcNullableDateTimeConverter()
        : base(
            valor => valor.HasValue ? valor.Value.ToUniversalTime() : valor,
            valor => valor.HasValue ? DateTime.SpecifyKind(valor.Value, DateTimeKind.Utc) : valor)
    {
    }
}