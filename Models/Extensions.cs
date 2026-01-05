using Microsoft.EntityFrameworkCore;

namespace WebBanMayTinh.Models
{
    public static class Extensions
    {
        public static async Task ApplyMigrationsAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();
            await dbContext.Database.MigrateAsync();
        }
    }
}
