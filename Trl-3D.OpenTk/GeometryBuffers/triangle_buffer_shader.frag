#version 450 core

in float surfaceId;
in vec4 vertexColor;
in vec2 texCoords;
flat in float samplerIndex;

uniform sampler2D samplers[{{maxFragmentShaderTextureUnits}}];

out vec4 pixelColorOut;

void main() {
    int intSamplerIndex = int(floor(samplerIndex));
    if (intSamplerIndex != -1) {
        pixelColorOut = texture(samplers[intSamplerIndex], texCoords);       
    }
    else {
        pixelColorOut = vertexColor;
    }
}
