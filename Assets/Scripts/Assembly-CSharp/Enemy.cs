using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : Hazard
{
	public enum Kill
	{
		Rolling,
		Homing,
		Diving,
		Other
	}

	public enum Direction
	{
		TowardsPlayer,
		ToPlayersRight,
		ToPlayersLeft,
		AwayFromPlayer,
		Stationary
	}

	protected bool m_movementPermitted = true;

	protected float m_trackDistance;

	protected bool m_frozen;

	protected bool m_attackable = true;

	private TrackSegment m_trackSegment;

	private List<Enemy> m_group;

	[SerializeField]
	private GameObject m_enemyModel;

	[SerializeField]
	private Vector3 m_goldenFlareLocation = new Vector3(0f, 0.5f, 0f);

	public bool Golden { get; private set; }

	public List<Enemy> Group
	{
		get
		{
			return m_group;
		}
		set
		{
			m_group = value;
		}
	}

	public TrackSegment TrackSegment
	{
		get
		{
			return m_trackSegment;
		}
		set
		{
			m_trackSegment = value;
		}
	}

	public bool isMovementPermitted => m_movementPermitted;

	public float TrackDistance => m_trackDistance;

	public override void Start()
	{
		base.Start();
	}

	public void Place(OnEvent onDestroy, Track track, Spline spline, Direction initialDir, float trackDistance)
	{
		m_trackDistance = trackDistance;
		Place(onDestroy);
		Place(track, spline, initialDir);
		m_movementPermitted = true;
	}

	public void GroupTogether(Enemy enemy)
	{
		if (enemy.Group == null)
		{
			m_group = new List<Enemy>();
			m_group.Add(this);
			m_group.Add(enemy);
			enemy.Group = m_group;
		}
		else
		{
			m_group = enemy.Group;
			m_group.Add(this);
		}
	}

	public bool isAttackable()
	{
		return m_attackable;
	}

	public void updateMovementPermission(float movementDisallowDistance)
	{
		if (!(Sonic.Tracker == null))
		{
			float trackPosition = Sonic.Tracker.TrackPosition;
			if (isMovementPermitted && TrackDistance - trackPosition < movementDisallowDistance)
			{
				setMovementPermission(allow: false, affectGroupEnemies: true);
			}
		}
	}

	public void setMovementPermission(bool allow, bool affectGroupEnemies)
	{
		m_movementPermitted = allow;
		if (!affectGroupEnemies || m_group == null)
		{
			return;
		}
		foreach (Enemy item in m_group)
		{
			item.setMovementPermission(allow, affectGroupEnemies: false);
		}
	}

	public virtual bool isSlowdownTarget()
	{
		return false;
	}

	public virtual bool isChopper()
	{
		return false;
	}

	public abstract bool isAttackableFromAir();

	public abstract Vector3 getTargetPosition();

	public abstract Spline getSpline();

	public abstract void beginAttack();

	public abstract void endAttack();

	public abstract bool isLaneValid();

	public abstract Track.Lane getLane();

	protected abstract void Place(Track track, Spline spline, Direction initialDir);

	public abstract float getNearRange();

	public abstract float getFarRange();

	public void SetGoldenState(Material defaultMaterial, Material goldenMaterial)
	{
		Golden = Boosters.IsNextEnemyGolden;
		SkinnedMeshRenderer[] componentsInChildren = m_enemyModel.GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].sharedMaterial = ((!Golden) ? defaultMaterial : goldenMaterial);
		}
	}

	public Vector3 getGoldernFlareLocation()
	{
		return m_goldenFlareLocation;
	}
}
