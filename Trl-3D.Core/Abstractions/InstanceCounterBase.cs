namespace Trl_3D.Core.Abstractions
{
    public abstract class InstanceCounterBase : ObjectIdentityBase
    {
        private static ulong InstanceCount = 0;

        public InstanceCounterBase() : base(InstanceCount++)
        {
        }
    }
}
