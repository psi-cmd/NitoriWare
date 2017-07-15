﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleFloatingInteractive : MonoBehaviour
{
    public TitleInteractableSpawner spawner;
    public Vector2 lastVelocity;

#pragma warning disable 0649   //Serialized Fields
    [SerializeField]
    private float startSpeed, lifetime, escapeSpeed;
    [SerializeField]
    private Vector2 floatTowardsBounds, bounceVolumeSpeedBounds;
    [SerializeField]
    private Rigidbody2D _rigidBody;
    [SerializeField]
    private Collider2D wallHitCollider;
    [SerializeField]
    AudioSource sfxSource;
    [SerializeField]
    AudioClip bounceClip;
#pragma warning restore 0649

    private bool ignoreWalls;
    private float colliderExtent;

    void Start()
	{
        colliderExtent = Mathf.Max(wallHitCollider.bounds.extents.x, wallHitCollider.bounds.extents.y);
        wallHitCollider.enabled = false;

        Vector2 goal = new Vector2(Random.Range(-floatTowardsBounds.x, floatTowardsBounds.x),
            Random.Range(-floatTowardsBounds.y, floatTowardsBounds.y));
        _rigidBody.velocity = (goal - (Vector2)transform.localPosition).resize(startSpeed);
        lastVelocity = _rigidBody.velocity;
	}

    void LateUpdate()
    {
        if (GameMenu.shifting)
        {
            _rigidBody.bodyType = RigidbodyType2D.Kinematic;
            Vector2 escapeVelocity = MathHelper.getVector2FromAngle(
                ((Vector2)(transform.position - Camera.main.transform.position)).getAngle(), escapeSpeed);
            transform.position += (Vector3)escapeVelocity * Time.deltaTime;
            if (CameraHelper.isObjectOffscreen(transform, 10f))
                Destroy(gameObject);
            return;
        }
        else if (!wallHitCollider.enabled && !CameraHelper.isObjectOffscreen(transform,
            -colliderExtent))
        {
            wallHitCollider.enabled = true;
        }

        if (lifetime > 0f)
        {
            lifetime -= Time.deltaTime;
            if (lifetime <= 0f)
                setIgnoreWalls(true);
        }
        
        if (lastVelocity != Vector2.zero)
        {
            sfxSource.panStereo = AudioHelper.getAudioPan(transform.position.x);
            if ((Mathf.Sign(_rigidBody.velocity.x) == -Mathf.Sign(lastVelocity.x))
                || (Mathf.Sign(_rigidBody.velocity.y) == -Mathf.Sign(lastVelocity.y))
                || Mathf.Abs(_rigidBody.velocity.magnitude - lastVelocity.magnitude) > bounceVolumeSpeedBounds.x)
            {
                float speed = _rigidBody.velocity.magnitude;
                float volume = Mathf.Pow(Mathf.Lerp(0f, 1f,
                    ((speed - bounceVolumeSpeedBounds.x) / (bounceVolumeSpeedBounds.y - bounceVolumeSpeedBounds.y))),
                    .5f);
                if (volume > 0 && PrefsHelper.getVolume(PrefsHelper.VolumeType.SFX) > 0f && !float.IsNaN(volume))
                    sfxSource.PlayOneShot(bounceClip, volume);
            }
        }
        lastVelocity = _rigidBody.velocity;

        if (CameraHelper.isObjectOffscreen(transform, 10f))
            Destroy(gameObject);
    }

    public void setIgnoreWalls(bool ignore)
    {
        if (wallHitCollider == null)
            return;

        foreach (BoxCollider2D wall in spawner.wallColliders)
        {
            Physics2D.IgnoreCollision(wallHitCollider, wall, ignore);
        }
        ignoreWalls = ignore;
    }
}