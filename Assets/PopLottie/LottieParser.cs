using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

//	we need to dynamically change the structure as we parse, so the built in json parser wont cut it
//	com.unity.nuget.newtonsoft-json
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.VisualScripting;
using Object = UnityEngine.Object;


//  this is actually the bodymovin spec
namespace PopLottie
{
	//	spec is readable here
	//	https://lottiefiles.github.io/lottie-docs/breakdown/bouncy_ball/

	using FrameNumber = System.Single;	//	float

	[Serializable] public struct AssetMeta
	{
	}
	
	
	//	sometimes this is an AnimCurve (x/y graph)
	//	somtimes it's just a number? or an array of numbers 
	class ValueCurveConvertor : JsonConverter<ValueCurve>
	{
		public override void WriteJson(JsonWriter writer, ValueCurve value, JsonSerializer serializer) { throw new NotImplementedException(); }
		public override ValueCurve ReadJson(JsonReader reader, Type objectType, ValueCurve existingValue, bool hasExistingValue,JsonSerializer serializer)
		{
			if ( reader.TokenType == JsonToken.StartObject )
			{
				var ThisObject = JObject.Load(reader);
				var SingleFrame = ThisObject.ToObject<ValueCurveData>(serializer);
				existingValue.data = SingleFrame;
			}
			else if ( reader.TokenType == JsonToken.StartArray )
			{
				var ThisArray = JArray.Load(reader);
				foreach ( var Frame in ThisArray )
				{
					throw new Exception("todo handle an array of values");
				}
			}
			else 
			{
				//existingValue.ReadAnimatedOrNotAnimated(reader);
				Debug.LogWarning($"Decoding ValueCurveConvertor unhandled token type {reader.TokenType}");
			}
			return existingValue;
		}
	}
	

	[Serializable]
	public struct ValueCurveData
	{
		public float[]	x;	//	time X axis
		public float[]	y;	//	value Y axis
	}
	
	[JsonConverter(typeof(ValueCurveConvertor))]
	[Serializable]
	public struct ValueCurve
	{
		public ValueCurveData	data;
		public float[]	x => data.x;
		public float[]	y => data.y;
		
		public float	GetValue(TimeSpan NormalisedTime)
		{
			return y[0];
		}
	}
	
	[Serializable] public struct KeyframeFloats
	{
		public ValueCurve	i;
		public ValueCurve	o;
		public float		t;	//	time
		public float[]		s;	//	start value
		public float[]		e;	//	end value
	}
	
	//	https://lottiefiles.github.io/lottie-docs/playground/json_editor/
	[Serializable] public struct AnimatedVector
	{
		public int				a;
		public bool				Animated => a!=0;
		
		public Keyframed_FloatArray	k;	//	frames
		
		public float			GetValue(FrameNumber Frame,float Default)
		{
			return k.GetValue(Frame,new float[]{Default})[0];
		}
		
		public float[]			GetValue(FrameNumber Frame,float[] Default)
		{
			return k.GetValue(Frame,Default);
		}
		
		public Vector2			GetValue(FrameNumber Frame,Vector2 Default)
		{
			var Default2 = new float[]{Default.x,Default.y};
			var Values = GetValue(Frame,Default2);
			if ( Values.Length == 0 )
				return Default;
				
			//	1D scale... usually
			if ( Values.Length == 1 )
				return new Vector2(Values[0],Values[0]);
				
			return new Vector2(Values[0],Values[1]);
		}
	}

	
	[Serializable] public struct Keyframe2
	{
		public Vector2		i;
		public Vector2		o;
		public float		t;	//	time
		public float[]		s;	//	start value
		public float[]		e;	//	end value
	}
	
	[Serializable] public struct Float2
	{
		public float[]		x;
		public float[]		y;
	}

	
	
	//	make this generic too
	[Serializable] public struct Frame_Vector2 : IFrame
	{
		public Float2		i;	//	ease
		public Float2		o;	//	ease out
		public float		t;	//	time
		public float[]		s;	//	value at time
		//public float[]		e;	//	end value
		public FrameNumber	Frame => t;	
		
		public Float2		LerpTo(Frame_Vector2 Next,float Lerp)
		{
			//	lerp each member
			Float2 Values;
			Values.x = new float[]{ Mathf.Lerp( this.s[0], Next.s[0], Lerp ) };
			Values.y = new float[]{ Mathf.Lerp( this.s[1], Next.s[1], Lerp ) };
			return Values;
		}
	}
	
	[Serializable] public struct Frame_Float : IFrame
	{
		public ValueCurve	i;	//	ease in value
		public ValueCurve	o;	//	ease out value
		public float		t;	//	time
		public float[]		s;	//	value at time
		public float[]		e;	//	end value
		public FrameNumber	Frame => t;
		
		public float		LerpTo(Frame_Float Next,float Lerp,float Default)
		{
			//	gr: find out when these are missing (bad parse, or end?)
			if ( Next.s == null )
				Next = this;
			if ( this.s == null )
				return Default;
				
			return Mathf.Lerp( this.s[0], Next.s[0], Lerp );
		}
		
	}
	[Serializable] public struct Frame_FloatArray : IFrame
	{
		public ValueCurve	i;
		public ValueCurve	o;
		public float		t;	//	time
		public float[]		s;	//	start value
		public float[]		e;	//	end value
		public FrameNumber Frame	=> t;

			
		public float[]		LerpTo(Frame_FloatArray Next,float Lerp)
		{
			//	gr: find out why this is missing values
			if ( Next.s == null )
				Next = this;
			if ( this.s == null )
				return null;
		
			//	lerp each member
			var Values = new float[s.Length];
			for ( int i=0;	i<Values.Length;	i++ )
				Values[i] = Mathf.Lerp( this.s[i], Next.s[i], Lerp );
			return Values;
		}
	}
	
	
	class Keyframed_Vector2Convertor : JsonConverter<Keyframed_Vector2>
	{
		public override void WriteJson(JsonWriter writer, Keyframed_Vector2 value, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}

		public override Keyframed_Vector2 ReadJson(JsonReader reader, Type objectType, Keyframed_Vector2 existingValue, bool hasExistingValue,JsonSerializer serializer)
		{
			if ( reader.TokenType == JsonToken.StartObject )
			{
				var ThisObject = JObject.Load(reader);
				var SingleFrame = ThisObject.ToObject<Frame_Vector2>(serializer);
				existingValue.AddFrame(SingleFrame);
			}
			else if ( reader.TokenType == JsonToken.StartArray )
			{
				var ThisArray = JArray.Load(reader);
				foreach ( var Frame in ThisArray )
				{
					var FrameReader = new JTokenReader(Frame);
					var FrameObject = JObject.Load(FrameReader);
					var SingleFrame = FrameObject.ToObject<Frame_Vector2>(serializer);
					existingValue.AddFrame(SingleFrame);
				}
			}
			else 
			{
				//existingValue.ReadAnimatedOrNotAnimated(reader);
				Debug.LogWarning($"Decoding Keyframed_Vector2 unhandled token type {reader.TokenType}");
			}
			return existingValue;
		}
	}
	
	class KeyframedConvertor<KeyFramedType,FrameType> : JsonConverter<KeyFramedType> where KeyFramedType : IKeyframed<FrameType>
	{
		public override void WriteJson(JsonWriter writer, KeyFramedType value, JsonSerializer serializer) { throw new NotImplementedException(); }
		public override KeyFramedType ReadJson(JsonReader reader, Type objectType, KeyFramedType existingValue, bool hasExistingValue,JsonSerializer serializer)
		{
			if ( reader.TokenType == JsonToken.StartObject )
			{
				var ThisObject = JObject.Load(reader);
				existingValue.AddFrame( ThisObject, serializer );
			}
			else if ( reader.TokenType == JsonToken.StartArray )
			{
				var ThisArray = JArray.Load(reader);
				foreach ( var Frame in ThisArray )
				{
					var FrameReader = new JTokenReader(Frame);
					
					//	might be an array of numbers
					if ( Frame.Type == JTokenType.Integer || Frame.Type == JTokenType.Float )
					{
						var Value = Frame;
						var Number = (reader.TokenType == JsonToken.Integer) ? (long)Value : (float)Value;
						existingValue.AddFrame(Number);
					}
					else
					{
						var FrameObject = JObject.Load(FrameReader);
						existingValue.AddFrame( FrameObject, serializer );
					}
				}
			}
			else if ( reader.TokenType == JsonToken.Integer || reader.TokenType == JsonToken.Float )
			{
				var Value = reader.Value;
				var Number = (reader.TokenType == JsonToken.Integer) ? (long)Value : (float)Value;
				existingValue.AddFrame(Number);
			}
			else 
			{
				//existingValue.ReadAnimatedOrNotAnimated(reader);
				Debug.LogWarning($"Decoding Frame_Float unhandled token type {reader.TokenType}");
			}
			return existingValue;
		}
	}

	//	making the json convertor simpler with a generic interface
	interface IKeyframed<T>
	{
		public void AddFrame(JObject Object,JsonSerializer Serializer);
		public void AddFrame(T Frame);
		public void AddFrame(float Number);
	}
	
	//	make this generic
	[JsonConverter(typeof(Keyframed_Vector2Convertor))]
	public struct Keyframed_Vector2 : IKeyframed<Frame_Vector2>
	{
		//public int					a;
		//public int					ix;
		
		List<Frame_Vector2>		Frames;
		
		public void AddFrame(float Number)
		{
			throw new Exception($"Vector2 should not be constructed from just a number ({Number})");
		}

		public void AddFrame(JObject Object,JsonSerializer Serializer)
		{
			AddFrame( Object.ToObject<Frame_Vector2>(Serializer) );
		}
		
		public void AddFrame(Frame_Vector2 Frame)
		{
			Frames = Frames ?? new();
			Frames.Add(Frame);
		}

		public Vector2 GetValue(TimeSpan Time)
		{
			if ( Frames == null || Frames.Count == 0 )
				return new Vector2(1,1);
			var xy = Frames[0].i;
			return new Vector2(xy.x[0],xy.y[0]);
		}

	}

	public interface IFrame
	{
		public FrameNumber		Frame { get;}
		
		static (FRAMETYPE,float,FRAMETYPE) GetPrevNextFramesAtFrame<FRAMETYPE>(List<FRAMETYPE> Frames,FrameNumber TargetFrame) where FRAMETYPE : IFrame
		{
			if ( Frames == null || Frames.Count == 0 )
				throw new Exception("Missing frames");
			
			if ( Frames.Count == 1 )
				return (Frames[0],0,Frames[0]);
			
			//	find previous & next frames
			var PrevIndex = 0;
			for ( int f=0;	f<Frames.Count;	f++ )
			{
				var ThisFrame = Frames[f];
				if ( ThisFrame.Frame > TargetFrame )
					break;
				//if ( ThisFrame.Frame >= TargetFrame ) break;
				PrevIndex = f;
			}
			var NextIndex = Mathf.Min(PrevIndex + 1, Frames.Count-1);
			var Prev = Frames[PrevIndex];
			var Next = Frames[NextIndex];
			//	get the lerp(time) between prev & next
			float Range(float Min,float Max,float Value)
			{
				if ( Max-Min <= 0 )
					return 0;
				return (Value-Min)/(Max-Min);
			}
			//var Lerp = Mathf.InverseLerp( Prev.Frame, Next.Frame, TargetFrame );
			var Lerp = Range( Prev.Frame, Next.Frame, TargetFrame );
			if ( Lerp < 0 )
				Lerp = 0;
			if ( Lerp > 1 )
				Lerp = 1;
			return (Prev,Lerp,Next);
		}
		
	}
	
	
		//	make this generic
	[JsonConverter(typeof(KeyframedConvertor<Keyframed_Float,Frame_Float>))]
	public struct Keyframed_Float : IKeyframed<Frame_Float>
	{
		List<Frame_Float>		Frames;

		public void AddFrame(float Number)
		{
			var Frame = new Frame_Float();
			Frame.s = new []{Number};
			//Frame.e = new []{Number};
			AddFrame(Frame);
		}

		public void AddFrame(JObject Object,JsonSerializer Serializer)
		{
			AddFrame( Object.ToObject<Frame_Float>(Serializer) );
		}
		
		public void	AddFrame(Frame_Float Frame)
		{
			Frames = Frames ?? new();
			Frames.Add(Frame);
		}
		
		public float GetValue(FrameNumber Frame,float Default)
		{
			if ( Frames == null || Frames.Count == 0 )
				return Default;
				
			var (Prev,Lerp,Next) = IFrame.GetPrevNextFramesAtFrame(Frames,Frame);
			return Prev.LerpTo( Next, Lerp, Default );
		}
	}
	
	
		//	make this generic
	[JsonConverter(typeof(KeyframedConvertor<Keyframed_FloatArray,Frame_FloatArray>))]
	public struct Keyframed_FloatArray : IKeyframed<Frame_FloatArray>
	{
		List<Frame_FloatArray>		Frames;

		public void AddFrame(float Number)
		{
			var Frame = new Frame_FloatArray();
			Frame.s = new []{Number};
			//Frame.e = new []{Number};
			AddFrame(Frame);
		}

		public void AddFrame(JObject Object,JsonSerializer Serializer)
		{
			AddFrame( Object.ToObject<Frame_FloatArray>(Serializer) );
		}
		
		public void	AddFrame(Frame_FloatArray Frame)
		{
			Frames = Frames ?? new();
			Frames.Add(Frame);
		}
		
		//	default = hack whilst developing
		public float[] GetValue(FrameNumber Frame,float[] Default)
		{
			if ( Frames == null || Frames.Count == 0 )
				return Default;
			
			var (Prev,Lerp,Next) = IFrame.GetPrevNextFramesAtFrame(Frames,Frame);
			var LerpedValues = Prev.LerpTo(Next,Lerp);
			if ( LerpedValues == null || LerpedValues.Length == 0 )
				return Default;
			return LerpedValues;
		}
	}
	
	//	https://lottiefiles.github.io/lottie-docs/playground/json_editor/
	[Serializable] public struct AnimatedNumber
	{
		public int			a;
		public bool			Animated => a!=0;
		
		public Keyframed_Float	k;	//	frames
		
		public float		GetValue(FrameNumber Frame,float Default)
		{
			return k.GetValue(Frame,Default);
		}
	}


	//	https://lottiefiles.github.io/lottie-docs/playground/json_editor/
	[Serializable] public struct AnimatedPosition
	{
		public int			a;
		public bool			Animated => a!=0;
		public int			ix;	//	property index

		//	animated
		//public Keyframe2[]	k;	//	frames
		//	non animated
		public float[]		k;	//	frames
		
		public Vector2		GetPosition(FrameNumber Frame)
		{
			if ( k == null )
				return Vector2.zero;
				
			return new Vector2( k[0], k[1] );
		}
	}
	
	
	
	[Serializable] public struct Bezier
	{
		public List<float[]>	i;	//	in-tangents
		public List<float[]>	o;	//	out-tangents
		public List<float[]>	v;	//	vertexes
		public bool		c;
		public bool		Closed => c;

		public ControlPoint[]	GetControlPoints()
		{
			var Points = new ControlPoint[v.Count];
			for ( var Index=0;	Index<v.Count;	Index++ )
			{
				Points[Index].Position.x = v[Index][0];
				Points[Index].Position.y = v[Index][1];
				Points[Index].InTangent.x = i[Index][0];
				Points[Index].InTangent.y = i[Index][1];
				Points[Index].OutTangent.x = o[Index][0];
				Points[Index].OutTangent.y = o[Index][1];
			}
			return Points;
		}
		
		public struct ControlPoint
		{
			public Vector2	InTangent;
			public Vector2	OutTangent;
			public Vector2	Position;
		}
	}
	
	[Serializable] public struct AnimatedBezier
	{
		public int			a;
		public bool			Animated => a!=0;
		//	if not animated, k==Vector3
		public Bezier		k;	//	frames
		public int			ix;	//	property index
		
		public Bezier		GetBezier(FrameNumber Frame)
		{
			return k;
		}
	}
	
	[Serializable] public struct AnimatedColour
	{
		public int			a;
		public bool			Animated => a!=0;
		//	if not animated, k==Vector3
		public float[]		k;	//	4 elements 0..1
		public int			ix;	//	property index
		
		public Color		GetColour(FrameNumber Frame)
		{
			if ( Animated )
				Debug.Log($"todo: animating colour");
			var Alpha = k.Length == 4 ? k[3] : 1;
			if ( k.Length < 3 )
				return Color.magenta;
			return new Color(k[0],k[1],k[2],Alpha);
		}
	}

	
	[Serializable] public struct TransformMeta
	{
	/*
		public float	r;	//	rotation in degrees clockwise
		public float	sk;	//	skew angle degrees
		public float	sa;	//	Direction at which skew is applied, in degrees (0 skews along the X axis, 90 along the Y axis)
		*/
		public AnimatedVector	s;	//	scale factor, 100=no scaling
		public AnimatedPosition	a;	//	anchor point
		public AnimatedPosition	p;	//	position/translation
		//public AnimatedNumber	r;	//	rotation in degrees clockwise
		public AnimatedNumber	o;	//	opacity 0...100
		
		public Transformer		GetTransformer(FrameNumber Frame)
		{
			var Anchor = a.GetPosition(Frame);
			var Position = p.GetPosition(Frame);
			var FullScale = new Vector2(100,100);
			var Scale = s.GetValue(Frame,Default:FullScale) /FullScale;
			return new Transformer(Position,Anchor,Scale);
		}
		
		//	returns 0-1
		public float			GetOpacity(FrameNumber Frame)
		{
			var Opacity = o.GetValue(Frame,Default:100);
			return Opacity / 100.0f;
		}
	}
	
	
	
	public enum ShapeType
	{
		Fill,
		Stroke,
		Transform,
		Group,
		Path,
		Ellipse,
		TrimPath,		//	path trimmer, to modify (trim) a sibling shape
	}
	
	public class ShapeSpecificMeta
	{
	}

	public class ShapeConvertor : JsonConverter<ShapeWrapper>
	{
		public override void WriteJson(JsonWriter writer, ShapeWrapper value, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}
		
		public override ShapeWrapper ReadJson(JsonReader reader, Type objectType, ShapeWrapper existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			var ShapeObject = JObject.Load(reader);
			var ShapeBase = new Shape();
			ShapeBase.ty = ShapeObject["ty"].Value<String>();
			
			//	now based on type, serialise
			if ( ShapeBase.Type == ShapeType.Ellipse )
			{
				ShapeBase = ShapeObject.ToObject<ShapeEllipse>(serializer);
			}
			else if ( ShapeBase.Type == ShapeType.Fill )
			{
				ShapeBase = ShapeObject.ToObject<ShapeFillAndStroke>(serializer);
			}
			else if ( ShapeBase.Type == ShapeType.Stroke )
			{
				ShapeBase = ShapeObject.ToObject<ShapeFillAndStroke>(serializer);
			}
			else if ( ShapeBase.Type == ShapeType.Transform )
			{
				ShapeBase = ShapeObject.ToObject<ShapeTransform>(serializer);
			}
			else if ( ShapeBase.Type == ShapeType.Group )
			{
				ShapeBase = ShapeObject.ToObject<ShapeGroup>(serializer);
			}
			else if ( ShapeBase.Type == ShapeType.Path )
			{
				ShapeBase = ShapeObject.ToObject<ShapePath>(serializer);
			}
			else if ( ShapeBase.Type == ShapeType.TrimPath )
			{
				ShapeBase = ShapeObject.ToObject<ShapeTrimPath>(serializer);
			}

			existingValue.TheShape = ShapeBase;
			return existingValue;
		}
	}
	
	[JsonConverter(typeof(ShapeConvertor))]
	[Serializable] public struct ShapeWrapper 
	{
		public Shape		TheShape;
		public ShapeType	Type => TheShape.Type; 
	}

	[Serializable] public class Shape 
	{
		public int			ind;//	?
		public int			np;		//	number of properties
		public int			cix;	//	property index
		public int			ix;		//	property index
		public int			bm;		//	blend mode
		public String		nm;		// = "Lottie File"
		public String		Name => nm ?? "Unnamed";
		public String		mn;
		public String		MatchName => mn;
		public bool			hd;	//	i think sometimes this might an int. Newtonsoft is very strict with types
		public bool			Hidden => hd;
		public bool			Visible => !Hidden;
		public String		ty;	
		public ShapeType	Type => ty switch
		{
			"gr" => ShapeType.Group,
			"sh" => ShapeType.Path,
			"fl" => ShapeType.Fill,
			"tr" => ShapeType.Transform,
			"st" => ShapeType.Stroke,
			"el" => ShapeType.Ellipse,
			"tm" => ShapeType.TrimPath,
			_ => throw new Exception($"Unknown type {ty}")
		};
	}
	
	[Serializable] public class ShapePath : Shape
	{
		public AnimatedBezier	ks;	//	bezier for path
		public AnimatedBezier	Path_Bezier => ks;
	}
	
	[Serializable] public class ShapeTrimPath : Shape
	{
		public AnimatedNumber	s;	//	segment start
		public AnimatedNumber	e;	//	segment end
		public AnimatedNumber	o;	//	offset
		public int				m;
		public int				TrimMultipleShapes => m;
	}
		
				
	[Serializable] public class ShapeFillAndStroke : Shape 
	{
		public AnimatedColour	c;	//	colour
		public AnimatedColour	Fill_Colour => c;
		public AnimatedColour	Stroke_Colour => c;
		//public int				r;	//	fill rule
		public AnimatedNumber	o;	//	opacity? 
		public AnimatedNumber	w;	//	width
		public AnimatedNumber	Stroke_Width => w;
		
		public float			GetWidth(FrameNumber Frame)
		{
			var Value = w.GetValue(Frame,Default:42);
			//	gr: it kinda looks like unity's width is radius, and lotties is diameter, as it's consistently a bit thick
			Value *= 0.8f;
			return Value;
		}
		public Color			GetColour(FrameNumber Frame)
		{
			return c.GetColour(Frame);
		}
	}
		
		
	[Serializable] public class ShapeTransform : Shape 
	{
		//	transform
		public AnimatedPosition	p;	//	translation
		public AnimatedPosition	a;	//	anchor
		
		//	gr: not parsing as mix of animated & not
		public AnimatedVector	s;	//	scale
		//public AnimatedVector	r;	//	rotation
		public AnimatedNumber	o;	//	opacity
		
		public Transformer	GetTransformer(FrameNumber Frame)
		{
			var Anchor = a.GetPosition(Frame);
			var Position = p.GetPosition(Frame);
			var FullScale = new Vector2(100,100);
			var Scale = s.GetValue(Frame,Default:FullScale) /FullScale;
			return new Transformer( Position, Anchor, Scale);
		}
		
		public float GetAlpha(FrameNumber Frame)
		{
			var Opacity = o.GetValue(Frame,Default:100);
			float Alpha = Opacity / 100.0f;
			return Alpha;
		}
	}
	
	
	[Serializable] public class ShapeEllipse : Shape 
	{
		public AnimatedVector	s;
		public AnimatedPosition	p;
		public AnimatedVector	Size => s;	
		public AnimatedPosition	Center => p;	
		
	}
	
	public struct ShapeStyle
	{
		public Color?	FillColour;
		public Color?	StrokeColour;
		public float?	StrokeWidth;
		public bool		IsStroked => StrokeColour.HasValue;
		public bool		IsFilled => FillColour.HasValue;
	}

	//	also for layers, but can't call this Transform
	public struct Transformer
	{
		Vector2		Scale2;
		Vector2		Translation;
		Vector2		Anchor;
		
		public Transformer(Vector2 Translation,Vector2 Anchor,Vector2 Scale)
		{
			this.Translation = Translation;
			this.Anchor = Anchor;
			Scale2 = Scale;
		}
		
		Vector2		GetScale()
		{
			if ( Scale2 is Vector2 s2 )
				return s2;
			return Vector2.one;
		} 
		
		public Vector2	LocalToWorld(Vector2 LocalPosition)
		{
			//	gr: this needs to use anchor
			LocalPosition += Translation;
			LocalPosition -= Anchor;
			LocalPosition *= GetScale();
			//	rotate here
			LocalPosition += Anchor;
			
			return LocalPosition;
		}
		
		public float	LocalToWorld(float LocalSize)
		{
			LocalSize *= GetScale().x;
			return LocalSize;
		}
		
		//	gr: might all be simpler if we generate a 2D matrix...
		public Transformer	Multiply(Transformer ChildTransform)
		{
			var NewTransform = ChildTransform;
			NewTransform.Translation += this.Translation;
			var ThisScale = GetScale();
			var ChildScale = ChildTransform.GetScale();
			NewTransform.Scale2 = ChildScale *ThisScale;
			return NewTransform;
		}
	}

	[Serializable] public class ShapeGroup: Shape 
	{
		public List<ShapeWrapper>		it;	//	children
		public IEnumerable<Shape>		Children => it.Select( sw => sw.TheShape );
		
		Shape				GetChild(ShapeType MatchType)
		{
			//	handle multiple instances
			foreach (var s in it)//Children)
			{
				if ( s.Type == MatchType )
					return s.TheShape;
			}
			return null;
		}
		public Transformer		GetTransformer(FrameNumber Frame)
		{
			var Transform = GetChild(ShapeType.Transform) as ShapeTransform;
			if ( Transform == null )
				return new Transformer();
			return Transform.GetTransformer(Frame);
		}
		
		//	this comes from the transform, but we're just not keeping it with it
		public float		GetAlpha(FrameNumber Frame)
		{
			var Transform = GetChild(ShapeType.Transform) as ShapeTransform;
			if ( Transform == null )
				return 1.0f;
			return Transform.GetAlpha(Frame);
		}
		
		public ShapeStyle		GetShapeStyle(FrameNumber Frame)
		{
			var Fill = GetChild(ShapeType.Fill) as ShapeFillAndStroke;
			var Stroke = GetChild(ShapeType.Stroke) as ShapeFillAndStroke;
			var Style = new ShapeStyle();
			if ( Fill != null )
			{
				Style.FillColour = Fill.GetColour(Frame);
			}
			if ( Stroke != null )
			{
				Style.StrokeColour = Stroke.GetColour(Frame);
				Style.StrokeWidth = Stroke.GetWidth(Frame);
			}
			return Style;
		}

	}
	

	
	[Serializable]
	public struct LayerMeta	//	shape layer
	{
		public bool		IsVisible(FrameNumber Frame)
		{
			if ( Frame < FirstKeyFrame )
				return false;
			if ( Frame > LastKeyFrame )
				return false;
			/*
			if ( Time < StartTime )
				return false;
				*/
			return true;
		}
	
		public float				ip;
		public int					FirstKeyFrame => (int)ip;	//	visible after this
		public float				op;	//	= 10
		public int					LastKeyFrame => (int)op;		//	invisible after this (time?)
		
		public String				nm;// = "Lottie File"
		public String				Name => nm ?? "Unnamed";

		public String				refId;
		public String				ResourceId => refId ?? "";
		public int					ind;
		public int					LayerId => ind;
		public float				st;
		public double				StartTime => st;

		public int					ddd;	//	something to do with winding
		public int					parent;
		public int					ty;
		public int					sr;
		public TransformMeta		ks;
		public TransformMeta		Transform=>ks;
		public int					ao;
		public bool					AutoOrient => ao != 0;
		public ShapeWrapper[]		shapes;
		public IEnumerable<Shape>	Children => shapes.Select( sw => sw.TheShape );
		public int					bm;
		public int					BlendMode => bm;
	}
	
	[Serializable]
	public struct MarkerMeta
	{/*
		public var cm : String
		public var id : String		{ return Name }
		public var Name : String	{	return cm	}
		public var tm : Int
		public var Frame : Int	{	return tm	}
		public var dr : Int
		*/
	}
	
		
	[Serializable]
	public struct Root
	{
		public TimeSpan	FrameToTime(FrameNumber Frame)
		{
			return TimeSpan.FromSeconds(Frame/ FramesPerSecond);
		}
		//	gr: output is really float, but trying int for simplicity for a moment...
		public FrameNumber		TimeToFrame(TimeSpan Time,bool Looped)
		{
			var Duration = this.Duration.TotalSeconds;
			var TimeSecs = Looped ? TimeSpan.FromSeconds(Time.TotalSeconds % Duration) : TimeSpan.FromSeconds(Mathf.Min((float)Time.TotalSeconds,(float)Duration));
			var Frame = (TimeSecs.TotalSeconds * FramesPerSecond);
			//Frame += FirstKeyFrame;
			return (FrameNumber)Frame;
		}
		
	
		public string	v;	//"5.9.2"
		public float	fr;
		public float	FramesPerSecond => fr;
		public float	ip;
		public int		FirstKeyFrame => (int)ip;
		public TimeSpan	FirstKeyFrameTime => FrameToTime(FirstKeyFrame);
		public float	op;	//	= 10
		public int		LastKeyFrame => (int)op;
		public TimeSpan	LastKeyFrameTime => FrameToTime(LastKeyFrame);
		public TimeSpan	Duration => LastKeyFrameTime - FirstKeyFrameTime;
		public int		w;//: = 100
		public int		h;//: = 100
		public String	nm;// = "Lottie File"
		public String	Name => nm ?? "Unnamed";
		public int		ddd;	// = 0	//	not sure what this is, but when it's 3 "things are reversed"
			
		public AssetMeta[]	assets;
		public LayerMeta[]	layers;
		public MarkerMeta[]	markers;

		public AssetMeta[]	Assets => assets ?? Array.Empty<AssetMeta>();
		public LayerMeta[]	Layers => layers ?? Array.Empty<LayerMeta>();
		public MarkerMeta[]	Markers => markers ?? Array.Empty<MarkerMeta>();
	}
	
	public class Animation : IDisposable
	{
		Root	lottie;
		
		public Animation(string FileContents)
		{
			//	gr: can't use built in, as the structure changes depending on contents, and end up with clashing types
			//lottie = JsonUtility.FromJson<Root>(FileContents);
			//	can't use the default deserialiser, because for some reason, the parser misses out parsing
			//	[ {}, {} ] 
			//lottie = Newtonsoft.Json.JsonConvert.DeserializeObject<Root>(FileContents);
			
			//	we CAN parse with generic parser!
			var Parsed = JObject.Parse(FileContents);
			
			JsonSerializer serializer = new JsonSerializer();
			
			
			lottie = (Root)serializer.Deserialize(new JTokenReader(Parsed), typeof(Root));
			Debug.Log($"Decoded lottie ok x{lottie.layers.Length} layers");
		}
		
		public TimeSpan Duration => lottie.Duration;
		public int		FrameCount => lottie.LastKeyFrame-lottie.FirstKeyFrame;

		public void Dispose()
		{
			lottie = default;
		}
		
		struct DebugPoint
		{
			public Vector2	Start;
			public Vector2?	End;			//	if true, draw handle here
			public int		Uid;			//	see if we can automatically do this, but different sizes so we see overlaps
			public float	HandleSize => 1.0f + ((float)Uid*0.3f);
			public Color	Colour;
		}
		
		public void Render(TimeSpan PlayTime, Painter2D Painter,Rect ContentRect,bool EnableDebug)
		{
			//	get the time, move it to lottie-anim space and loop it
			var Frame = lottie.TimeToFrame(PlayTime,Looped:true);
			Render( Frame, Painter, ContentRect, EnableDebug );
		}
			
		public void Render(FrameNumber Frame, Painter2D Painter,Rect ContentRect,bool EnableDebug)
		{
			//Debug.Log($"Time = {Time.TotalSeconds} ({lottie.FirstKeyframe.TotalSeconds}...{lottie.LastKeyframe.TotalSeconds})");
		
			var width = ContentRect.width;
			var height = ContentRect.height;
			
			//	scale-to-canvas transformer
			float ExtraScale = 1;	//	for debug zooming
			var ScaleToCanvasWidth = (ContentRect.width / lottie.w)*ExtraScale;
			var ScaleToCanvasHeight = (ContentRect.height / lottie.h)*ExtraScale;
			bool Stretch = false;
			bool FitHeight = false;
			var ScaleToCanvasUniform = FitHeight ? ScaleToCanvasHeight : ScaleToCanvasWidth;
			var ScaleToCanvas = Stretch ? new Vector2( ScaleToCanvasWidth, ScaleToCanvasHeight ) : new Vector2( ScaleToCanvasUniform, ScaleToCanvasUniform );
			Transformer RootTransformer = new Transformer( Vector2.zero, Vector2.zero, ScaleToCanvas );

			void RenderGroup(ShapeGroup Group,Transformer LayerTransform,float LayerAlpha)
			{
				//	run through sub shapes
				var Children = Group.Children;

				//	elements (shapes) in the layer may be in the wrong order, so need to pre-extract style & transform
				var GroupTransform = Group.GetTransformer(Frame);
				GroupTransform = LayerTransform.Multiply(GroupTransform);
				var GroupStyle = Group.GetShapeStyle(Frame);
				var GroupAlpha = Group.GetAlpha(Frame);
				GroupAlpha *= LayerAlpha;
				
	
				//	to do holes in shapes, we need to do them all in one path
				//	so do all the debug stuff on the side
				List<DebugPoint> DebugPoints = new();
				void AddDebugPoint(Vector2 Position,int Uid,Color Colour,Vector2? End=null)
				{
					var Point = new DebugPoint();
					Point.Colour = Colour;
					Point.Uid = Uid;
					Point.Start = Position;
					Point.End = End;
					DebugPoints.Add(Point);
				}
	
				
				void ApplyStyle()
				{
					var FillColour = GroupStyle.FillColour ?? Color.green;
					var StrokeColour = GroupStyle.StrokeColour ?? Color.yellow;
					FillColour.a *= GroupAlpha;
					StrokeColour.a *= GroupAlpha;
					Painter.fillColor = FillColour;
					Painter.strokeColor = StrokeColour;
					Painter.lineWidth = GroupTransform.LocalToWorld( GroupStyle.StrokeWidth ?? 1 );
					if ( GroupStyle.IsStroked )
						Painter.Stroke();
					if ( GroupStyle.IsFilled )
						Painter.Fill(FillRule.OddEven);
				}
				
				void RenderChild(Shape Child)
				{
					//	force visible with debug
					if ( !Child.Visible )
						if ( !EnableDebug ) 
							return;
				
					if ( Child is ShapePath path )
					{
						var Bezier = path.Path_Bezier.GetBezier(Frame);
						var Points = Bezier.GetControlPoints();
						void CurveToPoint(Bezier.ControlPoint Point,Bezier.ControlPoint PrevPoint)
						{
							//	gr: working out this took quite a bit of time.
							//		the cubic bezier needs 4 points; Prev(start), tangent for first half of line(start+out), tangent for 2nd half(end+in), and the end
							var cp0 = PrevPoint.Position + PrevPoint.OutTangent;
							var cp1 = Point.Position + Point.InTangent;
							
							var VertexPosition = GroupTransform.LocalToWorld(Point.Position);
							var ControlPoint0 = GroupTransform.LocalToWorld(cp0);
							var ControlPoint1 = GroupTransform.LocalToWorld(cp1);
							
							AddDebugPoint( Point.Position, 0, Color.red );
							AddDebugPoint( Point.Position, 1, Color.green, cp0 );
							AddDebugPoint( Point.Position, 2, Color.cyan, cp1 );

							if ( true )
							{
								Painter.BezierCurveTo( ControlPoint0, ControlPoint1, VertexPosition  );
							}
							else
							{
								Painter.LineTo( VertexPosition );
							}
						}
						
						for ( var p=0;	p<Points.Length;	p++ )
						{
							var PrevIndex = (p==0 ? Points.Length-1 : p-1);
							var Point = Points[p];
							var PrevPoint = Points[PrevIndex];
							var VertexPosition = GroupTransform.LocalToWorld(Point.Position);
							//	skipping first one gives a more solid result, so wondering if
							//	we need to be doing a mix of p and p+1...
							if ( p==0 )
								Painter.MoveTo(VertexPosition);
							else
								CurveToPoint(Point,PrevPoint);
						}
						
						if ( Bezier.Closed )
						{
							CurveToPoint( Points[0], Points[Points.Length-1] );
						}
					}
					if ( Child is ShapeEllipse ellipse )
					{
						var EllipseSize = GroupTransform.LocalToWorld( ellipse.Size.GetValue(Frame,Default:10) );
						var LocalCenter = ellipse.Center.GetPosition(Frame);
						var EllipseCenter = GroupTransform.LocalToWorld(LocalCenter);
		
						var Radius = EllipseSize;
						Painter.Arc( EllipseCenter, Radius, 0, 360 );
						AddDebugPoint( LocalCenter, 0, Color.magenta );
					}
			
					if ( Child is ShapeGroup subgroup )
					{
						try
						{
							RenderGroup(subgroup,GroupTransform,GroupAlpha);
						}
						catch(Exception e)
						{
							Debug.LogException(e);
						}
					}
				}
				
				bool RenderGroupsAfter = true;
				
				Painter.BeginPath();
				
				foreach ( var Child in Children )
				{
					if ( Child is ShapeGroup && RenderGroupsAfter )
						continue;
					try
					{
						RenderChild(Child);
					}
					catch(Exception e)
					{
						Debug.LogException(e);
					}
				}
				ApplyStyle();
				Painter.ClosePath();
			
				foreach ( var Child in Children )
				{
					if ( !(Child is ShapeGroup)  || !RenderGroupsAfter )
						continue;
					try
					{
						RenderChild(Child);
					}
					catch(Exception e)
					{
						Debug.LogException(e);
					}
				}

				
				if ( EnableDebug )
				{
					foreach ( var Point in DebugPoints )
					{
						var WorldStart = GroupTransform.LocalToWorld(Point.Start);
						Vector2? WorldEnd = Point.End.HasValue ? GroupTransform.LocalToWorld(Point.End.Value) : null;
						
						Painter.lineWidth = 0.2f;
						Painter.strokeColor = Point.Colour;
						Painter.BeginPath();
						Painter.MoveTo( WorldStart );
						if ( WorldEnd is Vector2 end )
						{
							Painter.LineTo( end );
							Painter.Arc( end, Point.HandleSize, 0.0f, 360.0f);
						}
						else
						{
							Painter.Arc( WorldStart, Point.HandleSize, 0.0f, 360.0f);
						}
						Painter.Stroke();
						Painter.ClosePath();
					}
				}
			}
		
			//	not sure if its the json parser, or the format (front to back), but we need to render back to front
			foreach ( var Layer in lottie.layers.Reverse() )
			{
				if ( !Layer.IsVisible(Frame) )
					continue;
				
				var LayerTransform = Layer.Transform.GetTransformer(Frame);
				LayerTransform = RootTransformer.Multiply(LayerTransform);
				var LayerOpacity = Layer.Transform.GetOpacity(Frame);
				
				//	skip hidden layers
				if ( LayerOpacity <= 0 )
				{
					if ( EnableDebug )
					{
						LayerOpacity = 0.1f;
					}
					else
					{
						continue;
					}
				}

				//	render the shape
				foreach ( var Shape in Layer.Children )
				{
					try
					{
						if ( Shape is ShapeGroup group )
						{
							RenderGroup(group,LayerTransform,LayerOpacity);
						}
						else
						{
							Debug.Log($"Not a group...");
						}
					}
					catch(Exception e)
					{
						Debug.LogException(e);
					}
				}
			}
		}
		
	}
	
}

