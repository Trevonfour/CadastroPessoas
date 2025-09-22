// Dentro do projeto StefaniniCadastroPessoas.Tests
using Microsoft.EntityFrameworkCore;
using StefaniniCadastroPessoas.Data;
using System.Threading;
using System.Threading.Tasks;

public class TestApplicationDbContext : ApplicationDbContext
{
    public TestApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return base.SaveChangesAsync(true, cancellationToken);
    }
}