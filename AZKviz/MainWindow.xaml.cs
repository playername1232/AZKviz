using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AZKviz
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool OnGoingQuestion = false;
        bool AnimationOnGoing = false;
        bool WinnerDecided = false;
        bool StreamingScene = false;

        string UsedHexagonName = String.Empty;

        System.Windows.Shapes.Path CurrentHexagon;

        bool Countdown = false, StreamingCountdown = false;
        int Maximum = 7;
        int _Timer = 0;
        int _StreamingTimer = 0;

        const int DefaultStrokeThickness = 3;
        const double CompletedStrokeThickness = 6;

        const string DefaultColor = "#FFDADADA";
        const string DefaultAlternativeColor = "#FFB5AE9A";
        const string DefaultColorStroke = "#FF000000";

        const string BlueTeamColor = "#FF0174EF";
        const string BlueTeamColorStroke = "#FF3BA0BA";

        const string OrangeTeamColor = "#FFFF9600";
        const string OrangeTeamColorStroke = "#FFFF7800";

        const string InvalidColor = "#FF000000";
        const string InvalidAlternativeColor = "#FF303030";
        const string InvalidColorStroke = "#FF818181";

        const string BlackColor = "#FF000000";
        const string WhiteColor = "#FFFFFFFF";

        public string _NickOrangePlayer = string.Empty, _NickBluePlayer = string.Empty;

        List<string> _ListBlueTeamUsed, _ListOrangeTeamUsed;

        //Hexagons are written in lower case
        List<string> LeftWall = new List<string>()
        {
            "hexagon_a_path",
            "hexagon_b_path",
            "hexagon_cc_path",
            "hexagon_f_path",
            "hexagon_i_path",
            "hexagon_n_path",
            "hexagon_s_path"
        };

        //Hexagons are written in lower case
        List<string> RightWall = new List<string>()
        {
            "hexagon_a_path",
            "hexagon_c_path",
            "hexagon_e_path",
            "hexagon_ch_path",
            "hexagon_m_path",
            "hexagon_s_path",
            "hexagon_zz_path"
        };

        //Hexagons are written in lower case
        List<string> BottomWall = new List<string>()
        {
            "hexagon_s_path",
            "hexagon_t_path",
            "hexagon_u_path",
            "hexagon_v_path",
            "hexagon_x_path",
            "hexagon_z_path",
            "hexagon_zz_path"
        };

        public MainWindow()
        {
            InitializeComponent();

            CurrentHexagon = new System.Windows.Shapes.Path();

            _Timer = Maximum;

            _ListBlueTeamUsed = new List<string>();
            _ListOrangeTeamUsed = new List<string>();

            _NickOrangePlayer = "Oranžový hráč";
            _NickBluePlayer = "Modrý hráč";

            NamePlayerOrange_Label.Content = _NickOrangePlayer;
            NamePlayerBlue_Label.Content = _NickBluePlayer;
            

            SelectedLetterOrange_Label.Content = string.Empty;
            SelectedLetterBlue_Label.Content = string.Empty;

            SelectedCanvas_Canvas.Visibility = Visibility.Hidden;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            InitializeSelected();
            InitializePlayingGrid();
            //InitializeStreaming();

            StreamingProgress_ProgressBar.Value = StreamingProgress_ProgressBar.Maximum;
            StreamingCountdown_Label.Content = $"Zbývá: {StreamingProgress_ProgressBar.Value} sekund";
            _StreamingTimer = (int)StreamingProgress_ProgressBar.Value;
            StreamingNamePlayerOrange_Label.Content = "Jméno hráče";
            StreamingScene = true;

            PlayerInfoCanvas_Canvas.Visibility = Visibility.Hidden;
            BitmapImage image = new BitmapImage(new Uri($@"{Environment.CurrentDirectory}\images\OrangePlayerCustom.png"));
            OrangePlayerImage_Image.Source = image;

            image = new BitmapImage(new Uri($@"{Environment.CurrentDirectory}\images\BluePlayerCustom.png"));
            BluePlayerImage_Image.Source = image;

            InitiatePlayerSelection(1);
        }

        void SaveException(Exception e)
        {
            if(!Directory.Exists($@"{Environment.CurrentDirectory}\Errors"))
                Directory.CreateDirectory($@"{Environment.CurrentDirectory}\Errors");

            StreamWriter sw = new StreamWriter($@"{Environment.CurrentDirectory}\Errors\ErrorLog.txt", append:true, encoding: System.Text.Encoding.UTF8);
            sw.Write($"{DateTime.Now.ToString("G")}\nChyba: {e.Message}\nStackTrace: {e.Message}");
            sw.Close();
            sw.Dispose();
        }

        void InitializePlayingGrid()
        {
            PlayingGround_Canvas.Visibility = Visibility.Visible;

            DoubleAnimation animation = new DoubleAnimation(1, TimeSpan.FromMilliseconds(1));
            PlayingGround_Canvas.BeginAnimation(Canvas.OpacityProperty, animation);

            animation = new DoubleAnimation(0, TimeSpan.FromMilliseconds(1));
            PlayingGround_Canvas.BeginAnimation(Canvas.OpacityProperty, animation);
            PlayingGround_Canvas.Visibility = Visibility.Hidden;
        }

        async void InitiatePlayerSelection(int count)
        {
            await Task.Delay(1000);
            NicknameSelection newWin = new NicknameSelection(this, count);
            newWin.Owner = this;
            newWin.Show();
            this.IsEnabled = false;
        }

        void ShowPlayerSelection(int count)
        {
            NicknameSelection newWin = new NicknameSelection(this, count);
            newWin.Owner = this;
            newWin.Show();
            this.IsEnabled = false;
        }

        private async void TimerTick()
        {
            if (_Timer == 0)
                return;

            bool maxUsed = false;

            while (Countdown)
            {
                if(_Timer != Maximum || maxUsed)
                {
                    _Timer -= 1;
                    Countdown_ProgressBar.Value = _Timer;
                    Countdown_Label.Content = (_Timer == 1) ? $"Zbývá: {_Timer} sekunda" : $"Zbývá: {_Timer} sekund";

                    try
                    {
                        MediaPlayer media = new MediaPlayer();
                        media.Open(new Uri($@"{Environment.CurrentDirectory}\wav\Timer.wav"));
                        media.Volume = 0.2;
                        media.Play();


                        if (_Timer == 0)
                        {
                            Countdown = false;

                            media.Open(new Uri($@"{Environment.CurrentDirectory}\wav\EndTimer.wav"));
                            media.Volume = 0.2;
                            media.Play();

                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        //SaveException(ex);
                    }

                    await Task.Delay(1000);
                }
                if(!maxUsed && _Timer == Maximum)
                {
                    maxUsed = true;
                    Countdown_ProgressBar.Value = _Timer;

                    Countdown_Label.Content = $"Zbývá: {_Timer} sekund";
                    await Task.Delay(1000);
                }
            }
        }

        private async void StreamingTimerTick()
        {
            if (_Timer == 0)
                return;
            try
            {
                bool maxUsed = false;

                MediaPlayer media = new MediaPlayer();
                media.Open(new Uri($@"{Environment.CurrentDirectory}\wav\budanebo.wav"));
                media.Volume = 0.1;
                media.Play();

                while (StreamingCountdown)
                {
                    if (_StreamingTimer != 60 || maxUsed)
                    {
                        _StreamingTimer -= 1;
                        StreamingProgress_ProgressBar.Value = _StreamingTimer;
                        StreamingCountdown_Label.Content = (_Timer == 1) ? $"Zbývá: {_StreamingTimer} sekunda" : $"Zbývá: {_StreamingTimer} sekund";

                        if (_StreamingTimer == 20)
                        {
                            StreamingProgress_ProgressBar.Foreground = (Brush)new BrushConverter().ConvertFromString("#FFfEA248");
                        }

                        if (_StreamingTimer == 10)
                        {
                            StreamingProgress_ProgressBar.Foreground = (Brush)new BrushConverter().ConvertFromString("#FFFD4629");
                        }


                        if (_StreamingTimer == 0)
                        {
                            StreamingCountdown = false;

                            media.Open(new Uri($@"{Environment.CurrentDirectory}\wav\EndTimer.wav"));
                            media.Volume = 0.2;
                            media.Play();

                            return;
                        }
                        await Task.Delay(1000);
                    }
                    if (!maxUsed && _StreamingTimer == 60)
                    {
                        maxUsed = true;
                        StreamingProgress_ProgressBar.Value = _StreamingTimer;

                        StreamingCountdown_Label.Content = $"Zbývá: {_StreamingTimer} sekund";
                        await Task.Delay(1000);
                    }
                }
                media.Stop();
            }
            catch (Exception ex)
            {
                SaveException(ex);
            }
        }

        string FromAlternativeColor(string alternative)
        {
            if (alternative == DefaultAlternativeColor)
                return DefaultColor;
            if (alternative == InvalidAlternativeColor)
                return InvalidColor;

            return alternative;
        }

        void ModifyHexByName(string hexName, string color, string strokeColor, double strokeThickness, string FontColor)
        {
            foreach(var item in PlayingGround_Canvas.Children)
            {
                if (item is System.Windows.Shapes.Path)
                {
                    if ((item as System.Windows.Shapes.Path).Name.ToLower() == hexName.ToLower())
                    {
                        (item as System.Windows.Shapes.Path).Fill = (Brush)new BrushConverter().ConvertFromString(color);
                        (item as System.Windows.Shapes.Path).Stroke = (Brush)new BrushConverter().ConvertFromString(strokeColor);
                        (item as System.Windows.Shapes.Path).StrokeThickness = Convert.ToDouble(strokeThickness);
                        ChangeFontColor(hexName.Split('_')[1], FontColor);
                    }
                }
                if (item is Canvas)
                {
                    foreach(var _innerItem in (item as Canvas).Children)
                    {
                        if (_innerItem is System.Windows.Shapes.Path)
                        {
                            if ((_innerItem as System.Windows.Shapes.Path).Name.ToLower() == hexName.ToLower())
                            {
                                (_innerItem as System.Windows.Shapes.Path).Fill = (Brush)new BrushConverter().ConvertFromString(color);
                                (_innerItem as System.Windows.Shapes.Path).Stroke = (Brush)new BrushConverter().ConvertFromString(strokeColor);
                                (_innerItem as System.Windows.Shapes.Path).StrokeThickness = Convert.ToDouble(strokeThickness);
                                ChangeFontColor((_innerItem as System.Windows.Shapes.Path).Name.Split('_')[1], FontColor);
                            }
                        }
                    }
                }
            }
        }

        void ChangeFontColor(string letter, string color)
        {
            //TODO CHANGE FONT COLOR

            foreach(var item in PlayingGround_Canvas.Children)
                if (item is Canvas)
                    foreach (var _innerItem in (item as Canvas).Children)
                        if (_innerItem is TextBlock)
                            if ((_innerItem as TextBlock).Name.Split('_')[0] == letter)
                                (_innerItem as TextBlock).Foreground = (Brush)new BrushConverter().ConvertFromString(color);
        }

        private void Hexagon_Path_Blank(object sender, MouseButtonEventArgs e)
        { /*YEEEEEEEEEEEEEEEEEEEEEEEEET*/ }

        private void Hexagon_Path_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (AnimationOnGoing)
                return;

            if (sender == null)
                return;

            /*if ((sender as Path).Fill.ToString() != DefaultColor && (sender as Path).Fill.ToString() != InvalidColor &&
                (sender as Path).Fill.ToString() != DefaultAlternativeColor && (sender as Path).Fill.ToString() != InvalidAlternativeColor)
                return;*/

            CurrentHexagon = (sender as System.Windows.Shapes.Path);

            if (e.ChangedButton == MouseButton.Left)
            {
                string letter = (sender as System.Windows.Shapes.Path).Name.Split('_')[1];
                letter = (letter == "CC") ? "Č" : (letter == "RR") ? "Ř" : (letter == "SS") ? "Š" : (letter == "ZZ") ? "Ž" : letter;
                SelectedLetterOrange_Label.Content = letter;
                SelectedLetter_Label.Content = letter;
                Countdown_ProgressBar.Foreground = (Brush)new BrushConverter().ConvertFromString(OrangeTeamColor);
            }

            else if (e.ChangedButton == MouseButton.Right)
            {
                string letter = (sender as System.Windows.Shapes.Path).Name.Split('_')[1];
                letter = (letter == "CC") ? "Č" : (letter == "RR") ? "Ř" : (letter == "SS") ? "Š" : (letter == "ZZ") ? "Ž" : letter;
                SelectedLetterBlue_Label.Content = letter;
                SelectedLetter_Label.Content = letter;
                Countdown_ProgressBar.Foreground = (Brush)new BrushConverter().ConvertFromString(BlueTeamColor);
            }

            UsedHexagonName = (sender as System.Windows.Shapes.Path).Name;

            ShowSelected(FromAlternativeColor((sender as System.Windows.Shapes.Path).Fill.ToString()));
            HideGrid();
        }

        private void Hexagon_Path_MouseEnter(object sender, MouseEventArgs e)
        {
            if((sender as System.Windows.Shapes.Path).Fill.ToString() == DefaultColor)
                (sender as System.Windows.Shapes.Path).Fill = (Brush)new BrushConverter().ConvertFromString(DefaultAlternativeColor);

            if((sender as System.Windows.Shapes.Path).Fill.ToString() == InvalidColor)
                (sender as System.Windows.Shapes.Path).Fill = (Brush)new BrushConverter().ConvertFromString(InvalidAlternativeColor);
        }

        private void Hexagon_Path_MouseLeave(object sender, MouseEventArgs e)
        {
            if ((sender as System.Windows.Shapes.Path).Fill.ToString() == DefaultAlternativeColor)
                (sender as System.Windows.Shapes.Path).Fill = (Brush)new BrushConverter().ConvertFromString(DefaultColor);

            if ((sender as System.Windows.Shapes.Path).Fill.ToString() == InvalidAlternativeColor)
                (sender as System.Windows.Shapes.Path).Fill = (Brush)new BrushConverter().ConvertFromString(InvalidColor);
        }

        private async void HideGrid()
        {
            DoubleAnimation animation = new DoubleAnimation(0, TimeSpan.FromSeconds(1));
            PlayingGround_Canvas.BeginAnimation(Canvas.OpacityProperty, animation);
            await Task.Delay(1000);
            PlayingGround_Canvas.Visibility = Visibility.Hidden;
            AnimationOnGoing = false;
        }

        private async void ShowGrid()
        {
            AnimationOnGoing = true;
            PlayingGround_Canvas.Visibility = Visibility.Visible;
            DoubleAnimation animation = new DoubleAnimation(1, TimeSpan.FromSeconds(1));
            PlayingGround_Canvas.BeginAnimation(Canvas.OpacityProperty, animation);
        }

        private void InitializeSelected()
        {
            Hexagon_Selected_Path.Fill = (Brush)new BrushConverter().ConvertFromString(DefaultColor);
            Countdown_ProgressBar.Value = Countdown_ProgressBar.Maximum;
            Countdown_Label.Content = $"Zbývá: {Countdown_ProgressBar.Value}";
            
            SelectedCanvas_Canvas.Visibility = Visibility.Visible;

            DoubleAnimation animation = new DoubleAnimation(1, TimeSpan.FromMilliseconds(1));
            SelectedCanvas_Canvas.BeginAnimation(Canvas.OpacityProperty, animation);

            OnGoingQuestion = true;

            animation = new DoubleAnimation(0, TimeSpan.FromMilliseconds(1));
            SelectedCanvas_Canvas.BeginAnimation(Canvas.OpacityProperty, animation);
            SelectedCanvas_Canvas.Visibility = Visibility.Hidden;

            OnGoingQuestion = false;
        }

        private void ShowSelected(string pathColor, bool changevisibility = true)
        {
            AnimationOnGoing = true;
            _Timer = Maximum;
            Hexagon_Selected_Path.Fill = (Brush)new BrushConverter().ConvertFromString(pathColor);

            if (Hexagon_Selected_Path.Fill.ToString() == InvalidColor)
                SelectedLetter_Label.Foreground = (Brush)new BrushConverter().ConvertFromString(WhiteColor);
            else
                SelectedLetter_Label.Foreground = (Brush)new BrushConverter().ConvertFromString(BlackColor);

            Countdown_ProgressBar.Value = Countdown_ProgressBar.Maximum;
            Countdown_Label.Content = $"Zbývá: {Countdown_ProgressBar.Value} sekund";

            if(changevisibility)
                SelectedCanvas_Canvas.Visibility = Visibility.Visible;

            DoubleAnimation animation = new DoubleAnimation(1, TimeSpan.FromSeconds(1));
            SelectedCanvas_Canvas.BeginAnimation(Canvas.OpacityProperty, animation);

            OnGoingQuestion = true;
        }

        private async void HideSelected()
        {
            DoubleAnimation animation = new DoubleAnimation(0, TimeSpan.FromSeconds(1));
            SelectedCanvas_Canvas.BeginAnimation(Canvas.OpacityProperty, animation);
            await Task.Delay(1000);
            SelectedCanvas_Canvas.Visibility = Visibility.Hidden;

            OnGoingQuestion = false;

            SelectedLetterBlue_Label.Content = string.Empty;
            SelectedLetterOrange_Label.Content = string.Empty;
            AnimationOnGoing = false;
        }


        void InitializeStreaming()
        {
            StreamingProgress_ProgressBar.Value = StreamingProgress_ProgressBar.Maximum;
            Countdown_Label.Content = $"Zbývá: {Countdown_ProgressBar.Value} sekund";

            StreamingCanvas_Canvas.Visibility = Visibility.Visible;

            DoubleAnimation animation = new DoubleAnimation(1, TimeSpan.FromMilliseconds(1));
            StreamingCanvas_Canvas.BeginAnimation(Canvas.OpacityProperty, animation);

            animation = new DoubleAnimation(0, TimeSpan.FromMilliseconds(1));
            StreamingCanvas_Canvas.BeginAnimation(Canvas.OpacityProperty, animation);
            StreamingCanvas_Canvas.Visibility = Visibility.Hidden;
        }

        void ShowStreaming()
        {
            AnimationOnGoing = true;
            StreamingScene = true;
            StreamingCanvas_Canvas.Visibility = Visibility.Visible;

            StreamingNamePlayerOrange_Label.Content = "Jméno hráče";

            _Timer = 60;

            StreamingCountdown_Label.Content = $"Zbývá: {_Timer} sekund";

            DoubleAnimation animation = new DoubleAnimation(1, TimeSpan.FromSeconds(1));
            StreamingCanvas_Canvas.BeginAnimation(Canvas.OpacityProperty, animation);

            NicknameSelection win = new NicknameSelection(this, 1);
            win.Owner = this;
            win.Show();
            this.IsEnabled = false;
        }

        private async void HideStreaming()
        {
            DoubleAnimation animation = new DoubleAnimation(0, TimeSpan.FromSeconds(1));
            StreamingCanvas_Canvas.BeginAnimation(Canvas.OpacityProperty, animation);
            await Task.Delay(1000);
            StreamingCanvas_Canvas.Visibility = Visibility.Hidden;
            StreamingScene = false;
            StreamingCountdown = false;
            AnimationOnGoing = false;
        }

        void ShowPlayerInfo()
        {
            StreamingScene = true;
            PlayerInfoCanvas_Canvas.Visibility = Visibility.Visible;
            DoubleAnimation animation = new DoubleAnimation(1, TimeSpan.FromSeconds(1));
            PlayerInfoCanvas_Canvas.BeginAnimation(Canvas.OpacityProperty, animation);
        }

        private async void HidePlayerInfo()
        {
            DoubleAnimation animation = new DoubleAnimation(0, TimeSpan.FromSeconds(1));
            PlayerInfoCanvas_Canvas.BeginAnimation(Canvas.OpacityProperty, animation);
            await Task.Delay(1000);
            PlayerInfoCanvas_Canvas.Visibility = Visibility.Hidden;
            AnimationOnGoing = false;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (AnimationOnGoing)
                return;

            if(StreamingScene)
            {
                if (e.Key == Key.Space)
                {
                    if (!StreamingCountdown)
                    {
                        StreamingCountdown = true;
                        StreamingTimerTick();
                    }
                }

                if(e.Key == Key.N)
                {
                    NicknameSelection nick = new NicknameSelection(this, 1);
                    nick.Owner = this;
                    nick.Show();
                    this.IsEnabled = false;

                    StreamingCountdown = false;
                    _StreamingTimer = 60;
                    StreamingProgress_ProgressBar.Value = _StreamingTimer;
                    StreamingCountdown_Label.Content = $"Zbývá: {_StreamingTimer} sekund";
                    StreamingProgress_ProgressBar.Foreground = (Brush)new BrushConverter().ConvertFromString("#FF06B025");
                }

                if(e.Key == Key.T)
                {
                    StreamingCountdown = false;
                    _StreamingTimer = 60;
                    StreamingProgress_ProgressBar.Value = _StreamingTimer;
                    StreamingCountdown_Label.Content = $"Zbývá: {_StreamingTimer} sekund";
                    StreamingProgress_ProgressBar.Foreground = (Brush)new BrushConverter().ConvertFromString("#FF06B025");
                }
            }

            if (OnGoingQuestion)
            {
                //Timer start TODO:
                if(e.Key == Key.Space)
                {
                    if (!Countdown)
                    {
                        Countdown = true;
                        TimerTick();
                    }
                }

                //Assign Orange
                if(e.Key == Key.Y)
                {
                    _ListOrangeTeamUsed.Add(CurrentHexagon.Name);

                    Countdown = false;

                    try
                    {

                        MediaPlayer media = new MediaPlayer();
                        media.Open(new Uri($@"{Environment.CurrentDirectory}\wav\Right.wav"));
                        media.Volume = 0.2;
                        media.Play();
                    }

                    catch (Exception ex)
                    {
                        SaveException(ex);
                    }

                    Hexagon_Selected_Path.Fill = (Brush)new BrushConverter().ConvertFromString(OrangeTeamColor);
                    SelectedLetter_Label.Foreground = (Brush)new BrushConverter().ConvertFromString(BlackColor);
                    ModifyHexByName(UsedHexagonName, OrangeTeamColor, OrangeTeamColorStroke, CompletedStrokeThickness, "#FF000000");
                    HideSelected();
                    ShowGrid();
                }

                //Assign Blue
                if (e.Key == Key.X)
                {
                    _ListBlueTeamUsed.Add(CurrentHexagon.Name);
                    
                    Countdown = false;

                    try
                    {

                        MediaPlayer media = new MediaPlayer();
                        media.Open(new Uri($@"{Environment.CurrentDirectory}\wav\Right.wav"));
                        media.Volume = 0.2;
                        media.Play();
                    }

                    catch (Exception ex)
                    {
                        SaveException(ex);
                    }

                    Hexagon_Selected_Path.Fill = (Brush)new BrushConverter().ConvertFromString(BlueTeamColor);
                    SelectedLetter_Label.Foreground = (Brush)new BrushConverter().ConvertFromString(BlackColor);
                    ModifyHexByName(UsedHexagonName, BlueTeamColor, BlueTeamColorStroke, CompletedStrokeThickness, "#FF000000");
                    HideSelected();
                    ShowGrid();
                }

                //Assign Invalid
                if (e.Key == Key.C)
                {
                    Countdown = false;

                    try
                    {
                        /*SoundPlayer player = new SoundPlayer($@"{Environment.CurrentDirectory}\wav\Invalid.wav");
                        player.Load();
                        player.
                        player.Play();*/

                        MediaPlayer media = new MediaPlayer();
                        media.Open(new Uri($@"{Environment.CurrentDirectory}\wav\Invalid.wav"));
                        media.Volume = 1;
                        media.Play();
                    }

                    catch (Exception ex)
                    {
                        SaveException(ex);
                    }

                    Hexagon_Selected_Path.Fill = (Brush)new BrushConverter().ConvertFromString(InvalidColor);
                    SelectedLetter_Label.Foreground = (Brush)new BrushConverter().ConvertFromString(WhiteColor);
                    ModifyHexByName(UsedHexagonName, InvalidColor, InvalidColorStroke, CompletedStrokeThickness, "#FFFFFFFF");
                    HideSelected();
                    ShowGrid();
                }
            }

            if(!OnGoingQuestion)
            {
                //Assign Orange win
                if (e.Key == Key.Left && !WinnerDecided)
                    OrangeWinner();

                //Assign Blue win
                if (e.Key == Key.Right && !WinnerDecided)
                    BlueWinner();

                //Assign New Game
                if (e.Key == Key.N && !StreamingScene)
                {
                    MessageBoxResult res = MessageBox.Show("Začít novou hru?", "NOVÁ HRA?", MessageBoxButton.YesNo);

                    if(res == MessageBoxResult.Yes)
                    {
                        NewGame();
                    }
                }

                if(e.Key == Key.Up && !StreamingScene)
                {
                    ShowStreaming();
                    HidePlayerInfo();
                    HideGrid();
                    HideSelected();
                    return;
                }
                if(e.Key == Key.Down && StreamingScene)
                {
                    ShowGrid();
                    ShowPlayerInfo();
                    HideStreaming();

                    NicknameSelection nick = new NicknameSelection(this, 2);
                    nick.Owner = this;
                    nick.Show();
                    this.IsEnabled = false;

                    return;
                }
            }
        }

        private void OrangeWinner()
        {
            foreach(var item in PlayingGround_Canvas.Children)
            {
                if(item is Canvas)
                {
                    foreach(var _innerItem in (item as Canvas).Children)
                    {
                        if (_innerItem is TextBlock)
                                (_innerItem as TextBlock).Visibility = Visibility.Hidden;

                        if(_innerItem is System.Windows.Shapes.Path)
                        {
                            if((_innerItem as System.Windows.Shapes.Path).Fill.ToString() != OrangeTeamColor)
                            {
                                (_innerItem as System.Windows.Shapes.Path).Fill = (Brush)new BrushConverter().ConvertFromString("#FFC2C2C2");
                                (_innerItem as System.Windows.Shapes.Path).Stroke = (Brush)new BrushConverter().ConvertFromString("#FFC2C2C2");
                            }
                            (_innerItem as System.Windows.Shapes.Path).MouseDown -= Hexagon_Path_MouseDown;
                            (_innerItem as System.Windows.Shapes.Path).MouseDown += Hexagon_Path_Blank;
                        }
                    }
                }
            }
            WinnerDecided = true;

            try
            {
                MediaPlayer player = new MediaPlayer();
                player.Open(new Uri($@"{Environment.CurrentDirectory}\wav\Victory.wav"));
                player.Volume = 0.1;
                player.Play();
            }
            catch (Exception ex)
            {
                SaveException(ex);
            }

            MessageBox.Show($"{_NickOrangePlayer} vyhrál!", "GG!", MessageBoxButton.OK);
        }

        private void BlueWinner()
        {
            foreach (var item in PlayingGround_Canvas.Children)
            {
                if (item is Canvas)
                {
                    foreach (var _innerItem in (item as Canvas).Children)
                    {
                        if (_innerItem is TextBlock)
                            (_innerItem as TextBlock).Visibility = Visibility.Hidden;

                        if (_innerItem is System.Windows.Shapes.Path)
                        {
                            if ((_innerItem as System.Windows.Shapes.Path).Fill.ToString() != BlueTeamColor)
                            {
                                (_innerItem as System.Windows.Shapes.Path).Fill = (Brush)new BrushConverter().ConvertFromString("#FFC2C2C2");
                                (_innerItem as System.Windows.Shapes.Path).Stroke = (Brush)new BrushConverter().ConvertFromString("#FFC2C2C2");
                            }
                            (_innerItem as System.Windows.Shapes.Path).MouseDown -= Hexagon_Path_MouseDown;
                            (_innerItem as System.Windows.Shapes.Path).MouseDown += Hexagon_Path_Blank;
                        }
                    }
                }
            }
            WinnerDecided = true;

            try
            {
                MediaPlayer player = new MediaPlayer();
                player.Open(new Uri($@"{Environment.CurrentDirectory}\wav\Victory.wav"));
                player.Volume = 0.1;
                player.Play();
            }
            catch (Exception ex)
            {
                SaveException(ex);
            }

            MessageBox.Show($"{_NickBluePlayer} vyhrál!", "GG!", MessageBoxButton.OK);
        }

        private void NewGame()
        {
            NamePlayerOrange_Label.Content = "Oranžový hráč";
            NamePlayerBlue_Label.Content = "Modrý hráč";
            ShowPlayerSelection(2);

            WinnerDecided = false;

            foreach (var item in PlayingGround_Canvas.Children)
            {
                if (item is Canvas)
                {
                    foreach (var _innerItem in (item as Canvas).Children)
                    {
                        if(_innerItem is TextBlock)
                        {
                            (_innerItem as TextBlock).Foreground = (Brush)new BrushConverter().ConvertFromString(BlackColor);
                            (_innerItem as TextBlock).Visibility = Visibility.Visible;
                        }

                        if (_innerItem is System.Windows.Shapes.Path)
                        {
                            (_innerItem as System.Windows.Shapes.Path).Fill = (Brush)new BrushConverter().ConvertFromString(DefaultColor);
                            (_innerItem as System.Windows.Shapes.Path).Stroke = (Brush)new BrushConverter().ConvertFromString(DefaultColorStroke);
                            (_innerItem as System.Windows.Shapes.Path).StrokeThickness = DefaultStrokeThickness;
                            (_innerItem as System.Windows.Shapes.Path).Visibility = Visibility.Visible;
                            (_innerItem as System.Windows.Shapes.Path).MouseDown -= Hexagon_Path_Blank;
                            (_innerItem as System.Windows.Shapes.Path).MouseDown -= Hexagon_Path_MouseDown;
                            (_innerItem as System.Windows.Shapes.Path).MouseDown += Hexagon_Path_MouseDown;
                        }
                    }
                }
            }
            _ListBlueTeamUsed = new List<string>();
            _ListOrangeTeamUsed = new List<string>();
            ShowGrid();
            HideSelected();
            HideStreaming();
        }

        /*
        private void Winner()
        {
            if(_ListBlueTeamUsed.Count != 0)
            {
                bool left = false, right = false, bottom = false;
                foreach(string item in _ListBlueTeamUsed)
                {
                    if (!left)
                        left = LeftWall.Any(x => x.ToLower() == item.ToLower());
                    if (!right)
                        right = RightWall.Any(x => x.ToLower() == item.ToLower());
                    if (!bottom)
                        bottom = BottomWall.Any(x => x.ToLower() == item.ToLower());

                    if(left && right && bottom)
                    {
                        if(AllConnected("blue"))
                            MessageBox.Show($"{_NickBluePlayer} vyhrál!");

                        return;
                    }
                }
            }

            if(_ListOrangeTeamUsed.Count != 0)
            {
                bool left = false, right = false, bottom = false;
                foreach (string item in _ListOrangeTeamUsed)
                {
                    if (!left)
                        left = LeftWall.Any(x => x.ToLower() == item.ToLower());
                    if (!right)
                        right = RightWall.Any(x => x.ToLower() == item.ToLower());
                    if (!bottom)
                        bottom = BottomWall.Any(x => x.ToLower() == item.ToLower());

                    if (left && right && bottom)
                    {
                        if(AllConnected("orange"))
                            MessageBox.Show($"{_NickOrangePlayer} vyhrál!");

                        return;
                    }
                }
            }
        }

        private bool AllConnected(string team)
        {
            if(team == "orange")
            {
                bool connected = true;

                List<string> ordered = _ListOrangeTeamUsed.OrderBy(x => x.Split('_')[1]).ToList();

                foreach(string item in _ListOrangeTeamUsed)
                {
                    connected = AlternativeCheckNeighboursConnection(team);
                    if (!connected)
                        return false;
                }
                return true;
            }

            else if(team == "blue")
            {
                bool connected = true;

                List<string> used = new List<string>();

                foreach(string item in _ListBlueTeamUsed)
                {
                    connected = AlternativeCheckNeighboursConnection(team);
                    if (!connected)
                        return false;
                    used.Add(item);
                }

                return true;
            }

            return false;
        }

        private bool ColorChecker(Brush color1, Brush color2)
        {
            if (color1.ToString() == color2.ToString())
                return true;
            return false;
        }

        private bool AlternativeCheckNeighboursConnection(string team)
        {
            string[] firstRow = { Hexagon_A_Path.Name.ToLower() };
            string[] scndRow = { Hexagon_B_Path.Name.ToLower(), Hexagon_C_Path.Name.ToLower() };
            string[] thrdRow = { Hexagon_CC_Path.Name.ToLower(), Hexagon_D_Path.Name.ToLower(), Hexagon_E_Path.Name.ToLower() };
            string[] frthRow = { Hexagon_F_Path.Name.ToLower(), Hexagon_G_Path.Name.ToLower(), Hexagon_H_Path.Name.ToLower(), 
                Hexagon_CH_Path.Name.ToLower() };

            string[] fifthRow = { Hexagon_I_Path.Name.ToLower(), Hexagon_J_Path.Name.ToLower(), Hexagon_K_Path.Name.ToLower(), 
                Hexagon_L_Path.Name.ToLower(), Hexagon_M_Path.Name.ToLower() };

            string[] sixthRow = { Hexagon_N_Path.Name.ToLower(), Hexagon_O_Path.Name.ToLower(), Hexagon_P_Path.Name.ToLower(),
                Hexagon_R_Path.Name.ToLower(), Hexagon_RR_Path.Name.ToLower(), Hexagon_S_Path.Name.ToLower() };

            string[] seventhRow = { Hexagon_SS_Path.Name.ToLower(), Hexagon_T_Path.Name.ToLower(), Hexagon_U_Path.Name.ToLower(),
                Hexagon_V_Path.Name.ToLower(), Hexagon_X_Path.Name.ToLower(), Hexagon_X_Path.Name.ToLower(), Hexagon_Z_Path.Name.ToLower(), Hexagon_ZZ_Path.Name.ToLower()};

            bool containsFirst = false, containsSecond = false, containsThird = false, containsFourth = false, containsFifth = false, containsSixth = false, containsSeventh = false;

            if (team == "blue")
            {
                foreach(string item in _ListBlueTeamUsed)
                {
                    if (firstRow.Any(x => x == item))
                        containsFirst = true;

                    else if (scndRow.Any(x => x == item))
                        containsSecond = true;

                    else if (thrdRow.Any(x => x == item))
                        containsThird = true;

                    else if (frthRow.Any(x => x == item))
                        containsFourth = true;

                    else if (fifthRow.Any(x => x == item))
                        containsFifth = true;

                    else if (sixthRow.Any(x => x == item))
                        containsSixth = true;

                    else if (seventhRow.Any(x => x == item))
                        containsSeventh = true;
                }
                if (containsFirst && containsSecond && containsThird && containsFourth && containsFifth && containsSixth && containsSeventh)
                {

                }
            }

            if(team == "orange")
            {
                foreach (string item in _ListOrangeTeamUsed)
                {
                    if (firstRow.Any(x => x == item))
                        containsFirst = true;

                    if (scndRow.Any(x => x == item))
                        containsSecond = true;

                    if (thrdRow.Any(x => x == item))
                        containsThird = true;

                    if (frthRow.Any(x => x == item))
                        containsFourth = true;

                    if (fifthRow.Any(x => x == item))
                        containsFifth = true;

                    if (sixthRow.Any(x => x == item))
                        containsSixth = true;

                    if (seventhRow.Any(x => x == item))
                        containsSeventh = true;
                }
                return (containsFirst && containsSecond && containsThird && containsFourth && containsFifth && containsSixth && containsSeventh);
            }

            return false;
        }
        private bool CheckNeighboursConnection(string team, string hexname, List<string> used)
        {
            switch(hexname.ToLower())
            {
                case "hexagon_a_path":
                    {
                        int count = 0;
                        count += (ColorChecker(Hexagon_A_Path.Fill, Hexagon_B_Path.Fill)) ? 1 : 0;
                        count += (ColorChecker(Hexagon_A_Path.Fill, Hexagon_CC_Path.Fill)) ? 1 : 0;

                        if (count != 0)
                            return true;
                        return false;
                    }
                case "hexagon_b_path":
                    {
                        int count = 0;
                        count += ColorChecker(Hexagon_B_Path.Fill, Hexagon_A_Path.Fill) ? 1 : 0;
                        count += ColorChecker(Hexagon_B_Path.Fill, Hexagon_C_Path.Fill) ? 1 : 0;
                        count += ColorChecker(Hexagon_B_Path.Fill, Hexagon_D_Path.Fill) ? 1 : 0;
                        count += ColorChecker(Hexagon_B_Path.Fill, Hexagon_CC_Path.Fill) ? 1 : 0;

                        if (count >= 2)
                            return false;
                        return true;
                    }
                case "hexagon_c_path":
                    {
                        int count = 0;
                        count += ColorChecker(Hexagon_C_Path.Fill, Hexagon_A_Path.Fill) ? 1 : 0;
                        count += ColorChecker(Hexagon_C_Path.Fill, Hexagon_B_Path.Fill) ? 1 : 0;
                        count += ColorChecker(Hexagon_C_Path.Fill, Hexagon_D_Path.Fill) ? 1 : 0;
                        count += ColorChecker(Hexagon_C_Path.Fill, Hexagon_E_Path.Fill) ? 1 : 0;

                        if (count >= 2)
                            return false;
                        return true;
                    }
                case "hexagon_cc_path":
                    {
                        int count = 0;
                        count += ColorChecker(Hexagon_CC_Path.Fill, Hexagon_B_Path.Fill) ? 1 : 0;
                        count += ColorChecker(Hexagon_CC_Path.Fill, Hexagon_D_Path.Fill) ? 1 : 0;
                        count += ColorChecker(Hexagon_CC_Path.Fill, Hexagon_G_Path.Fill) ? 1 : 0;
                        count += ColorChecker(Hexagon_CC_Path.Fill, Hexagon_F_Path.Fill) ? 1 : 0;

                        if (count >= 2)
                            return false;
                        return true;
                    }
                case "hexagon_d_path":
                    {
                        int count = 0;
                        count += ColorChecker(Hexagon_D_Path.Fill, Hexagon_B_Path.Fill) ? 1 : 0;
                        count += ColorChecker(Hexagon_D_Path.Fill, Hexagon_C_Path.Fill) ? 1 : 0;
                        count += ColorChecker(Hexagon_D_Path.Fill, Hexagon_E_Path.Fill) ? 1 : 0;
                        count += ColorChecker(Hexagon_D_Path.Fill, Hexagon_H_Path.Fill) ? 1 : 0;
                        count += ColorChecker(Hexagon_D_Path.Fill, Hexagon_G_Path.Fill) ? 1 : 0;
                        count += ColorChecker(Hexagon_D_Path.Fill, Hexagon_CC_Path.Fill) ? 1 : 0;

                        if (count >= 2)
                            return false;
                        return true;

                    }
                case "hexagon_e_path":
                    {
                        int count = 0;
                        count += ColorChecker(Hexagon_E_Path.Fill, Hexagon_C_Path.Fill) ? 1 : 0;
                        count += ColorChecker(Hexagon_E_Path.Fill, Hexagon_D_Path.Fill) ? 1 : 0;
                        count += ColorChecker(Hexagon_E_Path.Fill, Hexagon_H_Path.Fill) ? 1 : 0;
                        count += ColorChecker(Hexagon_E_Path.Fill, Hexagon_CH_Path.Fill) ? 1 : 0;

                        if (count >= 2)
                            return false;
                        return true;
                    }
                case "hexagon_f_path":
                    {
                        if (team == "blue")
                        {

                        }
                        else if (team == "orange")
                        {

                        }
                        return false;
                    }
                case "hexagon_g_path":
                    {
                        if (team == "blue")
                        {

                        }
                        else if (team == "orange")
                        {

                        }
                        return false;
                    }
                case "hexagon_h_path":
                    {
                        if (team == "blue")
                        {

                        }
                        else if (team == "orange")
                        {

                        }
                        return false;

                    }
                case "hexagon_ch_path":
                    {
                        if (team == "blue")
                        {

                        }
                        else if (team == "orange")
                        {

                        }
                        return false;
                    }
                case "hexagon_i_path":
                    {
                        return ColorChecker(Hexagon_I_Path.Fill, Hexagon_F_Path.Fill) || ColorChecker(Hexagon_I_Path.Fill, Hexagon_J_Path.Fill) ||
                            ColorChecker(Hexagon_I_Path.Fill, Hexagon_O_Path.Fill) || ColorChecker(Hexagon_I_Path.Fill, Hexagon_N_Path.Fill);
                    }
                case "hexagon_j_path":
                    {
                        return ColorChecker(Hexagon_J_Path.Fill, Hexagon_F_Path.Fill) || ColorChecker(Hexagon_J_Path.Fill, Hexagon_G_Path.Fill) ||
                            ColorChecker(Hexagon_J_Path.Fill, Hexagon_K_Path.Fill) || ColorChecker(Hexagon_J_Path.Fill, Hexagon_P_Path.Fill) ||
                            ColorChecker(Hexagon_J_Path.Fill, Hexagon_O_Path.Fill) || ColorChecker(Hexagon_J_Path.Fill, Hexagon_I_Path.Fill);
                    }
                case "hexagon_k_path":
                    {
                        return ColorChecker(Hexagon_K_Path.Fill, Hexagon_G_Path.Fill) || ColorChecker(Hexagon_K_Path.Fill, Hexagon_H_Path.Fill) ||
                           ColorChecker(Hexagon_K_Path.Fill, Hexagon_L_Path.Fill) || ColorChecker(Hexagon_K_Path.Fill, Hexagon_R_Path.Fill) ||
                            ColorChecker(Hexagon_K_Path.Fill, Hexagon_P_Path.Fill) || ColorChecker(Hexagon_K_Path.Fill, Hexagon_J_Path.Fill);
                    }
                case "hexagon_l_path":
                    {
                        return ColorChecker(Hexagon_L_Path.Fill, Hexagon_H_Path.Fill) || ColorChecker(Hexagon_L_Path.Fill, Hexagon_CH_Path.Fill) ||
                            ColorChecker(Hexagon_L_Path.Fill, Hexagon_M_Path.Fill) || ColorChecker(Hexagon_L_Path.Fill, Hexagon_RR_Path.Fill) ||
                            ColorChecker(Hexagon_L_Path.Fill, Hexagon_R_Path.Fill) || ColorChecker(Hexagon_L_Path.Fill, Hexagon_K_Path.Fill);
                    }
                case "hexagon_m_path":
                    {
                        return ColorChecker(Hexagon_M_Path.Fill, Hexagon_CH_Path.Fill) || ColorChecker(Hexagon_M_Path.Fill, Hexagon_L_Path.Fill) ||
                            ColorChecker(Hexagon_M_Path.Fill, Hexagon_RR_Path.Fill) || ColorChecker(Hexagon_M_Path.Fill, Hexagon_S_Path.Fill);
                    }
                case "hexagon_n_path":
                    {
                        return ColorChecker(Hexagon_N_Path.Fill, Hexagon_I_Path.Fill) || ColorChecker(Hexagon_N_Path.Fill, Hexagon_O_Path.Fill) ||
                            ColorChecker(Hexagon_N_Path.Fill, Hexagon_T_Path.Fill) || ColorChecker(Hexagon_N_Path.Fill, Hexagon_SS_Path.Fill);
                    }
                case "hexagon_o_path":
                    {
                        return ColorChecker(Hexagon_O_Path.Fill, Hexagon_I_Path.Fill) || ColorChecker(Hexagon_O_Path.Fill, Hexagon_J_Path.Fill) ||
                            ColorChecker(Hexagon_O_Path.Fill, Hexagon_P_Path.Fill) || ColorChecker(Hexagon_O_Path.Fill, Hexagon_U_Path.Fill) ||
                            ColorChecker(Hexagon_O_Path.Fill, Hexagon_T_Path.Fill) || ColorChecker(Hexagon_O_Path.Fill, Hexagon_N_Path.Fill);
                    }
                case "hexagon_p_path":
                    {
                        return ColorChecker(Hexagon_P_Path.Fill, Hexagon_J_Path.Fill) || ColorChecker(Hexagon_P_Path.Fill, Hexagon_K_Path.Fill) ||
                            ColorChecker(Hexagon_P_Path.Fill, Hexagon_R_Path.Fill) || ColorChecker(Hexagon_P_Path.Fill, Hexagon_V_Path.Fill) ||
                            ColorChecker(Hexagon_P_Path.Fill, Hexagon_U_Path.Fill) || ColorChecker(Hexagon_P_Path.Fill, Hexagon_O_Path.Fill);
                    }
                case "hexagon_r_path":
                    {
                        return ColorChecker(Hexagon_R_Path.Fill, Hexagon_K_Path.Fill) || ColorChecker(Hexagon_R_Path.Fill, Hexagon_L_Path.Fill) ||
                            ColorChecker(Hexagon_R_Path.Fill, Hexagon_RR_Path.Fill) || ColorChecker(Hexagon_R_Path.Fill, Hexagon_X_Path.Fill) ||
                            ColorChecker(Hexagon_R_Path.Fill, Hexagon_V_Path.Fill) || ColorChecker(Hexagon_R_Path.Fill, Hexagon_P_Path.Fill);
                    }
                case "hexagon_rr_path":
                    {
                        return ColorChecker(Hexagon_RR_Path.Fill, Hexagon_L_Path.Fill) || ColorChecker(Hexagon_RR_Path.Fill, Hexagon_M_Path.Fill) ||
                            ColorChecker(Hexagon_RR_Path.Fill, Hexagon_S_Path.Fill) || ColorChecker(Hexagon_RR_Path.Fill, Hexagon_Z_Path.Fill) ||
                            ColorChecker(Hexagon_RR_Path.Fill, Hexagon_X_Path.Fill) || ColorChecker(Hexagon_RR_Path.Fill, Hexagon_R_Path.Fill);
                    }
                case "hexagon_s_path":
                    {
                        return ColorChecker(Hexagon_S_Path.Fill, Hexagon_M_Path.Fill) || ColorChecker(Hexagon_S_Path.Fill, Hexagon_RR_Path.Fill) ||
                            ColorChecker(Hexagon_S_Path.Fill, Hexagon_Z_Path.Fill) || ColorChecker(Hexagon_S_Path.Fill, Hexagon_ZZ_Path.Fill);
                    }
                case "hexagon_ss_path":
                    {
                        return ColorChecker(Hexagon_SS_Path.Fill, Hexagon_N_Path.Fill) || ColorChecker(Hexagon_SS_Path.Fill, Hexagon_T_Path.Fill);
                    }
                case "hexagon_t_path":
                    {
                        return ColorChecker(Hexagon_T_Path.Fill, Hexagon_SS_Path.Fill) || ColorChecker(Hexagon_T_Path.Fill, Hexagon_N_Path.Fill) ||
                           ColorChecker(Hexagon_T_Path.Fill, Hexagon_O_Path.Fill) || ColorChecker(Hexagon_T_Path.Fill, Hexagon_U_Path.Fill);
                    }
                case "hexagon_u_path":
                    {
                        return ColorChecker(Hexagon_U_Path.Fill, Hexagon_T_Path.Fill) || ColorChecker(Hexagon_U_Path.Fill, Hexagon_O_Path.Fill) ||
                           ColorChecker(Hexagon_U_Path.Fill, Hexagon_P_Path.Fill) || ColorChecker(Hexagon_U_Path.Fill, Hexagon_V_Path.Fill);
                    }
                case "hexagon_v_path":
                    {
                        return ColorChecker(Hexagon_V_Path.Fill, Hexagon_U_Path.Fill) || ColorChecker(Hexagon_V_Path.Fill, Hexagon_P_Path.Fill) ||
                            ColorChecker(Hexagon_V_Path.Fill, Hexagon_R_Path.Fill) || ColorChecker(Hexagon_V_Path.Fill, Hexagon_X_Path.Fill);
                    }
                case "hexagon_x_path":
                    {
                        return ColorChecker(Hexagon_X_Path.Fill, Hexagon_V_Path.Fill) || ColorChecker(Hexagon_X_Path.Fill, Hexagon_R_Path.Fill) ||
                            ColorChecker(Hexagon_X_Path.Fill, Hexagon_RR_Path.Fill) || ColorChecker(Hexagon_X_Path.Fill, Hexagon_Z_Path.Fill);
                    }
                case "hexagon_z_path":
                    {
                        return ColorChecker(Hexagon_Z_Path.Fill, Hexagon_X_Path.Fill) || ColorChecker(Hexagon_Z_Path.Fill, Hexagon_RR_Path.Fill) ||
                           ColorChecker(Hexagon_Z_Path.Fill, Hexagon_S_Path.Fill) || ColorChecker(Hexagon_Z_Path.Fill, Hexagon_ZZ_Path.Fill);
                    }
                case "hexagon_zz_path":
                    {
                        return ColorChecker(Hexagon_ZZ_Path.Fill, Hexagon_Z_Path.Fill) || ColorChecker(Hexagon_ZZ_Path.Fill, Hexagon_S_Path.Fill);
                    }
            }

            return false;
        }*/
    }
}