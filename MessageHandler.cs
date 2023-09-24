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

                var payloadObject = JsonSerializer.Deserialize<JsonNode>(payloadString);

                if (payloadObject["print"] != null)
                {
                    if (Settings.Instance.ShowAllMessages)
                    {
                        Console.WriteLine(payloadString);
                    }

                    HandlePrintMessage(payloadObject["print"]);
                }
            }

            return Task.CompletedTask;
        }

        private static PrinterStatus printer = new PrinterStatus();

        private static void HandlePrintMessage(JsonNode json)
        {
            //Here's where BambuStudio processes the "push_status" message: https://github.com/bambulab/BambuStudio/blob/5ef759ce41863f989da7b363f7c94e5edc5ade0d/src/slic3r/GUI/DeviceManager.cpp#L2740

            if (json["command"] != null)
            {
                string command = json["command"].GetValue<string>();
                if (command == "project_file")
                {
                    printer.PrintName = json["subtask_name"].GetValue<string>(); //Todo: this is the print/project name, but it would be nice if we could use the model name

                    Helpers.EchoMessage($"'{printer.PrintName}' has started printing", ConsoleColor.Green, true);
                }
            }

            if (json["gcode_state"] != null)
            {
                Helpers.SetIfDifferent(ref printer.Status, json["gcode_state"].GetValue<string>(), state =>
                {
                    if (state == "FINISH" && printer.Stopwatch.IsRunning)
                    {
                        printer.Stopwatch.Stop();
                        Helpers.EchoMessage($"'{printer.PrintName}' finished printing in {printer.Stopwatch.Elapsed.Format()}", ConsoleColor.Green, true);
                    }
                });
            }

            if (json["stg_cur"] != null)
            {
                Helpers.SetIfDifferent(ref printer.PrintStage, json["stg_cur"].GetValue<int>(), stage =>
                {
                    string stageMessage = PrinterStatus.InterpretStage(stage);
                    if (!string.IsNullOrEmpty(stageMessage))
                    {
                        Helpers.EchoMessage($"Stage: {stageMessage} (elapsed: {printer.Stopwatch.Elapsed.Format()})", ConsoleColor.Cyan);
                    }

                    if (stage == 5 /*M400 pause*/ || stage == 16 /*user pause*/)
                    {
                        printer.Stopwatch.Stop(); //Todo: need to test this and figure out when we should resume the Stopwatch
                        Helpers.EchoMessage($"'{printer.PrintName}' is paused", ConsoleColor.Gray, true);
                    }
                });
            }

            if (json["layer_num"] != null)
            {
                Helpers.SetIfDifferent(ref printer.LayerNum, json["layer_num"].GetValue<int>(), layer =>
                {
                    Helpers.EchoMessage($"LAYER: {layer}", ConsoleColor.Yellow);

                    if (layer == 0)
                    {
                        //Only start the stopwatch when the "layer_num: 0" message is sent. Otherwise, it seems that a "gcode_state: FINISH" can be sent AFTER
                        //the project_file command is sent (presumably, leftover from the previous print)
                        printer.MarkPrintAsStarted();
                    }
                });
            }

            if (json["mc_remaining_time"] != null && json["mc_remaining_time"].GetValue<int>() != 0 && !printer.NotifiedOfRemainingTime)
            {
                //https://github.com/bambulab/BambuStudio/blob/master/src/slic3r/GUI/DeviceManager.hpp#L616 says "time in seconds" but that's not what I'm seeing in practice
                TimeSpan remainingTime = TimeSpan.FromMinutes(json["mc_remaining_time"].GetValue<int>());
                Helpers.EchoMessage($"Estimated time remaining: {remainingTime.Format()}", ConsoleColor.Green, true);
                printer.NotifiedOfRemainingTime = true;
            }

            if (json["mc_percent"] != null)
            {
                Helpers.SetIfDifferent(ref printer.ProgressPercentage, json["mc_percent"].GetValue<int>());
            }

            if (json["print_error"] != null && json["print_error"].GetValue<int>() != 0)
            {
                int errorCode = json["print_error"].GetValue<int>();
                string errorMessage = PrinterStatus.InterpretErrorCode(errorCode);
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    Helpers.EchoMessage($"'{printer.PrintName}' needs attention: ({errorMessage})", ConsoleColor.Yellow);
                }
                else
                {
                    Helpers.EchoMessage($"'{printer.PrintName}' encountered an unknown error ({errorCode})", ConsoleColor.Red, true);
                }
            }
        }
    }

    public static class Helpers
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

        public static void SetIfDifferent<T>(ref T prop, T val, Action<T> afterSet = null)
        {
            if ((prop == null && val != null) || !prop.Equals(val))
            {
                prop = val;
                afterSet?.Invoke(val);
            }
        }

        public static void EchoMessage(string message, ConsoleColor color, bool alsoSendOnDiscord = false)
        {
            WriteToConsoleWithColor(message, color);
            if (alsoSendOnDiscord)
            {
                SendDiscordMessage(message);
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
}