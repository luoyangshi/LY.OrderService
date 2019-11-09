using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LY.OrderService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        /// <summary>
        /// Index
        /// </summary>
        /// <returns></returns>
        [HttpGet("Index")]
        public IEnumerable<string> Index()
        {
            return new List<string>(){"Home","Index"};
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
    }
}