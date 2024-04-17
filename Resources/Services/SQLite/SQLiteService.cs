using RestSharp;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoStatsX_MAUI.Resources.Services.SQLite
{

    public class SQLiteService
    {
        private static string dbPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        "Crypto.db3");

        private SQLiteConnection db = new SQLiteConnection(dbPath);

        public SQLiteService() 
        {
            try
            {
                db.CreateTable<TokensAssets>();
                db.CreateTable<AssetsPortfileList>();
                db.CreateTable<TokensTransactionBuy>();
                db.CreateTable<TokensTransactionSale>();
            }
            catch { }
        }

        public void AddDataToken(string TokenID, double TokenCount, double AVGPrice, int IdBagTokens)
        {
            TokensAssets bagTokens = new TokensAssets
            {
                TokenID = TokenID,
                TokenCount = TokenCount,
                AVGPrice = AVGPrice, 
                IdBagTokens = IdBagTokens
            };
            db.Insert(bagTokens);
        }
        public void AddBagToken(string Name, string Image)
        {
            AssetsPortfileList bagTokens = new AssetsPortfileList
            {
                Name = Name,
                Image = Image
            };
            db.Insert(bagTokens);
        }
        public void AddTransactionBuy(string TokenId, double Price, double Count, DateTime Date, int IdBagTokens)
        {
            TokensTransactionBuy bagTokens = new TokensTransactionBuy
            {
                TokenId = TokenId,
                Price = Price,
                Count = Count,
                Date = Date,
                IdBagTokens = IdBagTokens
            };
            db.Insert(bagTokens);
            
            TokensAssets tokensAssets = new TokensAssets
            {
                TokenID = TokenId,
                TokenCount = Count,
                AVGPrice = Price,
                IdBagTokens = IdBagTokens
            };
            // Проверяем, существует ли запись с указанным TokenID
            var existingItem = db.Table<TokensAssets>().FirstOrDefault(x => x.TokenID == tokensAssets.TokenID);

            if (existingItem == null)
            {
                // Если запись не существует, добавляем новую
                db.Insert(tokensAssets);
            }
            else
            {
                // Если запись существует, обновляем существующие данные
                existingItem.TokenCount += tokensAssets.TokenCount;
                existingItem.AVGPrice = ((existingItem.AVGPrice * (db.Table<TokensTransactionBuy>().Count(x => x.TokenId == TokenId)-1)) + tokensAssets.AVGPrice) / db.Table<TokensTransactionBuy>().Count(x => x.TokenId == TokenId);
                db.Update(existingItem);
            }
        }
        public void AddTransactionSell(string TokenId, double Price, double Count, DateTime Date, int IdBagTokens)
        {
            TokensTransactionSale bagTokens = new TokensTransactionSale
            {
                TokenId = TokenId,
                Price = Price,
                Count = Count,
                Date = Date,
                IdBagTokens = IdBagTokens
            };
            db.Insert(bagTokens);
        }

        public List<TokensAssets> GetListTokens()
        {
            var data = db.Table<TokensAssets>();
            List<TokensAssets> bagTokens = new List<TokensAssets>();
            foreach (var item in data)
            {
                bagTokens.Add(item);
            }
            return bagTokens;
        }
        public List<AssetsPortfileList> GetListBagTokens()
        {
            var data = db.Table<AssetsPortfileList>();

            List<AssetsPortfileList> bagTokens = new List<AssetsPortfileList>();
            foreach (var item in data)
            {
                bagTokens.Add(item);
            }
            return bagTokens;
        }


        public TokensAssets GetTokenToId(string TokenID)
        {
            var token = db.Get<TokensAssets>(TokenID);
            return token;
        }

        public void DelAll()
        {
            db.Query<TokensAssets>($"DELETE FROM BagTokens_1");
        }
    }
}
