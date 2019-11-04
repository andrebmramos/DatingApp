﻿using DatingApp.API.Helpers;
using DatingApp.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatingApp.API.Data
{
    public interface IDatingRepository
    {
        void Add<T>(T entity) where T : class;
        void Delete<T>(T entity) where T : class;
        Task<bool> SaveAll();

        // Usuários
        Task<PagedList<User>> GetUsers(UserParams userParams);
        Task<User> GetUser(int id);

        // Fotos
        Task<Photo> GetPhoto(int id);
        Task<Photo> GetMainPhotoForUser(int userId);

        // Likes
        Task<Like> GetLike(int userId, int recipientId);

        // Messages
        Task<Message> GetMessage(int id);
        Task<PagedList<Message>> GetMessagesForUser(MessageParams messageParams); // Temporariamente sem parâmetros
        Task<IEnumerable<Message>> GetMessageThread(int userId, int recipientId); // "A conversa entre 2 usuários"



    }

}
