using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveClose : MonoBehaviour {

    public GameObject target;
    // Use this for initialization

    private Vector3 originCameraPos;

    private float lerpRate = 0;
	void Start () {
        originCameraPos = transform.position;
	}
	
	// Update is called once per frame
	void Update () {
        lerpRate += Time.deltaTime * 0.05f;
        transform.position = Vector3.Lerp(originCameraPos, target.transform.position, lerpRate);
	}
}
