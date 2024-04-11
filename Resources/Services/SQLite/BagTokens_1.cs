using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoStatsX_MAUI.Resources.Services.SQLite
{
    [Table("BagTokens_1")]
    public class BagTokens_1
    {
        [PrimaryKey, AutoIncrement, Column("id")]
        public int Id { get; set; }

        [Column("TokenID")]
        public string TokenID { get; set; }

        [Column("TokenCount")]
        public double TokenCount { get; set; }
        
    }
}
