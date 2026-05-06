
namespace Vudaco.PayrollPeriods.Dtos
{
    public class PayrollPeriodDetailDto
    {
        public int Id { get; set; }
        public int StorageId { get; set; }
        public int PayrollPeriodId { get; set; }
        public string CycleName { get; set; }
        public int EmployeeId { get; set; }
        public int Status { get; set; }
        public int LuongCung { get; set; }
        public int SoNgayLam { get; set; }
        public int PhepTon { get; set; }
        public int LuongThucNhan { get; set; }
        public int LamThemNgayThuong { get; set; }
        public int LamThemNgayNghi { get; set; }
        public int LamThemNgayLe { get; set; }
        public int LuongThucTe { get; set; }
        public int TruPhep { get; set; }
        public int LaiXeThuCuoc { get; set; }
        public decimal TyLeHuongLuong { get; set; }
        public int DoanhSo { get; set; }
        public int NghiPhep { get; set; }
        public int NghiKhongLuong { get; set; }
        public string? ChiTietDebit { get; set; }
        public string? ChiTietNghiPhep { get; set; }
        public string? ChiTietPhieuChi { get; set; }
        public string? ChiTietKhoanChi { get; set; }
        public int TongTru { get; set; }
        public int TongUng { get; set; }
        public int Thuong { get; set; }
        public int TroCapKhac { get; set; }
        public int ChiKhac { get; set; }
        public int BaoHiemXaHoi { get; set; }
        public string? GhiChu { get; set; }
        public int DiemTraHang { get; set; }
        public int TienAn { get; set; }
        public int TienVe { get; set; }
        public int DienThoai { get; set; }
        public int QuaDem { get; set; }
        public int Luat { get; set; }
        public int LuongHangVe { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public int? DeletedBy { get; set; }
    }
}
