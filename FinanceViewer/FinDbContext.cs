using FinanceViewer.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceViewer;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

public class FinDbContext : IdentityDbContext
{
    public DbSet<ApiKey> ApiKeys => Set<ApiKey>();
    
    public FinDbContext(DbContextOptions<FinDbContext> options)
        : base(options) { }
}