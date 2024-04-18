using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoStatsX_MAUI.Resources.Services
{
    public static class APICoinGecko
    {
        private const string APIKeyCoinGecko = "CG-pibZCCfRXjV16buMmrrk16SU";

        public async static Task<Dictionary<string, object>> GetInfoTokenToID(string TokenID, string cyrrency)
        {
            //var client = new RestClient("https://api.coingecko.com");
            //var request = new RestRequest($"/api/v3/coins/markets?vs_currency={cyrrency}&ids={TokenID}&order=market_cap_desc&per_page=100&page=1&sparkline=false&price_change_percentage=1h%2C24h%2C7d%2C14d%2C30d%2C1y&locale=ru&precision=8");
            var options = new RestClientOptions($"https://api.coingecko.com/api/v3/coins/markets?vs_currency={cyrrency}&ids={TokenID}&order=market_cap_desc&per_page=100&page=1&sparkline=false&price_change_percentage=1h%2C24h%2C7d%2C14d%2C30d%2C1y&locale=ru&precision=8");
            var client = new RestClient(options);
            var request = new RestRequest("");
            request.AddHeader("x-cg-demo-api-key", APIKeyCoinGecko);
            var response = await client.GetAsync(request);

            string content = response.Content.Trim();
            if (content.StartsWith("[") && content.EndsWith("]"))
            {
                content = content.Substring(1, content.Length - 2);
            }

            Dictionary<string, object> dictionary = new Dictionary<string, object>();

            try
            {
                dictionary = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(content);

            }
            catch
            {
                dictionary.Add("symbol", response.Content);
            }

            return dictionary;
        }
        public enum CoinField
        {
            Id,
            Symbol,
            Name,
            Image,
            Current_Price,
            Market_Cap,
            Market_Cap_Rank,
            Fully_Diluted_Valuation,
            Total_Volume,
            High_24h,
            Low_24h,
            Price_Change_24h,
            Price_Change_Percentage_24h,
            Market_Cap_Change_24h,
            Market_Cap_Change_Percentage_24h,
            Circulating_Supply,
            Total_Supply,
            Max_Supply,
            Ath,
            Ath_Change_Percentage,
            Ath_Date,
            Atl,
            Atl_Change_Percentage,
            Atl_Date,
            Roi,
            Last_Updated,
            Price_Change_Percentage_1h_In_Currency,
            Price_Change_Percentage_1y_In_Currency,
            Price_Change_Percentage_14d_In_Currency,
            Price_Change_Percentage_24h_In_Currency,
            Price_Change_Percentage_30d_In_Currency,
            Price_Change_Percentage_7d_In_Currency
        }

        public static async Task<Coin[]> GetTokensInfoToIDs(List<string> TokensID)
        {
            string tokens = "";
            foreach (string token in TokensID)
            {
                tokens = tokens + "%2C%20" + token;
            }
            tokens = tokens.Substring(6);
            var options = new RestClientOptions($"https://api.coingecko.com/api/v3/coins/markets?vs_currency=usd&ids={tokens}&order=market_cap_desc&per_page=250&page=1&sparkline=false&price_change_percentage=1h%2C24h%2C7d%2C14d%2C30d%2C90d%2C1y&locale=en&precision=8");
            var client = new RestClient(options);
            var request = new RestRequest("");
            request.AddHeader("x-cg-demo-api-key", APIKeyCoinGecko);
            var response = await client.GetAsync(request);

            Coin[] coins = JsonConvert.DeserializeObject<Coin[]>(response.Content);
            return coins;
        }
        public class Coin
        {
            public string? Id { get; set; }
            public string? Symbol { get; set; }
            public string? Name { get; set; }
            public string? Image { get; set; }
            public double? current_price { get; set; }
            public long? Market_Cap { get; set; }
            public int? Market_Cap_Rank { get; set; }
            public long? Fully_Diluted_Valuation { get; set; }
            public long? Total_Volume { get; set; }
            public double? High_24h { get; set; }
            public double? Low_24h { get; set; }
            public double? Price_Change_24h { get; set; }
            public double? Price_Change_Percentage_24h { get; set; }
            public long? Market_Cap_Change_24h { get; set; }
            public double? Market_Cap_Change_Percentage_24h { get; set; }
            public long? Circulating_Supply { get; set; }
            public long? Total_Supply { get; set; }
            public long? Max_Supply { get; set; }
            public double Ath { get; set; }
            public double? Ath_Change_Percentage { get; set; }
            public DateTime? Ath_Date { get; set; }
            public double? Atl { get; set; }
            public double? Atl_Change_Percentage { get; set; }
            public DateTime? Atl_Date { get; set; }
            public object? Roi { get; set; }
            public DateTime? Last_Updated { get; set; }
            public double? price_change_percentage_14d_in_currency { get; set; }
            public double? Price_Change_Percentage_1h_In_Currency { get; set; }
            public double? Price_Change_Percentage_1y_In_Currency { get; set; }
            public double? Price_Change_Percentage_24h_In_Currency { get; set; }
            public double? Price_Change_Percentage_30d_In_Currency { get; set; }
            public double? Price_Change_Percentage_7d_In_Currency { get; set; }
        }

        public static async Task<List<string>> GetListToken()
        {
            var options = new RestClientOptions($"https://api.coingecko.com/api/v3/coins/list?include_platform=false");
            var client = new RestClient(options);
            var request = new RestRequest("");
            request.AddHeader("x-cg-demo-api-key", APIKeyCoinGecko);
            var response = await client.GetAsync(request);

            // Разбор строки JSON
            JArray jsonArray = JArray.Parse(response.Content);

            // Извлечение списка id с использованием LINQ
            List<string> idList = jsonArray.Select(j => (string)j["id"]).ToList();
            return idList;
        }

        public static async Task<CryptoCurrency[]> GetListTokenS()
        {
            var options = new RestClientOptions($"https://api.coingecko.com/api/v3/coins/markets?vs_currency=usd&order=market_cap_desc&per_page=250&page=1&sparkline=false&locale=en");
            var client = new RestClient(options);
            var request = new RestRequest("");
            request.AddHeader("x-cg-demo-api-key", APIKeyCoinGecko);
            var response = await client.GetAsync(request);

            CryptoCurrency[] cryptos = JsonConvert.DeserializeObject<CryptoCurrency[]>(response.Content);
            return cryptos;
        }

    }
}
