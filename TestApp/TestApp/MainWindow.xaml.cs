using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace TestApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public int gamestate;
        public string LocalPlayerName;
        public Game ActiveGame;
        public Dictionary<string,Deck> playerDecks;
        public MainWindowFunctions windowFunctions;
        public DispatcherTimer timer;
        public int bookmark;
        public MainWindow()
        {
            InitializeComponent();
            timer.Start();
            ActiveGame = new Game();
            playerDecks = new Dictionary<string, Deck>();
            windowFunctions = new MainWindowFunctions();
            var dir = "S:\\AVAC\\Arena\\MTGArena\\MTGA_Data\\Logs\\Logs\\";
            var directory = new DirectoryInfo(dir);
            var myFile = (from f in directory.GetFiles()
                          orderby f.LastWriteTime descending
                          select f).First();
            Console.Out.WriteLine(myFile.Name);
            // Need to find [UnityCrossThreadLogger]<== Deck.GetDeckListsV3
            string[] lines = System.IO.File.ReadAllLines(dir + myFile.Name);
            //Console.Out.WriteLine(lines.Length);
            string deckIDLine = "Currently Empty";
            foreach (string s in lines)
            {
                if (s.Contains("[Accounts - Client] Successfully logged in to account: "))
                {
                    string[] x = s.Split(new string[] { "[Accounts - Client] Successfully logged in to account: " }, StringSplitOptions.None);
                    LocalPlayerName = x[1];
                    //Console.WriteLine("Player Name:" + LocalPlayerName);
                }
                if (s.Contains("[UnityCrossThreadLogger]<== Deck.GetDeckListsV3"))
                {
                    deckIDLine = s;
                    break;
                }
                
            }
            // Console.Out.WriteLine(deckIDLine);
            string[] split = deckIDLine.Split(new string[] { "\"payload\":[" }, StringSplitOptions.None);
            split[1] = split[1].Substring(0, split[1].Length - 2);
            //Console.Out.WriteLine(split[1]);
            string[] deckjsons = split[1].Split(new string[] { "{\"commandZoneGRPIds\"" }, StringSplitOptions.None);

            for (int i = 1; i < deckjsons.Length; i++)
            {
               // Console.Out.WriteLine("{\"commandZoneGRPIds\"" + deckjsons[i]);
                if (deckjsons[i].Substring(deckjsons[i].Length - 1) == ",")
                {
                    deckjsons[i] = deckjsons[i].Substring(0, deckjsons[i].Length - 1);
                }
                Deck tempDeck = DecklistsProcessr.generateDeckCode("{\"commandZoneGRPIds\"" + deckjsons[i]);
                playerDecks.Add(tempDeck.id, tempDeck);
            }
            windowFunctions.showWins(playerDecks);
            // In Game Status is status code 7 8 is post game 6 is game load. 6 -> 7 -> 8 
            getGameState(lines);
        }
        
        public void getGameState(string[] lines)
        {
            bookmark = lines.Length;
            ActiveGame.playerName = LocalPlayerName;
            for (int i = 0; i < lines.Length; i++)
            {
                if (gamestate == 0)
                {
                    if (lines[i].Contains("[UnityCrossThreadLogger]<== Event.DeckSubmitV3"))
                    {
                        string[] temp = lines[i].Split(new string[] { "],\"id\":\"" }, StringSplitOptions.None);
                        temp = temp[1].Split('"');
                        string activeID = temp[0];
                        ActiveGame.activeDeck = playerDecks[activeID];
                        gamestate = 1;
                    }
                }
                else if (gamestate == 1)
                {
                    if (lines[i].Contains(",\"new\":"))
                    {
                        string[] temp = lines[i].Split(new string[] { ",\"new\":" }, StringSplitOptions.None);
                        if (temp[1].Contains("5"))
                        {
                            string info = lines[i + 2];
                            temp = info.Split(new string[] { ", \"playerName\": \"" }, StringSplitOptions.None);
                            if (temp[1].Contains(LocalPlayerName))
                            {
                                ActiveGame.teamID = 1;

                            }
                            else
                            {
                                ActiveGame.teamID = 2;
                            }

                            Console.Out.WriteLine(ActiveGame.playerName + " " + ActiveGame.teamID + " " + ActiveGame.activeDeck.id);
                            gamestate = 2;
                        }
                    }
                }
                else if (gamestate == 2)
                {
                    if (lines[i].Contains(",\"new\":"))
                    {
                        string[] temp = lines[i].Split(new string[] { ",\"new\":" }, StringSplitOptions.None);
                        if (temp[1].Contains("8"))
                        {
                            string info = lines[i - 1];
                            temp = info.Split(new string[] { "\"winningTeamId\": " }, StringSplitOptions.None);
                            if (temp[1].Contains(ActiveGame.teamID.ToString()))
                            {
                                playerDecks[ActiveGame.activeDeck.id].wins++;
                            }
                            else
                            {

                            }
                            playerDecks[ActiveGame.activeDeck.id].games++;
                            gamestate = 0;
                            windowFunctions.showWins(playerDecks);
                        }
                    }
                }
            }
        }
    }
}
