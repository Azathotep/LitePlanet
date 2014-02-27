struct TileType
{
   int TextureId;
   float3 ColorStart;
   float3 ColorEnd;
};

struct PlanetVsOut
{
    float4 Position   	: SV_POSITION;
    float4 Color	: COLOR;
    float3 WorldPos	: TEXCOORD0;
};

struct PsOut
{
    float4 Color : COLOR;
};

static const float PI = 3.14159265f;


float4x4 xView;
float4x4 xProjection;
float4x4 xWorld;
float zoom;
float2 planetPos;
float wav;
int planetWidth;

//------- Texture Samplers --------

Texture2D xTexture;
Texture2D xTilesTexture;

SamplerState TextureSampler
{ 
  Texture = <xTexture>; 
  Filter = MIN_MAG_MIP_POINT;
  AddressU = mirror; AddressV = mirror;
};

//MIN_MAG_LINEAR_MIP_POINT;
//MIN_MAG_MIP_POINT
//MIN_MAG_MIN_LINEAR

//------- Technique: Planet --------

PlanetVsOut PlanetVS( float4 inPos : SV_POSITION, float4 inColor: COLOR)
{	
	PlanetVsOut ret = (PlanetVsOut)0;
	float4x4 viewProjection = mul (xView, xProjection);
	float4x4 worldViewProjection = mul (xWorld, viewProjection);
    
	ret.Position = mul(inPos, worldViewProjection);	
	ret.WorldPos = mul(inPos, xWorld);
        ret.Color = inColor;
	return ret;    
}

PsOut PlanetPS(PlanetVsOut v) 
{
        PsOut ret = (PsOut)0;

        float2 relC = v.WorldPos - planetPos;
	float y = length(relC);

	float angle = atan2(-relC.x, relC.y) + PI;

	float anglePerTile = 2*PI / planetWidth;

	float x = angle / anglePerTile;

        int tileX = floor(x);
        int tileY = floor(y);
        float tileDx = x - tileX;
        float tileDy = y - tileY;

        ret.Color = float4(0,0,0,0);

	ret.Color = xTexture.Load(int3(tileX,tileY,0));
	float i = ret.Color.r;
	float a = ret.Color.a;

	bool visible;
        if (a > 0.05)
           visible = true;
        else
	   visible = false;
        if (zoom > 30)
          return ret;

	//calculate texture x and y for this tile	
	float tx = ret.Color.g;
	float ty = ret.Color.b;
	tx = tx + tileDx * 0.23;
        ty = ty + tileDy * 0.23;

	ret.Color = xTilesTexture.Sample(TextureSampler, float2(tx,ty));

	if (visible)
        {
           return ret;
	}
        ret.Color = float4(0,0,0,1);

	if (y > 500)
          ret.Color = float4(0,0,0,0);
	return ret;
}

technique Planet
{
	pass Pass0
	{   
		VertexShader = compile vs_4_0 PlanetVS();
		PixelShader  = compile ps_4_0 PlanetPS();
	}
}

