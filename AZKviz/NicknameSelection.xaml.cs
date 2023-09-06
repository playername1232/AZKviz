using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AZKviz
{
    /// <summary>
    /// Interaction logic for NicknameSelection.xaml
    /// </summary>
    public partial class NicknameSelection : Window
    {
        MainWindow _mainWindow;
        int _Count = 0;
        public NicknameSelection(MainWindow main, int count)
        {
            _mainWindow = main;
            ResizeMode = ResizeMode.NoResize;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            InitializeComponent();

            _Count = count;

            if (count == 1)
            {
                TwoPlayers_Canvas.Visibility = Visibility.Hidden;
                OnePlayer_Canvas.Visibility = Visibility.Visible;

                this.Height = 120;

                Confirm_Button.Margin = new Thickness(191, 48, 0, 0);
            }
            if (count == 2)
            {
                TwoPlayers_Canvas.Visibility = Visibility.Visible;
                OnePlayer_Canvas.Visibility = Visibility.Hidden;
            }
        }

        /// <summary>
        /// Event Handles Confirm Button click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Confirm_Button_Click(object sender, RoutedEventArgs e)
        {
            if(_Count == 2)
            {
                if (OrangePlayerNick_TextBox.Text.Length == 0 || BluePlayerNick_TextBox.Text.Length == 0)
                {
                    MessageBox.Show($"Musíte zadat jména obou hráčů!", "Chyba", MessageBoxButton.OK);
                    return;
                }

                _mainWindow._NickOrangePlayer = OrangePlayerNick_TextBox.Text;
                _mainWindow._NickBluePlayer = BluePlayerNick_TextBox.Text;

                _mainWindow.NamePlayerOrange_Label.Content = _mainWindow._NickOrangePlayer;
                _mainWindow.NamePlayerBlue_Label.Content = _mainWindow._NickBluePlayer;

            }
            else if(_Count == 1)
            {
                if(OnePlayer_TextBox.Text.Length == 0)
                {
                    MessageBox.Show($"Musíte zadat jméno hráče!", "Chyba", MessageBoxButton.OK);
                    return;
                }
                _mainWindow.StreamingNamePlayerOrange_Label.Content = OnePlayer_TextBox.Text;
            }

            this.Close();
        }

        /// <summary>
        /// Handles closing Window and enabling MainWindow
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _mainWindow.IsEnabled = true;
        }
    }
}
