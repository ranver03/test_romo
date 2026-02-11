using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using romo.API.Models.Entities;

namespace romo.API.Data
{
    public class FacturacionContext : DbContext
    {
        // El nombre "FacturacionDB" debe ser igual al que pusiste en el Web.config
        public FacturacionContext() : base("name=romo") { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Factura> Facturas { get; set; }
        public DbSet<FacturaDetalle> FacturaDetalles { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Evita que Entity Framework intente pluralizar los nombres de las tablas
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }
    }
}