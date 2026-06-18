using System.Text.RegularExpressions;

namespace Pedidos.Api.Utils;

public static class DocumentValidator
{
    public static string OnlyDigits(string value) => Regex.Replace(value ?? string.Empty, "[^0-9]", string.Empty);

    public static bool IsValidWhenCpfOrCnpj(string documento)
    {
        var digits = OnlyDigits(documento);
        return digits.Length switch
        {
            11 => IsCpf(digits),
            14 => IsCnpj(digits),
            _ => !string.IsNullOrWhiteSpace(documento)
        };
    }

    private static bool IsCpf(string cpf)
    {
        if (cpf.Length != 11 || cpf.Distinct().Count() == 1) return false;
        int[] mult1 = [10, 9, 8, 7, 6, 5, 4, 3, 2];
        int[] mult2 = [11, 10, 9, 8, 7, 6, 5, 4, 3, 2];
        var temp = cpf[..9];
        var sum = temp.Select((t, i) => (t - '0') * mult1[i]).Sum();
        var rem = sum % 11;
        var digit = rem < 2 ? 0 : 11 - rem;
        temp += digit;
        sum = temp.Select((t, i) => (t - '0') * mult2[i]).Sum();
        rem = sum % 11;
        digit = rem < 2 ? 0 : 11 - rem;
        return cpf.EndsWith(digit.ToString());
    }

    private static bool IsCnpj(string cnpj)
    {
        if (cnpj.Length != 14 || cnpj.Distinct().Count() == 1) return false;
        int[] mult1 = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
        int[] mult2 = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
        var temp = cnpj[..12];
        var sum = temp.Select((t, i) => (t - '0') * mult1[i]).Sum();
        var rem = sum % 11;
        var digit = rem < 2 ? 0 : 11 - rem;
        temp += digit;
        sum = temp.Select((t, i) => (t - '0') * mult2[i]).Sum();
        rem = sum % 11;
        digit = rem < 2 ? 0 : 11 - rem;
        return cnpj.EndsWith(digit.ToString());
    }
}
