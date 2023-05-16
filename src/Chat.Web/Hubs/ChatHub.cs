using AutoMapper;
using Chat.Web.Data;
using Chat.Web.Models;
using Chat.Web.ViewModels;
using ChatTempoReal.Web.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Chat.Web.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        public readonly static List<UserViewModel> _Connections = new List<UserViewModel>();
        private readonly static Dictionary<string, string> _ConnectionsMap = new Dictionary<string, string>();


        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public ChatHub(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task SendPrivate(string senderId, string receiverName, string message)
        {
            try
            {
                    // Who is the sender;
                    var sender = _Connections.Where(u => u.Id == senderId).First();

                    var fromUser = _context.Users.FirstOrDefault(u => u.Id == sender.Id);
                    var toUser = _context.Users.FirstOrDefault(u => u.Id == receiverName);

                    await Leave(sender.CurrentRoom);

                    if (!string.IsNullOrEmpty(message.Trim()))
                    {
                        // Build the message
                        var messageViewModel = new MessageViewModel()
                        {
                            Content = Regex.Replace(message, @"<.*?>", string.Empty),
                            FromUserName = sender.UserName,
                            FromFullName = sender.FullName,
                            Avatar = sender.Avatar,
                            Room = "",
                            Timestamp = DateTime.Now
                        };

                        var msg = new Message
                        {
                            Content = Regex.Replace(message, @"<.*?>", string.Empty),
                            FromUserId = fromUser.Id,
                            ToUserId = toUser.Id,
                            Timestamp = DateTime.Now
                        };

                        await _context.Messages.AddAsync(msg);
                        await _context.SaveChangesAsync();

                        // Send the message
                        await Clients.Client(toUser.Id).SendAsync("newMessage", messageViewModel);
                        await Clients.Caller.SendAsync("newMessage", messageViewModel);
                    }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public async Task Join(string roomName)
        {
            try
            {
                var user = _Connections.Where(u => u.UserName == IdentityName).FirstOrDefault();
                if (user != null && user.CurrentRoom != roomName)
                {
                    // Remove user from others list
                    if (!string.IsNullOrEmpty(user.CurrentRoom))
                        await Clients.OthersInGroup(user.CurrentRoom).SendAsync("removeUser", user);

                    // Join to new chat room
                    await Leave(user.CurrentRoom);
                    await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
                    user.CurrentRoom = roomName;

                    // Tell others to update their list of users
                    await Clients.OthersInGroup(roomName).SendAsync("addUser", user);
                }
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("onError", "You failed to join the chat room!" + ex.Message);
            }
        }

        public async Task JoinPrivateChat(string roomId)
        {
            try
            {
                var Emissor = _Connections.Where(u => u.UserName == IdentityName).FirstOrDefault();
                var destinatario = _Connections.Where(u => u.Id == roomId).FirstOrDefault();
                if (Emissor != null)
                {
                    // Remove user from others list
                    if (!string.IsNullOrEmpty(Emissor.CurrentRoom))
                        await Clients.OthersInGroup(Emissor.CurrentRoom).SendAsync("removeUser", Emissor);

                    // Join to new chat room
                    await Leave(Emissor.CurrentRoom);
                    await Groups.AddToGroupAsync(Context.ConnectionId, destinatario.Id);
                    Emissor.CurrentRoom = destinatario.Id;

                    // Tell others to update their list of users
                    await Clients.OthersInGroup(destinatario.Id).SendAsync("addUser", Emissor);

                   // await Clients.Client(connection).SendAsync("Receive", chat.from, chat.message);
                }
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("onError", "You failed to join the chat room!" + ex.Message);
            }
        }

        public async Task Leave(string roomName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomName);
        }

        public IEnumerable<UserViewModel> GetUsers(string roomName)
        {
            return _Connections.Where(u => u.CurrentRoom == roomName).ToList();
        }

        public async Task<IEnumerable<UserViewModel>> GeRegistredUsers()
        {
            var users = await _context.Users.ToListAsync();

            var userViewModel = users.Select(user => new UserViewModel
            {
                Id = user.Id,
                Avatar = user.Avatar,
                UserName = user.UserName,
                FullName = user.FullName,
                Status =  GetStatus(user.Id),
            }).ToList();
         
            return userViewModel;
        }

        public override Task OnConnectedAsync()
        {
            try
            {
                var user = _context.Users.Where(u => u.UserName == IdentityName).FirstOrDefault();
                var userViewModel = _mapper.Map<ApplicationUser, UserViewModel>(user);
                userViewModel.Device = GetDevice();
                userViewModel.CurrentRoom = "";

                if (!_Connections.Any(u => u.UserName == IdentityName))
                {
                    _Connections.Add(userViewModel);
                    _ConnectionsMap.Add(IdentityName, Context.ConnectionId);
                }

                Clients.Caller.SendAsync("getProfileInfo", userViewModel);
            }
            catch (Exception ex)
            {
                Clients.Caller.SendAsync("onError", "OnConnected:" + ex.Message);
            }
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            try
            {
                var user = _Connections.Where(u => u.UserName == IdentityName).First();
                _Connections.Remove(user);

                // Tell other users to remove you from their list
                Clients.OthersInGroup(user.CurrentRoom).SendAsync("removeUser", user);

                // Remove mapping
                _ConnectionsMap.Remove(user.UserName);
            }
            catch (Exception ex)
            {
                Clients.Caller.SendAsync("onError", "OnDisconnected: " + ex.Message);
            }

            return base.OnDisconnectedAsync(exception);
        }

        private string IdentityName
        {
            get { return Context.User.Identity.Name; }
        }

        private string GetDevice()
        {
            var device = Context.GetHttpContext().Request.Headers["Device"].ToString();
            if (!string.IsNullOrEmpty(device) && (device.Equals("Desktop") || device.Equals("Mobile")))
                return device;

            return "Web";
        }

        private  string GetStatus(string userId)
           => _Connections.Exists(x => x.Id == userId) ? UserStatus.Disponivel : UserStatus.Indisponivel;
    }
}
