namespace Pedidos.Api.Utils;

public static class DateTimeHelper
{
    private static readonly TimeZoneInfo SaoPauloTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

    public static DateTime NowUtc() => DateTime.UtcNow;

    public static DateTime ToSaoPaulo(DateTime utc)
    {
        var value = utc.Kind == DateTimeKind.Utc ? utc : DateTime.SpecifyKind(utc, DateTimeKind.Utc);
        return TimeZoneInfo.ConvertTimeFromUtc(value, SaoPauloTimeZone);
    }
}
