uniform vec2 resolution;
uniform float time;

const float PI = 3.1415927;

const vec4 hlf = vec4(0.5);
const vec4 white = vec4(1.0);
const vec4 black = vec4(0.0);
const vec4 red = vec4(1.0, 0.0, 0.0, 0.0);
const vec4 green = vec4(0.0, 1.0, 0.0, 0.0);
const vec4 blue = vec4(0.0, 0.0, 1.0, 0.0);

float dotSpacing = 0.007;
float dotSz = 0.002;

const float EPS = 0.0001;

const int marchSteps = 64;
const float marchDist = 20.0;

vec3 rdrFlare(vec2 uv)
{
	float a = atan(uv.y, uv.x) / 3.1415927;
	float wave = (sin(a*PI)*cos(a*PI*20.0 + time * 10.0)*sin((a*PI)*50.0))*0.5 + 0.5;
	float l = length(uv);
	float minL = 0.3*(sin(time)*0.5 + 0.5);
	float maxL = 1.0;
	float op = (l - minL) / (maxL - minL);
	op = (op)*op*op;
	float opa = sin(time*12.24)*0.2 + 0.6;
	return vec3(wave)*clamp(op, 0.0, 1.0)*opa*vec3(0.792, 0.123, 0.543);
}


vec3 rotate(vec3 v, float psi, float theta, float phi)
{
	float cosPsi = cos(psi);
	float sinPsi = sin(psi);
	float cosTheta = cos(theta);
	float sinTheta = sin(theta);
	float cosPhi = cos(phi);
	float sinPhi = sin(phi);
	mat3 rot = mat3(cosPsi*cosTheta*cosPhi - sinPsi * sinPhi, -cosPsi * cosTheta*sinPhi - sinPsi * cosPhi, cosPsi*sinTheta,
		sinPsi*cosTheta*cosPhi + cosPsi * sinPhi, -sinPsi * cosTheta*sinPhi + cosPsi * cosPhi, sinPsi*sinTheta,
		-sinTheta * cosPhi, sinTheta*sinPhi, cosTheta);
	return rot * v;
}

float sdf_cylinder(vec3 p, float rad)
{
	return length(p.xy) - rad;
}

float sdf_cube(vec3 p, vec3 sz)
{
	float xDiff = abs(p.x) - sz.x;
	float yDiff = abs(p.y) - sz.y;
	float zDiff = abs(p.z) - sz.z;
	return max(xDiff, max(yDiff, zDiff));
}

float sdf_inter(float a, float b)
{
	return max(a, b);
}

float sdf_union(float a, float b)
{
	return min(a, b);
}

float sdf_diff(float a, float b)
{
	return max(a, -b);
}

float sdf_button(vec3 p, float rad, float thick)
{
	float cyl = sdf_cylinder(p, rad);
	float cube = sdf_cube(p, vec3(rad, rad, thick));
	return sdf_inter(cyl, cube);
}

float sdf_buttons(vec3 p)
{
	float depth = 0.5;
	float height = 0.6;
	float rad = 0.15;
	float thick = 0.15;
	float a = sdf_button(p + vec3(1.2, height, depth), rad, thick);
	float b = sdf_button(p + vec3(0.8, height, depth), rad, thick);
	float c = sdf_button(p + vec3(0.4, height, depth), rad, thick);

	return sdf_union(sdf_union(a, b), c);
}

float sdf_speaker(vec3 p)
{
	float maxSize = 0.5;
	float timeFact = 29.5;
	float sz = sin(time*timeFact);
	bool isInc = sz > sin(time*timeFact - 0.1);
	float size = (isInc ? maxSize : sz)*0.25 + 0.5;

	float depth = 0.3 + size * 0.4;
	float height = -0.2;
	float rad = size;
	float thick = 0.15;
	float a = sdf_button(p + vec3(-1.2, height, depth), rad, thick);
	float b = sdf_button(p + vec3(1.2, height, depth), rad, thick);

	return sdf_union(a, b);
}

float sdf_scene(vec3 p)
{
	p -= vec3(0.0, 0.0, 15.0);
	p = rotate(p, cos(time) + PI, time, 0.0);
	float thick = 0.4;
	float width = 1.0;
	float leng = 2.5;
	float horiz = sdf_cube(p, vec3(leng, width, thick));
	float vert = sdf_cube(p, vec3(width, leng, thick));
	return  sdf_union(horiz, vert);
}

vec3 sdf_normal(vec3 p)
{
	float epsilon = EPS;
	return vec3(
		sdf_scene(vec3(p.x + epsilon, p.y, p.z)) - sdf_scene(vec3(p.x - epsilon, p.y, p.z)),
		sdf_scene(vec3(p.x, p.y + epsilon, p.z)) - sdf_scene(vec3(p.x, p.y - epsilon, p.z)),
		sdf_scene(vec3(p.x, p.y, p.z + epsilon)) - sdf_scene(vec3(p.x, p.y, p.z - epsilon)));
}

float lengthNY(vec3 v)
{
	return abs(v.x) + abs(v.y) + abs(v.z);
}
float lengthNY(vec2 v)
{
	return lengthNY(vec3(v.x, v.y, 0.0));
}
float lengthSqr(vec3 a)
{
	return a.x*a.x + a.y*a.y + a.z*a.z;
}

float sat(float a)
{
	return clamp(a, 0.0, 1.0);
}

vec3 sat(vec3 v)
{
	return vec3(sat(v.x), sat(v.y), sat(v.z));
}

vec2 dirToUv(vec3 dir, float offYaw)
{
	float borderUp = 0.2507;
	float yaw = atan(dir.x, dir.z) / PI;
	yaw *= 0.5;
	yaw += 0.5;
	float pitch = acos(dir.y) / PI;
	float ratioY = 1.0 - borderUp * 2.0;
	return vec2(yaw, (1.0 - pitch)*ratioY + borderUp);
}

vec3 rdrStar(vec2 angles)
{
	return vec3(0.0);
}

vec3 rdrSky(vec3 v)
{
	return vec3(0.0);
}

vec3 setCam(vec2 uv, float fov, float yaw, float pitch, float roll)
{
	vec3 fwd = vec3(0.0, 0.0, 1.0);
	vec3 right = vec3(-1.0, 0.0, 0.0);
	vec3 up = vec3(0.0, 1.0, 0.0);
	float cosRoll = cos(roll);
	float sinRoll = sin(roll);
	float cosYaw = cos(yaw);
	float sinYaw = sin(yaw);
	float cosPitch = cos(pitch);
	float sinPitch = sin(pitch);

	mat3 rotZ = mat3(1.0, 0.0, 0.0,
		0.0, cosPitch, -sinPitch,
		0.0, sinPitch, cosPitch);
	mat3 rotY = mat3(cosYaw, 0.0, sinYaw,
		0.0, 1.0, 0.0,
		-sinYaw, 0.0, cosYaw);
	mat3 rotX = mat3(cosRoll, -sinRoll, 0.0,
		sinRoll, cosRoll, 0.0,
		0.0, 0.0, 1.0);
	vec3 vec = fwd;
	vec += (uv.x*right + uv.y*up)*fov;
	vec = rotY * rotX * (rotZ* vec);
	vec = normalize(vec);

	return vec;
}

vec3 rdrMat(vec3 p, vec2 uv)
{

	return vec3(0.0);
}

vec3 rdr(vec2 uv)
{
	vec3 orig = vec3(0.0, 0.0, 2.0);
	vec3 tex = black.xxx;
	vec3 dirMarch = setCam(uv, 0.9*(sin(time*12.389)*0.5 + 1.2), 0.0, 0.0, 0.0);
	float totDist = 0.0;
	vec3 p = orig + dirMarch;
	float lastDist = -1.0;
	vec3 lightPos = vec3(5.0, 10.0, -4.0);
	for (int i = 0; i < marchSteps && totDist < marchDist; ++i)
	{
		float dist = sdf_scene(p) - EPS * 10.0;
		if (dist < EPS)
		{
			vec3 norm = normalize(sdf_normal(p));
			vec3 lightDir = normalize(lightPos - p);
			vec3 colCube = vec3(1.0 - uv.x, uv.y, sin(time)*0.5 + 0.9);
			vec3 refl = reflect(normalize(p - orig), norm);
			vec2 reflUv = dirToUv(refl, 0.0);
			float minDist = 12.0;
			float maxDist = 16.0;
			float deltaDist = maxDist - minDist;
			tex = (vec3(uv.x + 0.5, uv.y + sin(uv.y + time), 1.0)*float(i) / 32.0) + (1.0 - (clamp(totDist - minDist, 0.0, deltaDist) / deltaDist))*vec3(0.3);
			tex += rdrMat(p, uv);
			break;
		}

		lastDist = dist;
		p += dirMarch * dist;
		totDist += dist;
	}
	return tex;
}

vec3 renderAsDot(vec2 uv, vec2 center)
{
	uv -= center;
	float curSize = dotSz;
	vec2 uvMod = mod(uv, dotSpacing);

	vec3 outCol = vec3((length(uvMod - (dotSpacing / 2.0)) < curSize));

	return outCol;
}

vec3 rdrChroma(vec2 uv)
{
int pointerCount=1;
	float factFlare2 = lengthNY(uv)*float(pointerCount + 1)*0.2;
	vec2 dir = normalize(uv)*pow(length(uv), 0.5*sin(time) + 1.5);/* vec2(0.01)*sin(uv.y + time);*/
	float r = rdr(uv + dir).x;
	float g = rdr(uv).y;
	float b = rdr(uv - dir).z;
	vec3 rdrV = vec3(r, g, b);
	return (rdrV*blue.xyz) + vec3(0.543, 0.22, 0.789)*factFlare2 + 2.0*rdrV*renderAsDot(uv, vec2(0.5)) + rdrFlare(uv)*5.0;
}

void main(void) {
	vec2 uv = gl_FragCoord.xy / resolution.xy;
	float ratio = resolution.x / resolution.y;
	uv -= hlf.xy;
	uv.x *= ratio;

	vec3 outCol = rdrChroma(uv);
	gl_FragColor = vec4(outCol, 1.0);
}

