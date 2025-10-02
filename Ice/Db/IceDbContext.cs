using Ice.Db.Models;
using Microsoft.EntityFrameworkCore;

namespace Ice.Db;

public class IceDbContext(DbContextOptions<IceDbContext> options) : DbContext(options), IIceDbContext
{
    public DbSet<AdminUsers> AdminUsers { get; set; }
    public DbSet<StudentGroups> StudentGroups { get; set; }
    public DbSet<Assignments> Assignments { get; set; }
    public DbSet<StudentGroupAssignmentsProgress> StudentGroupAssignmentsProgress { get; set; }
    public DbSet<Tickets> Tickets { get; set; }
    public DbSet<TicketAdminUsers> TicketAdminUsers { get; set; }
    public DbSet<TicketAssignments> TicketAssignments { get; set; }
    
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder
            .Properties<Enum>()
            .HaveConversion<string>();
    }
}