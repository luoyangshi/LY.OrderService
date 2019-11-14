using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace LY.OrderService.Controllers
{
    [Route("order/api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public HomeController(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        /// <summary>
        /// 获取当前环境变量
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetEnvironment")]
        public string GetEnvironment()
        {
            return _webHostEnvironment.EnvironmentName;
        }

        /// <summary>
        /// Index
        /// </summary>
        /// <returns></returns>
        [HttpGet("Index")]
        public IEnumerable<string> Index()
        {
            return new List<string>() { "Home", "Index" };
        }

        /// <summary>
        /// 获取信息
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpGet("Identity")]
        public IEnumerable<string> Identity()
        {
            return User.Claims.Select(claim => $"{claim.Type}-----{claim.Value}").ToList();
        }

        /// <summary>
        /// redis操作
        /// </summary>
        [HttpGet("Redis")]
        public void Redis()
        {
            RedisHelper.Set("Test", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        /// <summary>
        /// 获取姓名
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetName")]
        public string GetName()
        {
            return "Hello word";
        }
    }
}