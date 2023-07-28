// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "M3DS/Water"
{
	Properties
	{
		[HideInInspector] _EmissionColor("Emission Color", Color) = (1,1,1,1)
		[HideInInspector] _AlphaCutoff("Alpha Cutoff ", Range(0, 1)) = 0.5
		[ASEBegin]_ShallowColor("Shallow Color", Color) = (0.2736739,0.4617023,0.4716981,1)
		_DeepColor("Deep Color", Color) = (0.1117391,0.2199386,0.254717,1)
		_WaveTile("Wave Tile", Float) = 1
		_WaveSpeed("Wave Speed", Float) = 1
		_WaterColorDepth("Water Color Depth", Range( 0 , 1)) = 0.5
		_EdgeFadeDistance("Edge Fade Distance", Range( 0 , 1)) = 0.2
		_VisibilityDepth("Visibility Depth", Range( 0 , 1)) = 0.5
		_NormalStrength("Normal Strength", Range( 0 , 2)) = 1.5
		_RefractionStrength("Refraction Strength", Range( 0 , 1)) = 0.5
		_HeightMap("Height Map", 2D) = "gray" {}
		_WaveHeight("Wave Height", Float) = 0
		_WaveSizeBalance("Wave Size Balance", Range( 0 , 1)) = 0.5
		_SmallWaveStrength("Small Wave Strength", Float) = 1
		_LargeWaveStrength("Large Wave Strength", Float) = 1
		_Wave1Direction("Wave 1 Direction", Vector) = (0,1,0,0)
		[ASEEnd]_Wave2Direction("Wave 2 Direction", Vector) = (0,1,0,0)

		//_TransmissionShadow( "Transmission Shadow", Range( 0, 1 ) ) = 0.5
		//_TransStrength( "Trans Strength", Range( 0, 50 ) ) = 1
		//_TransNormal( "Trans Normal Distortion", Range( 0, 1 ) ) = 0.5
		//_TransScattering( "Trans Scattering", Range( 1, 50 ) ) = 2
		//_TransDirect( "Trans Direct", Range( 0, 1 ) ) = 0.9
		//_TransAmbient( "Trans Ambient", Range( 0, 1 ) ) = 0.1
		//_TransShadow( "Trans Shadow", Range( 0, 1 ) ) = 0.5
		//_TessPhongStrength( "Tess Phong Strength", Range( 0, 1 ) ) = 0.5
		_TessValue( "Max Tessellation", Range( 1, 32 ) ) = 16
		_TessMin( "Tess Min Distance", Float ) = 10
		_TessMax( "Tess Max Distance", Float ) = 25
		//_TessEdgeLength ( "Tess Edge length", Range( 2, 50 ) ) = 16
		//_TessMaxDisp( "Tess Max Displacement", Float ) = 25
	}

	SubShader
	{
		LOD 0

		

		Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent" }
		Cull Off
		AlphaToMask Off
		HLSLINCLUDE
		#pragma target 2.0

		#ifndef ASE_TESS_FUNCS
		#define ASE_TESS_FUNCS
		float4 FixedTess( float tessValue )
		{
			return tessValue;
		}
		
		float CalcDistanceTessFactor (float4 vertex, float minDist, float maxDist, float tess, float4x4 o2w, float3 cameraPos )
		{
			float3 wpos = mul(o2w,vertex).xyz;
			float dist = distance (wpos, cameraPos);
			float f = clamp(1.0 - (dist - minDist) / (maxDist - minDist), 0.01, 1.0) * tess;
			return f;
		}

		float4 CalcTriEdgeTessFactors (float3 triVertexFactors)
		{
			float4 tess;
			tess.x = 0.5 * (triVertexFactors.y + triVertexFactors.z);
			tess.y = 0.5 * (triVertexFactors.x + triVertexFactors.z);
			tess.z = 0.5 * (triVertexFactors.x + triVertexFactors.y);
			tess.w = (triVertexFactors.x + triVertexFactors.y + triVertexFactors.z) / 3.0f;
			return tess;
		}

		float CalcEdgeTessFactor (float3 wpos0, float3 wpos1, float edgeLen, float3 cameraPos, float4 scParams )
		{
			float dist = distance (0.5 * (wpos0+wpos1), cameraPos);
			float len = distance(wpos0, wpos1);
			float f = max(len * scParams.y / (edgeLen * dist), 1.0);
			return f;
		}

		float DistanceFromPlane (float3 pos, float4 plane)
		{
			float d = dot (float4(pos,1.0f), plane);
			return d;
		}

		bool WorldViewFrustumCull (float3 wpos0, float3 wpos1, float3 wpos2, float cullEps, float4 planes[6] )
		{
			float4 planeTest;
			planeTest.x = (( DistanceFromPlane(wpos0, planes[0]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos1, planes[0]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos2, planes[0]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.y = (( DistanceFromPlane(wpos0, planes[1]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos1, planes[1]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos2, planes[1]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.z = (( DistanceFromPlane(wpos0, planes[2]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos1, planes[2]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos2, planes[2]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.w = (( DistanceFromPlane(wpos0, planes[3]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos1, planes[3]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos2, planes[3]) > -cullEps) ? 1.0f : 0.0f );
			return !all (planeTest);
		}

		float4 DistanceBasedTess( float4 v0, float4 v1, float4 v2, float tess, float minDist, float maxDist, float4x4 o2w, float3 cameraPos )
		{
			float3 f;
			f.x = CalcDistanceTessFactor (v0,minDist,maxDist,tess,o2w,cameraPos);
			f.y = CalcDistanceTessFactor (v1,minDist,maxDist,tess,o2w,cameraPos);
			f.z = CalcDistanceTessFactor (v2,minDist,maxDist,tess,o2w,cameraPos);

			return CalcTriEdgeTessFactors (f);
		}

		float4 EdgeLengthBasedTess( float4 v0, float4 v1, float4 v2, float edgeLength, float4x4 o2w, float3 cameraPos, float4 scParams )
		{
			float3 pos0 = mul(o2w,v0).xyz;
			float3 pos1 = mul(o2w,v1).xyz;
			float3 pos2 = mul(o2w,v2).xyz;
			float4 tess;
			tess.x = CalcEdgeTessFactor (pos1, pos2, edgeLength, cameraPos, scParams);
			tess.y = CalcEdgeTessFactor (pos2, pos0, edgeLength, cameraPos, scParams);
			tess.z = CalcEdgeTessFactor (pos0, pos1, edgeLength, cameraPos, scParams);
			tess.w = (tess.x + tess.y + tess.z) / 3.0f;
			return tess;
		}

		float4 EdgeLengthBasedTessCull( float4 v0, float4 v1, float4 v2, float edgeLength, float maxDisplacement, float4x4 o2w, float3 cameraPos, float4 scParams, float4 planes[6] )
		{
			float3 pos0 = mul(o2w,v0).xyz;
			float3 pos1 = mul(o2w,v1).xyz;
			float3 pos2 = mul(o2w,v2).xyz;
			float4 tess;

			if (WorldViewFrustumCull(pos0, pos1, pos2, maxDisplacement, planes))
			{
				tess = 0.0f;
			}
			else
			{
				tess.x = CalcEdgeTessFactor (pos1, pos2, edgeLength, cameraPos, scParams);
				tess.y = CalcEdgeTessFactor (pos2, pos0, edgeLength, cameraPos, scParams);
				tess.z = CalcEdgeTessFactor (pos0, pos1, edgeLength, cameraPos, scParams);
				tess.w = (tess.x + tess.y + tess.z) / 3.0f;
			}
			return tess;
		}
		#endif //ASE_TESS_FUNCS
		ENDHLSL

		
		Pass
		{
			
			Name "Forward"
			Tags { "LightMode"="UniversalForward" }
			
			Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
			ZWrite Off
			ZTest LEqual
			Offset 0 , 0
			ColorMask RGBA
			

			HLSLPROGRAM
			#define _NORMAL_DROPOFF_TS 1
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define TESSELLATION_ON 1
			#pragma require tessellation tessHW
			#pragma hull HullFunction
			#pragma domain DomainFunction
			#define ASE_DISTANCE_TESSELLATION
			#define _NORMALMAP 1
			#define ASE_SRP_VERSION 70501
			#define REQUIRE_DEPTH_TEXTURE 1
			#define REQUIRE_OPAQUE_TEXTURE 1

			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x

			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
			#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
			#pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
			#pragma multi_compile _ _SHADOWS_SOFT
			#pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
			
			#pragma multi_compile _ DIRLIGHTMAP_COMBINED
			#pragma multi_compile _ LIGHTMAP_ON

			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS_FORWARD

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			
			#if ASE_SRP_VERSION <= 70108
			#define REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR
			#endif

			#if defined(UNITY_INSTANCING_ENABLED) && defined(_TERRAIN_INSTANCED_PERPIXEL_NORMAL)
			    #define ENABLE_TERRAIN_PERPIXEL_NORMAL
			#endif

			#define ASE_NEEDS_FRAG_SCREEN_POSITION
			#define ASE_NEEDS_FRAG_WORLD_POSITION
			#define ASE_NEEDS_FRAG_WORLD_NORMAL
			#define ASE_NEEDS_FRAG_WORLD_TANGENT
			#define ASE_NEEDS_FRAG_WORLD_BITANGENT
			#define ASE_NEEDS_VERT_POSITION


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_tangent : TANGENT;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord : TEXCOORD0;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				float4 lightmapUVOrVertexSH : TEXCOORD0;
				half4 fogFactorAndVertexLight : TEXCOORD1;
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
				float4 shadowCoord : TEXCOORD2;
				#endif
				float4 tSpace0 : TEXCOORD3;
				float4 tSpace1 : TEXCOORD4;
				float4 tSpace2 : TEXCOORD5;
				#if defined(ASE_NEEDS_FRAG_SCREEN_POSITION)
				float4 screenPos : TEXCOORD6;
				#endif
				float4 ase_texcoord7 : TEXCOORD7;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _DeepColor;
			float4 _ShallowColor;
			float2 _Wave1Direction;
			float2 _Wave2Direction;
			float _WaveSpeed;
			float _WaveTile;
			float _SmallWaveStrength;
			float _LargeWaveStrength;
			float _WaveSizeBalance;
			float _WaveHeight;
			float _WaterColorDepth;
			float _NormalStrength;
			float _RefractionStrength;
			float _VisibilityDepth;
			float _EdgeFadeDistance;
			#ifdef _TRANSMISSION_ASE
				float _TransmissionShadow;
			#endif
			#ifdef _TRANSLUCENCY_ASE
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef TESSELLATION_ON
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END
			sampler2D _HeightMap;
			uniform float4 _CameraDepthTexture_TexelSize;


			void StochasticTiling( float2 UV, out float2 UV1, out float2 UV2, out float2 UV3, out float W1, out float W2, out float W3 )
			{
				float2 vertex1, vertex2, vertex3;
				// Scaling of the input
				float2 uv = UV * 3.464; // 2 * sqrt (3)
				// Skew input space into simplex triangle grid
				const float2x2 gridToSkewedGrid = float2x2( 1.0, 0.0, -0.57735027, 1.15470054 );
				float2 skewedCoord = mul( gridToSkewedGrid, uv );
				// Compute local triangle vertex IDs and local barycentric coordinates
				int2 baseId = int2( floor( skewedCoord ) );
				float3 temp = float3( frac( skewedCoord ), 0 );
				temp.z = 1.0 - temp.x - temp.y;
				if ( temp.z > 0.0 )
				{
					W1 = temp.z;
					W2 = temp.y;
					W3 = temp.x;
					vertex1 = baseId;
					vertex2 = baseId + int2( 0, 1 );
					vertex3 = baseId + int2( 1, 0 );
				}
				else
				{
					W1 = -temp.z;
					W2 = 1.0 - temp.y;
					W3 = 1.0 - temp.x;
					vertex1 = baseId + int2( 1, 1 );
					vertex2 = baseId + int2( 1, 0 );
					vertex3 = baseId + int2( 0, 1 );
				}
				UV1 = UV + frac( sin( mul( float2x2( 127.1, 311.7, 269.5, 183.3 ), vertex1 ) ) * 43758.5453 );
				UV2 = UV + frac( sin( mul( float2x2( 127.1, 311.7, 269.5, 183.3 ), vertex2 ) ) * 43758.5453 );
				UV3 = UV + frac( sin( mul( float2x2( 127.1, 311.7, 269.5, 183.3 ), vertex3 ) ) * 43758.5453 );
				return;
			}
			
			float3 PerturbNormal107_g25( float3 surf_pos, float3 surf_norm, float height, float scale )
			{
				// "Bump Mapping Unparametrized Surfaces on the GPU" by Morten S. Mikkelsen
				float3 vSigmaS = ddx( surf_pos );
				float3 vSigmaT = ddy( surf_pos );
				float3 vN = surf_norm;
				float3 vR1 = cross( vSigmaT , vN );
				float3 vR2 = cross( vN , vSigmaS );
				float fDet = dot( vSigmaS , vR1 );
				float dBs = ddx( height );
				float dBt = ddy( height );
				float3 vSurfGrad = scale * 0.05 * sign( fDet ) * ( dBs * vR1 + dBt * vR2 );
				return normalize ( abs( fDet ) * vN - vSurfGrad );
			}
			

			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				float localStochasticTiling2_g24 = ( 0.0 );
				float WaveSpeed43 = ( _TimeParameters.x * _WaveSpeed * 0.1 );
				float2 WaveDirection140 = _Wave1Direction;
				float3 ase_worldPos = mul(GetObjectToWorldMatrix(), v.vertex).xyz;
				float4 appendResult10 = (float4(ase_worldPos.x , ase_worldPos.z , 0.0 , 0.0));
				float4 WorldSpaceTile11 = appendResult10;
				float4 WaveTileUV38 = ( ( WorldSpaceTile11 * float4( float2( 1,1 ), 0.0 , 0.0 ) * 0.1 ) * _WaveTile );
				float2 panner4 = ( WaveSpeed43 * WaveDirection140 + WaveTileUV38.xy);
				float2 Input_UV145_g24 = panner4;
				float2 UV2_g24 = Input_UV145_g24;
				float2 UV12_g24 = float2( 0,0 );
				float2 UV22_g24 = float2( 0,0 );
				float2 UV32_g24 = float2( 0,0 );
				float W12_g24 = 0.0;
				float W22_g24 = 0.0;
				float W32_g24 = 0.0;
				StochasticTiling( UV2_g24 , UV12_g24 , UV22_g24 , UV32_g24 , W12_g24 , W22_g24 , W32_g24 );
				float4 Output_2D293_g24 = ( ( tex2Dlod( _HeightMap, float4( UV12_g24, 0, 0.0) ) * W12_g24 ) + ( tex2Dlod( _HeightMap, float4( UV22_g24, 0, 0.0) ) * W22_g24 ) + ( tex2Dlod( _HeightMap, float4( UV32_g24, 0, 0.0) ) * W32_g24 ) );
				float4 break31_g24 = Output_2D293_g24;
				float localStochasticTiling2_g23 = ( 0.0 );
				float2 WaveDirection2201 = _Wave2Direction;
				float2 panner28 = ( WaveSpeed43 * WaveDirection2201 + ( WaveTileUV38 * 0.9 ).xy);
				float2 Input_UV145_g23 = panner28;
				float2 UV2_g23 = Input_UV145_g23;
				float2 UV12_g23 = float2( 0,0 );
				float2 UV22_g23 = float2( 0,0 );
				float2 UV32_g23 = float2( 0,0 );
				float W12_g23 = 0.0;
				float W22_g23 = 0.0;
				float W32_g23 = 0.0;
				StochasticTiling( UV2_g23 , UV12_g23 , UV22_g23 , UV32_g23 , W12_g23 , W22_g23 , W32_g23 );
				float4 Output_2D293_g23 = ( ( tex2Dlod( _HeightMap, float4( UV12_g23, 0, 0.0) ) * W12_g23 ) + ( tex2Dlod( _HeightMap, float4( UV22_g23, 0, 0.0) ) * W22_g23 ) + ( tex2Dlod( _HeightMap, float4( UV32_g23, 0, 0.0) ) * W32_g23 ) );
				float4 break31_g23 = Output_2D293_g23;
				float FineWaves100 = ( break31_g24.g + break31_g23.g );
				float localStochasticTiling2_g22 = ( 0.0 );
				float2 panner78 = ( ( WaveSpeed43 * 0.15 ) * WaveDirection140 + ( WaveTileUV38 * 0.1 ).xy);
				float2 Input_UV145_g22 = panner78;
				float2 UV2_g22 = Input_UV145_g22;
				float2 UV12_g22 = float2( 0,0 );
				float2 UV22_g22 = float2( 0,0 );
				float2 UV32_g22 = float2( 0,0 );
				float W12_g22 = 0.0;
				float W22_g22 = 0.0;
				float W32_g22 = 0.0;
				StochasticTiling( UV2_g22 , UV12_g22 , UV22_g22 , UV32_g22 , W12_g22 , W22_g22 , W32_g22 );
				float4 Output_2D293_g22 = ( ( tex2Dlod( _HeightMap, float4( UV12_g22, 0, 0.0) ) * W12_g22 ) + ( tex2Dlod( _HeightMap, float4( UV22_g22, 0, 0.0) ) * W22_g22 ) + ( tex2Dlod( _HeightMap, float4( UV32_g22, 0, 0.0) ) * W32_g22 ) );
				float4 break31_g22 = Output_2D293_g22;
				float localStochasticTiling2_g21 = ( 0.0 );
				float2 panner73 = ( ( WaveSpeed43 * 0.15 ) * WaveDirection2201 + ( WaveTileUV38 * 0.09 ).xy);
				float2 Input_UV145_g21 = panner73;
				float2 UV2_g21 = Input_UV145_g21;
				float2 UV12_g21 = float2( 0,0 );
				float2 UV22_g21 = float2( 0,0 );
				float2 UV32_g21 = float2( 0,0 );
				float W12_g21 = 0.0;
				float W22_g21 = 0.0;
				float W32_g21 = 0.0;
				StochasticTiling( UV2_g21 , UV12_g21 , UV22_g21 , UV32_g21 , W12_g21 , W22_g21 , W32_g21 );
				float4 Output_2D293_g21 = ( ( tex2Dlod( _HeightMap, float4( UV12_g21, 0, 0.0) ) * W12_g21 ) + ( tex2Dlod( _HeightMap, float4( UV22_g21, 0, 0.0) ) * W22_g21 ) + ( tex2Dlod( _HeightMap, float4( UV32_g21, 0, 0.0) ) * W32_g21 ) );
				float4 break31_g21 = Output_2D293_g21;
				float LargeWaves101 = ( break31_g22.g + break31_g21.g );
				float lerpResult110 = lerp( ( FineWaves100 * _SmallWaveStrength ) , ( ( LargeWaves101 * 5.0 ) * _LargeWaveStrength ) , _WaveSizeBalance);
				float FinalWave106 = lerpResult110;
				float3 Wave47 = ( ( float3(0,1,0) * 1.0 ) * FinalWave106 );
				
				float3 objectToViewPos = TransformWorldToView(TransformObjectToWorld(v.vertex.xyz));
				float eyeDepth = -objectToViewPos.z;
				o.ase_texcoord7.x = eyeDepth;
				
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord7.yzw = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = ( Wave47 * _WaveHeight * 0.1 );
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif
				v.ase_normal = v.ase_normal;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				float3 positionVS = TransformWorldToView( positionWS );
				float4 positionCS = TransformWorldToHClip( positionWS );

				VertexNormalInputs normalInput = GetVertexNormalInputs( v.ase_normal, v.ase_tangent );

				o.tSpace0 = float4( normalInput.normalWS, positionWS.x);
				o.tSpace1 = float4( normalInput.tangentWS, positionWS.y);
				o.tSpace2 = float4( normalInput.bitangentWS, positionWS.z);

				OUTPUT_LIGHTMAP_UV( v.texcoord1, unity_LightmapST, o.lightmapUVOrVertexSH.xy );
				OUTPUT_SH( normalInput.normalWS.xyz, o.lightmapUVOrVertexSH.xyz );

				#if defined(ENABLE_TERRAIN_PERPIXEL_NORMAL)
					o.lightmapUVOrVertexSH.zw = v.texcoord;
					o.lightmapUVOrVertexSH.xy = v.texcoord * unity_LightmapST.xy + unity_LightmapST.zw;
				#endif

				half3 vertexLight = VertexLighting( positionWS, normalInput.normalWS );
				#ifdef ASE_FOG
					half fogFactor = ComputeFogFactor( positionCS.z );
				#else
					half fogFactor = 0;
				#endif
				o.fogFactorAndVertexLight = half4(fogFactor, vertexLight);
				
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
				VertexPositionInputs vertexInput = (VertexPositionInputs)0;
				vertexInput.positionWS = positionWS;
				vertexInput.positionCS = positionCS;
				o.shadowCoord = GetShadowCoord( vertexInput );
				#endif
				
				o.clipPos = positionCS;
				#if defined(ASE_NEEDS_FRAG_SCREEN_POSITION)
				o.screenPos = ComputeScreenPos(positionCS);
				#endif
				return o;
			}
			
			#if defined(TESSELLATION_ON)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				float4 ase_tangent : TANGENT;
				float4 texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				o.ase_tangent = v.ase_tangent;
				o.texcoord = v.texcoord;
				o.texcoord1 = v.texcoord1;
				
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				o.ase_tangent = patch[0].ase_tangent * bary.x + patch[1].ase_tangent * bary.y + patch[2].ase_tangent * bary.z;
				o.texcoord = patch[0].texcoord * bary.x + patch[1].texcoord * bary.y + patch[2].texcoord * bary.z;
				o.texcoord1 = patch[0].texcoord1 * bary.x + patch[1].texcoord1 * bary.y + patch[2].texcoord1 * bary.z;
				
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			#if defined(ASE_EARLY_Z_DEPTH_OPTIMIZE)
				#define ASE_SV_DEPTH SV_DepthLessEqual  
			#else
				#define ASE_SV_DEPTH SV_Depth
			#endif

			half4 frag ( VertexOutput IN 
						#ifdef ASE_DEPTH_WRITE_ON
						,out float outputDepth : ASE_SV_DEPTH
						#endif
						 ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);

				#ifdef LOD_FADE_CROSSFADE
					LODDitheringTransition( IN.clipPos.xyz, unity_LODFade.x );
				#endif

				#if defined(ENABLE_TERRAIN_PERPIXEL_NORMAL)
					float2 sampleCoords = (IN.lightmapUVOrVertexSH.zw / _TerrainHeightmapRecipSize.zw + 0.5f) * _TerrainHeightmapRecipSize.xy;
					float3 WorldNormal = TransformObjectToWorldNormal(normalize(SAMPLE_TEXTURE2D(_TerrainNormalmapTexture, sampler_TerrainNormalmapTexture, sampleCoords).rgb * 2 - 1));
					float3 WorldTangent = -cross(GetObjectToWorldMatrix()._13_23_33, WorldNormal);
					float3 WorldBiTangent = cross(WorldNormal, -WorldTangent);
				#else
					float3 WorldNormal = normalize( IN.tSpace0.xyz );
					float3 WorldTangent = IN.tSpace1.xyz;
					float3 WorldBiTangent = IN.tSpace2.xyz;
				#endif
				float3 WorldPosition = float3(IN.tSpace0.w,IN.tSpace1.w,IN.tSpace2.w);
				float3 WorldViewDirection = _WorldSpaceCameraPos.xyz  - WorldPosition;
				float4 ShadowCoords = float4( 0, 0, 0, 0 );
				#if defined(ASE_NEEDS_FRAG_SCREEN_POSITION)
				float4 ScreenPos = IN.screenPos;
				#endif

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
					ShadowCoords = IN.shadowCoord;
				#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
					ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
				#endif
	
				WorldViewDirection = SafeNormalize( WorldViewDirection );

				float4 ase_screenPosNorm = ScreenPos / ScreenPos.w;
				ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
				float screenDepth180 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( ase_screenPosNorm.xy ),_ZBufferParams);
				float distanceDepth180 = abs( ( screenDepth180 - LinearEyeDepth( ase_screenPosNorm.z,_ZBufferParams ) ) / ( ( _WaterColorDepth * -10.0 ) ) );
				float clampResult182 = clamp( ( 1.0 - distanceDepth180 ) , 0.0 , 1.0 );
				float4 lerpResult61 = lerp( _DeepColor , _ShallowColor , clampResult182);
				float4 Albedo165 = lerpResult61;
				float3 surf_pos107_g25 = WorldPosition;
				float3 surf_norm107_g25 = WorldNormal;
				float localStochasticTiling2_g24 = ( 0.0 );
				float WaveSpeed43 = ( _TimeParameters.x * _WaveSpeed * 0.1 );
				float2 WaveDirection140 = _Wave1Direction;
				float4 appendResult10 = (float4(WorldPosition.x , WorldPosition.z , 0.0 , 0.0));
				float4 WorldSpaceTile11 = appendResult10;
				float4 WaveTileUV38 = ( ( WorldSpaceTile11 * float4( float2( 1,1 ), 0.0 , 0.0 ) * 0.1 ) * _WaveTile );
				float2 panner4 = ( WaveSpeed43 * WaveDirection140 + WaveTileUV38.xy);
				float2 Input_UV145_g24 = panner4;
				float2 UV2_g24 = Input_UV145_g24;
				float2 UV12_g24 = float2( 0,0 );
				float2 UV22_g24 = float2( 0,0 );
				float2 UV32_g24 = float2( 0,0 );
				float W12_g24 = 0.0;
				float W22_g24 = 0.0;
				float W32_g24 = 0.0;
				StochasticTiling( UV2_g24 , UV12_g24 , UV22_g24 , UV32_g24 , W12_g24 , W22_g24 , W32_g24 );
				float2 temp_output_10_0_g24 = ddx( Input_UV145_g24 );
				float2 temp_output_12_0_g24 = ddy( Input_UV145_g24 );
				float4 Output_2D293_g24 = ( ( tex2D( _HeightMap, UV12_g24, temp_output_10_0_g24, temp_output_12_0_g24 ) * W12_g24 ) + ( tex2D( _HeightMap, UV22_g24, temp_output_10_0_g24, temp_output_12_0_g24 ) * W22_g24 ) + ( tex2D( _HeightMap, UV32_g24, temp_output_10_0_g24, temp_output_12_0_g24 ) * W32_g24 ) );
				float4 break31_g24 = Output_2D293_g24;
				float localStochasticTiling2_g23 = ( 0.0 );
				float2 WaveDirection2201 = _Wave2Direction;
				float2 panner28 = ( WaveSpeed43 * WaveDirection2201 + ( WaveTileUV38 * 0.9 ).xy);
				float2 Input_UV145_g23 = panner28;
				float2 UV2_g23 = Input_UV145_g23;
				float2 UV12_g23 = float2( 0,0 );
				float2 UV22_g23 = float2( 0,0 );
				float2 UV32_g23 = float2( 0,0 );
				float W12_g23 = 0.0;
				float W22_g23 = 0.0;
				float W32_g23 = 0.0;
				StochasticTiling( UV2_g23 , UV12_g23 , UV22_g23 , UV32_g23 , W12_g23 , W22_g23 , W32_g23 );
				float2 temp_output_10_0_g23 = ddx( Input_UV145_g23 );
				float2 temp_output_12_0_g23 = ddy( Input_UV145_g23 );
				float4 Output_2D293_g23 = ( ( tex2D( _HeightMap, UV12_g23, temp_output_10_0_g23, temp_output_12_0_g23 ) * W12_g23 ) + ( tex2D( _HeightMap, UV22_g23, temp_output_10_0_g23, temp_output_12_0_g23 ) * W22_g23 ) + ( tex2D( _HeightMap, UV32_g23, temp_output_10_0_g23, temp_output_12_0_g23 ) * W32_g23 ) );
				float4 break31_g23 = Output_2D293_g23;
				float FineWaves100 = ( break31_g24.g + break31_g23.g );
				float localStochasticTiling2_g22 = ( 0.0 );
				float2 panner78 = ( ( WaveSpeed43 * 0.15 ) * WaveDirection140 + ( WaveTileUV38 * 0.1 ).xy);
				float2 Input_UV145_g22 = panner78;
				float2 UV2_g22 = Input_UV145_g22;
				float2 UV12_g22 = float2( 0,0 );
				float2 UV22_g22 = float2( 0,0 );
				float2 UV32_g22 = float2( 0,0 );
				float W12_g22 = 0.0;
				float W22_g22 = 0.0;
				float W32_g22 = 0.0;
				StochasticTiling( UV2_g22 , UV12_g22 , UV22_g22 , UV32_g22 , W12_g22 , W22_g22 , W32_g22 );
				float2 temp_output_10_0_g22 = ddx( Input_UV145_g22 );
				float2 temp_output_12_0_g22 = ddy( Input_UV145_g22 );
				float4 Output_2D293_g22 = ( ( tex2D( _HeightMap, UV12_g22, temp_output_10_0_g22, temp_output_12_0_g22 ) * W12_g22 ) + ( tex2D( _HeightMap, UV22_g22, temp_output_10_0_g22, temp_output_12_0_g22 ) * W22_g22 ) + ( tex2D( _HeightMap, UV32_g22, temp_output_10_0_g22, temp_output_12_0_g22 ) * W32_g22 ) );
				float4 break31_g22 = Output_2D293_g22;
				float localStochasticTiling2_g21 = ( 0.0 );
				float2 panner73 = ( ( WaveSpeed43 * 0.15 ) * WaveDirection2201 + ( WaveTileUV38 * 0.09 ).xy);
				float2 Input_UV145_g21 = panner73;
				float2 UV2_g21 = Input_UV145_g21;
				float2 UV12_g21 = float2( 0,0 );
				float2 UV22_g21 = float2( 0,0 );
				float2 UV32_g21 = float2( 0,0 );
				float W12_g21 = 0.0;
				float W22_g21 = 0.0;
				float W32_g21 = 0.0;
				StochasticTiling( UV2_g21 , UV12_g21 , UV22_g21 , UV32_g21 , W12_g21 , W22_g21 , W32_g21 );
				float2 temp_output_10_0_g21 = ddx( Input_UV145_g21 );
				float2 temp_output_12_0_g21 = ddy( Input_UV145_g21 );
				float4 Output_2D293_g21 = ( ( tex2D( _HeightMap, UV12_g21, temp_output_10_0_g21, temp_output_12_0_g21 ) * W12_g21 ) + ( tex2D( _HeightMap, UV22_g21, temp_output_10_0_g21, temp_output_12_0_g21 ) * W22_g21 ) + ( tex2D( _HeightMap, UV32_g21, temp_output_10_0_g21, temp_output_12_0_g21 ) * W32_g21 ) );
				float4 break31_g21 = Output_2D293_g21;
				float LargeWaves101 = ( break31_g22.g + break31_g21.g );
				float lerpResult110 = lerp( ( FineWaves100 * _SmallWaveStrength ) , ( ( LargeWaves101 * 5.0 ) * _LargeWaveStrength ) , _WaveSizeBalance);
				float FinalWave106 = lerpResult110;
				float height107_g25 = FinalWave106;
				float scale107_g25 = _NormalStrength;
				float3 localPerturbNormal107_g25 = PerturbNormal107_g25( surf_pos107_g25 , surf_norm107_g25 , height107_g25 , scale107_g25 );
				float3x3 ase_worldToTangent = float3x3(WorldTangent,WorldBiTangent,WorldNormal);
				float3 worldToTangentDir42_g25 = mul( ase_worldToTangent, localPerturbNormal107_g25);
				float3 normalizeResult56 = normalize( worldToTangentDir42_g25 );
				float3 Normal150 = normalizeResult56;
				float eyeDepth = IN.ase_texcoord7.x;
				float eyeDepth28_g26 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( ase_screenPosNorm.xy ),_ZBufferParams);
				float2 temp_output_20_0_g26 = ( (Normal150).xy * ( _RefractionStrength / max( eyeDepth , 0.1 ) ) * saturate( ( eyeDepth28_g26 - eyeDepth ) ) );
				float eyeDepth2_g26 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( ( float4( temp_output_20_0_g26, 0.0 , 0.0 ) + ase_screenPosNorm ).xy ),_ZBufferParams);
				float2 temp_output_32_0_g26 = (( float4( ( temp_output_20_0_g26 * saturate( ( eyeDepth2_g26 - eyeDepth ) ) ), 0.0 , 0.0 ) + ase_screenPosNorm )).xy;
				float4 fetchOpaqueVal154 = float4( SHADERGRAPH_SAMPLE_SCENE_COLOR( temp_output_32_0_g26 ), 1.0 );
				float4 clampResult155 = clamp( fetchOpaqueVal154 , float4( 0,0,0,0 ) , float4( 1,1,1,0 ) );
				float4 Refraction156 = clampResult155;
				float screenDepth158 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( ase_screenPosNorm.xy ),_ZBufferParams);
				float distanceDepth158 = abs( ( screenDepth158 - LinearEyeDepth( ase_screenPosNorm.z,_ZBufferParams ) ) / ( ( -10.0 * _VisibilityDepth ) ) );
				float clampResult160 = clamp( ( 1.0 - distanceDepth158 ) , 0.0 , 1.0 );
				float Depth161 = clampResult160;
				float4 lerpResult167 = lerp( Albedo165 , Refraction156 , Depth161);
				
				float screenDepth194 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( ase_screenPosNorm.xy ),_ZBufferParams);
				float distanceDepth194 = abs( ( screenDepth194 - LinearEyeDepth( ase_screenPosNorm.z,_ZBufferParams ) ) / ( _EdgeFadeDistance ) );
				float clampResult196 = clamp( distanceDepth194 , 0.0 , 1.0 );
				float Edge137 = clampResult196;
				
				float3 Albedo = lerpResult167.rgb;
				float3 Normal = Normal150;
				float3 Emission = 0;
				float3 Specular = 0.5;
				float Metallic = 0;
				float Smoothness = 0.98;
				float Occlusion = 1;
				float Alpha = Edge137;
				float AlphaClipThreshold = 0.5;
				float AlphaClipThresholdShadow = 0.5;
				float3 BakedGI = 0;
				float3 RefractionColor = 1;
				float RefractionIndex = 1;
				float3 Transmission = 1;
				float3 Translucency = 1;
				#ifdef ASE_DEPTH_WRITE_ON
				float DepthValue = 0;
				#endif

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				InputData inputData;
				inputData.positionWS = WorldPosition;
				inputData.viewDirectionWS = WorldViewDirection;
				inputData.shadowCoord = ShadowCoords;

				#ifdef _NORMALMAP
					#if _NORMAL_DROPOFF_TS
					inputData.normalWS = TransformTangentToWorld(Normal, half3x3( WorldTangent, WorldBiTangent, WorldNormal ));
					#elif _NORMAL_DROPOFF_OS
					inputData.normalWS = TransformObjectToWorldNormal(Normal);
					#elif _NORMAL_DROPOFF_WS
					inputData.normalWS = Normal;
					#endif
					inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
				#else
					inputData.normalWS = WorldNormal;
				#endif

				#ifdef ASE_FOG
					inputData.fogCoord = IN.fogFactorAndVertexLight.x;
				#endif

				inputData.vertexLighting = IN.fogFactorAndVertexLight.yzw;
				#if defined(ENABLE_TERRAIN_PERPIXEL_NORMAL)
					float3 SH = SampleSH(inputData.normalWS.xyz);
				#else
					float3 SH = IN.lightmapUVOrVertexSH.xyz;
				#endif

				inputData.bakedGI = SAMPLE_GI( IN.lightmapUVOrVertexSH.xy, SH, inputData.normalWS );
				#ifdef _ASE_BAKEDGI
					inputData.bakedGI = BakedGI;
				#endif
				half4 color = UniversalFragmentPBR(
					inputData, 
					Albedo, 
					Metallic, 
					Specular, 
					Smoothness, 
					Occlusion, 
					Emission, 
					Alpha);

				#ifdef _TRANSMISSION_ASE
				{
					float shadow = _TransmissionShadow;

					Light mainLight = GetMainLight( inputData.shadowCoord );
					float3 mainAtten = mainLight.color * mainLight.distanceAttenuation;
					mainAtten = lerp( mainAtten, mainAtten * mainLight.shadowAttenuation, shadow );
					half3 mainTransmission = max(0 , -dot(inputData.normalWS, mainLight.direction)) * mainAtten * Transmission;
					color.rgb += Albedo * mainTransmission;

					#ifdef _ADDITIONAL_LIGHTS
						int transPixelLightCount = GetAdditionalLightsCount();
						for (int i = 0; i < transPixelLightCount; ++i)
						{
							Light light = GetAdditionalLight(i, inputData.positionWS);
							float3 atten = light.color * light.distanceAttenuation;
							atten = lerp( atten, atten * light.shadowAttenuation, shadow );

							half3 transmission = max(0 , -dot(inputData.normalWS, light.direction)) * atten * Transmission;
							color.rgb += Albedo * transmission;
						}
					#endif
				}
				#endif

				#ifdef _TRANSLUCENCY_ASE
				{
					float shadow = _TransShadow;
					float normal = _TransNormal;
					float scattering = _TransScattering;
					float direct = _TransDirect;
					float ambient = _TransAmbient;
					float strength = _TransStrength;

					Light mainLight = GetMainLight( inputData.shadowCoord );
					float3 mainAtten = mainLight.color * mainLight.distanceAttenuation;
					mainAtten = lerp( mainAtten, mainAtten * mainLight.shadowAttenuation, shadow );

					half3 mainLightDir = mainLight.direction + inputData.normalWS * normal;
					half mainVdotL = pow( saturate( dot( inputData.viewDirectionWS, -mainLightDir ) ), scattering );
					half3 mainTranslucency = mainAtten * ( mainVdotL * direct + inputData.bakedGI * ambient ) * Translucency;
					color.rgb += Albedo * mainTranslucency * strength;

					#ifdef _ADDITIONAL_LIGHTS
						int transPixelLightCount = GetAdditionalLightsCount();
						for (int i = 0; i < transPixelLightCount; ++i)
						{
							Light light = GetAdditionalLight(i, inputData.positionWS);
							float3 atten = light.color * light.distanceAttenuation;
							atten = lerp( atten, atten * light.shadowAttenuation, shadow );

							half3 lightDir = light.direction + inputData.normalWS * normal;
							half VdotL = pow( saturate( dot( inputData.viewDirectionWS, -lightDir ) ), scattering );
							half3 translucency = atten * ( VdotL * direct + inputData.bakedGI * ambient ) * Translucency;
							color.rgb += Albedo * translucency * strength;
						}
					#endif
				}
				#endif

				#ifdef _REFRACTION_ASE
					float4 projScreenPos = ScreenPos / ScreenPos.w;
					float3 refractionOffset = ( RefractionIndex - 1.0 ) * mul( UNITY_MATRIX_V, WorldNormal ).xyz * ( 1.0 - dot( WorldNormal, WorldViewDirection ) );
					projScreenPos.xy += refractionOffset.xy;
					float3 refraction = SHADERGRAPH_SAMPLE_SCENE_COLOR( projScreenPos ) * RefractionColor;
					color.rgb = lerp( refraction, color.rgb, color.a );
					color.a = 1;
				#endif

				#ifdef ASE_FINAL_COLOR_ALPHA_MULTIPLY
					color.rgb *= color.a;
				#endif

				#ifdef ASE_FOG
					#ifdef TERRAIN_SPLAT_ADDPASS
						color.rgb = MixFogColor(color.rgb, half3( 0, 0, 0 ), IN.fogFactorAndVertexLight.x );
					#else
						color.rgb = MixFog(color.rgb, IN.fogFactorAndVertexLight.x);
					#endif
				#endif
				
				#ifdef ASE_DEPTH_WRITE_ON
					outputDepth = DepthValue;
				#endif

				return color;
			}

			ENDHLSL
		}

		
		Pass
		{
			
			Name "DepthOnly"
			Tags { "LightMode"="DepthOnly" }

			ZWrite On
			ColorMask 0
			AlphaToMask Off

			HLSLPROGRAM
			#define _NORMAL_DROPOFF_TS 1
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define TESSELLATION_ON 1
			#pragma require tessellation tessHW
			#pragma hull HullFunction
			#pragma domain DomainFunction
			#define ASE_DISTANCE_TESSELLATION
			#define _NORMALMAP 1
			#define ASE_SRP_VERSION 70501
			#define REQUIRE_DEPTH_TEXTURE 1

			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x

			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS_DEPTHONLY

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

			

			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 worldPos : TEXCOORD0;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
				float4 shadowCoord : TEXCOORD1;
				#endif
				float4 ase_texcoord2 : TEXCOORD2;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _DeepColor;
			float4 _ShallowColor;
			float2 _Wave1Direction;
			float2 _Wave2Direction;
			float _WaveSpeed;
			float _WaveTile;
			float _SmallWaveStrength;
			float _LargeWaveStrength;
			float _WaveSizeBalance;
			float _WaveHeight;
			float _WaterColorDepth;
			float _NormalStrength;
			float _RefractionStrength;
			float _VisibilityDepth;
			float _EdgeFadeDistance;
			#ifdef _TRANSMISSION_ASE
				float _TransmissionShadow;
			#endif
			#ifdef _TRANSLUCENCY_ASE
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef TESSELLATION_ON
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END
			sampler2D _HeightMap;
			uniform float4 _CameraDepthTexture_TexelSize;


			void StochasticTiling( float2 UV, out float2 UV1, out float2 UV2, out float2 UV3, out float W1, out float W2, out float W3 )
			{
				float2 vertex1, vertex2, vertex3;
				// Scaling of the input
				float2 uv = UV * 3.464; // 2 * sqrt (3)
				// Skew input space into simplex triangle grid
				const float2x2 gridToSkewedGrid = float2x2( 1.0, 0.0, -0.57735027, 1.15470054 );
				float2 skewedCoord = mul( gridToSkewedGrid, uv );
				// Compute local triangle vertex IDs and local barycentric coordinates
				int2 baseId = int2( floor( skewedCoord ) );
				float3 temp = float3( frac( skewedCoord ), 0 );
				temp.z = 1.0 - temp.x - temp.y;
				if ( temp.z > 0.0 )
				{
					W1 = temp.z;
					W2 = temp.y;
					W3 = temp.x;
					vertex1 = baseId;
					vertex2 = baseId + int2( 0, 1 );
					vertex3 = baseId + int2( 1, 0 );
				}
				else
				{
					W1 = -temp.z;
					W2 = 1.0 - temp.y;
					W3 = 1.0 - temp.x;
					vertex1 = baseId + int2( 1, 1 );
					vertex2 = baseId + int2( 1, 0 );
					vertex3 = baseId + int2( 0, 1 );
				}
				UV1 = UV + frac( sin( mul( float2x2( 127.1, 311.7, 269.5, 183.3 ), vertex1 ) ) * 43758.5453 );
				UV2 = UV + frac( sin( mul( float2x2( 127.1, 311.7, 269.5, 183.3 ), vertex2 ) ) * 43758.5453 );
				UV3 = UV + frac( sin( mul( float2x2( 127.1, 311.7, 269.5, 183.3 ), vertex3 ) ) * 43758.5453 );
				return;
			}
			

			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				float localStochasticTiling2_g24 = ( 0.0 );
				float WaveSpeed43 = ( _TimeParameters.x * _WaveSpeed * 0.1 );
				float2 WaveDirection140 = _Wave1Direction;
				float3 ase_worldPos = mul(GetObjectToWorldMatrix(), v.vertex).xyz;
				float4 appendResult10 = (float4(ase_worldPos.x , ase_worldPos.z , 0.0 , 0.0));
				float4 WorldSpaceTile11 = appendResult10;
				float4 WaveTileUV38 = ( ( WorldSpaceTile11 * float4( float2( 1,1 ), 0.0 , 0.0 ) * 0.1 ) * _WaveTile );
				float2 panner4 = ( WaveSpeed43 * WaveDirection140 + WaveTileUV38.xy);
				float2 Input_UV145_g24 = panner4;
				float2 UV2_g24 = Input_UV145_g24;
				float2 UV12_g24 = float2( 0,0 );
				float2 UV22_g24 = float2( 0,0 );
				float2 UV32_g24 = float2( 0,0 );
				float W12_g24 = 0.0;
				float W22_g24 = 0.0;
				float W32_g24 = 0.0;
				StochasticTiling( UV2_g24 , UV12_g24 , UV22_g24 , UV32_g24 , W12_g24 , W22_g24 , W32_g24 );
				float4 Output_2D293_g24 = ( ( tex2Dlod( _HeightMap, float4( UV12_g24, 0, 0.0) ) * W12_g24 ) + ( tex2Dlod( _HeightMap, float4( UV22_g24, 0, 0.0) ) * W22_g24 ) + ( tex2Dlod( _HeightMap, float4( UV32_g24, 0, 0.0) ) * W32_g24 ) );
				float4 break31_g24 = Output_2D293_g24;
				float localStochasticTiling2_g23 = ( 0.0 );
				float2 WaveDirection2201 = _Wave2Direction;
				float2 panner28 = ( WaveSpeed43 * WaveDirection2201 + ( WaveTileUV38 * 0.9 ).xy);
				float2 Input_UV145_g23 = panner28;
				float2 UV2_g23 = Input_UV145_g23;
				float2 UV12_g23 = float2( 0,0 );
				float2 UV22_g23 = float2( 0,0 );
				float2 UV32_g23 = float2( 0,0 );
				float W12_g23 = 0.0;
				float W22_g23 = 0.0;
				float W32_g23 = 0.0;
				StochasticTiling( UV2_g23 , UV12_g23 , UV22_g23 , UV32_g23 , W12_g23 , W22_g23 , W32_g23 );
				float4 Output_2D293_g23 = ( ( tex2Dlod( _HeightMap, float4( UV12_g23, 0, 0.0) ) * W12_g23 ) + ( tex2Dlod( _HeightMap, float4( UV22_g23, 0, 0.0) ) * W22_g23 ) + ( tex2Dlod( _HeightMap, float4( UV32_g23, 0, 0.0) ) * W32_g23 ) );
				float4 break31_g23 = Output_2D293_g23;
				float FineWaves100 = ( break31_g24.g + break31_g23.g );
				float localStochasticTiling2_g22 = ( 0.0 );
				float2 panner78 = ( ( WaveSpeed43 * 0.15 ) * WaveDirection140 + ( WaveTileUV38 * 0.1 ).xy);
				float2 Input_UV145_g22 = panner78;
				float2 UV2_g22 = Input_UV145_g22;
				float2 UV12_g22 = float2( 0,0 );
				float2 UV22_g22 = float2( 0,0 );
				float2 UV32_g22 = float2( 0,0 );
				float W12_g22 = 0.0;
				float W22_g22 = 0.0;
				float W32_g22 = 0.0;
				StochasticTiling( UV2_g22 , UV12_g22 , UV22_g22 , UV32_g22 , W12_g22 , W22_g22 , W32_g22 );
				float4 Output_2D293_g22 = ( ( tex2Dlod( _HeightMap, float4( UV12_g22, 0, 0.0) ) * W12_g22 ) + ( tex2Dlod( _HeightMap, float4( UV22_g22, 0, 0.0) ) * W22_g22 ) + ( tex2Dlod( _HeightMap, float4( UV32_g22, 0, 0.0) ) * W32_g22 ) );
				float4 break31_g22 = Output_2D293_g22;
				float localStochasticTiling2_g21 = ( 0.0 );
				float2 panner73 = ( ( WaveSpeed43 * 0.15 ) * WaveDirection2201 + ( WaveTileUV38 * 0.09 ).xy);
				float2 Input_UV145_g21 = panner73;
				float2 UV2_g21 = Input_UV145_g21;
				float2 UV12_g21 = float2( 0,0 );
				float2 UV22_g21 = float2( 0,0 );
				float2 UV32_g21 = float2( 0,0 );
				float W12_g21 = 0.0;
				float W22_g21 = 0.0;
				float W32_g21 = 0.0;
				StochasticTiling( UV2_g21 , UV12_g21 , UV22_g21 , UV32_g21 , W12_g21 , W22_g21 , W32_g21 );
				float4 Output_2D293_g21 = ( ( tex2Dlod( _HeightMap, float4( UV12_g21, 0, 0.0) ) * W12_g21 ) + ( tex2Dlod( _HeightMap, float4( UV22_g21, 0, 0.0) ) * W22_g21 ) + ( tex2Dlod( _HeightMap, float4( UV32_g21, 0, 0.0) ) * W32_g21 ) );
				float4 break31_g21 = Output_2D293_g21;
				float LargeWaves101 = ( break31_g22.g + break31_g21.g );
				float lerpResult110 = lerp( ( FineWaves100 * _SmallWaveStrength ) , ( ( LargeWaves101 * 5.0 ) * _LargeWaveStrength ) , _WaveSizeBalance);
				float FinalWave106 = lerpResult110;
				float3 Wave47 = ( ( float3(0,1,0) * 1.0 ) * FinalWave106 );
				
				float4 ase_clipPos = TransformObjectToHClip((v.vertex).xyz);
				float4 screenPos = ComputeScreenPos(ase_clipPos);
				o.ase_texcoord2 = screenPos;
				
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = ( Wave47 * _WaveHeight * 0.1 );
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = v.ase_normal;
				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				float4 positionCS = TransformWorldToHClip( positionWS );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				o.worldPos = positionWS;
				#endif

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					VertexPositionInputs vertexInput = (VertexPositionInputs)0;
					vertexInput.positionWS = positionWS;
					vertexInput.positionCS = positionCS;
					o.shadowCoord = GetShadowCoord( vertexInput );
				#endif
				o.clipPos = positionCS;
				return o;
			}

			#if defined(TESSELLATION_ON)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			#if defined(ASE_EARLY_Z_DEPTH_OPTIMIZE)
				#define ASE_SV_DEPTH SV_DepthLessEqual  
			#else
				#define ASE_SV_DEPTH SV_Depth
			#endif
			half4 frag(	VertexOutput IN 
						#ifdef ASE_DEPTH_WRITE_ON
						,out float outputDepth : ASE_SV_DEPTH
						#endif
						 ) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 WorldPosition = IN.worldPos;
				#endif
				float4 ShadowCoords = float4( 0, 0, 0, 0 );

				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = IN.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif

				float4 screenPos = IN.ase_texcoord2;
				float4 ase_screenPosNorm = screenPos / screenPos.w;
				ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
				float screenDepth194 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( ase_screenPosNorm.xy ),_ZBufferParams);
				float distanceDepth194 = abs( ( screenDepth194 - LinearEyeDepth( ase_screenPosNorm.z,_ZBufferParams ) ) / ( _EdgeFadeDistance ) );
				float clampResult196 = clamp( distanceDepth194 , 0.0 , 1.0 );
				float Edge137 = clampResult196;
				
				float Alpha = Edge137;
				float AlphaClipThreshold = 0.5;
				#ifdef ASE_DEPTH_WRITE_ON
				float DepthValue = 0;
				#endif

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				#ifdef LOD_FADE_CROSSFADE
					LODDitheringTransition( IN.clipPos.xyz, unity_LODFade.x );
				#endif
				#ifdef ASE_DEPTH_WRITE_ON
				outputDepth = DepthValue;
				#endif
				return 0;
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "Meta"
			Tags { "LightMode"="Meta" }

			Cull Off

			HLSLPROGRAM
			#define _NORMAL_DROPOFF_TS 1
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define TESSELLATION_ON 1
			#pragma require tessellation tessHW
			#pragma hull HullFunction
			#pragma domain DomainFunction
			#define ASE_DISTANCE_TESSELLATION
			#define _NORMALMAP 1
			#define ASE_SRP_VERSION 70501
			#define REQUIRE_DEPTH_TEXTURE 1
			#define REQUIRE_OPAQUE_TEXTURE 1

			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x

			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS_META

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MetaInput.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

			#define ASE_NEEDS_FRAG_WORLD_POSITION
			#define ASE_NEEDS_VERT_NORMAL
			#define ASE_NEEDS_VERT_POSITION


			#pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				float4 ase_tangent : TANGENT;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 worldPos : TEXCOORD0;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
				float4 shadowCoord : TEXCOORD1;
				#endif
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_texcoord3 : TEXCOORD3;
				float4 ase_texcoord4 : TEXCOORD4;
				float4 ase_texcoord5 : TEXCOORD5;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _DeepColor;
			float4 _ShallowColor;
			float2 _Wave1Direction;
			float2 _Wave2Direction;
			float _WaveSpeed;
			float _WaveTile;
			float _SmallWaveStrength;
			float _LargeWaveStrength;
			float _WaveSizeBalance;
			float _WaveHeight;
			float _WaterColorDepth;
			float _NormalStrength;
			float _RefractionStrength;
			float _VisibilityDepth;
			float _EdgeFadeDistance;
			#ifdef _TRANSMISSION_ASE
				float _TransmissionShadow;
			#endif
			#ifdef _TRANSLUCENCY_ASE
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef TESSELLATION_ON
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END
			sampler2D _HeightMap;
			uniform float4 _CameraDepthTexture_TexelSize;


			void StochasticTiling( float2 UV, out float2 UV1, out float2 UV2, out float2 UV3, out float W1, out float W2, out float W3 )
			{
				float2 vertex1, vertex2, vertex3;
				// Scaling of the input
				float2 uv = UV * 3.464; // 2 * sqrt (3)
				// Skew input space into simplex triangle grid
				const float2x2 gridToSkewedGrid = float2x2( 1.0, 0.0, -0.57735027, 1.15470054 );
				float2 skewedCoord = mul( gridToSkewedGrid, uv );
				// Compute local triangle vertex IDs and local barycentric coordinates
				int2 baseId = int2( floor( skewedCoord ) );
				float3 temp = float3( frac( skewedCoord ), 0 );
				temp.z = 1.0 - temp.x - temp.y;
				if ( temp.z > 0.0 )
				{
					W1 = temp.z;
					W2 = temp.y;
					W3 = temp.x;
					vertex1 = baseId;
					vertex2 = baseId + int2( 0, 1 );
					vertex3 = baseId + int2( 1, 0 );
				}
				else
				{
					W1 = -temp.z;
					W2 = 1.0 - temp.y;
					W3 = 1.0 - temp.x;
					vertex1 = baseId + int2( 1, 1 );
					vertex2 = baseId + int2( 1, 0 );
					vertex3 = baseId + int2( 0, 1 );
				}
				UV1 = UV + frac( sin( mul( float2x2( 127.1, 311.7, 269.5, 183.3 ), vertex1 ) ) * 43758.5453 );
				UV2 = UV + frac( sin( mul( float2x2( 127.1, 311.7, 269.5, 183.3 ), vertex2 ) ) * 43758.5453 );
				UV3 = UV + frac( sin( mul( float2x2( 127.1, 311.7, 269.5, 183.3 ), vertex3 ) ) * 43758.5453 );
				return;
			}
			
			float3 PerturbNormal107_g25( float3 surf_pos, float3 surf_norm, float height, float scale )
			{
				// "Bump Mapping Unparametrized Surfaces on the GPU" by Morten S. Mikkelsen
				float3 vSigmaS = ddx( surf_pos );
				float3 vSigmaT = ddy( surf_pos );
				float3 vN = surf_norm;
				float3 vR1 = cross( vSigmaT , vN );
				float3 vR2 = cross( vN , vSigmaS );
				float fDet = dot( vSigmaS , vR1 );
				float dBs = ddx( height );
				float dBt = ddy( height );
				float3 vSurfGrad = scale * 0.05 * sign( fDet ) * ( dBs * vR1 + dBt * vR2 );
				return normalize ( abs( fDet ) * vN - vSurfGrad );
			}
			

			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				float localStochasticTiling2_g24 = ( 0.0 );
				float WaveSpeed43 = ( _TimeParameters.x * _WaveSpeed * 0.1 );
				float2 WaveDirection140 = _Wave1Direction;
				float3 ase_worldPos = mul(GetObjectToWorldMatrix(), v.vertex).xyz;
				float4 appendResult10 = (float4(ase_worldPos.x , ase_worldPos.z , 0.0 , 0.0));
				float4 WorldSpaceTile11 = appendResult10;
				float4 WaveTileUV38 = ( ( WorldSpaceTile11 * float4( float2( 1,1 ), 0.0 , 0.0 ) * 0.1 ) * _WaveTile );
				float2 panner4 = ( WaveSpeed43 * WaveDirection140 + WaveTileUV38.xy);
				float2 Input_UV145_g24 = panner4;
				float2 UV2_g24 = Input_UV145_g24;
				float2 UV12_g24 = float2( 0,0 );
				float2 UV22_g24 = float2( 0,0 );
				float2 UV32_g24 = float2( 0,0 );
				float W12_g24 = 0.0;
				float W22_g24 = 0.0;
				float W32_g24 = 0.0;
				StochasticTiling( UV2_g24 , UV12_g24 , UV22_g24 , UV32_g24 , W12_g24 , W22_g24 , W32_g24 );
				float4 Output_2D293_g24 = ( ( tex2Dlod( _HeightMap, float4( UV12_g24, 0, 0.0) ) * W12_g24 ) + ( tex2Dlod( _HeightMap, float4( UV22_g24, 0, 0.0) ) * W22_g24 ) + ( tex2Dlod( _HeightMap, float4( UV32_g24, 0, 0.0) ) * W32_g24 ) );
				float4 break31_g24 = Output_2D293_g24;
				float localStochasticTiling2_g23 = ( 0.0 );
				float2 WaveDirection2201 = _Wave2Direction;
				float2 panner28 = ( WaveSpeed43 * WaveDirection2201 + ( WaveTileUV38 * 0.9 ).xy);
				float2 Input_UV145_g23 = panner28;
				float2 UV2_g23 = Input_UV145_g23;
				float2 UV12_g23 = float2( 0,0 );
				float2 UV22_g23 = float2( 0,0 );
				float2 UV32_g23 = float2( 0,0 );
				float W12_g23 = 0.0;
				float W22_g23 = 0.0;
				float W32_g23 = 0.0;
				StochasticTiling( UV2_g23 , UV12_g23 , UV22_g23 , UV32_g23 , W12_g23 , W22_g23 , W32_g23 );
				float4 Output_2D293_g23 = ( ( tex2Dlod( _HeightMap, float4( UV12_g23, 0, 0.0) ) * W12_g23 ) + ( tex2Dlod( _HeightMap, float4( UV22_g23, 0, 0.0) ) * W22_g23 ) + ( tex2Dlod( _HeightMap, float4( UV32_g23, 0, 0.0) ) * W32_g23 ) );
				float4 break31_g23 = Output_2D293_g23;
				float FineWaves100 = ( break31_g24.g + break31_g23.g );
				float localStochasticTiling2_g22 = ( 0.0 );
				float2 panner78 = ( ( WaveSpeed43 * 0.15 ) * WaveDirection140 + ( WaveTileUV38 * 0.1 ).xy);
				float2 Input_UV145_g22 = panner78;
				float2 UV2_g22 = Input_UV145_g22;
				float2 UV12_g22 = float2( 0,0 );
				float2 UV22_g22 = float2( 0,0 );
				float2 UV32_g22 = float2( 0,0 );
				float W12_g22 = 0.0;
				float W22_g22 = 0.0;
				float W32_g22 = 0.0;
				StochasticTiling( UV2_g22 , UV12_g22 , UV22_g22 , UV32_g22 , W12_g22 , W22_g22 , W32_g22 );
				float4 Output_2D293_g22 = ( ( tex2Dlod( _HeightMap, float4( UV12_g22, 0, 0.0) ) * W12_g22 ) + ( tex2Dlod( _HeightMap, float4( UV22_g22, 0, 0.0) ) * W22_g22 ) + ( tex2Dlod( _HeightMap, float4( UV32_g22, 0, 0.0) ) * W32_g22 ) );
				float4 break31_g22 = Output_2D293_g22;
				float localStochasticTiling2_g21 = ( 0.0 );
				float2 panner73 = ( ( WaveSpeed43 * 0.15 ) * WaveDirection2201 + ( WaveTileUV38 * 0.09 ).xy);
				float2 Input_UV145_g21 = panner73;
				float2 UV2_g21 = Input_UV145_g21;
				float2 UV12_g21 = float2( 0,0 );
				float2 UV22_g21 = float2( 0,0 );
				float2 UV32_g21 = float2( 0,0 );
				float W12_g21 = 0.0;
				float W22_g21 = 0.0;
				float W32_g21 = 0.0;
				StochasticTiling( UV2_g21 , UV12_g21 , UV22_g21 , UV32_g21 , W12_g21 , W22_g21 , W32_g21 );
				float4 Output_2D293_g21 = ( ( tex2Dlod( _HeightMap, float4( UV12_g21, 0, 0.0) ) * W12_g21 ) + ( tex2Dlod( _HeightMap, float4( UV22_g21, 0, 0.0) ) * W22_g21 ) + ( tex2Dlod( _HeightMap, float4( UV32_g21, 0, 0.0) ) * W32_g21 ) );
				float4 break31_g21 = Output_2D293_g21;
				float LargeWaves101 = ( break31_g22.g + break31_g21.g );
				float lerpResult110 = lerp( ( FineWaves100 * _SmallWaveStrength ) , ( ( LargeWaves101 * 5.0 ) * _LargeWaveStrength ) , _WaveSizeBalance);
				float FinalWave106 = lerpResult110;
				float3 Wave47 = ( ( float3(0,1,0) * 1.0 ) * FinalWave106 );
				
				float4 ase_clipPos = TransformObjectToHClip((v.vertex).xyz);
				float4 screenPos = ComputeScreenPos(ase_clipPos);
				o.ase_texcoord2 = screenPos;
				float3 ase_worldNormal = TransformObjectToWorldNormal(v.ase_normal);
				o.ase_texcoord3.xyz = ase_worldNormal;
				float3 ase_worldTangent = TransformObjectToWorldDir(v.ase_tangent.xyz);
				o.ase_texcoord4.xyz = ase_worldTangent;
				float ase_vertexTangentSign = v.ase_tangent.w * unity_WorldTransformParams.w;
				float3 ase_worldBitangent = cross( ase_worldNormal, ase_worldTangent ) * ase_vertexTangentSign;
				o.ase_texcoord5.xyz = ase_worldBitangent;
				float3 objectToViewPos = TransformWorldToView(TransformObjectToWorld(v.vertex.xyz));
				float eyeDepth = -objectToViewPos.z;
				o.ase_texcoord3.w = eyeDepth;
				
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord4.w = 0;
				o.ase_texcoord5.w = 0;
				
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = ( Wave47 * _WaveHeight * 0.1 );
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = v.ase_normal;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				o.worldPos = positionWS;
				#endif

				o.clipPos = MetaVertexPosition( v.vertex, v.texcoord1.xy, v.texcoord1.xy, unity_LightmapST, unity_DynamicLightmapST );
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					VertexPositionInputs vertexInput = (VertexPositionInputs)0;
					vertexInput.positionWS = positionWS;
					vertexInput.positionCS = o.clipPos;
					o.shadowCoord = GetShadowCoord( vertexInput );
				#endif
				return o;
			}

			#if defined(TESSELLATION_ON)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				float4 ase_tangent : TANGENT;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				o.texcoord1 = v.texcoord1;
				o.texcoord2 = v.texcoord2;
				o.ase_tangent = v.ase_tangent;
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				o.texcoord1 = patch[0].texcoord1 * bary.x + patch[1].texcoord1 * bary.y + patch[2].texcoord1 * bary.z;
				o.texcoord2 = patch[0].texcoord2 * bary.x + patch[1].texcoord2 * bary.y + patch[2].texcoord2 * bary.z;
				o.ase_tangent = patch[0].ase_tangent * bary.x + patch[1].ase_tangent * bary.y + patch[2].ase_tangent * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			half4 frag(VertexOutput IN  ) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 WorldPosition = IN.worldPos;
				#endif
				float4 ShadowCoords = float4( 0, 0, 0, 0 );

				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = IN.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif

				float4 screenPos = IN.ase_texcoord2;
				float4 ase_screenPosNorm = screenPos / screenPos.w;
				ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
				float screenDepth180 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( ase_screenPosNorm.xy ),_ZBufferParams);
				float distanceDepth180 = abs( ( screenDepth180 - LinearEyeDepth( ase_screenPosNorm.z,_ZBufferParams ) ) / ( ( _WaterColorDepth * -10.0 ) ) );
				float clampResult182 = clamp( ( 1.0 - distanceDepth180 ) , 0.0 , 1.0 );
				float4 lerpResult61 = lerp( _DeepColor , _ShallowColor , clampResult182);
				float4 Albedo165 = lerpResult61;
				float3 surf_pos107_g25 = WorldPosition;
				float3 ase_worldNormal = IN.ase_texcoord3.xyz;
				float3 surf_norm107_g25 = ase_worldNormal;
				float localStochasticTiling2_g24 = ( 0.0 );
				float WaveSpeed43 = ( _TimeParameters.x * _WaveSpeed * 0.1 );
				float2 WaveDirection140 = _Wave1Direction;
				float4 appendResult10 = (float4(WorldPosition.x , WorldPosition.z , 0.0 , 0.0));
				float4 WorldSpaceTile11 = appendResult10;
				float4 WaveTileUV38 = ( ( WorldSpaceTile11 * float4( float2( 1,1 ), 0.0 , 0.0 ) * 0.1 ) * _WaveTile );
				float2 panner4 = ( WaveSpeed43 * WaveDirection140 + WaveTileUV38.xy);
				float2 Input_UV145_g24 = panner4;
				float2 UV2_g24 = Input_UV145_g24;
				float2 UV12_g24 = float2( 0,0 );
				float2 UV22_g24 = float2( 0,0 );
				float2 UV32_g24 = float2( 0,0 );
				float W12_g24 = 0.0;
				float W22_g24 = 0.0;
				float W32_g24 = 0.0;
				StochasticTiling( UV2_g24 , UV12_g24 , UV22_g24 , UV32_g24 , W12_g24 , W22_g24 , W32_g24 );
				float2 temp_output_10_0_g24 = ddx( Input_UV145_g24 );
				float2 temp_output_12_0_g24 = ddy( Input_UV145_g24 );
				float4 Output_2D293_g24 = ( ( tex2D( _HeightMap, UV12_g24, temp_output_10_0_g24, temp_output_12_0_g24 ) * W12_g24 ) + ( tex2D( _HeightMap, UV22_g24, temp_output_10_0_g24, temp_output_12_0_g24 ) * W22_g24 ) + ( tex2D( _HeightMap, UV32_g24, temp_output_10_0_g24, temp_output_12_0_g24 ) * W32_g24 ) );
				float4 break31_g24 = Output_2D293_g24;
				float localStochasticTiling2_g23 = ( 0.0 );
				float2 WaveDirection2201 = _Wave2Direction;
				float2 panner28 = ( WaveSpeed43 * WaveDirection2201 + ( WaveTileUV38 * 0.9 ).xy);
				float2 Input_UV145_g23 = panner28;
				float2 UV2_g23 = Input_UV145_g23;
				float2 UV12_g23 = float2( 0,0 );
				float2 UV22_g23 = float2( 0,0 );
				float2 UV32_g23 = float2( 0,0 );
				float W12_g23 = 0.0;
				float W22_g23 = 0.0;
				float W32_g23 = 0.0;
				StochasticTiling( UV2_g23 , UV12_g23 , UV22_g23 , UV32_g23 , W12_g23 , W22_g23 , W32_g23 );
				float2 temp_output_10_0_g23 = ddx( Input_UV145_g23 );
				float2 temp_output_12_0_g23 = ddy( Input_UV145_g23 );
				float4 Output_2D293_g23 = ( ( tex2D( _HeightMap, UV12_g23, temp_output_10_0_g23, temp_output_12_0_g23 ) * W12_g23 ) + ( tex2D( _HeightMap, UV22_g23, temp_output_10_0_g23, temp_output_12_0_g23 ) * W22_g23 ) + ( tex2D( _HeightMap, UV32_g23, temp_output_10_0_g23, temp_output_12_0_g23 ) * W32_g23 ) );
				float4 break31_g23 = Output_2D293_g23;
				float FineWaves100 = ( break31_g24.g + break31_g23.g );
				float localStochasticTiling2_g22 = ( 0.0 );
				float2 panner78 = ( ( WaveSpeed43 * 0.15 ) * WaveDirection140 + ( WaveTileUV38 * 0.1 ).xy);
				float2 Input_UV145_g22 = panner78;
				float2 UV2_g22 = Input_UV145_g22;
				float2 UV12_g22 = float2( 0,0 );
				float2 UV22_g22 = float2( 0,0 );
				float2 UV32_g22 = float2( 0,0 );
				float W12_g22 = 0.0;
				float W22_g22 = 0.0;
				float W32_g22 = 0.0;
				StochasticTiling( UV2_g22 , UV12_g22 , UV22_g22 , UV32_g22 , W12_g22 , W22_g22 , W32_g22 );
				float2 temp_output_10_0_g22 = ddx( Input_UV145_g22 );
				float2 temp_output_12_0_g22 = ddy( Input_UV145_g22 );
				float4 Output_2D293_g22 = ( ( tex2D( _HeightMap, UV12_g22, temp_output_10_0_g22, temp_output_12_0_g22 ) * W12_g22 ) + ( tex2D( _HeightMap, UV22_g22, temp_output_10_0_g22, temp_output_12_0_g22 ) * W22_g22 ) + ( tex2D( _HeightMap, UV32_g22, temp_output_10_0_g22, temp_output_12_0_g22 ) * W32_g22 ) );
				float4 break31_g22 = Output_2D293_g22;
				float localStochasticTiling2_g21 = ( 0.0 );
				float2 panner73 = ( ( WaveSpeed43 * 0.15 ) * WaveDirection2201 + ( WaveTileUV38 * 0.09 ).xy);
				float2 Input_UV145_g21 = panner73;
				float2 UV2_g21 = Input_UV145_g21;
				float2 UV12_g21 = float2( 0,0 );
				float2 UV22_g21 = float2( 0,0 );
				float2 UV32_g21 = float2( 0,0 );
				float W12_g21 = 0.0;
				float W22_g21 = 0.0;
				float W32_g21 = 0.0;
				StochasticTiling( UV2_g21 , UV12_g21 , UV22_g21 , UV32_g21 , W12_g21 , W22_g21 , W32_g21 );
				float2 temp_output_10_0_g21 = ddx( Input_UV145_g21 );
				float2 temp_output_12_0_g21 = ddy( Input_UV145_g21 );
				float4 Output_2D293_g21 = ( ( tex2D( _HeightMap, UV12_g21, temp_output_10_0_g21, temp_output_12_0_g21 ) * W12_g21 ) + ( tex2D( _HeightMap, UV22_g21, temp_output_10_0_g21, temp_output_12_0_g21 ) * W22_g21 ) + ( tex2D( _HeightMap, UV32_g21, temp_output_10_0_g21, temp_output_12_0_g21 ) * W32_g21 ) );
				float4 break31_g21 = Output_2D293_g21;
				float LargeWaves101 = ( break31_g22.g + break31_g21.g );
				float lerpResult110 = lerp( ( FineWaves100 * _SmallWaveStrength ) , ( ( LargeWaves101 * 5.0 ) * _LargeWaveStrength ) , _WaveSizeBalance);
				float FinalWave106 = lerpResult110;
				float height107_g25 = FinalWave106;
				float scale107_g25 = _NormalStrength;
				float3 localPerturbNormal107_g25 = PerturbNormal107_g25( surf_pos107_g25 , surf_norm107_g25 , height107_g25 , scale107_g25 );
				float3 ase_worldTangent = IN.ase_texcoord4.xyz;
				float3 ase_worldBitangent = IN.ase_texcoord5.xyz;
				float3x3 ase_worldToTangent = float3x3(ase_worldTangent,ase_worldBitangent,ase_worldNormal);
				float3 worldToTangentDir42_g25 = mul( ase_worldToTangent, localPerturbNormal107_g25);
				float3 normalizeResult56 = normalize( worldToTangentDir42_g25 );
				float3 Normal150 = normalizeResult56;
				float eyeDepth = IN.ase_texcoord3.w;
				float eyeDepth28_g26 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( ase_screenPosNorm.xy ),_ZBufferParams);
				float2 temp_output_20_0_g26 = ( (Normal150).xy * ( _RefractionStrength / max( eyeDepth , 0.1 ) ) * saturate( ( eyeDepth28_g26 - eyeDepth ) ) );
				float eyeDepth2_g26 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( ( float4( temp_output_20_0_g26, 0.0 , 0.0 ) + ase_screenPosNorm ).xy ),_ZBufferParams);
				float2 temp_output_32_0_g26 = (( float4( ( temp_output_20_0_g26 * saturate( ( eyeDepth2_g26 - eyeDepth ) ) ), 0.0 , 0.0 ) + ase_screenPosNorm )).xy;
				float4 fetchOpaqueVal154 = float4( SHADERGRAPH_SAMPLE_SCENE_COLOR( temp_output_32_0_g26 ), 1.0 );
				float4 clampResult155 = clamp( fetchOpaqueVal154 , float4( 0,0,0,0 ) , float4( 1,1,1,0 ) );
				float4 Refraction156 = clampResult155;
				float screenDepth158 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( ase_screenPosNorm.xy ),_ZBufferParams);
				float distanceDepth158 = abs( ( screenDepth158 - LinearEyeDepth( ase_screenPosNorm.z,_ZBufferParams ) ) / ( ( -10.0 * _VisibilityDepth ) ) );
				float clampResult160 = clamp( ( 1.0 - distanceDepth158 ) , 0.0 , 1.0 );
				float Depth161 = clampResult160;
				float4 lerpResult167 = lerp( Albedo165 , Refraction156 , Depth161);
				
				float screenDepth194 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( ase_screenPosNorm.xy ),_ZBufferParams);
				float distanceDepth194 = abs( ( screenDepth194 - LinearEyeDepth( ase_screenPosNorm.z,_ZBufferParams ) ) / ( _EdgeFadeDistance ) );
				float clampResult196 = clamp( distanceDepth194 , 0.0 , 1.0 );
				float Edge137 = clampResult196;
				
				
				float3 Albedo = lerpResult167.rgb;
				float3 Emission = 0;
				float Alpha = Edge137;
				float AlphaClipThreshold = 0.5;

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				MetaInput metaInput = (MetaInput)0;
				metaInput.Albedo = Albedo;
				metaInput.Emission = Emission;
				
				return MetaFragment(metaInput);
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "Universal2D"
			Tags { "LightMode"="Universal2D" }

			Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
			ZWrite Off
			ZTest LEqual
			Offset 0 , 0
			ColorMask RGBA

			HLSLPROGRAM
			#define _NORMAL_DROPOFF_TS 1
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define TESSELLATION_ON 1
			#pragma require tessellation tessHW
			#pragma hull HullFunction
			#pragma domain DomainFunction
			#define ASE_DISTANCE_TESSELLATION
			#define _NORMALMAP 1
			#define ASE_SRP_VERSION 70501
			#define REQUIRE_DEPTH_TEXTURE 1
			#define REQUIRE_OPAQUE_TEXTURE 1

			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x

			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS_2D

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			
			#define ASE_NEEDS_FRAG_WORLD_POSITION
			#define ASE_NEEDS_VERT_NORMAL
			#define ASE_NEEDS_VERT_POSITION


			#pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_tangent : TANGENT;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 worldPos : TEXCOORD0;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
				float4 shadowCoord : TEXCOORD1;
				#endif
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_texcoord3 : TEXCOORD3;
				float4 ase_texcoord4 : TEXCOORD4;
				float4 ase_texcoord5 : TEXCOORD5;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _DeepColor;
			float4 _ShallowColor;
			float2 _Wave1Direction;
			float2 _Wave2Direction;
			float _WaveSpeed;
			float _WaveTile;
			float _SmallWaveStrength;
			float _LargeWaveStrength;
			float _WaveSizeBalance;
			float _WaveHeight;
			float _WaterColorDepth;
			float _NormalStrength;
			float _RefractionStrength;
			float _VisibilityDepth;
			float _EdgeFadeDistance;
			#ifdef _TRANSMISSION_ASE
				float _TransmissionShadow;
			#endif
			#ifdef _TRANSLUCENCY_ASE
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef TESSELLATION_ON
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END
			sampler2D _HeightMap;
			uniform float4 _CameraDepthTexture_TexelSize;


			void StochasticTiling( float2 UV, out float2 UV1, out float2 UV2, out float2 UV3, out float W1, out float W2, out float W3 )
			{
				float2 vertex1, vertex2, vertex3;
				// Scaling of the input
				float2 uv = UV * 3.464; // 2 * sqrt (3)
				// Skew input space into simplex triangle grid
				const float2x2 gridToSkewedGrid = float2x2( 1.0, 0.0, -0.57735027, 1.15470054 );
				float2 skewedCoord = mul( gridToSkewedGrid, uv );
				// Compute local triangle vertex IDs and local barycentric coordinates
				int2 baseId = int2( floor( skewedCoord ) );
				float3 temp = float3( frac( skewedCoord ), 0 );
				temp.z = 1.0 - temp.x - temp.y;
				if ( temp.z > 0.0 )
				{
					W1 = temp.z;
					W2 = temp.y;
					W3 = temp.x;
					vertex1 = baseId;
					vertex2 = baseId + int2( 0, 1 );
					vertex3 = baseId + int2( 1, 0 );
				}
				else
				{
					W1 = -temp.z;
					W2 = 1.0 - temp.y;
					W3 = 1.0 - temp.x;
					vertex1 = baseId + int2( 1, 1 );
					vertex2 = baseId + int2( 1, 0 );
					vertex3 = baseId + int2( 0, 1 );
				}
				UV1 = UV + frac( sin( mul( float2x2( 127.1, 311.7, 269.5, 183.3 ), vertex1 ) ) * 43758.5453 );
				UV2 = UV + frac( sin( mul( float2x2( 127.1, 311.7, 269.5, 183.3 ), vertex2 ) ) * 43758.5453 );
				UV3 = UV + frac( sin( mul( float2x2( 127.1, 311.7, 269.5, 183.3 ), vertex3 ) ) * 43758.5453 );
				return;
			}
			
			float3 PerturbNormal107_g25( float3 surf_pos, float3 surf_norm, float height, float scale )
			{
				// "Bump Mapping Unparametrized Surfaces on the GPU" by Morten S. Mikkelsen
				float3 vSigmaS = ddx( surf_pos );
				float3 vSigmaT = ddy( surf_pos );
				float3 vN = surf_norm;
				float3 vR1 = cross( vSigmaT , vN );
				float3 vR2 = cross( vN , vSigmaS );
				float fDet = dot( vSigmaS , vR1 );
				float dBs = ddx( height );
				float dBt = ddy( height );
				float3 vSurfGrad = scale * 0.05 * sign( fDet ) * ( dBs * vR1 + dBt * vR2 );
				return normalize ( abs( fDet ) * vN - vSurfGrad );
			}
			

			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );

				float localStochasticTiling2_g24 = ( 0.0 );
				float WaveSpeed43 = ( _TimeParameters.x * _WaveSpeed * 0.1 );
				float2 WaveDirection140 = _Wave1Direction;
				float3 ase_worldPos = mul(GetObjectToWorldMatrix(), v.vertex).xyz;
				float4 appendResult10 = (float4(ase_worldPos.x , ase_worldPos.z , 0.0 , 0.0));
				float4 WorldSpaceTile11 = appendResult10;
				float4 WaveTileUV38 = ( ( WorldSpaceTile11 * float4( float2( 1,1 ), 0.0 , 0.0 ) * 0.1 ) * _WaveTile );
				float2 panner4 = ( WaveSpeed43 * WaveDirection140 + WaveTileUV38.xy);
				float2 Input_UV145_g24 = panner4;
				float2 UV2_g24 = Input_UV145_g24;
				float2 UV12_g24 = float2( 0,0 );
				float2 UV22_g24 = float2( 0,0 );
				float2 UV32_g24 = float2( 0,0 );
				float W12_g24 = 0.0;
				float W22_g24 = 0.0;
				float W32_g24 = 0.0;
				StochasticTiling( UV2_g24 , UV12_g24 , UV22_g24 , UV32_g24 , W12_g24 , W22_g24 , W32_g24 );
				float4 Output_2D293_g24 = ( ( tex2Dlod( _HeightMap, float4( UV12_g24, 0, 0.0) ) * W12_g24 ) + ( tex2Dlod( _HeightMap, float4( UV22_g24, 0, 0.0) ) * W22_g24 ) + ( tex2Dlod( _HeightMap, float4( UV32_g24, 0, 0.0) ) * W32_g24 ) );
				float4 break31_g24 = Output_2D293_g24;
				float localStochasticTiling2_g23 = ( 0.0 );
				float2 WaveDirection2201 = _Wave2Direction;
				float2 panner28 = ( WaveSpeed43 * WaveDirection2201 + ( WaveTileUV38 * 0.9 ).xy);
				float2 Input_UV145_g23 = panner28;
				float2 UV2_g23 = Input_UV145_g23;
				float2 UV12_g23 = float2( 0,0 );
				float2 UV22_g23 = float2( 0,0 );
				float2 UV32_g23 = float2( 0,0 );
				float W12_g23 = 0.0;
				float W22_g23 = 0.0;
				float W32_g23 = 0.0;
				StochasticTiling( UV2_g23 , UV12_g23 , UV22_g23 , UV32_g23 , W12_g23 , W22_g23 , W32_g23 );
				float4 Output_2D293_g23 = ( ( tex2Dlod( _HeightMap, float4( UV12_g23, 0, 0.0) ) * W12_g23 ) + ( tex2Dlod( _HeightMap, float4( UV22_g23, 0, 0.0) ) * W22_g23 ) + ( tex2Dlod( _HeightMap, float4( UV32_g23, 0, 0.0) ) * W32_g23 ) );
				float4 break31_g23 = Output_2D293_g23;
				float FineWaves100 = ( break31_g24.g + break31_g23.g );
				float localStochasticTiling2_g22 = ( 0.0 );
				float2 panner78 = ( ( WaveSpeed43 * 0.15 ) * WaveDirection140 + ( WaveTileUV38 * 0.1 ).xy);
				float2 Input_UV145_g22 = panner78;
				float2 UV2_g22 = Input_UV145_g22;
				float2 UV12_g22 = float2( 0,0 );
				float2 UV22_g22 = float2( 0,0 );
				float2 UV32_g22 = float2( 0,0 );
				float W12_g22 = 0.0;
				float W22_g22 = 0.0;
				float W32_g22 = 0.0;
				StochasticTiling( UV2_g22 , UV12_g22 , UV22_g22 , UV32_g22 , W12_g22 , W22_g22 , W32_g22 );
				float4 Output_2D293_g22 = ( ( tex2Dlod( _HeightMap, float4( UV12_g22, 0, 0.0) ) * W12_g22 ) + ( tex2Dlod( _HeightMap, float4( UV22_g22, 0, 0.0) ) * W22_g22 ) + ( tex2Dlod( _HeightMap, float4( UV32_g22, 0, 0.0) ) * W32_g22 ) );
				float4 break31_g22 = Output_2D293_g22;
				float localStochasticTiling2_g21 = ( 0.0 );
				float2 panner73 = ( ( WaveSpeed43 * 0.15 ) * WaveDirection2201 + ( WaveTileUV38 * 0.09 ).xy);
				float2 Input_UV145_g21 = panner73;
				float2 UV2_g21 = Input_UV145_g21;
				float2 UV12_g21 = float2( 0,0 );
				float2 UV22_g21 = float2( 0,0 );
				float2 UV32_g21 = float2( 0,0 );
				float W12_g21 = 0.0;
				float W22_g21 = 0.0;
				float W32_g21 = 0.0;
				StochasticTiling( UV2_g21 , UV12_g21 , UV22_g21 , UV32_g21 , W12_g21 , W22_g21 , W32_g21 );
				float4 Output_2D293_g21 = ( ( tex2Dlod( _HeightMap, float4( UV12_g21, 0, 0.0) ) * W12_g21 ) + ( tex2Dlod( _HeightMap, float4( UV22_g21, 0, 0.0) ) * W22_g21 ) + ( tex2Dlod( _HeightMap, float4( UV32_g21, 0, 0.0) ) * W32_g21 ) );
				float4 break31_g21 = Output_2D293_g21;
				float LargeWaves101 = ( break31_g22.g + break31_g21.g );
				float lerpResult110 = lerp( ( FineWaves100 * _SmallWaveStrength ) , ( ( LargeWaves101 * 5.0 ) * _LargeWaveStrength ) , _WaveSizeBalance);
				float FinalWave106 = lerpResult110;
				float3 Wave47 = ( ( float3(0,1,0) * 1.0 ) * FinalWave106 );
				
				float4 ase_clipPos = TransformObjectToHClip((v.vertex).xyz);
				float4 screenPos = ComputeScreenPos(ase_clipPos);
				o.ase_texcoord2 = screenPos;
				float3 ase_worldNormal = TransformObjectToWorldNormal(v.ase_normal);
				o.ase_texcoord3.xyz = ase_worldNormal;
				float3 ase_worldTangent = TransformObjectToWorldDir(v.ase_tangent.xyz);
				o.ase_texcoord4.xyz = ase_worldTangent;
				float ase_vertexTangentSign = v.ase_tangent.w * unity_WorldTransformParams.w;
				float3 ase_worldBitangent = cross( ase_worldNormal, ase_worldTangent ) * ase_vertexTangentSign;
				o.ase_texcoord5.xyz = ase_worldBitangent;
				float3 objectToViewPos = TransformWorldToView(TransformObjectToWorld(v.vertex.xyz));
				float eyeDepth = -objectToViewPos.z;
				o.ase_texcoord3.w = eyeDepth;
				
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord4.w = 0;
				o.ase_texcoord5.w = 0;
				
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = ( Wave47 * _WaveHeight * 0.1 );
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = v.ase_normal;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				float4 positionCS = TransformWorldToHClip( positionWS );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				o.worldPos = positionWS;
				#endif

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					VertexPositionInputs vertexInput = (VertexPositionInputs)0;
					vertexInput.positionWS = positionWS;
					vertexInput.positionCS = positionCS;
					o.shadowCoord = GetShadowCoord( vertexInput );
				#endif

				o.clipPos = positionCS;
				return o;
			}

			#if defined(TESSELLATION_ON)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				float4 ase_tangent : TANGENT;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				o.ase_tangent = v.ase_tangent;
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				o.ase_tangent = patch[0].ase_tangent * bary.x + patch[1].ase_tangent * bary.y + patch[2].ase_tangent * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			half4 frag(VertexOutput IN  ) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 WorldPosition = IN.worldPos;
				#endif
				float4 ShadowCoords = float4( 0, 0, 0, 0 );

				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = IN.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif

				float4 screenPos = IN.ase_texcoord2;
				float4 ase_screenPosNorm = screenPos / screenPos.w;
				ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
				float screenDepth180 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( ase_screenPosNorm.xy ),_ZBufferParams);
				float distanceDepth180 = abs( ( screenDepth180 - LinearEyeDepth( ase_screenPosNorm.z,_ZBufferParams ) ) / ( ( _WaterColorDepth * -10.0 ) ) );
				float clampResult182 = clamp( ( 1.0 - distanceDepth180 ) , 0.0 , 1.0 );
				float4 lerpResult61 = lerp( _DeepColor , _ShallowColor , clampResult182);
				float4 Albedo165 = lerpResult61;
				float3 surf_pos107_g25 = WorldPosition;
				float3 ase_worldNormal = IN.ase_texcoord3.xyz;
				float3 surf_norm107_g25 = ase_worldNormal;
				float localStochasticTiling2_g24 = ( 0.0 );
				float WaveSpeed43 = ( _TimeParameters.x * _WaveSpeed * 0.1 );
				float2 WaveDirection140 = _Wave1Direction;
				float4 appendResult10 = (float4(WorldPosition.x , WorldPosition.z , 0.0 , 0.0));
				float4 WorldSpaceTile11 = appendResult10;
				float4 WaveTileUV38 = ( ( WorldSpaceTile11 * float4( float2( 1,1 ), 0.0 , 0.0 ) * 0.1 ) * _WaveTile );
				float2 panner4 = ( WaveSpeed43 * WaveDirection140 + WaveTileUV38.xy);
				float2 Input_UV145_g24 = panner4;
				float2 UV2_g24 = Input_UV145_g24;
				float2 UV12_g24 = float2( 0,0 );
				float2 UV22_g24 = float2( 0,0 );
				float2 UV32_g24 = float2( 0,0 );
				float W12_g24 = 0.0;
				float W22_g24 = 0.0;
				float W32_g24 = 0.0;
				StochasticTiling( UV2_g24 , UV12_g24 , UV22_g24 , UV32_g24 , W12_g24 , W22_g24 , W32_g24 );
				float2 temp_output_10_0_g24 = ddx( Input_UV145_g24 );
				float2 temp_output_12_0_g24 = ddy( Input_UV145_g24 );
				float4 Output_2D293_g24 = ( ( tex2D( _HeightMap, UV12_g24, temp_output_10_0_g24, temp_output_12_0_g24 ) * W12_g24 ) + ( tex2D( _HeightMap, UV22_g24, temp_output_10_0_g24, temp_output_12_0_g24 ) * W22_g24 ) + ( tex2D( _HeightMap, UV32_g24, temp_output_10_0_g24, temp_output_12_0_g24 ) * W32_g24 ) );
				float4 break31_g24 = Output_2D293_g24;
				float localStochasticTiling2_g23 = ( 0.0 );
				float2 WaveDirection2201 = _Wave2Direction;
				float2 panner28 = ( WaveSpeed43 * WaveDirection2201 + ( WaveTileUV38 * 0.9 ).xy);
				float2 Input_UV145_g23 = panner28;
				float2 UV2_g23 = Input_UV145_g23;
				float2 UV12_g23 = float2( 0,0 );
				float2 UV22_g23 = float2( 0,0 );
				float2 UV32_g23 = float2( 0,0 );
				float W12_g23 = 0.0;
				float W22_g23 = 0.0;
				float W32_g23 = 0.0;
				StochasticTiling( UV2_g23 , UV12_g23 , UV22_g23 , UV32_g23 , W12_g23 , W22_g23 , W32_g23 );
				float2 temp_output_10_0_g23 = ddx( Input_UV145_g23 );
				float2 temp_output_12_0_g23 = ddy( Input_UV145_g23 );
				float4 Output_2D293_g23 = ( ( tex2D( _HeightMap, UV12_g23, temp_output_10_0_g23, temp_output_12_0_g23 ) * W12_g23 ) + ( tex2D( _HeightMap, UV22_g23, temp_output_10_0_g23, temp_output_12_0_g23 ) * W22_g23 ) + ( tex2D( _HeightMap, UV32_g23, temp_output_10_0_g23, temp_output_12_0_g23 ) * W32_g23 ) );
				float4 break31_g23 = Output_2D293_g23;
				float FineWaves100 = ( break31_g24.g + break31_g23.g );
				float localStochasticTiling2_g22 = ( 0.0 );
				float2 panner78 = ( ( WaveSpeed43 * 0.15 ) * WaveDirection140 + ( WaveTileUV38 * 0.1 ).xy);
				float2 Input_UV145_g22 = panner78;
				float2 UV2_g22 = Input_UV145_g22;
				float2 UV12_g22 = float2( 0,0 );
				float2 UV22_g22 = float2( 0,0 );
				float2 UV32_g22 = float2( 0,0 );
				float W12_g22 = 0.0;
				float W22_g22 = 0.0;
				float W32_g22 = 0.0;
				StochasticTiling( UV2_g22 , UV12_g22 , UV22_g22 , UV32_g22 , W12_g22 , W22_g22 , W32_g22 );
				float2 temp_output_10_0_g22 = ddx( Input_UV145_g22 );
				float2 temp_output_12_0_g22 = ddy( Input_UV145_g22 );
				float4 Output_2D293_g22 = ( ( tex2D( _HeightMap, UV12_g22, temp_output_10_0_g22, temp_output_12_0_g22 ) * W12_g22 ) + ( tex2D( _HeightMap, UV22_g22, temp_output_10_0_g22, temp_output_12_0_g22 ) * W22_g22 ) + ( tex2D( _HeightMap, UV32_g22, temp_output_10_0_g22, temp_output_12_0_g22 ) * W32_g22 ) );
				float4 break31_g22 = Output_2D293_g22;
				float localStochasticTiling2_g21 = ( 0.0 );
				float2 panner73 = ( ( WaveSpeed43 * 0.15 ) * WaveDirection2201 + ( WaveTileUV38 * 0.09 ).xy);
				float2 Input_UV145_g21 = panner73;
				float2 UV2_g21 = Input_UV145_g21;
				float2 UV12_g21 = float2( 0,0 );
				float2 UV22_g21 = float2( 0,0 );
				float2 UV32_g21 = float2( 0,0 );
				float W12_g21 = 0.0;
				float W22_g21 = 0.0;
				float W32_g21 = 0.0;
				StochasticTiling( UV2_g21 , UV12_g21 , UV22_g21 , UV32_g21 , W12_g21 , W22_g21 , W32_g21 );
				float2 temp_output_10_0_g21 = ddx( Input_UV145_g21 );
				float2 temp_output_12_0_g21 = ddy( Input_UV145_g21 );
				float4 Output_2D293_g21 = ( ( tex2D( _HeightMap, UV12_g21, temp_output_10_0_g21, temp_output_12_0_g21 ) * W12_g21 ) + ( tex2D( _HeightMap, UV22_g21, temp_output_10_0_g21, temp_output_12_0_g21 ) * W22_g21 ) + ( tex2D( _HeightMap, UV32_g21, temp_output_10_0_g21, temp_output_12_0_g21 ) * W32_g21 ) );
				float4 break31_g21 = Output_2D293_g21;
				float LargeWaves101 = ( break31_g22.g + break31_g21.g );
				float lerpResult110 = lerp( ( FineWaves100 * _SmallWaveStrength ) , ( ( LargeWaves101 * 5.0 ) * _LargeWaveStrength ) , _WaveSizeBalance);
				float FinalWave106 = lerpResult110;
				float height107_g25 = FinalWave106;
				float scale107_g25 = _NormalStrength;
				float3 localPerturbNormal107_g25 = PerturbNormal107_g25( surf_pos107_g25 , surf_norm107_g25 , height107_g25 , scale107_g25 );
				float3 ase_worldTangent = IN.ase_texcoord4.xyz;
				float3 ase_worldBitangent = IN.ase_texcoord5.xyz;
				float3x3 ase_worldToTangent = float3x3(ase_worldTangent,ase_worldBitangent,ase_worldNormal);
				float3 worldToTangentDir42_g25 = mul( ase_worldToTangent, localPerturbNormal107_g25);
				float3 normalizeResult56 = normalize( worldToTangentDir42_g25 );
				float3 Normal150 = normalizeResult56;
				float eyeDepth = IN.ase_texcoord3.w;
				float eyeDepth28_g26 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( ase_screenPosNorm.xy ),_ZBufferParams);
				float2 temp_output_20_0_g26 = ( (Normal150).xy * ( _RefractionStrength / max( eyeDepth , 0.1 ) ) * saturate( ( eyeDepth28_g26 - eyeDepth ) ) );
				float eyeDepth2_g26 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( ( float4( temp_output_20_0_g26, 0.0 , 0.0 ) + ase_screenPosNorm ).xy ),_ZBufferParams);
				float2 temp_output_32_0_g26 = (( float4( ( temp_output_20_0_g26 * saturate( ( eyeDepth2_g26 - eyeDepth ) ) ), 0.0 , 0.0 ) + ase_screenPosNorm )).xy;
				float4 fetchOpaqueVal154 = float4( SHADERGRAPH_SAMPLE_SCENE_COLOR( temp_output_32_0_g26 ), 1.0 );
				float4 clampResult155 = clamp( fetchOpaqueVal154 , float4( 0,0,0,0 ) , float4( 1,1,1,0 ) );
				float4 Refraction156 = clampResult155;
				float screenDepth158 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( ase_screenPosNorm.xy ),_ZBufferParams);
				float distanceDepth158 = abs( ( screenDepth158 - LinearEyeDepth( ase_screenPosNorm.z,_ZBufferParams ) ) / ( ( -10.0 * _VisibilityDepth ) ) );
				float clampResult160 = clamp( ( 1.0 - distanceDepth158 ) , 0.0 , 1.0 );
				float Depth161 = clampResult160;
				float4 lerpResult167 = lerp( Albedo165 , Refraction156 , Depth161);
				
				float screenDepth194 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( ase_screenPosNorm.xy ),_ZBufferParams);
				float distanceDepth194 = abs( ( screenDepth194 - LinearEyeDepth( ase_screenPosNorm.z,_ZBufferParams ) ) / ( _EdgeFadeDistance ) );
				float clampResult196 = clamp( distanceDepth194 , 0.0 , 1.0 );
				float Edge137 = clampResult196;
				
				
				float3 Albedo = lerpResult167.rgb;
				float Alpha = Edge137;
				float AlphaClipThreshold = 0.5;

				half4 color = half4( Albedo, Alpha );

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				return color;
			}
			ENDHLSL
		}
		
	}
	/*ase_lod*/
	CustomEditor "UnityEditor.ShaderGraph.PBRMasterGUI"
	Fallback "Hidden/InternalErrorShader"
	
}
/*ASEBEGIN
Version=18909
1595;239;1546;929;3972.235;-499.4237;1.06298;True;False
Node;AmplifyShaderEditor.CommentaryNode;12;-4760.234,-2555.54;Inherit;False;646.9005;235.6001;World Space UVs;3;9;10;11;;1,1,1,1;0;0
Node;AmplifyShaderEditor.WorldPosInputsNode;9;-4710.234,-2505.541;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DynamicAppendNode;10;-4504.832,-2502.94;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;11;-4342.335,-2439.24;Inherit;False;WorldSpaceTile;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.Vector2Node;14;-5735.903,-1920.992;Inherit;False;Constant;_Tile;Tile;1;0;Create;True;0;0;0;False;0;False;1,1;1,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.GetLocalVarNode;13;-5742.278,-2017.991;Inherit;False;11;WorldSpaceTile;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;119;-5727.095,-1688.521;Inherit;False;Constant;_Float10;Float 10;11;0;Create;True;0;0;0;False;0;False;0.1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;6;-5586.87,-569.9277;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;120;-5577.328,-220.6299;Inherit;False;Constant;_Float11;Float 11;11;0;Create;True;0;0;0;False;0;False;0.1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;7;-5595.87,-452.9277;Inherit;True;Property;_WaveSpeed;Wave Speed;3;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;17;-5544.711,-1706.531;Inherit;False;Property;_WaveTile;Wave Tile;2;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;15;-5461.601,-1930.091;Inherit;False;3;3;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;8;-5242.869,-651.9277;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;16;-5217.861,-1820.801;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.Vector2Node;5;-5834.503,-1357.935;Inherit;False;Property;_Wave1Direction;Wave 1 Direction;14;0;Create;True;0;0;0;False;0;False;0,1;0.5,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.RegisterLocalVarNode;43;-5076.113,-648.4447;Inherit;False;WaveSpeed;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;200;-5826.983,-1186.669;Inherit;False;Property;_Wave2Direction;Wave 2 Direction;15;0;Create;True;0;0;0;False;0;False;0,1;1,-0.5;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.CommentaryNode;143;-5638.304,2433.468;Inherit;False;3530.118;2817.894;Comment;49;88;64;111;106;110;112;113;116;102;115;114;117;103;100;32;101;76;70;69;4;75;74;89;28;78;73;45;30;39;90;41;44;96;80;85;33;42;97;31;92;81;84;93;95;91;86;98;82;87;Waves;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;38;-5017.059,-1820.383;Inherit;False;WaveTileUV;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;93;-5194.613,3820.226;Inherit;False;38;WaveTileUV;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;86;-5058.482,4641.113;Inherit;False;38;WaveTileUV;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;82;-5001.177,4969.02;Inherit;False;43;WaveSpeed;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;91;-5157.968,3923.34;Inherit;False;Constant;_Float6;Float 6;4;0;Create;True;0;0;0;False;0;False;0.1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;95;-4975.509,5087.5;Inherit;False;Constant;_Float7;Float 7;4;0;Create;True;0;0;0;False;0;False;0.15;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;201;-5616.983,-1179.669;Inherit;False;WaveDirection2;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;81;-5235.637,4110.784;Inherit;False;43;WaveSpeed;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;87;-5021.837,4745.228;Inherit;False;Constant;_Float5;Float 5;4;0;Create;True;0;0;0;False;0;False;0.09;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;40;-5629.605,-1360.738;Inherit;False;WaveDirection1;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TexturePropertyNode;64;-5571.796,2853.746;Inherit;True;Property;_HeightMap;Height Map;9;0;Create;True;0;0;0;False;0;False;7667350b67a6e05408eab5d05068039b;1a7a9e141b92fe84e972535ec0345f29;False;gray;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.RangedFloatNode;98;-5120.533,4252.913;Inherit;False;Constant;_Float8;Float 8;4;0;Create;True;0;0;0;False;0;False;0.15;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;97;-4899.232,4120.913;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;88;-5295.309,2881.647;Inherit;False;HeightTexture;-1;True;1;0;SAMPLER2D;0;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.GetLocalVarNode;80;-4963.494,4016.977;Inherit;False;40;WaveDirection1;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;33;-5025.857,3204.853;Inherit;False;38;WaveTileUV;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;96;-4754.208,4955.5;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;85;-4777.046,4712.301;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;31;-4989.212,3307.967;Inherit;False;Constant;_Float3;Float 3;4;0;Create;True;0;0;0;False;0;False;0.9;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;84;-5036.469,4857.735;Inherit;False;201;WaveDirection2;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;92;-4913.176,3891.413;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.PannerNode;73;-4542.762,4795.128;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;44;-4911.811,2675.823;Inherit;False;43;WaveSpeed;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;39;-4912.662,2483.468;Inherit;False;38;WaveTileUV;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.PannerNode;78;-4651.242,3999.572;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;41;-4930.867,2580.715;Inherit;False;40;WaveDirection1;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;42;-5004.843,3421.474;Inherit;False;201;WaveDirection2;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;90;-4582.514,4400.983;Inherit;False;88;HeightTexture;1;0;OBJECT;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.GetLocalVarNode;45;-4750.831,3515.44;Inherit;False;43;WaveSpeed;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;30;-4744.419,3276.04;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.PannerNode;4;-4618.615,2563.311;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;89;-4407.807,2998.259;Inherit;False;88;HeightTexture;1;0;OBJECT;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.FunctionNode;75;-4159.429,4315.996;Inherit;False;Procedural Sample;-1;;22;f5379ff72769e2b4495e5ce2f004d8d4;2,157,0,315,0;7;82;SAMPLER2D;0;False;158;SAMPLER2DARRAY;0;False;183;FLOAT;0;False;5;FLOAT2;0,0;False;80;FLOAT3;0,0,0;False;104;FLOAT2;1,0;False;74;SAMPLERSTATE;0;False;5;COLOR;0;FLOAT;32;FLOAT;33;FLOAT;34;FLOAT;35
Node;AmplifyShaderEditor.FunctionNode;74;-4168.792,4546.537;Inherit;False;Procedural Sample;-1;;21;f5379ff72769e2b4495e5ce2f004d8d4;2,157,0,315,0;7;82;SAMPLER2D;0;False;158;SAMPLER2DARRAY;0;False;183;FLOAT;0;False;5;FLOAT2;0,0;False;80;FLOAT3;0,0,0;False;104;FLOAT2;1,0;False;74;SAMPLERSTATE;0;False;5;COLOR;0;FLOAT;32;FLOAT;33;FLOAT;34;FLOAT;35
Node;AmplifyShaderEditor.PannerNode;28;-4510.135,3358.867;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.FunctionNode;70;-4136.166,3110.276;Inherit;False;Procedural Sample;-1;;23;f5379ff72769e2b4495e5ce2f004d8d4;2,157,0,315,0;7;82;SAMPLER2D;0;False;158;SAMPLER2DARRAY;0;False;183;FLOAT;0;False;5;FLOAT2;0,0;False;80;FLOAT3;0,0,0;False;104;FLOAT2;1,0;False;74;SAMPLERSTATE;0;False;5;COLOR;0;FLOAT;32;FLOAT;33;FLOAT;34;FLOAT;35
Node;AmplifyShaderEditor.SimpleAddOpNode;76;-3929.56,4466.091;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;69;-4126.802,2879.734;Inherit;False;Procedural Sample;-1;;24;f5379ff72769e2b4495e5ce2f004d8d4;2,157,0,315,0;7;82;SAMPLER2D;0;False;158;SAMPLER2DARRAY;0;False;183;FLOAT;0;False;5;FLOAT2;0,0;False;80;FLOAT3;0,0,0;False;104;FLOAT2;1,0;False;74;SAMPLERSTATE;0;False;5;COLOR;0;FLOAT;32;FLOAT;33;FLOAT;34;FLOAT;35
Node;AmplifyShaderEditor.SimpleAddOpNode;32;-3895.887,3028.619;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;101;-3762.811,4467.06;Inherit;False;LargeWaves;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;100;-3760.904,3021.727;Inherit;False;FineWaves;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;117;-3306.562,3894.335;Inherit;False;Constant;_Float9;Float 9;11;0;Create;True;0;0;0;False;0;False;5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;103;-3288.036,3778.709;Inherit;False;101;LargeWaves;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;116;-3087.562,3771.335;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;102;-3272.903,3531.807;Inherit;False;100;FineWaves;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;114;-3117.481,3894.19;Inherit;False;Property;_LargeWaveStrength;Large Wave Strength;13;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;115;-3108.481,3656.19;Inherit;False;Property;_SmallWaveStrength;Small Wave Strength;12;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;112;-2888.481,3792.19;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;113;-2896.481,3533.19;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;111;-3056.481,4064.19;Inherit;False;Property;_WaveSizeBalance;Wave Size Balance;11;0;Create;True;0;0;0;False;0;False;0.5;0.75;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;110;-2704.481,3696.19;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;163;-2758.043,-320.7726;Inherit;False;1332.39;434.1833;Comment;5;54;53;56;150;128;Normal;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;106;-2440.265,3701.076;Inherit;False;FinalWave;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;54;-2712.844,-259.1401;Inherit;False;106;FinalWave;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;128;-2608.246,-108.1385;Inherit;False;Property;_NormalStrength;Normal Strength;7;0;Create;True;0;0;0;False;0;False;1.5;2;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;53;-2286.09,-270.7726;Inherit;False;Normal From Height;-1;;25;1942fe2c5f1a1f94881a33d532e4afeb;0;2;20;FLOAT;0;False;110;FLOAT;1;False;2;FLOAT3;40;FLOAT3;0
Node;AmplifyShaderEditor.CommentaryNode;170;-1966.599,-1544.496;Inherit;False;1604.884;852.724;Comment;10;165;61;60;59;180;181;179;182;183;184;Albedo;1,1,1,1;0;0
Node;AmplifyShaderEditor.NormalizeNode;56;-1932.208,-270.4749;Inherit;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CommentaryNode;164;-3761.485,647.5096;Inherit;False;1805.239;1040.284;Comment;18;147;144;156;155;154;152;148;153;146;145;159;158;160;161;174;185;186;208;Refraction;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;179;-1941.95,-1162.771;Inherit;False;Property;_WaterColorDepth;Water Color Depth;4;0;Create;True;0;0;0;False;0;False;0.5;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;184;-1881.55,-1003.997;Inherit;False;Constant;_Float13;Float 13;15;0;Create;True;0;0;0;False;0;False;-10;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;159;-3233.623,1534.704;Inherit;False;Property;_VisibilityDepth;Visibility Depth;6;0;Create;True;0;0;0;False;0;False;0.5;0.493;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;183;-1687.55,-1098.997;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;185;-3167.585,1397.578;Inherit;False;Constant;_Float15;Float 15;15;0;Create;True;0;0;0;False;0;False;-10;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;150;-1649.653,-262.3986;Inherit;False;Normal;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;152;-3672.875,886.0894;Inherit;False;150;Normal;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;146;-3711.485,1011.444;Inherit;False;Property;_RefractionStrength;Refraction Strength;8;0;Create;True;0;0;0;False;0;False;0.5;0.25;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.DepthFade;180;-1655.563,-821.0214;Inherit;False;True;False;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;186;-2960.617,1423.578;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;171;-2997.834,-1960.812;Inherit;False;904.627;399.396;Comment;6;26;108;27;47;191;192;Wave Up;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;192;-2917.256,-1651.829;Inherit;False;Constant;_Float0;Float 0;14;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;26;-2931.277,-1910.812;Inherit;False;Constant;_WaveUp;Wave Up;4;0;Create;True;0;0;0;False;0;False;0,1,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.OneMinusNode;181;-1374.057,-811.9603;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DepthFade;158;-2821.235,1519.454;Inherit;False;True;False;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;208;-3318.598,1078.488;Inherit;False;DepthMaskedRefraction;-1;;26;c805f061214177c42bca056464193f81;2,40,0,103,0;2;35;FLOAT3;0,0,0;False;37;FLOAT;0.02;False;1;FLOAT2;38
Node;AmplifyShaderEditor.CommentaryNode;187;-879.53,-2318.772;Inherit;False;1133.572;301.2458;Comment;4;193;194;196;137;Edge Fade;1,1,1,1;0;0
Node;AmplifyShaderEditor.ClampOpNode;182;-1199.552,-811.6813;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;59;-1160.321,-1494.496;Inherit;False;Property;_DeepColor;Deep Color;1;0;Create;True;0;0;0;False;0;False;0.1117391,0.2199386,0.254717,1;0.1149429,0.3867924,0.345606,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ScreenColorNode;154;-2953.264,795.2388;Inherit;False;Global;_GrabScreen0;Grab Screen 0;13;0;Create;True;0;0;0;False;0;False;Object;-1;False;False;False;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;60;-1160.007,-1243.792;Inherit;False;Property;_ShallowColor;Shallow Color;0;0;Create;True;0;0;0;False;0;False;0.2736739,0.4617023,0.4716981,1;0.23487,0.5236304,0.5471698,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;174;-2539.729,1528.515;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;193;-836.1754,-2180.16;Inherit;False;Property;_EdgeFadeDistance;Edge Fade Distance;5;0;Create;True;0;0;0;False;0;False;0.2;0.2;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;108;-2704.994,-1677.416;Inherit;False;106;FinalWave;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;191;-2709.256,-1843.829;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DepthFade;194;-521.1755,-2197.16;Inherit;False;True;False;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;27;-2503.092,-1819.087;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;61;-784.5994,-1345.772;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;155;-2759.054,800.3767;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,1,1,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;160;-2365.225,1528.794;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;161;-2180.246,1564.134;Inherit;False;Depth;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;196;-199.1754,-2212.16;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;47;-2317.207,-1823.385;Inherit;False;Wave;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;165;-585.716,-1332.747;Inherit;False;Albedo;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;156;-2580.257,816.8177;Inherit;False;Refraction;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;137;49.04218,-2183.272;Inherit;False;Edge;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;49;-52.07251,264.6881;Inherit;False;47;Wave;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;166;-39.83185,-462.6146;Inherit;False;165;Albedo;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;199;-21.81177,438.3529;Inherit;False;Constant;_Float12;Float 12;15;0;Create;True;0;0;0;False;0;False;0.1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;198;-42.64716,352.6791;Inherit;False;Property;_WaveHeight;Wave Height;10;0;Create;True;0;0;0;False;0;False;0;2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;168;-59.69727,-333.2135;Inherit;False;156;Refraction;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;169;-175.4747,-225.142;Inherit;False;161;Depth;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;148;-3571.685,1112.544;Inherit;False;Constant;_Float14;Float 14;14;0;Create;True;0;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;197;163.3528,260.6791;Inherit;False;3;3;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;50;106.751,66.336;Inherit;False;Constant;_Float4;Float 4;4;0;Create;True;0;0;0;False;0;False;0.98;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;190;92.0825,182.5227;Inherit;False;137;Edge;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;151;95.11841,-10.74243;Inherit;False;150;Normal;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;153;-3138.227,829.1487;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;147;-3364.585,930.6438;Inherit;False;3;3;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ComponentMaskNode;145;-3355.564,702.2587;Inherit;False;True;True;False;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GrabScreenPosition;144;-3682.625,697.5096;Inherit;False;0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;167;201.1682,-464.6146;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;202;417.4008,-28.8386;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;2;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;ExtraPrePass;0;0;ExtraPrePass;5;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;False;True;1;1;False;-1;0;False;-1;0;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;0;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;203;417.4008,-28.8386;Float;False;True;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;2;M3DS/Water;94348b07e5e8bab40bd6c8a1e3df54cd;True;Forward;0;1;Forward;18;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;2;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Transparent=RenderType;Queue=Transparent=Queue=0;True;0;0;False;True;1;5;False;-1;10;False;-1;1;1;False;-1;10;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;2;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;1;LightMode=UniversalForward;False;0;Hidden/InternalErrorShader;0;0;Standard;38;Workflow;1;Surface;1;  Refraction Model;0;  Blend;0;Two Sided;0;Fragment Normal Space,InvertActionOnDeselection;0;Transmission;0;  Transmission Shadow;0.5,False,-1;Translucency;0;  Translucency Strength;1,False,-1;  Normal Distortion;0.5,False,-1;  Scattering;2,False,-1;  Direct;0.9,False,-1;  Ambient;0.1,False,-1;  Shadow;0.5,False,-1;Cast Shadows;0;  Use Shadow Threshold;0;Receive Shadows;1;GPU Instancing;0;LOD CrossFade;0;Built-in Fog;1;_FinalColorxAlpha;0;Meta Pass;1;Override Baked GI;0;Extra Pre Pass;0;DOTS Instancing;0;Tessellation;1;  Phong;0;  Strength;0.5,False,-1;  Type;1;  Tess;16,False,-1;  Min;10,False,-1;  Max;25,False,-1;  Edge Length;16,False,-1;  Max Displacement;25,False,-1;Write Depth;0;  Early Z;0;Vertex Position,InvertActionOnDeselection;1;0;6;False;True;False;True;True;True;False;;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;205;417.4008,-28.8386;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;2;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;DepthOnly;0;3;DepthOnly;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;False;False;True;False;False;False;False;0;False;-1;False;False;False;False;False;False;False;False;False;True;1;False;-1;False;False;True;1;LightMode=DepthOnly;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;204;417.4008,-28.8386;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;2;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;ShadowCaster;0;2;ShadowCaster;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;False;-1;True;3;False;-1;False;True;1;LightMode=ShadowCaster;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;206;417.4008,-28.8386;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;2;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;Meta;0;4;Meta;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=Meta;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;207;417.4008,-28.8386;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;2;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;Universal2D;0;5;Universal2D;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;False;True;1;5;False;-1;10;False;-1;1;1;False;-1;10;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;2;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;1;LightMode=Universal2D;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
WireConnection;10;0;9;1
WireConnection;10;1;9;3
WireConnection;11;0;10;0
WireConnection;15;0;13;0
WireConnection;15;1;14;0
WireConnection;15;2;119;0
WireConnection;8;0;6;0
WireConnection;8;1;7;0
WireConnection;8;2;120;0
WireConnection;16;0;15;0
WireConnection;16;1;17;0
WireConnection;43;0;8;0
WireConnection;38;0;16;0
WireConnection;201;0;200;0
WireConnection;40;0;5;0
WireConnection;97;0;81;0
WireConnection;97;1;98;0
WireConnection;88;0;64;0
WireConnection;96;0;82;0
WireConnection;96;1;95;0
WireConnection;85;0;86;0
WireConnection;85;1;87;0
WireConnection;92;0;93;0
WireConnection;92;1;91;0
WireConnection;73;0;85;0
WireConnection;73;2;84;0
WireConnection;73;1;96;0
WireConnection;78;0;92;0
WireConnection;78;2;80;0
WireConnection;78;1;97;0
WireConnection;30;0;33;0
WireConnection;30;1;31;0
WireConnection;4;0;39;0
WireConnection;4;2;41;0
WireConnection;4;1;44;0
WireConnection;75;82;90;0
WireConnection;75;5;78;0
WireConnection;74;82;90;0
WireConnection;74;5;73;0
WireConnection;28;0;30;0
WireConnection;28;2;42;0
WireConnection;28;1;45;0
WireConnection;70;82;89;0
WireConnection;70;5;28;0
WireConnection;76;0;75;33
WireConnection;76;1;74;33
WireConnection;69;82;89;0
WireConnection;69;5;4;0
WireConnection;32;0;69;33
WireConnection;32;1;70;33
WireConnection;101;0;76;0
WireConnection;100;0;32;0
WireConnection;116;0;103;0
WireConnection;116;1;117;0
WireConnection;112;0;116;0
WireConnection;112;1;114;0
WireConnection;113;0;102;0
WireConnection;113;1;115;0
WireConnection;110;0;113;0
WireConnection;110;1;112;0
WireConnection;110;2;111;0
WireConnection;106;0;110;0
WireConnection;53;20;54;0
WireConnection;53;110;128;0
WireConnection;56;0;53;40
WireConnection;183;0;179;0
WireConnection;183;1;184;0
WireConnection;150;0;56;0
WireConnection;180;0;183;0
WireConnection;186;0;185;0
WireConnection;186;1;159;0
WireConnection;181;0;180;0
WireConnection;158;0;186;0
WireConnection;208;35;152;0
WireConnection;208;37;146;0
WireConnection;182;0;181;0
WireConnection;154;0;208;38
WireConnection;174;0;158;0
WireConnection;191;0;26;0
WireConnection;191;1;192;0
WireConnection;194;0;193;0
WireConnection;27;0;191;0
WireConnection;27;1;108;0
WireConnection;61;0;59;0
WireConnection;61;1;60;0
WireConnection;61;2;182;0
WireConnection;155;0;154;0
WireConnection;160;0;174;0
WireConnection;161;0;160;0
WireConnection;196;0;194;0
WireConnection;47;0;27;0
WireConnection;165;0;61;0
WireConnection;156;0;155;0
WireConnection;137;0;196;0
WireConnection;197;0;49;0
WireConnection;197;1;198;0
WireConnection;197;2;199;0
WireConnection;153;0;145;0
WireConnection;153;1;147;0
WireConnection;147;0;152;0
WireConnection;147;1;146;0
WireConnection;147;2;148;0
WireConnection;145;0;144;0
WireConnection;167;0;166;0
WireConnection;167;1;168;0
WireConnection;167;2;169;0
WireConnection;203;0;167;0
WireConnection;203;1;151;0
WireConnection;203;4;50;0
WireConnection;203;6;190;0
WireConnection;203;8;197;0
ASEEND*/
//CHKSM=912FBD242A435D3001C6BEE0336188340D199DF3