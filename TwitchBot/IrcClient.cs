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
        private static string _userName, _channel;
        private string chatStr;
        private Stopwatch stopwatch;
        private TcpClient tcpClient;
        private StreamReader inputStream;
        private StreamWriter outputStream;
        private bool throttled;
        private int messageCounter;
        public bool Connected;
        #endregion

        #region Constructor
        public IrcClient(string ip, int port, string user, string password)
        {
            _userName = user;
            Connected = InitTcp(ip, port, password);
            stopwatch = new Stopwatch();
            
        }
        #endregion

        #region Functions
        public void JoinRoom(string ch)
        {
            _channel = ch;
            outputStream.WriteLine("JOIN #" + _channel);
            outputStream.Flush();
        }

        private void SendIrcMessage(string message)
        {
            outputStream.WriteLine(message);
            outputStream.Flush();
        }

        public void SendChatMessage(string message)
        {
            chatStr = ":" + _userName + "!" + _userName + "@" + _userName + ".tmi.twitch.tv PRIVMSG #" + _channel + " :";

            try
            {
                if (!throttled)
                {
                    if (stopwatch.Elapsed.Seconds >= 30)
                    {
                        stopwatch.Stop();
                        messageCounter = 0;
                    }

                    SendIrcMessage(chatStr + message);
                    messageCounter++;

                    if (messageCounter == 1) stopwatch.Start();
                }

                while (messageCounter == 19)
                {
                    if (!throttled)
                    {
                        SendIrcMessage(chatStr + "/me " + "So fast! Stopping for " +
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
            catch (IOException) {
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
            catch (Exception)
            {
                return null;
            }
            return message;
        }

        private bool InitTcp(string ip, int port, string password)
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
            outputStream.WriteLine("NICK " + _userName);
            outputStream.WriteLine("USER " + _userName + " 8 * :" + _userName);
            outputStream.Flush();

            return true;
        }
        #endregion
    }
}
