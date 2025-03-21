using FrooxEngine;
using SkyFrost.Base;

namespace ResoniteApi
{
    internal class RunStandalone
    {
        public static void Main(string[] args) {
            StandaloneFrooxEngineRunner runner = new StandaloneFrooxEngineRunner();
            LaunchOptions options = new();
            options.StartInvisible = true;
            options.AdditionalAssemblies.Add("ResoniteApi.dll");

            Task.WaitAll(Task.Run(async () =>
            {
                await runner.Initialize(options);
                await runner.Login("JackTheFoxOtter", new PasswordLogin("ABC"));

                while (true)
                {
                    await Task.Delay(1);
                }
            }));
        }
    }
}
