namespace Trl_3D.Core.Abstractions
{
    /// <summary>
    /// Assertions asserting facts about the world that can be rendered as 3D models.
    /// Ex. "The clear colour for the current scene is blue" will be implemented by passing
    /// and instance of <see cref="Assertions.ClearColor"/> through the rendering system
    /// via <see cref="IAssertionLoader"/>. 
    /// 
    /// This is a declarative statement rather than a render command. The render commands
    /// are generated from groups of assertions.
    /// </summary>
    public interface IAssertion
    {       

    }
}
