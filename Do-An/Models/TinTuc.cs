namespace WebApplication17.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
    using System.Web.Mvc;

    [Table("TinTuc")]
    public partial class TinTuc
    {
        [Key]
        public int matintuc { get; set; }

        [StringLength(500)]
        public string tieude { get; set; }

        [StringLength(50)]
        public string hinhanh { get; set; }

        [Column(TypeName = "date")]
        public DateTime? ngaytao { get; set; }

        [StringLength(500)]
        public string gioithieu { get; set; }

        public bool? trangthai { get; set; }

        public int? makhachhang { get; set; }

        
        [AllowHtml]
        [Column(TypeName = "nvarchar(MAX)")]
        public string noidung { get; set; }
    }
}
