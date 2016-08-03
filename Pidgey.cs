using PidgeyBot.Utils;
using PokemonGo.RocketAPI.Enums;
using PokemonGo.RocketAPI.Exceptions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static PidgeyBot.Utils.Logger;

namespace PidgeyBot
{
    class Pidgey
    {
        public static ArrayList accountPtcHolder = new ArrayList();
        public static ArrayList accountPtcHolderTwo = new ArrayList();

        public static ArrayList accountGoogleHolder = new ArrayList();
        public static ArrayList accountGoogleHolderTwo = new ArrayList();

        static void Main(string[] args)
        {
            drawHeader();
            try {
                run();
            }
            catch (PtcOfflineException)
            {
                Logger.Write("PTC Servers are probably down OR your credentials are wrong. Try google", LogLevel.Error);
            }
            catch (Exception ex)
            {
                Logger.Write($"Unhandled exception: {ex}", LogLevel.Error);
            }
            Console.ReadKey();
        }

        public static void run()
        {
            Settings s = new Settings();
            TextReader tr;
            string line;

            // Pokemon Trainer Account Loading
            if (s.UsePtcAccounts)
            {
                Logger.Write("Loading Pokemon Trainer Accounts...");
                var accountsPtcPath = AppDomain.CurrentDomain.BaseDirectory + "accountsPtc.txt";
                tr = File.OpenText(accountsPtcPath);
                while ((line = tr.ReadLine()) != null)
                {
                    accountPtcHolder.Add(line);
                    accountPtcHolderTwo.Add(line);
                }
                Logger.Write(accountPtcHolder.Count + " Pokemon Trainer Account(s) Loaded");
                tr.Close();
            }
            // Google Account Loading
            if (s.UseGoogleAccounts)
            {
                Logger.Write("Loading Google Accounts...");
                var accountsGooglePath = AppDomain.CurrentDomain.BaseDirectory + "accountsGoogle.txt";
                tr = File.OpenText(accountsGooglePath);
                line = "";
                while ((line = tr.ReadLine()) != null)
                {
                    accountGoogleHolder.Add(line);
                    accountGoogleHolderTwo.Add(line);
                }
                Logger.Write(accountGoogleHolder.Count + " Google Account(s) Loaded");
                tr.Close();
            }


            if (s.UsePtcAccounts)
            {
                Logger.Write("Connecting first all PTC Accounts...");
                foreach (string account in accountPtcHolder)
                {
                    Settings seperated = s;
                    try
                    {
                        string[] stringSeparators = new string[] { "|" };
                        var result = account.Split(stringSeparators, StringSplitOptions.None);

                        accountPtcHolderTwo.RemoveAt(0);

                        if (result[0].Contains("username"))
                        {
                            Logger.Write("No PTC Accounts detected.", LogLevel.Warning);
                            continue;
                        }
                        if (result.Length > 2 && result[2] != null)
                        {
                            PidgeyInstance instance;
                            if (!result[2].Contains("lat"))
                                instance = new PidgeyInstance(seperated, AuthType.Ptc, result[0], result[1], double.Parse(result[2], CultureInfo.InvariantCulture), double.Parse(result[3], CultureInfo.InvariantCulture));
                            else
                                instance = new PidgeyInstance(seperated, AuthType.Ptc, result[0], result[1]);
                            Task.Run(() => instance.Execute());
                        }
                        else
                        {
                            PidgeyInstance instance = new PidgeyInstance(seperated, AuthType.Ptc, result[0], result[1]);
                            Task.Run(() => instance.Execute());
                        }
                        Thread.Sleep(50);
                    }
                    catch (Exception e)
                    {
                        Logger.Write("Accounts File Error: You may have an issue in your accountsPtc.txt\nTry creating it again!\n" + e.Message, LogLevel.Error);
                        Thread.Sleep(4000);
                    }
                }
            }

            if (s.UseGoogleAccounts)
            {
                Logger.Write("Connecting now all Google Accounts...");
                foreach (string account in accountGoogleHolder)
                {
                    Settings seperated = s;
                    try
                    {
                        string[] stringSeparators = new string[] { "|" };
                        var result = account.Split(stringSeparators, StringSplitOptions.None);

                        accountGoogleHolderTwo.RemoveAt(0);

                        if (result[0].Contains("email"))
                        {
                            Logger.Write("No Google Accounts detected.", LogLevel.Warning);
                        }
                        if (result.Length > 2 && result[2] != null)
                        {
                            PidgeyInstance instance;
                            if (!result[2].Contains("lat"))
                                instance = new PidgeyInstance(seperated, AuthType.Google, result[0], result[1], double.Parse(result[2], CultureInfo.InvariantCulture), double.Parse(result[3], CultureInfo.InvariantCulture));
                            else
                                instance = new PidgeyInstance(seperated, AuthType.Google, result[0], result[1]);
                            Task.Run(() => instance.Execute());
                        }
                        else
                        {
                            PidgeyInstance instance = new PidgeyInstance(seperated, AuthType.Google, result[0], result[1]);
                            Task.Run(() => instance.Execute());
                        }
                        Thread.Sleep(50);
                    }
                    catch (Exception e)
                    {
                        Logger.Write("Accounts File Error: You may have an issue in your accountsGoogle.txt\nTry creating it again!\n" + e.Message, LogLevel.Error);
                        Thread.Sleep(4000);
                    }
                }
            }
        }

        static void drawHeader()
        {
            Console.Title = "PidgeyBot - v1.6 | PidgeyBot.com";
            string t1 = "PidgeyBot - v1.6";
            string t2 = "Visit PidgeyBot.com";
            string t3 = "Thanks to FeroxRev and all Contributors";

            System.Console.ForegroundColor = ConsoleColor.Cyan;
            for (int i = 1; i < Console.WindowWidth; i++)
            {
                Console.Write("=");
            }
            Console.Write("\n");
            Console.WriteLine(String.Format("{0," + ((Console.WindowWidth / 2) + (t1.Length / 2)) + "}", t1));
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(String.Format("{0," + ((Console.WindowWidth / 2) + (t2.Length / 2)) + "}", t2));
            Console.WriteLine(String.Format("{0," + ((Console.WindowWidth / 2) + (t3.Length / 2)) + "}", t3));
            Console.ForegroundColor = ConsoleColor.Cyan;
            for (int i = 1; i < Console.WindowWidth; i++)
            {
                Console.Write("=");
            }
            Console.Write("\n");
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
