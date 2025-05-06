using System.CommandLine;
using System.Diagnostics;
using System.IO.Hashing;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

namespace RsyncExample
{
    internal class Program
    {
        private static string _rootDir = "H:\\"; //Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        public static async Task Main(string[] args)
        {
            RootCommand rootCommand = new RootCommand("RsyncExample - Examples of the rsync algorithum in C#");
            Type[] types = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsSubclassOf(typeof(Command)) && t.IsClass).ToArray();
            foreach (var type in types)
            {
                var instance = (Command)Activator.CreateInstance(type);
                rootCommand.AddCommand(instance);
            }

            await rootCommand.InvokeAsync(args);
        }
    }
}
