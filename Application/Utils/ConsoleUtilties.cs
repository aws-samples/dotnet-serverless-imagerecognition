using System;
using System.Collections.Generic;
using System.Text;

namespace CognitoLogin
{
    internal class ConsoleUtilties
    {
        internal static string Prompt(string message, bool secret)
        {
            Console.WriteLine(message);

            string value;
            if (secret)
            {
                value = ReadSecretLine();
            }
            else
            {
                value = Console.ReadLine();
            }

            return value;
        }

        internal static string PromptForNewPassword()
        {
            string password1, password2;

            password1 = Prompt("Password requires being reset, enter a new password:", true);
            do
            {
                password2 = Prompt("Confirm new password:", true);

                if (!string.Equals(password1, password2))
                {
                    password2 = string.Empty;
                    password1 = Prompt("Passwords do not match, enter a new password:", true);
                }

            } while (!string.Equals(password1, password2));

            return password1;
        }

        private static string ReadSecretLine()
        {
            string secret = "";
            do
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                // Backspace Should Not Work
                if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                {
                    secret += key.KeyChar;
                    Console.Write("*");
                }
                else
                {
                    if (key.Key == ConsoleKey.Backspace && secret.Length > 0)
                    {
                        secret = secret.Substring(0, (secret.Length - 1));
                        Console.Write("\b \b");
                    }
                    else if (key.Key == ConsoleKey.Enter)
                    {
                        break;
                    }
                }
            } while (true);
            Console.WriteLine("");
            return secret;
        }
    }
}
