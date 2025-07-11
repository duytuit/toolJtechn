﻿using System;
namespace WSChat.Backend.Domain.Enums
{
    public enum EventEnum
    {
        SocketConnect = 1,
        RegisterUser = 2,
        EnterRoom = 3,
        Messaging = 4,
        ExitRoom = 5,
        CreateRoom = 6,
        UpdateUserList = 7,
        UpdateRoomList = 8,
        WelcomeMessage = 9,
        GoodbyeMessage = 10,
        SocketDisconnect = 11,
        UpdateUsersRooms = 12,
        MessageAndUpdateUserRoom = 15,
        Error = 500
    }
}
