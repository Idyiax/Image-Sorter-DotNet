using Microsoft.EntityFrameworkCore;
using Image_Sorter_DotNet.Models;

namespace Image_Sorter_DotNet.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Images> Images { get; set; }
        public DbSet<Collections> Collections { get; set; }
        public DbSet<CollectionConnections> CollectionConnections { get; set; }
        public DbSet<Tags> Tags { get; set; }
        public DbSet<TagRelations> TagRelations { get; set; }
        public DbSet<TagConnections> TagConnections { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CollectionConnections>()
                .HasOne(icc => icc.Image)
                .WithMany(i => i.CollectionConnection)
                .HasForeignKey(icc => icc.ImageId);

            modelBuilder.Entity<CollectionConnections>()
                .HasOne(icc => icc.Collection)
                .WithMany(ic => ic.CollectionConnection)
                .HasForeignKey(icc => icc.CollectionId);


            modelBuilder.Entity<TagRelations>()
                .HasOne(tr => tr.ParentTag)
                .WithMany(t => t.ParentTagRelations)
                .HasForeignKey(tr => tr.ParentTagId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TagRelations>()
                .HasOne(tr => tr.ChildTag)
                .WithMany(t => t.ChildTagRelations)
                .HasForeignKey(tr => tr.ChildTagId)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<TagConnections>()
                .HasOne(tc => tc.Image)
                .WithMany(i => i.TagConnections)
                .HasForeignKey(tc => tc.ImageId);

            modelBuilder.Entity<TagConnections>()
                .HasOne(tc => tc.Tag)
                .WithMany(t => t.TagConnections)
                .HasForeignKey(tc => tc.TagId);  
        }
    }
}