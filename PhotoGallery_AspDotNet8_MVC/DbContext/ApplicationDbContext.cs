namespace PhotoGallery_AspDotNet8_MVC.DbContext;
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Photo> Photos { get; set; }
        public DbSet<Tag> Tags { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Photo>()
                .HasMany(p => p.Tags)
                .WithMany(t => t.Photos);
        }
    }
