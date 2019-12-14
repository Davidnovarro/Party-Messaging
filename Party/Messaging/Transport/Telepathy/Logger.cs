namespace Telepathy
{
    public static class Logger
    {

#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WEBGL
        public static UnityEngine.Events.UnityAction<string> Log = UnityEngine.Debug.Log;
        public static UnityEngine.Events.UnityAction<string> LogWarning = UnityEngine.Debug.LogWarning;
        public static UnityEngine.Events.UnityAction<string> LogError = UnityEngine.Debug.LogError;
#else
        public static System.Action<string> Log = System.Console.WriteLine;
        public static System.Action<string> LogWarning = System.Console.WriteLine;
        public static System.Action<string> LogError = System.Console.Error.WriteLine;
#endif

    }
}