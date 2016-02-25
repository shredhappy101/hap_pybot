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
        private static string _userName;
        private static string _channel;
        private static Stopwatch _stopwatch;
        private static TcpClient _tcpClient = null;
        private static StreamReader _inputStream;
        private static StreamWriter _outputStream;
        private static bool _throttled;
        private static int _messageCounter = 0;
        public static bool Connected;
        #endregion

        #region Constructor
        public static void IrcStart(string ip, int port, string user, string password)
        {
            _userName = user;
            Connected = InitTcp(ip, port, user, password);
            _stopwatch = new Stopwatch();
        }
        #endregion

        #region Functions
        public static void JoinRoom(string ch)
        {
            _channel = ch;
            _outputStream.WriteLine("JOIN #" + _channel);
            _outputStream.Flush();
        }

        private static void SendIrcMessage(string message)
        {
            _outputStream.WriteLine(message);
            _outputStream.Flush();
        }

        public static void SendChatMessage(string message)
        {
            try
            {
                if (!_throttled)
                {
                    if (_stopwatch.Elapsed.Seconds >= 30)
                    {
                        _stopwatch.Stop();
                        _messageCounter = 0;
                    }

                    SendIrcMessage(":" + _userName + "!" + _userName + "@" + _userName +
                    ".tmi.twitch.tv PRIVMSG #" + _channel + " :" + message);
                    _messageCounter++;

                    if (_messageCounter == 1) _stopwatch.Start();
                }

                while (_messageCounter == 19)
                {
                    if (!_throttled)
                    {
                        SendIrcMessage(":" + _userName + "!" + _userName + "@" + _userName +
                        ".tmi.twitch.tv PRIVMSG #" + _channel + " :" + "/me " + "So fast! Stopping for " +
                        (30 - _stopwatch.Elapsed.Seconds) + "second(s)");
                    }

                    _throttled = true;

                    if (_stopwatch.Elapsed.Seconds >= 30)
                    {
                        _stopwatch.Stop();
                        _throttled = false;
                        _messageCounter = 0;
                    }
                    Thread.Sleep(20);
                }
            }
            catch (IOException) { return; }
        }

        public static void Pong()
        {
            _outputStream.WriteLine("PONG tmi.twitch.tv\r\n");
            _outputStream.Flush();
        }

        public static string ReadMessage()
        {
            string message;
            try
            {
                message = _inputStream.ReadLine();
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
                _tcpClient = new TcpClient(ip, port);
            }
            catch (SocketException)
            {
                MessageBox.Show("Unable to get stream! Ip Blocker?");
                return false;
            }

            _inputStream = new StreamReader(_tcpClient.GetStream());
            _outputStream = new StreamWriter(_tcpClient.GetStream());

            _outputStream.WriteLine("PASS " + password);
            _outputStream.WriteLine("NICK " + _userName);
            _outputStream.WriteLine("USER " + _userName + " 8 * :" + _userName);
            _outputStream.Flush();

            return true;
        }
        #endregion
    }
}
