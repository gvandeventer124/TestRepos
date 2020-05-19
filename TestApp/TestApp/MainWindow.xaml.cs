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

namespace TestApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<Deck> playerDecks;
        public int gamestate;
        public string LocalPlayerName;
        public Game ActiveGame;
        public MainWindow()
        {
            InitializeComponent();
            ActiveGame = new Game();
            playerDecks = new List<Deck>();
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
                if (s.Contains("[UnityCrossThreadLogger]<== Deck.GetDeckListsV3"))
                {
                    deckIDLine = s;
                    break;
                }
                if (s.Contains("[Accounts - Client] Successfully logged in to account: "))
                {
                    string[] x = s.Split(new string[] { "[Accounts - Client] Successfully logged in to account: " }, StringSplitOptions.None);
                    LocalPlayerName = x[1];
                    //Console.WriteLine("Player Name:" + LocalPlayerName);
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
                //playerDecks.Add(DecklistsProcessr.generateDeckCode("{\"commandZoneGRPIds\"" + deckjsons[i]));
            }
            foreach (Deck d in playerDecks)
            {
                //Console.Out.WriteLine("DeckCode: " + d.id);
            }
            // In Game Status is status code 7 8 is post game 6 is game load. 6 -> 7 -> 8 
            int bookmark = 0;
            for (int i = 0; i < lines.Length; i++)
            {

                if (gamestate == 0)
                {
                    if (lines[i].Contains("[UnityCrossThreadLogger]STATE CHANGED "))
                    {
                        bookmark = i;
                        string[] n = lines[i].Split(new string[] { "\"new\":" }, StringSplitOptions.None);
                        n[1] = n[1].Substring(0, 1);
                        if (n[1] == "6") //game is being initialized.
                        {
                            gamestate = 6;
                        }
                    }
                }
                else if (gamestate == 6)
                {
                    if (lines[i].Contains(": MatchGameRoomStateChangedEvent"))
                    {
                        bookmark = i;
                        string relevantInfo = lines[i + 1];
                        string[] relevantInfoSplit = relevantInfo.Split(new string[] { "\"reservedPlayers\": [ " }, StringSplitOptions.None);
                        //Console.WriteLine(relevantInfoSplit.Length);
                        string[] relevantReSplit = relevantInfoSplit[1].Split(new string[] { ", \"matchId\": " }, StringSplitOptions.None);
                        string[] jsons = relevantReSplit[0].Split(new string[] { "}, { \"userId\"" }, StringSplitOptions.None);
                        if (jsons[0].Contains(LocalPlayerName))
                        {
                            ActiveGame.playerName = LocalPlayerName;
                            ActiveGame.teamID = 1;
                        }
                        else
                        {
                            ActiveGame.playerName = LocalPlayerName;
                            ActiveGame.teamID = 2;
                        }
                        Console.Out.WriteLine(ActiveGame.teamID);
                        gamestate = 7;
                    }
                }
                else if (gamestate == 7)
                {
                    if (lines[i].Contains("[UnityCrossThreadLogger]STATE CHANGED "))
                    {
                        string relevantline = lines[i - 1];
                        relevantline = relevantline.Split(new string[] { "\"winningTeamId\": " }, StringSplitOptions.None)[1];
                        if (relevantline.Contains(ActiveGame.teamID.ToString()))
                        {
                            //game won
                        }
                        else{
                           //game lost
                        }
                    }
                }
            }
        }
    }
}
