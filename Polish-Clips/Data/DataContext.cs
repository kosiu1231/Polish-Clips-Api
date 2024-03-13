namespace Polish_Clips.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Clip> Clips => Set<Clip>();
        public DbSet<Comment> Comments => Set<Comment>();
        public DbSet<Game> Games => Set<Game>();
        public DbSet<Like> Likes => Set<Like>();
        public DbSet<Report> Reports => Set<Report>();
    }
}
