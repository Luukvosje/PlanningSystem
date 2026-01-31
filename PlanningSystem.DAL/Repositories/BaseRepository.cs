using Microsoft.EntityFrameworkCore;
using PlanningSystem.Interfaces.DAL;

namespace PlanningSystem.DAL.Repositories
{
    internal abstract class BaseRepository
    {
        protected readonly AppSettings _settings;

        private static Dictionary<int, string>? CachedUsers = null;

        public BaseRepository(AppSettings settings)
        {
            _settings = settings;
        }

        public AppDbContext GetContext()
        {
            return GetContext(_settings);
        }

        public static AppDbContext GetContext(AppSettings settings)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlServer(settings.ConnectionString, options =>
            {
                options.CommandTimeout(600);
            });

            return new AppDbContext(optionsBuilder.Options, settings);
        }

        public void ReloadCachedUsers()
        {
            using (var db = GetContext())
            {
                CachedUsers = db.Users.ToList().ToDictionary((x) => x.Id, (x) => x.Name ?? "");
            }
        }
    }
}
