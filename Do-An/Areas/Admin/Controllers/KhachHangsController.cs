using PagedList;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using WebApplication17.Models;

namespace WebApplication17.Areas.Admin.Controllers
{
    public class KhachHangsController : Controller
    {
        private Model1 db = new Model1();

        // GET: Admin/KhachHangs
        public ActionResult Index(int? searchMaKH, string searchHoTen, int? page)
        {
            var khachHangs = db.KhachHang.AsQueryable();

            if (searchMaKH.HasValue)
            {
                khachHangs = khachHangs.Where(k => k.makhachhang == searchMaKH.Value);
            }

            if (!string.IsNullOrEmpty(searchHoTen))
            {
                khachHangs = khachHangs.Where(k => k.hoten.Contains(searchHoTen));
            }

            // số trang hiện tại (mặc định 1)
            int pageNumber = page ?? 1;
            int pageSize = 10;

            ViewBag.SearchMaKH = searchMaKH;
            ViewBag.SearchHoTen = searchHoTen;

            return View(khachHangs.OrderBy(k => k.makhachhang).ToPagedList(pageNumber, pageSize));
        }



        // GET: Admin/KhachHangs/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            KhachHang khachHang = db.KhachHang.Find(id);
            if (khachHang == null)
            {
                return HttpNotFound();
            }
            return View(khachHang);
        }

        // GET: Admin/KhachHangs/Create
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "makhachhang,hoten,diachi,dienthoai,email,matkhau,trangthai,chucvu,ngaydangky")] KhachHang khachHang)
        {
            if (ModelState.IsValid)
            {
                db.KhachHang.Add(khachHang);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(khachHang);
        }

        // GET: Admin/KhachHangs/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            KhachHang khachHang = db.KhachHang.Find(id);
            if (khachHang == null)
            {
                return HttpNotFound();
            }
            List<SelectListItem> trangThaiList = new List<SelectListItem>
            {
                new SelectListItem { Value = "true", Text = "Khóa" },
                new SelectListItem { Value = "false", Text = "Hoạt động" }
            };
            List<SelectListItem> chucvuList = new List<SelectListItem>
            {
                new SelectListItem { Value = "true", Text = "Nhân viên" },
                new SelectListItem { Value = "false", Text = "Khách hàng" }
            };
            ViewBag.TrangThaiList = trangThaiList;
            ViewBag.chucvuList = chucvuList;
            return View(khachHang);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(KhachHang model)
        {
            if (ModelState.IsValid)
            {
                var khachHang = db.KhachHang.Find(model.makhachhang);
                if (khachHang == null) return HttpNotFound();

                
                db.Entry(khachHang).CurrentValues.SetValues(model);

               
                db.Entry(khachHang).Property(x => x.ngaydangky).IsModified = false;

                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(model);
        }

        public ActionResult DeleteConfirmed(int? id)
        {
            KhachHang khachHang = db.KhachHang.Find(id);
            db.DonHangChiTiet.RemoveRange(khachHang.DonHang.SelectMany(dh => dh.DonHangChiTiet));
            db.DonHang.RemoveRange(khachHang.DonHang);
            db.GioHang.RemoveRange(khachHang.GioHang);
            db.KhachHang.Remove(khachHang);
            db.SaveChanges();
            //return RedirectToAction("Index");
            return Json(new { success = true });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }


    }
}
