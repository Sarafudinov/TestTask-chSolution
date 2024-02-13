
using System.Net;
using Newtonsoft.Json.Linq;
using WebSocket = WebSocket4Net.WebSocket;
using WebSocketState = WebSocket4Net.WebSocketState;

namespace TestTask.core
{
    public class Core
    {
        const int LIMIT = 10_000;
        public List<Thread> Threads { get; set; }
        public List<TradeItem> Trades { get; set; }
        public List<TradePair> Pairs { get; set; }

        public Core() 
        {
            Threads = new List<Thread>();

            Trades = new List<TradeItem>();

            Pairs = GetTradePairs();
        }

        public List<TradePair> GetTradePairs()
        {
            string apiUrl = $"https://api.binance.com/api/v3/exchangeInfo";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(apiUrl);
            request.Method = "GET";

            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream responseStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(responseStream);

                string responseString = reader.ReadToEnd();

                JObject exchangeInfo = JObject.Parse(responseString);

                JArray symbols = (JArray)exchangeInfo["symbols"];
                List<TradePair> tradePairs = new List<TradePair>();
                foreach (var symbol in symbols)
                {
                    TradePair tradePair = new TradePair();
                    tradePair.BaseAsset = symbol["baseAsset"].ToString();
                    tradePair.QuoteAsset = symbol["quoteAsset"].ToString();
                    tradePairs.Add(tradePair);
                }

                reader.Close();
                responseStream.Close();
                response.Close();
                return tradePairs;
            }
            catch (WebException ex)
            {
                Console.WriteLine("Ошибка: " + ex.Message);
                return null;
            }
        }

        public void SubscribeToTrades(string tradingPair)
        {
            string streamName = tradingPair.ToLower() + "@trade";
            using (var ws = new WebSocket($"wss://stream.binance.com:9443/ws/{streamName}"))
            {
                AutoResetEvent connectEvent = new AutoResetEvent(false);

                ws.Opened += (sender, e) => connectEvent.Set();

                ws.Open();

                if (!connectEvent.WaitOne(TimeSpan.FromSeconds(5)))
                {
                    Console.WriteLine("Не удалось подключиться к WebSocket.");
                    return;
                }

                ws.MessageReceived += (sender, e) =>
                {
                    string data = e.Message;
                    TradeItem tradeData = ParseTradeData(data);

                    lock (this.Trades)
                    {
                        this.Trades.Add(tradeData);
                    }
                };

                while (ws.State == WebSocketState.Open)
                {
                    Thread.Sleep(1550);

                }
            }
        }

        private TradeItem ParseTradeData(string jsonData)
        {
            JObject tradeInfo = JObject.Parse(jsonData);
            return new TradeItem 
            {
                TradeType = tradeInfo["e"].ToString(),
                Pair = tradeInfo[key: "s"].ToString(),

                Price = (double)tradeInfo[key: "p"],
                Quantity = (double)tradeInfo[key: "q"],

                DealId = (long)tradeInfo[key: "t"],
                TimeStamp = (long)tradeInfo[key: "E"],
                BuyOrderId = (long)tradeInfo[key: "b"],
                SellOrderId = (long)tradeInfo[key: "a"],
                TransactionTime = (long)tradeInfo[key: "T"],

                IsMakerOrTaker = (bool)tradeInfo[key: "m"],
                IsMarketOrLimit = (bool)tradeInfo[key: "M"],
            };
        }

    }





}
