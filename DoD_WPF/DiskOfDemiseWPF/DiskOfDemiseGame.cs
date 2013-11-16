using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiskOfDemiseWPF
{
    class DiskOfDemiseGame
    {

        private ArrayList Players = new ArrayList();
        private Player player0, player1, player2, player3;
        private int playerIndex;
        private Player currentPlayer;
        private ArrayList phrases = new ArrayList();
        private String phraseToGuess;
        private StringBuilder displayedPhrase;
        private bool correctGuess = false;
        private String bodyPart= "";

        public DiskOfDemiseGame()
        {
            addPlayers();
            addPhrases();
            assignPhrase();
        }

        //Add players to arrayList
        public void addPlayers()
        {
            player0 = new Player("Yellow");
            player1 = new Player("Red");
            player2 = new Player("Blue");
            player3 = new Player("Green");

            Players.Add(player0);
            Players.Add(player1);
            //Players.Add(player2);
            //Players.Add(player3);
            currentPlayer = player0;
            playerIndex = 0;
        }

        public void addPhrases()
        {
            phrases.Add(" H E L L O   W O R L D   S U N");
            phrases.Add(" T H E  C A T  I N  T H E  H A T");
            phrases.Add(" G O O D  E V E N I N G");
            phrases.Add(" H O W  A R E  Y O U  T O D A Y");
        }

        //Randomly assign a phrase from the arrayList to phraseToGuess & displayedPhrase
        public void assignPhrase()
        {
            displayedPhrase = new StringBuilder();
            Random random = new Random();
            int randomNumber = random.Next(0, phrases.Count);
            phraseToGuess = phrases[randomNumber] as string;
            for (int i = 0; i < phraseToGuess.Length; i++)
            {
                if(phraseToGuess[i] == ' ')
                {
                    displayedPhrase.Append(" ");
                }
                else
                {
                    displayedPhrase.Append("_");
                }
            }
        }

        public void setBodyPart(String bodyPart)
        {
            this.bodyPart = bodyPart;
        }

        //Check guessed letter in phrase
        public void checkLetterInPhrase(char character)
        {
            for(int i = 0; i < phraseToGuess.Length; i++)
            {
                if (displayedPhrase[i] == '_')
                {
                    if (phraseToGuess[i] == character)
                    {
                        displayedPhrase[i] = character;
                        correctGuess = true;
                    }
                }
            }
            if (!correctGuess)
            {
                //Lose Limb
                currentPlayer.removeLimb(bodyPart);
            }
            else
            {
                correctGuess = false;
            }

            //Console.WriteLine(displayedPhrase);
            if (!checkEndGame())
            {
                //Next Turn
                playerIndex++;
                if (playerIndex >= Players.Count)
                {
                    playerIndex = 0;
                }
                currentPlayer = (Player) Players[playerIndex];
            }
        }

        public bool checkEndGame()
        {
            bool finished = true;
            for (int i = 0; i < phraseToGuess.Length; i++)
            {
                if (displayedPhrase[i] == '_')
                {
                    finished = false;
                }
            }
            if (finished)
            {
                assignPhrase();
                Console.WriteLine("Game Ended");
                return true;
            }
            else
            {
                //Console.WriteLine("Not Over Yet");
                return false;
            }
        }

        public String displayPhrase()
        {
            Console.WriteLine(displayedPhrase.ToString());
            return displayedPhrase.ToString();
        }

        public String displayName()
        {
            return currentPlayer.returnColor();
        }

        public ArrayList returnBodyParts()
        {
            return currentPlayer.returnBodyParts();
        }
    }
}
