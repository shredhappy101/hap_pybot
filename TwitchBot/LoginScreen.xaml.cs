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
            MainWindow main = new MainWindow(channelTexBox.Text, oauthTexBox.Text);
            main.Show();

            this.Close();
        }

        private void join_Enter(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                MainWindow main = new MainWindow(channelTexBox.Text, oauthTexBox.Text);
                main.Show();

                this.Close();
            }
        }
    }
}
