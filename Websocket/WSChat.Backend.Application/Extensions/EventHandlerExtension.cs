﻿using System;
using WSChat.Backend.Domain.Enums;
using WSChat.Backend.Domain.Models;

namespace WSChat.Backend.Application.Extensions
{
    public static class EventHandlerExtension
    {
        public static Message GenerateEventMessage(this Message message, EventEnum newEvent)
        {
            Message objMessage = new Message()
            {
                Event = newEvent,
                Room = message.Room,
                User = message.User,
                TargetUserId = message.TargetUserId,
                IsPrivate = message.IsPrivate
            };

            switch (newEvent)
            {
                case EventEnum.UpdateUserList:
                    if (message.Event == EventEnum.WelcomeMessage)
                        objMessage.MessageText = $"{objMessage.User?.Nickname} entered in room.";
                    else
                        objMessage.MessageText = $"{objMessage.User?.Nickname} left the room.";
                    break;
                case EventEnum.UpdateRoomList:
                    objMessage.MessageText = $"New room called #{objMessage.Room?.Name} is available.";
                    break;
                case EventEnum.WelcomeMessage:
                    objMessage.MessageText = $"Welcome {objMessage.User?.Nickname} to this room.";
                    break;
                case EventEnum.GoodbyeMessage:
                    objMessage.MessageText = $"{objMessage.User?.Nickname} left the room.";
                    break;
                case EventEnum.UpdateUsersRooms:
                    objMessage.MessageText = $"Number of users per rooms was changed.";
                    break;
                case EventEnum.MessageAndUpdateUserRoom:
                    objMessage.MessageText = $"Send signal successfully";
                    break;
                default:
                    break;
            }

            return objMessage;

        }
    }
}
