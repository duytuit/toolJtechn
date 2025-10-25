using System;
namespace Vudaco.Vehicles.Dtos
{
    public class VehicleDto
    {
        public int Id { get; set; }
        public string NumberCode { get; set; }
        public int IsExternalDriver { get; set; }
        public string Note { get; set; }
        public int StorageId { get; set; }

        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
