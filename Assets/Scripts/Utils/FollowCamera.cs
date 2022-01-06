using UnityEngine;
using System.Collections;

public class FollowCamera : MonoBehaviour 
{
	public const float totalDegree = 360.0f;					
	public const float halfDegree = 180.0f;						
	public const float quarterDegree = 90.0f;					
	public const float isOne = 1.0f;							
	public const float isZero = 0.0f;							
	public const float isMinor = -1.0f;							
	
	private GameObject focusObject;						
	
	private Vector3 followCamTransform;					
	private Vector3 targetTransform;						
	
	private Vector3 initCamereaPos;						
	private Vector3 defaultNormalize;					
	private Vector3 cameraNormalize;						

	private bool wheelFlag = true;						
	private bool dragFlag = true;						
	private bool selectMode = false;						
	private bool resetMode = false;						
	private float distance = isZero;						
	private float selectDistance = isZero;				
	private float defaultDistance = isZero;				
	
	public GameObject followCam;								
	public GameObject targetObject;								

	public float smoothValue = 10000.0f;						
	public float cameraSpeed = 30.0f;							

	private Vector2 position;									
	private bool activeCam = true;						
	private Rect checkRect;

	private float speed = isZero;								
	private float speedX = isZero;								
	private float speedY = isZero;								
	private float offset = isZero;								

	void initfollowCamera()										
	{
		checkRect = new Rect(0,0,0,0);
		distance = isZero;
		
		if (followCam == null) 
		{
			followCamTransform = new Vector3 (isZero, isZero, isZero);
		} 
		else 
		{
			followCamTransform = followCam.transform.position;
		}
		
		if (targetObject == null) 
		{
			targetTransform = new Vector3 (isZero, isZero, isZero);
		} 
		else 
		{
			targetTransform = targetObject.transform.position;
		}

		defaultNormalize = cameraNormalize = new Vector3 (isZero, isZero, isZero);
		
		position = Vector2.zero;
	}

	// Use this for initialization
	void Start () 												
	{
		initfollowCamera ();

		focusObject = targetObject;
		StartCoroutine ("setTransformPosition");

		defaultDistance = distance = Vector3.Distance(followCamTransform, targetTransform);
		
		followCam.transform.LookAt (targetTransform);

		setTransformNormalize ();

		defaultNormalize = cameraNormalize;
	}
	
	IEnumerator setTransformPosition()							
	{
		while (true) 
		{
			yield return null;
			targetObject = focusObject;
			followCamTransform = followCam.transform.position;
			targetTransform = targetObject.transform.position;
		}
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(activeCam)
		{
			if(!selectMode)
			{
				trackingCamera ();

				if(Input.touchCount == 2)
				{
					dragFlag = false;
				}
				else if(Input.touchCount == 0)
				{
					dragFlag = true;
				}

				if(dragFlag)										
				{
					if (Input.GetMouseButtonDown(0)) 
					{
						down();
					} 
					else if (Input.GetMouseButton (0)) 
					{
						moved (Input.mousePosition);
					} 
					else if (Input.GetMouseButtonUp(0)) 
					{
						up();
					} 
					else 
					{
						remainSpeed ();
					}
				}

				if(wheelFlag && distance <= 5.0f && distance >= 1.0f)			
				{
					wheelControl ();
				}
				else if (wheelFlag && distance > 5.0f) 
				{
					distance = 5.0f;
					speed = isZero;
					speedX = isZero;
					speedY = isZero;
				}
				else if (wheelFlag && distance < 1.0f) 
				{
					distance = 1.0f;
					speed = isZero;
					speedX = isZero;
					speedY = isZero;
				}
				trackingCamera ();
			}
			else
			{		
				cameraAutoMove();												
			}
		}
		else
		{
			if (Input.GetMouseButtonDown(0)) 
			{
				down();
			} 
			else if (Input.GetMouseButton (0)) 
			{
				moved (Input.mousePosition, false);
			} 
			else if (Input.GetMouseButtonUp(0)) 
			{
				up(false);
			} 
			else 
			{
				remainSpeed (false);
			}
		}
	}
	
	private void cameraAutoMove () 											
	{
		targetObject = focusObject;

		speed = isZero;
		speedX = isZero;
		speedY = isZero;
		
		if(!resetMode)
		{
			followCamTransform = followCam.transform.position;

			targetTransform = targetObject.transform.position;

			float moveDistance = Vector3.Distance(followCam.transform.position, targetObject.transform.position);

			Quaternion beforeLookAt = followCam.transform.rotation;
			followCam.transform.LookAt(targetTransform);
			Quaternion afterLookAt = followCam.transform.rotation;
			
			followCam.transform.rotation = Quaternion.Lerp(beforeLookAt, afterLookAt, Time.deltaTime * (20.0f - (moveDistance)));

			if(moveDistance > selectDistance)
			{
				followCam.transform.position = Vector3.Lerp (followCam.transform.position, targetObject.transform.position, (Time.deltaTime * moveDistance / 40.0f) + 0.005f);
			}
			else
			{
				if(beforeLookAt.eulerAngles.y - afterLookAt.eulerAngles.y < 2.0f)
				{
					setTransformNormalize ();
					distance = Vector3.Distance(followCam.transform.position, targetObject.transform.position);
					selectMode = false;
				} 
				else 
				{
					followCam.transform.position = Vector3.Lerp (followCam.transform.position, targetObject.transform.position, (Time.deltaTime * moveDistance / 40.0f) + 0.005f);
				}
			}
		}
		else
		{
			followCamTransform = followCam.transform.position;
			Vector3 resetPosition = targetTransform = targetObject.transform.position;
			
			resetPosition += defaultNormalize * (selectDistance + isOne);
			
			float moveDistance = Vector3.Distance(followCam.transform.position, resetPosition);

			Quaternion beforeLookAt = followCam.transform.rotation;
			followCam.transform.LookAt(targetTransform);
			Quaternion afterLookAt = followCam.transform.rotation;

			followCam.transform.rotation = Quaternion.Lerp(beforeLookAt, afterLookAt, Time.deltaTime * (20.0f - (moveDistance)));
			
			if(moveDistance > isOne)
			{
				followCam.transform.position = Vector3.Lerp (followCam.transform.position, resetPosition, (Time.deltaTime * moveDistance / 40.0f) + 0.01f);
			}
			else
			{
				if(beforeLookAt.eulerAngles.y - afterLookAt.eulerAngles.y < 2.0f)
				{
					setTransformNormalize ();
					distance = Vector3.Distance(followCam.transform.position, targetObject.transform.position);
					selectMode = false;
					resetMode = false;
				}
				else
				{	
					followCam.transform.position = Vector3.Lerp (followCam.transform.position, resetPosition, (Time.deltaTime * moveDistance / 40.0f) + 0.01f);
				}
			}
		}
	}

	private void down() 													
	{
		position = Input.mousePosition;
		
		speedX = isZero;
		speedY = isZero;

		activeCam = true;

		if(checkRect.xMin < position.x && checkRect.xMax > position.x)
		{
			
			if(checkRect.yMin < Screen.height - position.y && checkRect.yMax > Screen.height - position.y)
			{
				activeCam = false;
			}
		}
	}
	
	private void moved (Vector2 trackingPosition, bool trs = true) 							
	{
		float calOffset = isZero;
		
		calculateOffset ();
		
		calOffset = variationOffset (offset);

		Vector2 difPosition = Vector2.zero;

		difPosition.x = position.x - trackingPosition.x;
		difPosition.y = position.y - trackingPosition.y;
		
		if (difPosition.x < 0.01f && difPosition.x > -0.01f) 
		{
			speedX -= (speedX / 20.0f);
		} 
		else 
		{
			speedX += (difPosition.x * Time.deltaTime * 0.002f * calOffset * cameraSpeed * (distance / defaultDistance));
		}
		
		if (difPosition.y < 0.01f && difPosition.y > -0.01f) 
		{
			speedY -= (speedY / 20.0f);
		} 
		else 
		{
			speedY += (difPosition.y * Time.deltaTime * 0.002f * calOffset * cameraSpeed * (distance / defaultDistance));	
		}

		if(trs)
		{
			changeLocation ();
		}
		
		position = trackingPosition;
	}	

	private void up(bool trs = true) 														
	{		
		calculateOffset ();
		
		speedX -= (speedX / 20.0f);
		speedY -= (speedY / 20.0f);

		if(trs)
		{
			changeLocation ();
		}
	}
	
	private void remainSpeed (bool trs = true)												
	{
		float smooth = isOne / smoothValue;
		if(speedX > (isMinor * smooth) && speedX < smooth)
		{
			speedX = isZero;
		}
		
		if(speedY > (isMinor * smooth) && speedY < smooth)
		{
			speedY = isZero;
		}
		
		up(trs);
	}
	
	private void calculateOffset ()											
	{
		offset = followCam.transform.eulerAngles.x;
		
		if (offset < isZero) 
		{
			offset += totalDegree;
		}
		
		if (offset >= halfDegree) 
		{
			offset -= halfDegree;
		}
		
		offset -= quarterDegree;
	}
	
	private float variationOffset (float value)								
	{
		float returnValue = value;
		if (returnValue < isZero) 
		{
			returnValue *= isMinor;
		}
		
		returnValue /= quarterDegree;
		
		return returnValue;
	}
	
	private void changeLocation()											
	{
		if ((offset >= (isMinor * quarterDegree) && offset < -10.0f) || (offset <= quarterDegree && offset > 10.0f)) 
		{
			followCam.transform.Translate (new Vector3 (speedX, speedY, isZero));
		} 
		else 
		{
			if(offset < isZero)
			{
				if(speedY < isZero)
				{
					followCam.transform.Translate (new Vector3 (isZero, speedY, isZero));
				}
			}
			else
			{
				if(speedY > isZero)
				{
					followCam.transform.Translate (new Vector3 (isZero, speedY, isZero));
				}
			}
			followCam.transform.Translate (new Vector3 (speedX, isZero, isZero));
		}

		followCamTransform = followCam.transform.position;
		setTransformNormalize ();
	}

	private void wheelControl()												
	{
		float mouseControlValue = 0.0f;
#if UNITY_EDITOR
		mouseControlValue = Input.GetAxis ("Mouse ScrollWheel");
#elif UNITY_ANDROID
		if(Input.touchCount == 2)
		{
			Touch Touch1 = Input.GetTouch(0);
			Touch Touch2 = Input.GetTouch(1);

			Vector2 touch1Pos = Touch1.position - Touch1.deltaPosition;
			Vector2 touch2Pos = Touch2.position - Touch2.deltaPosition;

			float prevTouchesDeltaMag = (touch1Pos - touch2Pos).magnitude;
			float touchesDeltaMag = (Touch1.position - Touch2.position).magnitude;

			mouseControlValue = (prevTouchesDeltaMag - touchesDeltaMag) * -0.001f;
		}
#endif
		if(mouseControlValue != 0) 
		{
			float translation = mouseControlValue * 0.25f * (speed + (cameraSpeed * 4.0f)) * Time.deltaTime;
			speed += translation;
		} 
		else 
		{
			float removeGravity = speed / 20.0f;
			speed -= removeGravity;
			float smooth = isOne / smoothValue;
			if(speed < smooth && speed > (isMinor * smooth))
			{
				speed = isZero;
			}
		}
		distance -= speed;
	}

	private void trackingCamera()											
	{
		followCam.transform.position = new Vector3 (targetTransform.x, targetTransform.y, targetTransform.z);
		followCam.transform.position += cameraNormalize * distance;
		
		followCam.transform.LookAt (targetTransform);
	}
	
	private void setTransformNormalize()									
	{
		cameraNormalize = new Vector3 (followCamTransform.x - targetTransform.x, followCamTransform.y - targetTransform.y, followCamTransform.z - targetTransform.z);
		cameraNormalize.Normalize ();
	}

	public GameObject gettargetObject()								
	{
		return focusObject;
	}

	public void settargetObject(GameObject targetObject, float targetDistance, bool resetMode)	
	{
		this.selectMode = true;
		this.resetMode = resetMode;
		this.focusObject = targetObject;
		this.selectDistance = targetDistance;
	}
	
	public void setDistance(float distance)			
	{
		this.distance = distance;
	}
	
	public void setDragMode(bool statusOnOff)		
	{
		this.dragFlag = statusOnOff;
	}
	
	public void setWheelMode(bool statusOnOff)		
	{
		this.wheelFlag = statusOnOff;
	}

	public void setChkWinRect(Rect winRect)//bool active)
	{
		this.checkRect = winRect;
		//activeCam = active;
	}

	public void setCamActive(bool active)
	{
		activeCam = active;
	}

	public Vector2 getMovePos()
	{
		if(!activeCam)
		{
			return new Vector2(speedX, -speedY);
		}
		else
		{
			return Vector2.zero;
		}
	}
}
