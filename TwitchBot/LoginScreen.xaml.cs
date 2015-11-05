using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
namespace TwitchBot
{
    public partial class LoginScreen : Window
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
            this.DragMove();
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

        private string LoadAuth()
        {
            string namesTmp = @AppDomain.CurrentDomain.BaseDirectory + "auth.txt";
            if (File.Exists(namesTmp)) return File.ReadAllText(namesTmp);
            else return "";
        }

        private void SaveAuth()
        {
            File.WriteAllText(@AppDomain.CurrentDomain.BaseDirectory + "auth.txt", oauthTexBox.Text);
        }

        private void Go()
        {
            SaveAuth();
            MainWindow main = new MainWindow(channelTexBox.Text, oauthTexBox.Text);
            main.Show();
            this.Close();
        }
    }
}
