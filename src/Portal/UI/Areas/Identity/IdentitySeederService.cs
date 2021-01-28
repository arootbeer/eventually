using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Eventually.Portal.UI.Areas.Identity
{
    public class IdentitySeederService : BackgroundService
    {
        private readonly IdentitySeeder _seeder;

        public IdentitySeederService(IdentitySeeder seeder)
        {
            _seeder = seeder;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _seeder.Seed();
        }
    }
}