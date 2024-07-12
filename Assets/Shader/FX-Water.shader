Shader "FX/Water" {
Properties {
 _WaveScale ("Wave scale", Range(0.02,0.15)) = 0.063
 _ReflDistort ("Reflection distort", Range(0,1.5)) = 0.44
 _RefrDistort ("Refraction distort", Range(0,1.5)) = 0.4
 _RefrColor ("Refraction color", Color) = (0.34,0.85,0.92,1)
 _Fresnel ("Fresnel (A) ", 2D) = "gray" {}
 _BumpMap ("Normalmap ", 2D) = "bump" {}
 WaveSpeed ("Wave speed (map1 x,y; map2 x,y)", Vector) = (19,9,-16,-7)
 _ReflectiveColor ("Reflective color (RGB) fresnel (A) ", 2D) = "" {}
 _ReflectiveColorCube ("Reflective color cube (RGB) fresnel (A)", CUBE) = "" { TexGen CubeReflect }
 _HorizonColor ("Simple water horizon color", Color) = (0.172,0.463,0.435,1)
 _MainTex ("Fallback texture", 2D) = "" {}
 _ReflectionTex ("Internal Reflection", 2D) = "" {}
 _RefractionTex ("Internal Refraction", 2D) = "" {}
}
SubShader { 
 Tags { "RenderType"="Opaque" "WaterMode"="Refractive" }
 Pass {
  Tags { "RenderType"="Opaque" "WaterMode"="Refractive" }
Program "vp" {
SubProgram "gles " {
Keywords { "WATER_REFRACTIVE" }
"!!GLES


#ifdef VERTEX

varying highp vec3 xlv_TEXCOORD3;
varying highp vec2 xlv_TEXCOORD2;
varying highp vec2 xlv_TEXCOORD1;
varying highp vec4 xlv_TEXCOORD0;
uniform highp vec4 _WaveOffset;
uniform highp vec4 _WaveScale4;
uniform highp vec4 unity_Scale;
uniform highp mat4 _World2Object;
uniform highp mat4 glstate_matrix_mvp;
uniform highp vec4 _ProjectionParams;
uniform highp vec3 _WorldSpaceCameraPos;
attribute vec4 _glesVertex;
void main ()
{
  highp vec4 temp_1;
  highp vec4 tmpvar_2;
  tmpvar_2 = (glstate_matrix_mvp * _glesVertex);
  temp_1 = (((_glesVertex.xzxz * _WaveScale4) / unity_Scale.w) + _WaveOffset);
  highp vec4 tmpvar_3;
  tmpvar_3.w = 1.0;
  tmpvar_3.xyz = _WorldSpaceCameraPos;
  highp vec4 o_4;
  highp vec4 tmpvar_5;
  tmpvar_5 = (tmpvar_2 * 0.5);
  highp vec2 tmpvar_6;
  tmpvar_6.x = tmpvar_5.x;
  tmpvar_6.y = (tmpvar_5.y * _ProjectionParams.x);
  o_4.xy = (tmpvar_6 + tmpvar_5.w);
  o_4.zw = tmpvar_2.zw;
  gl_Position = tmpvar_2;
  xlv_TEXCOORD0 = o_4;
  xlv_TEXCOORD1 = temp_1.xy;
  xlv_TEXCOORD2 = temp_1.wz;
  xlv_TEXCOORD3 = (((_World2Object * tmpvar_3).xyz * unity_Scale.w) - _glesVertex.xyz).xzy;
}



#endif
#ifdef FRAGMENT

varying highp vec3 xlv_TEXCOORD3;
varying highp vec2 xlv_TEXCOORD2;
varying highp vec2 xlv_TEXCOORD1;
varying highp vec4 xlv_TEXCOORD0;
uniform sampler2D _BumpMap;
uniform highp vec4 _RefrColor;
uniform sampler2D _RefractionTex;
uniform sampler2D _Fresnel;
uniform sampler2D _ReflectionTex;
uniform highp float _RefrDistort;
uniform highp float _ReflDistort;
void main ()
{
  mediump float fresnel_1;
  mediump vec4 refr_2;
  highp vec4 uv2_3;
  mediump vec4 refl_4;
  highp vec4 uv1_5;
  mediump float fresnelFac_6;
  mediump vec3 bump2_7;
  mediump vec3 bump1_8;
  lowp vec3 tmpvar_9;
  tmpvar_9 = ((texture2D (_BumpMap, xlv_TEXCOORD1).xyz * 2.0) - 1.0);
  bump1_8 = tmpvar_9;
  lowp vec3 tmpvar_10;
  tmpvar_10 = ((texture2D (_BumpMap, xlv_TEXCOORD2).xyz * 2.0) - 1.0);
  bump2_7 = tmpvar_10;
  mediump vec3 tmpvar_11;
  tmpvar_11 = ((bump1_8 + bump2_7) * 0.5);
  highp float tmpvar_12;
  tmpvar_12 = dot (normalize(xlv_TEXCOORD3), tmpvar_11);
  fresnelFac_6 = tmpvar_12;
  uv1_5.zw = xlv_TEXCOORD0.zw;
  uv1_5.xy = (xlv_TEXCOORD0.xy + (tmpvar_11 * _ReflDistort).xy);
  lowp vec4 tmpvar_13;
  tmpvar_13 = texture2DProj (_ReflectionTex, uv1_5);
  refl_4 = tmpvar_13;
  uv2_3.zw = xlv_TEXCOORD0.zw;
  uv2_3.xy = (xlv_TEXCOORD0.xy - (tmpvar_11 * _RefrDistort).xy);
  lowp vec4 tmpvar_14;
  tmpvar_14 = texture2DProj (_RefractionTex, uv2_3);
  highp vec4 tmpvar_15;
  tmpvar_15 = (tmpvar_14 * _RefrColor);
  refr_2 = tmpvar_15;
  lowp float tmpvar_16;
  tmpvar_16 = texture2D (_Fresnel, vec2(fresnelFac_6)).w;
  fresnel_1 = tmpvar_16;
  gl_FragData[0] = mix (refr_2, refl_4, vec4(fresnel_1));
}



#endif"
}
SubProgram "gles3 " {
Keywords { "WATER_REFRACTIVE" }
"!!GLES3#version 300 es


#ifdef VERTEX

#define gl_Vertex _glesVertex
in vec4 _glesVertex;
#define gl_Normal (normalize(_glesNormal))
in vec3 _glesNormal;

#line 150
struct v2f_vertex_lit {
    highp vec2 uv;
    lowp vec4 diff;
    lowp vec4 spec;
};
#line 186
struct v2f_img {
    highp vec4 pos;
    mediump vec2 uv;
};
#line 180
struct appdata_img {
    highp vec4 vertex;
    mediump vec2 texcoord;
};
#line 316
struct v2f {
    highp vec4 pos;
    highp vec4 ref;
    highp vec2 bumpuv0;
    highp vec2 bumpuv1;
    highp vec3 viewDir;
};
#line 310
struct appdata {
    highp vec4 vertex;
    highp vec3 normal;
};
uniform highp vec4 _Time;
uniform highp vec4 _SinTime;
#line 3
uniform highp vec4 _CosTime;
uniform highp vec4 unity_DeltaTime;
uniform highp vec3 _WorldSpaceCameraPos;
uniform highp vec4 _ProjectionParams;
#line 7
uniform highp vec4 _ScreenParams;
uniform highp vec4 _ZBufferParams;
uniform highp vec4 unity_CameraWorldClipPlanes[6];
uniform highp vec4 _WorldSpaceLightPos0;
#line 11
uniform highp vec4 _LightPositionRange;
uniform highp vec4 unity_4LightPosX0;
uniform highp vec4 unity_4LightPosY0;
uniform highp vec4 unity_4LightPosZ0;
#line 15
uniform highp vec4 unity_4LightAtten0;
uniform highp vec4 unity_LightColor[4];
uniform highp vec4 unity_LightPosition[4];
uniform highp vec4 unity_LightAtten[4];
#line 19
uniform highp vec4 unity_SHAr;
uniform highp vec4 unity_SHAg;
uniform highp vec4 unity_SHAb;
uniform highp vec4 unity_SHBr;
#line 23
uniform highp vec4 unity_SHBg;
uniform highp vec4 unity_SHBb;
uniform highp vec4 unity_SHC;
uniform highp vec3 unity_LightColor0;
uniform highp vec3 unity_LightColor1;
uniform highp vec3 unity_LightColor2;
uniform highp vec3 unity_LightColor3;
#line 27
uniform highp vec4 unity_ShadowSplitSpheres[4];
uniform highp vec4 unity_ShadowSplitSqRadii;
uniform highp vec4 unity_LightShadowBias;
uniform highp vec4 _LightSplitsNear;
#line 31
uniform highp vec4 _LightSplitsFar;
uniform highp mat4 unity_World2Shadow[4];
uniform highp vec4 _LightShadowData;
uniform highp vec4 unity_ShadowFadeCenterAndType;
#line 35
uniform highp mat4 glstate_matrix_mvp;
uniform highp mat4 glstate_matrix_modelview0;
uniform highp mat4 glstate_matrix_invtrans_modelview0;
uniform highp mat4 _Object2World;
#line 39
uniform highp mat4 _World2Object;
uniform highp vec4 unity_Scale;
uniform highp mat4 glstate_matrix_transpose_modelview0;
uniform highp mat4 glstate_matrix_texture0;
#line 43
uniform highp mat4 glstate_matrix_texture1;
uniform highp mat4 glstate_matrix_texture2;
uniform highp mat4 glstate_matrix_texture3;
uniform highp mat4 glstate_matrix_projection;
#line 47
uniform highp vec4 glstate_lightmodel_ambient;
uniform highp mat4 unity_MatrixV;
uniform highp mat4 unity_MatrixVP;
uniform lowp vec4 unity_ColorSpaceGrey;
#line 76
#line 81
#line 86
#line 90
#line 95
#line 119
#line 136
#line 157
#line 165
#line 192
#line 205
#line 214
#line 219
#line 228
#line 233
#line 242
#line 259
#line 264
#line 290
#line 298
#line 302
#line 306
uniform highp vec4 _WaveScale4;
uniform highp vec4 _WaveOffset;
uniform highp float _ReflDistort;
uniform highp float _RefrDistort;
#line 325
#line 337
uniform sampler2D _ReflectionTex;
uniform sampler2D _Fresnel;
uniform sampler2D _RefractionTex;
uniform highp vec4 _RefrColor;
#line 341
uniform sampler2D _BumpMap;
#line 283
highp vec4 ComputeScreenPos( in highp vec4 pos ) {
    #line 285
    highp vec4 o = (pos * 0.5);
    o.xy = (vec2( o.x, (o.y * _ProjectionParams.x)) + o.w);
    o.zw = pos.zw;
    return o;
}
#line 90
highp vec3 ObjSpaceViewDir( in highp vec4 v ) {
    highp vec3 objSpaceCameraPos = ((_World2Object * vec4( _WorldSpaceCameraPos.xyz, 1.0)).xyz * unity_Scale.w);
    return (objSpaceCameraPos - v.xyz);
}
#line 325
v2f vert( in appdata v ) {
    v2f o;
    o.pos = (glstate_matrix_mvp * v.vertex);
    #line 329
    highp vec4 temp;
    temp.xyzw = (((v.vertex.xzxz * _WaveScale4) / unity_Scale.w) + _WaveOffset);
    o.bumpuv0 = temp.xy;
    o.bumpuv1 = temp.wz;
    #line 333
    o.viewDir.xzy = ObjSpaceViewDir( v.vertex);
    o.ref = ComputeScreenPos( o.pos);
    return o;
}
out highp vec4 xlv_TEXCOORD0;
out highp vec2 xlv_TEXCOORD1;
out highp vec2 xlv_TEXCOORD2;
out highp vec3 xlv_TEXCOORD3;
void main() {
    v2f xl_retval;
    appdata xlt_v;
    xlt_v.vertex = vec4(gl_Vertex);
    xlt_v.normal = vec3(gl_Normal);
    xl_retval = vert( xlt_v);
    gl_Position = vec4(xl_retval.pos);
    xlv_TEXCOORD0 = vec4(xl_retval.ref);
    xlv_TEXCOORD1 = vec2(xl_retval.bumpuv0);
    xlv_TEXCOORD2 = vec2(xl_retval.bumpuv1);
    xlv_TEXCOORD3 = vec3(xl_retval.viewDir);
}


#endif
#ifdef FRAGMENT

#define gl_FragData _glesFragData
layout(location = 0) out mediump vec4 _glesFragData[4];

#line 150
struct v2f_vertex_lit {
    highp vec2 uv;
    lowp vec4 diff;
    lowp vec4 spec;
};
#line 186
struct v2f_img {
    highp vec4 pos;
    mediump vec2 uv;
};
#line 180
struct appdata_img {
    highp vec4 vertex;
    mediump vec2 texcoord;
};
#line 316
struct v2f {
    highp vec4 pos;
    highp vec4 ref;
    highp vec2 bumpuv0;
    highp vec2 bumpuv1;
    highp vec3 viewDir;
};
#line 310
struct appdata {
    highp vec4 vertex;
    highp vec3 normal;
};
uniform highp vec4 _Time;
uniform highp vec4 _SinTime;
#line 3
uniform highp vec4 _CosTime;
uniform highp vec4 unity_DeltaTime;
uniform highp vec3 _WorldSpaceCameraPos;
uniform highp vec4 _ProjectionParams;
#line 7
uniform highp vec4 _ScreenParams;
uniform highp vec4 _ZBufferParams;
uniform highp vec4 unity_CameraWorldClipPlanes[6];
uniform highp vec4 _WorldSpaceLightPos0;
#line 11
uniform highp vec4 _LightPositionRange;
uniform highp vec4 unity_4LightPosX0;
uniform highp vec4 unity_4LightPosY0;
uniform highp vec4 unity_4LightPosZ0;
#line 15
uniform highp vec4 unity_4LightAtten0;
uniform highp vec4 unity_LightColor[4];
uniform highp vec4 unity_LightPosition[4];
uniform highp vec4 unity_LightAtten[4];
#line 19
uniform highp vec4 unity_SHAr;
uniform highp vec4 unity_SHAg;
uniform highp vec4 unity_SHAb;
uniform highp vec4 unity_SHBr;
#line 23
uniform highp vec4 unity_SHBg;
uniform highp vec4 unity_SHBb;
uniform highp vec4 unity_SHC;
uniform highp vec3 unity_LightColor0;
uniform highp vec3 unity_LightColor1;
uniform highp vec3 unity_LightColor2;
uniform highp vec3 unity_LightColor3;
#line 27
uniform highp vec4 unity_ShadowSplitSpheres[4];
uniform highp vec4 unity_ShadowSplitSqRadii;
uniform highp vec4 unity_LightShadowBias;
uniform highp vec4 _LightSplitsNear;
#line 31
uniform highp vec4 _LightSplitsFar;
uniform highp mat4 unity_World2Shadow[4];
uniform highp vec4 _LightShadowData;
uniform highp vec4 unity_ShadowFadeCenterAndType;
#line 35
uniform highp mat4 glstate_matrix_mvp;
uniform highp mat4 glstate_matrix_modelview0;
uniform highp mat4 glstate_matrix_invtrans_modelview0;
uniform highp mat4 _Object2World;
#line 39
uniform highp mat4 _World2Object;
uniform highp vec4 unity_Scale;
uniform highp mat4 glstate_matrix_transpose_modelview0;
uniform highp mat4 glstate_matrix_texture0;
#line 43
uniform highp mat4 glstate_matrix_texture1;
uniform highp mat4 glstate_matrix_texture2;
uniform highp mat4 glstate_matrix_texture3;
uniform highp mat4 glstate_matrix_projection;
#line 47
uniform highp vec4 glstate_lightmodel_ambient;
uniform highp mat4 unity_MatrixV;
uniform highp mat4 unity_MatrixVP;
uniform lowp vec4 unity_ColorSpaceGrey;
#line 76
#line 81
#line 86
#line 90
#line 95
#line 119
#line 136
#line 157
#line 165
#line 192
#line 205
#line 214
#line 219
#line 228
#line 233
#line 242
#line 259
#line 264
#line 290
#line 298
#line 302
#line 306
uniform highp vec4 _WaveScale4;
uniform highp vec4 _WaveOffset;
uniform highp float _ReflDistort;
uniform highp float _RefrDistort;
#line 325
#line 337
uniform sampler2D _ReflectionTex;
uniform sampler2D _Fresnel;
uniform sampler2D _RefractionTex;
uniform highp vec4 _RefrColor;
#line 341
uniform sampler2D _BumpMap;
#line 271
lowp vec3 UnpackNormal( in lowp vec4 packednormal ) {
    #line 273
    return ((packednormal.xyz * 2.0) - 1.0);
}
#line 342
mediump vec4 frag( in v2f i ) {
    i.viewDir = normalize(i.viewDir);
    #line 345
    mediump vec3 bump1 = UnpackNormal( texture( _BumpMap, i.bumpuv0)).xyz;
    mediump vec3 bump2 = UnpackNormal( texture( _BumpMap, i.bumpuv1)).xyz;
    mediump vec3 bump = ((bump1 + bump2) * 0.5);
    mediump float fresnelFac = dot( i.viewDir, bump);
    #line 349
    highp vec4 uv1 = i.ref;
    uv1.xy += vec2( (bump * _ReflDistort));
    mediump vec4 refl = textureProj( _ReflectionTex, uv1);
    highp vec4 uv2 = i.ref;
    #line 353
    uv2.xy -= vec2( (bump * _RefrDistort));
    mediump vec4 refr = (textureProj( _RefractionTex, uv2) * _RefrColor);
    mediump vec4 color;
    mediump float fresnel = texture( _Fresnel, vec2( fresnelFac, fresnelFac)).w;
    #line 357
    color = mix( refr, refl, vec4( fresnel));
    return color;
}
in highp vec4 xlv_TEXCOORD0;
in highp vec2 xlv_TEXCOORD1;
in highp vec2 xlv_TEXCOORD2;
in highp vec3 xlv_TEXCOORD3;
void main() {
    mediump vec4 xl_retval;
    v2f xlt_i;
    xlt_i.pos = vec4(0.0);
    xlt_i.ref = vec4(xlv_TEXCOORD0);
    xlt_i.bumpuv0 = vec2(xlv_TEXCOORD1);
    xlt_i.bumpuv1 = vec2(xlv_TEXCOORD2);
    xlt_i.viewDir = vec3(xlv_TEXCOORD3);
    xl_retval = frag( xlt_i);
    gl_FragData[0] = vec4(xl_retval);
}


#endif"
}
SubProgram "gles " {
Keywords { "WATER_REFLECTIVE" }
"!!GLES


#ifdef VERTEX

varying highp vec3 xlv_TEXCOORD3;
varying highp vec2 xlv_TEXCOORD2;
varying highp vec2 xlv_TEXCOORD1;
varying highp vec4 xlv_TEXCOORD0;
uniform highp vec4 _WaveOffset;
uniform highp vec4 _WaveScale4;
uniform highp vec4 unity_Scale;
uniform highp mat4 _World2Object;
uniform highp mat4 glstate_matrix_mvp;
uniform highp vec4 _ProjectionParams;
uniform highp vec3 _WorldSpaceCameraPos;
attribute vec4 _glesVertex;
void main ()
{
  highp vec4 temp_1;
  highp vec4 tmpvar_2;
  tmpvar_2 = (glstate_matrix_mvp * _glesVertex);
  temp_1 = (((_glesVertex.xzxz * _WaveScale4) / unity_Scale.w) + _WaveOffset);
  highp vec4 tmpvar_3;
  tmpvar_3.w = 1.0;
  tmpvar_3.xyz = _WorldSpaceCameraPos;
  highp vec4 o_4;
  highp vec4 tmpvar_5;
  tmpvar_5 = (tmpvar_2 * 0.5);
  highp vec2 tmpvar_6;
  tmpvar_6.x = tmpvar_5.x;
  tmpvar_6.y = (tmpvar_5.y * _ProjectionParams.x);
  o_4.xy = (tmpvar_6 + tmpvar_5.w);
  o_4.zw = tmpvar_2.zw;
  gl_Position = tmpvar_2;
  xlv_TEXCOORD0 = o_4;
  xlv_TEXCOORD1 = temp_1.xy;
  xlv_TEXCOORD2 = temp_1.wz;
  xlv_TEXCOORD3 = (((_World2Object * tmpvar_3).xyz * unity_Scale.w) - _glesVertex.xyz).xzy;
}



#endif
#ifdef FRAGMENT

varying highp vec3 xlv_TEXCOORD3;
varying highp vec2 xlv_TEXCOORD2;
varying highp vec2 xlv_TEXCOORD1;
varying highp vec4 xlv_TEXCOORD0;
uniform sampler2D _BumpMap;
uniform sampler2D _ReflectiveColor;
uniform sampler2D _ReflectionTex;
uniform highp float _ReflDistort;
void main ()
{
  mediump vec4 water_1;
  mediump vec4 color_2;
  mediump vec4 refl_3;
  highp vec4 uv1_4;
  mediump float fresnelFac_5;
  mediump vec3 bump2_6;
  mediump vec3 bump1_7;
  lowp vec3 tmpvar_8;
  tmpvar_8 = ((texture2D (_BumpMap, xlv_TEXCOORD1).xyz * 2.0) - 1.0);
  bump1_7 = tmpvar_8;
  lowp vec3 tmpvar_9;
  tmpvar_9 = ((texture2D (_BumpMap, xlv_TEXCOORD2).xyz * 2.0) - 1.0);
  bump2_6 = tmpvar_9;
  mediump vec3 tmpvar_10;
  tmpvar_10 = ((bump1_7 + bump2_6) * 0.5);
  highp float tmpvar_11;
  tmpvar_11 = dot (normalize(xlv_TEXCOORD3), tmpvar_10);
  fresnelFac_5 = tmpvar_11;
  uv1_4.zw = xlv_TEXCOORD0.zw;
  uv1_4.xy = (xlv_TEXCOORD0.xy + (tmpvar_10 * _ReflDistort).xy);
  lowp vec4 tmpvar_12;
  tmpvar_12 = texture2DProj (_ReflectionTex, uv1_4);
  refl_3 = tmpvar_12;
  lowp vec4 tmpvar_13;
  tmpvar_13 = texture2D (_ReflectiveColor, vec2(fresnelFac_5));
  water_1 = tmpvar_13;
  color_2.xyz = mix (water_1.xyz, refl_3.xyz, water_1.www);
  color_2.w = (refl_3.w * water_1.w);
  gl_FragData[0] = color_2;
}



#endif"
}
SubProgram "gles3 " {
Keywords { "WATER_REFLECTIVE" }
"!!GLES3#version 300 es


#ifdef VERTEX

#define gl_Vertex _glesVertex
in vec4 _glesVertex;
#define gl_Normal (normalize(_glesNormal))
in vec3 _glesNormal;

#line 150
struct v2f_vertex_lit {
    highp vec2 uv;
    lowp vec4 diff;
    lowp vec4 spec;
};
#line 186
struct v2f_img {
    highp vec4 pos;
    mediump vec2 uv;
};
#line 180
struct appdata_img {
    highp vec4 vertex;
    mediump vec2 texcoord;
};
#line 315
struct v2f {
    highp vec4 pos;
    highp vec4 ref;
    highp vec2 bumpuv0;
    highp vec2 bumpuv1;
    highp vec3 viewDir;
};
#line 309
struct appdata {
    highp vec4 vertex;
    highp vec3 normal;
};
uniform highp vec4 _Time;
uniform highp vec4 _SinTime;
#line 3
uniform highp vec4 _CosTime;
uniform highp vec4 unity_DeltaTime;
uniform highp vec3 _WorldSpaceCameraPos;
uniform highp vec4 _ProjectionParams;
#line 7
uniform highp vec4 _ScreenParams;
uniform highp vec4 _ZBufferParams;
uniform highp vec4 unity_CameraWorldClipPlanes[6];
uniform highp vec4 _WorldSpaceLightPos0;
#line 11
uniform highp vec4 _LightPositionRange;
uniform highp vec4 unity_4LightPosX0;
uniform highp vec4 unity_4LightPosY0;
uniform highp vec4 unity_4LightPosZ0;
#line 15
uniform highp vec4 unity_4LightAtten0;
uniform highp vec4 unity_LightColor[4];
uniform highp vec4 unity_LightPosition[4];
uniform highp vec4 unity_LightAtten[4];
#line 19
uniform highp vec4 unity_SHAr;
uniform highp vec4 unity_SHAg;
uniform highp vec4 unity_SHAb;
uniform highp vec4 unity_SHBr;
#line 23
uniform highp vec4 unity_SHBg;
uniform highp vec4 unity_SHBb;
uniform highp vec4 unity_SHC;
uniform highp vec3 unity_LightColor0;
uniform highp vec3 unity_LightColor1;
uniform highp vec3 unity_LightColor2;
uniform highp vec3 unity_LightColor3;
#line 27
uniform highp vec4 unity_ShadowSplitSpheres[4];
uniform highp vec4 unity_ShadowSplitSqRadii;
uniform highp vec4 unity_LightShadowBias;
uniform highp vec4 _LightSplitsNear;
#line 31
uniform highp vec4 _LightSplitsFar;
uniform highp mat4 unity_World2Shadow[4];
uniform highp vec4 _LightShadowData;
uniform highp vec4 unity_ShadowFadeCenterAndType;
#line 35
uniform highp mat4 glstate_matrix_mvp;
uniform highp mat4 glstate_matrix_modelview0;
uniform highp mat4 glstate_matrix_invtrans_modelview0;
uniform highp mat4 _Object2World;
#line 39
uniform highp mat4 _World2Object;
uniform highp vec4 unity_Scale;
uniform highp mat4 glstate_matrix_transpose_modelview0;
uniform highp mat4 glstate_matrix_texture0;
#line 43
uniform highp mat4 glstate_matrix_texture1;
uniform highp mat4 glstate_matrix_texture2;
uniform highp mat4 glstate_matrix_texture3;
uniform highp mat4 glstate_matrix_projection;
#line 47
uniform highp vec4 glstate_lightmodel_ambient;
uniform highp mat4 unity_MatrixV;
uniform highp mat4 unity_MatrixVP;
uniform lowp vec4 unity_ColorSpaceGrey;
#line 76
#line 81
#line 86
#line 90
#line 95
#line 119
#line 136
#line 157
#line 165
#line 192
#line 205
#line 214
#line 219
#line 228
#line 233
#line 242
#line 259
#line 264
#line 290
#line 298
#line 302
#line 306
uniform highp vec4 _WaveScale4;
uniform highp vec4 _WaveOffset;
uniform highp float _ReflDistort;
#line 324
#line 336
uniform sampler2D _ReflectionTex;
uniform sampler2D _ReflectiveColor;
uniform sampler2D _BumpMap;
#line 283
highp vec4 ComputeScreenPos( in highp vec4 pos ) {
    #line 285
    highp vec4 o = (pos * 0.5);
    o.xy = (vec2( o.x, (o.y * _ProjectionParams.x)) + o.w);
    o.zw = pos.zw;
    return o;
}
#line 90
highp vec3 ObjSpaceViewDir( in highp vec4 v ) {
    highp vec3 objSpaceCameraPos = ((_World2Object * vec4( _WorldSpaceCameraPos.xyz, 1.0)).xyz * unity_Scale.w);
    return (objSpaceCameraPos - v.xyz);
}
#line 324
v2f vert( in appdata v ) {
    v2f o;
    o.pos = (glstate_matrix_mvp * v.vertex);
    #line 328
    highp vec4 temp;
    temp.xyzw = (((v.vertex.xzxz * _WaveScale4) / unity_Scale.w) + _WaveOffset);
    o.bumpuv0 = temp.xy;
    o.bumpuv1 = temp.wz;
    #line 332
    o.viewDir.xzy = ObjSpaceViewDir( v.vertex);
    o.ref = ComputeScreenPos( o.pos);
    return o;
}
out highp vec4 xlv_TEXCOORD0;
out highp vec2 xlv_TEXCOORD1;
out highp vec2 xlv_TEXCOORD2;
out highp vec3 xlv_TEXCOORD3;
void main() {
    v2f xl_retval;
    appdata xlt_v;
    xlt_v.vertex = vec4(gl_Vertex);
    xlt_v.normal = vec3(gl_Normal);
    xl_retval = vert( xlt_v);
    gl_Position = vec4(xl_retval.pos);
    xlv_TEXCOORD0 = vec4(xl_retval.ref);
    xlv_TEXCOORD1 = vec2(xl_retval.bumpuv0);
    xlv_TEXCOORD2 = vec2(xl_retval.bumpuv1);
    xlv_TEXCOORD3 = vec3(xl_retval.viewDir);
}


#endif
#ifdef FRAGMENT

#define gl_FragData _glesFragData
layout(location = 0) out mediump vec4 _glesFragData[4];

#line 150
struct v2f_vertex_lit {
    highp vec2 uv;
    lowp vec4 diff;
    lowp vec4 spec;
};
#line 186
struct v2f_img {
    highp vec4 pos;
    mediump vec2 uv;
};
#line 180
struct appdata_img {
    highp vec4 vertex;
    mediump vec2 texcoord;
};
#line 315
struct v2f {
    highp vec4 pos;
    highp vec4 ref;
    highp vec2 bumpuv0;
    highp vec2 bumpuv1;
    highp vec3 viewDir;
};
#line 309
struct appdata {
    highp vec4 vertex;
    highp vec3 normal;
};
uniform highp vec4 _Time;
uniform highp vec4 _SinTime;
#line 3
uniform highp vec4 _CosTime;
uniform highp vec4 unity_DeltaTime;
uniform highp vec3 _WorldSpaceCameraPos;
uniform highp vec4 _ProjectionParams;
#line 7
uniform highp vec4 _ScreenParams;
uniform highp vec4 _ZBufferParams;
uniform highp vec4 unity_CameraWorldClipPlanes[6];
uniform highp vec4 _WorldSpaceLightPos0;
#line 11
uniform highp vec4 _LightPositionRange;
uniform highp vec4 unity_4LightPosX0;
uniform highp vec4 unity_4LightPosY0;
uniform highp vec4 unity_4LightPosZ0;
#line 15
uniform highp vec4 unity_4LightAtten0;
uniform highp vec4 unity_LightColor[4];
uniform highp vec4 unity_LightPosition[4];
uniform highp vec4 unity_LightAtten[4];
#line 19
uniform highp vec4 unity_SHAr;
uniform highp vec4 unity_SHAg;
uniform highp vec4 unity_SHAb;
uniform highp vec4 unity_SHBr;
#line 23
uniform highp vec4 unity_SHBg;
uniform highp vec4 unity_SHBb;
uniform highp vec4 unity_SHC;
uniform highp vec3 unity_LightColor0;
uniform highp vec3 unity_LightColor1;
uniform highp vec3 unity_LightColor2;
uniform highp vec3 unity_LightColor3;
#line 27
uniform highp vec4 unity_ShadowSplitSpheres[4];
uniform highp vec4 unity_ShadowSplitSqRadii;
uniform highp vec4 unity_LightShadowBias;
uniform highp vec4 _LightSplitsNear;
#line 31
uniform highp vec4 _LightSplitsFar;
uniform highp mat4 unity_World2Shadow[4];
uniform highp vec4 _LightShadowData;
uniform highp vec4 unity_ShadowFadeCenterAndType;
#line 35
uniform highp mat4 glstate_matrix_mvp;
uniform highp mat4 glstate_matrix_modelview0;
uniform highp mat4 glstate_matrix_invtrans_modelview0;
uniform highp mat4 _Object2World;
#line 39
uniform highp mat4 _World2Object;
uniform highp vec4 unity_Scale;
uniform highp mat4 glstate_matrix_transpose_modelview0;
uniform highp mat4 glstate_matrix_texture0;
#line 43
uniform highp mat4 glstate_matrix_texture1;
uniform highp mat4 glstate_matrix_texture2;
uniform highp mat4 glstate_matrix_texture3;
uniform highp mat4 glstate_matrix_projection;
#line 47
uniform highp vec4 glstate_lightmodel_ambient;
uniform highp mat4 unity_MatrixV;
uniform highp mat4 unity_MatrixVP;
uniform lowp vec4 unity_ColorSpaceGrey;
#line 76
#line 81
#line 86
#line 90
#line 95
#line 119
#line 136
#line 157
#line 165
#line 192
#line 205
#line 214
#line 219
#line 228
#line 233
#line 242
#line 259
#line 264
#line 290
#line 298
#line 302
#line 306
uniform highp vec4 _WaveScale4;
uniform highp vec4 _WaveOffset;
uniform highp float _ReflDistort;
#line 324
#line 336
uniform sampler2D _ReflectionTex;
uniform sampler2D _ReflectiveColor;
uniform sampler2D _BumpMap;
#line 271
lowp vec3 UnpackNormal( in lowp vec4 packednormal ) {
    #line 273
    return ((packednormal.xyz * 2.0) - 1.0);
}
#line 339
mediump vec4 frag( in v2f i ) {
    #line 341
    i.viewDir = normalize(i.viewDir);
    mediump vec3 bump1 = UnpackNormal( texture( _BumpMap, i.bumpuv0)).xyz;
    mediump vec3 bump2 = UnpackNormal( texture( _BumpMap, i.bumpuv1)).xyz;
    mediump vec3 bump = ((bump1 + bump2) * 0.5);
    #line 345
    mediump float fresnelFac = dot( i.viewDir, bump);
    highp vec4 uv1 = i.ref;
    uv1.xy += vec2( (bump * _ReflDistort));
    mediump vec4 refl = textureProj( _ReflectionTex, uv1);
    #line 349
    mediump vec4 color;
    mediump vec4 water = texture( _ReflectiveColor, vec2( fresnelFac, fresnelFac));
    color.xyz = mix( water.xyz, refl.xyz, vec3( water.w));
    color.w = (refl.w * water.w);
    #line 353
    return color;
}
in highp vec4 xlv_TEXCOORD0;
in highp vec2 xlv_TEXCOORD1;
in highp vec2 xlv_TEXCOORD2;
in highp vec3 xlv_TEXCOORD3;
void main() {
    mediump vec4 xl_retval;
    v2f xlt_i;
    xlt_i.pos = vec4(0.0);
    xlt_i.ref = vec4(xlv_TEXCOORD0);
    xlt_i.bumpuv0 = vec2(xlv_TEXCOORD1);
    xlt_i.bumpuv1 = vec2(xlv_TEXCOORD2);
    xlt_i.viewDir = vec3(xlv_TEXCOORD3);
    xl_retval = frag( xlt_i);
    gl_FragData[0] = vec4(xl_retval);
}


#endif"
}
SubProgram "gles " {
Keywords { "WATER_SIMPLE" }
"!!GLES


#ifdef VERTEX

varying highp vec3 xlv_TEXCOORD2;
varying highp vec2 xlv_TEXCOORD1;
varying highp vec2 xlv_TEXCOORD0;
uniform highp vec4 _WaveOffset;
uniform highp vec4 _WaveScale4;
uniform highp vec4 unity_Scale;
uniform highp mat4 _World2Object;
uniform highp mat4 glstate_matrix_mvp;
uniform highp vec3 _WorldSpaceCameraPos;
attribute vec4 _glesVertex;
void main ()
{
  highp vec4 temp_1;
  temp_1 = (((_glesVertex.xzxz * _WaveScale4) / unity_Scale.w) + _WaveOffset);
  highp vec4 tmpvar_2;
  tmpvar_2.w = 1.0;
  tmpvar_2.xyz = _WorldSpaceCameraPos;
  gl_Position = (glstate_matrix_mvp * _glesVertex);
  xlv_TEXCOORD0 = temp_1.xy;
  xlv_TEXCOORD1 = temp_1.wz;
  xlv_TEXCOORD2 = (((_World2Object * tmpvar_2).xyz * unity_Scale.w) - _glesVertex.xyz).xzy;
}



#endif
#ifdef FRAGMENT

varying highp vec3 xlv_TEXCOORD2;
varying highp vec2 xlv_TEXCOORD1;
varying highp vec2 xlv_TEXCOORD0;
uniform sampler2D _BumpMap;
uniform highp vec4 _HorizonColor;
uniform sampler2D _ReflectiveColor;
void main ()
{
  mediump vec4 water_1;
  mediump vec4 color_2;
  mediump float fresnelFac_3;
  mediump vec3 bump2_4;
  mediump vec3 bump1_5;
  lowp vec3 tmpvar_6;
  tmpvar_6 = ((texture2D (_BumpMap, xlv_TEXCOORD0).xyz * 2.0) - 1.0);
  bump1_5 = tmpvar_6;
  lowp vec3 tmpvar_7;
  tmpvar_7 = ((texture2D (_BumpMap, xlv_TEXCOORD1).xyz * 2.0) - 1.0);
  bump2_4 = tmpvar_7;
  mediump vec3 tmpvar_8;
  tmpvar_8 = ((bump1_5 + bump2_4) * 0.5);
  highp float tmpvar_9;
  tmpvar_9 = dot (normalize(xlv_TEXCOORD2), tmpvar_8);
  fresnelFac_3 = tmpvar_9;
  lowp vec4 tmpvar_10;
  tmpvar_10 = texture2D (_ReflectiveColor, vec2(fresnelFac_3));
  water_1 = tmpvar_10;
  highp vec3 tmpvar_11;
  tmpvar_11 = mix (water_1.xyz, _HorizonColor.xyz, water_1.www);
  color_2.xyz = tmpvar_11;
  highp float tmpvar_12;
  tmpvar_12 = _HorizonColor.w;
  color_2.w = tmpvar_12;
  gl_FragData[0] = color_2;
}



#endif"
}
SubProgram "gles3 " {
Keywords { "WATER_SIMPLE" }
"!!GLES3#version 300 es


#ifdef VERTEX

#define gl_Vertex _glesVertex
in vec4 _glesVertex;
#define gl_Normal (normalize(_glesNormal))
in vec3 _glesNormal;

#line 150
struct v2f_vertex_lit {
    highp vec2 uv;
    lowp vec4 diff;
    lowp vec4 spec;
};
#line 186
struct v2f_img {
    highp vec4 pos;
    mediump vec2 uv;
};
#line 180
struct appdata_img {
    highp vec4 vertex;
    mediump vec2 texcoord;
};
#line 314
struct v2f {
    highp vec4 pos;
    highp vec2 bumpuv0;
    highp vec2 bumpuv1;
    highp vec3 viewDir;
};
#line 308
struct appdata {
    highp vec4 vertex;
    highp vec3 normal;
};
uniform highp vec4 _Time;
uniform highp vec4 _SinTime;
#line 3
uniform highp vec4 _CosTime;
uniform highp vec4 unity_DeltaTime;
uniform highp vec3 _WorldSpaceCameraPos;
uniform highp vec4 _ProjectionParams;
#line 7
uniform highp vec4 _ScreenParams;
uniform highp vec4 _ZBufferParams;
uniform highp vec4 unity_CameraWorldClipPlanes[6];
uniform highp vec4 _WorldSpaceLightPos0;
#line 11
uniform highp vec4 _LightPositionRange;
uniform highp vec4 unity_4LightPosX0;
uniform highp vec4 unity_4LightPosY0;
uniform highp vec4 unity_4LightPosZ0;
#line 15
uniform highp vec4 unity_4LightAtten0;
uniform highp vec4 unity_LightColor[4];
uniform highp vec4 unity_LightPosition[4];
uniform highp vec4 unity_LightAtten[4];
#line 19
uniform highp vec4 unity_SHAr;
uniform highp vec4 unity_SHAg;
uniform highp vec4 unity_SHAb;
uniform highp vec4 unity_SHBr;
#line 23
uniform highp vec4 unity_SHBg;
uniform highp vec4 unity_SHBb;
uniform highp vec4 unity_SHC;
uniform highp vec3 unity_LightColor0;
uniform highp vec3 unity_LightColor1;
uniform highp vec3 unity_LightColor2;
uniform highp vec3 unity_LightColor3;
#line 27
uniform highp vec4 unity_ShadowSplitSpheres[4];
uniform highp vec4 unity_ShadowSplitSqRadii;
uniform highp vec4 unity_LightShadowBias;
uniform highp vec4 _LightSplitsNear;
#line 31
uniform highp vec4 _LightSplitsFar;
uniform highp mat4 unity_World2Shadow[4];
uniform highp vec4 _LightShadowData;
uniform highp vec4 unity_ShadowFadeCenterAndType;
#line 35
uniform highp mat4 glstate_matrix_mvp;
uniform highp mat4 glstate_matrix_modelview0;
uniform highp mat4 glstate_matrix_invtrans_modelview0;
uniform highp mat4 _Object2World;
#line 39
uniform highp mat4 _World2Object;
uniform highp vec4 unity_Scale;
uniform highp mat4 glstate_matrix_transpose_modelview0;
uniform highp mat4 glstate_matrix_texture0;
#line 43
uniform highp mat4 glstate_matrix_texture1;
uniform highp mat4 glstate_matrix_texture2;
uniform highp mat4 glstate_matrix_texture3;
uniform highp mat4 glstate_matrix_projection;
#line 47
uniform highp vec4 glstate_lightmodel_ambient;
uniform highp mat4 unity_MatrixV;
uniform highp mat4 unity_MatrixVP;
uniform lowp vec4 unity_ColorSpaceGrey;
#line 76
#line 81
#line 86
#line 90
#line 95
#line 119
#line 136
#line 157
#line 165
#line 192
#line 205
#line 214
#line 219
#line 228
#line 233
#line 242
#line 259
#line 264
#line 290
#line 298
#line 302
#line 306
uniform highp vec4 _WaveScale4;
uniform highp vec4 _WaveOffset;
#line 322
uniform sampler2D _ReflectiveColor;
#line 334
uniform highp vec4 _HorizonColor;
uniform sampler2D _BumpMap;
#line 90
highp vec3 ObjSpaceViewDir( in highp vec4 v ) {
    highp vec3 objSpaceCameraPos = ((_World2Object * vec4( _WorldSpaceCameraPos.xyz, 1.0)).xyz * unity_Scale.w);
    return (objSpaceCameraPos - v.xyz);
}
#line 322
v2f vert( in appdata v ) {
    v2f o;
    o.pos = (glstate_matrix_mvp * v.vertex);
    #line 326
    highp vec4 temp;
    temp.xyzw = (((v.vertex.xzxz * _WaveScale4) / unity_Scale.w) + _WaveOffset);
    o.bumpuv0 = temp.xy;
    o.bumpuv1 = temp.wz;
    #line 330
    o.viewDir.xzy = ObjSpaceViewDir( v.vertex);
    return o;
}
out highp vec2 xlv_TEXCOORD0;
out highp vec2 xlv_TEXCOORD1;
out highp vec3 xlv_TEXCOORD2;
void main() {
    v2f xl_retval;
    appdata xlt_v;
    xlt_v.vertex = vec4(gl_Vertex);
    xlt_v.normal = vec3(gl_Normal);
    xl_retval = vert( xlt_v);
    gl_Position = vec4(xl_retval.pos);
    xlv_TEXCOORD0 = vec2(xl_retval.bumpuv0);
    xlv_TEXCOORD1 = vec2(xl_retval.bumpuv1);
    xlv_TEXCOORD2 = vec3(xl_retval.viewDir);
}


#endif
#ifdef FRAGMENT

#define gl_FragData _glesFragData
layout(location = 0) out mediump vec4 _glesFragData[4];

#line 150
struct v2f_vertex_lit {
    highp vec2 uv;
    lowp vec4 diff;
    lowp vec4 spec;
};
#line 186
struct v2f_img {
    highp vec4 pos;
    mediump vec2 uv;
};
#line 180
struct appdata_img {
    highp vec4 vertex;
    mediump vec2 texcoord;
};
#line 314
struct v2f {
    highp vec4 pos;
    highp vec2 bumpuv0;
    highp vec2 bumpuv1;
    highp vec3 viewDir;
};
#line 308
struct appdata {
    highp vec4 vertex;
    highp vec3 normal;
};
uniform highp vec4 _Time;
uniform highp vec4 _SinTime;
#line 3
uniform highp vec4 _CosTime;
uniform highp vec4 unity_DeltaTime;
uniform highp vec3 _WorldSpaceCameraPos;
uniform highp vec4 _ProjectionParams;
#line 7
uniform highp vec4 _ScreenParams;
uniform highp vec4 _ZBufferParams;
uniform highp vec4 unity_CameraWorldClipPlanes[6];
uniform highp vec4 _WorldSpaceLightPos0;
#line 11
uniform highp vec4 _LightPositionRange;
uniform highp vec4 unity_4LightPosX0;
uniform highp vec4 unity_4LightPosY0;
uniform highp vec4 unity_4LightPosZ0;
#line 15
uniform highp vec4 unity_4LightAtten0;
uniform highp vec4 unity_LightColor[4];
uniform highp vec4 unity_LightPosition[4];
uniform highp vec4 unity_LightAtten[4];
#line 19
uniform highp vec4 unity_SHAr;
uniform highp vec4 unity_SHAg;
uniform highp vec4 unity_SHAb;
uniform highp vec4 unity_SHBr;
#line 23
uniform highp vec4 unity_SHBg;
uniform highp vec4 unity_SHBb;
uniform highp vec4 unity_SHC;
uniform highp vec3 unity_LightColor0;
uniform highp vec3 unity_LightColor1;
uniform highp vec3 unity_LightColor2;
uniform highp vec3 unity_LightColor3;
#line 27
uniform highp vec4 unity_ShadowSplitSpheres[4];
uniform highp vec4 unity_ShadowSplitSqRadii;
uniform highp vec4 unity_LightShadowBias;
uniform highp vec4 _LightSplitsNear;
#line 31
uniform highp vec4 _LightSplitsFar;
uniform highp mat4 unity_World2Shadow[4];
uniform highp vec4 _LightShadowData;
uniform highp vec4 unity_ShadowFadeCenterAndType;
#line 35
uniform highp mat4 glstate_matrix_mvp;
uniform highp mat4 glstate_matrix_modelview0;
uniform highp mat4 glstate_matrix_invtrans_modelview0;
uniform highp mat4 _Object2World;
#line 39
uniform highp mat4 _World2Object;
uniform highp vec4 unity_Scale;
uniform highp mat4 glstate_matrix_transpose_modelview0;
uniform highp mat4 glstate_matrix_texture0;
#line 43
uniform highp mat4 glstate_matrix_texture1;
uniform highp mat4 glstate_matrix_texture2;
uniform highp mat4 glstate_matrix_texture3;
uniform highp mat4 glstate_matrix_projection;
#line 47
uniform highp vec4 glstate_lightmodel_ambient;
uniform highp mat4 unity_MatrixV;
uniform highp mat4 unity_MatrixVP;
uniform lowp vec4 unity_ColorSpaceGrey;
#line 76
#line 81
#line 86
#line 90
#line 95
#line 119
#line 136
#line 157
#line 165
#line 192
#line 205
#line 214
#line 219
#line 228
#line 233
#line 242
#line 259
#line 264
#line 290
#line 298
#line 302
#line 306
uniform highp vec4 _WaveScale4;
uniform highp vec4 _WaveOffset;
#line 322
uniform sampler2D _ReflectiveColor;
#line 334
uniform highp vec4 _HorizonColor;
uniform sampler2D _BumpMap;
#line 271
lowp vec3 UnpackNormal( in lowp vec4 packednormal ) {
    #line 273
    return ((packednormal.xyz * 2.0) - 1.0);
}
#line 336
mediump vec4 frag( in v2f i ) {
    #line 338
    i.viewDir = normalize(i.viewDir);
    mediump vec3 bump1 = UnpackNormal( texture( _BumpMap, i.bumpuv0)).xyz;
    mediump vec3 bump2 = UnpackNormal( texture( _BumpMap, i.bumpuv1)).xyz;
    mediump vec3 bump = ((bump1 + bump2) * 0.5);
    #line 342
    mediump float fresnelFac = dot( i.viewDir, bump);
    mediump vec4 color;
    mediump vec4 water = texture( _ReflectiveColor, vec2( fresnelFac, fresnelFac));
    color.xyz = mix( water.xyz, _HorizonColor.xyz, vec3( water.w));
    #line 346
    color.w = _HorizonColor.w;
    return color;
}
in highp vec2 xlv_TEXCOORD0;
in highp vec2 xlv_TEXCOORD1;
in highp vec3 xlv_TEXCOORD2;
void main() {
    mediump vec4 xl_retval;
    v2f xlt_i;
    xlt_i.pos = vec4(0.0);
    xlt_i.bumpuv0 = vec2(xlv_TEXCOORD0);
    xlt_i.bumpuv1 = vec2(xlv_TEXCOORD1);
    xlt_i.viewDir = vec3(xlv_TEXCOORD2);
    xl_retval = frag( xlt_i);
    gl_FragData[0] = vec4(xl_retval);
}


#endif"
}
}
Program "fp" {
SubProgram "gles " {
Keywords { "WATER_REFRACTIVE" }
"!!GLES"
}
SubProgram "gles3 " {
Keywords { "WATER_REFRACTIVE" }
"!!GLES3"
}
SubProgram "gles " {
Keywords { "WATER_REFLECTIVE" }
"!!GLES"
}
SubProgram "gles3 " {
Keywords { "WATER_REFLECTIVE" }
"!!GLES3"
}
SubProgram "gles " {
Keywords { "WATER_SIMPLE" }
"!!GLES"
}
SubProgram "gles3 " {
Keywords { "WATER_SIMPLE" }
"!!GLES3"
}
}
 }
}
SubShader { 
 Tags { "RenderType"="Opaque" "WaterMode"="Simple" }
 Pass {
  Tags { "RenderType"="Opaque" "WaterMode"="Simple" }
  Color (0.5,0.5,0.5,0.5)
  SetTexture [_MainTex] { Matrix [_WaveMatrix] combine texture * primary }
  SetTexture [_MainTex] { Matrix [_WaveMatrix2] combine texture * primary + previous }
  SetTexture [_ReflectiveColorCube] { Matrix [_Reflection] combine texture +- previous, primary alpha }
 }
}
SubShader { 
 Tags { "RenderType"="Opaque" "WaterMode"="Simple" }
 Pass {
  Tags { "RenderType"="Opaque" "WaterMode"="Simple" }
  Color (0.5,0.5,0.5,0.5)
  SetTexture [_MainTex] { Matrix [_WaveMatrix] combine texture }
  SetTexture [_ReflectiveColorCube] { Matrix [_Reflection] combine texture +- previous, primary alpha }
 }
}
SubShader { 
 Tags { "RenderType"="Opaque" "WaterMode"="Simple" }
 Pass {
  Tags { "RenderType"="Opaque" "WaterMode"="Simple" }
  Color (0.5,0.5,0.5,0)
  SetTexture [_MainTex] { Matrix [_WaveMatrix] combine texture, primary alpha }
 }
}
}