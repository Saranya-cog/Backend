using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace stockmarket.Models
{
    public class stock
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }
        [BsonElement("CompanyCode")]
        public string CompanyCode { get; set; }
        [BsonElement("StockPrice")]
        public decimal StockPrice { get; set; }
        [BsonElement("Date")]
        public DateTime Date { get; set; }

    }
}
