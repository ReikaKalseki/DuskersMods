using System;

using System.Collections.Generic;

using UnityEngine;

using ReikaKalseki.DIDrones;

namespace ReikaKalseki.DIDrones {

	public static class MathUtil {
		
		public static float py3d(Vector3 from, Vector3 to) {
			return py3d(from.x, from.y, from.z, to.x, to.y, to.z);
		}
		
	    public static float py3d(float rawX, float rawY, float rawZ, float rawX2, float rawY2, float rawZ2) {
	    	float dx = rawX2-rawX;
	    	float dy = rawY2-rawY;
	    	float dz = rawZ2-rawZ;
	    	return py3d(dx, dy, dz);
	    }
		
	    public static float py3d(float dx, float dy, float dz) {
			return Mathf.Sqrt(py3dS(dx, dy, dz));
	    }
		
	    public static float py3dS(float rawX, float rawY, float rawZ, float rawX2, float rawY2, float rawZ2) {
	    	float dx = rawX2-rawX;
	    	float dy = rawY2-rawY;
	    	float dz = rawZ2-rawZ;
	    	return py3dS(dx, dy, dz);
	    }
		
	    public static float py3dS(float dx, float dy, float dz) {
	    	return dx*dx+dy*dy+dz*dz;
	    }
		
		public static Vector3 findRandomPointInsideEllipse(Vector3 center, float length, float width) {
			Rect rec = new Rect(center.x-length/2, center.z-width/2, length, width);
			Vector2 vec = getRandomVectorInside(rec);
			//SBUtil.log(rec.ToString());
			int i = 0;
			while (!isPointInsideEllipse(vec.x-center.x, 0, vec.y-center.z, length/2, 0, width/2)) {
				float ra = length/2;
				float rc = width/2;
				float x = vec.x-center.x;
				float z = vec.y-center.z;
				//SBUtil.log("Need new pos @ "+i+", vec "+vec+" failed for "+rec.xMin+">"+rec.xMax+" , "+rec.yMin+">"+rec.yMax+" = "+((x*x)/(ra*ra))+" & "+((z*z)/(rc*rc)));
				vec = getRandomVectorInside(rec);
				i++;
			}
			return new Vector3(vec.x, center.y, vec.y);
		}
		
		public static Vector2 getRandomVectorInside(Rect rec) {
			float x = UnityEngine.Random.Range(rec.xMin, rec.xMax);
			float z = UnityEngine.Random.Range(rec.yMin, rec.yMax);
			return new Vector2(x, z);
		}

		public static bool isPointInsideEllipse(float x, float y, float z, float ra, float rb, float rc) {
			return (ra > 0 ? ((x*x)/(ra*ra)) : 0) + (rb > 0 ? ((y*y)/(rb*rb)) : 0) + (rc > 0 ? ((z*z)/(rc*rc)) : 0) <= 1;
		}

		public static bool isPointInCylinder(Vector3 center, Vector3 point, float r, float h) {
			return Math.Abs(point.y-center.y) <= h && (center-point).setY(0).magnitude <= r;
		}
		
		public static void rotateObjectAround(GameObject go, Vector3 point, float amt) {
			go.transform.RotateAround(point, Vector3.up, (float)amt);
		}
		
		public static Vector3 getRandomVectorBetween(Vector3 min, Vector3 max) {
			return new Vector3(UnityEngine.Random.Range(min.x, max.x), UnityEngine.Random.Range(min.y, max.y), UnityEngine.Random.Range(min.z, max.z));
		}
		
		public static Vector3 getRandomVectorAround(Vector3 pos, float range) {
			return getRandomVectorAround(pos, new Vector3(range, range, range));
		}
		
		public static Vector3 getRandomVectorAround(Vector3 pos, Vector3 range) {
			return getRandomVectorBetween(pos-range, pos+range);
		}
		
		public static Vector3 getRandomVectorAround(Vector3 pos, float r0, float r1) {
			float r = UnityEngine.Random.Range(r0, r1);
			float ang = UnityEngine.Random.Range(0, 360F);
			float cos = (float)Math.Cos(ang*Math.PI/180D);
			float sin = (float)Math.Sin(ang*Math.PI/180D);
			return pos+r*new Vector3(cos, 0, sin);
		}
		
		public static float getDistanceToLine(Vector3 point, Vector3 a, Vector3 b) {
			return getDistanceToLine(point, a.x, a.y, a.z, b.x, b.y, b.z);
		}
		//just like when I did it for ChokePoint: https://wikimedia.org/api/rest_v1/media/math/render/svg/aad3f60fa75c4e1dcbe3c1d3a3792803b6e78bf6
		public static float getDistanceToLine(Vector3 point, float x1, float y1, float z1, float x2, float y2, float z2) {
			float denom = (x2-x1)*(x2-x1)+(z2-z1)*(z2-z1);
			float num = (x2-x1)*(z1-point.z)-(x1-point.x)*(z2-z1);
			return Mathf.Abs(num)/Mathf.Sqrt(denom);
		}
		
		public static float getDistanceToLineSegment(Vector3 point, Vector3 a, Vector3 b) {
			return getDistanceToLineSegment(point, a.x, a.y, a.z, b.x, b.y, b.z);
		}
		
		public static float getScalarOfClosestPointToLineSegment(Vector3 point, float x1, float y1, float z1, float x2, float y2, float z2) {
			float dist = py3dS(x1, y1, z1, x2, y2, z2);
			if (dist <= 0.001)
				return py3d(point.x, point.y, point.z, x1, y1, z1);
			float t = ((point.x-x1)*(x2-x1)+(point.y-y1)*(y2-y1)+(point.z-z1)*(z2-z1))/dist;
			return Mathf.Clamp(t, 0, 1);
		}
		
		public static Vector3 getClosestPointToLineSegment(Vector3 point, Vector3 p1, Vector3 p2) {
			return getClosestPointToLineSegment(point, p1.x, p1.y, p1.z, p2.x, p2.y, p2.z);
		}
		
		public static Vector3 getClosestPointToLineSegment(Vector3 point, float x1, float y1, float z1, float x2, float y2, float z2) {
			float t = (float)getScalarOfClosestPointToLineSegment(point, x1, y1, z1, x2, y2, z2);
			return Vector3.Lerp(new Vector3((float)x1, (float)y1, (float)z1), new Vector3((float)x2, (float)y2, (float)z2), t);
		}
		
		public static float getDistanceToLineSegment(Vector3 point, float x1, float y1, float z1, float x2, float y2, float z2) {
			float t = getScalarOfClosestPointToLineSegment(point, x1, y1, z1, x2, y2, z2);
			return py3d(point.x, point.y, point.z, x1+t*(x2-x1), y1+t*(y2-y1), z1+t*(z2-z1));
		}

		public static float linterpolate(float x, float x1, float x2, float y1, float y2, bool clamp = false) {
			if (clamp && x <= x1)
				return y1;
			else if (clamp && x >= x2)
				return y2;
			return y1+(x-x1)/(x2-x1)*(y2-y1);
		}

		public static Vector3 interpolate(Vector3 a, Vector3 b, float amt) {
			return a+(b-a)*amt;
		}
	
		public static float getRandomPlusMinus(float val, float range) {
			return UnityEngine.Random.Range(val-range, val+range);
		}
	
		public static int getRandomPlusMinus(int val, int range) {
			return UnityEngine.Random.Range(val-range, val+range);
		}
		
		public static Bounds getBounds(float x1, float y1, float z1, float x2, float y2, float z2) {
			Vector3 v1 = new Vector3((float)x1, (float)y1, (float)z1);
			Vector3 v2 = new Vector3((float)x2, (float)y2, (float)z2);
			Bounds b = new Bounds(Vector3.zero, Vector3.zero);
			b.SetMinMax(v1, v2);
			return b;
		}
		
		//starts at 1 for the first digit after '.'
		public static int getNthDecimalPlace(float value, int place) {
			value = Mathf.Abs(value)*Mathf.Pow(10, place);
			return ((int)(value))%10;
		}
		
		public static Quaternion unitVecToRotation(Vector3 unit) {
			return Quaternion.FromToRotation(Vector3.up, unit);//.Euler();
		}
		
		public static Vector3 rotateVectorAroundAxis(Vector3 input, Vector3 axis, float angle) {
			return Quaternion.AngleAxis(angle, axis) * input;
		}
		
		public static Vector3 getRandomPointAtSetDistance(Vector3 pos, float dist) {
			return pos+new Vector3(UnityEngine.Random.Range(-1F, 1F), UnityEngine.Random.Range(-1F, 1F), UnityEngine.Random.Range(-1F, 1F)).setLength(dist);
		}
		
	}
}
