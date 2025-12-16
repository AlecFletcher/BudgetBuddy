using Android.Content;
using Android.OS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Budget_Buddy
{
    public class WakeLockHelper
    {
        public static PowerManager.WakeLock _wakeLock;

        public static void AcquireWakeLock(Context context)
        {
            PowerManager pm = (PowerManager)context.GetSystemService(Context.PowerService);
            // Use WakeLockFlags.Partial for CPU, not screen, or other flags for screen control
            _wakeLock = pm.NewWakeLock(WakeLockFlags.Partial, "MyMauiApp:WakeLockTag");
            _wakeLock.Acquire();
        }

        public static void ReleaseWakeLock()
        {
            if (_wakeLock != null && _wakeLock.IsHeld)
            {
                _wakeLock.Release();
                _wakeLock.Dispose();
            }
        }
    }
}
