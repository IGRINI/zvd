using Game.Utils.Console;
using UnityEngine;

namespace Game.Utils
{
    public static class Dev
    {
        private static ConsoleView _consoleView;
        
        public static void Log(object log)
        {
            Debug.Log(log);
            _consoleView.AddLine(log);
        }
    }
}