using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


static class TimerMethods
{
    public static string FormatTimerString(int milliseconds)
    {
        TimeSpan time = TimeSpan.FromMilliseconds(milliseconds);
        return time.ToString(@"mm\:ss\:ff");
    }
}

