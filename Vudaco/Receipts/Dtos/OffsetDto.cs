using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Vudaco.Debits.Models;

namespace Vudaco.Receipts.Dtos
{
    public class OffsetDto
    {
        public int Id { get; set; }
        public int? AReceiptId { get; set; }
        public int? BReceiptId { get; set; }
        public int StorageId { get; set; }
        public DateTime AccountingDate { get; set; }
        public string customerName { get; set; }
        public string DebitThu { get; set; }
        public string DebitChi { get; set; }
        public int Price { get; set; }
        public string Note { get; set; }
        public int? Type { get; set; } = null;
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
