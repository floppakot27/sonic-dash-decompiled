Shader "Sonic Dash/Waves" {
Properties {
 _Diffuse1 ("_Diffuse1", 2D) = "gray" {}
 _Color ("_Color", Color) = (1,1,1,1)
 _Texture1PanSpeedX ("_Texture1PanSpeedX", Float) = 7
 _Texture1PanSpeedY ("_Texture1PanSpeedY", Float) = 30
 _Texture2PanSpeed ("_Texture2PanSpeed", Float) = 5
}
SubShader { 
 Tags { "QUEUE"="Transparent" }
 Pass {
  Tags { "QUEUE"="Transparent" }
  ZWrite Off
  Blend One One
Program "vp" {
SubProgram "gles " {
"!!GLES


#ifdef VERTEX

varying mediump float xlv_;
varying mediump vec2 xlv_TEXCOORD1;
varying mediump vec2 xlv_TEXCOORD0;
uniform highp vec4 _Diffuse1_ST;
uniform mediump float _Texture2PanSpeed;
uniform mediump float _Texture1PanSpeedY;
uniform mediump float _Texture1PanSpeedX;
uniform highp mat4 glstate_matrix_mvp;
uniform highp vec4 _Time;
attribute vec4 _glesMultiTexCoord0;
attribute vec4 _glesColor;
attribute vec4 _glesVertex;
void main ()
{
  mediump float xAddition_1;
  mediump float multiplier_2;
  mediump vec4 texCoords2_3;
  mediump vec4 texCoords1_4;
  mediump vec2 tmpvar_5;
  mediump vec2 tmpvar_6;
  mediump float tmpvar_7;
  texCoords1_4.zw = _glesMultiTexCoord0.zw;
  texCoords2_3.yzw = _glesMultiTexCoord0.yzw;
  highp float tmpvar_8;
  tmpvar_8 = (_Time * _Texture1PanSpeedY).x;
  multiplier_2 = tmpvar_8;
  texCoords1_4.y = (_glesMultiTexCoord0.y + ((sin(multiplier_2) * 0.25) + 0.75));
  highp float tmpvar_9;
  tmpvar_9 = (_Time * _Texture1PanSpeedX).x;
  xAddition_1 = tmpvar_9;
  texCoords1_4.x = (_glesMultiTexCoord0.x + (xAddition_1 - floor(xAddition_1)));
  highp float tmpvar_10;
  tmpvar_10 = (_Time * _Texture2PanSpeed).x;
  xAddition_1 = tmpvar_10;
  mediump float tmpvar_11;
  tmpvar_11 = (xAddition_1 - floor(xAddition_1));
  xAddition_1 = tmpvar_11;
  texCoords2_3.x = (_glesMultiTexCoord0.x + tmpvar_11);
  highp float tmpvar_12;
  tmpvar_12 = _glesColor.w;
  tmpvar_7 = tmpvar_12;
  highp vec2 tmpvar_13;
  tmpvar_13 = ((texCoords1_4.xy * _Diffuse1_ST.xy) + _Diffuse1_ST.zw);
  tmpvar_5 = tmpvar_13;
  highp vec2 tmpvar_14;
  tmpvar_14 = ((texCoords2_3.xy * _Diffuse1_ST.xy) + _Diffuse1_ST.zw);
  tmpvar_6 = tmpvar_14;
  gl_Position = (glstate_matrix_mvp * _glesVertex);
  xlv_TEXCOORD0 = tmpvar_5;
  xlv_TEXCOORD1 = tmpvar_6;
  xlv_ = tmpvar_7;
}



#endif
#ifdef FRAGMENT

varying mediump float xlv_;
varying mediump vec2 xlv_TEXCOORD1;
varying mediump vec2 xlv_TEXCOORD0;
uniform mediump vec4 _Color;
uniform sampler2D _Diffuse1;
void main ()
{
  mediump vec4 Tex2D1_1;
  mediump vec4 Tex2D0_2;
  lowp vec4 tmpvar_3;
  tmpvar_3 = texture2D (_Diffuse1, xlv_TEXCOORD0);
  Tex2D0_2 = tmpvar_3;
  lowp vec4 tmpvar_4;
  tmpvar_4 = texture2D (_Diffuse1, xlv_TEXCOORD1);
  Tex2D1_1 = tmpvar_4;
  gl_FragData[0] = (((Tex2D0_2.xxxx + Tex2D1_1.yyyy) * xlv_) * _Color);
}



#endif"
}
SubProgram "gles3 " {
"!!GLES3#version 300 es


#ifdef VERTEX

#define gl_Vertex _glesVertex
in vec4 _glesVertex;
#define gl_Color _glesColor
in vec4 _glesColor;
#define gl_MultiTexCoord0 _glesMultiTexCoord0
in vec4 _glesMultiTexCoord0;

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
#line 318
struct v2f {
    highp vec4 pos;
    mediump vec2 uv0;
    mediump vec2 uv1;
    mediump float a;
};
#line 311
struct appdata {
    highp vec4 vertex;
    highp vec4 texcoord;
    highp vec4 color;
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
uniform sampler2D _Diffuse1;
uniform mediump vec4 _Color;
uniform mediump float _Texture1PanSpeedX;
uniform mediump float _Texture1PanSpeedY;
#line 310
uniform mediump float _Texture2PanSpeed;
#line 326
uniform highp vec4 _Diffuse1_ST;
#line 327
v2f vert( in appdata v ) {
    v2f o;
    #line 330
    o.pos = (glstate_matrix_mvp * v.vertex);
    mediump vec4 texCoords1 = v.texcoord;
    mediump vec4 texCoords2 = v.texcoord;
    mediump float multiplier = float( (_Time * _Texture1PanSpeedY));
    #line 334
    mediump float Sin0 = sin(multiplier);
    mediump float yAddition = ((Sin0 * 0.25) + 0.75);
    texCoords1.y += yAddition;
    mediump float xAddition = float( (_Time * _Texture1PanSpeedX));
    #line 338
    xAddition -= floor(xAddition);
    texCoords1.x += xAddition;
    xAddition = float( (_Time * _Texture2PanSpeed));
    xAddition -= floor(xAddition);
    #line 342
    texCoords2.x += xAddition;
    o.a = v.color.w;
    o.uv0 = ((texCoords1.xy * _Diffuse1_ST.xy) + _Diffuse1_ST.zw);
    o.uv1 = ((texCoords2.xy * _Diffuse1_ST.xy) + _Diffuse1_ST.zw);
    #line 346
    return o;
}
out mediump vec2 xlv_TEXCOORD0;
out mediump vec2 xlv_TEXCOORD1;
out mediump float xlv_;
void main() {
    v2f xl_retval;
    appdata xlt_v;
    xlt_v.vertex = vec4(gl_Vertex);
    xlt_v.texcoord = vec4(gl_MultiTexCoord0);
    xlt_v.color = vec4(gl_Color);
    xl_retval = vert( xlt_v);
    gl_Position = vec4(xl_retval.pos);
    xlv_TEXCOORD0 = vec2(xl_retval.uv0);
    xlv_TEXCOORD1 = vec2(xl_retval.uv1);
    xlv_ = float(xl_retval.a);
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
#line 318
struct v2f {
    highp vec4 pos;
    mediump vec2 uv0;
    mediump vec2 uv1;
    mediump float a;
};
#line 311
struct appdata {
    highp vec4 vertex;
    highp vec4 texcoord;
    highp vec4 color;
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
uniform sampler2D _Diffuse1;
uniform mediump vec4 _Color;
uniform mediump float _Texture1PanSpeedX;
uniform mediump float _Texture1PanSpeedY;
#line 310
uniform mediump float _Texture2PanSpeed;
#line 326
uniform highp vec4 _Diffuse1_ST;
#line 348
mediump vec4 frag( in v2f i ) {
    #line 350
    mediump vec4 Tex2D0 = texture( _Diffuse1, i.uv0);
    mediump vec4 Splat1 = Tex2D0.xxxx;
    mediump vec4 Tex2D1 = texture( _Diffuse1, i.uv1);
    mediump vec4 Splat2 = Tex2D1.yyyy;
    #line 354
    mediump vec4 col;
    col.xyzw = (((Splat1 + Splat2) * i.a) * _Color);
    return col;
}
in mediump vec2 xlv_TEXCOORD0;
in mediump vec2 xlv_TEXCOORD1;
in mediump float xlv_;
void main() {
    mediump vec4 xl_retval;
    v2f xlt_i;
    xlt_i.pos = vec4(0.0);
    xlt_i.uv0 = vec2(xlv_TEXCOORD0);
    xlt_i.uv1 = vec2(xlv_TEXCOORD1);
    xlt_i.a = float(xlv_);
    xl_retval = frag( xlt_i);
    gl_FragData[0] = vec4(xl_retval);
}


#endif"
}
}
Program "fp" {
SubProgram "gles " {
"!!GLES"
}
SubProgram "gles3 " {
"!!GLES3"
}
}
 }
}
Fallback "Diffuse"
}