Shader "Minecraft/Blocks/Standard Blocks" {
	Properties{
		_MainTex("Block Texture Atlas", 2D) = "white" {}
	}
		SubShader{
			Tags {"RenderType"="Opaque"}
			LOD 100
			Lighting Off

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

					fixed4 fragFunc(v2f IN) : SV_Target {
						fixed4 col = tex2D(_MainTex, IN.uv);
						float shade = (maxGlobalLightLevel - minGlobalLightLevel) * GlobalLightLevel + minGlobalLightLevel;
						shade *= IN.color.a;
						shade = clamp(1 - shade, minGlobalLightLevel, maxGlobalLightLevel);
						float localLightLevel = clamp(GlobalLightLevel + IN.color.a, 0, 1);
						//clip(col.a - 1);

						col = lerp(col, float4(0, 0, 0, 1), shade);

						return col;
					}
				ENDCG
		}
	}
}

