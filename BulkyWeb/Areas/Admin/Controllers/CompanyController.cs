using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize(Roles = SD.Role_Admin)]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            List<Company> objCompanyList = _unitOfWork.CompanyRepository.GetAll().ToList();
            return View(objCompanyList);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost] //annotation has to be present
        public IActionResult Create(Company obj)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.CompanyRepository.Add(obj);
                _unitOfWork.Save();
                TempData["success"] = "Company created successfully";
                return RedirectToAction("Index", "Company");
            }
            return View(obj);
        }

        public IActionResult Upsert(int? id)
        {
            if (id == null || id == 0)
            {
                return View(new Company());
            }
            else
            {
                Company company = _unitOfWork.CompanyRepository.Get(x => x.Id == id);
                return View(company);
            }
        }

        [HttpPost]
        public IActionResult Upsert(Company obj)
        {
            if (ModelState.IsValid)
            {
                if (obj.Id == 0)
                {
                    _unitOfWork.CompanyRepository.Add(obj);
                }
                else
                {
                    _unitOfWork.CompanyRepository.Update(obj);
                }

                _unitOfWork.Save();
                TempData["success"] = "Company created successfully";
                return RedirectToAction("Index");
            }
            else
            {
                return View(obj);
            }
        }





        #region Api Calls
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Company> objCompanyList = _unitOfWork.CompanyRepository.GetAll().ToList();
            return Json(new { data = objCompanyList });
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var obj = _unitOfWork.CompanyRepository.Get(x => x.Id == id);
            if (obj == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            _unitOfWork.CompanyRepository.Remove(obj);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Delete successfull" });
        }
        #endregion
    }
}
