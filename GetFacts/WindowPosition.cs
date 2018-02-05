using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;

namespace GetFacts
{
    public class WindowPosition
    {
        [JsonProperty]
        public bool IsMaximized
        {
            get;
            private set;
        }

        [JsonProperty]
        public double Top
        {
            get;
            private set;

        }

        [JsonProperty]
        public double Left
        {
            get;
            private set;
        }

        [JsonProperty]
        public double Width
        {
            get;
            private set;
        }

        [JsonProperty]
        public double Height
        {
            get;
            private set;
        }

        [JsonProperty]
        public bool TopMost
        {
            get;
            private set;
        }

        public static WindowPosition CreateFrom(System.Windows.Controls.Control c)
        {
            WindowPosition wp = new WindowPosition();

            System.Windows.Window w = System.Windows.Window.GetWindow(c);
            wp.IsMaximized = (w.WindowState == System.Windows.WindowState.Maximized);
            wp.Top = w.Top;
            wp.Left = w.Left;
            wp.Width = w.Width;
            wp.Height = w.Height;
            wp.TopMost = w.Topmost;
            return wp;
        }

        /*private JsonSerializerSettings Settings
        {
            get
            {
                JsonSerializerSettings settings = new JsonSerializerSettings();
                return settings;
            }
        }*/

        public static WindowPosition CreateFrom(string json)
        {
            try
            {
                WindowPosition wp = JsonConvert.DeserializeObject<WindowPosition>(json);
                return wp;
            }
            catch
            {
                return null;
            }
        }

        public string ToJson()
        {
            string result = JsonConvert.SerializeObject(this, Formatting.None);
            return result;
        }

        public void ApplyTo(System.Windows.Window w)
        {
            if ( (w.Top>=SystemParameters.VirtualScreenTop) 
                && (w.Top+w.Height<=SystemParameters.VirtualScreenTop+SystemParameters.VirtualScreenHeight) )
            {
                w.Top = Top;
                w.Height = Height;
            }

            if ((w.Left >= SystemParameters.VirtualScreenLeft)
                && (w.Left + w.Width <= SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth))
            {
                w.Left = Left;
                w.Width = Width;
            }

            w.Topmost = TopMost;
            if (IsMaximized)
            {
                w.WindowState = System.Windows.WindowState.Maximized;
            }
        }
    }
}
