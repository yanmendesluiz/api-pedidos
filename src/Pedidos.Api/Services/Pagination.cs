namespace Pedidos.Api.Services;

public static class Pagination
{
    public static int NormalizePage(int page) => page < 1 ? 1 : page;
    public static int NormalizePageSize(int pageSize) => pageSize switch
    {
        < 1 => 10,
        > 100 => 100,
        _ => pageSize
    };
}
