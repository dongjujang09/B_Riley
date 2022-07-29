using Microsoft.EntityFrameworkCore;
using B_Riley.BankingApp.Models.Entities;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using B_Riley.BankingApp.Models.Entities.Base;

namespace B_Riley.BankingApp.Data
{
    public class BankingAppContext : DbContext
    {
        public BankingAppContext(DbContextOptions<BankingAppContext> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<Transfer>()
                .HasOne(t => t.FromAccount)
                .WithOne()
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder
                .Entity<Transfer>()
                .HasOne(t => t.ToAccount)
                .WithOne()
                .OnDelete(DeleteBehavior.NoAction);
        }


        public DbSet<Account> Accounts { get; set; }

        public DbSet<Transfer> Transfers { get; set; }
    }
}
