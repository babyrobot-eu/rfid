using System;
using System.Text;
using System.Windows;
using Impinj.OctaneSdk;
using OctaneSdkExamples;
using System.Net.Sockets;
using System.Diagnostics;

// This code is designed for the qube tracking game using RFID system. 
// The communication module is implemeted using IrisTK. 
namespace CubeTrackingRFID
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Create an instance of the ImpinjReader class.
        static ImpinjReader reader = new ImpinjReader();
        public static TcpClient socket;
        public static string side;
        public static string eventName;

        public MainWindow()
        {
            InitializeComponent();
            
            try
            {
                // Connect to the reader.
                // Change the ReaderHostname constant in SolutionConstants.cs 
                // to the IP address or hostname of your reader.
                reader.Connect(SolutionConstants.ReaderHostname);

                // Get the default settings
                // We'll use these as a starting point
                // and then modify the settings we're 
                // interested in.

                Settings settings = reader.QueryDefaultSettings();

                // Tell the reader to include the antenna number
                // in all tag reports. Other fields can be added
                // to the reports in the same way by setting the 
                // appropriate Report.IncludeXXXXXXX property.
                settings.Report.IncludeAntennaPortNumber = true;

                // The reader can be set into various modes in which reader
                // dynamics are optimized for specific regions and environments.
                // The following mode, AutoSetDenseReader, monitors RF noise and interference and then automatically
                // and continuously optimizes the reader’s configuration
                settings.ReaderMode = ReaderMode.AutoSetDenseReader;
                settings.SearchMode = SearchMode.DualTarget;
                settings.Session = 2;

                // Enable antenna #1. Disable all others.
                //settings.Antennas.DisableAll();
                //settings.Antennas.GetAntenna(1).IsEnabled = true;

                // Enable All Antennas.
                settings.Antennas.EnableAll();


                // Set the Transmit Power and 
                // Receive Sensitivity to the maximum.
                settings.Antennas.GetAntenna(1).MaxTxPower = true;
                settings.Antennas.GetAntenna(1).MaxRxSensitivity = true;
                // You can also set them to specific values like this...
                //settings.Antennas.GetAntenna(1).TxPowerInDbm = 20;
                //settings.Antennas.GetAntenna(1).RxSensitivityInDbm = -70;

                // Apply the newly modified settings.
                reader.ApplySettings(settings);

                // Assign the TagsReported event handler.
                // This specifies which method to call
                // when tags reports are available.
                reader.TagsReported += OnTagsReported;

                // Start reading.
                reader.Start();

                // Wait for the user to press enter.
               // Console.WriteLine("Press enter to exit.");
            //    Console.ReadLine();

                // Stop reading.
             //  reader.Stop();

                // Disconnect from the reader.
             //   reader.Disconnect();
            }
            catch (OctaneSdkException e)
            {
                // Handle Octane SDK errors.
                Console.WriteLine("Octane SDK exception: {0}", e.Message);
            }
            catch (Exception e)
            {
                // Handle other .NET errors.
                Console.WriteLine("Exception : {0}", e.Message);
            }

        }

        public static string tagID = "";
        public static void OnTagsReported(ImpinjReader sender, TagReport report)
        {
            // This event handler is called asynchronously 
            // when tag reports are available.
            // Loop through each tag in the report 
            // and print the data.
			
            foreach (Tag t in report)
            {

                tagID = whichAnimalPictureontheCube(t, t.Epc.ToString());
                Console.WriteLine("Send to debug output." + tagID);
            }
        }
     
        public static string whichAnimalPictureontheCube(Tag _tag, string _tagString)
        {

            //Debug.WriteLine(_tagString);

            if (_tagString.Contains("CB01 1111") == true)
            {
                Debug.WriteLine("Tiger");
                side = "Tiger";
                eventName = "CubeSide.Tiger";
                if (iristkActivated == true)
                    SendDataToBroker(eventName);
            }

            else if (_tagString.Contains("CB01 2222") == true)
            {
                Debug.WriteLine("Pig");
                side = "Pig";
                eventName = "CubeSide.Pig";
                if (iristkActivated == true)
                    SendDataToBroker(eventName);
            }

            else if (_tagString.Contains("CB01 3333") == true)
            {
                Debug.WriteLine("Elephant");
                side = "Elephant";
                eventName = "CubeSide.Elephant";
                if (iristkActivated == true)
                    SendDataToBroker(eventName);
            }

            else if (_tagString.Contains("CB01 4444") == true)
            {
                Debug.WriteLine("Dog");
                side = "Dog";
                eventName = "CubeSide.Dog";
                if (iristkActivated == true)
                    SendDataToBroker(eventName);
            }
            else if (_tagString.Contains("CB01 5555") == true)
            {
                Debug.WriteLine("Cat");
                side = "Cat";
                eventName = "CubeSide.Cat";
                if (iristkActivated == true)
                    SendDataToBroker(eventName);
            }

            else if (_tagString.Contains("CB01 6666") == true)
            {
                Debug.WriteLine("Monkey");
                side = "Monkey";
                eventName = "CubeSide.Monkey";
                if (iristkActivated == true)
                    SendDataToBroker(eventName);
            }

            return side;
           
        }

        public static void SendDataToBroker(string eventName)
        {
            string sendEventHeader2 = "EVENT " + eventName;
            string sampleSayEvent2 =
                    "{\"class\" : \"iristk.system.Event\", " +
                    "\"event_name\" : \"" + eventName + "\", " +
                    "\"event_id\" : \"my_unique_id_123\"}";

            //Send the header of the event and the number of bytes of the JSON event to furhatOS
            Send(sendEventHeader2 + " " + ASCIIEncoding.ASCII.GetByteCount(sampleSayEvent2) + "\n", socket);

            //Send the actual event and make furhat say Hello there
            Send(sampleSayEvent2, socket);

            // Console.WriteLine("JSON Event Sent to the Broker    +   " + eventName);

        }
        public static void Send(string msg, TcpClient socket)
        {
            //Provides the underlying stream of data for network access
            NetworkStream netStream = socket.GetStream();

            //Converts the intended message to byte format
            Byte[] sendBytes = Encoding.ASCII.GetBytes(msg);

            //Sends the message over the network
            netStream.Write(sendBytes, 0, sendBytes.Length);
        }

        static bool iristkActivated = false;  //note that it should be false    
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //Creates a Tcp Client that connects with Kaspar
            socket = new TcpClient("localhost", 1932);

            //Build a message to connect to furhat over TCP
            Send("CONNECT furhat RFID\n", socket);

            Console.WriteLine("The irisTK Connection Activated");
            iristkActivated = true;
            btnConnect.Content = "Connected";
        }
         
    }
}
