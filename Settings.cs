using System.Text.Json;

namespace BambuLabsListener
{
    public class Settings
    {
        public static Settings Instance { get; internal set; }

        public string IpAddress { get; set; }
        public string Port { get; set; }
        public string SerialNumber { get; set; }
        public string AccessCode { get; set; }
        public string DiscordWebhook { get; set; }
        public bool ShowAllMessages { get; set; }

        public static void Read(string file)
        {
            if (!File.Exists(file))
            {
                File.WriteAllText(file, JsonSerializer.Serialize(new Settings
                {
                    IpAddress = "",
                    Port = "",
                    SerialNumber = "",
                    AccessCode = "",
                    DiscordWebhook = "",
                }, 
                new JsonSerializerOptions
                {
                    WriteIndented = true,
                }));

                Console.WriteLine($"File '{file}' does not exist. An empty one has been created. Please populate it with your printer settings.");
                Console.WriteLine($"Exiting...");
                Environment.Exit(1);
            }

            Instance = JsonSerializer.Deserialize<Settings>(File.ReadAllText("settings.json"), new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            });
        }
    }
}