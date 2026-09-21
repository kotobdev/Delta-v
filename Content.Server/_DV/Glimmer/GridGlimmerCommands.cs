using Content.Server.Administration;
using Content.Shared._DV.Glimmer;
using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Shared._DV.Glimmer;

[AdminCommand(AdminFlags.Debug)]
public sealed class AddGlimmerCommand : IConsoleCommand
{
    public string Command => "addglimmer";
    public string Description => "Increases glimmer at your position by given amount in given radius."; // TODO: LOC
    public string Help => "idk vro lmao";

    public async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (!int.TryParse(args[0], out var amount))
            return;

        if (!float.TryParse(args[1], out var radius))
            return;

        var entManager = IoCManager.Resolve<IEntityManager>();
        entManager.EntitySysManager.GetEntitySystem<SharedGridGlimmerSystem>().AddGlimmer(shell.Player?.AttachedEntity, amount, radius);
        shell.WriteLine($"Added {amount} glimmer in radius {radius}.");
    }
}

[AdminCommand(AdminFlags.Debug)]
public sealed class GetGlimmerCommand : IConsoleCommand
{
    public string Command => "getglimmer";
    public string Description => "Gets the glimmer at your position."; // TODO: LOC
    public string Help => "idk vro lmao";

    public async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var entManager = IoCManager.Resolve<IEntityManager>();

        var glimmer = entManager.EntitySysManager.GetEntitySystem<SharedGridGlimmerSystem>().GetGlimmer(shell.Player?.AttachedEntity);
        shell.WriteLine($"Local glimmer: {glimmer}");
    }
}
