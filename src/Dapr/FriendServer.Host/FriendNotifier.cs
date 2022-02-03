﻿using System.Collections.ObjectModel;
using Dapr.Client;
using MUnique.OpenMU.Interfaces;
using MUnique.OpenMU.ServerClients;

namespace MUnique.OpenMU.FriendServer.Host;

public class FriendNotifier : IFriendNotifier
{
    private readonly DaprClient _daprClient;
    private readonly IReadOnlyDictionary<int, string> _appIds;

    public FriendNotifier(DaprClient daprClient)
    {
        this._daprClient = daprClient;

        var appIds = new Dictionary<int, string>();
        for (int i = 0; i < 100; i++)
        {
            appIds.Add(i, $"gameServer{i+1}");
        }

        this._appIds = new ReadOnlyDictionary<int, string>(appIds);
    }

    public void FriendRequest(string requester, string receiver, int serverId)
    {
        this._daprClient.InvokeMethodAsync(_appIds[serverId], nameof(IGameServer.FriendRequest), new RequestArguments(requester, receiver));
    }

    /// <inheritdoc />
    /// <remarks>It's usually never called here, but at <see cref="ServerClients.FriendServer.ForwardLetter"/>.</remarks>
    public void LetterReceived(LetterHeader letter)
    {
        this._daprClient.PublishEventAsync("pubsub", nameof(IGameServer.LetterReceived), letter);
    }

    public void FriendOnlineStateChanged(int playerServerId, string player, string friend, int friendServerId)
    {
        // todo: find out if this is correct when logging out
        if (_appIds.TryGetValue(playerServerId, out var gameServer))
        {
            this._daprClient.InvokeMethodAsync(gameServer, nameof(IGameServer.FriendOnlineStateChanged), new FriendOnlineStateChangedArguments(player, friend, friendServerId));
        }
    }

    public void ChatRoomCreated(int serverId, ChatServerAuthenticationInfo playerAuthenticationInfo, string friendName)
    {
        this._daprClient.InvokeMethodAsync(this._appIds[serverId], nameof(IGameServer.ChatRoomCreated), new ChatRoomCreationArguments(playerAuthenticationInfo, friendName));
    }

    public void InitializeMessenger(int serverId, MessengerInitializationData initializationData)
    {
        if (_appIds.TryGetValue(serverId, out var gameServer))
        {
            this._daprClient.InvokeMethodAsync(gameServer, nameof(IGameServer.InitializeMessenger), initializationData);
        }
    }
}