using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading;

public class LoaderThread : MonoBehaviour 
{
	public struct DelayedQueueItem 
	{
		public float time;
		public Action action;
	}

	private static int numThreads;
	public static int maxThreads = 4;
	private List<Action> actions = new List<Action> ();
	private List<Action> currentActions = new List<Action> ();
	private List<DelayedQueueItem> delayed = new List<DelayedQueueItem> ();
	private List<DelayedQueueItem> currentDelayed = new List<DelayedQueueItem> ();

	private int count;
	private static bool initialized;
	private static LoaderThread current;
	public static LoaderThread getCurrent 
	{
		get 
		{
			initialize ();
			return current;
		}
	}

	private static void initialize ( ) 
	{
		if (!initialized) 
		{
			if (!Application.isPlaying)
			{
				return;
			}
			initialized = true;
			GameObject gameObj = new GameObject ("LoaderThread");
			current = gameObj.AddComponent<LoaderThread> ();
		}
	}

	public static void queueOnMainThread (Action action) 
	{
		queueOnMainThread (action, 0f);
	}

	public static void queueOnMainThread (Action action, float time)
	{
		if (time != 0) 
		{
			lock (getCurrent.delayed) 
			{
				getCurrent.delayed.Add (new DelayedQueueItem { time = Time.time + time, action = action });
			}
		}
		else 
		{
			lock (getCurrent.actions)
			{
				getCurrent.actions.Add (action);
			}
		}
	}

	public static Thread runAsync (Action a) 
	{
		initialize ();
		while (numThreads >= maxThreads) Thread.Sleep (1);
		Interlocked.Increment (ref numThreads);
		ThreadPool.QueueUserWorkItem (runAction, a);
		return null;
	}

	private static void runAction (object action) 
	{
		try 
		{
			((Action)action) ();
		}
		catch 
		{
			;
		}
		finally 
		{
			Interlocked.Decrement (ref numThreads);
		}
	}

	void Awake ( ) 
	{
		current = this;
		initialized = true;
	}

	void Update ( ) 
	{
		lock (this.actions) 
		{
			this.currentActions.Clear ();
			this.currentActions.AddRange (this.actions);
			this.actions.Clear ();
		}
		foreach (var a in this.currentActions) 
		{
			a ();
		}
		lock (this.delayed) 
		{
			this.currentDelayed.Clear ();
			this.currentDelayed.AddRange (delayed.Where (d => d.time <= Time.time));
			foreach (var item in this.currentDelayed) 
			{
				this.delayed.Remove (item);
			}
		}
		foreach (var delayed in this.currentDelayed) 
		{
			delayed.action ();
		}
	}

	void OnDisable ( ) 
	{
		if (current == this) 
			current = null;
	}
}
