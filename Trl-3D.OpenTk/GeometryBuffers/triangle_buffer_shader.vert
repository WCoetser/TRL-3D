#version 450 core

layout (location = 0) in float vertexIdIn;
layout (location = 1) in float surfaceIdIn;
layout (location = 2) in vec3 vertexPosition;
layout (location = 3) in vec4 vertexColorIn;
layout (location = 4) in vec2 texCoordsIn;
layout (location = 5) in float samplerIndexIn;

float vertexId; // this will not work in the fragment shader, therefore no "out"

out float surfaceId;
out vec4 vertexColor;
out vec2 texCoords;
out float samplerIndex;

uniform mat4 viewMatrix;
uniform mat4 projectionMatrix;

void main()
{
    vertexId = vertexIdIn;
    surfaceId = surfaceIdIn;
    vertexColor = vertexColorIn;
    texCoords = texCoordsIn;
    samplerIndex = samplerIndexIn;

    gl_Position = projectionMatrix * viewMatrix * vec4(vertexPosition.x, vertexPosition.y, vertexPosition.z, 1.0);
}