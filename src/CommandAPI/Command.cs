using System;
using System.Reflection;
using Vintagestory.API.Common;

namespace RequeueRelief.CommandAPI;

internal abstract class Command(string name)
{
    protected virtual string Name { get; } = name;

    protected virtual string? Description { get; init; }
    protected virtual string? AdditionalInformation { get; init; }

    protected virtual CommandPreconditionDelegate? Precondition { get; init; }
    protected virtual string? RequiredPrivilege { get; init; }
    protected virtual bool RequiresPlayer { get; init; }

    protected virtual string[] Examples { get; init; } = [];
    protected virtual string[] Aliases { get; init; } = [];
    protected virtual string[] RootAliases { get; init; } = [];
    protected virtual ICommandArgumentParser[] Args { get; init; } = [];
    protected virtual Command[] SubCommands { get; init; } = [];

    protected virtual bool HasHandler => GetType().GetMember("Handle", BindingFlags.Instance | BindingFlags.NonPublic)[0].DeclaringType != typeof(Command);
    protected virtual TextCommandResult Handle(TextCommandCallingArgs args) => throw new NotSupportedException("The handler should never be called manually. If this was not called manually, something went wrong.");

    private IChatCommand Apply(IChatCommand command)
    {
        if (Aliases.Length > 0)
        {
            command = command.WithAlias(Aliases);
        }

        if (Description != null)
        {
            command = command.WithDescription(Description);
        }

        if (AdditionalInformation != null)
        {
            command = command.WithAdditionalInformation(AdditionalInformation);
        }

        if (Args.Length > 0)
        {
            command = command.WithArgs(Args);
        }

        if (Args.Length > 0)
        {
            command = command.WithExamples(Examples);
        }

        if (RequiredPrivilege != null)
        {
            command = command.RequiresPrivilege(RequiredPrivilege);
        }

        if (RequiresPlayer)
        {
            command = command.RequiresPlayer();
        }

        if (HasHandler)
        {
            command = command.HandleWith(Handle);
        }

        foreach (var sub in SubCommands)
        {
            var subCommand = command.BeginSubCommand(sub.Name);
            sub.Apply(subCommand);
            command = subCommand.EndSubCommand();
        }

        return command;
    }

    public IChatCommand Register(IChatCommandApi api) => Apply(api.Create(Name));
}