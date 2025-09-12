using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebApplication17.Models;
using WebApplication17.PayMethod;

namespace WebApplication17.Controllers
{
    public class GioHangsController : Controller
    {
        private Model1 db = new Model1();

        // GET: GioHangs
        public ActionResult Index()
        {
            var gioHangs = db.GioHang.Include(g => g.KhachHang)
                                     .Include(g => g.SanPham);
            return View(gioHangs.ToList());
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public ActionResult RemoveCart(int magiohang)
        {
            GioHang gh = db.GioHang.Where(s => s.magiohang == magiohang).FirstOrDefault();
            db.GioHang.Remove(gh);
            db.SaveChanges();
            Session["giohang"] = (int)Session["giohang"] - 1;
            return Json(new { success = true });
        }

        public ActionResult UpdateCart([Bind(Include = "magiohang,masp,makhachhang,soluong")] GioHang gioHang)
        {
            if (ModelState.IsValid)
            {
                var gh = db.GioHang.Find(gioHang.magiohang);
                if (gh == null)
                    return Json(new { success = false, message = "NotFound" });

                var sp = db.SanPham.Find(gh.masp);
                if (sp == null || sp.trangthai != true)
                    return Json(new { success = false, message = "NotFound" });

                int tonKho = sp.soluong ?? 0;
                int soLuongMoi = gioHang.soluong <= 0 ? 1 : gioHang.soluong;

                // ✅ Chặn vượt kho
                if (soLuongMoi > tonKho)
                {
                    return Json(new { success = false, message = "NotEnoughStock", available = tonKho });
                }

                // Update hợp lệ
                gh.soluong = soLuongMoi;
                db.SaveChanges();
                return Json(new { success = true });
            }

            return RedirectToAction("Index");
        }

        public ActionResult ThanhToan()
        {
            var gioHangs = db.GioHang.Include(g => g.KhachHang)
                                     .Include(g => g.SanPham);
            KhachHang taikhoan = db.KhachHang.Find((int)Session["ma"]);
            ViewBag.taikhoan = taikhoan;

            // Tính tổng tiền và phí vận chuyển
            decimal TienHang = 0;
            decimal PhiVanChuyen = 0;

            foreach (var item in gioHangs.Where(g => g.makhachhang == taikhoan.makhachhang))
            {
                //  Lấy giá trong giỏ hàng (đã cộng option)
                TienHang += (decimal)item.giaban * item.soluong;
                PhiVanChuyen += 30000 * item.soluong; // mỗi máy tính 30k
            }

            ViewBag.TienHang = TienHang;
            ViewBag.PhiVanChuyen = PhiVanChuyen;

            return View(gioHangs.ToList());
        }

        public ActionResult DatHang()
        {
            KhachHang taikhoan = db.KhachHang.Find((int)Session["ma"]);
            List<GioHang> dssp = db.GioHang
                                    .Include(g => g.SanPham)
                                    .Where(s => s.makhachhang == taikhoan.makhachhang)
                                    .ToList();

            decimal tongTienSanPham = 0;
            decimal phiVanChuyen = 0;
            var strSanPham = "";
            string tinh = Request.Form["tinh"];
            string phuongThucTT = Request.Form["thanhtoan"];

            // Tính tổng tiền sản phẩm và phí vận chuyển
            foreach (var item in dssp)
            {
                // Lấy giá trong giỏ hàng (đã cộng option)
                tongTienSanPham += (decimal)(item.giaban * item.soluong);

                // Tính phí vận chuyển dựa trên tỉnh/thành phố
                switch (tinh)
                {
                    case "Hà Nội": phiVanChuyen += 30000 * item.soluong; break;
                    case "Hồ Chí Minh": phiVanChuyen += 35000 * item.soluong; break;
                    default: phiVanChuyen += 40000 * item.soluong; break;
                }

                // Chuỗi HTML sản phẩm cho email
                strSanPham += "<tr>";
                strSanPham += $"<td>{item.SanPham.tensp}<br/><small>{item.CauHinhThem}</small></td>";
                strSanPham += $"<td>{item.soluong}</td>";
                strSanPham += $"<td>{item.giaban:#,##0}</td>";
                
                strSanPham += "</tr>";
            }

            // Tạo đơn hàng
            int soLuongMua = dssp.Sum(x => x.soluong);
            DonHang donHang = new DonHang
            {
                makhachhang = taikhoan.makhachhang,
                diachi = $"{Request.Form["diachi"]}, {Request.Form["phuongxa"]}, {Request.Form["huyen"]}, {Request.Form["tinh"]}",
                tongtien = tongTienSanPham + phiVanChuyen,
                phivanchuyen = phiVanChuyen,
                trangthai = TrangThaiDonHang.ChoXacNhan,
                thanhtoan = (phuongThucTT == "banking") ? "Chuyển khoản ngân hàng" : "Thanh toán khi nhận hàng",
                ngaydat = DateTime.Now,
                ngaynhan = DateTime.Now.AddDays(3),
                dienthoai = Request.Form["dienthoai"],
                soluongmua = soLuongMua
            };

            // Cập nhật thông tin khách hàng
            taikhoan.dienthoai = donHang.dienthoai;
            taikhoan.diachi = Request.Form["diachi"];
            taikhoan.tinh = Request.Form["tinh"];
            taikhoan.huyen = Request.Form["huyen"];
            taikhoan.xa = Request.Form["phuongxa"];
            taikhoan.thon = Request.Form["diachi"];

            db.DonHang.Add(donHang);
           

            foreach (var item in dssp)
            {
                DonHangChiTiet dhct = new DonHangChiTiet
                {
                    madonhang = donHang.madonhang,
                    masp = item.masp,
                    soluong = item.soluong,
                    // ✅ Lấy giá trong giỏ hàng (đã cộng option)
                    gia = item.giaban,
                    CauHinhThem = item.CauHinhThem,   // <-- chỗ này
                    tongtien = (item.giaban * item.soluong) + phiVanChuyen
                };

                item.SanPham.soluong -= item.soluong;
                db.DonHangChiTiet.Add(dhct);
                db.GioHang.Remove(item);
            }

            db.SaveChanges();
            Session["giohang"] = 0;

           
            // Gửi email như cũ
            string contentCustomer = System.IO.File.ReadAllText(Server.MapPath("~/Content/Template/send2.html"));
            contentCustomer = contentCustomer.Replace("{{madon}}", donHang.madonhang.ToString());
            contentCustomer = contentCustomer.Replace("{{sanpham}}", strSanPham);
            contentCustomer = contentCustomer.Replace("{{hoten}}", taikhoan.hoten);
            contentCustomer = contentCustomer.Replace("{{ngaydat}}", donHang.ngaydat.ToString());
            contentCustomer = contentCustomer.Replace("{{tongtien}}", (donHang.tongtien ?? 0).ToString("#,##0"));
            contentCustomer = contentCustomer.Replace("{{phivanchuyen}}", (donHang.phivanchuyen ?? 0).ToString("#,##0"));
            contentCustomer = contentCustomer.Replace("{{diachi}}", donHang.diachi);
            contentCustomer = contentCustomer.Replace("{{dienthoai}}", taikhoan.dienthoai);
            contentCustomer = contentCustomer.Replace("{{email}}", taikhoan.email);

            WebApplication17.Common.MailHelper.sendEmail(
                "TTGShop",
                "Đơn Hàng #" + donHang.madonhang,
                contentCustomer,
                taikhoan.email
            );
            if (phuongThucTT == "banking")
            {
                return RedirectToAction("ThanhToanVNPay", "GioHangs", new { id = donHang.madonhang });
            }

            return RedirectToAction("Index", "DonHangs");
        }

        


        [HttpPost]
        public JsonResult CapNhatPhiVanChuyen(string tinh)
        {
            int maKh = (int)Session["ma"];
            var gioHang = db.GioHang.Include(g => g.SanPham)
                                    .Where(g => g.makhachhang == maKh)
                                    .ToList();

            decimal tongTien = 0;
            foreach (var item in gioHang)
            {
                // Lấy giá trong giỏ hàng (đã cộng option)
                tongTien += (decimal)(item.giaban * item.soluong);
            }

            decimal phiVanChuyen = 30000;
            if (tinh == "Hà Nội") phiVanChuyen = 30000;
            else if (tinh == "Hồ Chí Minh") phiVanChuyen = 35000;
            else phiVanChuyen = 40000;

            decimal tongTienThanhToan = tongTien + phiVanChuyen;

            return Json(new { phiVanChuyen = phiVanChuyen.ToString("N0"), tongTien = tongTienThanhToan.ToString("N0") });
        }

            

        

        public ActionResult ThanhToanVNPay(int id)
        {
            var donHang = db.DonHang.Find(id);
            if (donHang == null) return HttpNotFound();

            string vnp_Returnurl = ConfigurationManager.AppSettings["vnp_Returnurl"];
            string vnp_Url = ConfigurationManager.AppSettings["vnp_Url"];
            string vnp_TmnCode = ConfigurationManager.AppSettings["vnp_TmnCode"];
            string vnp_HashSecret = ConfigurationManager.AppSettings["vnp_HashSecret"];

            var vnpay = new VnPayLibrary();

            vnpay.AddRequestData("vnp_Version", VnPayLibrary.VERSION);
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
            vnpay.AddRequestData("vnp_Amount", ((donHang.tongtien ?? 0) * 100).ToString());
            vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", Utils.GetIpAddress());
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", "Thanh toan don hang #" + donHang.madonhang);
            vnpay.AddRequestData("vnp_OrderType", "other");
            vnpay.AddRequestData("vnp_ReturnUrl", vnp_Returnurl);
            vnpay.AddRequestData("vnp_TxnRef", donHang.madonhang.ToString());

            string paymentUrl = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);

            return Redirect(paymentUrl);
        }
        public ActionResult ReturnVNPay()
        {
            var vnpayData = Request.QueryString;
            VnPayLibrary vnpay = new VnPayLibrary();

            foreach (string s in vnpayData)
            {
                if (!string.IsNullOrEmpty(s) && s.StartsWith("vnp_"))
                {
                    vnpay.AddResponseData(s, vnpayData[s]);
                }
            }

            string hashSecret = ConfigurationManager.AppSettings["vnp_HashSecret"];
            string vnp_SecureHash = vnpayData["vnp_SecureHash"];
            bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, hashSecret);

            if (checkSignature)
            {
                string responseCode = vnpay.GetResponseData("vnp_ResponseCode");
                string orderId = vnpay.GetResponseData("vnp_TxnRef");

                if (responseCode == "00")
                {
                    // ✅ Thanh toán thành công
                    var donhang = db.DonHang.Find(int.Parse(orderId));
                    if (donhang != null)
                    {
                        donhang.trangthai = TrangThaiDonHang.DaThanhToan;
                        db.SaveChanges();
                    }

                    ViewBag.Message = "Thanh toán thành công! Đơn hàng #" + orderId + " đã được duyệt.";
                }
                else
                {
                    ViewBag.Message = "Thanh toán thất bại. Mã lỗi: " + responseCode;
                }
            }
            else
            {
                ViewBag.Message = "Sai chữ ký VNPay, không xác thực được giao dịch!";
            }

            return View();
        }




    }
}
