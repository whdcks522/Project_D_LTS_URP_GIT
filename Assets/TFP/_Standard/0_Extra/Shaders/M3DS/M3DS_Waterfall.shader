// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "M3DS/Waterfall"
{
	Properties
	{
		_TessPhongStrength( "Phong Tess Strength", Range( 0, 1 ) ) = 0.5
		_MainTile("Main Tile", Float) = 1
		_Speed("Speed", Float) = 3
		_MainTex("Main Tex", 2D) = "white" {}
		_MainTexInfulence("Main Tex Infulence", Range( 0 , 1)) = 0.5
		_NormalMap("Normal Map", 2D) = "bump" {}
		_HeightMap("Height Map", 2D) = "white" {}
		_DisplacementTileAdjust("Displacement Tile Adjust", Range( 0 , 1)) = 0.5
		_WaveHeight("Wave Height", Float) = 2
		_Alpha("Alpha", Range( 0 , 1)) = 1
		_EdgeFadeDistance("Edge Fade Distance", Range( 0 , 1)) = 0.2
		_VertexFadeAmount("Vertex Fade Amount", Range( 0 , 2)) = 1
		_RefractionStrength("Refraction Strength", Range( 0 , 1)) = 0.5
		_Tiling("Tiling", Vector) = (1,1,0,0)
		_Direction1("Direction 1", Vector) = (0,1,0,0)
		_Direction2("Direction 2", Vector) = (0,1,0,0)
		_TesselationAmount("Tesselation Amount", Range( 0.01 , 1)) = 0.5
		[Header(Translucency)]
		_Translucency("Strength", Range( 0 , 50)) = 1
		_TransNormalDistortion("Normal Distortion", Range( 0 , 1)) = 0.1
		_TransScattering("Scaterring Falloff", Range( 1 , 50)) = 2
		_TransDirect("Direct", Range( 0 , 1)) = 1
		_TransAmbient("Ambient", Range( 0 , 1)) = 0.2
		_TransShadow("Shadow", Range( 0 , 1)) = 0.9
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" }
		Cull Back
		GrabPass{ }
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
		#include "UnityCG.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Tessellation.cginc"
		#include "Lighting.cginc"
		#pragma target 4.6
		#if defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
		#define ASE_DECLARE_SCREENSPACE_TEXTURE(tex) UNITY_DECLARE_SCREENSPACE_TEXTURE(tex);
		#else
		#define ASE_DECLARE_SCREENSPACE_TEXTURE(tex) UNITY_DECLARE_SCREENSPACE_TEXTURE(tex)
		#endif
		struct Input
		{
			float2 uv_texcoord;
			float3 worldPos;
			float4 screenPos;
			float4 vertexColor : COLOR;
		};

		struct SurfaceOutputStandardCustom
		{
			half3 Albedo;
			half3 Normal;
			half3 Emission;
			half Metallic;
			half Smoothness;
			half Occlusion;
			half Alpha;
			half3 Translucency;
		};

		uniform sampler2D _HeightMap;
		uniform float _Speed;
		uniform float2 _Direction1;
		uniform float2 _Tiling;
		uniform float _MainTile;
		uniform float _DisplacementTileAdjust;
		uniform float2 _Direction2;
		uniform float _WaveHeight;
		uniform sampler2D _NormalMap;
		uniform sampler2D _MainTex;
		ASE_DECLARE_SCREENSPACE_TEXTURE( _GrabTexture )
		uniform float _RefractionStrength;
		UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
		uniform float4 _CameraDepthTexture_TexelSize;
		uniform float _MainTexInfulence;
		uniform half _Translucency;
		uniform half _TransNormalDistortion;
		uniform half _TransScattering;
		uniform half _TransDirect;
		uniform half _TransAmbient;
		uniform half _TransShadow;
		uniform float _Alpha;
		uniform float _VertexFadeAmount;
		uniform float _EdgeFadeDistance;
		uniform float _TesselationAmount;
		uniform float _TessPhongStrength;

		float4 tessFunction( appdata_full v0, appdata_full v1, appdata_full v2 )
		{
			return UnityDistanceBasedTess( v0.vertex, v1.vertex, v2.vertex, 10.0,50.0,( _TesselationAmount * 50.0 ));
		}

		void vertexDataFunc( inout appdata_full v )
		{
			float WaveSpeed19 = ( _Time.y * _Speed * 0.5 );
			float2 WaveDirection127 = _Direction1;
			float2 uv_TexCoord138 = v.texcoord.xy * _Tiling;
			float2 WorldSpaceTile7 = uv_TexCoord138;
			float2 WaveTileUV22 = ( ( WorldSpaceTile7 * float2( 1,1 ) * 0.1 ) * _MainTile );
			float2 panner56 = ( WaveSpeed19 * WaveDirection127 + WaveTileUV22);
			float2 Panner1146 = panner56;
			float2 WaveDirection231 = _Direction2;
			float2 panner55 = ( WaveSpeed19 * WaveDirection231 + ( WaveTileUV22 * 0.9 ));
			float2 Panner2147 = panner55;
			float4 Movement64 = ( tex2Dlod( _HeightMap, float4( ( Panner1146 * _DisplacementTileAdjust ), 0, 0.0) ) + tex2Dlod( _HeightMap, float4( ( Panner2147 * _DisplacementTileAdjust ), 0, 0.0) ) );
			float4 Wave121 = Movement64;
			v.vertex.xyz += ( Wave121 * _WaveHeight * 0.1 ).rgb;
			v.vertex.w = 1;
		}

		inline half4 LightingStandardCustom(SurfaceOutputStandardCustom s, half3 viewDir, UnityGI gi )
		{
			#if !defined(DIRECTIONAL)
			float3 lightAtten = gi.light.color;
			#else
			float3 lightAtten = lerp( _LightColor0.rgb, gi.light.color, _TransShadow );
			#endif
			half3 lightDir = gi.light.dir + s.Normal * _TransNormalDistortion;
			half transVdotL = pow( saturate( dot( viewDir, -lightDir ) ), _TransScattering );
			half3 translucency = lightAtten * (transVdotL * _TransDirect + gi.indirect.diffuse * _TransAmbient) * s.Translucency;
			half4 c = half4( s.Albedo * translucency * _Translucency, 0 );

			SurfaceOutputStandard r;
			r.Albedo = s.Albedo;
			r.Normal = s.Normal;
			r.Emission = s.Emission;
			r.Metallic = s.Metallic;
			r.Smoothness = s.Smoothness;
			r.Occlusion = s.Occlusion;
			r.Alpha = s.Alpha;
			return LightingStandard (r, viewDir, gi) + c;
		}

		inline void LightingStandardCustom_GI(SurfaceOutputStandardCustom s, UnityGIInput data, inout UnityGI gi )
		{
			#if defined(UNITY_PASS_DEFERRED) && UNITY_ENABLE_REFLECTION_BUFFERS
				gi = UnityGlobalIllumination(data, s.Occlusion, s.Normal);
			#else
				UNITY_GLOSSY_ENV_FROM_SURFACE( g, s, data );
				gi = UnityGlobalIllumination( data, s.Occlusion, s.Normal, g );
			#endif
		}

		void surf( Input i , inout SurfaceOutputStandardCustom o )
		{
			float WaveSpeed19 = ( _Time.y * _Speed * 0.5 );
			float2 WaveDirection127 = _Direction1;
			float2 uv_TexCoord138 = i.uv_texcoord * _Tiling;
			float2 WorldSpaceTile7 = uv_TexCoord138;
			float2 WaveTileUV22 = ( ( WorldSpaceTile7 * float2( 1,1 ) * 0.1 ) * _MainTile );
			float2 panner56 = ( WaveSpeed19 * WaveDirection127 + WaveTileUV22);
			float2 Panner1146 = panner56;
			float2 WaveDirection231 = _Direction2;
			float2 panner55 = ( WaveSpeed19 * WaveDirection231 + ( WaveTileUV22 * 0.9 ));
			float2 Panner2147 = panner55;
			float3 normalizeResult78 = normalize( ( UnpackNormal( tex2D( _NormalMap, Panner1146 ) ) + UnpackNormal( tex2D( _NormalMap, Panner2147 ) ) ) );
			float3 Normal82 = normalizeResult78;
			o.Normal = Normal82;
			float4 Color156 = ( tex2D( _MainTex, Panner1146 ) + tex2D( _MainTex, Panner2147 ) );
			float4 break163 = Color156;
			float4 appendResult164 = (float4(break163.r , break163.g , break163.b , 0.0));
			float4 ase_vertex4Pos = mul( unity_WorldToObject, float4( i.worldPos , 1 ) );
			float3 ase_viewPos = UnityObjectToViewPos( ase_vertex4Pos );
			float ase_screenDepth = -ase_viewPos.z;
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			float eyeDepth28_g1 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
			float2 temp_output_20_0_g1 = ( (Normal82).xy * ( _RefractionStrength / max( ase_screenDepth , 0.1 ) ) * saturate( ( eyeDepth28_g1 - ase_screenDepth ) ) );
			float eyeDepth2_g1 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ( float4( temp_output_20_0_g1, 0.0 , 0.0 ) + ase_screenPosNorm ).xy ));
			float2 temp_output_32_0_g1 = (( float4( ( temp_output_20_0_g1 * saturate( ( eyeDepth2_g1 - ase_screenDepth ) ) ), 0.0 , 0.0 ) + ase_screenPosNorm )).xy;
			float2 temp_output_1_0_g1 = ( ( floor( ( temp_output_32_0_g1 * (_CameraDepthTexture_TexelSize).zw ) ) + 0.5 ) * (_CameraDepthTexture_TexelSize).xy );
			float4 screenColor106 = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_GrabTexture,temp_output_1_0_g1);
			float4 clampResult111 = clamp( screenColor106 , float4( 0,0,0,0 ) , float4( 1,1,1,0 ) );
			float4 Refraction120 = clampResult111;
			float clampResult183 = clamp( ( ( 1.0 - _MainTexInfulence ) * break163.a ) , 0.0 , 1.0 );
			float4 lerpResult133 = lerp( appendResult164 , Refraction120 , clampResult183);
			o.Albedo = lerpResult133.rgb;
			o.Smoothness = 0.98;
			float3 temp_cast_4 = (1.0).xxx;
			o.Translucency = temp_cast_4;
			float4 clampResult167 = clamp( ( ( break163.a * _Alpha ) * ( i.vertexColor * _VertexFadeAmount ) ) , float4( 0,0,0,0 ) , float4( 1,1,1,0 ) );
			float screenDepth114 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
			float distanceDepth114 = abs( ( screenDepth114 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( _EdgeFadeDistance ) );
			float clampResult116 = clamp( distanceDepth114 , 0.0 , 1.0 );
			float Edge123 = clampResult116;
			o.Alpha = ( clampResult167 * Edge123 ).r;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf StandardCustom alpha:fade keepalpha fullforwardshadows exclude_path:deferred vertex:vertexDataFunc tessellate:tessFunction tessphong:_TessPhongStrength 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 4.6
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			sampler3D _DitherMaskLOD;
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float3 worldPos : TEXCOORD2;
				float4 screenPos : TEXCOORD3;
				float4 tSpace0 : TEXCOORD4;
				float4 tSpace1 : TEXCOORD5;
				float4 tSpace2 : TEXCOORD6;
				half4 color : COLOR0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				vertexDataFunc( v );
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				half3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				half3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				o.worldPos = worldPos;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				o.screenPos = ComputeScreenPos( o.pos );
				o.color = v.color;
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xy;
				float3 worldPos = IN.worldPos;
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.screenPos = IN.screenPos;
				surfIN.vertexColor = IN.color;
				SurfaceOutputStandardCustom o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandardCustom, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				half alphaRef = tex3D( _DitherMaskLOD, float3( vpos.xy * 0.25, o.Alpha * 0.9375 ) ).a;
				clip( alphaRef - 0.01 );
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18909
799;312;1546;929;4623.778;-473.7874;1;False;False
Node;AmplifyShaderEditor.CommentaryNode;4;-5405.426,-2519.45;Inherit;False;908.9005;241.6001;UVs;3;7;138;168;;1,1,1,1;0;0
Node;AmplifyShaderEditor.Vector2Node;168;-5333.764,-2431.603;Inherit;False;Property;_Tiling;Tiling;12;0;Create;True;0;0;0;False;0;False;1,1;1.5,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.TextureCoordinatesNode;138;-5050.437,-2408.528;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;7;-4764.527,-2409.15;Inherit;False;WorldSpaceTile;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;10;-6372.288,-1652.431;Inherit;False;Constant;_Float10;Float 10;11;0;Create;True;0;0;0;False;0;False;0.1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;9;-6387.47,-1981.901;Inherit;False;7;WorldSpaceTile;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector2Node;8;-6381.095,-1884.902;Inherit;False;Constant;_Tile;Tile;1;0;Create;True;0;0;0;False;0;False;1,1;1,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.RangedFloatNode;14;-6189.903,-1670.44;Inherit;False;Property;_MainTile;Main Tile;0;0;Create;True;0;0;0;False;0;False;1;4;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;15;-6106.793,-1894.001;Inherit;False;3;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;17;-5863.053,-1784.711;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;13;-6241.063,-418.1373;Inherit;False;Property;_Speed;Speed;1;0;Create;True;0;0;0;False;0;False;3;1.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;11;-6232.063,-533.8375;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;12;-6232.521,-304.5395;Inherit;False;Constant;_Float11;Float 11;11;0;Create;True;0;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;22;-5662.251,-1784.293;Inherit;False;WaveTileUV;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;21;-6283.497,2469.558;Inherit;False;3530.118;2817.894;Comment;43;64;60;56;55;52;51;50;48;47;45;44;41;37;34;139;140;141;142;143;144;145;146;147;148;149;150;151;152;153;154;155;156;174;175;176;177;178;179;78;82;192;191;193;Waves;1,1,1,1;0;0
Node;AmplifyShaderEditor.Vector2Node;20;-6472.175,-1150.579;Inherit;False;Property;_Direction2;Direction 2;14;0;Create;True;0;0;0;False;0;False;0,1;0.05,0.85;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;16;-5888.062,-615.8375;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;18;-6479.695,-1321.845;Inherit;False;Property;_Direction1;Direction 1;13;0;Create;True;0;0;0;False;0;False;0,1;0,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.RegisterLocalVarNode;31;-6262.175,-1143.579;Inherit;False;WaveDirection2;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;19;-5721.305,-612.3546;Inherit;False;WaveSpeed;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;41;-5634.404,3344.057;Inherit;False;Constant;_Float0;Float 0;4;0;Create;True;0;0;0;False;0;False;0.9;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;27;-6274.797,-1324.647;Inherit;False;WaveDirection1;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;37;-5671.049,3240.943;Inherit;False;22;WaveTileUV;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;48;-5650.035,3457.564;Inherit;False;31;WaveDirection2;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;44;-5557.003,2711.913;Inherit;False;19;WaveSpeed;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;50;-5396.023,3551.53;Inherit;False;19;WaveSpeed;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;47;-5576.06,2616.805;Inherit;False;27;WaveDirection1;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;45;-5557.854,2519.558;Inherit;False;22;WaveTileUV;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;51;-5389.611,3312.13;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;56;-5263.808,2599.401;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;55;-5155.327,3394.957;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TexturePropertyNode;140;-6144.036,3491.281;Inherit;True;Property;_NormalMap;Normal Map;4;0;Create;True;0;0;0;False;0;False;621e0f759c69ed64bb860fc223285aa9;621e0f759c69ed64bb860fc223285aa9;True;bump;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.RegisterLocalVarNode;142;-5907.631,3494.206;Inherit;False;NormalTexture;-1;True;1;0;SAMPLER2D;0;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;147;-4938.886,3405.249;Inherit;False;Panner2;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;146;-5048.557,2622.011;Inherit;False;Panner1;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;174;-4547.779,4266.958;Inherit;False;146;Panner1;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;176;-4546.541,4539.455;Inherit;False;147;Panner2;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;175;-4563.535,4397.55;Inherit;False;142;NormalTexture;1;0;OBJECT;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.TexturePropertyNode;139;-6146.543,3274.567;Inherit;True;Property;_MainTex;Main Tex;2;0;Create;True;0;0;0;False;0;False;c193182784e709a4aa10d88425ef33dc;c193182784e709a4aa10d88425ef33dc;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SamplerNode;177;-4252.101,4488.3;Inherit;True;Property;_TextureSample1;Texture Sample 1;16;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;178;-4253.689,4232.08;Inherit;True;Property;_TextureSample4;Texture Sample 4;16;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;143;-5922.631,3273.206;Inherit;False;DiffuseTexture;-1;True;1;0;SAMPLER2D;0;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.GetLocalVarNode;151;-4511.285,3557.951;Inherit;False;146;Panner1;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;179;-3766.433,4384.408;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TexturePropertyNode;141;-6144.037,3714.199;Inherit;True;Property;_HeightMap;Height Map;5;0;Create;True;0;0;0;False;0;False;d6e909a2aa630234eb9b105d3a66f903;d6e909a2aa630234eb9b105d3a66f903;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.GetLocalVarNode;150;-4527.041,3688.543;Inherit;False;143;DiffuseTexture;1;0;OBJECT;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.GetLocalVarNode;152;-4510.047,3830.448;Inherit;False;147;Panner2;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;192;-4901.371,2959.536;Inherit;False;Property;_DisplacementTileAdjust;Displacement Tile Adjust;6;0;Create;True;0;0;0;False;0;False;0.5;0.4;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;149;-4496.741,3044.364;Inherit;False;147;Panner2;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;34;-5902.501,3715.737;Inherit;False;HeightTexture;-1;True;1;0;SAMPLER2D;0;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.SamplerNode;154;-4215.607,3779.293;Inherit;True;Property;_TextureSample2;Texture Sample 2;16;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;153;-4217.196,3523.073;Inherit;True;Property;_TextureSample3;Texture Sample 3;16;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.NormalizeNode;78;-3349.311,4382.06;Inherit;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;148;-4497.979,2771.867;Inherit;False;146;Panner1;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;155;-3729.941,3675.401;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;191;-4365.884,2629.748;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;83;-4406.677,683.5999;Inherit;False;1805.239;1040.284;Comment;11;120;111;106;99;93;91;89;87;86;85;194;Refraction;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;82;-3172.128,4377.129;Inherit;False;Normal;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;52;-4513.735,2902.459;Inherit;False;34;HeightTexture;1;0;OBJECT;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;193;-4359.371,3170.536;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;156;-3555.135,3670.269;Inherit;False;Color;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;100;-1524.722,-2282.682;Inherit;False;1133.572;301.2458;Comment;4;123;116;114;105;Edge Fade;1,1,1,1;0;0
Node;AmplifyShaderEditor.SamplerNode;144;-4203.891,2736.989;Inherit;True;Property;_TextureSample24;Texture Sample 24;16;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;145;-4202.302,2993.209;Inherit;True;Property;_TextureSample0;Texture Sample 0;16;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;85;-4356.677,1047.534;Inherit;False;Property;_RefractionStrength;Refraction Strength;11;0;Create;True;0;0;0;False;0;False;0.5;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;86;-4318.067,923.1797;Inherit;False;82;Normal;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;105;-1481.368,-2144.07;Inherit;False;Property;_EdgeFadeDistance;Edge Fade Distance;9;0;Create;True;0;0;0;False;0;False;0.2;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;60;-3716.635,2889.317;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;157;-1525.835,-226.8626;Inherit;False;156;Color;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;194;-3937.778,1077.787;Inherit;False;DepthMaskedRefraction;-1;;1;c805f061214177c42bca056464193f81;2,40,0,103,0;2;35;FLOAT3;0,0,0;False;37;FLOAT;0.02;False;1;FLOAT2;38
Node;AmplifyShaderEditor.BreakToComponentsNode;163;-1224.584,-219.0626;Inherit;False;COLOR;1;0;COLOR;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.VertexColorNode;158;-1467.652,102.0541;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DepthFade;114;-1166.368,-2161.07;Inherit;False;True;False;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScreenColorNode;106;-3598.457,831.329;Inherit;False;Global;_GrabScreen0;Grab Screen 0;13;0;Create;True;0;0;0;False;0;False;Object;-1;False;False;False;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;172;-1571.383,320.2792;Inherit;False;Property;_VertexFadeAmount;Vertex Fade Amount;10;0;Create;True;0;0;0;False;0;False;1;1;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;185;-1569.226,-522.0358;Inherit;False;Property;_MainTexInfulence;Main Tex Infulence;3;0;Create;True;0;0;0;False;0;False;0.5;0.75;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;170;-1590.383,-67.72083;Inherit;False;Property;_Alpha;Alpha;8;0;Create;True;0;0;0;False;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;64;-3457.718,2882.016;Inherit;False;Movement;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;92;-3643.027,-1924.722;Inherit;False;904.627;399.396;Comment;6;121;113;104;102;101;98;Wave;1,1,1,1;0;0
Node;AmplifyShaderEditor.OneMinusNode;188;-1282.181,-514.9811;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;102;-3350.187,-1641.326;Inherit;False;64;Movement;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;111;-3404.247,836.467;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,1,1,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;171;-1205.383,194.2792;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;169;-1079.383,-91.72083;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;116;-844.3677,-2176.07;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;186;-1155.826,-404.5358;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;120;-3225.45,852.908;Inherit;False;Refraction;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;165;-958.5842,82.93738;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;118;-1185.243,735.6415;Inherit;False;Constant;_Float16;Float 16;14;0;Create;True;0;0;0;False;0;False;50;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;119;-1239.549,636.6519;Inherit;False;Property;_TesselationAmount;Tesselation Amount;15;0;Create;True;0;0;0;False;0;False;0.5;0.25;0.01;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;123;-596.1501,-2147.182;Inherit;False;Edge;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;121;-2962.4,-1787.295;Inherit;False;Wave;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;127;-667.004,474.4434;Inherit;False;Constant;_Float12;Float 12;15;0;Create;True;0;0;0;False;0;False;0.1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;129;-705.8895,-296.1232;Inherit;False;120;Refraction;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;132;-762.1097,206.6132;Inherit;False;123;Edge;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;130;-697.2648,300.7786;Inherit;False;121;Wave;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.DynamicAppendNode;164;-585.5842,-223.0626;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;128;-687.8395,388.7695;Inherit;False;Property;_WaveHeight;Wave Height;7;0;Create;True;0;0;0;False;0;False;2;2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;167;-766.5842,89.93738;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,1,1,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;183;-988.6248,-405.3361;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;125;-920.2432,631.6416;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;124;-1077.55,874.6518;Inherit;False;Constant;_Float2;Float 2;4;0;Create;True;0;0;0;False;0;False;10;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;131;-1021.549,1002.652;Inherit;False;Constant;_Float3;Float 3;4;0;Create;True;0;0;0;False;0;False;50;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;133;-365.0242,-330.5243;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;136;-481.8395,296.7695;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.Vector3Node;101;-3576.47,-1874.722;Inherit;False;Constant;_WaveUp;Wave Up;4;0;Create;True;0;0;0;False;0;False;0,1,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;104;-3354.449,-1807.739;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;190;-309.0115,124.0134;Inherit;False;Constant;_Float6;Float 6;16;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;134;-538.4413,102.4265;Inherit;False;Constant;_Float5;Float 5;4;0;Create;True;0;0;0;False;0;False;0.98;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;137;-550.0739,25.34797;Inherit;False;82;Normal;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ComponentMaskNode;91;-4000.756,738.3491;Inherit;False;True;True;False;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;113;-3148.285,-1782.997;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;98;-3562.449,-1615.739;Inherit;False;Constant;_Float1;Float 1;14;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DistanceBasedTessNode;135;-771.5497,896.6518;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;93;-4009.777,966.7341;Inherit;False;3;3;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;99;-3783.42,865.239;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;87;-4216.877,1148.634;Inherit;False;Constant;_Float4;Float 4;14;0;Create;True;0;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;189;-421.5365,183.7719;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GrabScreenPosition;89;-4327.817,733.5999;Inherit;False;0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;-124,-30;Float;False;True;-1;6;ASEMaterialInspector;0;0;Standard;M3DS/Waterfall;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;True;0;False;Transparent;;Transparent;ForwardOnly;16;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;True;2;15;10;25;True;0.5;True;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;16;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;138;0;168;0
WireConnection;7;0;138;0
WireConnection;15;0;9;0
WireConnection;15;1;8;0
WireConnection;15;2;10;0
WireConnection;17;0;15;0
WireConnection;17;1;14;0
WireConnection;22;0;17;0
WireConnection;16;0;11;0
WireConnection;16;1;13;0
WireConnection;16;2;12;0
WireConnection;31;0;20;0
WireConnection;19;0;16;0
WireConnection;27;0;18;0
WireConnection;51;0;37;0
WireConnection;51;1;41;0
WireConnection;56;0;45;0
WireConnection;56;2;47;0
WireConnection;56;1;44;0
WireConnection;55;0;51;0
WireConnection;55;2;48;0
WireConnection;55;1;50;0
WireConnection;142;0;140;0
WireConnection;147;0;55;0
WireConnection;146;0;56;0
WireConnection;177;0;175;0
WireConnection;177;1;176;0
WireConnection;178;0;175;0
WireConnection;178;1;174;0
WireConnection;143;0;139;0
WireConnection;179;0;178;0
WireConnection;179;1;177;0
WireConnection;34;0;141;0
WireConnection;154;0;150;0
WireConnection;154;1;152;0
WireConnection;153;0;150;0
WireConnection;153;1;151;0
WireConnection;78;0;179;0
WireConnection;155;0;153;0
WireConnection;155;1;154;0
WireConnection;191;0;148;0
WireConnection;191;1;192;0
WireConnection;82;0;78;0
WireConnection;193;0;149;0
WireConnection;193;1;192;0
WireConnection;156;0;155;0
WireConnection;144;0;52;0
WireConnection;144;1;191;0
WireConnection;145;0;52;0
WireConnection;145;1;193;0
WireConnection;60;0;144;0
WireConnection;60;1;145;0
WireConnection;194;35;86;0
WireConnection;194;37;85;0
WireConnection;163;0;157;0
WireConnection;114;0;105;0
WireConnection;106;0;194;38
WireConnection;64;0;60;0
WireConnection;188;0;185;0
WireConnection;111;0;106;0
WireConnection;171;0;158;0
WireConnection;171;1;172;0
WireConnection;169;0;163;3
WireConnection;169;1;170;0
WireConnection;116;0;114;0
WireConnection;186;0;188;0
WireConnection;186;1;163;3
WireConnection;120;0;111;0
WireConnection;165;0;169;0
WireConnection;165;1;171;0
WireConnection;123;0;116;0
WireConnection;121;0;102;0
WireConnection;164;0;163;0
WireConnection;164;1;163;1
WireConnection;164;2;163;2
WireConnection;167;0;165;0
WireConnection;183;0;186;0
WireConnection;125;0;119;0
WireConnection;125;1;118;0
WireConnection;133;0;164;0
WireConnection;133;1;129;0
WireConnection;133;2;183;0
WireConnection;136;0;130;0
WireConnection;136;1;128;0
WireConnection;136;2;127;0
WireConnection;104;0;101;0
WireConnection;104;1;98;0
WireConnection;91;0;89;0
WireConnection;113;0;104;0
WireConnection;113;1;102;0
WireConnection;135;0;125;0
WireConnection;135;1;124;0
WireConnection;135;2;131;0
WireConnection;93;0;86;0
WireConnection;93;1;85;0
WireConnection;93;2;87;0
WireConnection;99;0;91;0
WireConnection;99;1;93;0
WireConnection;189;0;167;0
WireConnection;189;1;132;0
WireConnection;0;0;133;0
WireConnection;0;1;137;0
WireConnection;0;4;134;0
WireConnection;0;7;190;0
WireConnection;0;9;189;0
WireConnection;0;11;136;0
WireConnection;0;14;135;0
ASEEND*/
//CHKSM=73DE29EF48C03D9DEA034F92D4EFB3C9A7805609