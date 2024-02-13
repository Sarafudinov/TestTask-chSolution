using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TestTask.client;

namespace TestTask.core
{
    public class App
    {
        private readonly object _lockObject = new object();
        private volatile bool _isRunning = true;
        private int deleted = 0;
        private int geted = 0;

        public void MainDialog()
        {
            Chat chat = new Chat();


            Console.WriteLine("Getting trade pairs...");

            Core core = new Core();

            Dictionary<string, TradeItem> memoryStorage = new Dictionary<string, TradeItem>();

            Console.WriteLine("1. Manual selection");
            Console.WriteLine("2. Interactive selection");
            string choise = Console.ReadLine();
            string symbol = "";
            switch (choise)
            {
                case "1":
                    Console.WriteLine("Enter the trading pair symbol (for example, BTCUSDT):");
                    symbol = Console.ReadLine().ToUpper();
                    break;
                case "2":
                    symbol = chat.InteractivSelection(core.Pairs);
                    break;
                default:
                    symbol = "BTCUSDT";
                    break;
            }

            List<string> remake = new List<string>();
            if (symbol.Contains(','))
            {
                string temp = Regex.Replace(symbol, @"\s", ""); ;
                remake = temp.Split(',').ToList();
            }
            else if (symbol.Contains(' '))
            {
                remake = symbol.Split(' ').ToList();
            }
            else
            {
                remake.Add(symbol);
            }
            remake.RemoveAll(s => string.IsNullOrWhiteSpace(s));

            List<Thread> threads = new List<Thread>();
            List<TradeItem> tradeItems = new List<TradeItem>();

            foreach (var pair in remake)
            {
                Thread pairThread = new Thread(() => core.SubscribeToTrades(pair));
                threads.Add(pairThread);
                pairThread.Start();
            }

            Thread dataDisplayThread = new Thread(() => DisplayData(core.Trades));
            dataDisplayThread.Start();

            Thread cleanupThread = new Thread(() => CleanupOldData(core.Trades));
            cleanupThread.Start();

            dataDisplayThread.Join();
            cleanupThread.Join();

        }
        public void DisplayData(List<TradeItem> allTrades)
        {
            while (_isRunning)
            {
                geted = allTrades.Count;
                Console.Clear();
                Console.WriteLine($"\nTRADES count: {geted}\tDELETED count:{deleted}");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("{0,-10} {1,-10} {2,-10} {3,-10} {4,-25}", "Pair", "DealId", "Price", "Quantity", "TransactionTime");
                Console.ResetColor();

                int index = 0;
                while (index < allTrades.Count && allTrades[index] != null)
                {
                    PrintColoredRow(allTrades[index], allTrades[index].IsMakerOrTaker ? ConsoleColor.Green : ConsoleColor.Red);
                    index++;
                }
                index = 0;

                Thread.Sleep(2000);
            }
        }
        void PrintColoredRow(TradeItem item, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine("{0,-10} {1,-10} {2,-10} {3,-10} {4,-25}", item.Pair, item.DealId, item.Price, item.Quantity, item.TransactionTime);
            Console.ResetColor(); 
        }

        public void CleanupOldData(List<TradeItem> allTrades)
        {
            while (_isRunning)
            {
                int c = allTrades.Count;
                lock (_lockObject)
                {
                    allTrades.RemoveAll(trade => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - trade.TimeStamp > 60000);
                }
                deleted = c - allTrades.Count;

                Thread.Sleep(60000);
            }
        }

    }
}

