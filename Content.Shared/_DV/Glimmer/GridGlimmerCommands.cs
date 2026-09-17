using Content.Shared._DV.Glimmer;
using Robust.Shared.Console;

namespace Content.Shared._DV.Glimmer;

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
        entManager.EntitySysManager.GetEntitySystem<GridGlimmerSystem>().AddGlimmer(shell.Player?.AttachedEntity, amount, radius);
    }
}
