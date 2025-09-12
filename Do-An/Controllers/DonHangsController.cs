using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebApplication17.Models;
using static WebApplication17.Models.DonHang;

namespace WebApplication17.Controllers
{
    public class DonHangsController : Controller
    {
        private Model1 db = new Model1();

        // GET: DonHangs
        public ActionResult Index()
        {
            List<DonHang> donHangs = db.DonHang.Include(d => d.KhachHang).ToList();
            donHangs = donHangs.OrderByDescending(d => d.madonhang).ToList();
            return View(donHangs);
        }

        public ActionResult ChiTietDonHang(int? id)
        {
            if (id == null)
                return HttpNotFound();

            // Lấy đơn hàng
            DonHang donHang = db.DonHang.Find(id);
            if (donHang == null)
                return HttpNotFound();

            // Lấy chi tiết đơn hàng
            List<DonHangChiTiet> listDHCT = db.DonHangChiTiet
                                              .Where(s => s.madonhang == id)
                                              .ToList();
            ViewBag.DHCT = listDHCT;
            decimal phiVanChuyen = donHang.phivanchuyen ?? 0;
            ViewBag.PhiVanChuyen = phiVanChuyen;

            return View(donHang);
        }

        public ActionResult Huy(int id)
        {
            var donHang = db.DonHang.Find(id);
            if (donHang == null)
                return Json(new { success = false, message = "Không tìm thấy đơn hàng" });

            var listDHCT = db.DonHangChiTiet.Where(ct => ct.madonhang == id).ToList();

            foreach (var ct in listDHCT)
            {
                var sp = db.SanPham.Find(ct.masp);
                if (sp != null)
                {
                    sp.soluong += ct.soluong; // trả lại kho
                }
            }

            donHang.trangthai = TrangThaiDonHang.DaHuy;
            db.SaveChanges();

            return Json(new { success = true });
        }

        [HttpPost]
        public ActionResult XacNhan(int id)
        {
            var donHang = db.DonHang.Find(id);
            if (donHang == null)
                return Json(new { success = false, message = "Không tìm thấy đơn hàng" });

            // ✅ Chỉ xác nhận khi đơn hàng đang ở trạng thái "Chờ xác nhận" hoặc "Đã thanh toán"
            if (donHang.trangthai == TrangThaiDonHang.ChoXacNhan || donHang.trangthai == TrangThaiDonHang.DaThanhToan)
            {
                donHang.trangthai = TrangThaiDonHang.DangGiao;
                db.SaveChanges();
                return Json(new { success = true, message = "Đơn hàng đã chuyển sang trạng thái Đang giao." });
            }

            return Json(new { success = false, message = "Không thể xác nhận đơn hàng ở trạng thái hiện tại." });
        }
    }
}
