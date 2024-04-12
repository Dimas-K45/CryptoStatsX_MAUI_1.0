using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoStatsX_MAUI.Resources.Services.SQLite
{
    [Table("TokensTransactionSale")]
    public class TokensTransactionSale
    {
        [PrimaryKey, AutoIncrement, Column("Id")]
        public int Id { get; set; }

        [System.ComponentModel.DataAnnotations.Schema.ForeignKey("BagTokens_1")]
        public string TokenId { get; set; }

        [Column("Price")]
        public double Price { get; set; }

        [Column("Count")]
        public double Count { get; set; }

        [Column("Date")]
        public DateTime Date { get; set; }


    }
}
