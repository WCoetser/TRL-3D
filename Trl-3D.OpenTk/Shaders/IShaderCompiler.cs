namespace Trl_3D.OpenTk.Shaders
{
    public interface IShaderCompiler
    {
        public ShaderProgram Compile(string vertexShader, string fragmentShader);
    }
}
