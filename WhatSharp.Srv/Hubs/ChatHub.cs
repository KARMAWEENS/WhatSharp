using Microsoft.AspNetCore.SignalR;

namespace WhatSharp.Srv.Hubs;

public class ChatHub : Hub
{
    private static string GroupName(int chatId) => $"chat:{chatId}";
    
    private static string ChatGroup(int chatId) => $"chat:{chatId}";
    private static string UserGroup(int userId) => $"user:{userId}";

    public Task RegisterUser(int userId) =>
        Groups.AddToGroupAsync(Context.ConnectionId, UserGroup(userId));


    public Task JoinChat(int chatId) =>
        Groups.AddToGroupAsync(Context.ConnectionId, GroupName(chatId));

    public Task LeaveChat(int chatId) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(chatId));
}