using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
namespace TwitchBot
{
    public partial class LoginScreen
    {
        private string dir = @AppDomain.CurrentDomain.BaseDirectory + "auth.txt";             

        public LoginScreen()
        {
            InitializeComponent();
            channelTextBox.Focus();
            oauthTexBox.Text = LoadAuth();
        }
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            DragMove();
        }
        private void join_Enter(object s, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) Go();
        }
        private void Go()
        {
            SaveAuth();
            var main = new MainWindow(channelTextBox.Text, oauthTexBox.Text);
            main.Show();
            Close();
        }
        private void closeButton_Click(object s, RoutedEventArgs e) => Application.Current.Shutdown();
        private void loginButton_Click(object s, MouseButtonEventArgs e) => Go();
        private string LoadAuth() => File.Exists(dir) ? File.ReadAllText(dir) : "";
        private void SaveAuth() => File.WriteAllText(dir, oauthTexBox.Text);
    }
}
