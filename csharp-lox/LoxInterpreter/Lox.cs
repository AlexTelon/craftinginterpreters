using System;
using System.Collections.Generic;
using System.IO;

namespace LoxInterpreter
{
    class Lox
    {
        private static bool _hadError = false;

        static void Main(string[] args)
        {
            if (args.Length > 1)
            {
                Console.WriteLine("Usage: cslox [script]");
                Environment.Exit(64); // EX_USAGE - FreeBSD sysexit codes.
            } else if (args.Length == 1)
            {
                RunFile(args[0]);
            } else
            {
                RunPrompt();
            }
        }

        /// <summary>
        /// Runs the contents of a file.
        /// </summary>
        /// <param name="path"></param>
        private static void RunFile(string path)
        {
            var data = File.ReadAllText(path);
            Run(data);

            // Indicate an error in the exit code.
            if (_hadError) Environment.Exit(65); //  EX_DATAERR
        }

        /// <summary>
        /// Runs a command in prompt mode.
        /// </summary>
        private static void RunPrompt()
        {
            while (true)
            {
                Console.Write("> ");
                Run(Console.ReadLine());
                _hadError = false; // Reset this after each command in interactive prompt.
            }
        }

        private static void Run(string source)
        {
            Scanner scanner = new Scanner(source);
            List<Token> tokens = scanner.ScanTokens();

            foreach (var token in tokens)
            {
                Console.WriteLine(token);
            }
        }

        private static void Error(int line, string message)
        {
            Report(line, "", message);
        }

        private static void Report(int line, string where, string message)
        {
            System.Diagnostics.Debug.WriteLine("[line {0} Error{1}: {2}", line, where, message);
            _hadError = true;
        }
    }
}
