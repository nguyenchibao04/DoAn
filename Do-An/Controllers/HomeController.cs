using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Services.Description;
using System.Xml.Linq;
using WebApplication17.Common;
using WebApplication17.Models;


namespace WebApplication17.Controllers
{
    public class HomeController : Controller
    {
        private Model1 db = new Model1();
        public ActionResult Index()
        {          
            List<SanPham> ds = new List<SanPham>();

            // Lấy Hot Deals theo cột IsHotDeal
            List<SanPham> ds_sale = db.SanPham
                                      .Where(s => s.trangthai == true && s.IsHotDeal == true)
                                      .OrderByDescending(s => s.masp)
                                      .Take(50)
                                      .ToList();
           
            List<SanPham> ds_new = db.SanPham
                                      .Where(s => s.trangthai == true)
                                      .OrderByDescending(s => s.masp)
                                      .Take(15)
                                      .ToList();

            List<SanPham> ds_pc = db.SanPham
                                      .Where(s => s.trangthai == true && s.mahang == 17)
                                      .OrderByDescending(s => s.masp)
                                      .Take(10)
                                      .ToList();
            List<SanPham> ds_pc1 = db.SanPham
                                      .Where(s => s.trangthai == true && s.mahang == 16)
                                      .OrderByDescending(s => s.masp)
                                      .Take(10)
                                      .ToList();

            List<HangSP> ds_hang = db.HangSP.ToList();

            List<TinTuc> ds_tintuc = db.TinTuc.ToList();
            ViewBag.dstintuc = ds_tintuc;

            ViewBag.sale = ds_sale;   
            ViewBag.dsnew = ds_new;   
            ViewBag.dspc = ds_pc;
            ViewBag.dspc1 = ds_pc1;

            Session["dshang"] = ds_hang;
            Session.Timeout = 100;
            ViewBag.dsh = ds_hang;

            return View();
        }

        public ActionResult DetailProduct()
        {
            return View();
        }


        public ActionResult LienHe()
        {         
            return View();
        }

        [HttpPost]
        public JsonResult GuiLienHe(string name, string email, string phone, string subject, string message)
        {
            string content = "<h3>Khách hàng liên hệ</h3>";
            content += "<p><b>Họ tên:</b> " + name + "</p>";
            content += "<p><b>Email:</b> " + email + "</p>";
            content += "<p><b>Số điện thoại:</b> " + phone + "</p>";
            content += "<p><b>Chủ đề:</b> " + subject + "</p>";
            content += "<p><b>Nội dung:</b><br/>" + message + "</p>";

            try
            {
                // Gửi mail về cho shop
                WebApplication17.Common.MailHelper.sendEmail(
                    "TTGShop",                
                    "Liên hệ từ khách hàng",  
                    content,                  
                    "baowibu01@gmail.com"     
                );

                return Json(new { success = true, message = "Gửi liên hệ thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi gửi liên hệ: " + ex.Message });
            }
        }
        public ActionResult TinTuc()
        {
            var dstintuc = db.TinTuc
                   .Where(t => t.trangthai == true)
                   .OrderByDescending(t => t.ngaytao)
                   .ToList();

            return View(dstintuc);
        }
        public ActionResult ChiTietTinTuc(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var tintuc = db.TinTuc.Find(id);
            if (tintuc == null)
            {
                return HttpNotFound();
            }

            return View(tintuc);
        }

        public ActionResult PhuongThucThanhToan()
        {
            return View();
        }

        public ActionResult ChinhSachDoiTra()
        {
            return View();  
        }
        public ActionResult VanChuyen()
        {
            return View();
        }
        public ActionResult DieuKhoan()
        {
            return View();
        }
        public ActionResult BaoMat()
        {
            return View();
        }
        public ActionResult TuVanPC()
        {
            return View();
        }

    }
}