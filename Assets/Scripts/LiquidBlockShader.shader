Shader "Minecraft/Blocks/Liquid Blocks" {
	Properties{
		_MainTex("First Atlas", 2D) = "white" {}
		_SecondaryTex("Second Texture", 2D) = "white" {}
	}
		SubShader{
			Tags {"Queue"="Transparent" "RenderType"="Transparent"}
			LOD 100
			Lighting Off
			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha

			Pass {
				CGPROGRAM
					#pragma vertex vertFunc
					#pragma fragment fragFunc
					#pragma target 2.0

					#include "UnityCG.cginc"

					struct appdata {
						float4 vertex : POSITION;
						float2 uv : TEXCOORD0;
						float4 color : COLOR;
					};
					struct v2f {
						float4 vertex : SV_POSITION;
						float2 uv : TEXCOORD0;
						float4 color : COLOR;
					};
					sampler2D _MainTex;
					sampler2D _SecondaryTex;
					float GlobalLightLevel;
					float minGlobalLightLevel;
					float maxGlobalLightLevel;
				
					v2f vertFunc(appdata v) {
						v2f OUT;

						OUT.vertex = UnityObjectToClipPos(v.vertex);
						OUT.uv = v.uv;
						OUT.color = v.color;

						return OUT;
					}

					fixed4 fragFunc(v2f IN) : SV_Target{
						IN.uv.x += (_SinTime.w * 0.5);

						fixed4 tex1 = tex2D(_MainTex, IN.uv);
						fixed4 tex2 = tex2D(_SecondaryTex, IN.uv);
						fixed4 col = lerp(tex1, tex2, 0.5 + (_SinTime.w * 0.5));

						float shade = (maxGlobalLightLevel - minGlobalLightLevel) * GlobalLightLevel + minGlobalLightLevel;
						shade *= IN.color.a;
						shade = clamp(1 - shade, minGlobalLightLevel, maxGlobalLightLevel);
						float localLightLevel = clamp(GlobalLightLevel + IN.color.a, 0, 1);
						clip(col.a - 1);

						col = lerp(col, float4(0, 0, 0, 1), shade);

						col.a = 0.5f;

						return col;
					}
				ENDCG
		}
	}
}

