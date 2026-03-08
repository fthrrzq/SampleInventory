using System.Collections.Generic;
using System.Reflection.Emit;
using Microsoft.EntityFrameworkCore;
namespace SampleInventory.Database
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Warehouse> Warehouses { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<StockMovement> StockMovements { get; set; }
        public DbSet<BarInventory> BarInventories { get; set; }
        public DbSet<StockEntry> StockEntries { get; set; }
        public DbSet<StockEntryItem> StockEntryItems { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<StockEntry>()
           .HasOne(e => e.Warehouse)
           .WithMany(w => w.StockEntries)
           .HasForeignKey(e => e.WarehouseId)
           .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StockEntryItem>()
                .HasOne(i => i.StockEntry)
                .WithMany(e => e.Items)
                .HasForeignKey(i => i.StockEntryId)
                .OnDelete(DeleteBehavior.Cascade);

            // Add index for faster lookup
            modelBuilder.Entity<StockEntry>()
                .HasIndex(e => e.EntryNumber)
                .IsUnique();


            // Warehouse configuration
            modelBuilder.Entity<Warehouse>()
                    .HasOne(w => w.ParentWarehouse)
                    .WithMany(w => w.ChildWarehouses)
                    .HasForeignKey(w => w.ParentWarehouseId)
                    .OnDelete(DeleteBehavior.Restrict);

            // Inventory configuration
            modelBuilder.Entity<Inventory>()
                .HasIndex(i => new { i.WarehouseId, i.ProductId })
                .IsUnique();

            // BarInventory configuration
            modelBuilder.Entity<BarInventory>()
                .HasIndex(b => new { b.WarehouseId, b.ProductId, b.BarLocation })
                .IsUnique();

            // StockMovement configuration
            modelBuilder.Entity<StockMovement>()
                .HasOne(s => s.SourceWarehouse)
                .WithMany(w => w.SourceMovements)
                .HasForeignKey(s => s.SourceWarehouseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StockMovement>()
                .HasOne(s => s.DestinationWarehouse)
                .WithMany(w => w.DestinationMovements)
                .HasForeignKey(s => s.DestinationWarehouseId)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed initial data untuk struktur warehouse
            modelBuilder.Entity<Warehouse>().HasData(
                new Warehouse { Id = 1, Code = "GP", Name = "Gudang Pusat", Type = WarehouseType.Pusat, Location = "Pusat", IsActive = true, CreatedAt = DateTime.Now },

                // Gudang Outlet utama
                new Warehouse { Id = 2, Code = "GO", Name = "Gudang Outlet", Type = WarehouseType.Outlet, ParentWarehouseId = 1, Location = "Outlet", IsActive = true, CreatedAt = DateTime.Now },

                // Hawaii
                new Warehouse { Id = 3, Code = "HW", Name = "Hawaii", Type = WarehouseType.Outlet, ParentWarehouseId = 2, Location = "Hawaii", IsActive = true, CreatedAt = DateTime.Now }
            );
        }
    }
}
