using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace TestApp
{
    public class MainWindowFunctions
    {
        public void showWins(Dictionary<string, Deck> playerDecks)
        {
            foreach (KeyValuePair<string, Deck> entry in playerDecks)
            {
                Console.Out.WriteLine(entry.Key + " has " + entry.Value.wins + " Wins in " + entry.Value.games + " games.");
            }
        }
        public void writeToFile(Dictionary<string, Deck> decks)
        {
            List<string> entries = new List<string>();
            foreach (KeyValuePair<string, Deck> entry in decks)
            {
                entries.Add(entry.Key + "|" + entry.Value.games + "|" + entry.Value.wins);
            }
        }
    }
}
