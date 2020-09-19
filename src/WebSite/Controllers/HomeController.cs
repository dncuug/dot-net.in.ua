﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WebSite.AppCode;
using Core;

namespace WebSite.Controllers
{
    public class HomeController : Controller
    {
        private readonly IWebAppPublicationService _service;
        private readonly Settings _settings;

        public HomeController(IWebAppPublicationService service, Settings settings) 
        {
            _settings = settings;
            _service = service;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            ViewBag.Vacancies = await _service.LoadHotVacancies();

            await base.OnActionExecutionAsync(context, next);
        }

        public async Task<IActionResult> Index()
        {
            var model = await _service.GetHomePageInformation();
            
            ViewBag.Title = "Welcome!";

            return View(model);
        }
        
        [Route("covid")]
        public async Task<IActionResult> Covid()
        {
            var model = await _service.FindPublications("covid", "coronavirus");
            
            ViewBag.Title = $"COVID-19  – news and updates";

            return View("~/Views/Home/Covid.cshtml", model);
        }

        [Route("page/{page}")]
        public async Task<IActionResult> Page(int? categoryId = null, int page = 1, string language = Core.Language.English)
        {
            var model = await _service.GetPublications(categoryId, page);
            
            ViewBag.Title = $"Page {page}";
            ViewBag.CategoryId = categoryId;

            return View("~/Views/Home/Page.cshtml", model);
        }

        [Route("vacancies/{page}")]
        public async Task<IActionResult> Vacancies(int page = 1)
        {
            var model= await _service.GetVacancies(page);
            
            ViewBag.Title = "Job";
            
            return View("~/Views/Home/Vacancies.cshtml", model);
        }

        [Route("vacancy/{id}")]
        public async Task<IActionResult> Vacancy(int id)
        {
            var model = await _service.GetVacancy(id);
            
            if (model == null)
            {
                return NotFound();
            }
            
            ViewBag.Title = model.Title;

            return View("~/Views/Home/Vacancy.cshtml", model);
        }

        [Route("post/{id}")]
        public async Task<IActionResult> Post(int id)
        {
            var publication = await _service.GetPublication(id);
            
            if (publication == null)
            {
                return NotFound();
            }

            ViewBag.Title = publication.Title;
            ViewBag.Configuration = _settings;

            return View("~/Views/Home/Post.cshtml", publication);
        }
        
        [Route("platform")]
        public async Task<IActionResult> Platform()
        {
            var model = await _service.GetPlatformInformation();
            
            ViewBag.Title = "Platform";
            ViewBag.Description = "This project is an information space for people who live in the modern world of IT technologies. For developers, analysts, architects, and engineers.";

            return View("~/Views/Home/Platform.cshtml", model);
        }
    }
}