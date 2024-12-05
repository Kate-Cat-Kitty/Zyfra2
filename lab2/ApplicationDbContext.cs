using Microsoft.EntityFrameworkCore;


namespace lab2
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // Таблицы в базе данных
        public DbSet<User> Users { get; set; }
        public DbSet<Session> Sessions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Настройка модели User
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id); // Первичный ключ
                entity.Property(u => u.Username)
                      .IsRequired()
                      .HasMaxLength(100); // Логин пользователя
                entity.Property(u => u.PasswordHash)
                      .IsRequired(); // Хэш пароля
                entity.Property(u => u.Status)
                      .IsRequired(); // Статус пользователя
            });

            // Настройка модели Session
            modelBuilder.Entity<Session>(entity =>
            {
                entity.HasKey(s => s.Id); // Первичный ключ
                entity.Property(s => s.SessionId)
                      .IsRequired()
                      .HasMaxLength(36); // Идентификатор сессии (GUID)
                entity.HasOne(s => s.User)
                      .WithMany()
                      .HasForeignKey(s => s.UserId)
                      .OnDelete(DeleteBehavior.Cascade); // Связь с пользователем
            });
        }
    }
    public class User
    {
        public int Id { get; set; } // Уникальный идентификатор пользователя

        public string Username { get; set; } // Имя пользователя

        public string PasswordHash { get; set; } // Хэш пароля

        public int Status { get; set; } // Статус пользователя (0 — неактивен, 1 — активен)
    }
    public class Session
    {
        public int Id { get; set; } // Уникальный идентификатор сессии

        public string SessionId { get; set; } // GUID для идентификации сессии

        public int UserId { get; set; } // Внешний ключ, связывающий с пользователем

        public User User { get; set; } // Навигационное свойство для связи с пользователем
    }
}
