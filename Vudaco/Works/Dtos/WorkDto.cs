using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vudaco.Works.Dtos
{
    public class CreateWorkRequest
    {
        public int Id { get; set; }
        public List<WorkDto> CongViec { get; set; } = new();
        public int StorageId { get; set; }
    }

    public class WorkDto
    {
        public string TieuDe { get; set; }

        public int LoaiCongViec { get; set; }

        public bool NhomCongViec { get; set; }

        public DateTime? ThoiGianLap { get; set; }

        public DateTime? ThoiGianKetThucLap { get; set; }

        public List<FileItemDto> FileList { get; set; } = new();

        public List<WorkDetailDto> ChiTiet { get; set; } = new();
    }

    public class FileItemDto
    {
        public string FileName { get; set; }

        public string ExternalLink { get; set; }
    }

    public class WorkDetailDto
    {
        public string TenCongViec { get; set; }

        public string MoTaCongViec { get; set; }

        // Danh sách ID nhân viên phụ trách
        public List<int> NguoiPhuTrach { get; set; } = new();

        public DateTime? HanHoanThanh { get; set; }

        public List<string> Checklist { get; set; } = new();
    }
}
