using Microsoft.EntityFrameworkCore;

namespace FinanceViewer;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

public class FinDbContext : IdentityDbContext
{
    public FinDbContext(DbContextOptions<FinDbContext> options)
        : base(options) { }
}