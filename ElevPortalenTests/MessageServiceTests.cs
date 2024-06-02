﻿using ElevPortalen.Data;
using ElevPortalen.Models;
using ElevPortalen.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElevPortalenTests {
    public class MessageServiceTests {
        private readonly DbContextOptions<ElevPortalenDataDbContext> _options;
        private readonly ElevPortalenDataDbContext _context;
        private readonly MessageService _messageService;

        #region Constructor
        public MessageServiceTests() {
            _options = new DbContextOptionsBuilder<ElevPortalenDataDbContext>()
                .UseInMemoryDatabase(databaseName: "MessageServiceTests")
                .Options;

            _context = new ElevPortalenDataDbContext(_options);
            _messageService = new MessageService(_context);
        }
        #endregion

        #region SendMessage test1 - Create Message (send) - Function should return success
        [Fact]
        public async void SendMessage_ShouldReturnSuccess_WhenMessageModelIsCorrect() {
            // Arrange
            await _context.Database.EnsureDeletedAsync(); //Ensure InMemory db is clear
            var message = new MessageModel {
                ReceiverId = 1,
                SenderName = "Sender",
                Subject = "Test Subject",
                Content = "Test Content",
                Timestamp = DateTime.Now,
                IsRead = false
            };

            // Act
            var (resultMessage, isSuccess) = await _messageService.SendMessage(message);

            // Assert
            Assert.True(isSuccess);
            Assert.Equal("Message sent.", resultMessage);
        }
        #endregion

        #region Delete Message with the messageId test1 - Function should delete message
        [Fact]
        public async Task Delete_ShouldDeleteMessage_WhenMessageExists() {
            // Arrange
            await _context.Database.EnsureDeletedAsync(); // Ensure InMemory db is clear
            var message = new MessageModel {
                MessageId = 1,
                ReceiverId = 2,
                SenderName = "Sender",
                Subject = "Test Subject",
                Content = "Test Content",
                Timestamp = DateTime.Now,
                IsRead = false
            };

            // Add the message to the database
            await _context.Messages.AddAsync(message);
            await _context.SaveChangesAsync();

            // Act
            var (deleteSuccess, deleteMessage) = await _messageService.Delete(1);

            // Assert
            Assert.True(deleteSuccess);
            Assert.Equal("Message Deleted.", deleteMessage);

            // Check if the message with ID 1 is deleted
            var deletedMessage = await _context.Messages.FindAsync(1);
            Assert.Null(deletedMessage); // Assert that the message is null, meaning it has been deleted
        }
        #endregion

        #region Delete Message with the messageId test2 - Function should return message not found
        [Fact]
        public async Task Delete_ShouldReturnErrorMessage_WhenMessageDoesNotExist() {
            // Arrange
            await _context.Database.EnsureDeletedAsync(); // Ensure InMemory db is clear

            // Act
            var (deleteSuccess, deleteMessage) = await _messageService.Delete(1); // ID 1 does not exist

            // Assert
            Assert.False(deleteSuccess);
            Assert.Equal("Error: message could not be deleted - messageId : 1.", deleteMessage);
        }
        #endregion

        #region DeleteAllWithReceiverId test1 - Delete Messages with ReceiverId - Function should delete messages
        [Fact]
        public async Task DeleteAllWithReceiverId_ShouldDeleteMessages_WhenMessagesExist() {
            // Arrange
            await _context.Database.EnsureDeletedAsync(); // Ensure InMemory db is clear
            var receiverId = 1;
            var messages = new List<MessageModel>
            {
            new MessageModel { ReceiverId = receiverId, SenderName = "Sender1", Subject = "Subject1", Content = "Content1", Timestamp = DateTime.Now, IsRead = false },
            new MessageModel { ReceiverId = receiverId, SenderName = "Sender2", Subject = "Subject2", Content = "Content2", Timestamp = DateTime.Now, IsRead = false }
        };
            await _context.Messages.AddRangeAsync(messages);
            await _context.SaveChangesAsync();

            // Act
            var (success, message) = await _messageService.DeleteAllWithReceiverId(receiverId);

            // Assert
            Assert.True(success);
            Assert.Equal("Messages Deleted.", message);
            Assert.Empty(await _context.Messages.Where(m => m.ReceiverId == receiverId).ToListAsync()); // Check if messages with ReceiverId are deleted
        }
        #endregion

        #region DeleteAllWithReceiverId test2 - Delete Messages with ReceiverId - Function should handle case when no messages exist
        [Fact]
        public async Task DeleteAllWithReceiverId_ShouldHandleNoMessages_WhenNoMessagesExist() {
            // Arrange
            await _context.Database.EnsureDeletedAsync(); // Ensure InMemory db is clear
            var receiverId = 1;

            // Act
            var (success, message) = await _messageService.DeleteAllWithReceiverId(receiverId);

            // Assert
            Assert.False(success);
            Assert.Equal($"No messages found with ReceiverId - {receiverId}.", message);
        }
        #endregion

        #region MarkMessageAsRead test1 - Mark Message as Read - Function should mark message as read
        [Fact]
        public async Task MarkMessageAsRead_ShouldMarkMessageAsRead_WhenMessageExists() {
            // Arrange
            await _context.Database.EnsureDeletedAsync(); // Ensure InMemory db is clear
            var message = new MessageModel {
                MessageId = 1,
                ReceiverId = 1,
                SenderName = "Sender",
                Subject = "Test Subject",
                Content = "Test Content",
                Timestamp = DateTime.Now,
                IsRead = false
            };
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            // Act
            await _messageService.MarkMessageAsRead(message.MessageId);

            // Assert
            var updatedMessage = await _context.Messages.FindAsync(message.MessageId);
            Assert.NotNull(updatedMessage);
            Assert.True(updatedMessage.IsRead);
        }
        #endregion

        #region GetMessageWithReceiverId test1 - Get Message with ReceiverId - Function should return messages when messages exist
        [Fact]
        public async Task GetMessageWithReceiverId_ShouldReturnMessages_WhenMessagesExist() {
            // Arrange
            await _context.Database.EnsureDeletedAsync(); // Ensure InMemory db is clear
            var receiverId = 1;
            var messages = new List<MessageModel>
            {
                new MessageModel { ReceiverId = receiverId, SenderName = "Sender1", Subject = "Subject1", Content = "Content1", Timestamp = DateTime.Now, IsRead = false },
                new MessageModel { ReceiverId = receiverId, SenderName = "Sender2", Subject = "Subject2", Content = "Content2", Timestamp = DateTime.Now, IsRead = false }
            };
            await _context.Messages.AddRangeAsync(messages);
            await _context.SaveChangesAsync();

            // Act
            var result = await _messageService.GetMessageWithReceiverId(receiverId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(messages.Count, result.Count);
            foreach (var message in messages) {
                Assert.Contains(message, result);
            }
        }
        #endregion

        #region GetUnreadMessageCount test1 - Count Unread Messages - Function should return the correct count of unread messages
        [Fact]
        public async Task GetUnreadMessageCount_ShouldReturnCorrectCount_WhenMessagesExist() {
            // Arrange
            await _context.Database.EnsureDeletedAsync(); // Ensure InMemory db is clear
            var receiverId = 1;
            var messages = new List<MessageModel>
            {
                new MessageModel { ReceiverId = receiverId, IsRead = false },
                new MessageModel { ReceiverId = receiverId, IsRead = false },
                new MessageModel { ReceiverId = receiverId, IsRead = true }, // Read message
                new MessageModel { ReceiverId = receiverId, IsRead = false },
            };
            await _context.Messages.AddRangeAsync(messages);
            await _context.SaveChangesAsync();

            // Act
            var unreadMessageCount = await _messageService.GetUnredMessageCount(receiverId);

            // Assert
            Assert.Equal(3, unreadMessageCount);
        }
        #endregion

        # region GetUnreadMessageCount test2 - Count Unread Messages - return zero when all messages have been read
        [Fact]
        public async Task GetUnreadMessageCount_ShouldReturnZero_WhenNoUnreadMessagesExist() {
            // Arrange
            await _context.Database.EnsureDeletedAsync(); // Ensure InMemory db is clear
            var receiverId = 1;
            var messages = new List<MessageModel>
            {
                new MessageModel { ReceiverId = receiverId, IsRead = true },
                new MessageModel { ReceiverId = receiverId, IsRead = true },
                new MessageModel { ReceiverId = receiverId, IsRead = true },
            };
            await _context.Messages.AddRangeAsync(messages);
            await _context.SaveChangesAsync();

            // Act
            var unreadMessageCount = await _messageService.GetUnredMessageCount(receiverId);

            // Assert
            Assert.Equal(0, unreadMessageCount);
        }
        #endregion

        #region GetUnreadMessageCount test3 - Count Unread Messages - return zero when user has no messages
        [Fact]
        public async Task GetUnreadMessageCount_ShouldReturnZero_WhenNoMessagesExist() {
            // Arrange
            await _context.Database.EnsureDeletedAsync(); // Ensure InMemory db is clear
            var receiverId = 1;

            // Act
            var unreadMessageCount = await _messageService.GetUnredMessageCount(receiverId);

            // Assert
            Assert.Equal(0, unreadMessageCount);
        }

        #endregion










    }
}
