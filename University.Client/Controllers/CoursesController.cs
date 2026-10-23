using Microsoft.AspNetCore.Mvc;

namespace University.Client.Controllers
{
    // Убираем любые упоминания apiVersion здесь. Это обычный MVC контроллер сайта.
    public class CoursesController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;

        public CoursesController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        // Этот метод будет отвечать на адрес: https://localhost:7001/Courses/Index или просто на главную
        public async Task<IActionResult> Index()
        {
            var client = _clientFactory.CreateClient("UniversityApiClient");
            try
            {
                // Запрашиваем у нашего сервера API первую версию списка курсов напрямую
                var response = await client.GetAsync("api/v1/courses");

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<List<dynamic>>();
                    return View(data);
                }

                ViewBag.Error = $"Сервер API вернул ошибку: {response.StatusCode}";
            }
            catch (HttpRequestException)
            {
                // Обработка недоступности сервера (Пункт 1 задания)
                ViewBag.Error = "Внимание: API сервис университета сейчас недоступен. Проверьте, запущен ли проект University.API.";
            }

            return View(new List<dynamic>());
        }
    }
}