using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using VaultApp.Service;

namespace VaultApp.Objects
{
    class Entry
    {
        List<string> prevPasswords = new List<string>();
        string username;
        string password;

        public Entry()
        {

        }

        public string GetUsername()
        {
            return username;
        }

        public string GetPassword()
        {
            return password;
        }

        public Error SetUsername(string username)
        {
            Regex r = new Regex("^[a-zA-Z0-9.!@?#$%&:';()*,^=_+-]+$");
            if (r.IsMatch(username))
            {
                this.username = username;
                return Error.None;
            }
            else
            {
                return Error.InvalidUsername;
            }
        }

        public Error SetPassword(string password)
        {
            if (prevPasswords.Contains(password))
            {
                return Error.ExistingPassword;
            }
            else
            {
                if(prevPasswords.Count >= 10)
                {
                    prevPasswords.RemoveAt(0);
                }
                if (this.password != null)
                {
                    prevPasswords.Add(this.password);
                }
                this.password = password;
                return Error.None;
            }
        }

        public enum Error
        {
            InvalidUsername,
            InvalidPassword,
            ExistingPassword,
            None
        }

        public string Serialize()
        {
            string Serialized = "";
            Serialized += "<Entry>\n";
            Serialized += "<Username>" + username + "</Username>\n";
            Serialized += "<Password>" + Crypto.Encrypt(password, ApplicationSettings.GeneralEncryptionKey) + "</Password>\n";
            Serialized += "<PreviousPasswords>\n";
            foreach(string prevPassword in prevPasswords)
            {
                Serialized += "<PreviousPassword>" + Crypto.Encrypt(prevPassword, ApplicationSettings.GeneralEncryptionKey) + "</PreviousPassword>\n";
            }
            Serialized += "</PreviousPasswords>\n";
            Serialized += "</Entry>\n";
            return Serialized;
        }
    }
}
