﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheeseFindDrawer : MonoBehaviour {
    private bool _isLocked = true;
    public bool isLocked {
        get { return _isLocked; }
        set { _isLocked = value; }
    }

    private bool _isOpen = true;
    public bool isOpen {
        get { return _isOpen; }
        set { _isOpen = value; }
    }

    private CheeseFindController _controller;
    public CheeseFindController controller {
        get { return _controller; }
        set { _controller = value; }
    }

    private bool _hasItem = false;
    
    private CheeseFindItem _item;
    public CheeseFindItem item {
        get { return _item; }
        set {
            if(value == null) {
                _item = null;
                _hasItem = false;
                return;
            }
            _item = value;
            _hasItem = true;
            _item.drawerPosition = transform.parent.position;
        }
    }

    private bool _isPulling = false;
    private float _pullFactor = 1f;
    private float _pullLimit = .5f;

	void Start() {
	}
	
	void Update() {
        if(!_isPulling && _pullFactor > 0f) {
            if(!_isOpen) {
                _pullFactor -=  Time.deltaTime;
                if(_pullFactor < 0f)
                    _pullFactor = 0f;
            }

            Vector3 drawerPos = new Vector3(
                transform.parent.position.x,
                transform.parent.position.y - Mathf.Lerp(0f, _pullLimit, EaseInOutSine(_pullFactor)),
                transform.parent.position.z);
            transform.position = drawerPos;
        }

        float scaleFactor = .25f * _pullFactor + 1f;
		transform.localScale = new Vector3(scaleFactor, scaleFactor, 1f);
	}

    void OnMouseDown() {
        if(_isLocked)
            return;

        _isPulling = true;

        //TODO: Click feedback (Sound + pop effect on the drawer)
    }

    void OnMouseUp() {
        if(_isLocked || !_isPulling)
            return;
        _isPulling = false;
    }

    void OnMouseDrag() {
        if(_isLocked || !_isPulling)
            return;

        float cursorY = CameraHelper.getCursorPosition().y;
        float deltaY = transform.parent.position.y - cursorY;

        _pullFactor = Mathf.Clamp(deltaY / _pullLimit, 0f, 1f);
        
        Vector3 drawerPos = new Vector3(
            transform.parent.position.x,
            transform.parent.position.y - Mathf.Lerp(0f, _pullLimit, EaseInOutQuart(_pullFactor)),
            transform.parent.position.z);
        transform.position = drawerPos;

        if(_pullFactor >= 1f) {
            _isPulling = false;
            _isLocked = true;
            _isOpen = true;

            //TODO: Added effects

            if(_hasItem) {
                _item.MoveTo(transform.parent.position + new Vector3(0f, 3f, 0f), 1f);
                _controller.AddPoint(1);
            }
            else {
                _controller.SetVictory(false);
            }
        }
    }

    float EaseInOutSine(float t) {
        return (1f - Mathf.Cos(t * Mathf.PI)) / 2f;
    }

    float EaseInOutQuart(float t) {
        if(t < .5f)
            return 8f * t * t * t * t;
        else {
            float f = (t - 1f);
            return -8f * f * f * f * f + 1f;
        }
    }
}
