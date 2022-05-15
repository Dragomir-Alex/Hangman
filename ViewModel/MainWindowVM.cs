using Hangman.Model;
using Hangman.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Hangman.ViewModel
{
    class MainWindowVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<UserModel> PlayerList { get; set; }

        private string playerImage;
        public string PlayerImage
        {
            get => playerImage;
            set => SetProperty(ref playerImage, value);
        }

        public static UserModel selectedPlayer;
        public UserModel SelectedPlayer
        {
            get => selectedPlayer;
            set
            {
                SetProperty(ref selectedPlayer, value);
                if (selectedPlayer == null)
                    return;

                string[] formats = { ".png", ".bmp", ".gif", ".jpg" };
                foreach (string format in formats)
                {
                    string path = Directory.GetCurrentDirectory() + "/Data/Images/" + SelectedPlayer.UserName + format;
                    if (File.Exists(path))
                        PlayerImage = path;
                }
                
            }
        }

        private ICommand m_newUser;
        private ICommand m_deleteUser;
        private ICommand m_play;
        public MainWindowVM()
        {
            PlayerList = new ObservableCollection<UserModel>();
            PlayerImage = "pack://application:,,,/Resources/NoImage.png";

            if (!Directory.Exists("./Data"))
            {
                Directory.CreateDirectory("./Data");
                Directory.CreateDirectory("./Data/Images");
                Directory.CreateDirectory("./Data/Users");
            }
            string dir = "./Data/Users";

            string[] subdirectoryEntries = Directory.GetDirectories(dir);
            foreach (string subdirectory in subdirectoryEntries)
            {
                string playerName = subdirectory.Substring(subdirectory.IndexOf(@"\") + 1);
                PlayerList.Add(new UserModel { UserName = playerName });
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void NewUser(object parameter)
        {
            var newUserBox = new NewUserBox();
            newUserBox.ShowDialog();

            string playerName = newUserBox.GetInputList()[0];
            if (playerName == "")
                return;

            for (int i = 0; i < PlayerList.Count(); ++i)
                if (PlayerList[i].UserName == playerName)
                {
                    MessageBox.Show("A player with this name already exists!");
                    return;
                }

            string userDirectory = "./Data/Users/" + playerName;
            System.IO.Directory.CreateDirectory(userDirectory);

            string profilePicturePath = newUserBox.GetInputList()[1];
            if (profilePicturePath != "")
            {
                string pictureFormat = profilePicturePath.Substring(profilePicturePath.LastIndexOf('.'));
                string sourcePath = profilePicturePath;
                string destinationPath = "./Data/Images/" + playerName + pictureFormat;

                System.IO.File.Copy(sourcePath, destinationPath, true);
                PlayerList.Add(new UserModel { UserName = playerName });
            }
            else
            {
                System.IO.File.Copy("../../Resources/NoImage.png", "./Data/Images/" + playerName + ".png");
                PlayerList.Add(new UserModel { UserName = playerName });
            }

            OnPropertyChanged("PlayerList");
        }

        public void DeleteUser(object parameter)
        {
            if (SelectedPlayer == null)
                return;

            var result = MessageBox.Show("You're about to delete player " + SelectedPlayer.UserName + ". Are you sure?", "Warning", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                string deletedUser = SelectedPlayer.UserName;
                PlayerList.Remove(SelectedPlayer);
                SelectedPlayer = null;
                PlayerImage = null;

                Directory.Delete("./Data/Users/" + deletedUser, true);
            }         
            else return;
        }

        public void Play(object parameter)
        {
            if (SelectedPlayer != null)
            {
                var vm = new HangmanGameVM();
                var hangmanWindow = new HangmanGame
                {
                    DataContext = vm
                };
                vm.OnRequestClose += (s, e) => hangmanWindow.Close();

                hangmanWindow.ShowDialog();
            }
        }

        public ICommand NewUserCommand
        {
            get
            {
                if (m_newUser == null)
                    m_newUser = new RelayCommand(NewUser);
                return m_newUser;
            }
        }

        public ICommand DeleteUserCommand
        {
            get
            {
                if (m_deleteUser == null)
                    m_deleteUser = new RelayCommand(DeleteUser);
                return m_deleteUser;
            }
        }

        public ICommand PlayCommand
        {
            get
            {
                if (m_play == null)
                    m_play = new RelayCommand(Play);
                return m_play;
            }
        }

        protected bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (!Equals(field, newValue))
            {
                field = newValue;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                return true;
            }

            return false;
        }
    }
}
