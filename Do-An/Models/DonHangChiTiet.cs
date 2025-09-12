namespace WebApplication17.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("DonHangChiTiet")]
    public partial class DonHangChiTiet
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int machitietdonhang { get; set; }

    
        public int madonhang { get; set; }

       
        public int masp { get; set; }

        public int? soluong { get; set; }

        public decimal? gia { get; set; }

        public string CauHinhThem { get; set; }
        public decimal? tongtien { get; set; }

        public virtual DonHang DonHang { get; set; }

        public virtual SanPham SanPham { get; set; }
    }
}
