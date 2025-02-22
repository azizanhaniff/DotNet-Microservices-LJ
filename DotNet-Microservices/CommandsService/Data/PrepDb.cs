using CommandsService.Models;
using CommandsService.SyncDataServices.Grpc;

namespace CommandsService.Data
{
    public static class PrepDB
    {
        public static void PrepPopulation(IApplicationBuilder applicationBuilder)
        {
            using var serviceScope = applicationBuilder.ApplicationServices.CreateScope();

            var grpcClient = serviceScope.ServiceProvider.GetService<IPlatformDataClient>();

            var platforms = grpcClient.ReturnAllPlatforms();

            SeedData(serviceScope.ServiceProvider.GetService<ICommandRepo>(), platforms);
        }

        private static void SeedData(ICommandRepo repo, IEnumerable<Platform> platforms)
        {
            Console.WriteLine("Seeding new platforms...");

            foreach (Platform plat in platforms)
            {
                if (repo.ExternalPlatformExists(plat.ExternalID) == false)
                {
                    repo.CreatePlatform(plat);
                }

                repo.SaveChanges();
            }
        }
    }
}
