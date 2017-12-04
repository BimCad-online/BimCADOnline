using UnityEngine;
using System.Collections.Generic;
    
//------------------------------------------------------------------------------
//
// class GeomUtils
//
//------------------------------------------------------------------------------

public static class GeomUtils {

	public static float GetAngle(Vector3 d0, Vector3 d1) {
		var du = d1.x * d0.x + d1.y * d0.y;
		var dv = d0.x * d1.y - d0.y * d1.x;
		return Mathf.Atan2(dv, du);
	}
		
	public static Vector3 projectToPlane(Vector3 pos, Vector3 normal, Vector3 point) {
		Plane p = new Plane(normal, point);
		return pos - p.GetDistanceToPoint(pos) * normal;
	}
	
	public enum Classify {
		etLEFT,
		etRIGHT,
		etORIGIN,
		etDESTINATION,
		etBEHIND,
		etBEYOND,
		etBETWEEN,
		etTOUCHA,
		etTOUCHB,
		etOUTSIDE,
	}
	
	public enum Cross {
		NONE,
		TOUCH,
		INTERSECTION,
		COLLINEAR,
	}

	public static Classify classify(Vector3 P, Vector3 A, Vector3 B, float EPS) {
		
		Vector3 l = B - A;
	    l.z = 0;
		l = l.normalized;
		
		Plane plane = new Plane(Vector3.Cross(l, Vector3.forward).normalized, A);
		float ro = plane.GetDistanceToPoint(P);

		if ((P - A).sqrMagnitude < EPS) return Classify.etTOUCHA;
		if ((P - B).sqrMagnitude < EPS) return Classify.etTOUCHB;

		if (ro < -EPS) return Classify.etLEFT;
		if (ro >  EPS)  return Classify.etRIGHT;

		plane = new Plane(l, P);
	
		float roA = plane.GetDistanceToPoint(A);
		float roB = plane.GetDistanceToPoint(B);
	
		if (roA > 0.0f && roB > 0.0f) return Classify.etBEYOND;
		if (roA < 0.0f && roB < 0.0f) return Classify.etBEHIND;
		
		
		return Classify.etBETWEEN;	
	}
	
	
	static bool isSegmentOutside(Classify a, Classify b) {
		return
			a == Classify.etLEFT && b == Classify.etLEFT || 
			a == Classify.etRIGHT && b == Classify.etRIGHT ||
			a == Classify.etBEHIND && b == Classify.etBEHIND ||
			a == Classify.etBEYOND && b == Classify.etBEYOND;
				
	}
	
	static bool isPointTouch(Classify a) {
		return a == Classify.etTOUCHA || a == Classify.etTOUCHB || a == Classify.etBETWEEN;
	}
	
	static bool isPointOnLine(Classify a) {
		return a == Classify.etTOUCHA || a == Classify.etTOUCHB || a == Classify.etBETWEEN || a == Classify.etBEHIND || a == Classify.etBEYOND;
	}
	
	public static Cross classifyCollinearSegmentCross(Classify a, Classify b) {

		if (a == Classify.etTOUCHA && b == Classify.etBEYOND) return Cross.TOUCH;
		if (a == Classify.etTOUCHB && b == Classify.etBEHIND) return Cross.TOUCH;
		if (b == Classify.etTOUCHA && a == Classify.etBEYOND) return Cross.TOUCH;
		if (b == Classify.etTOUCHB && a == Classify.etBEHIND) return Cross.TOUCH;
		if (a == Classify.etTOUCHA && b == Classify.etTOUCHA) return Cross.TOUCH;
		if (a == Classify.etTOUCHB && b == Classify.etTOUCHB) return Cross.TOUCH;

		
		if (isPointOnLine(a)) {
			if (isPointOnLine(b)) return Cross.INTERSECTION;
			return Cross.TOUCH;
		}

		if (isPointOnLine(b)) {
			if (a == Classify.etBEYOND) return Cross.TOUCH;
			if (isPointOnLine(a)) return Cross.INTERSECTION;
			return Cross.TOUCH;
		}

		return Cross.NONE;
	}
	
	public static Cross isSegmentsCrossed(Vector3 A1, Vector3 B1, Vector3 A2, Vector3 B2, ref Vector3 itr, float USEEPS) {
		Vector3 L1 = B1 - A1;
		Vector3 L2 = B2 - A2;
	
		Classify A1C2 = classify(A1, A2, B2, USEEPS);
		Classify B1C2 = classify(B1, A2, B2, USEEPS);
	
		Classify A2C1 = classify(A2, A1, B1, USEEPS);
		Classify B2C1 = classify(B2, A1, B1, USEEPS);
		
		if (isSegmentOutside(A1C2, B1C2)) return Cross.NONE;
		if (isSegmentOutside(A2C1, B2C1)) return Cross.NONE;
		
		/*Cross co_cross = classifyCollinearSegmentCross(A1C2, B1C2);
		if (co_cross != Cross.NONE) return co_cross;*/

		Cross co_cross = classifyCollinearSegmentCross(A2C1, B2C1);
		if (co_cross != Cross.NONE) return co_cross;

		Vector3 N1 = Vector3.Cross(L1, Vector3.forward).normalized;
		Plane plane = new Plane(N1, A1);
		Ray ray = new Ray(A2, L2);
		float enter = 0.0f;
		
		if (plane.Raycast(ray, out enter) == false) return Cross.NONE;
		
		itr = ray.GetPoint(enter);
		
		return Cross.INTERSECTION;
	}

	public static bool isLinesCrossed(Vector3 A1, Vector3 B1, Vector3 A2, Vector3 B2, ref Vector3 itr, float USEEPS) {
		Vector3 L1 = B1 - A1;
		Vector3 L2 = B2 - A2;
	

		Vector3 N1 = Vector3.Cross(L1, Vector3.forward).normalized;
		Plane plane = new Plane(N1, A1);
		Ray ray = new Ray(A2, L2);
		float enter = 0.0f;
		
		if (plane.Raycast(ray, out enter) == false) return false;
		
		itr = ray.GetPoint(enter);
		
		return true;
	}
	
}

//------------------------------------------------------------------------------