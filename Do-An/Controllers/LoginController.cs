using WebApplication17.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace WebApplication17.Controllers
{
    public class LoginController : Controller
    {
        Model1 db = new Model1();

        // GET: Login
        public ActionResult Index()
        {
            return View();
        }

        // Xử lý đăng nhập
        [HttpPost]
        public ActionResult DangNhap(string email, string password)
        {
            var user = db.KhachHang.FirstOrDefault(s => s.email == email && s.matkhau == password);

            if (user == null)
            {
                return Json(new { success = false, message = "Sai email hoặc mật khẩu" });
            }

            if (user.chucvu == true) // admin
            {
                Session["maadmin"] = user.makhachhang;
                Session["hotenadmin"] = user.hoten;
                Session.Timeout = 100;

                return Json(new
                {
                    success = true,
                    redirectUrl = Url.Action("Index", "Home", new { area = "Admin" }),
                    message = "Đăng nhập admin thành công"
                });
            }
            if (user.trangthai == true)
            {
                return Json(new { success = false, message = "Tài khoản đã bị khóa, không thể đăng nhập" });

            }

            // user thường
            Session["ma"] = user.makhachhang; 
            Session["hoten"] = user.hoten;
            Session["giohang"] = db.GioHang.Where(s => s.makhachhang == user.makhachhang).Count();
            Session.Timeout = 100;

            return Json(new
            {
                success = true,
                redirectUrl = Url.Action("Index", "Home", new { area = "" }),
                message = "Đăng nhập thành công"
            });
        }

        

        // Xử lý đăng ký
        [HttpPost]
        public ActionResult DangKy(string hoten, string email, string pass, string repass)
        {
            if (pass != repass)
            {
                return Json(new { success = false, message = "Mật khẩu nhập lại không khớp" });
            }

            var exists = db.KhachHang.Any(s => s.email == email);
            if (exists)
            {
                return Json(new { success = false, message = "Email đã tồn tại" });
            }

           

            KhachHang kh = new KhachHang
            {
                hoten = hoten,
                email = email,
                matkhau = pass,
                chucvu=false,
                trangthai=false,
                ngaydangky = DateTime.Now
            };

            
            db.KhachHang.Add(kh);
            db.SaveChanges();

            return Json(new { success = true, message = "Đăng ký thành công" });
        }

        // Đăng xuất
        public ActionResult SignOut()
        {
            Session["ma"] = null;
            Session["hoten"] = null;
            Session["giohang"] = null;
            Session.Abandon();
            return RedirectToAction("Index", "Home");
        }

        // Đăng ký (cách 2)
        public ActionResult Register(string hoten, string email, string pass, string repass)
        {
            if (pass == repass)
            {
                KhachHang kh = new KhachHang
                {
                    hoten = hoten,
                    email = email,
                    matkhau = pass
                };

                db.KhachHang.Add(kh);
                db.SaveChanges();

                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false });
            }
        }
        [HttpPost]
        public ActionResult QuenMatKhau(string email)
        {
            var user = db.KhachHang.FirstOrDefault(u => u.email == email);
            if (user == null)
            {
                return Json(new { success = false, message = "Email không tồn tại trong hệ thống!" });
            }

            // Tạo mật khẩu mới ngẫu nhiên
            string newPassword = Guid.NewGuid().ToString().Substring(0, 8);

            // Cập nhật mật khẩu vào DB
            user.matkhau = newPassword;
            db.SaveChanges();

            // Tạo nội dung mail
            string content = "<h3>Xin chào " + user.hoten + "</h3>";
            content += "<p>Bạn vừa yêu cầu đặt lại mật khẩu.</p>";
            content += "<p>Mật khẩu mới của bạn là: <b>" + newPassword + "</b></p>";
            content += "<p>Vui lòng đăng nhập và đổi mật khẩu ngay sau khi vào hệ thống.</p>";

            try
            {
                // Gửi mail (MailHelper)
                WebApplication17.Common.MailHelper.sendEmail(
                    "TTGShop",
                    "Mật khẩu mới",
                    content,
                    user.email
                );

                return Json(new { success = true, message = "Mật khẩu mới đã được gửi về email của bạn!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi gửi email: " + ex.Message });
            }
        }

        [HttpPost]
        public JsonResult DoiMatKhau(string email, string oldpass, string newpass, string renewpass)
        {
            var user = db.KhachHang.FirstOrDefault(x => x.email == email && x.matkhau == oldpass);
            if (user == null)
            {
                return Json(new { success = false, message = "Email hoặc mật khẩu hiện tại không đúng!" });
            }

            if (newpass != renewpass)
            {
                return Json(new { success = false, message = "Mật khẩu mới không khớp!" });
            }

            user.matkhau = newpass;
            db.SaveChanges();

            return Json(new { success = true, message = "Đổi mật khẩu thành công!" });
        }


    }
}
