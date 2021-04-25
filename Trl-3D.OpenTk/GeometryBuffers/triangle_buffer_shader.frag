#version 450 core

in float surfaceId; // TODO: Cater for more bits to accomodate ulong
in vec4 vertexColor;
in vec2 texCoords;
flat in float samplerIndex;

uniform sampler2D samplers[{{maxFragmentShaderTextureUnits}}];

layout(location = 0) out vec4 pixelColorOut;
layout(location = 1) out vec4 pickedObjectIdOut;

void main() 
{
    // Main screen output
    int intSamplerIndex = int(floor(samplerIndex));
    if (intSamplerIndex != -1) 
    {
        pixelColorOut = texture(samplers[intSamplerIndex], texCoords);       
    }
    else {
        pixelColorOut = vertexColor;
    }

    // Object ID of object being rendered
    float id = surfaceId;
    float green = mod(id, 256);
    id = floor(id / 256);
    float blue = mod(id, 256);
    id = floor(id / 256);
    float red =  mod(id, 256);       
    pickedObjectIdOut = vec4(red / 255, blue / 255, green / 255, 1);
}
