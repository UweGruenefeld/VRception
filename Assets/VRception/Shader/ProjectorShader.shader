Shader "Custom/ProjectorShader" {
	Properties {
		_ProjectorDir("Projector Direction", Vector) = (0,0,0,0)
		_ShadowTex ("Cookie", 2D) = "gray" {}
		_Color("Main Color", Color) = (1,1,1,1)
		_FOV("Field of View", float) = 0
		_DontProjectOntoBackfaces("Dont project onto the backface of objects", float) = 1

	}
	Subshader {
		Tags 
		{
			"Queue"="Transparent"
		}
		Pass {
			
			ZWrite On
			ColorMask RGB
			Blend DstColor Zero
			Offset -1, -1

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			
			struct v2f {
				float4 uv : TEXCOORD0;
				float4 pos : SV_POSITION;
				half4 color : COLOR;
			};

			float4x4 unity_Projector;
			float4x4 unity_ProjectorClip;
			float4 _ProjectorDir;
			float _FOV;
			half4 _Color;
			float _DontProjectOntoBackfaces;

			v2f vert (appdata_base v)
			{
				v2f o;
				
				o.uv = mul (unity_Projector, v.vertex);

				if (_DontProjectOntoBackfaces == 1) {
					o.uv.a *= max(0, sign(-dot(_ProjectorDir, v.normal)));
				}
				
				o.pos = UnityObjectToClipPos(v.vertex);
				o.color = _Color;

				return o;
			}
			
			sampler2D _ShadowTex;
			float4 _ShadowTex_ST;
			
			fixed4 frag (v2f i) : SV_Target
			{
				float projAngle = _FOV / 100.0;
				float2 uv_normalized = float2(i.uv.x / i.uv.w, i.uv.y / i.uv.w);
			
				// Calculate texel
				fixed4 texel = tex2Dproj (_ShadowTex, UNITY_PROJ_COORD(i.uv));			

				uv_normalized = TRANSFORM_TEX(uv_normalized, _ShadowTex);			

				// Make transparent
				float factor = 1 - i.color.a;
				texel.r = clamp(texel.r + factor, 0, 1);
				texel.g = clamp(texel.g + factor, 0, 1);
				texel.b = clamp(texel.b + factor, 0, 1);

				// Uncomment for Debugging
				// texel = float4(0,0,0,1);

				// Clip projection on the borders
				if(uv_normalized.x <= 0)
					texel = float4(1, 1, 1, 1);

				if(uv_normalized.x >= 1)
					texel = float4(1, 1, 1, 1);

				if(uv_normalized.y <= 0)
					texel = float4(1, 1, 1, 1);

				if(uv_normalized.y >= 1)
					texel = float4(1, 1, 1, 1);

				return texel;
			}
			ENDCG
		}
	}
}
