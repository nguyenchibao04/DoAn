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
    public class HangSPsController : Controller
    {
        private Model1 db = new Model1();

        // GET: Admin/HangSPs
        public ActionResult Index(int? page)
        {
            var hangSanPhams = db.HangSP.ToList();
            int pageSize = 10; // Số lượng trên mỗi trang
            int pageNumber = (page ?? 1); // mặc định là trang 1

            return View(hangSanPhams.ToPagedList(pageNumber, pageSize));
        }

        // GET: Admin/HangSPs/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/HangSPs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "mahang,tenhang,trangthai")] HangSP hangSP)
        {
            if (ModelState.IsValid)
            {
                hangSP.trangthai = true;
                db.HangSP.Add(hangSP);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(hangSP);
        }

        // GET: Admin/HangSPs/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            HangSP hangSP = db.HangSP.Find(id);
            if (hangSP == null)
            {
                return HttpNotFound();
            }
            return View(hangSP);
        }

        // POST: Admin/HangSPs/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "mahang,tenhang,trangthai")] HangSP hangSP)
        {
            if (ModelState.IsValid)
            {
                db.Entry(hangSP).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(hangSP);
        }

        public ActionResult DeleteConfirmed(int id)
        {
            List<SanPham> phamList = db.SanPham.Where(sp => sp.mahang == id).ToList();
            foreach (var pham in phamList)
            {
                db.SanPham.Remove(pham);
            }
            HangSP hangSP = db.HangSP.Find(id);
            db.HangSP.Remove(hangSP);
            db.SaveChanges();
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
