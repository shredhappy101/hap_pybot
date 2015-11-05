using System;
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
        private static string userName;
        private static string channel;
        private static Stopwatch stopwatch = new Stopwatch();
        private static TcpClient tcpClient = null;
        private static StreamReader inputStream;
        private static StreamWriter outputStream;
        private static bool throttled;
        private static int messageCounter = 0;
        public static bool connected;
        #endregion

        #region Constructor
        public static void IrcStart(string ip, int port, string user, string password)
        {
            userName = user;
            connected = InitTcp(ip, port, user, password);
        }
        #endregion

        #region Functions
        public static void JoinRoom(string ch)
        {
            channel = ch;
            outputStream.WriteLine("JOIN #" + channel);
            outputStream.Flush();
        }

        private static void SendIrcMessage(string message)
        {
            outputStream.WriteLine(message);
            outputStream.Flush();
        }

        public static void SendChatMessage(string message)
        {
            try
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
            catch (IOException) { return; }        
        }

        public static void Pong()
        {
            outputStream.WriteLine("PONG tmi.twitch.tv\r\n");
            outputStream.Flush();
        }

        public static string ReadMessage()
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

        private static bool InitTcp(string ip, int port, string user, string password)
        {
            try
            {
                tcpClient = new TcpClient(ip, port);
            }
            catch (SocketException)
            {
                MessageBox.Show("Unable to get stream! Ip Blocker?");
                return false;
            }

            inputStream = new StreamReader(tcpClient.GetStream());
            outputStream = new StreamWriter(tcpClient.GetStream());

            outputStream.WriteLine("PASS " + password);
            outputStream.WriteLine("NICK " + userName);
            outputStream.WriteLine("USER " + userName + " 8 * :" + userName);
            outputStream.Flush();

            return true;
        }
        #endregion

    }
}
