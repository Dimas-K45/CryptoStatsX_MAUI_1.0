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
        "Crypto2.db3");

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

        public void AddDataToken(string TokenID, double TokenCount, double AVGPriceBuy, double AVGPriceSale, int IdBagTokens)
        {
            TokensAssets bagTokens = new TokensAssets
            {
                TokenID = TokenID,
                TokenCount = TokenCount,
                AVGPriceBuy = AVGPriceBuy, 
                AVGPriceSale = AVGPriceSale, 
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

        public bool UpDateTokenPlus(string TokenId, double Count, double Price, int BagTokensId)
        {
            var existingItem = db.Table<TokensAssets>().FirstOrDefault(x => x.TokenID == TokenId && x.IdBagTokens == BagTokensId);
            if (existingItem != null)
            {
                existingItem.TokenCount += Count;
                db.Update(existingItem);
                return true;
            }
            else
            {
                AddDataToken(TokenId, 0, Price, 0, BagTokensId);
                var existingItem2 = db.Table<TokensAssets>().FirstOrDefault(x => x.TokenID == TokenId && x.IdBagTokens == BagTokensId);
                existingItem2.TokenCount += Count;
                db.Update(existingItem2);
                return true;
            }
        }
        public bool UpDateTokenMinus(string TokenId, double Count, int BagTokensId)
        {
            var existingItem = db.Table<TokensAssets>().FirstOrDefault(x => x.TokenID == TokenId && x.IdBagTokens == BagTokensId);
            if (existingItem != null)
            {
                if (existingItem.TokenCount - Count >= 0)
                {
                    existingItem.TokenCount = existingItem.TokenCount - Count;
                    db.Update(existingItem);
                    return true;
                }
                else return false;
            }
            else
            {
                return false;
            }
        }
        public void AddTransactionBuy(string TokenId, double Price, double Count, DateTime Date, int BagTokensId)
        {
            TokensTransactionBuy bagTokens = new TokensTransactionBuy
            {
                TokenId = TokenId,
                Price = Price,
                Count = Count,
                Date = Date,
                IdBagTokens = BagTokensId
            };
            db.Insert(bagTokens);
            
            TokensAssets tokensAssets = new TokensAssets
            {
                TokenID = TokenId,
                TokenCount = Count,
                AVGPriceBuy = Price,
                IdBagTokens = BagTokensId
            };
            // Проверяем, существует ли запись с указанным TokenID
            var existingItem = db.Table<TokensAssets>().FirstOrDefault(x => x.TokenID == tokensAssets.TokenID && x.IdBagTokens == BagTokensId);

            if (existingItem == null)
            {
                // Если запись не существует, добавляем новую
                db.Insert(tokensAssets);
            }
            else
            {
                var trans = db.Table<TokensTransactionBuy>().Where(x => x.TokenId == TokenId && x.IdBagTokens == BagTokensId);

                // Если запись существует, обновляем существующие данные
                existingItem.TokenCount += tokensAssets.TokenCount;
                double summTokenTrans = 0;
                foreach (var item in trans)
                {
                    summTokenTrans += item.Price * item.Count;
                }
                existingItem.AVGPriceBuy = summTokenTrans / existingItem.TokenCount;
                db.Update(existingItem);
            }
        }

        public bool CheckExist(string TokenID, int BagTokenId)
        {
            var existingItem = db.Table<TokensAssets>().FirstOrDefault(x => x.TokenID == TokenID && x.IdBagTokens == BagTokenId);

            if (existingItem == null)
            {
                return false;
            }
            else { return true; }
        }
        public void AddTransactionSale(string TokenId, double Price, double Count, DateTime Date, int BagTokenId, bool tetherIsAdd)
        {
            TokensTransactionSale bagTokens = new TokensTransactionSale
            {
                TokenId = TokenId,
                Price = Price,
                Count = Count,
                Date = Date,
                IdBagTokens = BagTokenId
            };
            db.Insert(bagTokens);

            var transBuy = db.Table<TokensTransactionBuy>().Where(x => x.TokenId == TokenId && x.IdBagTokens == BagTokenId);
            var transSell = db.Table<TokensTransactionSale>().Where(x => x.TokenId == TokenId && x.IdBagTokens == BagTokenId);
            var existingItem = db.Table<TokensAssets>().FirstOrDefault(x => x.TokenID == TokenId && x.IdBagTokens == BagTokenId);

            double summTokenTrans = 0;
            double summCountTrans = 0;
            foreach (var item in transBuy)
            {
                summTokenTrans += item.Price * item.Count;
                summCountTrans += item.Count;
            }
            foreach (var item in transSell)
            {
                summTokenTrans += item.Price * item.Count;
                summCountTrans += item.Count;
            }
            existingItem.AVGPriceSale = summTokenTrans / summCountTrans;
            db.Update(existingItem);

            if (tetherIsAdd )
            {
                UpDateTokenPlus("tether", Price * Count, 1, BagTokenId);
            }
        }

        public List<TokensTransactionBuy> GetListTokensTransactionBuy(int BagTokenId)
        {
            var data = db.Table<TokensTransactionBuy>().Where(x => x.IdBagTokens == BagTokenId).ToList();
            return data;
        }
        public List<TokensTransactionSale> GetListTokensTransactionSale(int BagTokenId)
        {
            var data = db.Table<TokensTransactionSale>().Where(x => x.IdBagTokens == BagTokenId).ToList();
            return data;
        }
        public List<TokensTransactionBuy> GetListTokensTransactionBuyToTokenId(string TokenId, int BagTokenId)
        {
            var data = db.Table<TokensTransactionBuy>().Where(x => x.TokenId == TokenId && x.IdBagTokens == BagTokenId).ToList();
            return data;
        }
        public List<TokensTransactionSale> GetListTokensTransactionSaleToTokenId(string TokenId, int BagTokenId)
        {
            var data = db.Table<TokensTransactionSale>().Where(x => x.TokenId == TokenId && x.IdBagTokens == BagTokenId).ToList();
            return data;
        }

        public List<TokensAssets> GetListTokens(int BagTokenId)
        {
            var data = db.Table<TokensAssets>().Where(x => x.IdBagTokens == BagTokenId).ToList();
            return data;
        }
        public List<AssetsPortfileList> GetListBagTokens()
        {
            var data = db.Table<AssetsPortfileList>().ToList();

            return data;
        }

        public TokensAssets GetTokenToId(string TokenID, int BagTokenId)
        {
            var token = db.Table<TokensAssets>()
              .Where(x => x.TokenID == TokenID && x.IdBagTokens == BagTokenId)
              .FirstOrDefault();
            
            return token;
        }

        public void UpDateTransactionBuy(int id, string TokenId, double Price, double Count, DateTime Date, int BagTokenId)
        {
            TokensTransactionBuy update = new TokensTransactionBuy
            {
                Id = id,
                TokenId = TokenId,
                Price = Price,
                Count = Count,
                Date = Date,
                IdBagTokens = BagTokenId
            };
            db.Update(update);

            var trans = db.Table<TokensTransactionBuy>().Where(x => x.TokenId == TokenId && x.IdBagTokens == BagTokenId);
            var existingItem = db.Table<TokensAssets>().FirstOrDefault(x => x.TokenID == TokenId && x.IdBagTokens == BagTokenId);

            double summTokenTrans = 0;
            foreach (var item in trans)
            {
                summTokenTrans += item.Price * item.Count;
            }
            existingItem.AVGPriceBuy = summTokenTrans / existingItem.TokenCount + Count;
            db.Update(existingItem);
        }
        public void UpDateTransactionSale(int id, string TokenId, double Price, double Count, DateTime Date, int BagTokenId)
        {
            TokensTransactionSale update = new TokensTransactionSale
            {
                Id = id,
                TokenId = TokenId,
                Price = Price,
                Count = Count,
                Date = Date,
                IdBagTokens = BagTokenId
            };
            db.Update(update);

            var trans = db.Table<TokensTransactionSale>().Where(x => x.TokenId == TokenId && x.IdBagTokens == BagTokenId);
            var existingItem = db.Table<TokensAssets>().FirstOrDefault(x => x.TokenID == TokenId && x.IdBagTokens == BagTokenId);

            double summTokenTrans = 0;
            foreach (var item in trans)
            {
                summTokenTrans += item.Price * item.Count;
            }
            existingItem.AVGPriceSale = summTokenTrans / existingItem.TokenCount + Count;
            db.Update(existingItem);
        }

        public void DelTransactionBuyIsId(int id, string TokenId, int BagTokenId)
        {
            var existingItem = db.Table<TokensTransactionBuy>().FirstOrDefault(x => x.Id == id);
            
            if (existingItem != null)
            {
                db.Delete(existingItem);
            }

            var trans = db.Table<TokensTransactionBuy>().Where(x => x.TokenId == TokenId && x.IdBagTokens == BagTokenId);
            var existingItemToken = db.Table<TokensAssets>().FirstOrDefault(x => x.TokenID == TokenId && x.IdBagTokens == BagTokenId);

            double summTokenTrans = 0;
            foreach (var item in trans)
            {
                summTokenTrans += item.Price * item.Count;
            }
            existingItemToken.AVGPriceBuy = summTokenTrans / existingItemToken.TokenCount;
            db.Update(existingItem);
        }
        public TokensTransactionBuy GetTransactionBuyIsId(int id)
        {
            var existingItem = db.Table<TokensTransactionBuy>().FirstOrDefault(x => x.Id == id);
            return existingItem;
        }
        public void DelTransactionSaleIsId(int id, string TokenId, int BagTokenId)
        {
            var existingItem = db.Table<TokensTransactionSale>().FirstOrDefault(x => x.Id == id);
            if (existingItem != null)
            {
                db.Delete(existingItem);
            }
            var trans = db.Table<TokensTransactionSale>().Where(x => x.TokenId == TokenId && x.IdBagTokens == BagTokenId);
            var existingItemToken = db.Table<TokensAssets>().FirstOrDefault(x => x.TokenID == TokenId && x.IdBagTokens == BagTokenId);

            double summTokenTrans = 0;
            foreach (var item in trans)
            {
                summTokenTrans += item.Price * item.Count;
            }
            existingItemToken.AVGPriceSale = summTokenTrans / existingItemToken.TokenCount;
            db.Update(existingItem);
        }
        public TokensTransactionSale GetTransactionSaleIsId(int id)
        {
            var existingItem = db.Table<TokensTransactionSale>().FirstOrDefault(x => x.Id == id);
            return existingItem;
        }

        public void DelAll()
        {
            
            db.DeleteAll<TokensTransactionSale>();
            db.DeleteAll<TokensTransactionBuy>();
            db.DeleteAll<TokensAssets>();
            //db.Query<TokensAssets>($"DELETE FROM BagTokens_1");
        }
    }
}
