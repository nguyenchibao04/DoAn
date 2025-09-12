using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using WebApplication17.Models;

namespace WebApplication17.Areas.Admin.Controllers
{
    public class DonHangsController : Controller
    {
        private Model1 db = new Model1();

        // GET: Admin/DonHangs
        public ActionResult Index(int? searchMaDH, string searchSDT, int? page)
        {
            var donhangs = db.DonHang.Include(d => d.KhachHang).AsQueryable();

            // lọc theo mã đơn hàng
            if (searchMaDH.HasValue)
            {
                donhangs = donhangs.Where(d => d.madonhang == searchMaDH.Value);
            }

            // lọc theo số điện thoại
            if (!string.IsNullOrEmpty(searchSDT))
            {
                donhangs = donhangs.Where(d => d.dienthoai.Contains(searchSDT));
            }

            // sắp xếp giảm dần theo mã đơn hàng
            donhangs = donhangs.OrderByDescending(d => d.madonhang);

            // phân trang
            int pageSize = 15;
            int pageNumber = (page ?? 1);

            // giữ lại giá trị search khi render ra View
            ViewBag.SearchMaDH = searchMaDH;
            ViewBag.SearchSDT = searchSDT;

            return View(donhangs.ToPagedList(pageNumber, pageSize));
        }


        // GET: Admin/DonHangs/Details/5
        public ActionResult Details(int? id)
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

            
            // lưu phí vận chuyển trong bảng DonHang
            decimal phiVanChuyen = donHang.phivanchuyen ?? 0;
            ViewBag.PhiVanChuyen = phiVanChuyen;

            
            return View(donHang);
        }

        // GET: Admin/DonHangs/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            DonHang donHang = db.DonHang.Find(id);
            if (donHang == null) return HttpNotFound();

            ViewBag.makhachhang = new SelectList(db.KhachHang, "makhachhang", "hoten", donHang.makhachhang);

            // Dropdown enum trạng thái
            ViewBag.TrangThaiList = Enum.GetValues(typeof(TrangThaiDonHang))
                                        .Cast<TrangThaiDonHang>()
                                        .Select(tt => new SelectListItem
                                        {
                                            Text = tt.ToString(),
                                            Value = ((int)tt).ToString(),
                                            Selected = donHang.trangthai == tt
                                        }).ToList();

            return View(donHang);
        }

        // POST: Admin/DonHangs/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "madonhang,makhachhang,dienthoai,diachi,ngaydat,ngaynhan,thanhtoan,soluongmua,phivanchuyen,tongtien,trangthai")] DonHang donHang)
        {
            if (ModelState.IsValid)
            {
                db.Entry(donHang).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.makhachhang = new SelectList(db.KhachHang, "makhachhang", "hoten", donHang.makhachhang);

            // Dropdown enum trạng thái
            ViewBag.TrangThaiList = Enum.GetValues(typeof(TrangThaiDonHang))
                                        .Cast<TrangThaiDonHang>()
                                        .Select(tt => new SelectListItem
                                        {
                                            Text = tt.ToString(),
                                            Value = ((int)tt).ToString(),
                                            Selected = donHang.trangthai == tt
                                        }).ToList();

            return View(donHang);
        }

        [HttpPost]
        public ActionResult UpdateOrder(int id, string status)
        {
            var donHang = db.DonHang.Find(id);
            if (donHang == null) return Json(new { success = false });

            switch (status)
            {
                case "DangGiao":
                    donHang.trangthai = TrangThaiDonHang.DangGiao;
                    break;
                case "DaGiao":
                    donHang.trangthai = TrangThaiDonHang.DaGiao;
                    break;
            }

            db.SaveChanges();
            return Json(new { success = true });
        }
        [HttpPost]
        public ActionResult CancelOrder(int id)
        {
            var donHang = db.DonHang.Find(id);
            if (donHang == null) return Json(new { success = false });

            // Chỉ hủy nếu chưa giao hoặc đang giao
            if (donHang.trangthai == TrangThaiDonHang.ChoXacNhan || donHang.trangthai == TrangThaiDonHang.DaThanhToan)
            {
                donHang.trangthai = TrangThaiDonHang.DaHuy;

                // Lấy danh sách chi tiết đơn hàng
                var chitiet = db.DonHangChiTiet.Where(ct => ct.madonhang == id).ToList();

                foreach (var ct in chitiet)
                {
                    var sp = db.SanPham.Find(ct.masp);
                    if (sp != null)
                    {
                        sp.soluong += ct.soluong; // cộng lại vào kho
                        db.Entry(sp).State = EntityState.Modified;
                    }
                }

                db.SaveChanges();
                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Không thể hủy đơn này!" });
        }

        [HttpPost]
        public JsonResult DeleteMultipleOrders(int[] ids)
        {
            if (ids == null || ids.Length == 0)
                return Json(new { success = false, message = "Không có đơn hàng nào được chọn." });

            foreach (var id in ids)
            {
                var donHang = db.DonHang.Find(id);
                if (donHang == null) continue;

                if (donHang.trangthai != TrangThaiDonHang.ChoXacNhan &&
                   donHang.trangthai != TrangThaiDonHang.DaHuy) continue;

                var chiTiets = db.DonHangChiTiet.Where(x => x.madonhang == id).ToList();
                db.DonHangChiTiet.RemoveRange(chiTiets);

                db.DonHang.Remove(donHang);
            }

            db.SaveChanges();
            return Json(new { success = true });
        }




    }
}
