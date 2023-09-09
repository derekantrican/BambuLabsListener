using System.Diagnostics;

namespace BambuLabsListener
{
    public class PrintStatus
    {
        public string Name { get; set; }
        public int ProgressPercentage { get; set; }
        public Stopwatch Stopwatch { get; set; } = new Stopwatch(); //The MQTT messages don't seem to contain times, so we'll keep track of our own stopwatch
    }
}