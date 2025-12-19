
using Microsoft.AspNetCore.Identity;

namespace FinanceViewer.Models;



public class ApiKey
{
    
    
    public int Id { get; set; }

    public string KeyHash { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string UserId { get; set; } = null!;
    public IdentityUser User { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
}