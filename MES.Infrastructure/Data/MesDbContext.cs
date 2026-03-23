using MES.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace MES.Infrastructure.Data;

public class MesDbContext : DbContext
{
    public MesDbContext(DbContextOptions<MesDbContext> options) : base(options) { }

    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
    public DbSet<StepDefinition> StepDefinitions => Set<StepDefinition>();
    public DbSet<StepExecution> StepExecutions => Set<StepExecution>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Soft delete  (query otomatis exclude yang sudah dihapus)
        modelBuilder.Entity<WorkOrder>().HasQueryFilter(wo => wo.DeletedAt == null);

        // Seed master data step (untuk jaga flow MES)
        modelBuilder.Entity<StepDefinition>().HasData(
            new StepDefinition { Id = 1, Name = "Processing", Order = 1 },
            new StepDefinition { Id = 2, Name = "Assembly", Order = 2 },
            new StepDefinition { Id = 3, Name = "Quality Check", Order = 3 },
            new StepDefinition { Id = 4, Name = "Finishing", Order = 4 }
        );
    }
}