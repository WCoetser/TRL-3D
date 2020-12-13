using System;

namespace Trl_3D.Core.Abstractions
{
    public interface IAssertion : IDisposable
    {
        RenderProcessStep ProcessStep { get; }
        void SetState();
        void Render();
    }
}
