using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ZombieHunt
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            // initialized
            InitializeComponent();

            // setup
            IsGameOver = false;
            IsSwitchSides = false; // when false Player1 == Blue and Player2 == Brown, when true Player1 == Brown and Player2 == Blue
            Images = new BitmapImage[] {
                null, // Inaccessible
                new BitmapImage(new Uri("/ZombieHunt;component/media/blank.png", UriKind.RelativeOrAbsolute)), // blank
                new BitmapImage(new Uri("/ZombieHunt;component/media/back.jpg", UriKind.RelativeOrAbsolute)), // FaceDown
                new BitmapImage(new Uri("/ZombieHunt;component/media/hero.JPG", UriKind.RelativeOrAbsolute)), // Blue_One
                new BitmapImage(new Uri("/ZombieHunt;component/media/vigilantly.JPG", UriKind.RelativeOrAbsolute)), // Blue_Two
                new BitmapImage(new Uri("/ZombieHunt;component/media/rats.JPG", UriKind.RelativeOrAbsolute)), // Brown_One
                new BitmapImage(new Uri("/ZombieHunt;component/media/zombie_top.JPG", UriKind.RelativeOrAbsolute)), // Brown_Two_Top
                new BitmapImage(new Uri("/ZombieHunt;component/media/zombie_bottom.JPG", UriKind.RelativeOrAbsolute)), // Brown_Two_Bottom
                new BitmapImage(new Uri("/ZombieHunt;component/media/zombie_right.JPG", UriKind.RelativeOrAbsolute)), // Brown_Two_Right
                new BitmapImage(new Uri("/ZombieHunt;component/media/zombie_left.JPG", UriKind.RelativeOrAbsolute)), // Brown_Two_Left
                new BitmapImage(new Uri("/ZombieHunt;component/media/woman.JPG", UriKind.RelativeOrAbsolute)), // Neutral_One
                new BitmapImage(new Uri("/ZombieHunt;component/media/nerd.JPG", UriKind.RelativeOrAbsolute)), // Neutral_Two
                new BitmapImage(new Uri("/ZombieHunt;component/media/corpse1.JPG", UriKind.RelativeOrAbsolute)), // Neutral_One
                new BitmapImage(new Uri("/ZombieHunt;component/media/corpse2.JPG", UriKind.RelativeOrAbsolute)) // Neutral_Two
            };
            ExitImage = new BitmapImage(new Uri("/ZombieHunt;component/media/exit.png", UriKind.RelativeOrAbsolute));

            StatusMsgs = new string[] {
                "", // None
                "Select a card that is face down to flip it, or select a card to move", // SelectFirst
                "Select an adjacent square to move onto (following the attack rules)", // SelectSecond 
                "All cards are flipped, there are only a few turns left, select a card to move", // EndPhaseSelectFirst
                "This round is over!", // GameOver
            };

            ErrorMsgs = new string[] {
                "", // None
                "This card is not selectable", // NotSelectable
                "This card is invalid, and cannot be found", // InvalidCard
                "This card does not match your color", // NonPlayerCard
                "Cards cannot move diagonally", // InvalidDiagonalMove
                "There must be a clear path to move", // NonClearPath
                "This card has exceeded its maximum move distance", // ExceedMaxMove
                "This is not a valid attack", // InvalidAttack
                "This card can only attack from one direction", // InvalidAttackDirection 
            };

            // initialize UI
            Player1_Total.Content = "0";
            Player2_Total.Content = "0";

            // start game
            Game = new ZombieHuntGame();
            Game.Start();

            // update UI
            Refresh(Game.CurrentState);

            // start the AI thread
            AITimer = new DispatcherTimer();
            AITimer.Interval = new TimeSpan(0, 0, 0, 0, 1000);
            AITimer.Tick += new EventHandler(AdvanceAI_Callback);
            AITimer.Start();
        }

        #region private
        private ZombieHuntGame Game;
        private SolidColorBrush cYellow = new SolidColorBrush(Color.FromArgb(255, 255, 255, 0));
        private SolidColorBrush cWhite = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
        private SolidColorBrush cBlack = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
        private BitmapImage[] Images;
        private BitmapImage ExitImage;
        private string[] StatusMsgs;
        private string[] ErrorMsgs;
        private bool IsGameOver;
        private bool IsSwitchSides;
        private PlayerColor CurrentColor;
        private int CurrentPlayer;

        private DispatcherTimer AITimer;

        // monikers
        private const string Card = "Card_";
        private const string Box = "Box_";

        // collect user input
        private void Card_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Image image = (Image)sender;
            var location = Location(image.Name);

            // exit early if the game is over
            if (IsGameOver) return;

            // if this is a computer player's turn, exit early
            if ((CurrentPlayer == 1 && Player1_Com.IsChecked.Value)
                || (CurrentPlayer == 2 && Player2_Com.IsChecked.Value)) return;

            CardSelection(location);
        }

        private void PlayAgain_Click(object sender, RoutedEventArgs e)
        {
            // goto next round
            NextRound();
        }

        private void AdvanceAI_Callback(object o, EventArgs e)
        {
            lock (this)
            {
                // exit early if the game is over
                if (IsGameOver)
                {
                    if (Player1_Com.IsChecked.Value && Player2_Com.IsChecked.Value)
                    {
                        NextRound();
                        return;
                    }
                }

                // if this is a human, exit early
                if ((CurrentPlayer == 1 && !Player1_Com.IsChecked.Value)
                    || (CurrentPlayer == 2 && !Player2_Com.IsChecked.Value)) return;

                // ask the computer to make the selection
                foreach (string location in Computer.Selection(CurrentColor, Game.CurrentState))
                {
                    CardSelection(location);
                }
            }
        }

        // game state
        private void CardSelection(string location)
        {
            // select this tile
            Game.SelectCard(location);

            // update UI
            Refresh(Game.CurrentState);
        }

        private void NextRound()
        {
            // hide the button again
            PlayAgain.Visibility = Visibility.Collapsed;

            // switch the players colors
            var tmp = Player1_Box.Background;
            Player1_Box.Background = Player2_Box.Background;
            Player2_Box.Background = tmp;

            // update the totals
            Player1_Total.Content = (Convert.ToInt32(Player1_Total.Content) + Convert.ToInt32(Player1_Score.Content)).ToString();
            Player2_Total.Content = (Convert.ToInt32(Player2_Total.Content) + Convert.ToInt32(Player2_Score.Content)).ToString();

            // reset the current scores
            Player1_Score.Content = "0";
            Player2_Score.Content = "0";

            // reset the counter
            TurnsRemaining.Content = "";

            // switch sides
            IsSwitchSides = !IsSwitchSides;

            // reset the game
            Game.ReStart();

            Refresh(Game.CurrentState);
        }

        // helper functions
        private string Location(string name)
        {
            var parts = name.Split('_');
            if (parts.Length != 2) throw new Exception("invalid parts");
            return parts[1];
        }

        private string Location(int numeric, int alpha)
        {
            return $"{Convert.ToChar((int)'A' + alpha)}{Convert.ToChar((int)'0' + numeric)}";
        }

        // UI
        private void Refresh(State state)
        {
            lock (this)
            {
                Rectangle box;
                // update the image map
                for (int numeric = 0; numeric < state.Board.Length; numeric++)
                {
                    for (int alpha = 0; alpha < state.Board[numeric].Length; alpha++)
                    {
                        // unhighlight the boxes
                        box = (Rectangle)this.FindName(Box + Location(numeric, alpha));
                        if (box != null) box.Stroke = cWhite;

                        // set the image
                        string label = Card + Location(numeric, alpha);
                        var image = (Image)this.FindName(Card + Location(numeric, alpha));
                        if (image != null)
                        {
                            // put the exit images
                            if (numeric == 0 || numeric == state.Board.Length - 1 ||
                                alpha == 0 || alpha == state.Board[numeric].Length - 1)
                            {
                                // exit image
                                image.Source = ExitImage;
                            }
                            else
                            {
                                // matching image
                                image.Source = Images[(int)state.Board[numeric][alpha]];
                            }
                        }
                    }
                }

                // highlight the selected
                box = (Rectangle)this.FindName(Box + Location(state.SelectedNumeric, state.SelectedAlpha));
                if (box != null) box.Stroke = cYellow;

                // update the scores
                if (IsSwitchSides)
                {
                    Player1_Score.Content = state.BrownScore.ToString();
                    Player2_Score.Content = state.BlueScore.ToString();
                }
                else
                {
                    Player1_Score.Content = state.BlueScore.ToString();
                    Player2_Score.Content = state.BrownScore.ToString();
                }

                // update the endPhase logic
                if (state.EndPhase)
                {
                    TurnsRemaining.Content = state.TurnsRemaining;
                }

                // store the game over bit
                IsGameOver = state.GameOver;

                // update status/debug out
                Status_Text.Text = StatusMsgs[(int)state.Status];
                Error_Text.Text = ErrorMsgs[(int)state.Error];

                // highlight who's turn it is
                CurrentColor = state.Player;
                if ((state.Player == PlayerColor.Blue && !IsSwitchSides)
                    || (state.Player == PlayerColor.Brown && IsSwitchSides))
                {
                    CurrentPlayer = 1;
                    Player1_Background.Stroke = cYellow;
                    Player2_Background.Stroke = cBlack;
                }
                else
                {
                    CurrentPlayer = 2;
                    Player1_Background.Stroke = cBlack;
                    Player2_Background.Stroke = cYellow;
                }

                // check if the game is over
                if (IsGameOver)
                {
                    // clear turns remaining
                    TurnsRemaining.Content = "";

                    // mark the button as available
                    PlayAgain.Visibility = Visibility.Visible;
                }
            }
        }
        #endregion
    }
}