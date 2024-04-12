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
                db.CreateTable<BagTokens_1>();
            }
            catch { }
        }

        public void AddDataToken(string TokenID, double TokenCount, double AVGPrice)
        {
            BagTokens_1 bagTokens = new BagTokens_1
            {
                TokenID = TokenID,
                TokenCount = TokenCount,
                AVGPrice = AVGPrice
            };
            db.Insert(bagTokens);
        }

        public List<BagTokens_1> GetListTokens()
        {
            var data = db.Table<BagTokens_1>();
            List<BagTokens_1> bagTokens = new List<BagTokens_1>();
            foreach (var item in data)
            {
                bagTokens.Add(item);
            }
            return bagTokens;
        }


        public BagTokens_1 GetTokenToId(string TokenID)
        {
            var token = db.Get<BagTokens_1>(TokenID);
            return token;
        }

        public void DelAll()
        {
            db.Query<BagTokens_1>($"DELETE FROM BagTokens_1");
        }
    }
}
