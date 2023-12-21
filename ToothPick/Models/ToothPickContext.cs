namespace ToothPick.Models
{
    public class ToothPickContext : DbContext
    {
        public DbSet<Library> Libraries { get; set; }
        public DbSet<Series> Series { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Media> Media { get; set; }
        public DbSet<Setting> Settings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Series>()
                .HasOne(series => series.Library)
                .WithMany(library => library.Series)
                .HasForeignKey(series => series.LibraryName);

            modelBuilder.Entity<Media>()
                .HasOne(media => media.Series)
                .WithMany(series => series.Medias)
                .HasForeignKey(media => new { media.LibraryName, media.SeriesName });

            modelBuilder.Entity<Media>()
                .HasOne(media => media.Library)
                .WithMany(library => library.Medias)
                .HasForeignKey(media => media.LibraryName);

            modelBuilder.Entity<Location>()
                .HasOne(location => location.Series)
                .WithMany(series => series.Locations)
                .HasForeignKey(location => new { location.LibraryName, location.SeriesName });

            modelBuilder.Entity<Location>()
                .HasOne(location => location.Library)
                .WithMany(library => library.Locations)
                .HasForeignKey(location => location.LibraryName);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            FileInfo databaseDestinationFileInfo = new("/ToothPick/data/toothpick.db");

            optionsBuilder
                .UseLazyLoadingProxies()
                .UseSqlite($"Data Source=\"{databaseDestinationFileInfo.FullName}\";");
        }
    }
}
