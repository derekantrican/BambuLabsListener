using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using MQTTnet;
using MQTTnet.Internal;

namespace BambuLabsListener
{
    public static class MessageHandler
    {
        public static Task HandleMessage(MqttApplicationMessage message)
        {
            if (message != null && !(message.PayloadSegment == null || message.PayloadSegment == EmptyBuffer.ArraySegment))
            {
                var payloadSegment = message.PayloadSegment;
                string payloadString = Encoding.UTF8.GetString(payloadSegment.Array, payloadSegment.Offset, payloadSegment.Count);
                // Console.WriteLine(payloadString);

                var payloadObject = JsonSerializer.Deserialize<JsonNode>(payloadString);

                if (payloadObject["print"] != null)
                {
                    HandlePrintMessage(payloadObject["print"]);
                }

                if (payloadObject["info"] != null)
                {
                    HandleInfoMessage(payloadObject["info"]);
                }
            }

            return Task.CompletedTask;
        }

        private static PrintStatus currentPrint;

        private static void HandlePrintMessage(JsonNode json)
        {
            //Has information about the state of the printer like temps, print progress, etc

            //There also seems to be "mc_print_sub_stage" which seems interesting (might indicate whether we're printing, bed leveling, etc)
            //There's some mappings for X1C on the HomeAssistant forums here: https://community.home-assistant.io/t/bambu-lab-x1-x1c-mqtt/489510/165
            //Guesses as to what the values might be (from testing):
            //0: idle
            //1: homing print head? not sure. something about print head movement, I think
            //2: bed preheating?
            //3: auto bed leveling?
            //Note that the values flip back & forth a lot. Like bed leveling could be happening (3) but messages for 2 & 1 could happen when the print head moves

            //There's also "mc_print_stage" which values might be something like idle (0), heating (1), running gcode (2) or something

            string command = json["command"].GetValue<string>();
            string gcodeState = json["gcode_state"] != null ? json["gcode_state"].GetValue<string>() : null;
            int subStage = json["mc_print_sub_stage"] != null ? json["mc_print_sub_stage"].GetValue<int>() : 0;

            if (command == "project_file")
            {
                currentPrint = new PrintStatus
                {
                    Name = json["subtask_name"].GetValue<string>() //Todo: this is the print name, but it would be nice if we could use the model name
                };
            }

            if (json["layer_num"] != null && json["layer_num"].GetValue<int>() == 1) //Triggering off layer 1 will give us the model print time - excluding bed leveling & other prep
            {
                if (!currentPrint.Stopwatch.IsRunning)
                {
                    WriteToConsoleWithColor($"{currentPrint.Name} has started printing", ConsoleColor.Green);
                    SendDiscordMessage($"{currentPrint.Name} has started printing");

                    currentPrint.Stopwatch.Start();
                }
            }

            if (json["layer_num"] != null)
            {
                WriteToConsoleWithColor($"LAYER: {json["layer_num"].GetValue<int>()}", ConsoleColor.Yellow);
            }

            if (json["mc_percent"] != null)
            {
                currentPrint.ProgressPercentage = json["mc_percent"].GetValue<int>();
                //Not currently doing anything with this, but we could report progress somewhere
            }

            if (gcodeState == "FINISH")
            {
                if (currentPrint.Stopwatch.IsRunning) //It's possible for a "FINISH" event to happen at the beginning so this should prevent that
                {
                    WriteToConsoleWithColor($"{currentPrint.Name} finished printing in {currentPrint.Stopwatch.Elapsed.Format()}", ConsoleColor.Green);
                    currentPrint.Stopwatch.Stop();
                    SendDiscordMessage($"{currentPrint.Name} finished printing in {currentPrint.Stopwatch.Elapsed.Format()}");
                }
            }

            if ((gcodeState != null && gcodeState != "IDLE") || command != "push_status" || subStage > 0)
            {
                Console.WriteLine($"PRINT: {JsonSerializer.Serialize(json)}\n");
            }
        }

        private static void HandleInfoMessage(JsonNode json)
        {
            if (json["command"] == null || json["command"].GetValue<string>() != "get_version")
            {
                Console.WriteLine($"INFO: {JsonSerializer.Serialize(json)}\n");
            }
        }

        private static void SendDiscordMessage(string message)
        {
            using (var client = new HttpClient())
            {
                client.Send(new HttpRequestMessage(HttpMethod.Post, Settings.Instance.DiscordWebhook)
                {
                    Content = JsonContent.Create(new
                    {
                        content = message,
                        username = "BambuLabs printer",
                        avatar_url = "https://wiki.bambulab.com/admin/home/logo-large.png",
                    })
                });
            }
        }

        private static void WriteToConsoleWithColor(string message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }

    public static class ExtensionMethods
    {
        public static string Format(this TimeSpan timeSpan)
        {
            string result = $"{timeSpan:%m\\m\\ %s\\s}";
            if (timeSpan.Hours > 0)
            {
                result = $"{timeSpan.Hours}h {result}";
            }

            return result;
        }
    }
}