using Trl_3D.Core.Abstractions;

namespace Trl_3D.Core.Assertions
{
    public class ClearColor : IAssertion
    {
        public float Red { get; }
        public float Green { get; }
        public float Blue { get; }

        public ClearColor(float red, float green, float blue)
        {
            Red = red;
            Green = green;
            Blue = blue;
        }
    }
}
