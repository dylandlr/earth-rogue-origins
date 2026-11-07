using System;
namespace UnityEngine {
public struct Color32 {public Color32(byte r,byte g,byte b,byte a){}}
public static class Mathf {
public const float Deg2Rad=(float)Math.PI*2/360,Rad2Deg=1/Deg2Rad;
public static float Clamp(float x,float a,float b)=>Math.Min(b,Math.Max(x,a));
public static float Clamp01(float x)=>Clamp(x,0,1);
public static float Lerp(float a,float b,float t)=>a+(b-a)*Clamp01(t);
public static float Abs(float x)=>Math.Abs(x);
public static float Min(float a,float b)=>Math.Min(a,b);
public static float Max(float a,float b)=>Math.Max(a,b);
public static float Cos(float x)=>(float)Math.Cos(x);
public static float Sin(float x)=>(float)Math.Sin(x);
public static float Atan2(float y,float x)=>(float)Math.Atan2(y,x);
public static float Pow(float x,float y)=>(float)Math.Pow(x,y);
public static float Exp(float x)=>(float)Math.Exp(x);
}
public struct Vector2 {
public float x,y;
public Vector2(float a,float b){x=a;y=b;}
public static Vector2 zero=>new Vector2();
public static Vector2 operator +(Vector2 a,Vector2 b)=>new Vector2(a.x+b.x,a.y+b.y);
public static Vector2 operator -(Vector2 a,Vector2 b)=>new Vector2(a.x-b.x,a.y-b.y);
public static Vector2 operator *(Vector2 a,float t)=>new Vector2(a.x*t,a.y*t);
public static Vector2 operator *(float t,Vector2 a)=>a*t;
public static float Dot(Vector2 a,Vector2 b)=>a.x*b.x+a.y*b.y;
public float magnitude=>(float)Math.Sqrt(Dot(this,this));
public Vector2 normalized=>magnitude>1e-5f?new Vector2(x/magnitude,y/magnitude):zero;
public static Vector2 Lerp(Vector2 a,Vector2 b,float t)=>a+(b-a)*Mathf.Clamp01(t);
}}
