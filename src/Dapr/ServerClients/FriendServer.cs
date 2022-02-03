﻿// <copyright file="FriendServer.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.ServerClients;

using Microsoft.Extensions.Logging;
using Dapr.Client;
using MUnique.OpenMU.Interfaces;

public class FriendServer : IFriendServer
{
    private readonly DaprClient _daprClient;
    private readonly ILogger<FriendServer> _logger;
    private readonly string _targetAppId;

    public FriendServer(DaprClient daprClient, ILogger<FriendServer> logger)
    {
        this._daprClient = daprClient;
        this._logger = logger;
        this._targetAppId = "friendServer";
    }

    /// <inheritdoc />
    public void ForwardLetter(LetterHeader letter)
    {
        try
        {
            this._daprClient.PublishEventAsync("pubsub", nameof(IGameServer.LetterReceived), letter);
            //this._daprClient.InvokeMethodAsync(this._targetAppId, nameof(ForwardLetter), letter);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Unexpected error when forwarding a letter.");
        }
    }

    /// <inheritdoc />
    public void FriendResponse(string characterName, string friendName, bool accepted)
    {
        try
        {
            this._daprClient.InvokeMethodAsync(this._targetAppId, nameof(FriendResponse), new FriendResponseArguments(characterName, friendName, accepted));
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Unexpected error when sending a friend response.");
        }
        
    }

    /// <inheritdoc />
    public IEnumerable<string> GetFriendList(Guid characterId)
    {
        try
        {
            return this._daprClient.InvokeMethodAsync<Guid, IEnumerable<string>>(this._targetAppId, nameof(GetFriendList), characterId).Result;
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Unexpected error when retrieving the friend list.");
            return Enumerable.Empty<string>();
        }
    }

    /// <inheritdoc />
    public IEnumerable<string> GetOpenFriendRequests(Guid characterId)
    {
        try
        {
            return this._daprClient.InvokeMethodAsync<Guid, IEnumerable<string>>(this._targetAppId, nameof(GetOpenFriendRequests), characterId).Result;
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Unexpected error when retrieving the open friend requests.");
            return Enumerable.Empty<string>();
        }
    }

    /// <inheritdoc />
    public void PlayerEnteredGame(byte serverId, Guid characterId, string characterName)
    {
        // no action required - the friend server listens to the common pubsub, published by EventPublisher
    }

    /// <inheritdoc />
    public void PlayerLeftGame(Guid characterId, string characterName)
    {
        // no action required - the friend server listens to the common pubsub, published by EventPublisher
    }

    /// <inheritdoc />
    public void SetPlayerVisibilityState(byte serverId, Guid characterId, string characterName, bool isVisible)
    {
        try
        {
            this._daprClient.InvokeMethodAsync(this._targetAppId, nameof(SetPlayerVisibilityState), new PlayerFriendOnlineStateArguments(characterId, characterName, serverId, isVisible));
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Unexpected error when setting the friend visibility state.");
        }
    }

    /// <inheritdoc />
    public bool FriendRequest(string playerName, string friendName)
    {
        try
        {
            return this._daprClient.InvokeMethodAsync<RequestArguments, bool>(this._targetAppId, nameof(FriendRequest), new RequestArguments(playerName, friendName)).Result;
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Unexpected error when sending a friend request.");
            return false;
        }
    }

    /// <inheritdoc />
    public void DeleteFriend(string name, string friendName)
    {
        try
        {
            this._daprClient.InvokeMethodAsync(this._targetAppId, nameof(DeleteFriend), new RequestArguments(name, friendName));
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Unexpected error when deleting a friend.");
        }
    }

    /// <inheritdoc />
    public void CreateChatRoom(string playerName, string friendName)
    {
        try
        {
            this._daprClient.InvokeMethodAsync(this._targetAppId, nameof(CreateChatRoom), new RequestArguments(playerName, friendName));
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Unexpected error when creating a chat room.");
        }
    }

    /// <inheritdoc />
    public bool InviteFriendToChatRoom(string selectedCharacterName, string friendName, ushort roomNumber)
    {
        try
        {
            return this._daprClient.InvokeMethodAsync<ChatRoomInvitationArguments, bool>(this._targetAppId, nameof(InviteFriendToChatRoom), new ChatRoomInvitationArguments(selectedCharacterName, friendName, roomNumber)).Result;
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Unexpected error when inviting a friend to a chat room.");
            return false;
        }
    }
}