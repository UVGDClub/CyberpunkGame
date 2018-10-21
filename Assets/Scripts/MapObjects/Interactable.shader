Shader "SpriteShaders/Interactable"
{
	

	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_FlashColor ("_FlashColor", Color) = (1,1,1,1)
		_OutlineColor ("_OutlineColor", Color) = (1,1,1,1)
		_FlashMagnitude("_FlashMagnitude", Range(0,1)) = 0
		_SpriteWidth("_SpriteWidth", Int) = 128
	}
	SubShader
	{
		Blend SrcAlpha OneMinusSrcAlpha
		Cull Off
		Lighting Off
		ZWrite Off

		Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
        }

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _MainTex_TexelSize;
			fixed4 _FlashColor;
			float _FlashMagnitude;
			int _SpriteWidth;
			float4 _OutlineColor;

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				fixed4 color : COLOR;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				fixed4 color : COLOR;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.color = v.color;
				return o;
			}
			

			fixed4 frag (v2f i) : SV_Target
			{
				float timeFactor = _Time.y;
				fixed4 col = tex2D(_MainTex, i.uv);

				col.rgb *= i.color;
				float tempAlpha = col.a;
				col = lerp( col, _FlashColor, _FlashMagnitude);
				col.a = tempAlpha;
				if(col.a != 0)
				{
					fixed4 pixelAbove = tex2D(_MainTex, i.uv+fixed2(0, _MainTex_TexelSize.y));
					fixed4 pixelAbove2 = tex2D(_MainTex, i.uv+fixed2(0, _MainTex_TexelSize.y*2));
					fixed4 pixelBelow = tex2D(_MainTex, i.uv-fixed2(0, _MainTex_TexelSize.y));
					fixed4 pixelBelow2 = tex2D(_MainTex, i.uv-fixed2(0, _MainTex_TexelSize.y*2));

					fixed4 pixelRight = tex2D(_MainTex, i.uv+fixed2(_MainTex_TexelSize.x, 0));
					fixed4 pixelRight2 = tex2D(_MainTex, i.uv+fixed2(_MainTex_TexelSize.x*2, 0));


					fixed4 pixelLeft = tex2D(_MainTex, i.uv-fixed2(_MainTex_TexelSize.x, 0));
					fixed4 pixelLeft2 = tex2D(_MainTex, i.uv-fixed2(_MainTex_TexelSize.x*2, 0));

					float2 mid = float2(0.5,0.5);
					float truex = (i.uv.x*_MainTex_TexelSize.z)/_SpriteWidth;
					float truey = (i.uv.y*_MainTex_TexelSize.w)/_SpriteWidth;
					float2 truexy = float2(truex,truey);
					float2 localcoords = fmod(truexy, 1);
					float2 midcoords = localcoords-mid;
					float angle = atan2(midcoords.y,midcoords.x);
					if (angle <= 0){ angle += 6.28; }
					float wrapAngle = fmod(angle+timeFactor, 0.4);

					float adjacencyFinder = pixelAbove.a * pixelBelow.a * pixelRight.a * pixelLeft.a * pixelAbove2.a * pixelBelow2.a * pixelRight2.a * pixelLeft2.a;

					if(adjacencyFinder == 0 && wrapAngle >= 0.2 )
					{
						col = lerp( col, _OutlineColor, 0.5);
					}
				}

				//col.gb = 0;
				col += fixed4(0.1,0.1,0.1,0)*_OutlineColor;
				return col;
			}
			ENDCG
		}
	}
}
