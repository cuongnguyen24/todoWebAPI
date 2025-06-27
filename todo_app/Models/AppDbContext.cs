using Microsoft.EntityFrameworkCore;

namespace todo_app.Models
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<List> Lists { get; set; }
        public DbSet<Todo> Todos { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<TodoTag> TodoTags { get; set; }
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình khóa chính kép cho TodoTag
            modelBuilder.Entity<TodoTag>()
                .HasKey(tt => new { tt.TodoId, tt.TagId });

            // Quan hệ Todo - TodoTag (1-nhiều)
            modelBuilder.Entity<TodoTag>()
                .HasOne(tt => tt.Todo)
                .WithMany(t => t.TodoTags)
                .HasForeignKey(tt => tt.TodoId);

            // Quan hệ Tag - TodoTag (1-nhiều)
            modelBuilder.Entity<TodoTag>()
                .HasOne(tt => tt.Tag)
                .WithMany(t => t.TodoTags)
                .HasForeignKey(tt => tt.TagId);

            // Thêm để tránh lỗi multiple cascade path
            modelBuilder.Entity<Todo>()
                .HasOne(t => t.User)
                .WithMany(u => u.Todos)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.NoAction); // fix lỗi cascade
        }
    }
}
