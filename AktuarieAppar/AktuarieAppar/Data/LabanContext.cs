using Microsoft.EntityFrameworkCore;
using AktuarieAppar.Models;



namespace AktuarieAppar.Data
{
    public partial class LabanContext : DbContext
    {
        public LabanContext()
        {
        }

        public LabanContext(DbContextOptions<LabanContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Models.NSS.NSSParametrar> NSSParametrar { get; set; }
        public virtual DbSet<LabanGrund> Grunder { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //EF demands primary keys
            modelBuilder.Entity<LabanGrund>(entity => {
                entity.HasKey(c => new { c.Miljo, c.Bolag, c.Gren, c.Skatt, c.Grunder, c.FromDate })
                      .HasName("PK_Grunder_1__16"); ;
                entity.ToTable("Grunder", "dbo");
            });

            modelBuilder.Entity<Models.NSS.NSSParametrar>(entity =>
            {
                entity.HasKey(e => new { e.Grunder, e.KurvNamn, e.FromDate })
                    .HasName("PK_NSSParametrar_1__8");

                entity.ToTable("NSSParametrarTMP", "dbo");

                entity.Property(e => e.Grunder)
                    .HasMaxLength(6)
                    .IsUnicode(false);

                entity.Property(e => e.KurvNamn)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.FromDate).HasColumnType("datetime");
                entity.Property(e => e.Betha0).HasColumnName("betha0");
                entity.Property(e => e.Betha1).HasColumnName("betha1");
                entity.Property(e => e.Betha2).HasColumnName("betha2");
                entity.Property(e => e.Betha3).HasColumnName("betha3");
                entity.Property(e => e.TKonst).HasColumnName("tKonst");
                entity.Property(e => e.Tao0).HasColumnName("tao0");
                entity.Property(e => e.Tao1).HasColumnName("tao1");
            });
        }
    }
}
