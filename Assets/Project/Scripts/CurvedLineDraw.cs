using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CurvedLine;

public class CurvedLineDraw {
	private LineRenderer _lineRenderer;

	private CurvedLinePoint[] linePoints = new CurvedLinePoint[0];
	private Vector3[] linePositions = new Vector3[0];
	private Vector3[] linePositionsOld = new Vector3[0];

	public CurvedLineDraw(LineRenderer _line) {
		_lineRenderer = _line;
	}

	public void Draw() {
		GetPoints();
		SetPointsToLine();
	}

	public GameObject AddPoint(Vector3 position) {
		GameObject p = new GameObject ("LinePoint");
		p.AddComponent <CurvedLinePoint>();

		p.transform.parent = _lineRenderer.gameObject.transform;
		p.transform.localPosition = position;

		return p;
	}

	public void Reset() {
		GameObject[] _points = GameObjectUtils.GetChildWithNameGameObject (_lineRenderer.gameObject, "LinePoint", true);

		foreach ( GameObject _p in _points ) {
			GameObject.DestroyImmediate (_p);
		}

		_lineRenderer.SetVertexCount (0);
		_lineRenderer.SetPositions (new Vector3[]{});
	}

	void GetPoints()
	{
		//find curved points in children
		linePoints = _lineRenderer.gameObject.GetComponentsInChildren<CurvedLinePoint>();

		//add positions
		linePositions = new Vector3[linePoints.Length];
		for( int i = 0; i < linePoints.Length; i++ )
		{
			linePositions[i] = linePoints[i].transform.position;
		}
	}

	void SetPointsToLine()
	{
		//create old positions if they dont match
		if( linePositionsOld.Length != linePositions.Length )
		{
			linePositionsOld = new Vector3[linePositions.Length];
		}

		//check if line points have moved
		bool moved = false;
		for( int i = 0; i < linePositions.Length; i++ )
		{
			//compare
			if( linePositions[i] != linePositionsOld[i] )
			{
				moved = true;
			}
		}

		//update if moved
		if( moved == true )
		{
			//get smoothed values
			Vector3[] smoothedPoints = LineSmoother.SmoothLine( linePositions, 0.15f );

			//set line settings
			_lineRenderer.SetVertexCount( smoothedPoints.Length );
			_lineRenderer.SetPositions( smoothedPoints );
			_lineRenderer.SetWidth( 0.1f, 0.1f );
		}
	}
}
