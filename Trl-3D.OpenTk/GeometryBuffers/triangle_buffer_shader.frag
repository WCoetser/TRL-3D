#version 450 core

in float surfaceId; // TODO: Cater for more bits to accomodate ulong
in vec4 vertexColor;
in vec2 texCoords;
flat in float samplerIndex;

uniform bool isInPickingMode;
uniform sampler2D samplers[{{maxFragmentShaderTextureUnits}}];

out vec4 pixelColorOut;

void main() 
{
    if (isInPickingMode) 
    {
        float id = surfaceId;
        float green = mod(id, 256);
        id = floor(id / 256);
        float blue = mod(id, 256);
        id = floor(id / 256);
        float red =  mod(id, 256);       
        pixelColorOut = vec4(red / 255, blue / 255, green / 255, 1);
    }  
    else 
    {
        int intSamplerIndex = int(floor(samplerIndex));
        if (intSamplerIndex != -1) 
        {
            pixelColorOut = texture(samplers[intSamplerIndex], texCoords);       
        }
        else {
            pixelColorOut = vertexColor;
        }
    }
}
