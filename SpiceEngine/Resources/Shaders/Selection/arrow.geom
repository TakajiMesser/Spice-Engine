﻿#version 400

const float LINE_WIDTH = 0.1;
const float MIN_LINE_SCREEN_WIDTH = 0.1;
const float BETWEEN_WIDTH = 0.05;

layout(points) in;
layout(triangle_strip, max_vertices = 45) out;

uniform mat4 viewMatrix;
uniform mat4 projectionMatrix;
uniform vec3 cameraPosition;
uniform vec3 xDirection;
uniform vec3 yDirection;
uniform vec3 zDirection;

out vec4 fColor;

void drawArrow(mat4 viewProjectionMatrix, vec3 position, vec4 color, vec3 direction, vec3 perpendicular)
{
    fColor = color;
	//vec4 screenPosition;

	//vec4 initialScreenPosition = viewMatrix * vec4(position, 1.0);

    position -= perpendicular * (LINE_WIDTH / 2.0);
	/*screenPosition = viewMatrix * vec4(position, 1.0);

    vec4 outputPosition = viewProjectionMatrix * vec4(position, 1.0);

	vec2 diff = screenPosition.xy - initialScreenPosition.xy;
	float diffLength = length(diff);

	if (diffLength < MIN_LINE_SCREEN_WIDTH)
	{
		outputPosition.x = 5.0;
		outputPosition.y = 5.0;
	}*/

	gl_Position = viewProjectionMatrix * vec4(position, 1.0);
    EmitVertex();

    position += direction;
    //screenPosition = viewProjectionMatrix * vec4(position, 1.0);
	gl_Position = viewProjectionMatrix * vec4(position, 1.0);
    EmitVertex();
    
    position -= direction;
    position += perpendicular * LINE_WIDTH;
    gl_Position = viewProjectionMatrix * vec4(position, 1.0);
    EmitVertex();

    position += direction;
    gl_Position = viewProjectionMatrix * vec4(position, 1.0);
    EmitVertex();

    EndPrimitive();

    position -= perpendicular * LINE_WIDTH * 1.5;
    gl_Position = viewProjectionMatrix * vec4(position, 1.0);
    EmitVertex();

    position += perpendicular * LINE_WIDTH * 2;
    gl_Position = viewProjectionMatrix * vec4(position, 1.0);
    EmitVertex();

    position -= perpendicular * LINE_WIDTH;
    position += direction * LINE_WIDTH * 1.5;
    gl_Position = viewProjectionMatrix * vec4(position, 1.0);
    EmitVertex();

    EndPrimitive();
}

void drawBetween(mat4 viewProjectionMatrix, vec3 position, vec4 color, vec3 directionA, vec3 directionB)
{
    fColor = color;

    position += directionA * 0.25;
    //position += directionB * (LINE_WIDTH / 2.0);
    gl_Position = viewProjectionMatrix * vec4(position, 1.0);
    EmitVertex();

    position += directionB * 0.25;// - (LINE_WIDTH / 2.0);
    gl_Position = viewProjectionMatrix * vec4(position, 1.0);
    EmitVertex();
    
    position -= directionB * 0.25;// - (LINE_WIDTH / 2.0);
    position -= directionA * BETWEEN_WIDTH;
    gl_Position = viewProjectionMatrix * vec4(position, 1.0);
    EmitVertex();

    position += directionB * 0.25;
    gl_Position = viewProjectionMatrix * vec4(position, 1.0);
    EmitVertex();

    EndPrimitive();
}

void main() 
{
    mat4 viewProjectionMatrix = projectionMatrix * viewMatrix;
	vec3 position = gl_in[0].gl_Position.xyz;

    vec3 toCamera = normalize(cameraPosition - position);

    if (xDirection != vec3(0.0, 0.0, 0.0))
    {
        vec3 perpendicular = cross(toCamera, xDirection);
        vec4 color = vec4(1.0, 0.0, 0.0, 1.0);
        drawArrow(viewProjectionMatrix, position, color, xDirection, normalize(perpendicular));
    }
    
    if (yDirection != vec3(0.0, 0.0, 0.0))
    {
        vec3 perpendicular = cross(toCamera, yDirection);
        vec4 color = vec4(0.0, 1.0, 0.0, 1.0);
        drawArrow(viewProjectionMatrix, position, color, yDirection, normalize(perpendicular));
    }
    
    if (zDirection != vec3(0.0, 0.0, 0.0))
    {
        vec3 perpendicular = cross(toCamera, zDirection);
        vec4 color = vec4(0.0, 0.0, 1.0, 1.0);
        drawArrow(viewProjectionMatrix, position, color, zDirection, normalize(perpendicular));
    }

    if (xDirection != vec3(0) && yDirection != vec3(0))
    {
        vec4 color = vec4(1.0, 1.0, 0.0, 1.0);
        drawBetween(viewProjectionMatrix, position, color, xDirection, yDirection);
        drawBetween(viewProjectionMatrix, position, color, yDirection, xDirection);
    }
    
    if (yDirection != vec3(0) && zDirection != vec3(0))
    {
        vec4 color = vec4(0.0, 1.0, 1.0, 1.0);
        drawBetween(viewProjectionMatrix, position, color, yDirection, zDirection);
        drawBetween(viewProjectionMatrix, position, color, zDirection, yDirection);
    }
    
    if (xDirection != vec3(0) && zDirection != vec3(0))
    {
        vec4 color = vec4(1.0, 0.0, 1.0, 1.0);
        drawBetween(viewProjectionMatrix, position, color, zDirection, xDirection);
        drawBetween(viewProjectionMatrix, position, color, xDirection, zDirection);
    }
}