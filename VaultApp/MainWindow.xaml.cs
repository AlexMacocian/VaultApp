using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using VaultApp.Objects;
using VaultApp.Service;

namespace VaultApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region DECLARATIONS
        private const string Kernel32_DllName = "kernel32.dll";
        private const int WM_SYSCOMMAND = 0x112;
        private HwndSource hwndSource;
        private const int SHCNE_ASSOCCHANGED = 0x8000000;
        private const int SHCNF_FLUSH = 0x1000;
        [DllImport("Shell32.dll")]
        private static extern int SHChangeNotify(int eventId, int flags, IntPtr item1, IntPtr item2);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport(Kernel32_DllName)]
        private static extern bool AllocConsole();

        [DllImport(Kernel32_DllName)]
        private static extern bool FreeConsole();

        bool ShowingSettings = false, Maximized = false, AllowAccept = true, FirstTimeSettings = true, Changed = false;
        string pinCode = string.Empty, openFilePath = "";
        WindowState prevWindowsState;
        NotifyIcon trayIcon;
        Vault currentVault = null;
        Domain currentDomain = null;
        Entry currentEntry = null;
        #endregion

        #region INITIALIZATIONS
        public MainWindow()
        {
            try
            {
                InitializeComponent();
                InitializeApp();
                SetupTheme();
                RefreshText();
                string[] args = System.Environment.GetCommandLineArgs();

                if (args.Length > 1)
                {
                    openFilePath = args[1];
                    ApplicationSettings.ApplicationState = ApplicationSettings.AppState.LoadFile;
                    HideNewFileView();
                    HideVaultView();
                    ShowUnlockView();
                }
                else if (ApplicationSettings.RunAtStartup)
                {
                    openFilePath = ApplicationSettings.ApplicationStartupFile;
                    ApplicationSettings.ApplicationState = ApplicationSettings.AppState.LoadFile;
                    HideNewFileView();
                    HideVaultView();
                    ShowUnlockView();
                }
                else
                {
                    HideVaultView();
                    HideUnlockView();
                    HideNewFileView();
                    ShowIntroView();
                }

                prevWindowsState = this.WindowState;
                trayIcon = new NotifyIcon();
                trayIcon.Text = "VaultApp";
                trayIcon.Icon = Properties.Resources.ApplicationIcon;
                trayIcon.Visible = false;
                trayIcon.Click += OnTrayIconClicked;
                trayIcon.BalloonTipClicked += OnTrayIconClicked;
            }
            catch(Exception e)
            {
                System.Windows.MessageBox.Show(e.Message);
            }
        }

        private void SetupTheme()
        {
            MenuBar.Background = HelperFunctions.GetColor(MetroColor.MetroColors.LightGray);
            foreach (Object o in MenuBar.Items)
            {
                if (o is System.Windows.Controls.MenuItem)
                {
                    System.Windows.Controls.MenuItem mi = o as System.Windows.Controls.MenuItem;
                    mi.Background = HelperFunctions.GetColor(MetroColor.MetroColors.LightGray);
                    mi.Foreground = new SolidColorBrush(Colors.Black);
                    foreach (Object o2 in mi.Items)
                    {
                        if (o2 is System.Windows.Controls.MenuItem)
                        {
                            System.Windows.Controls.MenuItem mi2 = o2 as System.Windows.Controls.MenuItem;
                            mi2.Background = HelperFunctions.GetColor(MetroColor.MetroColors.LightGray);
                            mi2.Foreground = new SolidColorBrush(Colors.Black);
                            foreach (Object o3 in mi2.Items)
                            {
                                if (o3 is System.Windows.Controls.MenuItem)
                                {
                                    System.Windows.Controls.MenuItem mi3 = o3 as System.Windows.Controls.MenuItem;
                                    mi3.Background = HelperFunctions.GetColor(MetroColor.MetroColors.LightGray);
                                    mi3.Foreground = new SolidColorBrush(Colors.Black);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void InitializeApp()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey("Software", true);
            try
            {
                key = key.OpenSubKey(ApplicationSettings.AppId);
                if(key == null)
                {
                    throw new Exception("Key not present");
                }
                ApplicationSettings.StayOnTop = Boolean.Parse((string)key.GetValue("Stay On Top"));
                String openFile = (string)key.GetValue("Load On Startup");
                if(openFile != null)
                {
                    ApplicationSettings.LoadOnStartup = true;
                    ApplicationSettings.ApplicationStartupFile = openFile;
                }
                Service.Localization.Language = (Service.Localization.Languages)Enum.Parse(typeof(Service.Localization.Languages), (string)key.GetValue("Language"));
                ApplicationSettings.MinimizeToTray = Boolean.Parse((string)key.GetValue("Minimize To Tray"));
                ApplicationSettings.AllowBigLetters = Boolean.Parse((string)key.GetValue("Allow Uppercase"));
                ApplicationSettings.AllowSmallLetters = Boolean.Parse((string)key.GetValue("Allow Lowercase"));
                ApplicationSettings.AllowNumbers = Boolean.Parse((string)key.GetValue("Allow Numbers"));
                ApplicationSettings.AllowSymbols = Boolean.Parse((string)key.GetValue("Allow Symbols"));
                ApplicationSettings.RunAtStartup = Boolean.Parse((string)key.GetValue("Run At Startup"));
            }
            catch(Exception e)
            {
                key = Registry.CurrentUser.OpenSubKey("Software", true);
                key = key.CreateSubKey(ApplicationSettings.AppId);
                key.SetValue("Stay On Top", ApplicationSettings.StayOnTop);
                key.SetValue("Language", Enum.GetName(typeof(Service.Localization.Languages), Service.Localization.Language));
                key.SetValue("Minimize To Tray", ApplicationSettings.MinimizeToTray);
                key.SetValue("Allow Uppercase", ApplicationSettings.AllowBigLetters);
                key.SetValue("Allow Lowercase", ApplicationSettings.AllowSmallLetters);
                key.SetValue("Allow Numbers", ApplicationSettings.AllowNumbers);
                key.SetValue("Allow Symbols", ApplicationSettings.AllowSymbols);
                key.SetValue("Run At Startup", ApplicationSettings.RunAtStartup);
            }

            if (FileAssociation.IsAssociated(ApplicationSettings.FileExtension))
            {
                ApplicationSettings.Associated = true;
            }

            Grid parent = (Grid)VisualTreeHelper.GetParent(Keypad);
            parent.Children.Remove(Keypad);
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            hwndSource = PresentationSource.FromVisual((Visual)sender) as HwndSource;
        }
        #endregion

        #region WINDOW FUNCTIONS
        private void Window_Deactivated(object sender, EventArgs e)
        {
            Window window = sender as Window;
            window.Topmost = ApplicationSettings.StayOnTop;
            if(window.Topmost == false)
            {
                TitleBarBackground.Fill = new SolidColorBrush(Colors.Gray);
            }
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            TitleBarBackground.Fill = new SolidColorBrush(Colors.DodgerBlue);
        }
        
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ChangeKeypadFontSize();
            ChangeWarningFontSize();
        }

        private void Resize(object sender, MouseButtonEventArgs e)
        {
            var clickedShape = sender as System.Windows.Controls.Button;
            switch (clickedShape.Name)
            {
                case "TopBorder":
                    ResizeWindow(ResizeDirection.Top);
                    break;
                case "RightBorder":
                    ResizeWindow(ResizeDirection.Right);
                    break;
                case "BottomBorder":
                    ResizeWindow(ResizeDirection.Bottom);
                    break;
                case "LeftBorder":
                    ResizeWindow(ResizeDirection.Left);
                    break;
                case "TopLeftCorner":
                    ResizeWindow(ResizeDirection.TopLeft);
                    break;
                case "TopRightCorner":
                    ResizeWindow(ResizeDirection.TopRight);
                    break;
                case "BottomRightCorner":
                    ResizeWindow(ResizeDirection.BottomRight);
                    break;
                case "BottomLeftCorner":
                    ResizeWindow(ResizeDirection.BottomLeft);
                    break;
                default:
                    break;
            }
        }

        private void ResizeWindow(ResizeDirection direction)
        {
            SendMessage(hwndSource.Handle, WM_SYSCOMMAND, (IntPtr)direction, IntPtr.Zero);
        }
        #endregion

        #region TITLE BAR FUNCTIONS
        private void TitleBar_MouseDown(Object o, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            if (Changed)
            {
                ShowAlert();
            }
            else
            {
                trayIcon.Visible = false;
                this.Close();
            }
        }

        private void ResizeButton_Click(object sender, RoutedEventArgs e)
        {
            Maximized = !Maximized;
            if (Maximized)
            {
                this.WindowState = WindowState.Maximized;
                ResizeImage.Source = new BitmapImage(new Uri(@"/Resources/Restore Down.png", UriKind.Relative));
            }
            else
            {
                this.WindowState = WindowState.Normal;
                ResizeImage.Source = new BitmapImage(new Uri(@"/Resources/Maximize.png", UriKind.Relative));
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (ApplicationSettings.MinimizeToTray)
            {
                MoveToTray();
            }
            else
            {
                this.WindowState = WindowState.Minimized;
            }
        }
        #endregion

        #region SETTINGS VIEW FUNCTIONS
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            ShowingSettings = !ShowingSettings;
            if (ShowingSettings)
            {
                ShowSettings();
            }
            else
            {
                HideSettings();
            }
        }

        private void ShowSettings()
        {
            Storyboard s = new Storyboard();

            DoubleAnimation da = new DoubleAnimation();
            da.From = 0;
            da.To = 100;
            da.Duration = new Duration(TimeSpan.FromMilliseconds(500));
            da.Completed += ShowSettingsCompleted;

            DoubleAnimation da2 = new DoubleAnimation();
            da2.From = 0;
            da2.To = 25;
            da2.Duration = new Duration(TimeSpan.FromMilliseconds(500));

            DoubleAnimation da3 = new DoubleAnimation();
            da3.From = 0;
            da3.To = 90;
            da3.Duration = new Duration(TimeSpan.FromMilliseconds(500));

            s.Children.Add(da3);
            Storyboard.SetTarget(da3, SettingsImage);
            Storyboard.SetTargetProperty(da3, new PropertyPath("(Image.RenderTransform).(RotateTransform.Angle)"));


            SettingsGrid.BeginAnimation(Grid.OpacityProperty, da);
            MainBlurFx.BeginAnimation(BlurEffect.RadiusProperty, da2);
            s.Begin();
        }

        private void HideSettings()
        {
            Storyboard s = new Storyboard();

            DoubleAnimation da = new DoubleAnimation();
            da.To = 0;
            da.From = 100;
            da.Duration = new Duration(TimeSpan.FromMilliseconds(500));

            DoubleAnimation da2 = new DoubleAnimation();
            da2.From = 25;
            da2.To = 0;
            da2.Duration = new Duration(TimeSpan.FromMilliseconds(500));

            DoubleAnimation da3 = new DoubleAnimation();
            da3.From = 90;
            da3.To = 0;
            da3.Duration = new Duration(TimeSpan.FromMilliseconds(500));

            s.Children.Add(da3);
            Storyboard.SetTarget(da3, SettingsImage);
            Storyboard.SetTargetProperty(da3, new PropertyPath("(Image.RenderTransform).(RotateTransform.Angle)"));

            SettingsGrid.BeginAnimation(Grid.OpacityProperty, da);
            MainBlurFx.BeginAnimation(BlurEffect.RadiusProperty, da2);
            SettingsGrid.Visibility = Visibility.Hidden;
            s.Begin();
        }

        private void Setting_Check(object sender, RoutedEventArgs e)
        {
            foreach (object o in SettingsStackPanel.Children)
            {
                ToggleButton tb = o as ToggleButton;
                if (sender != o)
                {
                    tb.IsChecked = false;
                }
            }
            ToggleButton bttn = sender as ToggleButton;
            switch (bttn.Name)
            {
                case "StayOnTop":
                    ApplicationSettings.SelectedSetting = ApplicationSettings.Settings.StayOnTop;
                    DetailGrid.Visibility = Visibility.Visible;

                    DetailedSettingsTitle.Content = Service.Localization.GetLocalizedString("Stay on top");
                    DetailedSettingsToggle.Visibility = Visibility.Visible;
                    DetailedSettingsToggle.IsChecked = ApplicationSettings.StayOnTop;
                    DetailedSettingsExtra.Text = Service.Localization.GetLocalizedString("If toggled, the application window will always stay on top of the other applications");
                    DetailedSettingsExtraGrid.Children.Clear();
                    break;
                case "LoadOnStartup":
                    ApplicationSettings.SelectedSetting = ApplicationSettings.Settings.LoadOnStartup;
                    DetailGrid.Visibility = Visibility.Visible;

                    DetailedSettingsTitle.Content = Service.Localization.GetLocalizedString("Load on startup");
                    DetailedSettingsToggle.Visibility = Visibility.Visible;
                    DetailedSettingsToggle.IsChecked = ApplicationSettings.LoadOnStartup;
                    DetailedSettingsExtra.Text = Service.Localization.GetLocalizedString("If toggled, the application will try to run current loaded file at startup");
                    DetailedSettingsExtraGrid.Children.Clear();
                    if (ApplicationSettings.LoadOnStartup)
                    {
                        TextBlock tb = new TextBlock();
                        tb.TextWrapping = TextWrapping.Wrap;
                        tb.Text = Service.Localization.GetLocalizedString("Current file to be loaded at startup is") + ":\n" + ApplicationSettings.ApplicationStartupFile;
                        DetailedSettingsExtraGrid.Children.Add(tb);
                    }
                    break;
                case "RunAtStartup":
                    ApplicationSettings.SelectedSetting = ApplicationSettings.Settings.RunAtStartup;
                    DetailGrid.Visibility = Visibility.Visible;

                    DetailedSettingsTitle.Content = Service.Localization.GetLocalizedString("Run at startup");
                    DetailedSettingsToggle.Visibility = Visibility.Visible;
                    DetailedSettingsToggle.IsChecked = ApplicationSettings.RunAtStartup;
                    DetailedSettingsExtra.Text = Service.Localization.GetLocalizedString("If toggled, the application start when the computer starts.");
                    DetailedSettingsExtraGrid.Children.Clear();
                    break;
                case "Language":
                    ApplicationSettings.SelectedSetting = ApplicationSettings.Settings.Language;
                    DetailGrid.Visibility = Visibility.Visible;

                    DetailedSettingsTitle.Content = Service.Localization.GetLocalizedString("Language");
                    DetailedSettingsToggle.Visibility = Visibility.Hidden;
                    DetailedSettingsExtra.Text = Service.Localization.GetLocalizedString("Choose the language for the application");

                    DetailedSettingsExtraGrid.Children.Clear();
                    DetailedSettingsExtraGrid.Children.Add(Service.Localization.GetLanguageStackPanel());
                    break;
                case "Associate":
                    ApplicationSettings.SelectedSetting = ApplicationSettings.Settings.Associate;
                    DetailGrid.Visibility = Visibility.Visible;

                    DetailedSettingsTitle.Content = Service.Localization.GetLocalizedString("Associate extension");
                    DetailedSettingsToggle.Visibility = Visibility.Visible;
                    DetailedSettingsToggle.IsChecked = ApplicationSettings.Associated;
                    DetailedSettingsExtra.Text = Service.Localization.GetLocalizedString("If toggled, the .vlt files will be associated with this application");
                    DetailedSettingsExtraGrid.Children.Clear();
                    break;
                case "MinimizeToTray":
                    ApplicationSettings.SelectedSetting = ApplicationSettings.Settings.MinimizeToTray;
                    DetailGrid.Visibility = Visibility.Visible;

                    DetailedSettingsTitle.Content = Service.Localization.GetLocalizedString("Minimize to tray");
                    DetailedSettingsToggle.Visibility = Visibility.Visible;
                    DetailedSettingsToggle.IsChecked = ApplicationSettings.MinimizeToTray;
                    DetailedSettingsExtra.Text = Service.Localization.GetLocalizedString("If toggled, when minimizing the application, the icon will be moved to the tray");
                    DetailedSettingsExtraGrid.Children.Clear();
                    break;
                case "PasswordGenerator":
                    ApplicationSettings.SelectedSetting = ApplicationSettings.Settings.PasswordGenerator;
                    DetailGrid.Visibility = Visibility.Visible;

                    DetailedSettingsTitle.Content = Service.Localization.GetLocalizedString("Password settings");
                    DetailedSettingsToggle.Visibility = Visibility.Hidden;
                    DetailedSettingsExtra.Text = Service.Localization.GetLocalizedString("Choose which type of characters are allowed into the generated passwords");
                    DetailedSettingsExtraGrid.Children.Clear();
                    DetailedSettingsExtraGrid.Children.Add(GeneratePasswordStackPanel());
                    break;
                default:
                    ApplicationSettings.SelectedSetting = ApplicationSettings.Settings.None;
                    break;
            }
        }

        private void Settings_Uncheck(object sender, RoutedEventArgs e)
        {
            DetailGrid.Visibility = Visibility.Hidden;
        }

        private void DetailedSettingsToggle_Click(object sender, RoutedEventArgs e)
        {
            if (ApplicationSettings.SelectedSetting == ApplicationSettings.Settings.StayOnTop)
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey("Software", true);
                key = key.OpenSubKey(ApplicationSettings.AppId, true);
                key.SetValue("Stay On Top", (bool)DetailedSettingsToggle.IsChecked);
                ApplicationSettings.StayOnTop = (bool)DetailedSettingsToggle.IsChecked;
            }
            if (ApplicationSettings.SelectedSetting == ApplicationSettings.Settings.RunAtStartup)
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey("Software", true);
                key = key.OpenSubKey(ApplicationSettings.AppId, true);
                key.SetValue("Run At Startup", (bool)DetailedSettingsToggle.IsChecked);
                ApplicationSettings.RunAtStartup = (bool)DetailedSettingsToggle.IsChecked;
                if (ApplicationSettings.RunAtStartup)
                {
                    EnableRunAtStartup();
                }
                else
                {
                    DisableRunAtStartup();
                }
            }
            if (ApplicationSettings.SelectedSetting == ApplicationSettings.Settings.MinimizeToTray)
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey("Software", true);
                key = key.OpenSubKey(ApplicationSettings.AppId, true);
                key.SetValue("Minimize To Tray", (bool)DetailedSettingsToggle.IsChecked);
                ApplicationSettings.MinimizeToTray = (bool)DetailedSettingsToggle.IsChecked;
            }
            else if (ApplicationSettings.SelectedSetting == ApplicationSettings.Settings.LoadOnStartup)
            {
                ApplicationSettings.LoadOnStartup = (bool)DetailedSettingsToggle.IsChecked;
                if (ApplicationSettings.LoadOnStartup)
                {
                    if (currentVault != null && !string.IsNullOrEmpty(currentVault.Filepath))
                    {
                        RegistryKey key = Registry.CurrentUser.OpenSubKey("Software", true);
                        key = key.OpenSubKey(ApplicationSettings.AppId, true);
                        key.SetValue("Load On Startup", currentVault.Filepath);
                        ApplicationSettings.ApplicationStartupFile = currentVault.Filepath;
                        Setting_Check(LoadOnStartup, new RoutedEventArgs());
                    }
                    else
                    {
                        ShowWarning(Service.Localization.GetLocalizedString("File must be saved first") + "!");
                        DetailedSettingsToggle.IsChecked = false;
                    }
                }
                else
                {
                    RegistryKey key = Registry.CurrentUser.OpenSubKey("Software", true);
                    key = key.OpenSubKey(ApplicationSettings.AppId, true);
                    key.DeleteValue("Load On Startup");
                    ApplicationSettings.ApplicationStartupFile = "";
                    Setting_Check(LoadOnStartup, new RoutedEventArgs());
                }
            }
            else if (ApplicationSettings.SelectedSetting == ApplicationSettings.Settings.Associate)
            {
                ApplicationSettings.Associated = (bool)DetailedSettingsToggle.IsChecked;
                if (ApplicationSettings.Associated)
                {
                    if (!FileAssociation.IsAssociated(ApplicationSettings.FileExtension))
                    {
                        string iconpath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                        iconpath = iconpath.Remove(iconpath.Length - 3, 3);
                        iconpath += "ico";
                        FileAssociation.Associate(ApplicationSettings.FileExtension, ApplicationSettings.AppId, ApplicationSettings.FileDescription, iconpath, System.Reflection.Assembly.GetExecutingAssembly().Location);
                        ApplicationSettings.Associated = (bool)DetailedSettingsToggle.IsChecked;
                    }
                }
                else
                {
                    if (FileAssociation.IsAssociated(ApplicationSettings.FileExtension))
                    {
                        FileAssociation.Disassociate(ApplicationSettings.FileExtension, ApplicationSettings.AppId);
                    }
                }
            }
        }

        private void ShowSettingsCompleted(object sender, EventArgs e)
        {
            SettingsGrid.Visibility = Visibility.Visible;
            if (FirstTimeSettings)
            {
                foreach (object o in SettingsStackPanel.Children)
                {
                    if (o is ToggleButton)
                    {
                        ToggleButton tb = o as ToggleButton;
                        tb.IsChecked = true;
                        FirstTimeSettings = false;
                        return;
                    }
                }
            }
        }

        #endregion

        #region MENU BAR FUNCTIONS
        private void NewMenu_Click(object sender, RoutedEventArgs e)
        {
            HideVaultView();
            HideUnlockView();
            ShowNewFileView();
            ApplicationSettings.ApplicationState = ApplicationSettings.AppState.NewFile;
        }

        private void OpenMenu_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.DefaultExt = ".vlt";
            dialog.Filter = "Vault files (.vlt)|*.vlt";
            Nullable<bool> result = dialog.ShowDialog();
            if (result == true)
            {
                ApplicationSettings.ApplicationState = ApplicationSettings.AppState.LoadFile;
                HideNewFileView();
                HideVaultView();
                ShowUnlockView();
                openFilePath = dialog.FileName;
                Changed = false;
            }
        }

        private void SaveMenu_Click(object sender, RoutedEventArgs e)
        {
            if (ApplicationSettings.ApplicationState == ApplicationSettings.AppState.VaultView &&
                currentVault != null)
            {
                if (string.IsNullOrEmpty(currentVault.Filepath))
                {
                    Microsoft.Win32.SaveFileDialog dialog = new Microsoft.Win32.SaveFileDialog();
                    dialog.FileName = currentVault.Filename;
                    dialog.DefaultExt = ".vlt";
                    dialog.Filter = "Vault files (.vlt)|*.vlt";
                    Nullable<bool> result = dialog.ShowDialog();
                    if (result == true)
                    {
                        currentVault.Filepath = dialog.FileName;
                        string initialString = currentVault.Serialize();
                        string key = currentVault.HashedPinCode;
                        System.IO.File.WriteAllBytes(currentVault.Filepath, Encoding.ASCII.GetBytes(Crypto.Encrypt(initialString, key)));
                        Changed = false;
                    }
                }
                else
                {
                    string initialString = currentVault.Serialize();
                    string key = currentVault.HashedPinCode;
                    System.IO.File.WriteAllBytes(currentVault.Filepath, Encoding.ASCII.GetBytes(Crypto.Encrypt(initialString, key)));
                    Changed = false;
                }
            }
        }

        private void SaveAsMenu_Click(object sender, RoutedEventArgs e)
        {
            if (ApplicationSettings.ApplicationState == ApplicationSettings.AppState.VaultView &&
                currentVault != null)
            {
                Microsoft.Win32.SaveFileDialog dialog = new Microsoft.Win32.SaveFileDialog();
                dialog.FileName = currentVault.Filename;
                dialog.DefaultExt = ".vlt";
                dialog.Filter = "Vault files (.vlt)|*.vlt";
                Nullable<bool> result = dialog.ShowDialog();
                if (result == true)
                {
                    currentVault.Filepath = dialog.FileName;
                    string initialString = currentVault.Serialize();
                    string key = currentVault.HashedPinCode;
                    System.IO.File.WriteAllBytes(currentVault.Filepath, Encoding.ASCII.GetBytes(Crypto.Encrypt(initialString, key)));
                    Changed = false;
                }
            }
        }

        private void CloseMenu_Click(object sender, RoutedEventArgs e)
        {
            currentVault = null;
            currentEntry = null;
            currentDomain = null;
            pinCode = "";
            HideUnlockView();
            HideNewFileView();
            HideVaultView();
            ApplicationSettings.ApplicationState = ApplicationSettings.AppState.Intro;
        }

        private void GenerateKeyFile_Click(object sender, RoutedEventArgs e)
        {
            if (ApplicationSettings.ApplicationState == ApplicationSettings.AppState.VaultView &&
                currentVault != null)
            {       
                Microsoft.Win32.SaveFileDialog dialog = new Microsoft.Win32.SaveFileDialog();
                dialog.FileName = currentVault.Filename + " key";
                dialog.DefaultExt = ".vky";
                dialog.Filter = "Vault key files (.vky)|*.vky";
                Nullable<bool> result = dialog.ShowDialog();
                if (result == true)
                {
                    string key = currentVault.HashedPinCode;
                    System.IO.File.WriteAllBytes(dialog.FileName, Encoding.ASCII.GetBytes(key));
                }
            }
            else
            {
                ShowWarning(Service.Localization.GetLocalizedString("No file loaded yet") + "!", 40);
            }
        }

        private void GeneratePicture_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "Image Files(*.PNG; *.BMP; *.JPG; *.GIF)|*.PNG; *.BMP; *.JPG; *.GIF;";
                   Nullable <bool> result = dialog.ShowDialog();
            if (result == true)
            {
                string key = currentVault.HashedPinCode;
                System.IO.File.AppendAllText(dialog.FileName, key);
            }
        }

        private void ChangePin_Click(object sender, RoutedEventArgs e)
        {
            if(currentVault != null && ApplicationSettings.ApplicationState == ApplicationSettings.AppState.VaultView)
            {
                HideVaultView();
                ShowChangePinView();
                ApplicationSettings.ApplicationState = ApplicationSettings.AppState.ChangePinView;
            }
            else
            {
                ShowWarning(Service.Localization.GetLocalizedString("No file loaded yet") + "!", 40);
            }
        }
        #endregion

        #region KEYPAD FUNCTIONS

        private void ShowKeypad(Grid Parent)
        {
            KeyPadInputTextBox.Text = "";
            pinCode = "";
            Parent.Children.Add(Keypad);
            Keypad.Visibility = Visibility.Visible;
            KeyPadInputTextBox.Focus();
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(10);
            timer.Tick += KeypadLoaded;
            timer.IsEnabled = true;
        }

        private void HideKeypad()
        {
            Grid parent = (Grid)VisualTreeHelper.GetParent(Keypad);
            if (parent != null && parent.Children.Contains(Keypad))
            {
                parent.Children.Remove(Keypad);
            }
            Keypad.Visibility = Visibility.Hidden;
        }

        private void KeypadLoaded(object sender, EventArgs e)
        {
            DispatcherTimer timer = sender as DispatcherTimer;
            timer.IsEnabled = false;
            ChangeKeypadFontSize();
        }

        private void HideKeypad_Completed(object sender, EventArgs e)
        {
            Keypad.Visibility = Visibility.Hidden;
        }

        private void KeypadButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Button b = sender as System.Windows.Controls.Button;
            if ((string)b.Content == "DEL")
            {
                if (KeyPadInputTextBox.Text.Length > 0)
                {
                    pinCode = pinCode.Remove(pinCode.Length - 1);
                }
            }
            else if ((string)b.Content == "OK")
            {
                AcceptPin();
            }
            else
            {
                pinCode += b.Content;
            }
            PopulateKeypadInputTextbox();
        }

        private void KeyPadInputTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.NumPad0:
                case Key.D0:
                    pinCode += 0;
                    break;
                case Key.NumPad1:
                case Key.D1:
                    pinCode += 1;
                    break;
                case Key.NumPad2:
                case Key.D2:
                    pinCode += 2;
                    break;
                case Key.NumPad3:
                case Key.D3:
                    pinCode += 3;
                    break;
                case Key.NumPad4:
                case Key.D4:
                    pinCode += 4;
                    break;
                case Key.NumPad5:
                case Key.D5:
                    pinCode += 5;
                    break;
                case Key.NumPad6:
                case Key.D6:
                    pinCode += 6;
                    break;
                case Key.NumPad7:
                case Key.D7:
                    pinCode += 7;
                    break;
                case Key.NumPad8:
                case Key.D8:
                    pinCode += 8;
                    break;
                case Key.NumPad9:
                case Key.D9:
                    pinCode += 9;
                    break;
                case Key.Delete:
                case Key.Back:
                    pinCode = pinCode.Remove(pinCode.Length - 1);
                    break;
                case Key.Enter:
                    AcceptPin();
                    break;
            }
            PopulateKeypadInputTextbox();
            e.Handled = true;
        }

        private void PopulateKeypadInputTextbox()
        {
            KeyPadInputTextBox.Text = "";
            for (int i = 0; i < pinCode.Length; i++)
            {
                KeyPadInputTextBox.Text += '*';
            }
        }

        private void AcceptPin()
        {
            if (AllowAccept)
            {
                DispatcherTimer DisallowTimer = new DispatcherTimer();
                DisallowTimer.Interval = TimeSpan.FromSeconds(5);
                DisallowTimer.Tick += DisallowTimer_Tick;
                DisallowTimer.IsEnabled = true;
                AllowAccept = false;
                if (pinCode == "")
                {
                    ShowWarning(Service.Localization.GetLocalizedString("Error") + "! " + Service.Localization.GetLocalizedString("Incorrect pin code") + "!");
                    return;
                }
                if (ApplicationSettings.ApplicationState == ApplicationSettings.AppState.NewFile)
                {
                    if (string.IsNullOrWhiteSpace(NewFileTextbox.Text) || string.IsNullOrEmpty(NewFileTextbox.Text))
                    {
                        ShowWarning(Service.Localization.GetLocalizedString("Invalid file name") + "!");
                    }
                    else
                    {
                        currentVault = new Vault();
                        currentVault.HashedPinCode = Crypto.GenerateSHA512String(pinCode);
                        currentVault.Filename = NewFileTextbox.Text;
                        HideNewFileView();
                        ShowVaultView();
                        ApplicationSettings.ApplicationState = ApplicationSettings.AppState.VaultView;
                    }
                }
                else if (ApplicationSettings.ApplicationState == ApplicationSettings.AppState.LoadFile)
                {
                    string encryptedString = Encoding.ASCII.GetString(System.IO.File.ReadAllBytes(openFilePath));
                    string key = Crypto.GenerateSHA512String(pinCode);
                    string fileName = System.IO.Path.GetFileNameWithoutExtension(openFilePath);
                    OpenVault(encryptedString, key, fileName);
                }
                else if (ApplicationSettings.ApplicationState == ApplicationSettings.AppState.ChangePinView)
                {
                    currentVault.HashedPinCode = Crypto.GenerateSHA512String(pinCode);
                    HideChangePinView();
                    ShowVaultView();
                    ApplicationSettings.ApplicationState = ApplicationSettings.AppState.VaultView;
                }
            }
            else
            {
                ShowWarning(Service.Localization.GetLocalizedString("You must wait 5 seconds between pin entries"), 35);
            }
        }

        private void DisallowTimer_Tick(object sender, EventArgs e)
        {
            AllowAccept = true;
            DispatcherTimer timer = sender as DispatcherTimer;
            timer.IsEnabled = false;
        }

        private void ChangeKeypadFontSize()
        {
            double initialHeight = Keypad.MaxHeight;
            double currentHeight = Keypad.ActualHeight;
            double scale = (currentHeight / initialHeight);
            if (scale > 0)
            {
                KeyPadInputTextBox.FontSize = 40 * scale;
                KeypadLabel.FontSize = 30 * scale;
            }
        }

        private void ChangeKeypadFontSize(double scale)
        {
            double initialHeight = Keypad.MaxHeight;
            double currentHeight = Keypad.ActualHeight;
            double scaleN = (currentHeight / initialHeight) * scale;
            if (scale > 0)
            {
                KeyPadInputTextBox.FontSize = 40 * scaleN;
            }
        }

        private void KeypadVisibilityButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            KeyPadInputTextBox.Text = pinCode;
            ChangeKeypadFontSize(0.8);
        }

        private void KeypadVisibilityButton_Click(object sender, RoutedEventArgs e)
        {
            PopulateKeypadInputTextbox();
            ChangeKeypadFontSize();
        }

        private void KeypadVisibilityButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            PopulateKeypadInputTextbox();
            ChangeKeypadFontSize();
        }
        #endregion

        #region WARNING VIEW FUNCTIONS

        private void ShowWarning(string Text, TimeSpan duration, int fontSize)
        {
            DoubleAnimation da = new DoubleAnimation();
            da.From = 0;
            da.To = 100;
            da.Duration = new Duration(TimeSpan.FromMilliseconds(100));
            WarningGrid.BeginAnimation(Grid.HeightProperty, da);
            WarningLabel.Content = Text;

            ChangeWarningFontSize(fontSize);

            DispatcherTimer t = new DispatcherTimer();
            t.Interval = duration;
            t.IsEnabled = true;
            t.Tick += Timer_Tick;
        }

        private void ShowWarning(string Text, int fontSize)
        {
            DoubleAnimation da = new DoubleAnimation();
            da.From = 0;
            da.To = 100;
            da.Duration = new Duration(TimeSpan.FromMilliseconds(100));
            WarningGrid.BeginAnimation(Grid.HeightProperty, da);
            WarningLabel.Content = Text;

            ChangeWarningFontSize(fontSize);

            DispatcherTimer t = new DispatcherTimer();
            t.Interval = TimeSpan.FromSeconds(2);
            t.IsEnabled = true;
            t.Tick += Timer_Tick;
        }

        private void ShowWarning(string Text, TimeSpan duration)
        {
            DoubleAnimation da = new DoubleAnimation();
            da.From = 0;
            da.To = 100;
            da.Duration = new Duration(TimeSpan.FromMilliseconds(100));
            WarningGrid.BeginAnimation(Grid.HeightProperty, da);
            WarningLabel.Content = Text;

            DispatcherTimer t = new DispatcherTimer();
            t.Interval = duration;
            t.IsEnabled = true;
            t.Tick += Timer_Tick;
        }

        private void ShowWarning(string Text)
        {
            DoubleAnimation da = new DoubleAnimation();
            da.From = 0;
            da.To = 100;
            da.Duration = new Duration(TimeSpan.FromMilliseconds(100));
            WarningGrid.BeginAnimation(Grid.HeightProperty, da);
            WarningLabel.Content = Text;

            DispatcherTimer t = new DispatcherTimer();
            t.Interval = TimeSpan.FromSeconds(2);
            t.IsEnabled = true;
            t.Tick += Timer_Tick;
        }

        private void HideWarning()
        {
            DoubleAnimation da = new DoubleAnimation();
            da.From = 100;
            da.To = 0;
            da.Duration = new Duration(TimeSpan.FromMilliseconds(100));
            WarningGrid.BeginAnimation(Grid.HeightProperty, da);
        }

        private void ChangeWarningFontSize()
        {
            int initialSize = ApplicationSettings.InitialWarningFontSize;
            double initialWidth = 768;
            double currentWidth = WarningLabel.ActualWidth;
            double newSize = initialSize - (30 * (1 - (currentWidth / initialWidth)));
            if (newSize > 0)
            {
                WarningLabel.FontSize = (int)newSize;
            }
        }

        private void ChangeWarningFontSize(int initialFontSize)
        {
            int initialSize = initialFontSize;
            double initialWidth = 768;
            double currentWidth = WarningLabel.ActualWidth;
            double newSize = initialSize - (30 * (1 - (currentWidth / initialWidth)));
            if (newSize > 0)
            {
                WarningLabel.FontSize = (int)newSize;
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            DispatcherTimer t = sender as DispatcherTimer;
            t.IsEnabled = false;
            t.Stop();
            HideWarning();
        }
        #endregion

        #region UNLOCK VIEW FUNCTIONS
        private void ShowUnlockView()
        {
            UnlockView.Visibility = Visibility.Visible;
            ShowKeypad(UnlockviewKeypadHolder);
        }

        private void HideUnlockView()
        {
            UnlockView.Visibility = Visibility.Hidden;
            HideKeypad();
        }

        private void DropArea_DragOver(object sender, System.Windows.DragEventArgs e)
        {

        }

        private void DropArea_Drop(object sender, System.Windows.DragEventArgs e)
        {
            DropImage.Opacity = 0.3;
            DropArea.Fill = new SolidColorBrush(Colors.Transparent);
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop);
                if (files.Count() == 1)
                {
                    OpenKey(files[0]);
                }
                else //MULTIPLE FILES
                {
                    ShowWarning(Service.Localization.GetLocalizedString("Too many files dropped") + "!");
                }
            }
        }

        private void DropArea_DragEnter(object sender, System.Windows.DragEventArgs e)
        {
            DropImage.Opacity = 0.7;
            SolidColorBrush color = Service.HelperFunctions.GetColor(MetroColor.MetroColors.Black);
            Color newcolor = new Color();
            newcolor.A = 20;
            newcolor.R = color.Color.R;
            newcolor.G = color.Color.G;
            newcolor.B = color.Color.B;
            DropArea.Fill = new SolidColorBrush(newcolor);
        }

        private void DropArea_DragLeave(object sender, System.Windows.DragEventArgs e)
        {
            DropImage.Opacity = 0.3;
            DropArea.Fill = new SolidColorBrush(Colors.Transparent);
        }

        private void DropArea_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(e.ChangedButton == MouseButton.Left && e.LeftButton == MouseButtonState.Pressed)
            {
                Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
                dialog.Filter = "Image Files(*.PNG; *.BMP; *.JPG; *.GIF)|*.PNG; *.BMP; *.JPG; *.GIF;|Key Files(*.VKY)|*.VKY";
                Nullable<bool> result = dialog.ShowDialog();
                if (result == true)
                {
                    OpenKey(dialog.FileName);
                }
            }
        }

        private void OpenKey(string keyPath)
        {
            if (System.IO.Path.GetExtension(keyPath).ToLower() == ".vky")
            {
                string encryptedString = Encoding.ASCII.GetString(System.IO.File.ReadAllBytes(openFilePath));
                string fileName = System.IO.Path.GetFileNameWithoutExtension(openFilePath);
                string key = Encoding.ASCII.GetString(System.IO.File.ReadAllBytes(keyPath));
                OpenVault(encryptedString, key, fileName);
            }
            else if (System.IO.Path.GetExtension(keyPath).ToLower() == ".bmp" ||
                System.IO.Path.GetExtension(keyPath).ToLower() == ".png" ||
                System.IO.Path.GetExtension(keyPath).ToLower() == ".jpg" ||
                System.IO.Path.GetExtension(keyPath).ToLower() == ".gif")
            {
                string encryptedString = Encoding.ASCII.GetString(System.IO.File.ReadAllBytes(openFilePath));
                string key = "";
                using (var reader = new System.IO.StreamReader(keyPath))
                {
                    if (reader.BaseStream.Length > 128)
                    {
                        reader.BaseStream.Seek(-128, System.IO.SeekOrigin.End);
                    }
                    key = reader.ReadToEnd();
                }
                string fileName = System.IO.Path.GetFileNameWithoutExtension(openFilePath);
                OpenVault(encryptedString, key, fileName);
            }
        }
        #endregion

        #region NEW FILE VIEW FUNCTIONS
        private void ShowNewFileView()
        {
            NewFileView.Visibility = Visibility.Visible;
            HideVaultView();
            HideDomainView();
            HideEntryView();
            ShowKeypad(NewFileKeypadHolder);
            NewFileTextbox.Text = "";
            Changed = true;
        }

        private void HideNewFileView()
        {
            NewFileView.Visibility = Visibility.Hidden;
            HideKeypad();
        }
        #endregion

        #region VAULT VIEW FUNCTIONS
        public void ShowVaultView()
        {
            VaultView.Visibility = Visibility.Visible;
            VaultViewFilename.Content = currentVault.Filename;
            VaultViewFilename.FontSize = 45;
            PopulateDomainStackpanel();
            LoadOnStartup.Visibility = Visibility.Visible;
            SaveMenu.Visibility = Visibility.Visible;
            SaveAsMenu.Visibility = Visibility.Visible;
            GenerateMenu.Visibility = Visibility.Visible;
            Separator1.Visibility = Separator2.Visibility = Separator3.Visibility = Visibility.Visible;
            CloseMenu.Visibility = Visibility.Visible;
        }

        private void HideVaultView()
        {
            VaultView.Visibility = Visibility.Hidden;
            HideDomainView();
            HideAddDomainGrid();
            LoadOnStartup.Visibility = Visibility.Collapsed;
            SaveMenu.Visibility = Visibility.Collapsed;
            SaveAsMenu.Visibility = Visibility.Collapsed;
            GenerateMenu.Visibility = Visibility.Collapsed;
            Separator1.Visibility = Separator2.Visibility = Separator3.Visibility = Visibility.Collapsed;
            CloseMenu.Visibility = Visibility.Collapsed;
        }

        private void PopulateDomainStackpanel()
        {
            DomainStackPanel.Children.Clear();
            foreach (KeyValuePair<string, Domain> k in currentVault.Domains)
            {
                Domain domain = k.Value;
                ToggleButton Button = new ToggleButton();
                Button.Style = this.Resources["SettingsButton"] as Style;
                Button.Content = domain.Name;
                Button.Click += DomainButton_Click;
                Button.Height = 50;
                FontSize = 20;
                DomainStackPanel.Children.Add(Button);
            }
            ToggleButton addbttn = new ToggleButton();
            addbttn.Style = this.Resources["SettingsButton"] as Style;
            addbttn.Content = Service.Localization.GetLocalizedString("Add new domain");
            addbttn.Click += AddDomainButton_Click;
            addbttn.Height = 50;
            addbttn.FontSize = 20;
            DomainStackPanel.Children.Add(addbttn);
        }

        private void DomainButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleButton thisButton = sender as ToggleButton;
            foreach (ToggleButton Button in DomainStackPanel.Children)
            {
                if (Button != thisButton)
                {
                    Button.IsChecked = false;
                }
            }
            currentDomain = currentVault.Domains[(string)thisButton.Content];
            ShowDomainView();
        }

        private void AddDomainButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleButton thisButton = sender as ToggleButton;
            foreach (ToggleButton Button in DomainStackPanel.Children)
            {
                if (Button != thisButton)
                {
                    Button.IsChecked = false;
                }
            }
            ShowAddDomainGrid();
        }
        #endregion

        #region ADD NEW DOMAIN VIEW FUNCTIONS

        private void ShowAddDomainGrid()
        {
            AddDomainGrid.Visibility = Visibility.Visible;
            AddDomainNameTextBox.Text = "";
            HideDomainView();
        }

        private void HideAddDomainGrid()
        {
            AddDomainGrid.Visibility = Visibility.Hidden;
        }

        private void AddDomainAccept_Click(object sender, RoutedEventArgs e)
        {
            currentVault.AddDomain(AddDomainNameTextBox.Text);
            HideAddDomainGrid();
            PopulateDomainStackpanel();
            Changed = true;
        }

        private void AddDomainCancel_Click(object sender, RoutedEventArgs e)
        {
            HideAddDomainGrid();
        }

        #endregion

        #region DOMAIN VIEW FUNCTIONS

        public void ShowDomainView()
        {
            DomainGrid.Visibility = Visibility.Visible;
            HideAddDomainGrid();
            HideEntryView();
            PopulateDomainView();
        }

        private void HideDomainView()
        {
            DomainGrid.Visibility = Visibility.Hidden;
            HideEntryView();
        }

        private void PopulateDomainView()
        {
            DomainNameTextBlock.Text = currentDomain.Name;
            EntryStackPanel.Children.Clear();
            foreach(Entry entry in currentDomain.Entries)
            {
                ToggleButton Button = new ToggleButton();
                Button.Style = this.Resources["SettingsButton"] as Style;
                Button.Content = entry.GetUsername();
                Button.Click += EntryButton_Click;
                Button.Height = 50;
                Button.FontSize = 20;
                EntryStackPanel.Children.Add(Button);
            }

            ToggleButton tbutton = new ToggleButton();
            tbutton.Style = this.Resources["SettingsButton"] as Style;
            tbutton.Content = Service.Localization.GetLocalizedString("Add entry");
            tbutton.Click += EntryAddButton_Click;
            tbutton.Height = 50;
            tbutton.FontSize = 20;
            EntryStackPanel.Children.Add(tbutton);
        }

        private void EntryButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleButton thisButton = sender as ToggleButton;
            int index = 0;
            bool found = false;
            foreach (ToggleButton Button in EntryStackPanel.Children)
            {
                if (Button != thisButton)
                {
                    Button.IsChecked = false;
                    if (!found)
                    {
                        index++;
                    }
                }
                else if(!found)
                {
                    found = true;
                }
            }
            currentEntry = currentDomain.Entries[index];
            ShowEntryView();
        }

        private void EntryAddButton_Click(object sender, RoutedEventArgs e)
        {
            currentEntry = new Entry();
            currentDomain.Entries.Add(currentEntry);
            ShowEntryView();
        }

        private void DomainRemove_Click(object sender, RoutedEventArgs e)
        {
            if(currentDomain != null)
            {
                currentVault.Domains.Remove(currentDomain.Name);
                currentDomain = null;
                ShowVaultView();
                HideDomainView();
                Changed = true;
            }
        }

        #endregion

        #region ENTRY VIEW FUNCTIONS

        private void ShowEntryView()
        {
            EntryGrid.Visibility = Visibility.Visible;
            UsernameTextbox.Text = currentEntry.GetUsername();
            PasswordTextbox.Text = currentEntry.GetPassword();
        }

        private void HideEntryView()
        {
            EntryGrid.Visibility = Visibility.Hidden;
        }

        private void EntryBackButton_Click(object sender, RoutedEventArgs e)
        {
            HideEntryView();
            PopulateDomainView();
        }

        private void EntryAcceptButton_Click(object sender, RoutedEventArgs e)
        {
            if(currentEntry.SetUsername(UsernameTextbox.Text) != Entry.Error.None)
            {
                ShowWarning(Service.Localization.GetLocalizedString("Invalid username"));
                return;
            }
            if (currentEntry.SetPassword(PasswordTextbox.Text) != Entry.Error.None)
            {
                ShowWarning(Service.Localization.GetLocalizedString("Invalid password"));
                return;
            }
            HideEntryView();
            PopulateDomainView();
            Changed = true;
        }

        private void EntryRemoveButton_Click(object sender, RoutedEventArgs e)
        {
            HideEntryView();
            currentDomain.Entries.Remove(currentEntry);
            PopulateDomainView();
            Changed = true;
        }

        private void EntryDiceButton_Click(object sender, RoutedEventArgs e)
        {
            PasswordTextbox.Text = Crypto.GetRandomPassword();
        }

        #endregion

        #region CHANGE PIN VIEW FUNCTIONS
        private void ShowChangePinView()
        {
            pinCode = "";
            ChangePinView.Visibility = Visibility.Visible;
            ShowKeypad(ChangePinKeypadHolder);
            Changed = true;
        }

        private void HideChangePinView()
        {
            ChangePinView.Visibility = Visibility.Hidden;
            HideKeypad();
        }
        #endregion

        #region INTRO VIEW FUNCTIONS
        private void ShowIntroView()
        {
            IntroView.Visibility = Visibility.Visible;
        }

        private void HideIntroView()
        {
            IntroView.Visibility = Visibility.Hidden;
        }
        #endregion

        #region ALERT VIEW FUNCTIONS
        private void AlertButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Button bttn = sender as System.Windows.Controls.Button;
            if((string)bttn.Content == "Yes")
            {
                SaveMenu_Click(SaveMenu, e);
                this.Close();
            }
            else if ((string)bttn.Content == "No")
            {
                this.Close();
            }
            else if ((string)bttn.Content == "Cancel")
            {
                HideAlert();
            }
        }
        private void ShowAlert()
        {
            AlertGrid.Visibility = Visibility.Visible;
        }
        private void HideAlert()
        {
            AlertGrid.Visibility = Visibility.Hidden;
        }
        #endregion

        #region MISC
        public void RefreshText()
        {
            FileMenu.Header = Service.Localization.GetLocalizedString("File");
            EditMenu.Header = Service.Localization.GetLocalizedString("Edit");
            OpenMenu.Header = Service.Localization.GetLocalizedString("Open");
            NewMenu.Header = Service.Localization.GetLocalizedString("New");
            SaveMenu.Header = Service.Localization.GetLocalizedString("Save");
            SaveAsMenu.Header = Service.Localization.GetLocalizedString("Save As");
            CloseMenu.Header = Service.Localization.GetLocalizedString("Close");
            GenerateMenu.Header = Service.Localization.GetLocalizedString("Generate Key");
            GenerateTxtMenu.Header = Service.Localization.GetLocalizedString("As Simple File");
            GeneratePicture.Header = Service.Localization.GetLocalizedString("Hidden Into Picture File");
            ChangePinMenu.Header = Service.Localization.GetLocalizedString("Change Pin");
            SettingsLabel.Content = Service.Localization.GetLocalizedString("Settings");
            StayOnTop.Content = Service.Localization.GetLocalizedString("Stay on top");
            Language.Content = Service.Localization.GetLocalizedString("Language");
            RunAtStartup.Content = Service.Localization.GetLocalizedString("Run at startup");
            Associate.Content = Service.Localization.GetLocalizedString("Associate extension");
            PasswordGenerator.Content = Service.Localization.GetLocalizedString("Password settings");
            KeypadLabel.Text = Service.Localization.GetLocalizedString("Input pin code");
            WarningLabel.Content = Service.Localization.GetLocalizedString("Incorrect pin");
            UnlockViewKeypadLabel.Text = Service.Localization.GetLocalizedString("To unlock the file, enter the pin code in the keypad lower, or...");
            UnlockViewKeyLabel.Text = Service.Localization.GetLocalizedString("Drag the generated key file into the marked area lower");
            IntroTextBlock.Text = Service.Localization.GetLocalizedString("There is nothing loaded at the moment. Use the menu at the top to load or create a new vault file");
            NewFileNameLabel.Text = Service.Localization.GetLocalizedString("File name");
            NewFileTextBlock.Text = Service.Localization.GetLocalizedString("1. Write the file name in the textbox to the right \n2. Input the pin code in the provided keypad\n3. Click OK to create the new file");
            AddDomainLabel.Content = Service.Localization.GetLocalizedString("Domain name");
            AddDomainTextBlock.Text = Service.Localization.GetLocalizedString("Write the name of the new domain and then click \u2611 to save it");
            UsernameLabel.Text = Service.Localization.GetLocalizedString("Username");
            PasswordLabel.Text = Service.Localization.GetLocalizedString("Password");
            ChangePinLabel.Text = Service.Localization.GetLocalizedString("To change the pin, write the new one lower. The pin code will be permanently changed on pressing OK");
            if (ShowingSettings)
            {
                switch (ApplicationSettings.SelectedSetting)
                {
                    case ApplicationSettings.Settings.Language:
                        Setting_Check(Language, new RoutedEventArgs());
                        break;
                    case ApplicationSettings.Settings.StayOnTop:
                        Setting_Check(StayOnTop, new RoutedEventArgs());
                        break;
                    case ApplicationSettings.Settings.None:
                        break;
                }
            }
        }

        private enum ResizeDirection
        {
            Left = 61441,
            Right = 61442,
            Top = 61443,
            TopLeft = 61444,
            TopRight = 61445,
            Bottom = 61446,
            BottomLeft = 61447,
            BottomRight = 61448,
        }

        private StackPanel GeneratePasswordStackPanel()
        {
            StackPanel stackpanel = new StackPanel();

            Grid g = new Grid();
            g.Margin = new Thickness(0, 10, 0, 0);
            ColumnDefinition col = new ColumnDefinition();
            col.Width = new GridLength(0.8, GridUnitType.Star);
            g.ColumnDefinitions.Add(col);
            col = new ColumnDefinition();
            col.Width = new GridLength(0.2, GridUnitType.Star);
            g.ColumnDefinitions.Add(col);
            ToggleButton tb = new ToggleButton();
            tb.Name = "Uppercase";
            tb.Click += PasswordGeneratorButton_Clicked;
            tb.Style = this.FindResource("AnimatedSwitch") as Style;
            tb.IsChecked = ApplicationSettings.AllowBigLetters;
            Grid.SetColumn(tb, 1);
            g.Children.Add(tb);
            TextBlock t = new TextBlock();
            t.FontSize = 20;
            t.Text = Service.Localization.GetLocalizedString("Big letters (A,B,C,...)");
            g.Children.Add(t);
            stackpanel.Children.Add(g);

            g = new Grid();
            g.Margin = new Thickness(0, 10, 0, 0);
            col = new ColumnDefinition();
            col.Width = new GridLength(0.8, GridUnitType.Star);
            g.ColumnDefinitions.Add(col);
            col = new ColumnDefinition();
            col.Width = new GridLength(0.2, GridUnitType.Star);
            g.ColumnDefinitions.Add(col);
            tb = new ToggleButton();
            tb.Name = "Lowercase";
            tb.Click += PasswordGeneratorButton_Clicked;
            tb.Style = this.FindResource("AnimatedSwitch") as Style;
            tb.IsChecked = ApplicationSettings.AllowSmallLetters;
            Grid.SetColumn(tb, 1);
            g.Children.Add(tb);
            t = new TextBlock();
            t.FontSize = 20;
            t.Text = Service.Localization.GetLocalizedString("Small letters (a,b,c,...)");
            g.Children.Add(t);
            stackpanel.Children.Add(g);

            g = new Grid();
            g.Margin = new Thickness(0, 10, 0, 0);
            col = new ColumnDefinition();
            col.Width = new GridLength(0.8, GridUnitType.Star);
            g.ColumnDefinitions.Add(col);
            col = new ColumnDefinition();
            col.Width = new GridLength(0.2, GridUnitType.Star);
            g.ColumnDefinitions.Add(col);
            tb = new ToggleButton();
            tb.Name = "Numbers";
            tb.Click += PasswordGeneratorButton_Clicked;
            tb.Style = this.FindResource("AnimatedSwitch") as Style;
            tb.IsChecked = ApplicationSettings.AllowNumbers;
            Grid.SetColumn(tb, 1);
            g.Children.Add(tb);
            t = new TextBlock();
            t.FontSize = 20;
            t.Text = Service.Localization.GetLocalizedString("Numbers (1,2,3,...)");
            g.Children.Add(t);
            stackpanel.Children.Add(g);

            g = new Grid();
            g.Margin = new Thickness(0, 10, 0, 0);
            col = new ColumnDefinition();
            col.Width = new GridLength(0.8, GridUnitType.Star);
            g.ColumnDefinitions.Add(col);
            col = new ColumnDefinition();
            col.Width = new GridLength(0.2, GridUnitType.Star);
            g.ColumnDefinitions.Add(col);
            tb = new ToggleButton();
            tb.Name = "Symbols";
            tb.Style = this.FindResource("AnimatedSwitch") as Style;
            tb.Click += PasswordGeneratorButton_Clicked;
            tb.IsChecked = ApplicationSettings.AllowSymbols;
            Grid.SetColumn(tb, 1);
            g.Children.Add(tb);
            t = new TextBlock();
            t.FontSize = 20;
            t.Text = Service.Localization.GetLocalizedString("Symbols (!,@,#,...)");
            g.Children.Add(t);
            stackpanel.Children.Add(g);

            return stackpanel;
        }

        private void PasswordGeneratorButton_Clicked(object sender, RoutedEventArgs e)
        {
            ToggleButton button = sender as ToggleButton;
            switch (button.Name)
            {
                case "Uppercase":
                    if (ApplicationSettings.AllowNumbers || ApplicationSettings.AllowSmallLetters || ApplicationSettings.AllowSymbols)
                    {
                        ApplicationSettings.AllowBigLetters = !ApplicationSettings.AllowBigLetters;
                        RegistryKey key = Registry.CurrentUser.OpenSubKey("Software", true);
                        key = key.OpenSubKey("VaultApp", true);
                        key.SetValue("Allow Uppercase", ApplicationSettings.AllowBigLetters);
                    }
                    button.IsChecked = ApplicationSettings.AllowBigLetters;
                    break;
                case "Lowercase":
                    if (ApplicationSettings.AllowNumbers || ApplicationSettings.AllowBigLetters || ApplicationSettings.AllowSymbols)
                    {
                        ApplicationSettings.AllowSmallLetters = !ApplicationSettings.AllowSmallLetters;
                        RegistryKey key = Registry.CurrentUser.OpenSubKey("Software", true);
                        key = key.OpenSubKey("VaultApp", true);
                        key.SetValue("Allow Lowercase", ApplicationSettings.AllowSmallLetters);
                    }
                    button.IsChecked = ApplicationSettings.AllowSmallLetters;
                    break;
                case "Numbers":
                    if (ApplicationSettings.AllowBigLetters || ApplicationSettings.AllowSmallLetters || ApplicationSettings.AllowSymbols)
                    {
                        ApplicationSettings.AllowNumbers = !ApplicationSettings.AllowNumbers;
                        RegistryKey key = Registry.CurrentUser.OpenSubKey("Software", true);
                        key = key.OpenSubKey("VaultApp", true);
                        key.SetValue("Allow Numbers", ApplicationSettings.AllowNumbers);
                    }
                    button.IsChecked = ApplicationSettings.AllowNumbers;
                    break;
                case "Symbols":
                    if (ApplicationSettings.AllowNumbers || ApplicationSettings.AllowSmallLetters || ApplicationSettings.AllowBigLetters)
                    {
                        ApplicationSettings.AllowSymbols = !ApplicationSettings.AllowSymbols;
                        RegistryKey key = Registry.CurrentUser.OpenSubKey("Software", true);
                        key = key.OpenSubKey("VaultApp", true);
                        key.SetValue("Allow Symbols", ApplicationSettings.AllowSymbols);
                    }
                    button.IsChecked = ApplicationSettings.AllowSymbols;
                    break;
            }
        }

        private void MoveToTray()
        {
            prevWindowsState = this.WindowState;
            this.WindowState = WindowState.Minimized;
            this.Hide();
            trayIcon.Visible = true;
            trayIcon.ShowBalloonTip(5000, ApplicationSettings.AppId, Service.Localization.GetLocalizedString("Application has been minimized. Click here to return to normal"), ToolTipIcon.Info);
        }

        /// <summary>
        /// Open the file containing the vault information. If an error occurs, a message will be displayed onto the interface.
        /// </summary>
        /// <param name="encryptedString">Contents of the vault file.</param>
        /// <param name="key">Key to decrypt the vault file.</param>
        /// <param name="fileName">Name of the file to be used during future save operations.</param>
        private void OpenVault(string encryptedString, string key, string fileName)
        {
            MainBlurFx.Radius = 10;
            MenuBarBlurFx.Radius = 10;
            LoadingSpinnerGrid.Visibility = Visibility.Visible;
            Task.Run(() => 
            {
                TryDecryptVault(encryptedString, key, fileName);
            });
        }
        /// <summary>
        /// Called on a thread to decrypt the contents asynchronously.
        /// </summary>
        /// <param name="encryptedString">String to be decrypted.</param>
        /// <param name="key">Key used during decryption.</param>
        /// <param name="fileName">Name of the file.</param>
        private void TryDecryptVault(string encryptedString, string key, string fileName)
        {
            try
            {
                string decryptedString = Crypto.Decrypt(encryptedString, key);
                currentVault = new Vault(decryptedString);
                currentVault.HashedPinCode = Crypto.GenerateSHA512String(pinCode);
                string[] tokens = openFilePath.Split('.');
                currentVault.Filename = fileName;
                currentVault.Filepath = openFilePath;
                Changed = false;
                OnVaultOpenSuccess();
            }
            catch (Exception e)
            {
                OnVaultOpenFailure();
            }
        }
        /// <summary>
        /// Called when the async operation of opening the vault was succesfull.
        /// </summary>
        private void OnVaultOpenSuccess()
        {
            Dispatcher.Invoke(() =>
            {
                MainBlurFx.Radius = 0;
                MenuBarBlurFx.Radius = 0;
                LoadingSpinnerGrid.Visibility = Visibility.Hidden;
                HideUnlockView();
                ShowVaultView();
                ApplicationSettings.ApplicationState = ApplicationSettings.AppState.VaultView;
            });
        }
        /// <summary>
        /// Called when the async operation of opening the vault has failed.
        /// </summary>
        private void OnVaultOpenFailure()
        {
            Dispatcher.Invoke(() =>
            {
                MainBlurFx.Radius = 0;
                MenuBarBlurFx.Radius = 0;
                LoadingSpinnerGrid.Visibility = Visibility.Hidden;
                ShowWarning(Service.Localization.GetLocalizedString("Unable to open file"));
            });
        }
        /// <summary>
        /// Called when tray Icon was clicked
        /// </summary>
        private void OnTrayIconClicked(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = prevWindowsState;
            trayIcon.Visible = false;
        }

        private void EnableRunAtStartup()
        {
            try
            {
                Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                Assembly curAssembly = Assembly.GetExecutingAssembly();
                key.SetValue(ApplicationSettings.AppId, curAssembly.Location);
            }
            catch { }
        }

        private void DisableRunAtStartup()
        {
            try
            {
                Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                Assembly curAssembly = Assembly.GetExecutingAssembly();
                key.DeleteValue(ApplicationSettings.AppId);
            }
            catch { }
        }
        #endregion
    }
}
