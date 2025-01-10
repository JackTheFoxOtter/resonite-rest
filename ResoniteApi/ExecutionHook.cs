using Elements.Core;
using FrooxEngine;

namespace ResoniteApi;

[ImplementableClass(true)]
internal static class ExecutionHook
{
#pragma warning disable CS0169, IDE0051, CA1823, IDE0044
    // fields must exist due to reflective access
    private static Type? __connectorType;
    private static Type? __connectorTypes;

    // implementation not strictly required, but method must exist due to reflective access
    private static DummyConnector InstantiateConnector()
    {
        return new DummyConnector();
    }
#pragma warning restore CS0169, IDE0051, CA1823, IDE0044

    static ExecutionHook()
    {
        try
        {
            UniLog.Log("[ResoniteApi] Registering post-initialization step");
            Engine.Current.OnReady += () => Task.Run(async () => await UserspaceInit());
        }
        catch (Exception e)
        {
            UniLog.Error($"[ResoniteApi] Exception in ExecutionHook!\n{e}");
        }
    }

    async private static Task UserspaceInit()
    {
        try
        {
            World userspaceWorld = Userspace.UserspaceWorld ?? throw new ApplicationException("Userspace world not found!");
            if (userspaceWorld.State != World.WorldState.Running) throw new ApplicationException("Userspace world not running!");

            UniLog.Log("[ResoniteApi] Initializing plugin component(s) in userspace world...");
            userspaceWorld.RunSynchronously(() =>
            {
                Slot pluginSlot = userspaceWorld.AddSlot("Resonite Api", false);
                pluginSlot.OrderOffset = -1;

                Component apiComponent = pluginSlot.AttachComponent<ResoniteApi>();
                apiComponent.Persistent = false;
            });
        }
        catch (Exception e)
        {
            UniLog.Error($"[ResoniteApi] Exception in UserspaceInit!\n{e}");
        }
    }

    // type must match return type of InstantiateConnector()
    private sealed class DummyConnector : IConnector
    {
        public IImplementable? Owner { get; private set; }
        public void ApplyChanges() { }
        public void AssignOwner(IImplementable owner) => Owner = owner;
        public void Destroy(bool destroyingWorld) { }
        public void Initialize() { }
        public void RemoveOwner() => Owner = null;
    }
}