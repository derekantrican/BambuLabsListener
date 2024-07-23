using System;

namespace BambuLabsListener
{
    public class BambuMqttClient
    {
        private MqttClientOptions options;
        private string serialNumber;

        public BambuMqttClient(Settings settings) : this(
            settings.IpAddress, 
            Convert.ToInt32(settings.Port), 
            settings.AccessCode, 
            settings.SerialNumber
        ){}

        public BambuMqttClient(string ip, int port, string accessCode, string serialNumber)
        {
            this.serialNumber = serialNumber;

            options = new MqttClientOptionsBuilder()
            .WithTcpServer(ip, port)
            .WithCredentials("bblp", accessCode)
            .WithTls(new MqttClientOptionsBuilderTlsParameters
            {
                UseTls = true,
                CertificateValidationHandler = e =>
                {
                    return true;
                },
            })
            .Build();
        }

        public void Connect()
        {
            using (var mqttClient = mqttFactory.CreateMqttClient())
            {
                mqttClient.ApplicationMessageReceivedAsync += e => MessageHandler.HandleMessage(e?.ApplicationMessage);

                await mqttClient.ConnectAsync(options, CancellationToken.None);

                mqttClient.DisconnectedAsync += async (args) =>
                {
                    Console.WriteLine($"!!! MQTT Client disconnected !!!");
                    Console.WriteLine($"Reason: {args.ReasonString}");
                };

                var mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
                    .WithTopicFilter(f =>
                    {
                        f.WithTopic($"device/{serialNumber}/report");
                    })
                    .Build();

                await mqttClient.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);

                Console.WriteLine("MQTT client subscribed to topic.");

                Console.WriteLine("Press enter to exit.");
                Console.ReadLine();
            }
        }
    }
}