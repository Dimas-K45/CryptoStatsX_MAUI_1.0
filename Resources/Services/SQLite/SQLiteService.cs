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

        public void AddData(string TokenID, double TokenCount)
        {
            BagTokens_1 bagTokens = new BagTokens_1
            {
                TokenID = TokenID,
                TokenCount = TokenCount
            };
            db.Insert(bagTokens);
        }

        public string GetData()
        {
            var data = db.Table<BagTokens_1>();
            return data.ToList()[0].TokenID;
            
            foreach (var item in data)
            {
                return item.TokenID;
            }
            return "XZ";
        }
    }
}
