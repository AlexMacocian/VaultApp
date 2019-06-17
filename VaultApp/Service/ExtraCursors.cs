using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace XenomenalER.Serviceables
{
    public static class ExtraCursors
    {
        public static Cursor HandGrabCursor = new Cursor(Application.GetResourceStream(new Uri("Resources/Hand Move Grab.cur", UriKind.Relative)).Stream);
        public static Cursor HandNoGrabCursor = new Cursor(Application.GetResourceStream(new Uri("Resources/Hand Move No Grab.cur", UriKind.Relative)).Stream);
    }
}
