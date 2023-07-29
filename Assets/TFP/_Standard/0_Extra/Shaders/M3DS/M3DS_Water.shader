// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "M3DS/Water"
{
	Properties
	{
		_ShallowColor("Shallow Color", Color) = (0.2736739,0.4617023,0.4716981,1)
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
		_Wave2Direction("Wave 2 Direction", Vector) = (0,1,0,0)
		_TesselationAmount("Tesselation Amount", Range( 0.01 , 1)) = 0.5
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" }
		Cull Off
		GrabPass{ }
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#include "UnityCG.cginc"
		#include "Tessellation.cginc"
		#pragma target 4.6
		#if defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
		#define ASE_DECLARE_SCREENSPACE_TEXTURE(tex) UNITY_DECLARE_SCREENSPACE_TEXTURE(tex);
		#else
		#define ASE_DECLARE_SCREENSPACE_TEXTURE(tex) UNITY_DECLARE_SCREENSPACE_TEXTURE(tex)
		#endif
		#pragma surface surf Standard alpha:fade keepalpha noshadow exclude_path:deferred vertex:vertexDataFunc tessellate:tessFunction 
		struct Input
		{
			float3 worldPos;
			float3 worldNormal;
			INTERNAL_DATA
			float4 screenPos;
		};

		uniform sampler2D _HeightMap;
		uniform float _WaveSpeed;
		uniform float2 _Wave1Direction;
		uniform float _WaveTile;
		uniform float2 _Wave2Direction;
		uniform float _SmallWaveStrength;
		uniform float _LargeWaveStrength;
		uniform float _WaveSizeBalance;
		uniform float _WaveHeight;
		uniform float _NormalStrength;
		uniform float4 _DeepColor;
		uniform float4 _ShallowColor;
		UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
		uniform float4 _CameraDepthTexture_TexelSize;
		uniform float _WaterColorDepth;
		ASE_DECLARE_SCREENSPACE_TEXTURE( _GrabTexture )
		uniform float _RefractionStrength;
		uniform float _VisibilityDepth;
		uniform float _EdgeFadeDistance;
		uniform float _TesselationAmount;


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


		float3 PerturbNormal107_g24( float3 surf_pos, float3 surf_norm, float height, float scale )
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


		float4 tessFunction( appdata_full v0, appdata_full v1, appdata_full v2 )
		{
			return UnityDistanceBasedTess( v0.vertex, v1.vertex, v2.vertex, 10.0,50.0,( _TesselationAmount * 50.0 ));
		}

		void vertexDataFunc( inout appdata_full v )
		{
			float localStochasticTiling2_g23 = ( 0.0 );
			float WaveSpeed43 = ( _Time.y * _WaveSpeed * 0.1 );
			float2 WaveDirection140 = _Wave1Direction;
			float3 ase_worldPos = mul( unity_ObjectToWorld, v.vertex );
			float4 appendResult10 = (float4(ase_worldPos.x , ase_worldPos.z , 0.0 , 0.0));
			float4 WorldSpaceTile11 = appendResult10;
			float4 WaveTileUV38 = ( ( WorldSpaceTile11 * float4( float2( 1,1 ), 0.0 , 0.0 ) * 0.1 ) * _WaveTile );
			float2 panner4 = ( WaveSpeed43 * WaveDirection140 + WaveTileUV38.xy);
			float2 Input_UV145_g23 = panner4;
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
			float localStochasticTiling2_g22 = ( 0.0 );
			float2 WaveDirection2201 = _Wave2Direction;
			float2 panner28 = ( WaveSpeed43 * WaveDirection2201 + ( WaveTileUV38 * 0.9 ).xy);
			float2 Input_UV145_g22 = panner28;
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
			float FineWaves100 = ( break31_g23.g + break31_g22.g );
			float localStochasticTiling2_g20 = ( 0.0 );
			float2 panner78 = ( ( WaveSpeed43 * 0.15 ) * WaveDirection140 + ( WaveTileUV38 * 0.1 ).xy);
			float2 Input_UV145_g20 = panner78;
			float2 UV2_g20 = Input_UV145_g20;
			float2 UV12_g20 = float2( 0,0 );
			float2 UV22_g20 = float2( 0,0 );
			float2 UV32_g20 = float2( 0,0 );
			float W12_g20 = 0.0;
			float W22_g20 = 0.0;
			float W32_g20 = 0.0;
			StochasticTiling( UV2_g20 , UV12_g20 , UV22_g20 , UV32_g20 , W12_g20 , W22_g20 , W32_g20 );
			float4 Output_2D293_g20 = ( ( tex2Dlod( _HeightMap, float4( UV12_g20, 0, 0.0) ) * W12_g20 ) + ( tex2Dlod( _HeightMap, float4( UV22_g20, 0, 0.0) ) * W22_g20 ) + ( tex2Dlod( _HeightMap, float4( UV32_g20, 0, 0.0) ) * W32_g20 ) );
			float4 break31_g20 = Output_2D293_g20;
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
			float LargeWaves101 = ( break31_g20.g + break31_g21.g );
			float lerpResult110 = lerp( ( FineWaves100 * _SmallWaveStrength ) , ( ( LargeWaves101 * 5.0 ) * _LargeWaveStrength ) , _WaveSizeBalance);
			float FinalWave106 = lerpResult110;
			float3 Wave47 = ( ( float3(0,1,0) * 1.0 ) * FinalWave106 );
			v.vertex.xyz += ( Wave47 * _WaveHeight * 0.1 );
			v.vertex.w = 1;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float3 ase_worldPos = i.worldPos;
			float3 surf_pos107_g24 = ase_worldPos;
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float3 surf_norm107_g24 = ase_worldNormal;
			float localStochasticTiling2_g23 = ( 0.0 );
			float WaveSpeed43 = ( _Time.y * _WaveSpeed * 0.1 );
			float2 WaveDirection140 = _Wave1Direction;
			float4 appendResult10 = (float4(ase_worldPos.x , ase_worldPos.z , 0.0 , 0.0));
			float4 WorldSpaceTile11 = appendResult10;
			float4 WaveTileUV38 = ( ( WorldSpaceTile11 * float4( float2( 1,1 ), 0.0 , 0.0 ) * 0.1 ) * _WaveTile );
			float2 panner4 = ( WaveSpeed43 * WaveDirection140 + WaveTileUV38.xy);
			float2 Input_UV145_g23 = panner4;
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
			float localStochasticTiling2_g22 = ( 0.0 );
			float2 WaveDirection2201 = _Wave2Direction;
			float2 panner28 = ( WaveSpeed43 * WaveDirection2201 + ( WaveTileUV38 * 0.9 ).xy);
			float2 Input_UV145_g22 = panner28;
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
			float FineWaves100 = ( break31_g23.g + break31_g22.g );
			float localStochasticTiling2_g20 = ( 0.0 );
			float2 panner78 = ( ( WaveSpeed43 * 0.15 ) * WaveDirection140 + ( WaveTileUV38 * 0.1 ).xy);
			float2 Input_UV145_g20 = panner78;
			float2 UV2_g20 = Input_UV145_g20;
			float2 UV12_g20 = float2( 0,0 );
			float2 UV22_g20 = float2( 0,0 );
			float2 UV32_g20 = float2( 0,0 );
			float W12_g20 = 0.0;
			float W22_g20 = 0.0;
			float W32_g20 = 0.0;
			StochasticTiling( UV2_g20 , UV12_g20 , UV22_g20 , UV32_g20 , W12_g20 , W22_g20 , W32_g20 );
			float2 temp_output_10_0_g20 = ddx( Input_UV145_g20 );
			float2 temp_output_12_0_g20 = ddy( Input_UV145_g20 );
			float4 Output_2D293_g20 = ( ( tex2D( _HeightMap, UV12_g20, temp_output_10_0_g20, temp_output_12_0_g20 ) * W12_g20 ) + ( tex2D( _HeightMap, UV22_g20, temp_output_10_0_g20, temp_output_12_0_g20 ) * W22_g20 ) + ( tex2D( _HeightMap, UV32_g20, temp_output_10_0_g20, temp_output_12_0_g20 ) * W32_g20 ) );
			float4 break31_g20 = Output_2D293_g20;
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
			float LargeWaves101 = ( break31_g20.g + break31_g21.g );
			float lerpResult110 = lerp( ( FineWaves100 * _SmallWaveStrength ) , ( ( LargeWaves101 * 5.0 ) * _LargeWaveStrength ) , _WaveSizeBalance);
			float FinalWave106 = lerpResult110;
			float height107_g24 = FinalWave106;
			float scale107_g24 = _NormalStrength;
			float3 localPerturbNormal107_g24 = PerturbNormal107_g24( surf_pos107_g24 , surf_norm107_g24 , height107_g24 , scale107_g24 );
			float3 ase_worldTangent = WorldNormalVector( i, float3( 1, 0, 0 ) );
			float3 ase_worldBitangent = WorldNormalVector( i, float3( 0, 1, 0 ) );
			float3x3 ase_worldToTangent = float3x3( ase_worldTangent, ase_worldBitangent, ase_worldNormal );
			float3 worldToTangentDir42_g24 = mul( ase_worldToTangent, localPerturbNormal107_g24);
			float3 normalizeResult56 = normalize( worldToTangentDir42_g24 );
			float3 Normal150 = normalizeResult56;
			o.Normal = Normal150;
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			float screenDepth180 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
			float distanceDepth180 = abs( ( screenDepth180 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( ( _WaterColorDepth * -10.0 ) ) );
			float clampResult182 = clamp( ( 1.0 - distanceDepth180 ) , 0.0 , 1.0 );
			float4 lerpResult61 = lerp( _DeepColor , _ShallowColor , clampResult182);
			float4 Albedo165 = lerpResult61;
			float4 ase_vertex4Pos = mul( unity_WorldToObject, float4( i.worldPos , 1 ) );
			float3 ase_viewPos = UnityObjectToViewPos( ase_vertex4Pos );
			float ase_screenDepth = -ase_viewPos.z;
			float eyeDepth28_g25 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
			float2 temp_output_20_0_g25 = ( (Normal150).xy * ( _RefractionStrength / max( ase_screenDepth , 0.1 ) ) * saturate( ( eyeDepth28_g25 - ase_screenDepth ) ) );
			float eyeDepth2_g25 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ( float4( temp_output_20_0_g25, 0.0 , 0.0 ) + ase_screenPosNorm ).xy ));
			float2 temp_output_32_0_g25 = (( float4( ( temp_output_20_0_g25 * saturate( ( eyeDepth2_g25 - ase_screenDepth ) ) ), 0.0 , 0.0 ) + ase_screenPosNorm )).xy;
			float2 temp_output_1_0_g25 = ( ( floor( ( temp_output_32_0_g25 * (_CameraDepthTexture_TexelSize).zw ) ) + 0.5 ) * (_CameraDepthTexture_TexelSize).xy );
			float4 screenColor154 = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_GrabTexture,temp_output_1_0_g25);
			float4 clampResult155 = clamp( screenColor154 , float4( 0,0,0,0 ) , float4( 1,1,1,0 ) );
			float4 Refraction156 = clampResult155;
			float screenDepth158 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
			float distanceDepth158 = abs( ( screenDepth158 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( ( -10.0 * _VisibilityDepth ) ) );
			float clampResult160 = clamp( ( 1.0 - distanceDepth158 ) , 0.0 , 1.0 );
			float Depth161 = clampResult160;
			float4 lerpResult167 = lerp( Albedo165 , Refraction156 , Depth161);
			o.Albedo = lerpResult167.rgb;
			o.Smoothness = 0.98;
			float screenDepth194 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
			float distanceDepth194 = abs( ( screenDepth194 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( _EdgeFadeDistance ) );
			float clampResult196 = clamp( distanceDepth194 , 0.0 , 1.0 );
			float Edge137 = clampResult196;
			o.Alpha = Edge137;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18909
872;-1176;1546;929;3909.51;-278.8839;1.175534;False;False
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
Node;AmplifyShaderEditor.RegisterLocalVarNode;201;-5616.983,-1179.669;Inherit;False;WaveDirection2;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;98;-5120.533,4252.913;Inherit;False;Constant;_Float8;Float 8;4;0;Create;True;0;0;0;False;0;False;0.15;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;64;-5571.796,2853.746;Inherit;True;Property;_HeightMap;Height Map;9;0;Create;True;0;0;0;False;0;False;7667350b67a6e05408eab5d05068039b;1a7a9e141b92fe84e972535ec0345f29;False;gray;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.RegisterLocalVarNode;40;-5629.605,-1360.738;Inherit;False;WaveDirection1;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;91;-5157.968,3923.34;Inherit;False;Constant;_Float6;Float 6;4;0;Create;True;0;0;0;False;0;False;0.1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;87;-5021.837,4745.228;Inherit;False;Constant;_Float5;Float 5;4;0;Create;True;0;0;0;False;0;False;0.09;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;82;-5001.177,4969.02;Inherit;False;43;WaveSpeed;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;86;-5058.482,4641.113;Inherit;False;38;WaveTileUV;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;93;-5194.613,3820.226;Inherit;False;38;WaveTileUV;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;95;-4975.509,5087.5;Inherit;False;Constant;_Float7;Float 7;4;0;Create;True;0;0;0;False;0;False;0.15;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;81;-5235.637,4110.784;Inherit;False;43;WaveSpeed;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;92;-4913.176,3891.413;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;84;-5036.469,4857.735;Inherit;False;201;WaveDirection2;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;31;-4989.212,3307.967;Inherit;False;Constant;_Float3;Float 3;4;0;Create;True;0;0;0;False;0;False;0.9;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;33;-5025.857,3204.853;Inherit;False;38;WaveTileUV;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;97;-4899.232,4120.913;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;85;-4777.046,4712.301;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;80;-4963.494,4016.977;Inherit;False;40;WaveDirection1;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;88;-5295.309,2881.647;Inherit;False;HeightTexture;-1;True;1;0;SAMPLER2D;0;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;96;-4754.208,4955.5;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;73;-4542.762,4795.128;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;44;-4911.811,2675.823;Inherit;False;43;WaveSpeed;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;39;-4912.662,2483.468;Inherit;False;38;WaveTileUV;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.PannerNode;78;-4651.242,3999.572;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;41;-4930.867,2580.715;Inherit;False;40;WaveDirection1;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;42;-5004.843,3421.474;Inherit;False;201;WaveDirection2;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;90;-4582.514,4400.983;Inherit;False;88;HeightTexture;1;0;OBJECT;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.GetLocalVarNode;45;-4750.831,3515.44;Inherit;False;43;WaveSpeed;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;30;-4744.419,3276.04;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;89;-4407.807,2998.259;Inherit;False;88;HeightTexture;1;0;OBJECT;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.FunctionNode;75;-4159.429,4315.996;Inherit;False;Procedural Sample;-1;;20;f5379ff72769e2b4495e5ce2f004d8d4;2,157,0,315,0;7;82;SAMPLER2D;0;False;158;SAMPLER2DARRAY;0;False;183;FLOAT;0;False;5;FLOAT2;0,0;False;80;FLOAT3;0,0,0;False;104;FLOAT2;1,0;False;74;SAMPLERSTATE;0;False;5;COLOR;0;FLOAT;32;FLOAT;33;FLOAT;34;FLOAT;35
Node;AmplifyShaderEditor.FunctionNode;74;-4168.792,4546.537;Inherit;False;Procedural Sample;-1;;21;f5379ff72769e2b4495e5ce2f004d8d4;2,157,0,315,0;7;82;SAMPLER2D;0;False;158;SAMPLER2DARRAY;0;False;183;FLOAT;0;False;5;FLOAT2;0,0;False;80;FLOAT3;0,0,0;False;104;FLOAT2;1,0;False;74;SAMPLERSTATE;0;False;5;COLOR;0;FLOAT;32;FLOAT;33;FLOAT;34;FLOAT;35
Node;AmplifyShaderEditor.PannerNode;28;-4510.135,3358.867;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;4;-4618.615,2563.311;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.FunctionNode;70;-4136.166,3110.276;Inherit;False;Procedural Sample;-1;;22;f5379ff72769e2b4495e5ce2f004d8d4;2,157,0,315,0;7;82;SAMPLER2D;0;False;158;SAMPLER2DARRAY;0;False;183;FLOAT;0;False;5;FLOAT2;0,0;False;80;FLOAT3;0,0,0;False;104;FLOAT2;1,0;False;74;SAMPLERSTATE;0;False;5;COLOR;0;FLOAT;32;FLOAT;33;FLOAT;34;FLOAT;35
Node;AmplifyShaderEditor.SimpleAddOpNode;76;-3929.56,4466.091;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;69;-4126.802,2879.734;Inherit;False;Procedural Sample;-1;;23;f5379ff72769e2b4495e5ce2f004d8d4;2,157,0,315,0;7;82;SAMPLER2D;0;False;158;SAMPLER2DARRAY;0;False;183;FLOAT;0;False;5;FLOAT2;0,0;False;80;FLOAT3;0,0,0;False;104;FLOAT2;1,0;False;74;SAMPLERSTATE;0;False;5;COLOR;0;FLOAT;32;FLOAT;33;FLOAT;34;FLOAT;35
Node;AmplifyShaderEditor.SimpleAddOpNode;32;-3895.887,3028.619;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;101;-3762.811,4467.06;Inherit;False;LargeWaves;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;117;-3306.562,3894.335;Inherit;False;Constant;_Float9;Float 9;11;0;Create;True;0;0;0;False;0;False;5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;103;-3288.036,3778.709;Inherit;False;101;LargeWaves;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;100;-3760.904,3021.727;Inherit;False;FineWaves;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;114;-3117.481,3894.19;Inherit;False;Property;_LargeWaveStrength;Large Wave Strength;13;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;115;-3108.481,3656.19;Inherit;False;Property;_SmallWaveStrength;Small Wave Strength;12;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;116;-3087.562,3771.335;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;102;-3272.903,3531.807;Inherit;False;100;FineWaves;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;111;-3056.481,4064.19;Inherit;False;Property;_WaveSizeBalance;Wave Size Balance;11;0;Create;True;0;0;0;False;0;False;0.5;0.75;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;113;-2896.481,3533.19;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;112;-2888.481,3792.19;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;110;-2704.481,3696.19;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;163;-2758.043,-320.7726;Inherit;False;1332.39;434.1833;Comment;5;54;53;56;150;128;Normal;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;106;-2440.265,3701.076;Inherit;False;FinalWave;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;54;-2712.844,-259.1401;Inherit;False;106;FinalWave;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;128;-2608.246,-108.1385;Inherit;False;Property;_NormalStrength;Normal Strength;7;0;Create;True;0;0;0;False;0;False;1.5;1.2;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;53;-2286.09,-270.7726;Inherit;False;Normal From Height;-1;;24;1942fe2c5f1a1f94881a33d532e4afeb;0;2;20;FLOAT;0;False;110;FLOAT;1;False;2;FLOAT3;40;FLOAT3;0
Node;AmplifyShaderEditor.CommentaryNode;170;-1966.599,-1544.496;Inherit;False;1604.884;852.724;Comment;10;165;61;60;59;180;181;179;182;183;184;Albedo;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;164;-3761.485,647.5096;Inherit;False;1805.239;1040.284;Comment;18;147;144;156;155;154;152;148;153;146;145;159;158;160;161;174;185;186;202;Refraction;1,1,1,1;0;0
Node;AmplifyShaderEditor.NormalizeNode;56;-1932.208,-270.4749;Inherit;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;184;-1881.55,-1003.997;Inherit;False;Constant;_Float13;Float 13;15;0;Create;True;0;0;0;False;0;False;-10;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;179;-1941.95,-1162.771;Inherit;False;Property;_WaterColorDepth;Water Color Depth;4;0;Create;True;0;0;0;False;0;False;0.5;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;159;-3233.623,1534.704;Inherit;False;Property;_VisibilityDepth;Visibility Depth;6;0;Create;True;0;0;0;False;0;False;0.5;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;150;-1649.653,-262.3986;Inherit;False;Normal;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;185;-3167.585,1397.578;Inherit;False;Constant;_Float15;Float 15;15;0;Create;True;0;0;0;False;0;False;-10;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;183;-1687.55,-1098.997;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DepthFade;180;-1655.563,-821.0214;Inherit;False;True;False;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;186;-2960.617,1423.578;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;171;-2997.834,-1960.812;Inherit;False;904.627;399.396;Comment;6;26;108;27;47;191;192;Wave Up;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;146;-3711.485,1011.444;Inherit;False;Property;_RefractionStrength;Refraction Strength;8;0;Create;True;0;0;0;False;0;False;0.5;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;152;-3672.875,886.0894;Inherit;False;150;Normal;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;192;-2917.256,-1651.829;Inherit;False;Constant;_Float0;Float 0;14;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;187;-879.53,-2318.772;Inherit;False;1133.572;301.2458;Comment;4;193;194;196;137;Edge Fade;1,1,1,1;0;0
Node;AmplifyShaderEditor.Vector3Node;26;-2931.277,-1910.812;Inherit;False;Constant;_WaveUp;Wave Up;4;0;Create;True;0;0;0;False;0;False;0,1,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DepthFade;158;-2821.235,1519.454;Inherit;False;True;False;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;202;-3316.6,1082.306;Inherit;False;DepthMaskedRefraction;-1;;25;c805f061214177c42bca056464193f81;2,40,0,103,0;2;35;FLOAT3;0,0,0;False;37;FLOAT;0.02;False;1;FLOAT2;38
Node;AmplifyShaderEditor.OneMinusNode;181;-1374.057,-811.9603;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScreenColorNode;154;-2953.264,795.2388;Inherit;False;Global;_GrabScreen0;Grab Screen 0;13;0;Create;True;0;0;0;False;0;False;Object;-1;False;False;False;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;59;-1160.321,-1494.496;Inherit;False;Property;_DeepColor;Deep Color;1;0;Create;True;0;0;0;False;0;False;0.1117391,0.2199386,0.254717,1;0.1149429,0.3867923,0.3456059,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;193;-836.1754,-2180.16;Inherit;False;Property;_EdgeFadeDistance;Edge Fade Distance;5;0;Create;True;0;0;0;False;0;False;0.2;0.2;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;174;-2539.729,1528.515;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;108;-2704.994,-1677.416;Inherit;False;106;FinalWave;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;182;-1199.552,-811.6813;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;60;-1160.007,-1243.792;Inherit;False;Property;_ShallowColor;Shallow Color;0;0;Create;True;0;0;0;False;0;False;0.2736739,0.4617023,0.4716981,1;0.2348699,0.5236304,0.5471698,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;191;-2709.256,-1843.829;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;61;-784.5994,-1345.772;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;160;-2365.225,1528.794;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;27;-2503.092,-1819.087;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ClampOpNode;155;-2759.054,800.3767;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,1,1,0;False;1;COLOR;0
Node;AmplifyShaderEditor.DepthFade;194;-521.1755,-2197.16;Inherit;False;True;False;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;156;-2580.257,816.8177;Inherit;False;Refraction;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;189;-540.0509,699.5513;Inherit;False;Constant;_Float16;Float 16;14;0;Create;True;0;0;0;False;0;False;50;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;165;-585.716,-1332.747;Inherit;False;Albedo;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;161;-2180.246,1564.134;Inherit;False;Depth;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;196;-199.1754,-2212.16;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;23;-594.3572,600.5615;Inherit;False;Property;_TesselationAmount;Tesselation Amount;16;0;Create;True;0;0;0;False;0;False;0.5;1;0.01;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;47;-2317.207,-1823.385;Inherit;False;Wave;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;199;-21.81177,438.3529;Inherit;False;Constant;_Float12;Float 12;15;0;Create;True;0;0;0;False;0;False;0.1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;168;-59.69727,-333.2135;Inherit;False;156;Refraction;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;188;-275.0509,595.5513;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;137;49.04218,-2183.272;Inherit;False;Edge;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;169;-175.4747,-225.142;Inherit;False;161;Depth;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;166;-39.83185,-462.6146;Inherit;False;165;Albedo;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;198;-42.64716,352.6791;Inherit;False;Property;_WaveHeight;Wave Height;10;0;Create;True;0;0;0;False;0;False;0;2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;25;-376.3572,966.5615;Inherit;False;Constant;_Float2;Float 2;4;0;Create;True;0;0;0;False;0;False;50;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;24;-432.3572,838.5615;Inherit;False;Constant;_Float1;Float 1;4;0;Create;True;0;0;0;False;0;False;10;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;49;-52.07251,264.6881;Inherit;False;47;Wave;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;197;163.3528,260.6791;Inherit;False;3;3;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;147;-3364.585,930.6438;Inherit;False;3;3;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;167;201.1682,-464.6146;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;151;95.11841,-10.74243;Inherit;False;150;Normal;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DistanceBasedTessNode;21;-126.3574,860.5615;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleAddOpNode;153;-3142.929,811.5156;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;190;92.0825,182.5227;Inherit;False;137;Edge;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;145;-3355.564,702.2587;Inherit;False;True;True;False;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GrabScreenPosition;144;-3682.625,697.5096;Inherit;False;0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;148;-3571.685,1112.544;Inherit;False;Constant;_Float14;Float 14;14;0;Create;True;0;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;50;106.751,66.336;Inherit;False;Constant;_Float4;Float 4;4;0;Create;True;0;0;0;False;0;False;0.98;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;417.4008,-28.8386;Float;False;True;-1;6;ASEMaterialInspector;0;0;Standard;M3DS/Water;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;False;0;False;Transparent;;Transparent;ForwardOnly;16;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;True;2;15;10;25;False;0.5;False;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
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
WireConnection;92;0;93;0
WireConnection;92;1;91;0
WireConnection;97;0;81;0
WireConnection;97;1;98;0
WireConnection;85;0;86;0
WireConnection;85;1;87;0
WireConnection;88;0;64;0
WireConnection;96;0;82;0
WireConnection;96;1;95;0
WireConnection;73;0;85;0
WireConnection;73;2;84;0
WireConnection;73;1;96;0
WireConnection;78;0;92;0
WireConnection;78;2;80;0
WireConnection;78;1;97;0
WireConnection;30;0;33;0
WireConnection;30;1;31;0
WireConnection;75;82;90;0
WireConnection;75;5;78;0
WireConnection;74;82;90;0
WireConnection;74;5;73;0
WireConnection;28;0;30;0
WireConnection;28;2;42;0
WireConnection;28;1;45;0
WireConnection;4;0;39;0
WireConnection;4;2;41;0
WireConnection;4;1;44;0
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
WireConnection;113;0;102;0
WireConnection;113;1;115;0
WireConnection;112;0;116;0
WireConnection;112;1;114;0
WireConnection;110;0;113;0
WireConnection;110;1;112;0
WireConnection;110;2;111;0
WireConnection;106;0;110;0
WireConnection;53;20;54;0
WireConnection;53;110;128;0
WireConnection;56;0;53;40
WireConnection;150;0;56;0
WireConnection;183;0;179;0
WireConnection;183;1;184;0
WireConnection;180;0;183;0
WireConnection;186;0;185;0
WireConnection;186;1;159;0
WireConnection;158;0;186;0
WireConnection;202;35;152;0
WireConnection;202;37;146;0
WireConnection;181;0;180;0
WireConnection;154;0;202;38
WireConnection;174;0;158;0
WireConnection;182;0;181;0
WireConnection;191;0;26;0
WireConnection;191;1;192;0
WireConnection;61;0;59;0
WireConnection;61;1;60;0
WireConnection;61;2;182;0
WireConnection;160;0;174;0
WireConnection;27;0;191;0
WireConnection;27;1;108;0
WireConnection;155;0;154;0
WireConnection;194;0;193;0
WireConnection;156;0;155;0
WireConnection;165;0;61;0
WireConnection;161;0;160;0
WireConnection;196;0;194;0
WireConnection;47;0;27;0
WireConnection;188;0;23;0
WireConnection;188;1;189;0
WireConnection;137;0;196;0
WireConnection;197;0;49;0
WireConnection;197;1;198;0
WireConnection;197;2;199;0
WireConnection;147;0;152;0
WireConnection;147;1;146;0
WireConnection;147;2;148;0
WireConnection;167;0;166;0
WireConnection;167;1;168;0
WireConnection;167;2;169;0
WireConnection;21;0;188;0
WireConnection;21;1;24;0
WireConnection;21;2;25;0
WireConnection;153;0;145;0
WireConnection;153;1;147;0
WireConnection;145;0;144;0
WireConnection;0;0;167;0
WireConnection;0;1;151;0
WireConnection;0;4;50;0
WireConnection;0;9;190;0
WireConnection;0;11;197;0
WireConnection;0;14;21;0
ASEEND*/
//CHKSM=1F859A896356A67FF092A78DD04E540A7D138128