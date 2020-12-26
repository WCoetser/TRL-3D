using Trl_3D.Core.Abstractions;

namespace Trl_3D.Core.Assertions
{
    public record ClearColor(float Red, float Green, float Blue): IAssertion;

    public record GrabScreenshot() : IAssertion;

    public record RenderTestTriagle() : IAssertion;

}
