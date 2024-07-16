using Microsoft.EntityFrameworkCore;
using MotoRentalService.Domain.Entities;
using MotoRentalService.Domain.Enums;

namespace MotoRentalService.Infrastructure.Data
{
    public class MotoRentalDbContext(DbContextOptions<MotoRentalDbContext> options) : DbContext(options)
    {
        public DbSet<Moto> Motos { get; set; }
        public DbSet<DeliveryPerson> DeliveryPeople { get; set; }
        public DbSet<Rental> Rentals { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure DeliveryPerson's Cnh as an owned entity
            modelBuilder.Entity<DeliveryPerson>()
                        .OwnsOne(d => d.Cnh, c =>
                        {
                            c.Property(cnh => cnh.Number)
                             .HasColumnName("CnhNumber")
                             .IsRequired();

                            c.Property(cnh => cnh.Type)
                             .HasColumnName("CnhType")
                             .HasConversion(
                                 v => v.ToString(),
                                 v => (LicenseType)Enum.Parse(typeof(LicenseType), v))
                             .IsRequired();
                        });

            // Configure DeliveryPerson's Cnpj as an owned entity
            modelBuilder.Entity<DeliveryPerson>()
                        .OwnsOne(d => d.Cnpj, c =>
                        {
                            c.Property(cnpj => cnpj.Value)
                             .HasColumnName("Cnpj")
                             .IsRequired();
                        });

            // Configure the PlanType property of the Rental entity to use conversion between string and RentalPlanType
            modelBuilder.Entity<Rental>()
                        .Property(r => r.PlanType)
                        .HasColumnName("PlanType")
                        .HasConversion(
                            v => v.ToString(),
                            v => (RentalPlanType)Enum.Parse(typeof(RentalPlanType), v))
                        .IsRequired();

            base.OnModelCreating(modelBuilder);
        }
    }
}