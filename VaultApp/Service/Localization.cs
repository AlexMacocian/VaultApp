using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace VaultApp.Service
{
    class Localization
    {
        public static Languages Language = Languages.English;
        public enum Languages
        {
            English,
            Romanian
        }

        public static StackPanel GetLanguageStackPanel()
        {
            StackPanel s = new StackPanel();

            foreach(string lang in Enum.GetNames(typeof (Languages))){
                ToggleButton t = new ToggleButton();
                t.Style = Application.Current.MainWindow.FindResource("SettingsButton") as Style;
                if(Language == Languages.English)
                    t.Content = lang;
                else if(Language == Languages.Romanian)
                {
                    switch (lang)
                    {
                        case "English":
                            t.Content = "Engleză";
                            break;
                        case "Romanian":
                            t.Content = "Română";
                            break;
                        case "Czech":
                            t.Content = "Cehă";
                            break;
                    }
                }
                t.FontSize = 20;
                t.Height = 50;
                t.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                t.HorizontalContentAlignment = HorizontalAlignment.Center;
                t.Background = new SolidColorBrush(Colors.DarkGray);
                if(lang == Enum.GetName((typeof(Languages)), Language))
                {
                    t.IsChecked = true;
                }
                t.Click += LanguageSelect_Click;
                s.Children.Add(t);
            }
            return s;
        }

        private static void LanguageSelect_Click(object sender, RoutedEventArgs e)
        {
            ToggleButton tb = sender as ToggleButton;
            MainWindow m = Application.Current.MainWindow as MainWindow;
            StackPanel s = m.DetailedSettingsExtraGrid.Children[0] as StackPanel;
            foreach (ToggleButton t in s.Children)
            {
                if(tb != t)
                {
                    t.IsChecked = false;
                }
            }

            if((string)tb.Content == "English" || 
                (string)tb.Content == "Engleză" || 
                (string)tb.Content == "Angličtina")
            {
                Language = Languages.English;
            }
            else if((string)tb.Content == "Romanian" ||
                (string)tb.Content == "Română" ||
                (string)tb.Content == "Rumunština")
            {
                Language = Languages.Romanian;
            }

            RegistryKey key = Registry.CurrentUser.OpenSubKey("Software", true);
            key = key.OpenSubKey("VaultApp", true);
            key.SetValue("Language", Enum.GetName(typeof(Languages), Language));

            m.RefreshText();
            if(ApplicationSettings.ApplicationState == ApplicationSettings.AppState.VaultView)
            {
                m.ShowVaultView();
                if(m.DomainGrid.Visibility == Visibility.Visible)
                {
                    m.ShowDomainView();
                }
            }
        }

        public static string GetLocalizedString(string s)
        {
            switch (Language)
            {
                case Languages.English:
                    switch (s)
                    {
                        case "File":
                            return "File";
                        case "Edit":
                            return "Edit";
                        case "Change Pin":
                            return "Change Pin";
                        case "New":
                            return "New";
                        case "Open":
                            return "Open";
                        case "Save":
                            return "Save";
                        case "Save As":
                            return "Save As";
                        case "Close":
                            return "Close";
                        case "Settings":
                            return "Settings";
                        case "Stay on top":
                            return "Stay on top";
                        case "Language":
                            return "Language";
                        case "Run at startup":
                            return "Run at startup";
                        case "Minimize to tray":
                            return "Minimize to tray";
                        case "If toggled, the application window will always stay on top of the other applications":
                            return "If toggled, the application window will always stay on top of the other applications";
                        case "If toggled, the application will start when the computer starts":
                            return "If toggled, the application will start when the computer starts";
                        case "Choose the language for the application":
                            return "Choose the language for the application";
                        case "Error":
                            return "Error";
                        case "Warning":
                            return "Warning";
                        case "Incorrect pin":
                            return "Incorrect pin";
                        case "Incorrect pin code":
                            return "Incorrect pin code";
                        case "Input pin code":
                            return "Input pin code";
                        case "To unlock the file, enter the pin code in the keypad lower, or...":
                            return "To unlock the file, enter the pin code in the keypad lower, or...";
                        case "Drag the generated key file into the marked area lower":
                            return "Drag the generated key file into the marked area lower";
                        case "Invalid dropped file":
                            return "Invalid dropped file";
                        case "Too many files dropped":
                            return "Too many files dropped";
                        case "There is nothing loaded at the moment. Use the menu at the top to load or create a new vault file":
                            return "There is nothing loaded at the moment. Use the menu at the top to load or create a new vault file";
                        case "File name":
                            return "File name";
                        case "1. Write the file name in the textbox to the right \n2. Input the pin code in the provided keypad\n3. Click OK to create the new file":
                            return "1. Write the file name in the textbox to the right\n2. Input the pin code in the provided keypad\n3. Click OK to create the new file";
                        case "Invalid file name":
                            return "Invalid file name";
                        case "Add new domain":
                            return "Add new domain";
                        case "Domain name":
                            return "Domain name";
                        case "Write the name of the new domain and then click \u2611 to save it":
                            return "Write the name of the new domain and then click \u2611 to save it";
                        case "Username":
                            return "Username";
                        case "Password":
                            return "Password";
                        case "Add entry":
                            return "Add entry";
                        case "Invalid username":
                            return "Invalid username";
                        case "Invalid password":
                            return "Invalid password";
                        case "Unable to open file":
                            return "Unable to open file";
                        case "No file loaded yet":
                            return "No file loaded yet";
                        case "To change the pin, write the new one lower. The pin code will be permanently changed on pressing OK":
                            return "To change the pin, write the new one lower. The pin code will be permanently changed on pressing OK";
                        case "Generate Key File":
                            return "Generate Key File";
                        case "As Simple File":
                            return "As Simple File";
                        case "You must wait 5 seconds between pin entries":
                            return "You must wait 5 seconds between pin entries";
                        case "Hidden Into Picture File":
                            return "Hidden Into Picture File";
                        case "If toggled, the application will try to run current loaded file at startup":
                            return "If toggled, the application will try to run current loaded file at startup";
                        case "Associate extension":
                            return "Associate extension";
                        case "If toggled, the .vlt files will be associated with this application":
                            return "If toggled, the .vlt files will be associated with this application";
                        case "File must be saved first":
                            return "File must be saved first";
                        case "Current file to be loaded at startup is":
                            return "Current file to be loaded at startup is";
                        case "Generate Key":
                            return "Generate Key";
                        case "Password settings":
                            return "Password settings";
                        case "Choose which type of characters are allowed into the generated passwords":
                            return "Choose which type of characters are allowed into the generated passwords";
                        case "Symbols (!,@,#,...)":
                            return "Symbols (!,@,#,...)";
                        case "Numbers (1,2,3,...)":
                            return "Numbers (1,2,3,...)";
                        case "Small letters (a,b,c,...)":
                            return "Small letters (a,b,c,...)";
                        case "Big letters (A,B,C,...)":
                            return "Big letters (A,B,C,...)";
                        case "If toggled, when minimizing the application, the icon will be moved to the tray":
                            return "If toggled, when minimizing the application, the icon will be moved to the tray";
                        case "Application has been minimized. Click here to return to normal":
                            return "Application has been minimized. Click here to return to normal";
                        case "Load on startup":
                            return "Load on startup";
                        case "If toggled, the application start when the computer starts.":
                            return "If toggled, the application start when the computer starts.";
                    }
                    break;

                case Languages.Romanian:
                    switch (s)
                    {
                        case "File":
                            return "Fișier";
                        case "Edit":
                            return "Editează";
                        case "Change Pin":
                            return "Schimbă pinul";
                        case "New":
                            return "Nou";
                        case "Open":
                            return "Deschide";
                        case "Save":
                            return "Salvează";
                        case "Save As":
                            return "Salvează ca";
                        case "Close":
                            return "Închide";
                        case "Settings":
                            return "Setări";
                        case "Stay on top":
                            return "Rămâne deasupra";
                        case "Language":
                            return "Limbă";
                        case "Run at startup":
                            return "Rulează la pornire";
                        case "If toggled, the application window will always stay on top of the other applications":
                            return "Dacă este activată, aplicația va sta tot timpul deasupra altor aplicații";
                        case "If toggled, the application will start when the computer starts":
                            return "Dacă este activată, aplicația se va rula la pornirea calculatorului";
                        case "Choose the language for the application":
                            return "Alege limba aplicației";
                        case "Error":
                            return "Eroare";
                        case "Warning":
                            return "Avertisment";
                        case "Incorrect pin":
                            return "Pin incorect";
                        case "Incorrect pin code":
                            return "Cod pin incorect";
                        case "Input pin code":
                            return "Introduceți codul pin";
                        case "To unlock the file, enter the pin code in the keypad lower, or...":
                            return "Pentru a debloca fișierul, introduceți codul pin în tastatura de mai jos, sau...";
                        case "Drag the generated key file into the marked area lower":
                            return "Trageți și plasați fișierul cheie generat peste zona marcată mai jos";
                        case "Invalid dropped file":
                            return "Fișierul plasat este incorect";
                        case "Too many files dropped":
                            return "Prea multe fișiere plasate";
                        case "There is nothing loaded at the moment. Use the menu at the top to load or create a new vault file":
                            return "Nimic nu este încărcat pentru moment. Folosiți meniul de sus pentru a încărca sau crea un nou fișier";
                        case "File name":
                            return "Numele fișierului";
                        case "1. Write the file name in the textbox to the right \n2. Input the pin code in the provided keypad\n3. Click OK to create the new file":
                            return "1. Introduceți numele fișierului în zona din dreapta\n2. Introduceți codul pin în tastatura din dreapta\n3. Apăsați OK pentru a crea noul fișier";
                        case "Invalid file name":
                            return "Numele fișierului este incorect";
                        case "Add new domain":
                            return "Adăugați domeniu";
                        case "Domain name":
                            return "Numele domeniului";
                        case "Write the name of the new domain and then click \u2611 to save it":
                            return "Scrieți numele domeniului și apoi apăsați pe \u2611 pentru a salva domeniul";
                        case "Username":
                            return "Nume de utilizator";
                        case "Password":
                            return "Parolă";
                        case "Add entry":
                            return "Adăugați câmp";
                        case "Invalid username":
                            return "Nume utilizator incorect";
                        case "Invalid password":
                            return "Parolă incorectă";
                        case "Unable to open file":
                            return "Fișierul nu a putut fi deschis";
                        case "No file loaded yet":
                            return "Nici un fișier nu e încărcat momentan";
                        case "To change the pin, write the new one lower. The pin code will be permanently changed on pressing OK":
                            return "Pentru a schimba codul pin, introduceți codul mai jos. Pinul va fi schimbat permanent la apăsarea butonului OK";
                        case "Generate Key File":
                            return "Generează fișier cheie";
                        case "As Simple File":
                            return "Ca și fișier simplu";
                        case "You must wait 5 seconds between pin entries":
                            return "Trebuie să așteptați 5 secunde între încercări";
                        case "Hidden Into Picture File":
                            return "Ascunsă într-o poză";
                        case "If toggled, the application will try to run current loaded file at startup":
                            return "Dacă opțiunea este activată, aplicația va încerca să încarce fișierul curent la fiecare pornire";
                        case "Associate extension":
                            return "Asociază extensia";
                        case "If toggled, the .vlt files will be associated with this application":
                            return "Dacă opțiunea este activată, fișierele de tip .vlt vor fi asociate cu această aplicație";
                        case "File must be saved first":
                            return "Fișierul trebuie salvat";
                        case "Current file to be loaded at startup is":
                            return "Fișierul ce va fi încărcat este";
                        case "Generate Key":
                            return "Generează cheie";
                        case "Password settings":
                            return "Setări parole";
                        case "Choose which type of characters are allowed into the generated passwords":
                            return "Alege ce tip de caractere pot fi folosite pentru a genera parole";
                        case "Symbols (!,@,#,...)":
                            return "Simboluri (!,@,#,...)";
                        case "Numbers (1,2,3,...)":
                            return "Cifre (1,2,3,...)";
                        case "Small letters (a,b,c,...)":
                            return "Litere mici (a,b,c,...)";
                        case "Big letters (A,B,C,...)":
                            return "Litere mari (A,B,C,...)";
                        case "Minimize to tray":
                            return "Minimizați în bara de notificări";
                        case "If toggled, when minimizing the application, the icon will be moved to the tray":
                            return "Dacă este activat, când aplicația este minimizată, icoana va fi mutată în bara de notificări";
                        case "Application has been minimized. Click here to return to normal":
                            return "Apliația a fost minimizată. Apăsați aici pentru a o returna la interfața normală.";
                        case "Load on startup":
                            return "Încarcă la pornire";
                        case "If toggled, the application start when the computer starts.":
                            return "Dacă este activat, aplicația va porni la pornirea calculatorului.";
                    }
                    break;
            }
            return "";
        }
    }
}
