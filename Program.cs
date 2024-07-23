using BambuLabsListener;
using MQTTnet;
using MQTTnet.Client;

var mqttFactory = new MqttFactory();
Settings.Read("settings.json");

BambuMqttClient client = new BambuMqttClient(Settings.Instance);
client.Connect(); //Todo: add some sort of while loop to retry connections if they get disconnected