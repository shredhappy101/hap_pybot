using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Windows;

namespace TwitchBot
{
    public class IrcClient
    {
        #region Globals
        private string userName;
        private string channel;
        private Stopwatch stopwatch = new Stopwatch();
        private TcpClient tcpClient;
        private StreamReader inputStream;
        private StreamWriter outputStream;
        private bool throttled;
        private int messageCounter = 0;
        public bool connected;
        #endregion

        #region Constructor
        public IrcClient(string ip, int port, string userName, string password)
        {

            this.userName = userName;
            try
            {
                tcpClient = new TcpClient(ip, port);

            }
            catch (SocketException)
            {

                MessageBox.Show("Unable to get stream!");
                connected = false;
                return;
                
            }
            connected = true;
            inputStream = new StreamReader(tcpClient.GetStream());
            outputStream = new StreamWriter(tcpClient.GetStream());

            outputStream.WriteLine("PASS " + password);
            outputStream.WriteLine("NICK " + userName);
            outputStream.WriteLine("USER " + userName + " 8 * :" + userName);
            outputStream.Flush();
        }
        #endregion

        #region Functions
        public void JoinRoom(string channel)
        {
            this.channel = channel;
            outputStream.WriteLine("JOIN #" + channel);
            outputStream.Flush();
        }

        private void SendIrcMessage(string message)
        {
            outputStream.WriteLine(message);
            outputStream.Flush();
        }

        public void SendChatMessage(string message)
        {
            if (!throttled)
            {
                if (stopwatch.Elapsed.Seconds >= 30)
                {
                    stopwatch.Stop();
                    messageCounter = 0;
                }

                SendIrcMessage(":" + userName + "!" + userName + "@" + userName +
                ".tmi.twitch.tv PRIVMSG #" + channel + " :" + message);
                messageCounter++;

                if (messageCounter == 1) stopwatch.Start();

            }

            while (messageCounter == 19)
            {
                if (!throttled)
                {
                    SendIrcMessage(":" + userName + "!" + userName + "@" + userName +
                    ".tmi.twitch.tv PRIVMSG #" + channel + " :" + "/me " + "So fast! Stopping for " +
                    (30 - stopwatch.Elapsed.Seconds) + "second(s)");
                }

                throttled = true;

                if (stopwatch.Elapsed.Seconds >= 30)
                {
                    stopwatch.Stop();
                    throttled = false;
                    messageCounter = 0;
                }
                Thread.Sleep(20);
            }
        }

        public void Pong()
        {
            outputStream.WriteLine("PONG tmi.twitch.tv\r\n");
            outputStream.Flush();
        }

        public string ReadMessage()
        {
            string message;
            try
            {
                message = inputStream.ReadLine();
            }
            catch (System.Exception)
            {
                return null;
            }
            return message;
        }
        #endregion

    }
}
