using PagedList;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.DynamicData;
using System.Web.Mvc;
using System.Web.Services.Description;
using WebApplication17.Models;

namespace WebApplication17.Areas.Admin.Controllers
{
    public class SanPhamsController : Controller
    {
        private Model1 db = new Model1();

        // GET: Admin/SanPhams
        public ActionResult Index(int? page, string hangName, bool? isHotDeal)
        {
            var sanPhams = db.SanPham.Include(s => s.HangSP).AsQueryable();

            

            // Lọc theo hãng
            if (!string.IsNullOrEmpty(hangName))
            {
                sanPhams = sanPhams.Where(sp => sp.HangSP.tenhang == hangName);
                ViewBag.SelectedHang = hangName;
            }

            // Lọc hot deal 
            if (isHotDeal == true)
            {
                sanPhams = sanPhams.Where(sp => sp.IsHotDeal == true);
                ViewBag.IsHotDeal = true;
            }

            // Tạo SelectList các hãng để hiển thị dropdown
            ViewBag.HangList = new SelectList(db.HangSP.ToList(), "tenhang", "tenhang");

            // Phân trang
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            return View(sanPhams.OrderByDescending(s => s.masp).ToPagedList(pageNumber, pageSize));
        }


        // GET: Admin/SanPhams/Details/5
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
            return View(sanPham);
        }

        // GET: Admin/SanPhams/Create
        public ActionResult Create()
        {
            ViewBag.mahang = new SelectList(db.HangSP, "mahang", "tenhang");
            List<SelectListItem> trangThaiList = new List<SelectListItem>
            {
                new SelectListItem { Value = "true", Text = "Hiển thị" },
                new SelectListItem { Value = "false", Text = "Ẩn" }
            };
            ViewBag.TrangThaiList = trangThaiList;

            return View();
        }

        // POST: Admin/SanPhams/Create  
        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "masp,mahang,tensp,hinhanh,soluong,giaban,mota,CPU,RAM,OS,manhinh,carddohoa,SSD,trangthai,IsHotDeal,model")] SanPham sanPham)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    sanPham.RAM = sanPham.RAM?.Trim().Replace("\u00A0", "");
                    sanPham.SSD = sanPham.SSD?.Trim().Replace("\u00A0", "");
                    List<string> fileNames = new List<string>();

                    // Lặp qua tất cả file upload
                    for (int i = 0; i < Request.Files.Count; i++)
                    {
                        var file = Request.Files[i];
                        if (file != null && file.ContentLength > 0)
                        {
                            string filename = Path.GetFileName(file.FileName);
                            string filepath = Server.MapPath("~/wwwroot/images/" + filename);

                            file.SaveAs(filepath);
                            fileNames.Add(filename); 
                        }
                    }

                    // Ghép các ảnh lại thành chuỗi ngăn cách bằng ;
                    sanPham.hinhanh = string.Join(";", fileNames);

                    db.SanPham.Add(sanPham);
                    db.SaveChanges();

                    
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Có lỗi nhập dữ liệu: " + ex.Message;
                ViewBag.mahang = new SelectList(db.HangSP, "mahang", "tenhang");
                return View(sanPham);
            }
        }

        // GET: Admin/SanPhams/Edit/5

        public ActionResult Edit(int? id)
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
            sanPham.mota = System.Web.HttpUtility.HtmlDecode(sanPham.mota ?? "");

            ViewBag.mahang = new SelectList(db.HangSP, "mahang", "tenhang", sanPham.mahang);
            List<SelectListItem> trangThaiList = new List<SelectListItem>
            {
                new SelectListItem { Value = "true", Text = "Hiển thị" },
                new SelectListItem { Value = "false", Text = "Ẩn" }
            };
            sanPham.RAM = sanPham.RAM?.Trim().Replace("\u00A0", "");
            sanPham.SSD = sanPham.SSD?.Trim().Replace("\u00A0", "");
            ViewBag.TrangThaiList = trangThaiList;
            return View(sanPham);
        }

        // POST: Admin/SanPhams/Edit/5
        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "masp,mahang,tensp,hinhanh,soluong,giaban,mota,CPU,RAM,OS,manhinh,carddohoa,SSD,trangthai,IsHotDeal,model")] SanPham sanPham)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    sanPham.RAM = sanPham.RAM?.Trim().Replace("\u00A0", "");
                    sanPham.SSD = sanPham.SSD?.Trim().Replace("\u00A0", "");
                    sanPham.mota = System.Web.HttpUtility.HtmlDecode(sanPham.mota ?? "");

                    // Danh sách tên file ảnh
                    List<string> fileNames = new List<string>();

                    // Lặp qua toàn bộ file được upload
                    for (int i = 0; i < Request.Files.Count; i++)
                    {
                        var file = Request.Files[i];
                        if (file != null && file.ContentLength > 0)
                        {
                            string filename = Path.GetFileName(file.FileName);
                            string filepath = Server.MapPath("~/wwwroot/images/" + filename);
                            file.SaveAs(filepath);
                            fileNames.Add(filename);
                        }
                    }

                    // Nếu có upload ảnh thì nối tên file bằng dấu ";"
                    if (fileNames.Count > 0)
                    {
                        sanPham.hinhanh = string.Join(";", fileNames);
                    }

                    db.Entry(sanPham).State = EntityState.Modified;
                    db.SaveChanges();
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Có lỗi nhập dữ liệu: " + ex.Message;
                ViewBag.mahang = new SelectList(db.HangSP, "mahang", "tenhang", sanPham.mahang);
                return View(sanPham);
            }

        }

        // GET: Admin/SanPhams/Delete/5
        public ActionResult Delete(int? id)
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
            return View(sanPham);
        }

        public ActionResult DeleteConfirmed(int id)
        {
            SanPham sanPham = db.SanPham.Find(id);
            db.SanPham.Remove(sanPham);
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
