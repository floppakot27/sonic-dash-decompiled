Shader "Sonic Dash/Diffuse, Lit + Vx Colours" {
Properties {
 _MainTex ("Base (RGB)", 2D) = "" {}
}
SubShader { 
 Pass {
  Tags { "QUEUE"="Geometry" }
  Lighting On
  Cull Off
  ColorMaterial AmbientAndDiffuse
  SetTexture [_MainTex] { combine texture * primary double }
  SetTexture [_MainTex] { ConstantColor (0.5,0.5,0.5,1) combine previous * constant double }
 }
}
}