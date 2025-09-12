namespace WebApplication17.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
    using System.Web.Mvc;

    [Table("SanPham")]
    public partial class SanPham
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SanPham()
        {
            GioHang = new HashSet<GioHang>();
            DonHangChiTiet = new HashSet<DonHangChiTiet>();
        }

        [Key]
        public int masp { get; set; }

        public int mahang { get; set; }

        [Required(ErrorMessage = "Tên sản phẩm không được bỏ trống")]
        [StringLength(500)]
        
        public string tensp { get; set; }

        [StringLength(500)]
        public string hinhanh { get; set; }

        public int? soluong { get; set; }

        public decimal? giaban { get; set; }

        [AllowHtml]
        [Column(TypeName = "nvarchar(MAX)")]
        public string mota { get; set; }

        [StringLength(500)]
        public string CPU { get; set; }

        [StringLength(500)]
        public string RAM { get; set; }

        [StringLength(50)]
        public string OS { get; set; }

        [StringLength(500)]
        public string manhinh { get; set; }

        [StringLength(200)]
        public string carddohoa { get; set; }

        [StringLength(50)]
        public string SSD { get; set; }

        public bool? trangthai { get; set; }

        public bool? IsHotDeal { get; set; }

        [StringLength(200)]
        public string model { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<GioHang> GioHang { get; set; }

        public virtual HangSP HangSP { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DonHangChiTiet> DonHangChiTiet { get; set; }
    }
}
