
namespace Vudaco.Vehicles.Dtos
{
    public class VehicleDto
    {
        public long Id { get; set; }
        public string NumberCode { get; set; }
        public bool IsExternalDriver { get; set; }
        public string Note { get; set; }
        public int StorageId { get; set; }

        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public int? DeletedBy { get; set; }
    }
}
