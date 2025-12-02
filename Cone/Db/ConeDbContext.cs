using Cone.Db.Models;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Cone.Db;

public class ConeDbContext(DbContextOptions<ConeDbContext> options) : DbContext(options), IConeDbContext, IDataProtectionKeyContext
{
    public DbSet<AdminUsers> AdminUsers { get; set; }
    public DbSet<StudentGroups> StudentGroups { get; set; }
    public DbSet<Assignments> Assignments { get; set; }
    public DbSet<StudentGroupAssignmentsProgress> StudentGroupAssignmentsProgress { get; set; }
    public DbSet<Tickets> Tickets { get; set; }
    public DbSet<TicketAdminUsers> TicketAdminUsers { get; set; }
    public DbSet<TicketAssignments> TicketAssignments { get; set; }
    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }
    
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder
            .Properties<Enum>()
            .HaveConversion<string>();
    }
}