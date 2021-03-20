using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Product_Management_App.Models;
using System.IO;

namespace Product_Management_App.Controllers
{
    [RoutePrefix("Products")]
    public class ProductsController : Controller
    {
        private ProductManagementAppEntities db = new ProductManagementAppEntities();
        // GET: Products
        public ActionResult Index()
        {
            try
            {
                var products = db.Products.Include(p => p.Catagories);
                return View(products.ToList());
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
                return View();
            }
        }

        // GET: Products/Details/5
        public ActionResult Details(int? id)
        {
            try
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Products products = db.Products.Find(id);
                if (products == null)
                {
                    return HttpNotFound();
                }
                return View(products);
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
                return View();
            }
        }

        // GET: Products/AddProduct
        [Route("AddProduct")]
        public ActionResult AddProduct()
        {
            try
            {
                ViewBag.CategoryList = new SelectList(db.Catagories, "ID", "Name");
                return View();
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
                return View();
            }
        }

        // POST: Products/AddProduct
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("AddProduct")]
        public ActionResult AddProduct([Bind(Include = "Name,CategoryId,Description,SKU,SellingPrice,AvailableQty,Type,Weight,Length,Width,Height")] Products products)
        {
            try
            {
                if (ModelState.IsValid && TempData.ContainsKey("imagePath"))
                {
                    products.ImagePath = TempData["imagePath"].ToString();
                    products.Catagories = db.Catagories.Find(products.CategoryId);
                    if (products.Type == 0)
                    {
                        double Volumetric_weight = (double)(products.Length * products.Width * products.Height) / 5000;
                        products.ShippingCost = (decimal)(50 * Math.Max(Volumetric_weight, (double)products.Weight));
                    }
                    else
                    {
                        products.ShippingCost = Math.Max(50, (products.SellingPrice * 10 / 100));
                    }
                    db.Products.Add(products);
                    db.SaveChanges();
                    return Json(new { Success = true, redirectToUrl = Url.Action("Index", "Products") });
                }

                ViewBag.CategoryList = new SelectList(db.Catagories, "ID", "Name", products.CategoryId);
                return Json(new { Success = false, Message = "Please Check All Fields before Adding." });
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }
            return View(products);
        }

        // GET: Products/Edit/5
        public ActionResult Edit(int? id)
        {
            try
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Products products = db.Products.Find(id);
                if (products == null)
                {
                    return HttpNotFound();
                }
                ViewBag.CategoryList = new SelectList(db.Catagories, "ID", "Name", products.CategoryId.ToString());
                return View(products);
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
                return View();
            }
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,CategoryId,Name,Description,SKU,ImagePath,SellingPrice,AvailableQty,Type,Weight,Length,Width,Height")] Products products)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (TempData.ContainsKey("imagePath"))
                    {
                        products.ImagePath = TempData["imagePath"].ToString();
                    }
                    db.Entry(products).State = EntityState.Modified;
                    products.Catagories = db.Catagories.Find(products.CategoryId);
                    if (products.Type == 0)
                    {
                        double Volumetric_weight = (double)(products.Length * products.Width * products.Height) / 5000;
                        products.ShippingCost = (decimal)(50 * Math.Max(Volumetric_weight, (double)products.Weight));
                    }
                    else
                    {
                        products.ShippingCost = Math.Max(50, (products.SellingPrice * 10 / 100));
                    }
                    db.Products.Add(products);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                ViewBag.CategoryList = new SelectList(db.Catagories, "ID", "Name", products.CategoryId);
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }
            return View(products);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                Products products = db.Products.Find(id);
                db.Products.Remove(products);
                db.SaveChanges();
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }
            return Json(new { redirectToUrl = Url.Action("Index", "Products") });
        }

        [Route("Upload")]
        [HttpPost]
        public JsonResult Upload()
        {
            // Checking no of files injected in Request object  
            if (Request.Files.Count > 0)
            {
                try
                {
                    //  Get all files from Request object  
                    HttpFileCollectionBase files = Request.Files;
                    for (int i = 0; i < files.Count; i++)
                    {
                        //string path = AppDomain.CurrentDomain.BaseDirectory + "Uploads/";  
                        //string filename = Path.GetFileName(Request.Files[i].FileName);  

                        HttpPostedFileBase file = files[i];
                        string fname;

                        // Checking for Internet Explorer  
                        if (Request.Browser.Browser.ToUpper() == "IE" || Request.Browser.Browser.ToUpper() == "INTERNETEXPLORER")
                        {
                            string[] testfiles = file.FileName.Split(new char[] { '\\' });
                            fname = testfiles[testfiles.Length - 1];
                        }
                        else
                        {
                            fname = file.FileName;
                        }

                        // Get the complete folder path and store the file inside it.  
                        fname = Path.Combine(Server.MapPath("~/Uploads/"), fname);
                        TempData["imagePath"] = fname;
                        file.SaveAs(fname);
                    }
                    // Returns message that successfully uploaded  
                    return Json("File Uploaded Successfully!", JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json("Error occurred. Error details: " + ex.Message, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json("No files selected.", JsonRequestBehavior.AllowGet);
            }
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
