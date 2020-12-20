using Microsoft.EntityFrameworkCore;


namespace AktuarieAppar.Data
{
    public class DWAktuarieContext : DbContext
    {
        public DWAktuarieContext (DbContextOptions<DWAktuarieContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //EF demands primary keys
            modelBuilder.Entity<Models.NSS.vSwap>(entity => {
                entity.HasKey(c => new { c.PRICEDATE, c.Name });
                entity.ToTable("vSwap");
            });
            //EF demands primary keys
            modelBuilder.Entity<Models.SGB.vBond>(entity => {
                entity.HasKey(c => new { c.PRICE_DATE, c.SEC_SHORT_NAME });
                entity.ToTable("vBond");
            });
            //EF demands primary keys
            modelBuilder.Entity<Models.TAT.PeriodAvkastning>(entity => {
                entity.HasKey(c => new { c.ToDate, c.NodeName });
                entity.ToTable("PeriodAvkastning", "dbo");
            });
        }
        public DbSet<Models.SGB.vBond> vBond { get; set; }
        public DbSet<Models.TAT.PeriodAvkastning> tPeriodAvkastning { get; set; }
    }
}
