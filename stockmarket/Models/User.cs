using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace stockmarket.Models
{
    public class User
    {

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }
        [BsonElement("id")]
        public string id { get; set; }
        [BsonElement("CompanyName")]
        public string CompanyName { get; set; }
        [BsonElement("CompanyCode")]
        public string CompanyCode { get; set; }
        [BsonElement("CompanyCeo")]
        public string CompanyCeo { get; set; }
        [BsonElement("Turnover")]
        public string Turnover { get; set; }
        [BsonElement("Website")]
        public string Website { get; set; }
        [BsonElement("StockExchange")]
        public List<string> StockExchange { get; set; }


    }
}
