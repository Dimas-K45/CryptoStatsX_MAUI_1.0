using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoStatsX_MAUI.Resources.Services.SQLite
{
    [Table("TokensTransactionBuy")]
    public class TokensTransactionBuy
    {
        [PrimaryKey, AutoIncrement, Column("Id")]
        public int Id { get; set; }

        [Column("TokenId")]
        public string TokenId { get; set; }

        [Column("Price")]
        public double Price { get; set; }

        [Column("Count")]
        public double Count { get; set; }

        [Column("Date")]
        public DateTime Date { get; set; }

        [Column("IdBagTokens")]
        public int IdBagTokens { get; set; }

    }
}
