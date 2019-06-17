using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VaultApp.Service
{
    class ApplicationSettings
    {
        public static Settings SelectedSetting = Settings.None;
        public static bool StayOnTop, RunAtStartup, Associated, AllowBigLetters = true, 
            AllowSmallLetters = true, AllowSymbols = true, AllowNumbers = true, MinimizeToTray, LoadOnStartup = false;
        public static AppState ApplicationState = AppState.Intro;
        public static int InitialWarningFontSize = 45;
        public static string GeneralEncryptionKey = @"Ok4+X*So%PIPtAs1";
        public static string FileExtension = ".vlt", FileDescription = "vlt File";
        public static string ApplicationStartupFile = "";
        public static string AppId = "VaultApp";
        public enum Settings
        {
            StayOnTop,
            Language,
            RunAtStartup,
            LoadOnStartup,
            Associate,
            PasswordGenerator,
            MinimizeToTray,
            None
        }
        public enum AppState
        {
            Intro,
            NewFile,
            LoadFile,
            VaultView,
            ChangePinView,
            None
        }
    }
}
