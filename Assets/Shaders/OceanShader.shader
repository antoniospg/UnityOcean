Shader "Custom/OceanShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
	_Upwelling ("Ocean Color", Color) = (0, 0.2, 0.3, 1)
	_Sky ("Sky Color", Color) = (0.69, 0.84, 1, 1)
	_Air ("Air Color", Color) = (0.1, 0.1, 0.1, 1)
	_Kdiffuse("Diffuse", Range(0, 1)) = 0.91
	
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Lambert

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
	sampler2D _FresnelTable;
	sampler2D _FoldingTable;
	sampler2D _FoamTex;
	float4 _Sky;
	float4 _Air;
	float4 _Upwelling;
	float _Kdiffuse;
        fixed4 _Color;

        struct Input
        {
            float3 worldNormal;
	    float3 worldPos;
	    float3 worldRefl;
	    float2 uv_MainTex;
        };

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

	float Fresnel(float3 V, float3 N)
	{
	    float costhetai = abs(dot(V,N));
	    return tex2D(_FresnelTable, float2(costhetai,0.0)).a;
	}

        void surf (Input IN, inout SurfaceOutput o)
        {
	    //calculate illumination effects

	    float3 V = normalize(_WorldSpaceCameraPos-IN.worldPos);
	    float3 N = IN.worldNormal;

	    float fresnel = Fresnel(V, N);

	    float dist = length(V) * _Kdiffuse;
    	    dist = exp(-dist);

	    float4 Ci = dist*(fresnel*_Sky + (1-fresnel)*_Upwelling)
			+ (1-dist)*_Air;

	    //calculate foam 
	    fixed4 text = tex2D(_MainTex, IN.uv_MainTex*8);

	    //gray scale
    	    float gray = (text.r+text.g+text.b)/3;
	    fixed4 graytext = (gray, gray, gray, 1);
		
	    //pick jacobian coefficients
	    fixed4 foam = tex2D(_FoldingTable, IN.uv_MainTex);
	    fixed4 foamMask = text*foam;


            // Albedo comes from a texture tinted by color
            //fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            //o.Albedo.r = N.x;
	    //o.Albedo.g = N.y;
	    //o.Albedo.b = N.z;
	    
	    //o.Albedo = Ci.rgb;
	    fixed4 color = lerp(_Upwelling, _Sky, fresnel);
	    o.Albedo = (Ci + foam*(text*Ci)).rgb;
	    //o.Albedo = lerp(Ci, text+Ci, foam);

		//o.Albedo = foamMask.rgb;
            o.Alpha = 1;


        }
        ENDCG
    }
   // FallBack "Diffuse"
}
