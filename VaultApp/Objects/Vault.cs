using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using VaultApp.Service;

namespace VaultApp.Objects
{
    class Vault
    {
        public string HashedPinCode;
        public string Filename;
        public string Filepath;
        public SortedList<string, Domain> Domains = new SortedList<string, Domain>();

        public Vault()
        {

        }

        public Vault(string serializedObject)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(serializedObject);

            XmlNodeList l = xmlDoc.GetElementsByTagName("Domain");
            foreach (XmlNode node in l)
            {
                string name = node.SelectSingleNode("Name").InnerText;
                Domain domain = new Domain(name);
                XmlNodeList ln = node.SelectNodes("Entry");
                foreach (XmlNode lnNode in ln)
                {
                    Entry entry = new Entry();
                    string username = lnNode.SelectSingleNode("Username").InnerText;
                    entry.SetUsername(username);

                    XmlNodeList lnp = lnNode.SelectSingleNode("PreviousPasswords").SelectNodes("PreviousPassword");
                    foreach (XmlNode lnpNode in lnp)
                    {
                        entry.SetPassword(Crypto.Decrypt(lnpNode.InnerText, ApplicationSettings.GeneralEncryptionKey));
                    }

                    string encryptedPassword = lnNode.SelectSingleNode("Password").InnerText;
                    entry.SetPassword(Crypto.Decrypt(encryptedPassword, ApplicationSettings.GeneralEncryptionKey));
                    domain.Entries.Add(entry);
                }
                Domains.Add(domain.Name, domain);
            }
        }

        public Domain AddDomain(string Name)
        {
            if(Domains.ContainsKey(Name))
            {
                return Domains[Name];
            }
            else
            {
                Domain dom = new Domain(Name);
                Domains.Add(Name, dom);
                return dom;
            }
        }

        public Domain FindDomain(string Name)
        {
            if (Domains.ContainsKey(Name))
            {
                return Domains[Name];
            }
            return null;
        }

        public bool ContainsDomain(Domain dom)
        {
            if(Domains.ContainsKey(dom.Name))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool ContainsDomain(String Name)
        {
            if (Domains.ContainsKey(Name))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public string Serialize()
        {
            string Serialized = "";
            XmlDocument xmlDocument = new XmlDocument();
            Serialized += "<Vault>\n";
            foreach(KeyValuePair<string, Domain> p in Domains)
            {
                Domain domain = p.Value;
                Serialized += "<Domain>\n";
                Serialized += "<Name>" + domain.Name + "</Name>\n";
                foreach (Entry entry in domain.Entries)
                {
                    Serialized += entry.Serialize();
                }
                Serialized += "</Domain>\n";
            }
            Serialized += "</Vault>";
            return Serialized;
        }
    }
}
