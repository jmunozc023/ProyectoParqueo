using Microsoft.EntityFrameworkCore;
using ParqueoApp3.Models;
using System.Security.Cryptography;

namespace ParqueoApp3.Data
{
    public class AppDBContext : DbContext
    {
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Parqueo> Parqueos { get; set; }
        public DbSet<Espacio> Espacios { get; set; }
        public DbSet<Vehiculo> Vehiculos { get; set; }
        public DbSet<Asig_vehiculo> Asig_vehiculos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Usuario>(tb =>
            {
                tb.HasKey(col => col.id_usuario);
                tb.Property(col => col.id_usuario).UseIdentityColumn().ValueGeneratedOnAdd();

                tb.Property(col => col.nombre).HasMaxLength(50).IsRequired();
                tb.Property(col => col.apellido).HasMaxLength(50).IsRequired();
                tb.Property(col => col.correo).HasMaxLength(100).IsRequired();
                tb.Property(col => col.password).HasMaxLength(256).IsRequired().HasConversion(v => EncryptPassword(v),v => v); // No decryption needed for password
                tb.Property(col => col.role).HasMaxLength(50);
            });
            modelBuilder.Entity<Usuario>().ToTable("Usuario");
            modelBuilder.Entity<Parqueo>(tb =>
            {
                tb.HasKey(col => col.id_parqueo);
                tb.Property(col => col.id_parqueo).UseIdentityColumn().ValueGeneratedOnAdd();
                tb.Property(col => col.nombre_parqueo).HasMaxLength(50).IsRequired();
                tb.Property(col => col.ubicacion).HasMaxLength(50).IsRequired();
            });
            modelBuilder.Entity<Parqueo>().ToTable("Parqueo");
            modelBuilder.Entity<Espacio>(tb =>
            {
                tb.HasKey(col => col.id_espacio);
                tb.Property(col => col.id_espacio).UseIdentityColumn().ValueGeneratedOnAdd();
                tb.Property(col => col.tipo_espacio).HasMaxLength(50).IsRequired();
                tb.Property(col => col.disponibilidad);
                tb.HasOne<Parqueo>()
                  .WithMany()
                  .HasForeignKey("id_parqueo")
                  .IsRequired();
            });
            modelBuilder.Entity<Espacio>().ToTable("Espacio");
            modelBuilder.Entity<Vehiculo>(tb =>
            {
                tb.HasKey(col => col.id_vehiculo);
                tb.Property(col => col.id_vehiculo).UseIdentityColumn().ValueGeneratedOnAdd();
                tb.Property(col => col.placa).HasMaxLength(50).IsRequired();
                tb.Property(col => col.tipo_vehiculo).HasMaxLength(50).IsRequired();
                tb.HasOne<Usuario>()
                  .WithMany()
                  .HasForeignKey("id_usuario")
                  .IsRequired();
            });
            modelBuilder.Entity<Vehiculo>().ToTable("Vehiculo");
            modelBuilder.Entity<Asig_vehiculo>(tb =>
            {
                tb.HasKey(col => col.id_asig_vehiculo);
                tb.Property(col => col.id_asig_vehiculo).UseIdentityColumn().ValueGeneratedOnAdd();
                tb.Property(col => col.fecha_ingreso).IsRequired();
                tb.Property(col => col.fecha_salida);
                tb.HasOne<Vehiculo>()
                  .WithMany()
                  .HasForeignKey("id_vehiculo")
                  .IsRequired();
                tb.HasOne<Espacio>()
                  .WithMany()
                  .HasForeignKey("id_espacio")
                  .IsRequired();
            });
        }

        private string EncryptPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }
        
    }
}
