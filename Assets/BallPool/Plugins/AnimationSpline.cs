using UnityEngine;
using System.Collections;

public class AnimationSpline
{
	public AnimationCurve _curveX;
	public AnimationCurve _curveY;
	public AnimationCurve _curveZ;
	
	private float _splineLength = 0.0f;
	private float _parabolaDistance = 0.0f;
		
	public AnimationCurve curveX
	{
		get {return this._curveX;}
	}
	public AnimationCurve curveY
	{
		get {return this._curveY;}
	}
	public AnimationCurve curveZ
	{
		get {return this._curveZ;}
	}
	
	private SplineKeyframe[] _keys;

	public AnimationSpline (WrapMode wrapMode)
	{
		CreateSpline (wrapMode);
	}
	public AnimationSpline (WrapMode wrapMode, SplineKeyframe[] keys)
	{
		CreateSpline (wrapMode);
		SetKeys(keys);
	}
	private void CreateSpline (WrapMode wrapMode)
	{
		_curveX = new AnimationCurve();
		_curveY = new AnimationCurve();
		_curveZ = new AnimationCurve();
		
		SetWrapMode(wrapMode);
	}
	public void SetWrapMode(WrapMode wrapMode)
	{
		_curveX.preWrapMode = wrapMode;
		_curveX.postWrapMode = wrapMode;
		
		_curveY.preWrapMode = wrapMode;
		_curveY.postWrapMode = wrapMode;
		
		_curveZ.preWrapMode = wrapMode;
		_curveZ.postWrapMode = wrapMode;
	}
	public void AddKey (SplineKeyframe valueKeyframe)
	{
		_curveX.AddKey(valueKeyframe.time, valueKeyframe.valueX);
		_curveY.AddKey(valueKeyframe.time, valueKeyframe.valueY);
		_curveZ.AddKey(valueKeyframe.time, valueKeyframe.valueZ);
	}
	public void AddKey (float time, float valueX, float valueY, float valueZ)
	{
		_curveX.AddKey(time, valueX);
		_curveY.AddKey(time, valueY);
		_curveZ.AddKey(time, valueZ);
	}
	public void AddKey (float time, Vector3 valueKeyframe)
	{
		_curveX.AddKey(time, valueKeyframe.x);
		_curveY.AddKey(time, valueKeyframe.y);
		_curveZ.AddKey(time, valueKeyframe.z);
	}
	public void SmoothTangents (int index, float weight)
	{
		_curveX.SmoothTangents(index, weight);
		_curveY.SmoothTangents(index, weight);
		_curveZ.SmoothTangents(index, weight);
	}
	public void Smooth ()
	{
		for(int i = 0; i < this.length; i ++)
		this.SmoothTangents(i,0.0f);
	}
	public Vector3 GetTangentsVector (float time, float kaeficient)
	{
		Vector3 pos0;
		Vector3 pos;
		float delta = kaeficient;
		pos0 = Evaluate(time);
		pos = Evaluate(time + delta); 
		return (1.0f/delta)*(pos - pos0);
	}
	public Vector3 GetTangentsVector (float time, Vector3 position, float kaeficient)
	{
		Vector3 pos0;
		Vector3 pos;
		float delta = kaeficient;
		pos0 = position;
		if(time < _splineLength - kaeficient)
		{
			pos = Evaluate(time + delta); 
			return (1.0f/delta)*(pos - pos0);
		}
		else
		{
			pos = Evaluate(time - delta); 
			return (-1.0f/delta)*(pos - pos0);
		}
	}
	public Vector3 GetNormalVector(float time, float kaeficient)
	{
		Vector3 norm0;
		Vector3 norm;
		float delta = kaeficient;
		norm0 = GetTangentsVector(time, delta).normalized;
		norm = GetTangentsVector(time + delta, delta).normalized; 
		return ((1.0f/delta)*(norm - norm0)).normalized;
	}
	public Vector3 GetNormalVector(float time, Vector3 position, float kaeficient)
	{
		Vector3 norm0;
		Vector3 norm;
		float delta = kaeficient;
		norm0 = GetTangentsVector(time,position, delta).normalized;
		norm = GetTangentsVector(time + delta,position, delta).normalized; 
		return ((1.0f/delta)*(norm - norm0)).normalized;
	}
	public Vector3 GetNormalAcceleration(float time, Vector3 position, float kaeficient)
	{
		Vector3 norm0;
		Vector3 norm;
		float delta = kaeficient;
		norm0 = GetTangentsVector(time,position, delta).normalized;
		norm = GetTangentsVector(time + delta,position, delta).normalized; 
		return (1.0f/delta)*(norm - norm0);
	}
	public void RemoveKey (int index)
	{
		_curveX.RemoveKey(index);
		_curveY.RemoveKey(index);
		_curveZ.RemoveKey(index);
	}
	public SplineKeyframe[] keys
	{
		get {return this._keys;}
		set 
		{
			SetKeys(value);
		}
	}
	private void SetKeys (SplineKeyframe[] valueKeyframe )
	{
		this._keys = valueKeyframe;
		int _length = length;
		Keyframe[] KeyframeX = new Keyframe[_length];
		Keyframe[] KeyframeY = new Keyframe[_length];
		Keyframe[] KeyframeZ = new Keyframe[_length];
		for(int i = 0; i < _length; i++)
		{
			KeyframeX[i] = new Keyframe(this._keys[i].time, this._keys[i].valueX, this._keys[i].inTangent, this._keys[i].outTangent);
			KeyframeX[i].tangentMode = this._keys[i].tangentMode;
			KeyframeY[i] = new Keyframe(this._keys[i].time, this._keys[i].valueY, this._keys[i].inTangent, this._keys[i].outTangent);
			KeyframeY[i].tangentMode = this._keys[i].tangentMode;
			KeyframeZ[i] = new Keyframe(this._keys[i].time, this._keys[i].valueZ, this._keys[i].inTangent, this._keys[i].outTangent);
			KeyframeZ[i].tangentMode = this._keys[i].tangentMode;
		}
		_curveX.keys = KeyframeX;
		_curveY.keys = KeyframeY;
		_curveZ.keys = KeyframeZ;
	}
	public int length
	{
		get {return this._keys.Length;}
	}
	public float splineLength
	{
		get {return this._splineLength;}
	}
	public float parabolaDistance
	{
		get {return this._parabolaDistance;}
	}
	public Vector3 Evaluate (float time)
	{
		return new Vector3(this._curveX.Evaluate(time), this._curveY.Evaluate(time), this._curveZ.Evaluate(time));
	}
	public float EvaluateX (float time)
	{
		return this._curveX.Evaluate(time);
	}
	public float EvaluateY (float time)
	{
		return this._curveY.Evaluate(time);
	}
	public float EvaluateZ (float time)
	{
		return this._curveZ.Evaluate(time);
	}
	
	public void CreateSpline (Transform[] nodes, bool smooth, bool close, bool drawLine, float drawDistance)
	{
		int nodesLength = close? nodes.Length + 1: nodes.Length;
		float curveLength = 0.0f;
		SplineKeyframe[] chainKeys;
		
		if(this.keys == null || this.keys.Length != nodesLength)
		chainKeys = new SplineKeyframe[nodesLength];
		else 
		chainKeys = this.keys;
		
		if(nodesLength > 0)
		chainKeys[0] = new SplineKeyframe(0, nodes[0].position);
		
		
		
		for(int i = 1; i < nodesLength - 1; i++)
		{
			curveLength += Vector3.Distance(nodes[i].position, nodes[i - 1].position);
			chainKeys[i] = new SplineKeyframe(curveLength, nodes[i].position);
		}
		
		if(close && nodesLength > 2)
		{
		curveLength += Vector3.Distance(nodes[0].position, nodes[nodesLength - 2].position);
		chainKeys[nodesLength - 1] = new SplineKeyframe(curveLength, nodes[0].position);
		}
		else
		{
		curveLength += Vector3.Distance(nodes[nodesLength - 2].position, nodes[nodesLength - 1].position);
		chainKeys[nodesLength - 1] = new SplineKeyframe(curveLength, nodes[nodesLength - 1].position);
		}
		this.keys = chainKeys;

		if(smooth)
		this.Smooth();
		
		if(drawLine)
		{
			/*for (float d = 0; d < curveLength; d+= drawDistance) 
			{
				Debug.DrawLine(this.Evaluate(d), this.Evaluate(d + drawDistance), Color.red);
			}*/
		}
		_splineLength = curveLength;
	}
	
	public void AnimationSlider (Transform slider, float speed, ref float step, out Vector3 velocity, int sliderOrient, bool orientToCurve)
	{
		Vector3 newPosition = this.Evaluate(step + speed*Time.deltaTime);
		slider.position = this.Evaluate(step);
		step += sliderOrient*speed*Time.deltaTime;
		if(step + speed*Time.deltaTime >= splineLength)
			velocity = (1.0f/Time.deltaTime)*(slider.position - this.Evaluate(step - speed*Time.deltaTime));
		else
			velocity = (1.0f/Time.deltaTime)*(newPosition - slider.position);
		if(orientToCurve && velocity != Vector3.zero)
		{
			slider.LookAt(slider.position + velocity.normalized);
		}


	}
}

public class SplineKeyframe 
{
	private float _time;
	private float _valueX;
	private float _valueY;
	private float _valueZ;
	private Vector3 _valueVector;
	private float _inTangent;
	private float _outTangent;
	private  int _tangentMode;
	public float time 
	{
		get {return this._time;}
		set {this._time = value;}
	}
	public float valueX 
	{
		get {return this._valueX;}
		set {this._valueX = value;}
	}
	public float valueY 
	{
		get {return this._valueY;}
		set {this._valueY = value;}
	}
	public float valueZ 
	{
		get {return this._valueZ;}
		set {this._valueZ = value;}
	}
	public Vector3 valueVector
	{
		get {return this._valueVector;}
		set {this._valueVector = value;}
	}
	public float inTangent
	{
		get {return this._inTangent;}
		set {this._inTangent = value;}
	}
	public float outTangent
	{
		get {return this._outTangent;}
		set {this._outTangent = value;}
	}
	public int tangentMode
	{
		get {return this._tangentMode;}
		set {this._tangentMode = value;}
	}
    public SplineKeyframe (float time, float valueX, float valueY, float valueZ)
	{
		this._time = time;
		this._valueX = valueX;
		this._valueY = valueY;
		this._valueZ = valueZ;
		this._valueVector = new Vector3( this._valueX, this._valueY, this._valueZ);
		this._inTangent = 0;
		this._outTangent = 0;
	}
	public SplineKeyframe (float time, float valueX, float valueY, float valueZ, float inTangent, float outTangent)
	{
		this._time = time;
		this._valueX = valueX;
		this._valueY = valueY;
		this._valueZ = valueZ;
		this._valueVector = new Vector3( this._valueX, this._valueY, this._valueZ);
		this._inTangent = inTangent;
		this._outTangent = outTangent;
	}
	public SplineKeyframe (float time, Vector3 valueVector)
	{
		this._time = time;
		this._valueVector = valueVector;
		this._valueX = valueVector.x;
		this._valueY = valueVector.y;
		this._valueZ = valueVector.z;
		this._inTangent = 0;
		this._outTangent = 0;
	}
	public SplineKeyframe (float time, Vector3 valueVector, float inTangent, float outTangent)
	{
		this._time = time;
		this._valueVector = valueVector;
		this._valueX = valueVector.x;
		this._valueY = valueVector.y;
		this._valueZ = valueVector.z;
		this._inTangent = inTangent;
		this._outTangent = outTangent;
	}
}

