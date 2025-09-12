using System.Linq;
using System.Web.Mvc;
using WebApplication17.Models;

namespace WebApplication17.Controllers
{
    public class KhachHangsController : Controller
    {
        private Model1 db = new Model1();

        // Trang thông tin cá nhân
        public ActionResult UserInfor()
        {
            if (Session["ma"] == null) // chưa đăng nhập
            {
                return RedirectToAction("Index", "Login");
            }

            int maKH = (int)Session["ma"];
            var khach = db.KhachHang.FirstOrDefault(x => x.makhachhang == maKH);

            if (khach == null)
            {
                return HttpNotFound();
            }

            return View(khach);
        }

        // Sửa thông tin cá nhân
        [HttpPost]
        public ActionResult UpdateInfor(int id, string hoten, string email, string dienthoai, string tinh, string huyen, string xa, string diachi)
        {
            var khach = db.KhachHang.Find(id);
            if (khach == null)
                return Json(new { success = false, message = "Không tìm thấy khách hàng!" });

            khach.hoten = hoten;
            khach.email = email;
            khach.dienthoai = dienthoai;
            khach.tinh = tinh;
            khach.huyen = huyen;
            khach.xa = xa;
            
            khach.diachi = diachi;

            db.SaveChanges();
            Session["hoten"] = hoten;

            return Json(new { success = true, message = "Cập nhật thông tin thành công!" });
        }
    }
}
