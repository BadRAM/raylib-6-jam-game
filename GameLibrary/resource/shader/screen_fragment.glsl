#version 330

// Input vertex attributes (from vertex shader)
in vec2 fragTexCoord;
in vec4 fragColor;

// Input uniform values
uniform sampler2D texture0;
uniform sampler2D mask;

// Output fragment color
out vec4 finalColor;

void main()
{
    vec4 maskColour = texture(mask, fragTexCoord / textureSize(mask, 0) * 30000) + 0.1;
    vec4 texelColor = texture(texture0, fragTexCoord);

    finalColor = texelColor*maskColour;
    
    //col = mix(vec4(0.0,0.0,0.0,1.0), col, 0.5 + mask.x);
}