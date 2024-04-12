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
        [PrimaryKey, Column("TokenID")]
        public string TokenID { get; set; }

        [Column("TokenCount")]
        public double TokenCount { get; set; }

        [Column("AVGPrice")]
        public double AVGPrice { get; set; }
        
    }
}
