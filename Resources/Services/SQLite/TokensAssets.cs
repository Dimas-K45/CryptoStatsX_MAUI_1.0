using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoStatsX_MAUI.Resources.Services.SQLite
{
    [Table("TokensAssets")]
    public class TokensAssets
    {
        [PrimaryKey, AutoIncrement, Column("Id")]
        public int Id { get; set; }
        
        [Column("TokenID")]
        public string TokenID { get; set; }

        [Column("TokenCount")]
        public double TokenCount { get; set; }

        [Column("AVGPrice")]
        public double AVGPrice { get; set; }

        [Column("IdBagTokens")]
        public int IdBagTokens { get; set; }

    }
}
