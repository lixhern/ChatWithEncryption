using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using ChatWithEncryption.Models;
using ChatWithEncryption.Data;
using Microsoft.EntityFrameworkCore;

namespace ChatWithEncryption.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _context;

        // Хранение активных чатов
        private static readonly ConcurrentDictionary<string, string> ActiveChats = new(); // Key: User1, Value: User2

        // Хранение ключей чатов
        private static readonly ConcurrentDictionary<string, string> ChatKeys = new(); // Key: User1|User2, Value: EncryptionKey

        public ChatHub(ApplicationDbContext context)
        {
            _context = context;
        }

        // Отправка запроса на чат
        public async Task SendChatRequest(string targetUserId)
        {
            var requesterId = Context.UserIdentifier;

            if (requesterId == null || string.IsNullOrEmpty(targetUserId))
                throw new InvalidOperationException("Invalid requester or target.");

            var targetUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == targetUserId);
            if (targetUser == null)
                throw new InvalidOperationException("Target user not found.");

            // Отправить запрос на чат целевому пользователю
            await Clients.User(targetUserId).SendAsync("ReceiveChatRequest", requesterId);
        }

        // Принятие чата
        public async Task AcceptChatRequest(string requesterId)
        {
            var targetUserId = Context.UserIdentifier;
            Console.WriteLine($"AcceptChatRequest called by {targetUserId} for {requesterId}");

            if (targetUserId == null || string.IsNullOrEmpty(requesterId))
                throw new InvalidOperationException("Invalid chat participants.");

            // Генерация ключа для шифрования
            var encryptionKey = GenerateKey();
            Console.WriteLine($"Generated encryption key: {encryptionKey}");

            // Сохранение чата и ключа
            ActiveChats[requesterId] = targetUserId;
            ActiveChats[targetUserId] = requesterId;

            var chatKey = CreateChatKey(requesterId, targetUserId);
            ChatKeys[chatKey] = encryptionKey;
            Console.WriteLine($"ChatKey saved: {chatKey}");
            Console.WriteLine("---------------------");
            Console.WriteLine("---------------------");
            Console.WriteLine("---------------------");
            Console.WriteLine("---------------------");


            // Отправить подтверждение чата и ключ участникам
            await Clients.User(requesterId).SendAsync("ChatStarted");
            await Clients.User(targetUserId).SendAsync("ChatStarted");
            await Clients.User(requesterId).SendAsync("ReceiveKey", encryptionKey);
            await Clients.User(targetUserId).SendAsync("ReceiveKey", encryptionKey);
        }


        // Отклонение чата
        public async Task DeclineChatRequest(string requesterId)
        {
            var targetUserId = Context.UserIdentifier;

            if (targetUserId == null || string.IsNullOrEmpty(requesterId))
                throw new InvalidOperationException("Invalid chat participants.");

            // Уведомить инициатора
            await Clients.User(requesterId).SendAsync("ChatRequestDeclined");
        }

        // Завершение чата
        public async Task EndChat(string partnerId)
        {
            var userId = Context.UserIdentifier;

            if (userId == null || string.IsNullOrEmpty(partnerId))
                throw new InvalidOperationException("Invalid chat participants.");

            // Удалить активный чат и ключ
            ActiveChats.TryRemove(userId, out _);
            ActiveChats.TryRemove(partnerId, out _);

            var chatKey = CreateChatKey(userId, partnerId);
            ChatKeys.TryRemove(chatKey, out _);

            // Уведомить обоих участников
            await Clients.User(userId).SendAsync("ChatEnded");
            await Clients.User(partnerId).SendAsync("ChatEnded");
        }

        // Отправка сообщения
        public async Task SendMessage(string targetUserId, string encryptedMessage)
        {
            
            var senderId = Context.UserIdentifier;
            Console.WriteLine("__________________________");
            Console.WriteLine("Sender ID" + senderId);
            Console.WriteLine("TArget ID" + targetUserId);
            Console.WriteLine("MEssage" +  encryptedMessage);
            Console.WriteLine("__________________________");
            Console.WriteLine("__________________________");

            Console.WriteLine("__________________________");

            if (senderId == null || string.IsNullOrEmpty(targetUserId) || string.IsNullOrEmpty(encryptedMessage))
            {
                if (targetUserId == null)
                {
                    targetUserId = ActiveChats[senderId];
                }
            }

            //var senderUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == senderId);
            //var targetUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == targetUserId);
            // Проверить, активен ли чат
            if (!ActiveChats.ContainsKey(senderId) || ActiveChats[senderId] != targetUserId)
                throw new InvalidOperationException("Chat is not active between these users.");

            // Отправить сообщение только получателю
            await Clients.User(targetUserId).SendAsync("ReceiveMessage", encryptedMessage);
            await Clients.User(senderId).SendAsync("ReceiveMessage", encryptedMessage);

        }

        // Генерация ключа шифрования
        private static string GenerateKey()
        {
            return Guid.NewGuid().ToString("N");
        }

        // Создание уникального ключа чата
        private static string CreateChatKey(string user1, string user2)
        {
            var users = new[] { user1, user2 };
            Array.Sort(users); // Сортируем для консистентности ключа
            return string.Join("|", users);
        }

        // Метод, вызываемый при отключении пользователя
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userId = Context.UserIdentifier;

            if (userId != null && ActiveChats.TryGetValue(userId, out var partnerId))
            {
                // Завершить чат при выходе пользователя
                await EndChat(partnerId);
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
