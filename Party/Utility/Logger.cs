namespace Party.Utility
{
    public static class Logger
    {

        public static bool PushDebug = false;

        public static bool PushInfo = true;

        public static bool PushWarnig = true;
        
        public static bool PushError = true;

        public static void Debug(string value)
        {
            if (!PushDebug)
                return;

#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WEBGL
            UnityEngine.Debug.Log(value);
#else
            System.Console.WriteLine("[Debug] " + value);
#endif
        }

        public static void Info(string value)
        {
            if (!PushInfo)
                return;

#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WEBGL
            UnityEngine.Debug.Log(value);
#else
            System.Console.WriteLine("[Info] " + value);
#endif
        }

        public static void Warning(string value)
        {
            if (!PushWarnig)
                return;

#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WEBGL
            UnityEngine.Debug.LogWarning(value);
#else
            System.Console.ForegroundColor = System.ConsoleColor.Yellow;
            System.Console.WriteLine("[Warning] " + value);
            System.Console.ForegroundColor = System.ConsoleColor.Gray;
#endif
        }


        public static void Error(string value)
        {
            if (!PushError)
                return;

#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WEBGL
            UnityEngine.Debug.LogError(value);
#else
            System.Console.ForegroundColor = System.ConsoleColor.Red;
            System.Console.WriteLine("[Error] " + value);
            System.Console.ForegroundColor = System.ConsoleColor.Gray;
#endif
        }

    }
}
