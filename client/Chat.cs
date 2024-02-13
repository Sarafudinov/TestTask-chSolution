using TestTask.core;

namespace TestTask.client
{
    public class Chat
    {
        public string InteractivSelection(List<TradePair> tradePairs)
        {
            Console.WriteLine("Press \"↑\" or \"↓\" to scroll in the appropriate direction");
            Console.WriteLine("Press \"ENTER\" to select a pair");
            Console.WriteLine("Press \"ESC\" to end selection");

            int selectedIndex = 0;
            bool selectionMade = false;
            string result = "";

            while (!selectionMade)
            {
                ConsoleKeyInfo key = Console.ReadKey();

                switch (key.Key)
                {
                    case ConsoleKey.UpArrow:
                        selectedIndex = Math.Max(0, selectedIndex - 1);
                        break;

                    case ConsoleKey.DownArrow:
                        selectedIndex = (selectedIndex + 1) % tradePairs.Count;
                        break;

                    case ConsoleKey.Enter:
                        result += $"{tradePairs[selectedIndex].BaseAsset}{tradePairs[selectedIndex].QuoteAsset} ";
                        continue;
                    case ConsoleKey.Escape:
                        Console.Clear();
                        return result;
                }

                if (selectedIndex % 5 == 0) Console.Clear();
                Console.WriteLine($"Pair selected: {tradePairs[selectedIndex].BaseAsset}/{tradePairs[selectedIndex].QuoteAsset}");
            }
            return result;
        }

    }
}
