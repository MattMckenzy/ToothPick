namespace ToothPick.Models
{
    public class ToothPickContext : DbContext
    {
        public DbSet<Library> Libraries { get; set; }
        public DbSet<Serie> Series { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Media> Medias { get; set; }
        public DbSet<Setting> Settings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Serie>()
                .HasKey(serie => new { serie.LibraryName, serie.Name });

            modelBuilder.Entity<Serie>()
                .HasOne(serie => serie.Library)
                .WithMany(library => library.Series)
                .HasForeignKey(serie => serie.LibraryName);


            modelBuilder.Entity<Media>()
                .HasKey(media => new { media.LibraryName, media.SerieName, media.Location });

            modelBuilder.Entity<Media>()
                .HasOne(media => media.Serie)
                .WithMany(serie => serie.Medias)
                .HasForeignKey(media => new { media.LibraryName, media.SerieName });

            modelBuilder.Entity<Media>()
                .HasOne(media => media.Library)
                .WithMany(library => library.Medias)
                .HasForeignKey(media => media.LibraryName);


            modelBuilder.Entity<Location>()
                .HasKey(location => new { location.LibraryName, location.SerieName, location.Url });

            modelBuilder.Entity<Location>()
                .HasOne(location => location.Serie)
                .WithMany(serie => serie.Locations)
                .HasForeignKey(location => new { location.LibraryName, location.SerieName });

            modelBuilder.Entity<Location>() 
                .HasOne(location => location.Library)
                .WithMany(library => library.Locations)
                .HasForeignKey(location => location.LibraryName);


            modelBuilder.Entity<Setting>()
                .HasKey(setting => new { setting.Name });
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            FileInfo databaseDestinationFileInfo = new("/root/ToothPick/data/toothpick.db");

            optionsBuilder
                .UseLazyLoadingProxies()
                .UseSqlite($"Data Source=\"{databaseDestinationFileInfo.FullName}\";");
        }
    }
}
