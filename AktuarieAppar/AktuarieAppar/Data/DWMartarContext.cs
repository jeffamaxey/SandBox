using Microsoft.EntityFrameworkCore;


namespace AktuarieAppar.Data
{
    public class DWMartarContext : DbContext
    {
        public DWMartarContext (DbContextOptions<DWMartarContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //EF demands primary keys
            modelBuilder.Entity<Models.NSS.vSwap>(entity => {
                entity.HasKey(c => new { c.PRICEDATE, c.ID, c.Name });
                entity.ToView("vMart_Simcorp_Dimension_Swaps", "amf");
            });

            //EF demands primary keys
            modelBuilder.Entity<Models.TAT.Avkastning>(entity => {
                entity.HasKey(c => new { c.ToDate, c.NodeName });
                entity.ToView("vTotalAvkastning", "amf");
            });
        }
        public DbSet<Models.NSS.vSwap> vSwap { get; set; }
        public DbSet<Models.TAT.Avkastning> vTotalAvkastning { get; set; }
    }
}
