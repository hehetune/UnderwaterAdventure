﻿using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using _Scripts.Game.PoolingSystem;
using UnityEditor.Localization.Editor;
using UnityEngine;

namespace _Scripts.Game
{
    public static class Util
{
	// public constants
	public const float TwoPi =						2.0f*Mathf.PI;
	
	// default parameters
	public const float m_defaultMaxDampingScale =	0.125f;

	//----------------------------------------------------------------------------
	// Angle functions
	//----------------------------------------------------------------------------

	// Get an angle into 0-360 range
	public static float FixAngleDegrees(float ang)
	{
		// Is this really the best way to do this?
		while(ang >= 360.0f)
			ang -= 360.0f;
		while(ang < 0)
			ang += 360.0f;
		return ang;
	}
	
	// As above, in radians
	public static float FixAngleRadians(float ang)
	{
		while(ang >= TwoPi)
			ang -= TwoPi;
		while(ang < 0)
			ang += TwoPi;
		return ang;
	}
	
	// Get an angle into -180 to +180 range
	public static float FixAnglePlusMinusDegrees(float ang)
	{
		while(ang >= 180.0f)
			ang -= 360.0f;
		while(ang < -180.0f)
			ang += 360.0f;
		return ang;
	}
	
	// As above, in radians
	public static float FixAnglePlusMinusRadians(float ang)
	{
		while(ang >= Mathf.PI)
			ang -= TwoPi;
		while(ang < -Mathf.PI)
			ang += TwoPi;
		return ang;
	}
	
	//----------------------------------------------------------------------------
	// Blending and damping functions
	//----------------------------------------------------------------------------
	
	// Copy of MoveValueWithDamping from old engine.  The MathF class provides a MoveTowards method, but no damped version.
	public static float MoveTowardsWithDamping(float from, float to, float step, float dampingRange, float maxDampingScale = m_defaultMaxDampingScale)
	{
		if(to > from)
		{
			if((to-from) < dampingRange)
			{
				float slowdown = (to-from)/dampingRange;
				slowdown = Mathf.Clamp(slowdown, maxDampingScale, 1.0f);
				step *= slowdown;
			}
			from = Mathf.Min(from+step, to);
		}
		else if(to < from)
		{
			if((from-to) < dampingRange)
			{
				float slowdown = (from-to)/dampingRange;
				slowdown = Mathf.Clamp(slowdown, maxDampingScale, 1.0f);
				step *= slowdown;
			}
			from = Mathf.Max(from-step, to);
		}
		return from;
	}
	
	// Equivalent version for MoveAngleWithDamping / MathF.MoveTowardsAngle
	public static float MoveTowardsAngleRadiansWithDamping(float from, float to, float step, float dampingRange, float maxDampingScale = m_defaultMaxDampingScale)
	{
		float d = FixAnglePlusMinusRadians(to-from);
		float s = MoveTowardsWithDamping(0.0f, d, step, dampingRange, maxDampingScale);
		return FixAngleRadians(from+s);
	}
	
	public static float MoveTowardsAngleDegreesWithDamping(float from, float to, float step, float dampingRange, float maxDampingScale = m_defaultMaxDampingScale)
	{
		float d = FixAnglePlusMinusDegrees(to-from);
		float s = MoveTowardsWithDamping(0.0f, d, step, dampingRange, maxDampingScale);
		return FixAngleDegrees(from+s);
	}
	
	// Various versions for using on vectors.
	// Vector2 and Vector3 already contain a MoveTowards method.  Here we have damping versions, and also versions that work on a Vector3 with only the X/Y
	// components (seems to be easier to work with than Vector2).
	public static void MoveTowardsVector2WithDamping(ref Vector2 v, ref Vector2 target, float step, float dampingRange, float maxDampingScale = m_defaultMaxDampingScale)
	{
		v.x = MoveTowardsWithDamping(v.x, target.x, step, dampingRange, maxDampingScale);
		v.y = MoveTowardsWithDamping(v.y, target.y, step, dampingRange, maxDampingScale);
	}
	
	public static void MoveTowardsVector3(ref Vector3 v, ref Vector3 target, float step)
	{
		v.x = Mathf.MoveTowards(v.x, target.x, step);
		v.y = Mathf.MoveTowards(v.y, target.y, step);
		v.z = Mathf.MoveTowards(v.z, target.z, step);
	}
	
	public static void MoveTowardsVector3XY(ref Vector3 v, ref Vector3 target, float step)
	{
		v.x = Mathf.MoveTowards(v.x, target.x, step);
		v.y = Mathf.MoveTowards(v.y, target.y, step);
	}
	
	public static void MoveTowardsVector3XYWithDamping(ref Vector3 v, ref Vector3 target, float step, float dampingRange, float maxDampingScale = m_defaultMaxDampingScale)
	{
		v.x = MoveTowardsWithDamping(v.x, target.x, step, dampingRange, maxDampingScale);
		v.y = MoveTowardsWithDamping(v.y, target.y, step, dampingRange, maxDampingScale);
	}
	
	public static void MoveTowardsVector3WithDamping(ref Vector3 v, ref Vector3 target, float step, float dampingRange, float maxDampingScale = m_defaultMaxDampingScale)
	{
		v.x = MoveTowardsWithDamping(v.x, target.x, step, dampingRange, maxDampingScale);
		v.y = MoveTowardsWithDamping(v.y, target.y, step, dampingRange, maxDampingScale);
		v.z = MoveTowardsWithDamping(v.z, target.z, step, dampingRange, maxDampingScale);
	}
	
	public static void FilterValue(ref float value, ref float oldValue, float filter)
	{
		value = (value*filter) + (oldValue*(1.0f-filter));
		oldValue = value;
	}
	
	public static void FilterVector3(ref Vector3 vec, ref Vector3 oldVec, float filter)
	{
		vec.x = (vec.x*filter) + (oldVec.x*(1.0f-filter));
		vec.y = (vec.y*filter) + (oldVec.y*(1.0f-filter));
		vec.z = (vec.z*filter) + (oldVec.z*(1.0f-filter));
		oldVec = vec;
	}
	
	public static void FilterVector3XY(ref Vector3 vec, ref Vector3 oldVec, float filter)
	{
		vec.x = (vec.x*filter) + (oldVec.x*(1.0f-filter));
		vec.y = (vec.y*filter) + (oldVec.y*(1.0f-filter));
		oldVec = vec;
	}
	
	// Remap a value from one range (in0 -> in1, CLAMPED) to a different range (out0 -> out1).
	// Use this to interpolate between 2 values (out0, out1) based on where some other number (value) sits
	// between two other values (in0, in1).
	public static float Remap(float value, float in0, float in1, float out0, float out1)
	{
		float d = in1-in0;
		float t = (value-in0)/d;
		t = Mathf.Clamp01(t);
		return Mathf.Lerp(out0, out1, t);
	}
	
	// As above, but use to interpolate between two vectors.
	public static Vector3 RemapToVector3(float value, float in0, float in1, Vector3 out0, Vector3 out1)
	{
		float d = in1-in0;
		float t = (value-in0)/d;
		t = Mathf.Clamp01(t);
		float x = Mathf.Lerp(out0.x, out1.x, t);
		float y = Mathf.Lerp(out0.y, out1.y, t);
		float z = Mathf.Lerp(out0.z, out1.z, t);
		return new Vector3(x, y, z);
	}
	
	public static Vector3 RemapToVector3XY(float value, float in0, float in1, Vector3 out0, Vector3 out1)
	{
		float d = in1-in0;
		float t = (value-in0)/d;
		t = Mathf.Clamp01(t);
		float x = Mathf.Lerp(out0.x, out1.x, t);
		float y = Mathf.Lerp(out0.y, out1.y, t);
		return new Vector3(x, y, out0.z);
	}
	
	//----------------------------------------------------------------------------
	// Debug draw functions
	//----------------------------------------------------------------------------

	public static void DrawDebugPoint(Vector3 p, Color color, float r=0.5f)
	{
		Vector3 s0 = new Vector3(p.x-r, p.y, p.z);
		Vector3 e0 = new Vector3(p.x+r, p.y, p.z);
		Vector3 s1 = new Vector3(p.x, p.y-r, p.z);
		Vector3 e1 = new Vector3(p.x, p.y+r, p.z);
		Debug.DrawLine(s0, e0, color, 0.0f, false);
		Debug.DrawLine(s1, e1, color, 0.0f, false);
	}
	public static void DrawDebugPoint(Vector3 p)
	{
		DrawDebugPoint(p, Color.white);
	}
	
	public static void DrawDebugLine(Vector3 s, Vector3 e, Color color)
	{
		Debug.DrawLine(s, e, color, 0.0f, false);
	}
	
	public static void DrawDebugLine(Vector3 s, Vector3 e)
	{
		DrawDebugLine(s, e, Color.white);
	}

	public static void DrawDebugCircle(Vector3 p, float r, Color color, int divisions=32)
	{
		float ang = 0;
		float angStep = TwoPi/divisions;
		Vector3 lastPos = Vector3.zero;
		Vector3 firstPos = Vector3.zero;

		for(int i=0; i<divisions; i++)
		{
			Vector3 s = new Vector3(p.x + r*Mathf.Cos(ang), p.y + r*Mathf.Sin(ang), p.z);
			if(i > 0)
				Debug.DrawLine(s, lastPos, color, 0.0f, false);
			else
				firstPos = s;
				
			lastPos = s;
			ang += angStep;
		}
		Debug.DrawLine(firstPos, lastPos, color, 0.0f, false);
	}
	public static void DrawDebugCircle(Vector3 p, float r)
	{
		DrawDebugCircle(p, r, Color.white);
	}
	
	public static void DrawDebugCapsule2D(Vector3 p0, Vector3 p1, float r, Color color, int divisions=32)
	{
		if(p0==p1)
		{
			DrawDebugCircle(p0, r, color, divisions);
			return;
		}
		Vector3 dir = (p1-p0).NormalizedXY();
		float baseAng = Mathf.Atan2(dir.y, dir.x);
	
		float ang = 0;
		divisions /= 2;							// only need half as many divisions per side of the capsule, we do both sides in parallel
		float angStep = Mathf.PI/divisions;		// only doing 180 degrees per side
		Vector3 lastPos0 = Vector3.zero;
		Vector3 lastPos1 = Vector3.zero;
		Vector3 firstPos0 = Vector3.zero;
		Vector3 firstPos1 = Vector3.zero;
		
		for(int i=0; i<divisions; i++)
		{
			float ang0 = baseAng + ang + (Mathf.PI*0.5f);
			float ang1 = baseAng + ang - (Mathf.PI*0.5f);
			Vector3 s0 = new Vector3(p0.x + r*Mathf.Cos(ang0), p0.y + r*Mathf.Sin(ang0), p0.z);
			Vector3 s1 = new Vector3(p1.x + r*Mathf.Cos(ang1), p1.y + r*Mathf.Sin(ang1), p1.z);
			if(i > 0)
			{
				Debug.DrawLine(s0, lastPos0, color, 0.0f, false);
				Debug.DrawLine(s1, lastPos1, color, 0.0f, false);
			}
			else
			{
				firstPos0 = s0;
				firstPos1 = s1;
			}
			
			lastPos0 = s0;
			lastPos1 = s1;
			ang += angStep;
		}
		Debug.DrawLine(firstPos0, lastPos1, color, 0.0f, false);
		Debug.DrawLine(firstPos1, lastPos0, color, 0.0f, false);
	}
	
	public static void DrawDebugCapsule2D(Vector3 p0, Vector3 p1, float r)
	{
		DrawDebugCapsule2D(p0, p1, r, Color.white);
	}
	
	public static void DrawDebugEllipse(Vector3 p, float rx, float ry, Color color, int divisions=32)
	{
		float ang = 0;
		float angStep = TwoPi/divisions;
		Vector3 lastPos = Vector3.zero;
		Vector3 firstPos = Vector3.zero;

		for(int i=0; i<divisions; i++)
		{
			Vector3 s = new Vector3(p.x + rx*Mathf.Cos(ang), p.y + ry*Mathf.Sin(ang), p.z);
			if(i > 0)
				Debug.DrawLine(s, lastPos, color, 0.0f, false);
			else
				firstPos = s;
				
			lastPos = s;
			ang += angStep;
		}
		Debug.DrawLine(firstPos, lastPos, color, 0.0f, false);
	}
	public static void DrawDebugEllipse(Vector3 p, float rx, float ry)
	{
		DrawDebugEllipse(p, rx, ry, Color.white);
	}
	
	public static void DrawDebugBounds2D(Bounds b, Color color)
	{
		Vector3 tl = new Vector3(b.min.x, b.max.y, b.center.z);
		Vector3 tr = new Vector3(b.max.x, b.max.y, b.center.z);
		Vector3 bl = new Vector3(b.min.x, b.min.y, b.center.z);
		Vector3 br = new Vector3(b.max.x, b.min.y, b.center.z);
		Debug.DrawLine(tl, tr, color, 0.0f, false);
		Debug.DrawLine(tr, br, color, 0.0f, false);
		Debug.DrawLine(br, bl, color, 0.0f, false);
		Debug.DrawLine(bl, tl, color, 0.0f, false);
	}
	public static void DrawDebugBounds2D(Bounds b)
	{
		DrawDebugBounds2D(b, Color.white);
	}
	
	public static void DrawDebugBounds2D(FastBounds2D b, Color color)
	{
		Vector3 tl = new Vector3(b.x0, b.y1, 0.0f);
		Vector3 tr = new Vector3(b.x1, b.y1, 0.0f);
		Vector3 bl = new Vector3(b.x0, b.y0, 0.0f);
		Vector3 br = new Vector3(b.x1, b.y0, 0.0f);
		Debug.DrawLine(tl, tr, color, 0.0f, false);
		Debug.DrawLine(tr, br, color, 0.0f, false);
		Debug.DrawLine(br, bl, color, 0.0f, false);
		Debug.DrawLine(bl, tl, color, 0.0f, false);
	}
	
	public static void DrawDebugBounds2D(FastBounds2D b)
	{
		DrawDebugBounds2D(b, Color.white);
	}
	
	public static void DrawDebugTransform(Transform t, float r=1.0f)
	{
		Vector3 s = t.position;
		Debug.DrawLine(s, s + (t.right.normalized * r), Color.red, 0.0f, false);
		Debug.DrawLine(s, s + (t.up.normalized * r), Color.green, 0.0f, false);
		Debug.DrawLine(s, s + (t.forward.normalized * r), Color.blue, 0.0f, false);
	}
	
	public static void DrawDebug3PointSpline(Vector3 p0, Vector3 p1, Vector3 p2, Color color, int divisions = 32)
	{
		float step = 1.0f/divisions;
		Vector3 lastPos = p0;
		float t = step;
		for(int i=0; i<divisions; i++)
		{
			Vector3 pos = GetSimpleSplinePoint(p0, p1, p2, t);
			Debug.DrawLine(lastPos, pos, color, 0.0f, false);
			lastPos = pos;
			t += step;
		}
	}
	public static void DrawDebug3PointSpline(Vector3 p0, Vector3 p1, Vector3 p2)
	{
		DrawDebug3PointSpline(p0, p1, p2, Color.white);
	}
	
	public static void DrawDebugContactPoint(ContactPoint p, float pointRadius = 0.2f, float normalLength = 1.0f)
	{
		if(pointRadius > 0.0f)
			DrawDebugPoint(p.point, Color.white, pointRadius);
		if(normalLength > 0.0f)
			DrawDebugLine(p.point, p.point + (p.normal * normalLength), Color.yellow);
	}
	
	public static void DrawDebugCollisionContacts(Collision coll, float pointRadius = 0.2f)
	{
		ContactPoint[] c = coll.contacts;
		for(int i=0, l=c.Length; i<l; i++)
			DrawDebugContactPoint(c[i], pointRadius);
	}
	
	//----------------------------------------------------------------------------
	// Random functions
	//----------------------------------------------------------------------------

	// RandRange function that returns an int between rmin and rmax inclusive, compatible with the old skool engine convention.
	// Unity's Random.Range is not inclusive of the max value.
	public static int RandRange(int rmin, int rmax)
	{
		return (rmin==rmax) ? rmin : Random.Range(rmin, rmax+1);
	}
	
	// Float version, this just calls the Unity one, just there for consistency so you can call Util.RandRange on either ints or floats
	public static float RandRange(float rmin, float rmax)
	{
		return (rmin==rmax) ? rmin : Random.Range(rmin, rmax);
	}
	
	// return a random float between 0 and 1 inclusive
	// update: is this necessary?  think Random.value does this
	public static float Rand01()
	{
		return Random.Range(0.0f, 1.0f);
	}

    public static float RandMinus1To1()
    {
        return Random.Range(-1f, 1f);
    }
	
	public static float RandAngleDegrees()
	{
		float r = Random.Range(0.0f, 360.0f);
		return (r==360.0f) ? 0.0f : r;
	}
	
	public static float RandAngleRadians()
	{
		float r = Random.Range(0.0f, TwoPi);
		return (r==TwoPi) ? 0.0f : r;
	}
	
	public static bool RandBool()
	{
		return Random.value < 0.5f;
	}
	
	// version of Random.InsideUnitCircle that returns a Vector3 with Z at zero (as opposed to a Vector2)
	public static Vector3 RandInsideUnitCircle()
	{
		Vector2 v2 = Random.insideUnitCircle;
		Vector3 v3 = new Vector3(v2.x, v2.y, 0.0f);
		return v3;
	}
	
	public static Vector3 RandOnUnitCircle()
	{
		return RandInsideUnitCircle().normalized;
	}
	
	//----------------------------------------------------------------------------
	// Vector/math functions
	//----------------------------------------------------------------------------

	// Thing to determine if an object's velocity is roughly pointing in the direction of another object.
	// We return normalize(targetPos - objectPos) DOT normalize(objectVelocity).
	// So we return 1 if moving directly to the point, 0 if moving perpendicular to it, -1 if moving directly away from it.
	public static float DotFacingPoint(Vector3 objectPos, Vector3 objectVel, Vector3 targetPos)
	{
		Vector3 dirToTarget = (targetPos-objectPos).normalized;
		Vector3 dirVel = objectVel.normalized;
		return Vector3.Dot(dirToTarget, dirVel);
	}

	// Function to calculate the launch direction for a projectile so it will try to intercept a moving target.
	// Does not currently solve this properly, just gives an improved direction versus aiming straight at the target.
	// This is nicked from Grabatron.  Adapted for 2D XY only, using Vector3.
	public static Vector3 GetInterceptDirectionXY(Vector3 from, float speed, Vector3 destPos, Vector3 destVel)
	{
		// only interested in 2D XY pos
		from.z = 0.0f;
		destPos.z = 0.0f;
		destVel.z = 0.0f;
		
		// figure out time taken if we shoot straight at target point
		float t = 1.0f / (destPos-from).magnitude;
	
		// see where the target would end up if it moved for that much time without changing speed/dir
		Vector3 guessPos = destPos + (destVel * t);
		
		// aim at that point instead
		return (guessPos-from).normalized;
	}
	
	// version that just gets the guessed position (for use with MovementControl.MoveTowardsPoint)
	public static Vector3 GetInterceptPositionXY(Vector3 from, float speed, Vector3 destPos, Vector3 destVel)
	{
		from.z = 0.0f;
		destPos.z = 0.0f;
		destVel.z = 0.0f;
		float t = 1.0f / (destPos-from).magnitude;
		return destPos + (destVel*t);
	}
	
	// Function to calculate the angular velocity required to blend between two rotations.
	// rotSpeed is in degrees per second, dampingRange is in degrees.
	//
	// Use this instead of Quaternion.RotateTowards in cases where you want to blend rotation but avoid writing directly to the
	// transform.rotation.  On things that use rigidbody and colliders (i.e. physics objects), writing to transform.rotation is
	// slow and also error-prone (you could rotate the object into solid collision), it is faster and less glitchy to use
	// this function and manipulate the angular velocity instead.
	public static Vector3 GetAngularVelocityForRotationBlend(Quaternion from, Quaternion to, float rotSpeed=500.0f, float dampingRange=20.0f)
	{
		Quaternion qDelta = to * Quaternion.Inverse(from);
		Vector3 deltaAxis;
		float deltaAng;
		qDelta.ToAngleAxis(out deltaAng, out deltaAxis);			// this gives us the axis to rotate around, and the current angle difference in DEGREES
		float signedDeltaAng = FixAnglePlusMinusDegrees(deltaAng);
		deltaAng = Mathf.Abs(signedDeltaAng);
		
		if ((dampingRange > 0.0f) && (deltaAng < dampingRange))
			rotSpeed *= (deltaAng/dampingRange);
			
		if(deltaAng == 0.0f)
			return Vector3.zero;
		rotSpeed *= Mathf.Deg2Rad;									// angular velocity is in RADIANS per second
		return deltaAxis * ((signedDeltaAng < 0.0f) ? -rotSpeed : rotSpeed);
	}
	
	// Thing to get a curve between 3 points.  todo: improve to work with arbitrary array of points, or whatever.
	public static Vector3 GetSimpleSplinePoint(Vector3 p0, Vector3 p1, Vector3 p2, float t)
	{
		Vector3 pc = p1*2.0f - (p0+p2)*0.5f;	// get control point to use instead of p1, so curve will pass through p1 instead of just getting dragged part way towards it
		Vector3 lp0 = Vector3.Lerp(p0, pc, t);	// get lerped point between start and control point
		Vector3 lp1 = Vector3.Lerp(pc, p2, t);	// get lerped point between control point and end
		return Vector3.Lerp(lp0, lp1, t);		// get final lerped point between those two
	}
	
	// Functions to rotate a 2D vector around the Z axis (or 3D vector ignoring Z)
	public static Vector2 RotateRadians(this Vector2 v, float ang)
	{
		float sin = Mathf.Sin(ang);
		float cos = Mathf.Cos(ang);
		float x = v.x;
		float y = v.y;
		return new Vector2(x*cos - y*sin, x*sin + y*cos);
	}
	
	public static Vector2 RotateDegrees(this Vector2 v, float ang)
	{
		ang *= Mathf.Deg2Rad;
		float sin = Mathf.Sin(ang);
		float cos = Mathf.Cos(ang);
		float x = v.x;
		float y = v.y;
		return new Vector2(x*cos - y*sin, x*sin + y*cos);
	}
	
	public static Vector3 RotateXYRadians(this Vector3 v, float ang)
	{
		float sin = Mathf.Sin(ang);
		float cos = Mathf.Cos(ang);
		float x = v.x;
		float y = v.y;
		return new Vector3(x*cos - y*sin, x*sin + y*cos);
	}
	
	public static Vector3 RotateXYDegrees(this Vector3 v, float ang)
	{
		ang *= Mathf.Deg2Rad;
		float sin = Mathf.Sin(ang);
		float cos = Mathf.Cos(ang);
		float x = v.x;
		float y = v.y;
		return new Vector3(x*cos - y*sin, x*sin + y*cos);
	}
	
	
	//----------------------------------------------------------------------------
	// Unity GameObject/Component helper functions
	//----------------------------------------------------------------------------
	
	// FindSubObject is a recursive version of Transform.Find(), it will find a child
	// object by name, returning the first one encountered (searches depth first, does
	// not necessarily find closest one to the root, which maybe we want?)
	
	public static Transform FindSubObjectRecursive(this Transform trans, string name)
	{
		if(trans.name==name)
			return trans;
			
		foreach(Transform t in trans)
		{
			Transform found  = t.FindSubObjectRecursive(name);
			if(found != null)
				return found;
		}
		return null;
	}

    public static T FindSubObject<T>(this GameObject obj, string name) where T : Component
    {
        GameObject go = obj.FindSubObject(name);
        if(go != null )
        {
            return go.GetComponent<T>();
        }
        else
        {
            return default(T);
        }
    }

	public static GameObject FindSubObject(this Component comp, string name)			// on component, return object
	{
		Transform t = comp.transform.FindSubObjectRecursive(name);
		return (t==null) ? null : t.gameObject;
	}
	public static GameObject FindSubObject(this GameObject obj, string name)			// on object, return object
	{
        if( obj && obj.transform )
        {
            Transform t = obj.transform.FindSubObjectRecursive(name);
            return (t == null) ? null : t.gameObject;
        }
		else
        {
            return null;
        }
	}
	public static Transform FindSubObjectTransform(this Component comp, string name)	// on component, return transform
	{
		return comp.transform.FindSubObjectRecursive(name);
	}
	public static Transform FindSubObjectTransform(this GameObject obj, string name)	// on object, return transform
	{
		return obj.transform.FindSubObjectRecursive(name);
	}
	public static Transform[] GetImmediateChildren(this Transform trans)
	{
		List<Transform> immediateChildrenList = new List<Transform>();
		foreach(Transform t in trans)
		{
			if(t.parent == trans)
			{
				immediateChildrenList.Add(t);
			}
		}
		return immediateChildrenList.ToArray();
	}
	
	// GetFirstComponentInChildren is a version of GetComponentInChildren that finds the one closest
	// to the root of the hierarchy, instead the deepest one
	
	public static T GetFirstComponentInChildren<T>(this Component comp) where T : Component
	{
		Transform trans = comp.transform;
	
		// first see if it's on the root component
		T t = comp.GetComponent<T>();
		if(t != null)
			return t;
		
		// next see if it can be found at the first child level
		foreach(Transform tr in trans)
		{
			t = tr.GetComponent<T>();
			if(t != null)
				return t;
		}
	
		// haven't found it, now we'll search the entire hierarchy
		// and if we find any, find the least deep one.
		//
		// The first 2 checks are not necessary, just an optimization
		// to stop us trawling through the entire hierarchy every time
		// because this is a bit crap.
		T[] ts = comp.GetComponentsInChildren<T>();
		if((ts == null) || (ts.Length==0))
			return null;
			
		// found at least one, now search for the one nearest the root
		T bestOne = null;
		int bestDepth = 9999;
		foreach(T test in ts)
		{
			Transform tr = test.transform;
			int depth = 0;
			while(tr != trans)				// count how many parents to step through before we get back to the root
			{
				depth++;
				tr = tr.parent;
			}
			if(depth < bestDepth)
			{
				bestDepth = depth;
				bestOne = test;
			}
		}	
		return bestOne;
	}
	
	public static T GetFirstComponentInChildren<T>(this GameObject obj) where T : Component
	{
		return obj.transform.GetFirstComponentInChildren<T>();
	}
	
	// extension method Transform.GetRootPrefab()
	// Works like Transform.root but returns the first Transform that is on an object with a PrefabInstance component.
	// Can use this on things like a collider on a child object of a human that is parented to a boat, and we want to
	// find the human, not the boat.
	public static Transform GetRootPrefab(this Transform t)
	{
        
        // Reverting this as the original code would actually return the lowest transform if it didn't find a PrefabInstance
        //PrefabInstance parentPrefabInstance = t.GetComponentInParent<PrefabInstance>();
        //if (parentPrefabInstance)
        //{
        //    return parentPrefabInstance.transform;
        //}
        //else
        //{
        //    return null;
        //}

        while(true)
        {
            if(t.GetComponent<PrefabInstance>() != null)
                return t;
            Transform parent = t.parent;
            if(parent == null)
                return t;
            t = parent;
        }
	}
	
	// get a list of renderers that includes mesh and skinned mesh renderers but no other types (like particle renderers).
	// We use this to get models to be faded out, or have swallow shader applied etc., without messing up on attached particle emitters
	public static Renderer[] GetModelRenderers(this GameObject obj)
	{
		MeshRenderer[] mr = obj.GetComponentsInChildren<MeshRenderer>();
		SkinnedMeshRenderer[] smr = obj.GetComponentsInChildren<SkinnedMeshRenderer>();
		
		if((mr != null) && (mr.Length == 0))
			mr = null;
		if((smr != null) && (smr.Length == 0))
			smr = null;
		
		// if we have only mesh or skinnedmesh renderers, return a single array
		if(mr == null)
			return smr;
		else if(smr==null)
			return mr;
		
		// if we have both types, build an array containing both lists.  Maybe there is a built in way to concatenate arrays?
		int len = mr.Length + smr.Length;
		Renderer[] r = new Renderer[len];
		int idx = 0;
		if(mr != null)
		{
			foreach(Renderer t in mr)
				r[idx++] = t;
		}
		if(smr != null)
		{
			foreach(Renderer t in smr)
				r[idx++] = t;
		}
		return r;
	}
	
	#if UNITY_STANDALONE && FGOL_DESKTOP
	public static void SetModelCastShadows(this GameObject obj, Renderer[] rs=null)
	{
		if(rs==null)
			rs = obj.GetModelRenderers();
		foreach(Renderer r in rs)
		{
			r.castShadows = true;
			r.receiveShadows = false;
		}
	}
	
	public static void SetModelReceiveShadows(this GameObject obj, Renderer[] rs=null)
	{
		if(rs==null)
			rs = obj.GetModelRenderers();
		foreach(Renderer r in rs)
		{
			r.castShadows = false;
			r.receiveShadows = true;
		}
	}
	
	public static void SetModelIgnoreShadows(this GameObject obj, Renderer[] rs=null)
	{
		if(rs==null)
			rs = obj.GetModelRenderers();
		foreach(Renderer r in rs)
		{
			r.castShadows = false;
			r.receiveShadows = false;
		}
	}
	#else
	public static void SetModelCastShadows(this GameObject obj, Renderer[] rs=null) 	{}
	public static void SetModelReceiveShadows(this GameObject obj, Renderer[] rs=null)	{}
	public static void SetModelIgnoreShadows(this GameObject obj, Renderer[] rs=null)	{}
	#endif
	
	// extension methods for Transform, for setting individual components of position etc
	public static float SetPosX(this Transform t, float x)
	{
		Vector3 v = t.position;
		v.x = x;
		t.position = v;
		return x;	// return the value we set, for convenience
	}
	public static float SetPosY(this Transform t, float y)
	{
		Vector3 v = t.position;
		v.y = y;
		t.position = v;
		return y;
	}
	public static float SetPosZ(this Transform t, float z)
	{
		Vector3 v = t.position;
		v.z = z;
		t.position = v;
		return z;
	}
	public static float SetLocalScale(this Transform t, float s)
	{
		t.localScale = new Vector3(s, s, s);
		return s;
	}
	public static float GetGlobalScaleQuick(this Transform t)	// gets the global scale of a transform by multiplying the X component of this and all parents.
	{															// So only works on things with uniform scale (x/y/z the same) all the way up the hierarchy
		float s = 1.0f;
		while(t != null)
		{
			s *= t.localScale.x;
			t = t.parent;
		}
		return s;
	}
	
	public static void CopyFrom(this Transform t, Transform from, bool includeScale=true)
	{
		t.position = from.position;
		t.rotation = from.rotation;
		if(includeScale)
			t.localScale = from.localScale;
	}
	
	// extension methods for getting a single roll angle from direction vectors (Vector2 and XY components of Vector3)
	public static float ToAngleDegrees(this Vector2 v)
	{
		return Mathf.Atan2(v.y, v.x)*Mathf.Rad2Deg;
	}
	public static float ToAngleRadians(this Vector2 v)
	{
		return Mathf.Atan2(v.y, v.x);
	}
	public static float ToAngleDegreesXY(this Vector3 v)
	{
		return Mathf.Atan2(v.y, v.x)*Mathf.Rad2Deg;
	}
	public static float ToAngleRadiansXY(this Vector3 v)
	{
		return Mathf.Atan2(v.y, v.x);
	}
	
	// Returns a normalized vector that has had its z cleared, so result is guaranteed to be in the XY plane.
	// Does not handle zero length, so if x & y components are zero, it will return a zero vector.
	public static Vector3 NormalizedXY(this Vector3 v)
	{
		Vector3 n = v;
		n.z = 0.0f;
		return n.normalized;
	}
	
	// int to hex string - a simple extension method of int, rather than typing the gibberish string.Format stuff.
	public static string ToHexString(this int n)
	{
		return string.Format("{0:X}", n);
	}

	// Methods for float, for adding or subtracting a value, then clamping to some limit, and returning a bool to say whether it was clamped.
	// Doesn't appear to be possible to do with extension methods.
	// Use for very common situations of updating timers etc., e.g. for some thing that counts down to zero and then does something:
	// if(Util.SubClamp(ref timer, dt))
	//		...
	public static bool SubClamp(ref float f, float subVal, float clampVal=0.0f)
	{
		f -= subVal;
		if(f <= clampVal)
		{
			f = clampVal;
			return true;
		}
		return false;
	}
	
	public static bool AddClamp(ref float f, float addVal, float clampVal=1.0f)
	{
		f += addVal;
		if(f >= clampVal)
		{
			f = clampVal;
			return true;
		}
		return false;
	}

	// convert stats for gameplay accessories from percentage increase, to value to scale by (e.g. for something to increase by +5%, convert stat from 5 to 1.05)
	public static float ScaleFromPerc(this float f)
	{
		return 1.0f + (f*0.01f);
	}
	public static float ScaleFromPercClamped(this float f)
	{
		return 1.0f + Mathf.Clamp(f*0.01f, -1.0f, 1.0f);
	}
	
    //----------------------------------------------------------------------------
    // Helpers for serialization / parsing
    //----------------------------------------------------------------------------

    // If the named field exists, overwrite the output value, otherwise leave it unchanged
    public static void OverrideInt(Dictionary<string, string> dic, string stat, ref int i)
	{
		if(dic.ContainsKey(stat))
			int.TryParse(dic[stat], out i);
	}
	
	public static void OverrideFloat(Dictionary<string, string> dic, string stat, ref float f)
	{
		if(dic.ContainsKey(stat))
			float.TryParse(dic[stat], out f);
	}

	public static void OverrideBool(Dictionary<string, string> dic, string stat, ref bool b)
	{
		if(dic.ContainsKey(stat))
			bool.TryParse(dic[stat], out b);
	}
	
	// extension method of string to remove "(Clone)" from the end if it is present, otherwise returns an unchanged string.
	public static string RemoveCloneSuffix(this string s)
	{
		if(s.EndsWith("(Clone)"))
			return s.Substring(0, s.Length-7);
		return s;
	}

	public static void SetLayerRecursively(GameObject obj, string newLayer)
	{
		Util.SetLayerRecursively(obj, LayerMask.NameToLayer(newLayer));
	}
	
	public static void SetLayerRecursively(GameObject obj, int newLayer)
	{
		if (null == obj)
		{
			return;
		}        
		
		obj.layer = newLayer;       
		
		foreach (Transform child in obj.transform)
		{
			if (null == child)
			{
				continue;
			}
			
			SetLayerRecursively(child.gameObject, newLayer);
		}
	}

    #if UNITY_EDITOR

    public static void CopyTextToClipBoard(string text)
    {
        TextEditor te = new TextEditor();
        te.content = new GUIContent(text);
        te.SelectAll();
        te.Copy();
    }

    #endif

    

    /// <summary>
    /// Perform a deep Copy of the object.
    /// </summary>
    /// <typeparam name="T">The type of object being copied.</typeparam>
    /// <param name="source">The object instance to copy.</param>
    /// <returns>The copied object.</returns>
    public static T Clone<T>( this T source)
    {
        if (!typeof(T).IsSerializable)
        {
            throw new System.ArgumentException("The type must be serializable.", "source");
        }

        // Don't serialize a null object, simply return the default for that object
        if (object.ReferenceEquals(source, null))
        {
            return default(T);
        }

        IFormatter formatter = new BinaryFormatter();
        Stream stream = new MemoryStream();
        using (stream)
        {
            formatter.Serialize(stream, source);
            stream.Seek(0, SeekOrigin.Begin);
            return (T)formatter.Deserialize(stream);
        }
    }

    // public static string TimespanToFriendlyString( System.TimeSpan timeSpan, string formatString, string lessThanAMinuteString )
    // {
    //     string epochName;
    //     int epochQuantity;
    //
    //     // Round the hours so that when you come straight back through the portal it says 'wait 24 hours'
    //     int roundedHours = Mathf.RoundToInt((float)timeSpan.TotalHours);
    //
    //     // Determine the epoch name i.e. minutes/hours and how many epochs the player must wait
    //     if (roundedHours > 1)
    //     {
    //         epochQuantity = roundedHours;
    //         epochName = Localization.instance.Get("IG_EPOCH_HOURS");
    //     }
    //     else if (roundedHours == 1)
    //     {
    //         epochQuantity = roundedHours;
    //         epochName = Localization.instance.Get("IG_EPOCH_HOUR");
    //     }
    //     else if (timeSpan.Minutes > 1)
    //     {
    //         epochQuantity = timeSpan.Minutes;
    //         epochName = Localization.instance.Get("IG_EPOCH_MINUTES");
    //     }
    //     else if (timeSpan.Minutes == 1)
    //     {
    //         epochQuantity = timeSpan.Minutes;
    //         epochName = Localization.instance.Get("IG_EPOCH_MINUTE");
    //     }
    //     else
    //     {
    //         epochQuantity = 0;
    //         epochName = "";
    //     }
    //
    //     // Format the message
    //     if (epochQuantity == 0)
    //     {
    //         return lessThanAMinuteString;
    //     }
    //     else
    //     {
    //         return string.Format(formatString, epochQuantity, epochName);
    //     }
    // }


//     public static void LoadTextureFromURL(string url, System.Action<bool, Texture2D> callback)
//     {
// #if UNITY_WEBGL
// 		GameObject go = new GameObject ();
// 		go.AddComponent<WebGLImageLoader> ();
// 		go.GetComponent<WebGLImageLoader> ().LoadImageFromUrl (url, callback);
// #else
//         HTTPRequest req = new HTTPRequest(new System.Uri(url), HTTPMethods.Get, delegate(HTTPRequest request, HTTPResponse response)
//         {
//             if (response != null && response.IsSuccess)
//             {
//                 Texture2D texture = new Texture2D(2,2,TextureFormat.RGB565,false);
//                 texture.LoadImage(response.Data);
//                 callback(true,texture);
//             }
//             else
//             {
//                 callback(false,null);
//             }
//         });
//
//         req.Send();
// #endif
//     }

    public static Color HexToColor(string hex)
    {
        return new Color32(
            byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber),
            byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber),
            byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber),
            255);
    }

    public static Color HexToColor(string hex, byte alpha)
    {
        Color32 c = HexToColor(hex);
        c.a = alpha;
        return c;
    }

    public static long EstimateScreenBufferMemoryUsage( int aaLevel)
    {
		// If aa is off then set it to 1 as that's what the multiplier is!
        if (aaLevel == 0)
        {
            aaLevel = 1;
        }

        // Screen.width * Screen.height * ( 3 (rgb) + 3 (depth)) * (anti-aliasing level ^ 2).
        return Screen.width*Screen.height*(3 + 3)* aaLevel * aaLevel;
    }

	// -------------------------------------------------------------- //


	public static int GetUnixTimestamp()
	{
		return (int)(System.DateTime.UtcNow.Subtract(new System.DateTime(1970, 1, 1))).TotalSeconds;
	}

	public static int GetUnixTimestamp(System.DateTime dateTime)
	{
		return (int)(dateTime.Subtract(new System.DateTime(1970, 1, 1))).TotalSeconds;
	}

	// -------------------------------------------------------------- //

	public static float ExtractPriceFromString(string dirtyPrice)
	{
		try
		{
			int priceStartIndex = 0;
			int priceEndIndex = 0;

			// Take only the digits and seperators from price string
			for(int i = 0; i < dirtyPrice.Length; i++)
			{
				// Check if within digit ASCII range 
				if(dirtyPrice[i] - 48 >= 0 && dirtyPrice[i] - 48 <= 9)
				{
					priceStartIndex = i;
					break;
				}
			}

			for(int i = dirtyPrice.Length - 1; i >= 0; i--)
			{
				// Check if within digit ASCII range 
				if(dirtyPrice[i] - 48 >= 0 && dirtyPrice[i] - 48 <= 9)
				{
					priceEndIndex = i;
					break;
				}
			}

			string validStringPrice = dirtyPrice.Substring(priceStartIndex, (priceEndIndex + 1) - priceStartIndex);

			//  Check for spaces (HSX-4109, parse issue when spaces present, char code 160, india, regular " " space checking won't work)
			if (validStringPrice.IndexOf((char)160) != -1)
			{
				string emp = "" + ((char)160);
				validStringPrice = validStringPrice.Replace(emp, "");
			}

			//	We first try current culture parse, if that fails we try invariant one
			//	if that fails we try swaping manually decimal and thousands symbol
			//	Could be overkill, but since hard to test on different stores this way we support multiple cultures
			float parsed = 0;
			bool success = float.TryParse(validStringPrice, NumberStyles.Currency, CultureInfo.CurrentCulture, out parsed);
			if (!success)
			{
				success = float.TryParse(validStringPrice, NumberStyles.Currency, CultureInfo.InvariantCulture, out parsed);
				if (!success)
				{
					//	Manually swap decimal and thousands
					CultureInfo info = CultureInfo.InvariantCulture.Clone() as CultureInfo;
					string dec = info.NumberFormat.NumberDecimalSeparator;
					string tho = info.NumberFormat.NumberGroupSeparator;

					info.NumberFormat.NumberDecimalSeparator = tho;
					info.NumberFormat.NumberGroupSeparator = dec;

					success = float.TryParse(validStringPrice, NumberStyles.Currency, info, out parsed);
				}
			}

			return parsed;
		}
		catch(System.Exception e)
		{
			string err = e.Message;
			Debug.LogError("Unable to parse price: " + dirtyPrice + " " + err);
			return 0f;
		}
	}

}
}