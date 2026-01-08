using Microsoft.EntityFrameworkCore;
using backend.Data;

namespace backend.Extensions
{
    public static class DatabaseExtensions
    {
        public static void ApplyMigrations(this WebApplication app)
        {
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                
                try
                {
                    // تطبيق جميع الـ migrations المعلقة
                    db.Database.Migrate();
                    Console.WriteLine("✅ Database migrations applied successfully!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error applying migrations: {ex.Message}");
                    throw;
                }
            }
        }
    }
}
