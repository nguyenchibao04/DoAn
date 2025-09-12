using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using WebApplication17.Models;

namespace WebApplication17.Areas.Admin.Controllers
{
    public class HomeController : Controller
    {
        private Model1 db = new Model1();

        // GET: Admin/Home
        public ActionResult Index(string fill)
        {
            if (Session["hotenadmin"] == null)
            {
                return RedirectToAction("Index", "Home", new { area = "" });
            }

            DateTime ngayhomnay = DateTime.Now;

            // Danh sách đơn hôm nay (chỉ tính đơn đã giao)
            var dsdh = db.DonHang.Where(d => d.ngaydat.Day == ngayhomnay.Day &&
                                d.ngaydat.Month == ngayhomnay.Month &&
                                d.ngaydat.Year == ngayhomnay.Year &&
                                d.trangthai == TrangThaiDonHang.DaGiao).ToList();

            var dskhh = db.KhachHang.Where(k => k.ngaydangky.HasValue &&
                                k.ngaydangky.Value.Day == ngayhomnay.Day &&
                                k.ngaydangky.Value.Month == ngayhomnay.Month &&
                                k.ngaydangky.Value.Year == ngayhomnay.Year).ToList();

            decimal doanhthu = dsdh.Sum(d => d.tongtien ?? 0);

            ViewBag.donhang = dsdh.Count;
            ViewBag.khachhang = dskhh.Count;
            ViewBag.doanhthu = doanhthu;

            // Thống kê doanh thu theo tháng (chỉ đơn đã giao)
            List<decimal> thongkedoanhthu = new List<decimal>();
            for (int month = 1; month <= 12; month++)
            {
                decimal dtThang = db.DonHang
                    .Where(d => d.ngaydat.Month == month &&
                                d.ngaydat.Year == ngayhomnay.Year &&
                                 d.trangthai == TrangThaiDonHang.DaGiao)
                    .Sum(d => (decimal?)d.tongtien) ?? 0;

                thongkedoanhthu.Add(dtThang);
            }
            ViewBag.doanhthuthang = thongkedoanhthu;

            // Nếu có filter
            if (!string.IsNullOrEmpty(fill))
            {
                int orderCount = 0;
                decimal doanhthuFilter = 0;
                int userCount = 0;

                switch (fill)
                {
                    case "1": // Hôm nay
                        var ds1 = db.DonHang.Where(d => d.ngaydat.Day == ngayhomnay.Day &&
                                                        d.ngaydat.Month == ngayhomnay.Month &&
                                                        d.ngaydat.Year == ngayhomnay.Year &&
                                                        d.trangthai == TrangThaiDonHang.DaGiao).ToList();
                        orderCount = ds1.Count;
                        doanhthuFilter = ds1.Sum(d => d.tongtien ?? 0);
                        userCount = dskhh.Count;
                        break;

                    case "2": // Tuần này
                        DateTime batDau = ngayhomnay.AddDays(-(int)ngayhomnay.DayOfWeek);
                        DateTime ketThuc = batDau.AddDays(6);
                        var ds2 = db.DonHang.Where(d => d.ngaydat >= batDau &&
                                                        d.ngaydat <= ketThuc &&
                                                        d.trangthai == TrangThaiDonHang.DaGiao).ToList();
                        orderCount = ds2.Count;
                        doanhthuFilter = ds2.Sum(d => d.tongtien ?? 0);
                        userCount = db.KhachHang.Count(k => k.ngaydangky.HasValue &&
                                                            k.ngaydangky.Value >= batDau &&
                                                            k.ngaydangky.Value <= ketThuc);
                        break;

                    case "3": // Tháng này
                        var ds3 = db.DonHang.Where(d => d.ngaydat.Month == ngayhomnay.Month &&
                                                        d.ngaydat.Year == ngayhomnay.Year &&
                                                         d.trangthai == TrangThaiDonHang.DaGiao).ToList();
                        orderCount = ds3.Count;
                        doanhthuFilter = ds3.Sum(d => d.tongtien ?? 0);
                        userCount = db.KhachHang.Count(k => k.ngaydangky.HasValue &&
                                                            k.ngaydangky.Value.Month == ngayhomnay.Month &&
                                                            k.ngaydangky.Value.Year == ngayhomnay.Year);
                        break;

                    case "4": // Tất cả
                        var ds4 = db.DonHang.Where(d => d.trangthai == TrangThaiDonHang.DaGiao).ToList();
                        orderCount = ds4.Count;
                        doanhthuFilter = ds4.Sum(d => d.tongtien ?? 0);
                        userCount = db.KhachHang.Count();
                        break;
                }

                return Json(new { success = true, dh = orderCount, dt = doanhthuFilter, kh = userCount }, JsonRequestBehavior.AllowGet);
            }

            return View();
        }
        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();

            return RedirectToAction("Index", "Home"); 
        }
    }
}
