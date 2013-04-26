#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace LudumDare26.Mono.Linux
{
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        private static LudumDareGame game;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            game = new LudumDareGame();
            game.Run();
        }
    }
}
