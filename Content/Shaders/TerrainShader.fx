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

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float4 Color : COLOR0;
	nointerpolation float4 Normal : NORMAL0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION0;
    float4 WorldPosition : TEXCOORD0;
    float4 Color : COLOR0;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

	output.Position = mul(input.Position, WorldViewProjection);
    output.WorldPosition = input.Position;
    output.Color = input.Color;
	
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
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};