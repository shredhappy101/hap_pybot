using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
namespace TwitchBot
{
    public partial class LoginScreen
    {
        public LoginScreen()
        {
            InitializeComponent();
            channelTexBox.Focus();
            oauthTexBox.Text = LoadAuth();
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            DragMove();
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void loginButton_Click(object sender, MouseButtonEventArgs e)
        {
            Go();
        }

        private void join_Enter(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Go();
            }
        }

        private static string LoadAuth()
        {
            var namesTmp = @AppDomain.CurrentDomain.BaseDirectory + "auth.txt";
            return File.Exists(namesTmp) ? File.ReadAllText(namesTmp) : "";
        }

        private void SaveAuth()
        {
            File.WriteAllText(@AppDomain.CurrentDomain.BaseDirectory + "auth.txt", oauthTexBox.Text);
        }

        private void Go()
        {
            SaveAuth();
            var main = new MainWindow(channelTexBox.Text, oauthTexBox.Text);
            main.Show();
            Close();
        }
    }
}
