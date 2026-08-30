//---------------------------------------------------
// Shaders from www.riemers.net, adapted for DX12. --
//---------------------------------------------------

struct PSInput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float LightingFactor : TEXCOORD0;
    float2 TextureCoords : TEXCOORD1;
};

//------- Constants --------
float4x4 xView;
float4x4 xProjection;
float4x4 xWorld;
float3 xLightDirection;
float xAmbient;
bool xEnableLighting;
bool xShowNormals;
float3 xCamPos;
float3 xCamUp;
float xPointSpriteSize;

//------- Texture Samplers --------
Texture2D<float4> xTexture : register(t0);
SamplerState TextureSampler : register(s0);

//------- Technique: Pretransformed --------

technique Pretransformed
{
    pass Pass0
    {
        VertexShader = compile vs_6_0 PretransformedVS();
        PixelShader = compile ps_6_0 PretransformedPS();
    }
}

PSInput PretransformedVS(float4 inPos : POSITION, float4 inColor : COLOR)
{
    PSInput output;
    output.Position = inPos;
    output.Color = inColor;
    return output;
}

float4 PretransformedPS(PSInput input) : SV_TARGET
{
    return input.Color;
}

//------- Technique: Colored --------

technique Colored
{
    pass Pass0
    {
        VertexShader = compile vs_6_0 ColoredVS();
        PixelShader = compile ps_6_0 ColoredPS();
    }
}

PSInput ColoredVS(float4 inPos : POSITION, float3 inNormal : NORMAL, float4 inColor : COLOR)
{
    PSInput output;
    float4x4 preViewProjection = mul(xView, xProjection);
    float4x4 preWorldViewProjection = mul(xWorld, preViewProjection);
    
    output.Position = mul(inPos, preWorldViewProjection);
    output.Color = inColor;
	
    float3 normal = normalize(mul(normalize(inNormal), (float3x3) xWorld));
    output.LightingFactor = 1;
    if (xEnableLighting)
        output.LightingFactor = dot(normal, -xLightDirection);
    
    return output;
}

float4 ColoredPS(PSInput input) : SV_TARGET
{
    float4 color = input.Color;
    color.rgb *= saturate(input.LightingFactor) + xAmbient;
    return color;
}

//------- Technique: ColoredNoShading --------

technique ColoredNoShading
{
    pass Pass0
    {
        VertexShader = compile vs_6_0 ColoredNoShadingVS();
        PixelShader = compile ps_6_0 ColoredNoShadingPS();
    }
}

PSInput ColoredNoShadingVS(float4 inPos : POSITION, float4 inColor : COLOR)
{
    PSInput output;
    float4x4 preViewProjection = mul(xView, xProjection);
    float4x4 preWorldViewProjection = mul(xWorld, preViewProjection);
    output.Position = mul(inPos, preWorldViewProjection);
    output.Color = inColor;
    return output;
}

float4 ColoredNoShadingPS(PSInput input) : SV_TARGET
{
    return input.Color;
}

//------- Technique: Textured --------

technique Textured
{
    pass Pass0
    {
        VertexShader = compile vs_6_0 TexturedVS();
        PixelShader = compile ps_6_0 TexturedPS();
    }
}

PSInput TexturedVS(float4 inPos : POSITION, float3 inNormal : NORMAL, float2 inTexCoords : TEXCOORD0)
{
    PSInput output;
    float4x4 preViewProjection = mul(xView, xProjection);
    float4x4 preWorldViewProjection = mul(xWorld, preViewProjection);
    
    output.Position = mul(inPos, preWorldViewProjection);
    output.TextureCoords = inTexCoords;
	
    float3 Normal = normalize(mul(normalize(inNormal), (float3x3) xWorld));
    output.LightingFactor = 1;
    if (xEnableLighting)
        output.LightingFactor = dot(Normal, -xLightDirection);
    
    return output;
}

float4 TexturedPS(PSInput input) : SV_TARGET
{
    float4 output;
    output = xTexture.Sample(TextureSampler, input.TextureCoords);
    output.rgb *= saturate(input.LightingFactor) + xAmbient;
    return output;
}

//------- Technique: TexturedNoShading --------

technique TexturedNoShading
{
    pass Pass0
    {
        VertexShader = compile vs_6_0 TexturedNoShadingVS();
        PixelShader = compile ps_6_0 TexturedNoShadingPS();
    }
}

PSInput TexturedNoShadingVS(float4 inPos : POSITION, float3 inNormal : NORMAL, float2 inTexCoords : TEXCOORD0)
{
    PSInput output;
    float4x4 preViewProjection = mul(xView, xProjection);
    float4x4 preWorldViewProjection = mul(xWorld, preViewProjection);
    output.Position = mul(inPos, preWorldViewProjection);
    output.TextureCoords = inTexCoords;
    return output;
}

float4 TexturedNoShadingPS(PSInput input) : SV_TARGET
{
    return xTexture.Sample(TextureSampler, input.TextureCoords);
}

//------- Technique: PointSprites --------

technique PointSprites
{
    pass Pass0
    {
        VertexShader = compile vs_6_0 PointSpriteVS();
        PixelShader = compile ps_6_0 PointSpritePS();
    }
}

PSInput PointSpriteVS(float3 inPos : POSITION0, float2 inTexCoord : TEXCOORD0)
{
    PSInput output;

    float3 center = mul(inPos, (float3x3) xWorld);
    float3 eyeVector = center - xCamPos;

    float3 sideVector = cross(eyeVector, xCamUp);
    sideVector = normalize(sideVector);
    float3 upVector = cross(sideVector, eyeVector);
    upVector = normalize(upVector);

    float3 finalPosition = center;
    finalPosition += (inTexCoord.x - 0.5f) * sideVector * 0.5f * xPointSpriteSize;
    finalPosition += (0.5f - inTexCoord.y) * upVector * 0.5f * xPointSpriteSize;

    float4 finalPosition4 = float4(finalPosition, 1);

    float4x4 preViewProjection = mul(xView, xProjection);
    output.Position = mul(finalPosition4, preViewProjection);

    output.TextureCoords = inTexCoord;

    return output;
}

float4 PointSpritePS(PSInput input) : SV_TARGET
{
    return xTexture.Sample(TextureSampler, input.TextureCoords);
}
