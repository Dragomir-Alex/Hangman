using Hangman.Model;
using Hangman.View;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Hangman.ViewModel
{
    class HangmanGameVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private int selectedCategory, remainingAttempts, winCount, lossCount, level;
        private string word, hangmanImage, winLossMessage;
        private int timer;
        private System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
        private List<string> wordsList = new List<string>();
        private List<string> usedLetters = new List<string>();
        public event EventHandler OnRequestClose;

        public int SelectedCategory
        {
            get => selectedCategory;
            set
            {
                SetProperty(ref selectedCategory, value);

                Word = "";
                WinLossMessage = "";
                isGameRunning = false;
                Level = 1;
                Timer = 0;
                RemainingAttempts = 6;
                usedLetters.Clear();
                StopTimer();
                InitializeLetters();
                UpdateImage();

                OnPropertyChanged("Word");
            }
        }
        public int RemainingAttempts { get => remainingAttempts; set => SetProperty(ref remainingAttempts, value); }
        public string HangmanImage { get => hangmanImage; set => SetProperty(ref hangmanImage, value); }
        public string WinLossMessage { get => winLossMessage; set => SetProperty(ref winLossMessage, value); }
        public int Timer { get => timer; set => SetProperty(ref timer, value); }
        public int Level { get => level; set => SetProperty(ref level, value); }
        public UserModel selectedPlayer;

        public string Word
        {
            get => word;
            set
            {
                string spacelessWord = value;
                string spaceWord = "";
                foreach (var chr in spacelessWord)
                    spaceWord = spaceWord + chr + ' ';
                word = spaceWord;
                OnPropertyChanged("Word");
            }
        }
        public ObservableCollection<LetterModel> Letters { get; set; }
        public HangmanWord hangmanWord;
        public bool isGameRunning;

        private ICommand m_newGame;
        private ICommand m_saveGame;
        private ICommand m_openGame;
        private ICommand m_statistics;
        private ICommand m_exit;
        private ICommand m_pressLetter;
        private ICommand m_about;
        

        public HangmanGameVM()
        {
            UpdateImage();
            Word = "";
            isGameRunning = false;
            SelectedCategory = 1;
            Level = 1;
            InitializeLetters();
            selectedPlayer = MainWindowVM.selectedPlayer;
            InitializeStats();
            usedLetters.Clear();

            dispatcherTimer.Tick += new EventHandler(DispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);

            OnPropertyChanged("Word");
        }

        private void InitializeStats()
        {
            string path = Directory.GetCurrentDirectory() + "/Data/Users/" + selectedPlayer.UserName + "/stats.txt";
            if (!File.Exists(path))
            {
                lossCount = 0;
                winCount = 0;
                using (StreamWriter sw = File.CreateText(path))
                {
                    sw.WriteLine(0);
                    sw.WriteLine(0);
                }
            }
            else
            {
                List<string> savedData = new List<string>();
                using (StreamReader sr = File.OpenText(path))
                {
                    string s = "";
                    while ((s = sr.ReadLine()) != null)
                    {
                        savedData.Add(s);
                    }
                }
                if (savedData.Count == 2)
                {
                    winCount = int.Parse(savedData[0]);
                    lossCount = int.Parse(savedData[1]);
                }
            }
        }

        private void UpdateStats()
        {
            string path = Directory.GetCurrentDirectory() + "/Data/Users/" + selectedPlayer.UserName + "/stats.txt";
            using (StreamWriter sw = new StreamWriter(path))
            {
                sw.WriteLine(winCount);
                sw.WriteLine(lossCount);
            }
        }

        private void InitializeLetters()
        {
            char[] alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

            if (Letters != null && Letters.Count == 26)
            {
                int i = 0;
                foreach (char ch in alpha)
                {
                    Letters[i++].Letter = ch.ToString();
                }
            }

            Letters = new ObservableCollection<LetterModel>();
            foreach (char ch in alpha)
            {
                LetterModel letterModel = new LetterModel { Letter = ch.ToString(), IsEnabled = true };
                Letters.Add(letterModel);
            }

            OnPropertyChanged("Letters");
        }

        private void UpdateImage()
        {
            if (HangmanImage == null)
            {
                HangmanImage = "pack://application:,,,/Resources/hang1.png";
                return;
            }
            HangmanImage = "pack://application:,,,/Resources/hang" + (7 - remainingAttempts) + ".png";
            OnPropertyChanged("HangmanImage");
        }

        private void InitializeWords()
        {
            wordsList.Clear();
            if (selectedCategory == 1 || selectedCategory == 6)
                LoadWordsFromFile("../../Resources/anime.txt");
            if (selectedCategory == 1 || selectedCategory == 5)
                LoadWordsFromFile("../../Resources/states.txt");
            if (selectedCategory == 1 || selectedCategory == 4)
                LoadWordsFromFile("../../Resources/plants.txt");
            if (selectedCategory == 1 || selectedCategory == 3)
                LoadWordsFromFile("../../Resources/animals.txt");
            if (selectedCategory == 1 || selectedCategory == 2)
                LoadWordsFromFile("../../Resources/cars.txt");
        }

        private void LoadWordsFromFile(string filePath)
        {
            using (var inputFile = File.OpenText(filePath))
            {
                string text = inputFile.ReadToEnd();
                string[] lines = text.Split(Environment.NewLine.ToCharArray());

                foreach (string line in lines)
                {
                    if (line != "")
                    {
                        wordsList.Add(line);
                    }
                }
            }
        }

        private void ChooseWord()
        {
            Random rand = new Random();
            int randPos = rand.Next(wordsList.Count);
            hangmanWord = new HangmanWord(wordsList[randPos]);
            Word = hangmanWord.CurrentWord;
        }

        private void StartTimer()
        {
            dispatcherTimer.Start();
        }

        private void StopTimer()
        {
            dispatcherTimer.Stop();
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            Timer++;
            if (Timer >= 60)
            {
                StopTimer();
                Timer = 60;
                isGameRunning = false;
                WinLossMessage = "You lost!";
                Word = hangmanWord.OriginalWord;
                lossCount++;
                UpdateStats();
                Level = 1;
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void NewGame(object parameter)
        {
            if (SelectedCategory == 0)
            {
                MessageBox.Show("Select a category before starting the game!");
                return;
            }
            if (isGameRunning)
                Level = 1;
            RemainingAttempts = 6;
            InitializeWords();
            InitializeLetters();
            ChooseWord();
            UpdateImage();
            WinLossMessage = "";
            Timer = 0;
            usedLetters.Clear();
            StartTimer();
            isGameRunning = true;
        }

        private void SaveGame(object parameter)
        {
            if (isGameRunning != true)
                return;
            DirectoryInfo saveFileDirInfo = new DirectoryInfo("./Data/Users/" + selectedPlayer.UserName);
            int fileCount = saveFileDirInfo.GetFiles().Length;
            string saveName = "save" + fileCount;
            string filePath = "Data/Users/" + selectedPlayer.UserName + "/" + saveName + ".txt";

            string usedLettersStr = "";
            if (usedLetters.Count != 0)
            {
                foreach (var usedLetter in usedLetters)
                    usedLettersStr = usedLettersStr + usedLetter + ' ';
                usedLettersStr = usedLettersStr.Remove(usedLettersStr.Length - 1, 1);
            }

            if (!File.Exists(filePath))
            {
                using (StreamWriter sw = File.CreateText(filePath))
                {
                    sw.WriteLine(Level);
                    sw.WriteLine(RemainingAttempts);
                    sw.WriteLine(Timer);
                    sw.WriteLine(SelectedCategory);
                    sw.WriteLine(hangmanWord.CurrentWord);
                    sw.WriteLine(hangmanWord.OriginalWord);
                    sw.WriteLine(usedLettersStr);
                }
                MessageBox.Show("Saved successfully!");
            }          
        }

        private void OpenGame(object parameter)
        {
            string path = "Data\\Users\\" + selectedPlayer.UserName;
            string combinedPath = Path.Combine(Directory.GetCurrentDirectory(), path);
            List<string> savedData = new List<string>();

            var openFileDialog = new OpenFileDialog
            {
                Title = "Select a save file...",
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*"
            };
            openFileDialog.InitialDirectory = combinedPath;
            if (openFileDialog.ShowDialog() == true)
            {
                string selectedSave = openFileDialog.FileName;
                if (selectedSave.IndexOf(combinedPath) == -1)
                {
                    MessageBox.Show("You can't open the save files of other users!", "Open error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                using (StreamReader sr = File.OpenText(selectedSave))
                {
                    string s = "";
                    while ((s = sr.ReadLine()) != null)
                    {
                        savedData.Add(s);
                    }
                }
            }
            else return;
            if (savedData.Count() == 7)
            {
                isGameRunning = false;
                StopTimer();

                // Order ===> Level, RemainingAttempts, Timer, SelectedCategory, hangmanWord.CurrentWord, hangmanWord.OriginalWord

                SelectedCategory = int.Parse(savedData[3]);
                Level = int.Parse(savedData[0]);
                RemainingAttempts = int.Parse(savedData[1]);
                Timer = int.Parse(savedData[2]);

                if (hangmanWord == null)
                    hangmanWord = new HangmanWord("");
                hangmanWord.CurrentWord = savedData[4];
                hangmanWord.OriginalWord = savedData[5];
                Word = hangmanWord.CurrentWord;
                hangmanWord.IsFinished = false;

                string[] savedUsedLetters = savedData[6].Split(' ');
                usedLetters.Clear();
                if (savedUsedLetters[0] != "")
                {
                    foreach (var savedUserLetter in savedUsedLetters)
                    {
                        usedLetters.Add(savedUserLetter);
                        foreach (var letter in Letters)
                        {
                            if (letter.Letter[0] == savedUserLetter[0])
                            {
                                letter.Letter = "-";
                                letter.IsEnabled = false;
                                break;
                            }
                        }
                    }
                }

                StartTimer();
                UpdateImage();
                isGameRunning = true;
                OnPropertyChanged("Word");
            }
            else MessageBox.Show("The save file is invalid!", "Open error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void Statistics(object parameter)
        {
            MessageBox.Show("Wins: " + winCount + '\n' + "Losses: " + lossCount);
        }

        private void Exit(object parameter)
        {
            StopTimer();
            OnRequestClose(this, new EventArgs());
            //Environment.Exit(0);
            
        }

        private void About(object parameter)
        {
            var aboutBox = new AboutBox();
            aboutBox.ShowDialog();
        }

        private void PressLetter(object parameter)
        {
            if (!isGameRunning)
                return;

            var pressedLetter = parameter as LetterModel;
            if (pressedLetter.Letter == "-")
                return;
            if (!hangmanWord.TryLetter(pressedLetter.Letter))
                RemainingAttempts--;
            usedLetters.Add(pressedLetter.Letter);
            pressedLetter.Letter = "-";
            pressedLetter.IsEnabled = false;
            
            Word = hangmanWord.CurrentWord;

            if (hangmanWord.IsFinished)
            {
                StopTimer();
                isGameRunning = false;
                WinLossMessage = "You won!";
                if (Level == 5)
                {
                    winCount++;
                    UpdateStats();
                    Level = 1;
                }
                else Level++;
            }
            else if (RemainingAttempts == 0)
            {
                StopTimer();
                isGameRunning = false;
                WinLossMessage = "You lost!";
                Word = hangmanWord.OriginalWord;
                lossCount++;
                UpdateStats();
                Level = 1;
            }

            UpdateImage();

            OnPropertyChanged("Letters");
        }

        public ICommand NewGameCommand
        {
            get
            {
                if (m_newGame == null)
                    m_newGame = new RelayCommand(NewGame);
                return m_newGame;
            }
        }

        public ICommand SaveGameCommand
        {
            get
            {
                if (m_saveGame == null)
                    m_saveGame = new RelayCommand(SaveGame);
                return m_saveGame;
            }
        }

        public ICommand OpenGameCommand
        {
            get
            {
                if (m_openGame == null)
                    m_openGame = new RelayCommand(OpenGame);
                return m_openGame;
            }
        }

        public ICommand StatisticsCommand
        {
            get
            {
                if (m_statistics == null)
                    m_statistics = new RelayCommand(Statistics);
                return m_statistics;
            }
        }

        public ICommand ExitCommand
        {
            get
            {
                if (m_exit == null)
                    m_exit = new RelayCommand(Exit);
                return m_exit;
            }
        }

        public ICommand PressLetterCommand
        {
            get
            {
                if (m_pressLetter == null)
                    m_pressLetter = new RelayCommand(PressLetter);
                return m_pressLetter;
            }
        }

        public ICommand AboutCommand
        {
            get
            {
                if (m_about == null)
                    m_about = new RelayCommand(About);
                return m_about;
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