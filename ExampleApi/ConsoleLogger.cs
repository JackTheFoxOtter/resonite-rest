using ApiFramework.Interfaces;

namespace ExampleApi
{
    internal class ConsoleLogger : ILogger
    {
        private string _moduleName;

        public ConsoleLogger(string moduleName)
        {
            _moduleName = moduleName;
        }

        public void Log(string message)
        {
            Console.WriteLine($"[{_moduleName}] {message}");
        }
    }
}
