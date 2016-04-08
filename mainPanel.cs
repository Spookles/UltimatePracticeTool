using System;
using System.Linq;
using System.Media;
using System.Windows;
using System.Windows.Resources;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Windows.Threading;
using System.Runtime.InteropServices;

namespace ReactieSnelheid_Game
{
    public partial class MainWindow : Window
    {
        //Score
        public int score = 0;
        //Dot counter
        public int count = 0;
        //Creating random class
        public Random random = new Random();
        //Creating an Ellipse called Dot
        public Ellipse Dot;
        //Adding a SoundPlayer for making the hitsounds
        public SoundPlayer hitSound = new SoundPlayer("Audio/hitsound.wav");
        //Check if game is started
        public bool GameStart = false;
        //Creates lists for the average time for milliseconds & seconds
        public List<Double> averageMSList = new List<double>();
        public Double averageMS;
        public List<Double> averageSList = new List<double>();
        public Double averageS;
        public DateTime timeAtCreate { get; set; }
        //A little work around to check if the window has changed size
        public DispatcherTimer _resizeTimer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, 100), IsEnabled = false };
        public DispatcherTimer DotTimer = new DispatcherTimer { IsEnabled = true };
        public double dotOpa = 1;
        public bool mouseEnabled = true;
        public TimeSpan currentTime;


        public ImageBrush DotImg = new ImageBrush();

        public double hitcircleSettingPath = Properties.Settings.Default.HitcircleSize;
        public int resWidthPath = Properties.Settings.Default.ResolutionWidth;
        public int resHeightPath = Properties.Settings.Default.ResolutionHeight;
        public bool Fullscreen = Properties.Settings.Default.Fullscreen;
        public string KeyOnePath = Properties.Settings.Default.CustomKeyOne;
        public string KeyTwoPath = Properties.Settings.Default.CustomKeyTwo;
        public double bgOpa = Properties.Settings.Default.bgOpaSens;
        public double timerTimeSpan = Properties.Settings.Default.timerTimespan;

        //Method launced at start of application
        public MainWindow()
        {
            InitializeComponent();

            //Creates an event handler for the timer to do something if the window changed size
            _resizeTimer.Tick += _resizeTimer_Tick;

            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);

            Grid.Width = resWidthPath;
            Grid.Height = resHeightPath;
            backgroundImageGrid.Background.Opacity = bgOpa;

            StreamResourceInfo cursorImg = Application.GetResourceStream(new Uri("images/cursor.cur", UriKind.RelativeOrAbsolute));
            Cursor = new Cursor(cursorImg.Stream);
        }

        //Makes me able to change PC settings (in this case sensitivity)
        public class WinAPI
        {
            [DllImport("user32.dll", CharSet = CharSet.Auto),]
            public static extern int SystemParametersInfo(uint uiAction, uint uiParam, uint pvParam, uint fWinIni);
        }
        //On exit of application changes sensitivity back to original of windows
        private void OnProcessExit(object sender, EventArgs e)
        {
            int res = WinAPI.SystemParametersInfo(113, 0, 10, 0);
        }

        //Calculates the minimal and maximal window size for the Dot to spawn in
        private double RandomBetween(double min, double max)
        {
            return random.NextDouble() * (max - min) + min;
        }
        //Checks circle size from settings file
        public void updateSettings()
        {
            timerTimeSpan = Properties.Settings.Default.timerTimespan;
            DotTimer.Interval = new TimeSpan(0, 0, 0, 0, Convert.ToInt32(timerTimeSpan));
            hitcircleSettingPath = Properties.Settings.Default.HitcircleSize;
            circleSize.Content = hitcircleSettingPath + "x" + hitcircleSettingPath;
            resWidthPath = Properties.Settings.Default.ResolutionWidth;
            resHeightPath = Properties.Settings.Default.ResolutionHeight;
            Fullscreen = Properties.Settings.Default.Fullscreen;
            KeyOnePath = Properties.Settings.Default.CustomKeyOne;
            KeyTwoPath = Properties.Settings.Default.CustomKeyTwo;
            bgOpa = Properties.Settings.Default.bgOpaSens;
            backgroundBrush.Opacity = bgOpa;
        }
        //Gives Dot a position
        public void placeDot()
        {
            //Give Dot random position
            // The farthest left the dot can be
            double minLeft = 0;
            // The farthest right the dot can be without it going off the screen
            double maxLeft = spawnArea.ActualWidth - Dot.Width;
            // The farthest up the dot can be
            double minTop = 0;
            // The farthest down the dot can be without it going off the screen
            double maxTop = spawnArea.ActualHeight - Dot.Height;


            double left = RandomBetween(minLeft, maxLeft);
            double top = RandomBetween(minTop, maxTop);
            Dot.Margin = new Thickness(left, top, 0, 0);
        }

        //createEllipse method, used for creating new Dots
        public void createEllipse()
        {
            spawnArea.Children.Remove(Dot);

            //Gets time from you PC right now
            timeAtCreate = DateTime.Now;

            //Create new Dot
            DotImg.ImageSource =
                new BitmapImage(new Uri(@"Images/hitcircle.png", UriKind.Relative));
            Dot = new Ellipse() { Width = hitcircleSettingPath, Height = hitcircleSettingPath, Fill = DotImg, };

            Dot.HorizontalAlignment = HorizontalAlignment.Left;
            Dot.VerticalAlignment = VerticalAlignment.Top;

            //Activates placeDot() method to give the Dot a random location
            placeDot();

            //Add Dot to the game area
            spawnArea.Children.Add(Dot);
        }
        
        public void missEllipse()
        {
            if (score > 0)
            {
                score = score - (score / 8);
            } else if (score <= 0)
            {
                score = 0;
            }
            scoreCount.Content = Convert.ToString(score);
            DotImg.ImageSource =
            new BitmapImage(new Uri(@"Images/hitcircle-bad.png", UriKind.Relative));
        }

        public void hitEllipse()
        {
            score = score + (200 * count) - (currentTime.Milliseconds / 10);
            scoreCount.Content = Convert.ToString(score);
            //Remove previous Dot
            spawnArea.Children.Remove(Dot);

            //Makes a hitsound play
            hitSound.Play();

            //Makes Dot counter go up by 1
            dotCount.Content = count.ToString();
            count++;

        }

        //Method to get average time
        public void averageTime()
        {
            //currentTime is now the time it took between creating the Dot and removing the Dot
            currentTime = DateTime.Now - timeAtCreate;

            //Adds the time it took in a list in Milliseconds
            averageMSList.Add(currentTime.TotalMilliseconds);
            //Gets the average of the list
            averageMS = averageMSList.Average();
            // "
            averageSList.Add(currentTime.TotalSeconds);
            averageS = averageSList.Average();

            //If averageMS is above 1000 milliseconds it will print out seconds. if its below 1000 milliseconds it will print it out in milliseconds
            if(averageMS > 1000)
            {
                reactionTime.Content = Math.Round(averageS, 2) + "s";
            } else
            {
                reactionTime.Content = Math.Round(averageMS) + "ms";
            }
        }
        //This happens when you press reset
        public void restart()
        {
            //Gives focus on gameWrapper so the key functions work
            GameStart = true;
            GameStartTrueOrFalse();
            //Cleans scores
            averageMS = 0;
            averageS = 0;
            averageMSList.Clear();
            averageSList.Clear();
            count = 0;
            dotCount.Content = count;
            score = 0;
            scoreCount.Content = score;
            //Removes current Dot
            spawnArea.Children.Remove(Dot);
            reactionTime.Content = "";
            //Checks circle size
            updateSettings();
            changeRes();

        }

        public void GameStartTrueOrFalse()
        {
            if (GameStart == false)
            {
                pauseScreen.Visibility = Visibility.Visible;
            }
            else
            {
                pauseScreen.IsEnabled = false;
                pauseScreen.Visibility = Visibility.Hidden;
                gameWrapper.Visibility = Visibility.Visible;
            }
        }

        public void changeRes()
        {
            Grid.Width = resWidthPath;
            Grid.Height = resHeightPath;
            if (Fullscreen == true)
            {
                Grid.WindowState = WindowState.Maximized;
                Grid.WindowStyle = WindowStyle.None;
            }
            else
            {
                Grid.WindowState = WindowState.Normal;
                Grid.WindowStyle = WindowStyle.SingleBorderWindow;
            }
        }

        //Start button
        private void Startbtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            updateSettings();
            changeRes();
            restart();
            //When the start button is pressed the game will be started
            GameStart = true;
            //Calls createEllipse method creating the first Dot
            GameStartTrueOrFalse();
            gameWrapper.Visibility = Visibility.Visible;
            createEllipse();
            gameWrapper.Focus();
        }

        //The timer for changing window size
        void _resizeTimer_Tick(object sender, EventArgs e)
        {
            _resizeTimer.IsEnabled = false;

            placeDot();
        }

        //Method for if the window size changed
        private void gameWrapper_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (GameStart == true)
            {
                _resizeTimer.IsEnabled = true;
                _resizeTimer.Stop();
                _resizeTimer.Start();
                gameWrapper.Visibility = Visibility.Visible;
                pauseScreen.Visibility = Visibility.Hidden;
            }
            else
            {
                gameWrapper.Visibility = Visibility.Hidden;
                _resizeTimer.IsEnabled = false;
            }
        }
        //Reset button
        private void resetBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            restart();
            createEllipse();
        }
        //Opens settings panel
        private void settingsBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //restart();
            Settings settingsWindow = new Settings();
            settingsWindow.Show();
        }
        //If your mouse is over gameWrapper, gameWrapper gets the focus so key functions work
        private void gameWrapper_MouseEnter(object sender, MouseEventArgs e)
        {
            gameWrapper.Focus();
        }

        public bool timerStart = false;
        private void startTimerBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (timerStart == false)
            {
                DotTimer.Start();
                DotTimer.Tick += DotTimer_Tick;
                timerStart = true;
                ImageBrush ib = new ImageBrush();
                BitmapImage bmi = new BitmapImage(new Uri(@"images/stoptimer.png", UriKind.Relative));
                ib.ImageSource = bmi;
                startTimerBtn.Fill = ib;
            }
            else
            {
                DotTimer.Stop();
                timerStart = false;
                ImageBrush ib = new ImageBrush();
                BitmapImage bmi = new BitmapImage(new Uri(@"images/starttimer.png", UriKind.Relative));
                ib.ImageSource = bmi;
                startTimerBtn.Fill = ib;
            }
        }

        private void DotTimer_Tick(object sender, EventArgs e)
        {
            createEllipse();
            gameWrapper.Focus();
        }

        private void quitBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Application.Current.Shutdown();
        }

        //Dot mouse click event handler
        private void LayoutRoot_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (mouseEnabled == true)
            {
                if (GameStart == true)
                {
                    if (Mouse.DirectlyOver == Dot)
                    {
                        if (timerStart == true)
                        {
                            averageTime();
                            hitEllipse();
                        }
                        else
                        {
                            averageTime();
                            hitEllipse();
                            createEllipse();
                        }
                    }
                    else if (Mouse.DirectlyOver == Startbtn || Mouse.DirectlyOver == resetBtn || Mouse.DirectlyOver == startTimerBtn)
                    {

                    }
                    else
                    {
                        missEllipse();
                    }
                }
            }
        }

        private void Grid_KeyDown(object sender, KeyEventArgs e)
        {
            Key KeyOne = (Key)Enum.Parse(typeof(Key), KeyOnePath);
            Key KeyTwo = (Key)Enum.Parse(typeof(Key), KeyTwoPath);
            if (e.Key == KeyOne || e.Key == KeyTwo)
            {
                if (Mouse.DirectlyOver == Dot)
                {
                    if (timerStart == true)
                    {
                        averageTime();
                        hitEllipse();
                    }
                    else
                    {
                        averageTime();
                        hitEllipse();
                        createEllipse();
                    }
                }
                else if (Mouse.DirectlyOver == gameWrapper)
                {

                }
                else
                {
                    missEllipse();
                }
            }

            //If the Escape key is pressed the application closes
            if (e.Key == Key.Escape)
            {
                if (pauseScreen.Visibility == Visibility.Hidden)
                {
                    GameStart = false;
                    pauseScreen.Visibility = Visibility.Visible;
                    gameWrapper.Visibility = Visibility.Hidden;
                    pauseScreen.IsEnabled = true;
                }
                else
                {
                    GameStart = true;
                    pauseScreen.Visibility = Visibility.Hidden;
                    gameWrapper.Visibility = Visibility.Visible;
                    pauseScreen.IsEnabled = false;
                }
            }

            ShortLabel.Tick += ShortLabel_Tick;
            if (e.Key == Key.M)
            {
                if (mouseEnabled == true)
                {
                    if (notTimer == false)
                    {
                        notTimer = true;
                        ShortLabel.Start();
                        NotificationsLbl.Content = "MOUSE BUTTONS TURNED OFF, PRESS ('M') TO TURN BACK ON";
                        NotificationsLbl.Visibility = Visibility.Visible;
                        mouseEnabled = false;
                    }
                }
                else
                {
                    if (notTimer == false)
                    {
                        notTimer = true;
                        ShortLabel.Start();
                        NotificationsLbl.Content = "MOUSE BUTTONS TURNED ON, PRESS ('M') TO TURN BACK OFF";
                        NotificationsLbl.Visibility = Visibility.Visible;
                        mouseEnabled = true;
                    }
                }
            }
        }
        public bool notTimer = false;
        public DispatcherTimer ShortLabel = new DispatcherTimer { Interval = new TimeSpan(0,0,3)};
        private void ShortLabel_Tick(object sender, EventArgs e)
        {
            notTimer = false;
            NotificationsLbl.Visibility = Visibility.Hidden;
            ShortLabel.Stop();
        }
    }
}
