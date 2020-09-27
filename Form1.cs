using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Management;
using System.Threading.Tasks;
using System.Windows.Forms;

using OpenHardwareMonitor.Hardware;

using Timer = System.Windows.Forms.Timer;

namespace CPUFrequencyWin
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        Timer timer;
        Computer computer = new Computer();
        private void Form1_Load(object sender, EventArgs e)
        {
            timer = new Timer()
            {
                Interval = 100,
                Enabled = true
            };
            var task = new Task(TaskWork);

            task.Start();
            timer.Tick += (o, ea) =>
            {
                timer.Stop();
                    if (task.Status == TaskStatus.RanToCompletion)
                    {
                        task = new Task(TaskWork);
                        task.Start();
                    }
                timer.Start();
            };
            
        }

        private void TaskWork()
        {
                var cpuInfo = GetCPUInfoOHM();
                if (InvokeRequired)
                {
                    Invoke(new Action(() => { richTextBox1.Text = cpuInfo; }));
                }
                else
                {
                    richTextBox1.Text = cpuInfo;
                }
        }

        private static string GetCPUInfo()
        {
            PerformanceCounter cpuCounter = new PerformanceCounter("Processor Information", "% Processor Performance", "_Total");
            double cpuValue = cpuCounter.NextValue();

            foreach (ManagementObject obj in new ManagementObjectSearcher("SELECT *, Name FROM Win32_Processor").Get())
            {
                double maxSpeed = Convert.ToDouble(obj["MaxClockSpeed"]) / 1000;
                double turboSpeed = maxSpeed * cpuValue / 100;
                return string.Format("{0} Running at {1:0.00}Ghz, Turbo Speed: {2:0.00}Ghz", obj["Name"], maxSpeed, turboSpeed);
            }

            return string.Empty;
        }
        private string GetCPUInfoOHM()
        {
            computer.CPUEnabled = true;
            computer.Open();

            string returnValue="";
            foreach (var hardware in computer.Hardware)
            {
                if(hardware.HardwareType == HardwareType.CPU)
                {
                    var cpu = hardware;
                    cpu.Update();
                    foreach(var sensor in cpu.Sensors)
                    {
                        if (sensor.SensorType == SensorType.Clock)
                        {
                            returnValue += string.Format("[{2}] {0}=>{1}\n",sensor.Name, sensor.Value,DateTime.UtcNow);
                        }
                    }
                }
            }
            return returnValue;
        }
        private void exitButton_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }
    }
}
