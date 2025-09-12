using PagedList;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using WebApplication17.Models;

namespace WebApplication17.Controllers
{
    public class SanPhamsController : Controller
    {
        private Model1 db = new Model1();

        // loại bỏ dấu
        private string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();
            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }
            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        public ActionResult Index(int? page, List<int> brands, string price, int? category)
        {
            var searchString = Request.Form["timkiem"];
            int pageSize = 16;
            int pageNumber = page ?? 1;

            // Lấy dữ liệu cơ bản từ DB, loc theo hang SP
            var sanPhamsQuery = db.SanPham.Include(s => s.HangSP)
                                         .Where(s => s.trangthai == true);

            if (category != null)
            {
                sanPhamsQuery = sanPhamsQuery.Where(sp => sp.HangSP.mahang == category);
                ViewBag.SelectedCategory = category;
            }

            if (brands != null && brands.Count > 0)
            {
                sanPhamsQuery = sanPhamsQuery.Where(sp => brands.Contains(sp.HangSP.mahang));
            }

            // Lọc theo giá và sắp xếp nếu cần
            bool hasOrder = false;
            if (!string.IsNullOrEmpty(price))
            {
                switch (price)
                {
                    case "price-1":
                        sanPhamsQuery = sanPhamsQuery.OrderBy(s => s.giaban);
                        hasOrder = true;
                        break;
                    case "price-2":
                        sanPhamsQuery = sanPhamsQuery.OrderByDescending(s => s.giaban);
                        hasOrder = true;
                        break;
                    case "price-3":
                        sanPhamsQuery = sanPhamsQuery.Where(s => s.giaban <= 10000000);
                        break;
                    case "price-4":
                        sanPhamsQuery = sanPhamsQuery.Where(s => s.giaban >= 10000000 && s.giaban <= 15000000);
                        break;
                    case "price-5":
                        sanPhamsQuery = sanPhamsQuery.Where(s => s.giaban > 15000000 && s.giaban <= 20000000);
                        break;
                    case "price-6":
                        sanPhamsQuery = sanPhamsQuery.Where(s => s.giaban > 20000000 && s.giaban <= 25000000);
                        break;
                    case "price-7":
                        sanPhamsQuery = sanPhamsQuery.Where(s => s.giaban > 25000000);
                        break;
                }
            }

            if (!hasOrder)
            {
                sanPhamsQuery = sanPhamsQuery.OrderBy(sp => sp.masp);
            }

            // Chuyển sang List để lọc searchString bằng C#
            var sanPhamsList = sanPhamsQuery.ToList();
            if (!string.IsNullOrEmpty(searchString))
            {
                var keywords = RemoveDiacritics(searchString.ToLower())
                               .Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var word in keywords)
                {
                    sanPhamsList = sanPhamsList
                        .Where(sp => RemoveDiacritics(sp.tensp.ToLower()).Contains(word))
                        .ToList();
                }
            }

            // Lấy 7 hãng nổi bật
            ViewBag.dsh = db.HangSP.Take(7).ToList();
            ViewBag.searchString = searchString;

            // Phân trang
            return View(sanPhamsList.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            SanPham sanPham = db.SanPham.Find(id);
            if (sanPham == null)
            {
                return HttpNotFound();
            }

            List<SanPham> listsp = db.SanPham.Where(s => s.mahang == sanPham.mahang).ToList();
            listsp.Remove(sanPham);
            ViewBag.ds = listsp;

            return View(sanPham);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        [HttpPost]
        public ActionResult AddToCart(
            int id,
            int soluong = 1,
            string RamOption = "8GB",
            string SsdOption = "256GB",
            string returnController = "SanPhams",
            string returnAction = "Index")
        {
            if (Session["ma"] == null)
            {
                string returnUrl = Url.Action(returnAction, returnController, new { id });
                Session["ReturnUrl"] = returnUrl;
                return Json(new { success = false, message = "NotLogin" });
            }

            var sp = db.SanPham.Find(id);
            if (sp == null || sp.trangthai != true)
                return Json(new { success = false, message = "NotFound" });

            int tonKho = sp.soluong ?? 0;
            if (soluong <= 0) soluong = 1;

            int maKh = (int)Session["ma"];
            int dangCoTrongGio = db.GioHang
                                   .Where(g => g.makhachhang == maKh && g.masp == id)
                                   .Sum(x => (int?)x.soluong) ?? 0;

            if (soluong > tonKho)
                return Json(new { success = false, message = "NotEnoughStock", available = tonKho });

            int tongMoi = dangCoTrongGio + soluong;
            if (tongMoi > tonKho)
            {
                int coTheThemToiDa = Math.Max(tonKho - dangCoTrongGio, 0);
                return Json(new { success = false, message = "NotEnoughStock", available = coTheThemToiDa });
            }

            // (giá gốc sau giảm nếu là HotDeal) + phụ phí option
            decimal giaGoc = sp.giaban ?? 0;
            decimal giaBaseSauGiam = (sp.IsHotDeal == true) ? (giaGoc * 0.9m) : giaGoc; // giảm 10% trên base
            decimal giaThemRam = RamPrices.ContainsKey(RamOption) ? RamPrices[RamOption] : 0;
            decimal giaThemSsd = SsdPrices.ContainsKey(SsdOption) ? SsdPrices[SsdOption] : 0;
            decimal giaCuoi = giaBaseSauGiam + giaThemRam + giaThemSsd;

            var cartItem = new GioHang
            {
                makhachhang = maKh,
                masp = id,
                soluong = soluong,
                giaban = giaCuoi,
                RamOption = RamOption,
                SsdOption = SsdOption,
                CauHinhThem = RamOption + " + " + SsdOption
            };

            db.GioHang.Add(cartItem);
            db.SaveChanges();

            int cartCount = db.GioHang.Where(g => g.makhachhang == maKh).Sum(g => (int?)g.soluong) ?? 0;
            Session["giohang"] = cartCount;

            return Json(new { success = true, count = cartCount });
        }

        public ActionResult MuaNgay(int id, int soluong = 1, string RamOption = "8GB", string SsdOption = "256GB")
        {
            if (Session["ma"] == null)
                return RedirectToAction("Index", "Login");

            var sp = db.SanPham.Find(id);
            if (sp == null || sp.trangthai != true)
                return HttpNotFound();

            int tonKho = sp.soluong ?? 0;
            if (soluong <= 0) soluong = 1;

            if (soluong > tonKho)
            {
                TempData["Error"] = "Số lượng vượt quá tồn kho!";
                return RedirectToAction("Details", new { id = id });
            }

            int maKh = (int)Session["ma"];
            decimal giaGoc = sp.giaban ?? 0;
            decimal giaBaseSauGiam = (sp.IsHotDeal == true) ? (giaGoc * 0.9m) : giaGoc; // giảm 10% trên base
            decimal giaThemRam = RamPrices.ContainsKey(RamOption) ? RamPrices[RamOption] : 0;
            decimal giaThemSsd = SsdPrices.ContainsKey(SsdOption) ? SsdPrices[SsdOption] : 0;
            decimal giaCuoi = giaBaseSauGiam + giaThemRam + giaThemSsd;

            var gioHang = new GioHang
            {
                makhachhang = maKh,
                masp = id,
                soluong = soluong,
                giaban = giaCuoi,
                RamOption = RamOption,
                SsdOption = SsdOption,
                CauHinhThem = RamOption + " + " + SsdOption
            };

            db.GioHang.Add(gioHang);
            db.SaveChanges();

            int cartCount = db.GioHang.Where(g => g.makhachhang == maKh).Sum(g => (int?)g.soluong) ?? 0;
            Session["giohang"] = cartCount;

            return RedirectToAction("ThanhToan", "GioHangs");
        }

        private readonly Dictionary<string, decimal> RamPrices = new Dictionary<string, decimal>
        {
            { "8GB", 0 },
            { "16GB", 500000 },
            { "32GB", 1200000 }
        };

        private readonly Dictionary<string, decimal> SsdPrices = new Dictionary<string, decimal>
        {
            { "256GB", 0 },
            { "512GB", 800000 },
            { "1TB", 1500000 }
        };
    }
}