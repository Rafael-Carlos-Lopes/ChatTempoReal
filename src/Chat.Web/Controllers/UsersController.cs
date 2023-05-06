using AutoMapper;
using Chat.Web.Data;
using Chat.Web.Hubs;
using Chat.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Chat.Web.Controllers
{
    public class UsersController: ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController(ApplicationDbContext context, IMapper mapper, IHubContext<ChatHub> hubContext, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _mapper = mapper;
            _hubContext = hubContext;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<List<ApplicationUser>> ObterUsuariosAsync()
            => await _context.AppUsers.ToListAsync();
    }
}
