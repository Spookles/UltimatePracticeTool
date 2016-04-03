using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
using System.Windows.Shapes;

namespace ReactieSnelheid_Game
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public int circleSize;
        //Reads mouse sense from settings file
        public uint MouseSensPath = Properties.Settings.Default.MouseSens;
        public int resWidthPath = Properties.Settings.Default.ResolutionWidth;
        public int resHeightPath = Properties.Settings.Default.ResolutionHeight;
        public double bgSliderPath = Properties.Settings.Default.bgOpaSens;
        //Default slider position
        public uint sliderPos = 10;
        public double bgSliderPos = 0.25;

        public string keyOneInput;
        public string keyTwoInput;
        public Settings()
        {
            InitializeComponent();
        }

        //Method to read mouse sens from settings file
        public void updateSettings()
        {
            MouseSensPath = Properties.Settings.Default.MouseSens;
            resWidthPath = Properties.Settings.Default.ResolutionWidth;
            resHeightPath = Properties.Settings.Default.ResolutionHeight;
            bgSliderPath = Properties.Settings.Default.bgOpaSens;
    }
        //Makes sure only numbers can be filled in the circle size window
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
        //Makes me able to change PC settings (in this case sensitivity)
        public class WinAPI
        {
            [DllImport("user32.dll", CharSet = CharSet.Auto),]
            public static extern int SystemParametersInfo(uint uiAction, uint uiParam, uint pvParam, uint fWinIni);
        }
        //Updates mouse sense in the settings file
        private void sensitivitySlider_MouseLeave(object sender, MouseEventArgs e)
        {
            sliderPos = Convert.ToUInt32(sensitivitySlider.Value);
            Properties.Settings.Default.MouseSens = sliderPos;
            updateSettings();
        }
        //If you move the mouse on the slider the label next to it will give the current value
        private void sensitivitySlider_MouseMove(object sender, MouseEventArgs e)
        {
            currSense.Content = sensitivitySlider.Value;
        }


        private void sensitivitySlider_Loaded(object sender, RoutedEventArgs e)
        {
            sensitivitySlider.Value = Properties.Settings.Default.MouseSens;
            currSense.Content = sensitivitySlider.Value;
        }

        private void keyOne_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            keyOne.Text = e.Key.ToString();
            keyOneInput = keyOne.Text;
        }

        private void keyTwo_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            keyTwo.Text = e.Key.ToString();
            keyTwoInput = keyTwo.Text;
        }

        private void backgroundOpaSlider_MouseMove(object sender, MouseEventArgs e)
        {
            bgOpaSense.Content = Math.Round(backgroundOpaSlider.Value * 100) + "%";
        }

        private void backgroundOpaSlider_MouseLeave(object sender, MouseEventArgs e)
        {
            bgSliderPos = backgroundOpaSlider.Value;
            Properties.Settings.Default.bgOpaSens = bgSliderPos;
            updateSettings();
        }

        private void backgroundOpaSlider_Loaded(object sender, RoutedEventArgs e)
        {
            backgroundOpaSlider.Value = Properties.Settings.Default.bgOpaSens;
            bgOpaSense.Content = Math.Round(backgroundOpaSlider.Value * 100) + "%";
        }

        //This happens when save is pressed
        private void saveBtn_Click(object sender, RoutedEventArgs e)
        {
            bool CircleSizeDone = false;
            bool resDone = false;
            bool FullscreenYN = true;
            bool keysSet = false;
            //If the circle size input is empty it will be the default size 150
            if (string.IsNullOrWhiteSpace(circleSizeInput.Text))
            {
                circleSize = Properties.Settings.Default.HitcircleSize;
                CircleSizeDone = true;
            }
            else if (int.Parse(circleSizeInput.Text) > 600)
            {
                MessageBox.Show("Circle Size to big, 600 is the max");
            }
            else
            {
                circleSize = int.Parse(circleSizeInput.Text);
                CircleSizeDone = true;
            }

            if (string.IsNullOrWhiteSpace(resHeight.Text) && string.IsNullOrWhiteSpace(resWidth.Text))
            {
                resHeightPath = Properties.Settings.Default.ResolutionHeight;
                resWidthPath = Properties.Settings.Default.ResolutionWidth;
                resDone = true;
            }
            else if (string.IsNullOrWhiteSpace(resHeight.Text) || string.IsNullOrWhiteSpace(resWidth.Text))
            {
                MessageBox.Show("Please fill in a correct resolution");
            }
            else
            {
                resHeightPath = int.Parse(resHeight.Text);
                resWidthPath = int.Parse(resWidth.Text);
                resDone = true;
            }

            if (FullscreenBox.IsChecked == true)
            {
                FullscreenYN = true;
            }
            else
            {
                FullscreenYN = false;
            }

            if (string.IsNullOrWhiteSpace(keyOne.Text) || string.IsNullOrWhiteSpace(keyTwo.Text))
            {
                keyOneInput = Properties.Settings.Default.CustomKeyOne;
                keyTwoInput = Properties.Settings.Default.CustomKeyTwo;
                keysSet = true;
            }
            else
            {
                keysSet = true;
            }

            if (CircleSizeDone == true && resDone == true && keysSet == true)
            {
                this.Close();
            }

            //Saves your circle size
            Properties.Settings.Default.HitcircleSize = circleSize;
            Properties.Settings.Default.ResolutionHeight = resHeightPath;
            Properties.Settings.Default.ResolutionWidth = resWidthPath;
            Properties.Settings.Default.Fullscreen = FullscreenYN;
            Properties.Settings.Default.CustomKeyOne = keyOneInput;
            Properties.Settings.Default.CustomKeyTwo = keyTwoInput;

            //to change the mouse speed. For instance in a button click handler or Form_load
            //SPEED is an integer value between 0 and 20. 10 is the default.
            int res = WinAPI.SystemParametersInfo(113, 0, MouseSensPath, 0);
        }
    }
}
