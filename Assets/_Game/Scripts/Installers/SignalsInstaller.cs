using Game.Common;
using Zenject;

namespace Game.Installers
{
    public class SignalsInstaller : Installer<SignalsInstaller>
    {
        public override void InstallBindings()
        {
            SignalBusInstaller.Install(Container);
        }

        private DeclareSignalRequireHandlerAsyncTickPriorityCopyBinder DeclareSignal<TSignal>()
        {
            return Container.DeclareSignal<TSignal>();
        }
    }
}