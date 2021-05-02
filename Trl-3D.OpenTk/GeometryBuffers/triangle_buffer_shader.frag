#version 450 core

in float surfaceId; // TODO: Cater for more bits to accomodate ulong
in vec4 vertexColor;
in vec2 texCoords;
flat in float samplerIndex;

uniform sampler2D samplers[{{maxFragmentShaderTextureUnits}}];
uniform mat4 inverseProjectViewMatrix;
uniform float windowWidth;
uniform float windowHeight;

layout(location = 0) out vec4 pixelColorOut;
layout(location = 1) out vec4 pickingInfoOut;

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

    // Reference: https://www.khronos.org/opengl/wiki/Compute_eye_space_from_window_space
    vec4 ndcPos;    
    ndcPos.x = 2.0 * gl_FragCoord.x / windowWidth - 1;
    ndcPos.y = 2.0 * gl_FragCoord.y / windowHeight - 1;
    ndcPos.z = 2.0 * gl_FragCoord.z - 1.0;
    ndcPos.w = 1.0;

    // ndc -> clip coordinates
    vec4 clipPos = ndcPos / gl_FragCoord.w;

    // clip coordinates -> unproject -> move back to world coordinates
    vec4 worldCoordinates = inverseProjectViewMatrix * clipPos;

    // TODO: Add model transforms

    // Return object ID of object being rendered and world space coordinates
    pickingInfoOut = vec4(surfaceId, worldCoordinates.x, worldCoordinates.y, worldCoordinates.z);
}
