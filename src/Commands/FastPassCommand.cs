using System;
using System.Collections.Generic;
using QueueAPI;
using RequeueRelief.CommandAPI;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace RequeueRelief.Commands;

internal class FastPassCommand(Config config, BypassTicketManager ticketManager, ICoreServerAPI api) : Command("fastpass")
{
    protected override string? Description { get; init; } = "Issues a single use bypass ticket to a player, bringing them into the server immediately if they are in the queue. Ignores all max client and queue length limits.";

    protected override string? RequiredPrivilege { get; init; } = config.Privileges.FastPass;

    protected override Command[] SubCommands { get; init; } =
    [
        new ByClientId(ticketManager, api),
        new ByPlayerUid(ticketManager, api)
    ];

    private class ByClientId(BypassTicketManager ticketManager, ICoreServerAPI api) : Command("byClientId")
    {
        protected override ICommandArgumentParser[] Args { get; init; } =
        [
            new IntArgParser("clientID", 0, int.MaxValue, -1, true)
        ];

        protected override TextCommandResult Handle(TextCommandCallingArgs args)
        {
            var clientId = (int) args[0];
            if (!api.GetInternalServer().Clients.TryGetValue(clientId, out var client))
            {
                return TextCommandResult.Error($"[RequeueRelief] Client {clientId} is not connected. Maybe use byPlayerUid instead?");
            }

            if (client.State != EnumClientState.Queued)
            {
                return TextCommandResult.Error($"[RequeueRelief] Client {clientId} is online but not in the queue. No need for a bypass ticket.");
            }

            ticketManager.IssueTicket(client.SentPlayerUid, TimeSpan.MaxValue);
            return TextCommandResult.Success($"[RequeueRelief] Issued single use bypass ticket for client {client.PlayerName} ({clientId}) with no expiry.");
        }
    }

    private class ByPlayerUid(BypassTicketManager ticketManager, ICoreServerAPI api) : Command("byPlayerUid")
    {
        protected override ICommandArgumentParser[] Args { get; init; } =
        [
            new StringArgParser("playerUid", true)
        ];

        protected override TextCommandResult Handle(TextCommandCallingArgs args)
        {
            var playerUid = (string) args[0];
            var playerData = api.PlayerData.GetPlayerDataByUid(playerUid);
            if (playerData == null)
            {
                return TextCommandResult.Error($"[RequeueRelief] Player {playerUid} has not joined the server before.");
            }

            var client = api.GetInternalServer().GetClientByUID(playerUid);
            if (client != null && client.State != EnumClientState.Queued)
            {
                return TextCommandResult.Error($"[RequeueRelief] Player {client.PlayerName} ({client.Id}) is online but not in the queue. No need for a bypass ticket.");
            }

            ticketManager.IssueTicket(playerData.PlayerUID, TimeSpan.FromHours(1));
            return TextCommandResult.Success($"[RequeueRelief] Issued single use bypass ticket for {playerData.LastKnownPlayername} ({playerData.PlayerUID}) with 1 hour expiry.");
        }
    }
}