#version 450 core

//in float vertexId; // this will not work in the fragment shader
in float surfaceId;
in vec4 vertexColor;
in vec2 texCoords;
in float samplerIndex;

uniform sampler2D samplers[{{maxFragmentShaderTextureUnits}}];

out vec4 pixelColorOut;

void main() {
    int intSamplerIndex = int(samplerIndex);
    if (intSamplerIndex != -1) {
        pixelColorOut = texture(samplers[intSamplerIndex], texCoords);
    }
    else {
        pixelColorOut = vertexColor;
    }
}
