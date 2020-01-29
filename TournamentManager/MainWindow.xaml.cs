using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TournamentManager
{
    /// <summary>
    ///     Register teams and randomise each round bracket until there is a winner.
    /// </summary>
    public partial class MainWindow : Window
    {
        ObservableCollection<Team> teams;
        ObservableCollection<Round> MatchUp;
        ObservableCollection<Team> eliminated = new ObservableCollection<Team>();
        ObservableCollection<Team> AllTeams = new ObservableCollection<Team>();

        public MainWindow()
        {
            InitializeComponent();
            teams = new ObservableCollection<Team>();
            MatchUp = new ObservableCollection<Round>();
            matchList.ItemsSource = MatchUp;
            elimList.ItemsSource = eliminated;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if(!String.IsNullOrWhiteSpace(nameInput.Text))
                teams.Add(new Team { Name = nameInput.Text, Score = 0, Eliminated = false });

            teamList.ItemsSource = teams;
            nameInput.Clear();
        }
        // Checks for clicks on listbox items
        string teamSelected;
        private void teamList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (teamList.SelectedItem != null)
            {
                teamSelected = (teamList.SelectedItem as Team).Name;
            }
        }
        // Delete Team from Tournament
        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            foreach (var t in teams.ToList())
                if (teamSelected != null)
                    if (t.Name.Contains(teamSelected))
                        teams.Remove(t);
        }

        // Update Team Name
        private void UpdateNameBtn_Click(object sender, RoutedEventArgs e)
        {
            foreach (var t in teams.ToList())
                if (teamSelected != null)
                    if (t.Name.Contains(teamSelected))
                        if (nameInput.Text != "")
                        {
                            teams.Remove(t);
                            teams.Add(new Team { Name = nameInput.Text, Score = 0, Eliminated = false });
                        }
                        else
                            MessageBox.Show("Please Insert New Team Name");
        }

        private void RandomizeBtn_Click(object sender, RoutedEventArgs e)
        {
            if (teams.Count() <= 1)
            {
                MessageBox.Show("You dont have enough teams!");
            }else
            {
                MatchUp.Clear();
                GenerateBye();
                CreateRandomMatchUps(); // Create Round Bracket



                // Add each team to All Team list
                foreach (var team in teams.ToList())
                    AllTeams.Add(team);

                // Remove bye from All Teams
                foreach (var t in AllTeams.ToList())
                    if (t.Name.Equals("BYE"))
                        AllTeams.Remove(t);

                teamList.ItemsSource = AllTeams; // Change teamList Item Source
                                                 // Disable Buttons
                Random_Btn.IsEnabled = false;
                Add_Btn.IsEnabled = false;
                UpdateBtn.IsEnabled = false;
                DeleteBtn.IsEnabled = false;
            }
        }

        void GenerateBye()
        {
            // if total number of teams is not even - generate a bye
            if (teams.Count % 2 != 0)
                teams.Add(new Team { Name = "BYE", Eliminated = true });
        }

        // Creates the Round Match Ups
        void CreateRandomMatchUps()
        {
            var random = new Random();
            var randomTeams = teams.OrderBy(t => random.Next()).ToList();
            for (int i = 1; i < teams.Count; i += 2)
            {
                MatchUp.Add(new Round
                {
                    Team1 = randomTeams[i - 1].Name,
                    Team2 = randomTeams[i].Name
                });
            }
        }

        // Saves the selected Team names as strings
        string team1, team2;
        private void matchList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (matchList.SelectedItem != null)
            {
                team1 = (matchList.SelectedItem as Round).Team1;
                team2 = (matchList.SelectedItem as Round).Team2;
            }
            // Make the row with a bye not clickable
            if (team1.Equals("BYE") || team2.Equals("BYE"))
            {
                matchList.SelectedIndex = -1;
                team1 = null;
                team2 = null;
            }
        }
        
        // Next Round
        private void nextRoundBtn_Click(object sender, RoutedEventArgs e)
        {
            MatchUp.Clear(); // Clear Previous Round MatchUp

            // Remove Eliminated Teams from the teams list
            foreach (var t in teams.ToList())
                if (t.Eliminated)
                    teams.Remove(t);
            // If there is only one team left, display the winner
            if (teams.Count == 1)
                MessageBox.Show("Winner : " + teams[0].Name);
            else if (teams.Count > 1)
            {
                GenerateBye();
                CreateRandomMatchUps();
            }
        } 
        // Restart Tournament
        private void RestartBtn_Click(object sender, RoutedEventArgs e)
        {
            // Clear all lists
            teams.Clear();
            MatchUp.Clear();
            AllTeams.Clear();
            eliminated.Clear();
            // Enable all buttons
            Add_Btn.IsEnabled = true;
            Random_Btn.IsEnabled = true;
            UpdateBtn.IsEnabled = true;
            DeleteBtn.IsEnabled = true;
        }

        // Update Score
        private void UpdateBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // If textfields are not a int display message
                if(!int.TryParse(team1Score.Text, out int score1) || !int.TryParse(team2Score.Text, out int score2)) {
                    MessageBox.Show("Please enter a number");
                }
                else
                {
                    // Remove the bye ( Helps with accurate team placement )
                    foreach (var f in teams.ToList())
                        if (f.Name.Equals("BYE"))
                            teams.Remove(f);
                    // Check for Tie
                    if (score1 == score2)
                        MessageBox.Show("Game cannot end in a tie");

                    foreach (var t in AllTeams)
                    {
                        if (t.Name.Equals(team1))
                        {
                            t.Score = score1;        // set team score
                            if (score1 < score2)     // check for winner
                                t.Eliminated = true; // set eliminated
                        }

                        if (t.Name.Equals(team2))
                        {
                            t.Score = score2;
                            if (score2 < score1)
                                t.Eliminated = true;
                        }
                        // Add Eliminated Team to Listbox
                        if (t.Eliminated)
                        {
                            // Makes sure eliminated doesn't the same value
                            if(!eliminated.Any(n=>n.Name == t.Name))
                            {
                                eliminated.Add(t);
                                t.Placement = teams.Count();
                            }
                        }
                    }
                    // Clear Text Boxes
                    team1Score.Clear();
                    team2Score.Clear();
                }
            }
            catch
            {
                MessageBox.Show("Please Select A Game!");
            }
        }
        
    }
}

    

