using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Hangman.ViewModel
{
    class HangmanWord
    {
        private string originalWord, currentWord;
        private bool isFinished;
        public string OriginalWord { get => originalWord; set => originalWord = value; }
        public string CurrentWord { get => currentWord; set => currentWord = value; }
        public bool IsFinished { get => isFinished; set => isFinished = value; }
        private bool IsAlpha(string str)
        {
            return !Regex.IsMatch(str, @"[^a-zA-Z\s]");
        }
        public HangmanWord(string newWord)
        {
            if (!IsAlpha(newWord))
                return;
            newWord = newWord.ToUpper();
            OriginalWord = newWord;
            CurrentWord = "";
            for (int i = 0; i < OriginalWord.Length; ++i)
                if (newWord[i] != ' ')
                    CurrentWord += '_';
                else CurrentWord += ' ';
        }

        private void CheckFinished()
        {
            foreach (char chr in currentWord)
                if (chr == '_')
                    return;
            isFinished = true;
        }

        public bool TryLetter(string letter)
        {
            if (letter.Length != 1)
                return false;

            char chrLetter = letter[0];
            bool found = false;

            for (int i = 0; i < OriginalWord.Length; ++i)
                if (originalWord[i] == chrLetter)
                {
                    found = true;

                    StringBuilder sb = new StringBuilder(currentWord);
                    sb[i] = originalWord[i];
                    currentWord = sb.ToString();
                }

            CheckFinished();
            return found;
        }
    }
}
