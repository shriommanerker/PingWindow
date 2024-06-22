using System;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

namespace PingMonitor
{
    public partial class MainWindow : Window
    {
        private string ip1 = "45.116.0.238"; // Replace with your IP addresses
        private string ip2 = "8.8.8.8";
        private System.Net.NetworkInformation.Ping ping1 = new System.Net.NetworkInformation.Ping();
        private System.Net.NetworkInformation.Ping ping2 = new System.Net.NetworkInformation.Ping();
        private string settingsFilePath = "windowSettings.xml";

        private Queue<string> historyIP1 = new Queue<string>();
        private Queue<string> historyIP2 = new Queue<string>();

        public MainWindow()
        {
            InitializeComponent();
            LoadWindowSettings();
            StartPinging();
        }

        private async void StartPinging()
        {
            while (true)
            {
                try
                {
                    PingReply reply1 = await ping1.SendPingAsync(ip1, 300);
                    PingReply reply2 = await ping2.SendPingAsync(ip2, 300);

                    // Update history
                    UpdateHistory(historyIP1, reply1);
                    UpdateHistory(historyIP2, reply2);

                    // Update UI with latest ping results
                    UpdatePingResult(txtPingResult1, historyIP1);
                    UpdatePingResult(txtPingResult2, historyIP2);
                }
                catch (Exception ex)
                {
                    //MessageBox.Show($"Error: {ex.Message} Stacktrace:{ex.StackTrace}");
                }

                await Task.Delay(1000); // Wait for 1 second before pinging again
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Ensure the window opens within the visible screen bounds
            if (Left < 0 || Top < 0 ||
                Left + Width > SystemParameters.VirtualScreenWidth ||
                Top + Height > SystemParameters.VirtualScreenHeight)
            {
                //// Reset window position and size if it's out of bounds
                //Left = (SystemParameters.VirtualScreenWidth - Width) / 2;
                //Top = (SystemParameters.VirtualScreenHeight - Height) / 2;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveWindowSettings(); // Save window settings when the window is closing
        }
        private void LoadWindowSettings()
        {
            if (File.Exists(settingsFilePath))
            {
                try
                {
                    XElement settingsXml = XElement.Load(settingsFilePath);
                    double left = Convert.ToDouble(settingsXml.Element("Left").Value);
                    double top = Convert.ToDouble(settingsXml.Element("Top").Value);
                    double width = Convert.ToDouble(settingsXml.Element("Width").Value);
                    double height = Convert.ToDouble(settingsXml.Element("Height").Value);

                    Left = left;
                    Top = top;
                    Width = width;
                    Height = height;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading window settings: {ex.Message}");
                }
            }
        }

        private void SaveWindowSettings()
        {
            try
            {
                XElement settingsXml = new XElement("WindowSettings",
                                    new XElement("Left", Left),
                                    new XElement("Top", Top),
                                    new XElement("Width", Width),
                                    new XElement("Height", Height));

                settingsXml.Save(settingsFilePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving window settings: {ex.Message}");
            }
        }

        private void UpdateHistory(Queue<string> history, PingReply reply)
        {
            string result = reply.Status == IPStatus.Success ?
                $"Reply {reply.Address}: " +//$"Bytes={reply.Buffer.Length} " +
                $"Time={reply.RoundtripTime}ms at {DateTime.Now.Hour}:{DateTime.Now.Minute}:{DateTime.Now.Second}" :
                //$"TTL={reply.Options.Ttl}" :
                $"Ping failed: {reply.Status} at {DateTime.Now.Hour}:{DateTime.Now.Minute}:{DateTime.Now.Second}";

            // Add the new result to history
            history.Enqueue(result);

            // Maintain history size to 10 latest entries
            while (history.Count > 10)
            {
                history.Dequeue();
            }
        }

        private void UpdatePingResult(TextBlock textBlock, Queue<string> history)
        {
            // Display the last 10 entries in history
            textBlock.Text = string.Join("\n", history);
        }
    }
}
