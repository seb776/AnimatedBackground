uniform float time;
uniform vec2 resolution;

float sat(float a)
{
    return clamp(a, 0.,1.);
}

float rand(vec2 co){
  return sat(fract(sin(dot(co.xy ,vec2(12.9898,78.233))) * 43758.5453));
}

float drawExcentric(vec2 uv, float aRad, float bRad, vec2 excent)
{
    float luv = length(uv);
    
  	return float(length(uv - excent) > bRad && luv < aRad);
}

float drawCircle(vec2 uv, float rad)
{
    return float(length(uv) < rad);
}

float drawLight(vec2 uv, float sz)
{
    return float(sat(-(length(uv) - sz)));
}

vec4 drawPlanet(vec2 uv, float radius, float subRadius, vec2 excentricDir)
{
    float isOnPlanet = float(length(uv) < radius);
    
    vec3 col = vec3(float(drawExcentric(uv, radius,subRadius,excentricDir * (radius - subRadius)) > 0.5));
    
    col = col * (0.5 + 0.5*cos(time+uv.xyx+vec3(0,2,4)));
    return vec4(col, isOnPlanet);
}

float drawDim(vec2 uv, float sz, float strength)
{
    return pow(sat(length(uv/sz)), strength);
}

vec3 rdr(vec2 uv)
{
    vec3 col;
    
    vec2 bgPos = vec2(0.,0.1);
    
    col = vec3(1.)*(max(rand(uv),0.99)-0.99)*100.*0.8* length(uv);

    col += pow(sat(1.-length(uv*.5-bgPos)),5.9)*vec3(129, 116, 242)/255.; // background
    col *= drawDim(uv*vec2(1.,0.5)-vec2(0.3,-0.1), 0.25, 1.5);
    

    
    vec4 planetA = drawPlanet(uv + vec2(-0.21,-0.0), 0.17, 0.16, 0.8*normalize(vec2(1.,-0.2)));
    float orbitTime = time * 0.5;
    
    
    
    
    vec2 bPos = vec2(-(sin(orbitTime)*0.5-0.21), 0.);
    
    vec2 bDir = bgPos - bPos;
    
    float bRad = 0.1*(cos(orbitTime) * 0.3 + 0.7);
    vec4 planetB = drawPlanet(uv - bPos, bRad, sat(bRad-0.1*bRad), normalize(-bDir));
    
    
    
    
    
    if (planetA.w > 0.)
        col = planetA.xyz;
    if (!(bRad < 0.05 && planetA.w > 0.) )
    	col = (planetB.xyz*planetB.w)+col - vec3(0.1)*planetB.w;//mix((planetB.xyz*planetB.w)+col, (planetB.xyz*(1.-planetB.w))*col, 0.5);
    
    vec3 colFlare = vec3(89, 202, 247)/255.;
    col += colFlare*drawLight((uv-bgPos)*vec2(0.5,2.5), 0.5);  
    col += colFlare*drawLight((uv-bgPos)*vec2(0.9,100.5), 0.5);
    col += mix(colFlare, vec3(1.),0.9)*drawLight((uv-bgPos)*vec2(0.2,3.5), 0.5);
    
    col += mix(colFlare, vec3(1.),0.9)*drawLight((uv-bgPos)*vec2(0.2,3.5), 1.5)*0.1;
    return col;
}

void main()
{
    // Normalized pixel coordinates (from 0 to 1)
    vec2 uv = gl_FragCoord/resolution.xx;

    uv -= vec2(0.5) * (resolution.xy / resolution.xx);
    
    vec3 col;

   	col = rdr(uv);
    
    gl_FragColor = vec4(col,1.0);
}