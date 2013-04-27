sampler2D baseMap;

struct PS_INPUT 
{
   float2 Texcoord : TEXCOORD0;

};

float4 ps_main( PS_INPUT Input ) : COLOR0
{
   float4 color = tex2D( baseMap, Input.Texcoord );
   return float4(1.0f, 1.0f, 1.0f, color.w);
}