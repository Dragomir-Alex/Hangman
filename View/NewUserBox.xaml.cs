using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Hangman.View
{
    /// <summary>
    /// Interaction logic for NewUserBox.xaml
    /// </summary>
    public partial class NewUserBox : Window
    {
        public NewUserBox()
        {
            InitializeComponent();
        }

        public List<string> GetInputList()
        {
            var inputs = new List<string>();
            inputs.Add(PlayerName.Text);
            inputs.Add(ProfilePicturePath.Text);
            return inputs;
        }
        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Regex.IsMatch(PlayerName.Text, @"^[a-zA-Z0-9]+$"))
            {
                MessageBox.Show("The player name contains unpermitted characters!");
                return;
            }
            NewUserBoxWindow.Close();
        }
        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files(*png;*.jpg;*.gif;*.bmp)|*png;*.jpg;*.jpeg;.bmp;";

            if (openFileDialog.ShowDialog() == true)
            {
                ProfilePicturePath.Text = openFileDialog.FileName;
            }

        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
        }
    }
}
