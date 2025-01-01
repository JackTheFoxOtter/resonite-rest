using Elements.Core;
using FrooxEngine;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

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
            Engine engine = Engine.Current;
            engine.RunPostInit(() =>
                Task.Run(new Func<Task>(UserspaceInit)).ConfigureAwait(false)
            );
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
            UniLog.Log("[ResoniteApi] Waiting for userspace world...");
            World userspaceWorld = await GetUserspaceWorld();

            UniLog.Log("[ResoniteApi] Userspace world ready, initializing plugin components...");
            InitializePluginComponents(userspaceWorld);
        }
        catch (Exception e)
        {
            UniLog.Error($"[ResoniteApi] Exception in UserspaceInit!\n{e}");
        }
    }

    async private static Task<World> GetUserspaceWorld()
    {
        World? userspaceWorld = null;
        bool worldReady = false;
        do
        {
            userspaceWorld ??= Userspace.UserspaceWorld;
            worldReady = userspaceWorld != null && userspaceWorld.State == World.WorldState.Running;

            if (!worldReady)
            {
                await Task.Delay(100);
            }
        }
        while (!worldReady);

        return userspaceWorld;
    }

    private static void InitializePluginComponents(World userspaceWorld)
    {
        userspaceWorld.RunSynchronously(() =>
        {
            Slot pluginSlot = userspaceWorld.AddSlot("Resonite Api", false);
            pluginSlot.OrderOffset = -1;

            Component apiComponent = pluginSlot.AttachComponent<ResoniteApi>();
            apiComponent.Persistent = false;
        });
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