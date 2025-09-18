using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebApplication17.Models;

namespace WebApplication17.Areas.Admin.Controllers
{
    public class TinTucsController : Controller
    {
        private Model1 db = new Model1();

        // GET: Admin/TinTucs
        public ActionResult Index()
        {
            var tinTucs = db.TinTuc.ToList();
            tinTucs = tinTucs.OrderByDescending(n => n.matintuc).ToList();
            return View(tinTucs);
        }

        // GET: Admin/TinTucs/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TinTuc tinTuc = db.TinTuc.Find(id);
            if (tinTuc == null)
            {
                return HttpNotFound();
            }
            return View(tinTuc);
        }

        // GET: Admin/TinTucs/Create
        public ActionResult Create()
        {
            List<SelectListItem> trangThaiList = new List<SelectListItem>
            {
                new SelectListItem { Value = "true", Text = "Hiển thị" },
                new SelectListItem { Value = "false", Text = "Ẩn" }
            };
            ViewBag.TrangThaiList = trangThaiList;
            return View();

        }

        // POST: Admin/TinTucs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]  // Cho phép HTML
        public ActionResult Create([Bind(Include = "matintuc,tieude,hinhanh,ngaytao,gioithieu,trangthai,noidung")] TinTuc tinTuc)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    tinTuc.hinhanh = "";
                    var file = Request.Files["ImageUpload"];
                    if (file != null && file.ContentLength > 0)
                    {
                        string filename = Path.GetFileName(file.FileName);
                        string filepath = Server.MapPath("~/wwwroot/tintuc/" + filename);
                        file.SaveAs(filepath);
                        tinTuc.hinhanh = filename;
                    }
                    tinTuc.ngaytao = DateTime.Now;
                    db.TinTuc.Add(tinTuc);
                    db.SaveChanges();
                }
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                return View(tinTuc);
            }
        }

        // GET: Admin/TinTucs/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TinTuc tinTuc = db.TinTuc.Find(id);
            if (tinTuc == null)
            {
                return HttpNotFound();
            }
            List<SelectListItem> trangThaiList = new List<SelectListItem>
            {
                new SelectListItem { Value = "true", Text = "Hiển thị" },
                new SelectListItem { Value = "false", Text = "Ẩn" }
            };
            ViewBag.TrangThaiList = trangThaiList;
            return View(tinTuc);
        }

        // POST: Admin/TinTucs/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]  
        
        public ActionResult Edit([Bind(Include = "matintuc,tieude,hinhanh,gioithieu,trangthai,noidung")] TinTuc tinTuc)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var existingTinTuc = db.TinTuc.Find(tinTuc.matintuc);
                    if (existingTinTuc == null)
                    {
                        return HttpNotFound();
                    }

                   
                    tinTuc.ngaytao = existingTinTuc.ngaytao;

                    // Xử lý upload ảnh
                    var file = Request.Files["ImageUpload"];
                    if (file != null && file.ContentLength > 0)
                    {
                        string filename = Path.GetFileName(file.FileName);
                        string filepath = Server.MapPath("~/wwwroot/tintuc/" + filename);
                        file.SaveAs(filepath);
                        tinTuc.hinhanh = filename;
                    }
                    else
                    {
                        // Nếu không upload ảnh mới thì giữ ảnh cũ
                        tinTuc.hinhanh = existingTinTuc.hinhanh;
                    }

                    db.Entry(existingTinTuc).CurrentValues.SetValues(tinTuc);
                    db.SaveChanges();

                    return RedirectToAction("Index");
                }
            }
            catch (Exception)
            {
                return View(tinTuc);
            }

            return View(tinTuc);
        }


        // GET: Admin/TinTucs/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TinTuc tinTuc = db.TinTuc.Find(id);
            if (tinTuc == null)
            {
                return HttpNotFound();
            }
            return View(tinTuc);
        }

 
        public ActionResult DeleteConfirmed(int id)
        {
            TinTuc tinTuc = db.TinTuc.Find(id);
            db.TinTuc.Remove(tinTuc);
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
