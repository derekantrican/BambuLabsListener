using BambuLabsListener;
using MQTTnet;
using MQTTnet.Client;

var mqttFactory = new MqttFactory();
Settings.Read("settings.json");

using (var mqttClient = mqttFactory.CreateMqttClient())
{
    var mqttClientOptions = new MqttClientOptionsBuilder()
        .WithTcpServer(Settings.Instance.IpAddress, Convert.ToInt32(Settings.Instance.Port))
        .WithCredentials("bblp", Settings.Instance.AccessCode)
        .WithTls(new MqttClientOptionsBuilderTlsParameters
        {
            UseTls = true,
            CertificateValidationHandler = e =>
            {
                return true;
            },
        })
        .Build();

    mqttClient.ApplicationMessageReceivedAsync += e => MessageHandler.HandleMessage(e?.ApplicationMessage);

    await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

    var mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
        .WithTopicFilter(f =>
        {
            f.WithTopic($"device/{Settings.Instance.SerialNumber}/report");
        })
        .Build();

    await mqttClient.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);

    Console.WriteLine("MQTT client subscribed to topic.");

    Console.WriteLine("Press enter to exit.");
    Console.ReadLine();
}