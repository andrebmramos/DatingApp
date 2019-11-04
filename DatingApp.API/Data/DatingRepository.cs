using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
    public class DatingRepository : IDatingRepository
    {
        private readonly DataContext _context;

        public DatingRepository(DataContext context)
        {
            this._context = context;
        }

        public void Add<T>(T entity) where T : class
        {
            _context.Add(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
            _context.Remove(entity);
        }

        public async Task<User> GetUser(int id)
        {
            return await _context.Users.Include(u => u.Photos).FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<Photo> GetPhoto(int id)
        {
            var photo = await _context.Photos.FirstOrDefaultAsync(p => p.Id == id);
            return photo;
        }


        public async Task<Photo> GetMainPhotoForUser(int userId)
        {
            return await _context.Photos.Where(p => p.UserId == userId).FirstOrDefaultAsync(p => p.IsMain);
        }


        public async Task<PagedList<User>> GetUsers(UserParams userParams)
        {
            // return await _context.Users.Include(u => u.Photos).ToListAsync(); // Original, antes de paginação (retornava Task<User>)
            var users = _context.Users
                .Include(u => u.Photos)
                .OrderByDescending(u => u.LastActive) // ordem padrão
                .AsQueryable(); // deferred execution a ser feita adiante: retiro o async e o ToListAsync()

            // Filtros (necessitaram o AsQueriable() acima)
            users = users.Where(u => u.Id != userParams.UserId);
            users = users.Where(u => u.Gender == userParams.Gender);
            
            // Filtro de idade: se há valor diferente dos padrões 18 / 99, converteremos em datas mínimas
            // e máximas de nascimento e retornaremos resultados filtrados. Senão, simplesmente não filtramos
            if (userParams.MinAge != 18 || userParams.MaxAge != 99)
            {
                var minDateOfBirth = DateTime.Today.AddYears(-userParams.MaxAge - 1);
                var maxDateOfBirth = DateTime.Today.AddYears(-userParams.MinAge);
                users = users.Where(u => u.DateOfBirth >= minDateOfBirth && u.DateOfBirth <= maxDateOfBirth);
            }


            // Filtrar usuários que esse usuário curtiu ou usuários que curtiram este usuário
            // (fazendo uso da função auxiliar privada GetUserLikes; Aula 153)
            if (userParams.Likers)
            {
                var userLikers = await GetUserLikes(userParams.UserId, userParams.Likers);
                users = users.Where(u => userLikers.Contains(u.Id));
            }
            if (userParams.Likees)
            {
                var userLikees = await GetUserLikes(userParams.UserId, userParams.Likers);
                users = users.Where(u => userLikees.Contains(u.Id));
            }


            // Ordem
            if (!string.IsNullOrEmpty(userParams.OrderBy))
            {
                switch (userParams.OrderBy)
                {
                    case "created":
                        users = users.OrderByDescending(u => u.Created);
                        break;
                    default:
                        users = users.OrderByDescending(u => u.LastActive);
                        break;
                }
            }

            return await PagedList<User>.CreateAsync(users, userParams.PageNumber, userParams.PageSize);
        }


        private async Task<IEnumerable<int>> GetUserLikes(int id, bool likers) // Aula 153
        {
            // Carregar o usuário com a respecitvas listas de quem ele gostou e de quem gostou dele
            var user = await _context.Users
                .Include(u => u.Likees)
                .Include(u => u.Likers)
                .FirstOrDefaultAsync(u => u.Id == id);

            // Retorna os usuários que curtiram o usuário "id"
            if (likers) 
            {
                // Sintaxe extensão
                return user.Likers.Where(like => like.LikeeId == id).Select(like => like.LikerId);
            }
            // Retorna usuários que "id" curtiu
            else
            {
                // Sintaxe Linq
                return from like in user.Likees
                       where like.LikerId == id
                       select like.LikeeId;
            }
        }


        public async Task<Like> GetLike(int userId, int recipientId)
        {
            return await _context.Likes.FirstOrDefaultAsync(like => like.LikerId == userId && like.LikeeId == recipientId);
        }


        public async Task<bool> SaveAll()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<Message> GetMessage(int id)
        {
            return await _context.Messages.FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<PagedList<Message>> GetMessagesForUser(MessageParams messageParams)
        {
            var messages = _context.Messages
                .Include(user => user.Sender).ThenInclude(user => user.Photos)
                .Include(user => user.Recipient).ThenInclude(user => user.Photos)
                .AsQueryable();

            switch (messageParams.MessageContainer)
            {
                case "Inbox":
                    messages = messages.Where(m => m.RecipientId == messageParams.UserId && m.RecipientDeleted == false);
                    break;
                case "Outbox":
                    messages = messages.Where(m => m.SenderId == messageParams.UserId && m.SenderDeleted == false);
                    break;
                default:
                    messages = messages.Where(m => m.RecipientId == messageParams.UserId && m.IsRead == false && m.RecipientDeleted == false);
                    break;
            }

            messages = messages.OrderByDescending(m => m.MessageSent);
            return await PagedList<Message>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);

        }

        public async Task<IEnumerable<Message>> GetMessageThread(int userId, int recipientId)
        {
            var messages = await _context.Messages
                .Include(user => user.Sender).ThenInclude(user => user.Photos)
                .Include(user => user.Recipient).ThenInclude(user => user.Photos)
                .Where(m => m.RecipientId == userId && m.RecipientDeleted == false &&  m.SenderId == recipientId
                    || m.RecipientId == recipientId && m.SenderDeleted == false && m.SenderId == userId)
                .OrderByDescending(m => m.MessageSent)
                .ToListAsync();
            return messages;

        }
    }
}
