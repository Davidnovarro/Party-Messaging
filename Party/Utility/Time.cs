using System;
using System.Diagnostics;

namespace Party.Utility
{
    public static class Time
    {
        // Date and time when the application started
        static readonly Stopwatch stopwatch = new Stopwatch();

        static Time()
        {
            stopwatch.Start();
        }

        // returns the clock time _in this system_
        public static double time
        {
            get
            {
                return stopwatch.Elapsed.TotalSeconds;
            }
        }

        // returns the clock time _in this system_
        public static double timeMilliseconds
        {
            get
            {
                return stopwatch.Elapsed.TotalMilliseconds;
            }
        }

        public static long UtcNow => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();


        public static double ElapsedSinceUTC(long utcTime)
        {
            return (UtcNow - utcTime) / 1000d;
        }

        public static double ElapsedSince(double timeStamp)
        {
            return time - timeStamp;
        }
    }
}
