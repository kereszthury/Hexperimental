#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

matrix WorldViewProjection;
float3 CameraPosition;
float3 LightDirection;

float4 WaterColor;
float AnimationProgress;

struct VertexShaderInput
{
	float4 Position : POSITION0;
 	float4 Color : COLOR0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION0;
    float4 WorldPosition : TEXCOORD0;
    float4 Color : COLOR0;
};

VertexShaderOutput TerrainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

	output.Position = mul(input.Position, WorldViewProjection);
    output.WorldPosition = input.Position;
    output.Color = input.Color;
	
	return output;
}

VertexShaderOutput WaterVS(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput) 0;

    float depthPercent = input.Color.x;
    float waveSpeed = input.Color.y;
    float offset1 = input.Color.z;
    float offset2 = input.Color.w;
	
    float maxDepth = 10.0f;
	
    float4 position = (float4) 0;
    float len = length(input.Position.xyz);
    
    float radian = 2.0f * 3.14159265f;
    
    float waveOffset = 0.75f * sin(sign(offset2 - 0.5f) * waveSpeed * AnimationProgress * radian + offset1 * radian)
    + 0.25f * cos(sign(offset1 - 0.5f) * waveSpeed * 2.0f * AnimationProgress * radian + offset2 * radian);
    
    position.xyz = input.Position.xyz / len * (len + 0.1f * waveOffset);
    position.w = 1;
    
    output.Position = mul(position, WorldViewProjection);
    output.WorldPosition = position;
    output.Color = WaterColor * (0.5f + 0.5f * depthPercent);
    
    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float3 position = input.WorldPosition;
    float3 normal = normalize(cross(ddx(position), ddy(position)));

    float lightStrength = dot(normal, LightDirection);

    float4 output = input.Color;
    output.rgb *= saturate(lightStrength) + 0.1f;
	
	return output;
}

technique BasicColorDrawing
{
	pass P0
	{
        VertexShader = compile VS_SHADERMODEL TerrainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}

    pass P1
    {
        VertexShader = compile VS_SHADERMODEL WaterVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};