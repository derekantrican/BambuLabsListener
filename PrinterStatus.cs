using System.Diagnostics;

namespace BambuLabsListener
{
    public class PrinterStatus
    {
        public string Status;
        public string PrintName;
        public int PrintStage;
        public int ProgressPercentage;
        public int LayerNum;
        public Stopwatch Stopwatch = new Stopwatch(); //The MQTT messages don't seem to contain times, so we'll keep track of our own stopwatch

        //Copied from BambuStudio source code https://github.com/bambulab/BambuStudio/blob/5ef759ce41863f989da7b363f7c94e5edc5ade0d/src/slic3r/GUI/DeviceManager.cpp#L52
        public static string InterpretStage(int stage)
        {
            switch(stage) {
                case 0:
                    return "Printing";
                case 1:
                    return "Auto bed leveling";
                case 2:
                    return "Heatbed preheating";
                case 3:
                    return "Sweeping XY mech mode";
                case 4:
                    return "Changing filament";
                case 5:
                    return "M400 pause";
                case 6:
                    return "Paused due to filament runout";
                case 7:
                    return "Heating hotend";
                case 8:
                    return "Calibrating extrusion";
                case 9:
                    return "Scanning bed surface";
                case 10:
                    return "Inspecting first layer";
                case 11:
                    return "Identifying build plate type";
                case 12:
                    return "Calibrating Micro Lidar";
                case 13:
                    return "Homing toolhead";
                case 14:
                    return "Cleaning nozzle tip";
                case 15:
                    return "Checking extruder temperature";
                case 16:
                    return "Printing was paused by the user";
                case 17:
                    return "Pause of front cover falling";
                case 18:
                    return "Calibrating the micro lida";
                case 19:
                    return "Calibrating extrusion flow";
                case 20:
                    return "Paused due to nozzle temperature malfunction";
                case 21:
                    return "Paused due to heat bed temperature malfunction";
                default:
                    return null;
            }
        }
    }
}