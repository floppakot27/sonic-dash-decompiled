Shader "Custom/Skydome" {
Properties {
 _MainTex ("Base (RGB)", 2D) = "white" {}
}
SubShader { 
 LOD 100
 Tags { "QUEUE"="Background" }
 Pass {
  Tags { "QUEUE"="Background" }
  ZTest Less
  SetTexture [_MainTex] { combine texture }
 }
}
}