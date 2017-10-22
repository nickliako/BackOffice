using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Models
{
    public partial class Qualco4_DBFirstContext : DbContext
    {
        public virtual DbSet<Address> Address { get; set; }
        public virtual DbSet<Bill> Bill { get; set; }
        public virtual DbSet<User> User { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(@"Server=LENOVO\SQLEXPRESS;Database=Qualco4-DBFirst;User Id=xxxx;Password=xxxxx;Trusted_Connection=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Address>(entity =>
            {
                entity.Property(e => e.AddressId).HasColumnName("Address Id");

                entity.Property(e => e.AddressName)
                    .IsRequired()
                    .HasColumnName("Address Name")
                    .HasMaxLength(50);

                entity.Property(e => e.County)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<Bill>(entity =>
            {
                entity.HasIndex(e => e.BillId)
                    .HasName("IX_Bill")
                    .IsUnique();

                entity.Property(e => e.BillDescription)
                    .IsRequired()
                    .HasColumnName("Bill Description")
                    .HasMaxLength(50);

                entity.Property(e => e.BillId)
                    .IsRequired()
                    .HasColumnName("Bill Id")
                    .HasMaxLength(50);

                entity.Property(e => e.DateDue).HasColumnType("datetime");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Bill)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Bill_User");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.Vat)
                    .HasName("IX_User")
                    .IsUnique();

                entity.Property(e => e.EMail)
                    .IsRequired()
                    .HasColumnName("E-mail")
                    .HasMaxLength(150);

                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(e => e.LastLogOnDate).HasColumnType("datetime");

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(e => e.Vat).HasColumnName("VAT");

                entity.HasOne(d => d.Address)
                    .WithMany(p => p.User)
                    .HasForeignKey(d => d.AddressId)
                    .HasConstraintName("FK_User_Address");
            });
        }
    }
}
