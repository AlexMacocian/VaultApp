using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace VaultApp.Service
{
    class HelperFunctions
    {
        
        public static byte[] GetARGBFromInt(int a, int r, int g, int b)
        {
            byte[] bytes = new byte[4];
            bytes[0] = Convert.ToByte(a);
            bytes[1] = Convert.ToByte(r);
            bytes[2] = Convert.ToByte(g);
            bytes[3] = Convert.ToByte(b);
            return bytes;
        }

        public static byte[] GetARGBFromColor(MetroColor.MetroColors color)
        {
            byte[] bytes = new byte[4];
            if (color == MetroColor.MetroColors.Orange)
            {
                bytes = GetARGBFromInt(255, 243, 178, 0);
            }
            else if (color == MetroColor.MetroColors.Brown)
            {
                bytes = GetARGBFromInt(255, 99, 47, 0);
            }
            else if (color == MetroColor.MetroColors.Red)
            {
                bytes = GetARGBFromInt(255, 176, 30, 0);
            }
            else if (color == MetroColor.MetroColors.Blue)
            {
                bytes = GetARGBFromInt(255, 0, 106, 193);
            }
            else if (color == MetroColor.MetroColors.SunFlower)
            {
                bytes = GetARGBFromInt(255, 188, 157, 9);
            }
            else if (color == MetroColor.MetroColors.Magenta)
            {
                bytes = GetARGBFromInt(255, 255, 0, 97);
            }
            else if (color == MetroColor.MetroColors.Purple)
            {
                bytes = GetARGBFromInt(255, 114, 0, 172);
            }
            else if (color == MetroColor.MetroColors.DarkGray)
            {
                bytes = GetARGBFromInt(255, 39, 42, 43);
            }
            else if (color == MetroColor.MetroColors.White)
            {
                bytes = GetARGBFromInt(255, 255, 255, 255);
            }
            else if (color == MetroColor.MetroColors.Lime)
            {
                bytes = GetARGBFromInt(255, 140, 191, 38);
            }
            else if (color == MetroColor.MetroColors.Teal)
            {
                bytes = GetARGBFromInt(255, 0, 171, 169);
            }
            else if (color == MetroColor.MetroColors.Pink)
            {
                bytes = GetARGBFromInt(255, 230, 113, 184);
            }
            else if (color == MetroColor.MetroColors.Silver)
            {
                bytes = GetARGBFromInt(255, 200, 200, 200);
            }
            else if (color == MetroColor.MetroColors.LightBlue)
            {
                bytes = GetARGBFromInt(255, 27, 161, 226);
            }
            else if (color == MetroColor.MetroColors.Emerald)
            {
                bytes = GetARGBFromInt(255, 0, 138, 0);
            }
            else if (color == MetroColor.MetroColors.Crimson)
            {
                bytes = GetARGBFromInt(255, 162, 0, 37);
            }
            else if (color == MetroColor.MetroColors.Concrete)
            {
                bytes = GetARGBFromInt(255, 149, 165, 166);
            }
            else if (color == MetroColor.MetroColors.Cloud)
            {
                bytes = GetARGBFromInt(255, 236, 240, 241);
            }
            else if (color == MetroColor.MetroColors.Asbestos)
            {
                bytes = GetARGBFromInt(255, 127, 140, 141);
            }
            else if (color == MetroColor.MetroColors.Cobalt)
            {
                bytes = GetARGBFromInt(255, 0, 80, 239);
            }
            else if (color == MetroColor.MetroColors.Indigo)
            {
                bytes = GetARGBFromInt(255, 106, 0, 155);
            }
            else if (color == MetroColor.MetroColors.DarkBlue)
            {
                bytes = GetARGBFromInt(255, 15, 73, 140);
            }
            else if (color == MetroColor.MetroColors.Black)
            {
                bytes = GetARGBFromInt(255, 0, 0, 0);
            }
            else if (color == MetroColor.MetroColors.Transparent)
            {
                bytes = GetARGBFromInt(0, 255, 255, 255);
            }
            else if (color == MetroColor.MetroColors.LightGray)
            {
                bytes = GetARGBFromInt(255, 235, 235, 235);
            }
            return bytes;
        }

        public static SolidColorBrush GetColor(MetroColor.MetroColors color)
        {
            byte[] bytes = GetARGBFromColor(color);
            SolidColorBrush colorbrush = new SolidColorBrush(Color.FromArgb(bytes[0], bytes[1], bytes[2], bytes[3]));
            return colorbrush;
        }

        public static SolidColorBrush InvertColor(MetroColor.MetroColors color)
        {
            byte[] colortoinvert = GetARGBFromColor(color);
            return new SolidColorBrush(Color.FromArgb(colortoinvert[0], (byte)(255 - colortoinvert[1]), (byte)(255 - colortoinvert[2]), (byte)(255 - colortoinvert[3])));
        }

        public static void ColorAnimate(Control c, MetroColor.MetroColors to)
        {
            byte[] fARGB = GetARGBFromColor(to);
            ColorAnimation ca = new ColorAnimation(Color.FromArgb(fARGB[0], fARGB[1], fARGB[2], fARGB[3]), new Duration(TimeSpan.FromMilliseconds(100)));
            c.Background.BeginAnimation(SolidColorBrush.ColorProperty, ca);

        }

        public static void ColorAnimate(Control c, Color to)
        {
            ColorAnimation ca = new ColorAnimation(to, new Duration(TimeSpan.FromMilliseconds(100)));
            c.Background.BeginAnimation(SolidColorBrush.ColorProperty, ca);

        }

        public static void ColorAnimate(TextBlock c, MetroColor.MetroColors to)
        {
            byte[] fARGB = GetARGBFromColor(to);
            ColorAnimation ca = new ColorAnimation(Color.FromArgb(fARGB[0], fARGB[1], fARGB[2], fARGB[3]), new Duration(TimeSpan.FromMilliseconds(100)));
            c.Background.BeginAnimation(SolidColorBrush.ColorProperty, ca);

        }

        public static void ColorAnimate(TextBlock c, Color to)
        {
            ColorAnimation ca = new ColorAnimation(to, new Duration(TimeSpan.FromMilliseconds(100)));
            c.Background.BeginAnimation(SolidColorBrush.ColorProperty, ca);

        }

        public static void ColorAnimate(StackPanel c, MetroColor.MetroColors to)
        {
            byte[] fARGB = GetARGBFromColor(to);
            ColorAnimation ca = new ColorAnimation(Color.FromArgb(fARGB[0], fARGB[1], fARGB[2], fARGB[3]), new Duration(TimeSpan.FromMilliseconds(100)));
            c.Background.BeginAnimation(SolidColorBrush.ColorProperty, ca);

        }

        public static void ColorAnimate(StackPanel c, Color to)
        {
            ColorAnimation ca = new ColorAnimation(to, new Duration(TimeSpan.FromMilliseconds(100)));
            c.Background.BeginAnimation(SolidColorBrush.ColorProperty, ca);

        }

        public static void ColorAnimate(Grid c, MetroColor.MetroColors to)
        {
            byte[] fARGB = GetARGBFromColor(to);
            ColorAnimation ca = new ColorAnimation(Color.FromArgb(fARGB[0], fARGB[1], fARGB[2], fARGB[3]), new Duration(TimeSpan.FromMilliseconds(100)));
            c.Background.BeginAnimation(SolidColorBrush.ColorProperty, ca);

        }

        public static void ColorAnimate(Grid c, Color to)
        {
            ColorAnimation ca = new ColorAnimation(to, new Duration(TimeSpan.FromMilliseconds(100)));
            c.Background.BeginAnimation(SolidColorBrush.ColorProperty, ca);

        }

        public static int FindStringInListView(String s, ListView lv)
        {
            int index = lv.SelectedIndex;
            s = s.ToLower();
            bool found = false;
            if (index == -1)
            {
                index = 0;
            }
            for (int i = index + 1; i < lv.Items.Count; i++)
            {
                String content = (String)lv.Items[i];
                content = content.ToLower();
                if (content.Contains(s))
                {
                    index = i;
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                for (int i = 0; i <= index; i++)
                {
                    String content = (String)lv.Items[i];
                    content = content.ToLower();
                    if (content.Contains(s))
                    {
                        index = i;
                        found = true;
                        break;
                    }
                }
            }
            return index;
        }

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }
    }
}
