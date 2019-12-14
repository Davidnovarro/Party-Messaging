namespace Party.Utility
{
    public static class Logger
    {

        public static void Debug(string value)
        {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WEBGL
            UnityEngine.Debug.Log(value);
#else
            System.Console.WriteLine("[Debug] " + value);
#endif
        }


        public static void Warning(string value)
        {
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
