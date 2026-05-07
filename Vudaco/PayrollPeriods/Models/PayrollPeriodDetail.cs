using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Vudaco.Employees.Models;

namespace Vudaco.PayrollPeriods.Models
{
    [Table("payroll_period_details")]
    public class PayrollPeriodDetail
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("storage_id")]
        public int StorageId { get; set; }
        [Required]
        [Column("cycle_name")]
        public string CycleName { get; set; }
        [Required]
        [Column("employee_id")]
        public int EmployeeId { get; set; }
        [Column("status")]
        public int Status { get; set; }
        [Column("payroll_period_id")]
        public int PayrollPeriodId { get; set; }

        [Column("luongcung")]
        public int LuongCung { get; set; }

        [Column("songaylam")]
        public int SoNgayLam { get; set; }

        [Column("trocapkhac")]
        public int TroCapKhac { get; set; }
         [Column("chikhac")]
        public int ChiKhac { get; set; }
        [Column("baohiemxahoi")]
        public int BaoHiemXaHoi { get; set; }
        [Column("luongthucte")]
        public int? LuongThucTe { get; set; }
        [Column("truphep")]
        public int? TruPhep { get; set; }
        [Column("laixethucuoc")]
        public int? LaiXeThuCuoc { get; set; }

        [Column("phepton")]
        public int PhepTon { get; set; }

        [Column("luongthucnhan")]
        public int LuongThucNhan { get; set; }

        [Column("lamthemngaythuong")]
        public int LamThemNgayThuong { get; set; }

        [Column("lamthemngaynghi")]
        public int LamThemNgayNghi { get; set; }

        [Column("lamthemngayle")]
        public int LamThemNgayLe { get; set; }

        [Column("tylehuongluong")]
        public decimal TyLeHuongLuong { get; set; }

        [Column("doanhso")]
        public int DoanhSo { get; set; }

        [Column("nghiphep")]
        public int NghiPhep { get; set; }

        [Column("nghikhongluong")]
        public int NghiKhongLuong { get; set; }

        [Column("chitietdebit")]
        public string? ChiTietDebit { get; set; }

        [Column("chitietnghiphep")]
        public string? ChiTietNghiPhep { get; set; }

        [Column("chitietphieuchi")]
        public string? ChiTietPhieuChi { get; set; }

        [Column("chitietkhoanchi")]
        public string? ChiTietKhoanChi { get; set; }

        [Column("tongtru")]
        public int TongTru { get; set; }

        [Column("tongung")]
        public int TongUng { get; set; }

        [Column("thuong")]
        public int Thuong { get; set; }

        [Column("ghichu")]
        public string? GhiChu { get; set; }

        [Column("diemtrahang")]
        public int DiemTraHang { get; set; }

        [Column("tienan")]
        public int TienAn { get; set; }

        [Column("tienve")]
        public int TienVe { get; set; }

        [Column("dienthoai")]
        public int DienThoai { get; set; }

        [Column("quadem")]
        public int QuaDem { get; set; }

        [Column("luat")]
        public int Luat { get; set; }

        [Column("luonghangve")]
        public int LuongHangVe { get; set; }

        [Column("created_by")]
        public int? CreatedBy { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("updated_by")]
        public int? UpdatedBy { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [Column("deleted_by")]
        public int? DeletedBy { get; set; }

        [Column("deleted_at")]
        public DateTime? DeletedAt { get; set; }
        [NotMapped]
        public string DepartmentName { get; set; }

        [NotMapped]
        public Employee Employee { get; set; }
    }
}
