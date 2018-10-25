using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Diagnostics;
using Windows.UI.Xaml.Shapes;
using Windows.UI;

namespace Otrio
{
    public sealed partial class MainPage : Page
    {
        public int BoxHeight = 100;
        public int RingThickness = 10;
        public Button SelectedRing;
        int ringSize;
        int largeRing = 80;
        int mediumRing = 48;
        int smallRing = 20;
        SolidColorBrush ringColor = new SolidColorBrush();
        int currentPlayer;
        int players;
        int[] ringSizeOptions = new int[3];
        List<Shape> remainingRings= new List<Shape>();
        List<Shape> addedRings = new List<Shape>();

        public MainPage()
        {
            this.InitializeComponent();
            NewGame();
        }

        public void NewGame()
        {
            // Initialize parameters.  
            Prompt.Text = "Select ring size.";
            players = 1;
            currentPlayer = 1;
            SelectedRing = Small; //*** Update prompt with error message if no size is selected.
            int[] ringSizeOptions = {largeRing, mediumRing, smallRing};
            //               Debug.WriteLine("ringSizeOptions= " + ringSizeOptions[0] + ", " + ringSizeOptions[1]);


            // Create all the rings of all sizes in the grid and start them invisible
            for (int column = 0; column < 3; column++)
            {
                for (int row = 0; row < 3; row++)
                {
                    for (int x = 0; x < ringSizeOptions.Length; x++)
                    {
                        NewRing(column, row, ringSizeOptions[x]);
                    }
                }
            }

            // New list of searchable rings.
            foreach (Shape ring in Board.Children)
            {
                if (ring is Ellipse)
                {
                    remainingRings.Add(ring);
                }
            }
        }
        
        public void NextPlayer()
        {
            if (currentPlayer > 1 && currentPlayer <= players)
            {
                Random r = new Random();
                int tempLocation = r.Next(remainingRings.Count());
                Shape tempRing = remainingRings[tempLocation];
                ShowRing(Grid.GetRow(tempRing), Grid.GetColumn(tempRing), (int)tempRing.Height);
            }
            else
            {
                currentPlayer = 1;
                Prompt.Text = "Select ring size.";
            }
        }

        public void ShowRing(int row, int column, int size)
        {
            var tempColor = new SolidColorBrush();
            ringColor = tempColor as SolidColorBrush;

            switch (currentPlayer)
            {
                case 1:
                    ringColor.Color = Colors.SteelBlue;
                    break;
                case 2:
                    ringColor.Color = Colors.Green;
                    break;
                case 3:
                    ringColor.Color = Colors.Red;
                    break;
                case 4:
                    ringColor.Color = Colors.Orange;
                    break;
                default:
                    //       ringColor = new SolidColorBrush(Windows.UI.Colors.AliceBlue);
                    //*** Need error catching.
                    Debug.WriteLine("Player selection error!");
                    break;
            }

            // Find ring in grid.
            for (int x = 0; x < remainingRings.Count(); x++)
            {
                // Check if ring is in current column, row, and size.
                if (Grid.GetRow(remainingRings[x]) == row && Grid.GetColumn(remainingRings[x]) == column && (int)remainingRings[x].Height == size)
                {
                    if (remainingRings[x].Visibility == Visibility.Collapsed)
                    {
                        remainingRings[x].Visibility = Visibility.Visible;
                        remainingRings[x].Stroke = ringColor;
                        addedRings.Add(remainingRings[x]);
                        remainingRings.Remove(remainingRings[x]);
                        CheckWinner();
                        break;
                    }
                }
                else
                {
                    if (x == remainingRings.Count()-1 && currentPlayer == 1)
                    {

                        Prompt.Text = "Please make a different selection.";
                    }
                    else if (x == remainingRings.Count()-1 && currentPlayer != 1)
                    {
                        Debug.WriteLine("Error: ring not found.");
                    }
                }

            }
        }

        public void NewRing(int row, int column, int size)
        {
            // Create new ring.
            Ellipse ring = new Ellipse();
            ring.Height = size;
            ring.Width = ring.Height;
            ring.StrokeThickness = RingThickness;
        //   ring.Stroke = CurrentRingColor();
            ring.Visibility = Visibility.Collapsed;
            ring.Fill = new SolidColorBrush(Windows.UI.Colors.Transparent);
            Grid.SetColumn(ring, column);
            Grid.SetRow(ring, row);
            Board.Children.Add(ring);
 //           Debug.WriteLine("Player playing: " + currentPlayer + "; at " + column + ", " + row + "; of size: " + ring.Height);
        }

        public void CheckWinner()
        {
            // Clear winning options.
            /* winnerOptions[0 - 8] = each nine grid locations,
             * winnerOptions[9 - 11] = row totals for rows 0 - 2, 
             * winnerOptions[12 - 14] = column totals for columns 0 - 2, 
             * winnerOptions[15] = diagonal total starting from the left,
             * winnerOptions[16] = diagonal total starting from the right 
             * 
             * To make sure the winner formula works the following needs to be true:
             * ringSizeOptions = r1, r2, r3
             * r1 + r2 + r3 = rSum
             * r1 * 3 = r1Sum
             * r2 * 3 = r2Sum
             * r3 * 3 = r3Sum
             * 
             * Each of the four sums cannot equal each other.  The formula for this is:
             * x + y + z = x * 3, 
             * x + y + z = y * 3, 
             * x + y + z = z * 3
             * This results in x = y = z, which is never true. */
            int[] winnerOptions = new int[17];
            Array.Clear(winnerOptions, 0, winnerOptions.Length);
            int winnerFound = 0;
            int[] ringSizeOptions = { largeRing, mediumRing, smallRing };

            
            // Get size total for all grid locations for this player.
            for (int x = 0; x < addedRings.Count; x++)
            {
                Shape ring = addedRings[x];
                Debug.WriteLine("addedRings count is= " + addedRings.Count());

                if (((SolidColorBrush)ring.Stroke).Color == ringColor.Color)
                {
                    //           Debug.WriteLine("Getting ring info for " + currentPlayer);
                    Debug.WriteLine("x= " + x);

                    int tempRow = Grid.GetRow(ring);
                    int tempColumn = Grid.GetColumn(ring);
                    int tempHeight = (int)ring.Height;
                    int gridLocation = (tempRow * 3) + tempColumn;
                    Debug.WriteLine("gridLocation " + gridLocation);

                    winnerOptions[gridLocation] += tempHeight;
                    winnerOptions[tempRow + 9] += tempHeight;
                    winnerOptions[tempColumn + 12] += tempHeight;


                }
            }

            // Check diagonals.
            winnerOptions[15] = winnerOptions[0] + winnerOptions[4] + winnerOptions[8];
            winnerOptions[16] = winnerOptions[2] + winnerOptions[4] + winnerOptions[6];

            Debug.WriteLine("winnerOptions[16]= " + winnerOptions[16]);

            //    Debug.WriteLine("winnerOptions[16]= " + winnerOptions[16]);

            foreach (int winnerCheck in winnerOptions)
            {
                Debug.WriteLine("winnerCheck= " + winnerCheck);

                if (winnerCheck == ringSizeOptions.Sum() ||
                    winnerCheck == ringSizeOptions[0] * 3 ||
                    winnerCheck == ringSizeOptions[1] * 3 ||
                    winnerCheck == ringSizeOptions[2] * 3)
                {

                    winnerFound = 1;
                    break;
                }

            }
            if (winnerFound == 1)
            {
                Prompt.Text = "You win!";
                Debug.WriteLine("WINNER!");
            }
            else
            {
                currentPlayer++;
       //         Debug.WriteLine("Next player is " + currentPlayer);
                NextPlayer();
            }
        }

        private void PlayerAmount_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            String _players =  PlayerAmount.SelectedItem.ToString();
            Debug.WriteLine("Amount of players is: " + _players);

            switch (_players)
            {
                case "Two":
                    players = 2;
                    break;
                case "Three":
                    players = 3;
                    break;
                case "Four":
                    players = 4;
                    break;
                default:
                    //      ringColor = new SolidColorBrush(Windows.UI.Colors.AliceBlue);
                    //*** Need error catching.
                    Debug.WriteLine("Player selection error!");
                    break;
            }   
            PlayerAmount.Visibility = Visibility.Collapsed;
            Prompt.Visibility = Visibility.Visible;
            currentPlayer = 1;
            Debug.WriteLine("Start game!");
        }

        private void One_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ShowRing(0, 0, ringSize);
        }

        private void Two_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ShowRing(0, 1, ringSize);
        }

        private void Three_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ShowRing(0, 2, ringSize);
        }

        private void Four_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ShowRing(1, 0, ringSize);
        }

        private void Five_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ShowRing(1, 1, ringSize);
        }

        private void Six_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ShowRing(1, 2, ringSize);
        }

        private void Seven_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ShowRing(2, 0, ringSize);
        }

        private void Eight_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ShowRing(2, 1, ringSize);
        }

        private void Nine_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ShowRing(2, 2, ringSize);
        }

        private void Small_Tapped(object sender, RoutedEventArgs e)
        {
            SwitchSelectedRing((Button)sender);
            ringSize = smallRing;
        }

        private void Medium_Tapped(object sender, RoutedEventArgs e)
        {
            SwitchSelectedRing((Button)sender);
            ringSize = mediumRing;
        }

        private void Large_Tapped(object sender, RoutedEventArgs e)
        {
            SwitchSelectedRing((Button)sender);
            ringSize = largeRing;
        }

        public void SwitchSelectedRing(Button newRingSelection)
        {
            SelectedRing.Background = new SolidColorBrush(Windows.UI.Colors.AliceBlue);
            SelectedRing = newRingSelection;
            newRingSelection.Background = new SolidColorBrush(Windows.UI.Colors.Linen);
            Prompt.Text = "Select box to play ring in.";
        }

    }
}
