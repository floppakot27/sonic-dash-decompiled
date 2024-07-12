Shader "Sonic Dash/Diffuse, UnLit + Vx Colours" {
Properties {
 _MainTex ("Base (RGB)", 2D) = "" {}
}
SubShader { 
 Pass {
  ColorMaterial AmbientAndDiffuse
  SetTexture [_MainTex] { combine texture * primary double }
  SetTexture [_MainTex] { ConstantColor (0.35,0.35,0.35,1) combine previous * constant double }
 }
}
}