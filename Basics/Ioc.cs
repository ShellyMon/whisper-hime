using Microsoft.Extensions.DependencyInjection;

namespace SoraBot.Basics
{
    internal class Ioc
    {
        private static readonly Ioc _ioc = new();

        private IServiceProvider? _services;

        public static void Configure(Action<IServiceCollection> builder)
        {
            var services = new ServiceCollection();
            builder(services);
            _ioc._services = services.BuildServiceProvider();
        }

        public static T Require<T>() where T : notnull
        {
            if (_ioc._services == null)
                throw new InvalidOperationException("Ioc not configured.");

            return _ioc._services.GetRequiredService<T>();
        }
    }
}
