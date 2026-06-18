using Microsoft.EntityFrameworkCore;
using Pedidos.Api.Domain;

namespace Pedidos.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Produto> Produtos => Set<Produto>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<PedidoItem> PedidoItens => Set<PedidoItem>();
    public DbSet<PedidoStatusHistorico> PedidoStatusHistoricos => Set<PedidoStatusHistorico>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Nome).HasMaxLength(160).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(220).IsRequired();
            entity.Property(x => x.Documento).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => new { x.Email, x.Ativo });
            entity.HasIndex(x => new { x.Documento, x.Ativo });
        });

        modelBuilder.Entity<Produto>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Nome).HasMaxLength(160).IsRequired();
            entity.Property(x => x.Descricao).HasMaxLength(500);
            entity.Property(x => x.Preco).HasPrecision(18, 2);
            entity.ToTable(t => t.HasCheckConstraint("CK_Produtos_Preco_Positive", "Preco > 0"));
            entity.ToTable(t => t.HasCheckConstraint("CK_Produtos_Estoque_NaoNegativo", "EstoqueDisponivel >= 0"));
        });

        modelBuilder.Entity<Pedido>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ValorTotal).HasPrecision(18, 2);
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            entity.HasOne(x => x.Cliente).WithMany(x => x.Pedidos).HasForeignKey(x => x.ClienteId);
        });

        modelBuilder.Entity<PedidoItem>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.PrecoUnitario).HasPrecision(18, 2);
            entity.Property(x => x.ValorTotalItem).HasPrecision(18, 2);
            entity.HasOne(x => x.Pedido).WithMany(x => x.Itens).HasForeignKey(x => x.PedidoId);
            entity.HasOne(x => x.Produto).WithMany(x => x.ItensPedido).HasForeignKey(x => x.ProdutoId);
        });

        modelBuilder.Entity<PedidoStatusHistorico>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.StatusAnterior).HasConversion<string>().HasMaxLength(20);
            entity.Property(x => x.NovoStatus).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(x => x.Motivo).HasMaxLength(500);
            entity.HasOne(x => x.Pedido).WithMany(x => x.HistoricoStatus).HasForeignKey(x => x.PedidoId);
        });
    }
}
