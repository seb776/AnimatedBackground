uniform float time;
uniform vec2 resolution;

vec3 highlightRed = vec3(255,233,161)/155.0;
vec3 redStr =vec3(255.0,39.0,49.0)/255.0;// vec3(0.8793, 0.213,0.421);
const float PI = 3.1415927;
const float EPS = 0.0001;
float rand(vec2 co){ return fract(sin(dot(co.xy ,vec2(12.9898,78.233))) * 43758.5453); }

float lenSqr(vec2 v)
{
  return v.x*v.x+v.y*v.y;
}

float lenNY(vec2 v)
{
  return abs(v.x)+abs(v.y);
}

float getAngle(vec2 uv)
{
  return atan(uv.y, uv.x)/PI;
}

float sat(float a)
{
  return clamp(a,0.0,1.0);
}

vec3 sat(vec3 v)
{
  return vec3(sat(v.x),sat(v.y),sat(v.z));
}

float mypow(float a)
{
  return pow(a,15.0);
}


vec3 rdrStar(vec2 uv, vec3 pos)
{
  float val = 1.0-clamp(length(uv-pos.xy)/pos.z,0.0,1.0);
  return vec3(pow(val,5.0));
}


vec3 rdrStarTrail(vec2 uv, vec3 pos)
{
  //return vec3(0.0);
  float len = length(uv);
  float trailLen = length(pos.xy);
  float flen = 1.0-sat(dot(-uv+pos.xy, -pos.xy));//sat(len/trailLen);
	return vec3(flen)*0.01;
  float ftrail = 1.0-sat(trailLen / (.05));//end of trail
  return ftrail*2.2*len*len*pow(flen,50.0)*vec3(sat(mypow(dot(normalize(pos.xy), normalize(uv)))));
}



float renderStars(vec2 uv, float speed, float sz, float offset)
{
  float angle = getAngle(uv);
  int cntStars = 50;
  vec3 res;

  for(int i = 0; i < cntStars; ++i)
  {
    float fi = float(i);
    float maxRad = 1.5;
    float r = mod(fi/float(cntStars)+time*speed,maxRad);
    float a =fi*offset+offset;
    vec2 pos = vec2(r*cos(a),r*sin(a));
//    res+= rdrStarTrail(uv, vec3(pos,sz));
    res+= rdrStar(uv, vec3(pos,0.05*r*sz));
  }
  return res.x;
  //return vec3(1.0)-sat(mix(highlightRed, redStr, res.x));
}

vec3 rdrBg(vec2 uv)
{
  vec3 bg = 0.6*vec3(72,12,66)/255.0;
  float lenny = sat(lenNY(uv));
  vec3 sun = 0.7*mix(highlightRed, vec3(0.0), sqrt(sat(lenny*1.2)));
  vec3 sun2 = 0.2*mix(highlightRed, vec3(0.0), lenny*0.7);

  return bg+sun+sun2;
}


void main() {
  vec2 uv = gl_FragCoord.xy / resolution.xx;
  vec2 cuv = uv - (resolution.xy/resolution.xx)*0.5;
  if (abs(cuv.x) > EPS && abs(cuv.y) > EPS)
	cuv = normalize(cuv)*pow(length(cuv),0.5);
  
  float noise = 0.1*sat(rand(10.0*uv+vec2(time*0.0001,0.0)));
  cuv *= 2.0;
  vec3 bg = rdrBg(cuv);
  float spd = 0.1;

  float str1 = renderStars(cuv,1.0*spd, 3.0, 1.0);
  vec3 outCol = 0.5*(vec3(1.0)-sat(mix(highlightRed, redStr, str1)));
  vec3 outCol2 = 0.5*vec3(highlightRed)*renderStars(cuv,2.0*spd, 0.7, 3.53);
  gl_FragColor = vec4(bg+outCol+noise+outCol2, 0.0);
}