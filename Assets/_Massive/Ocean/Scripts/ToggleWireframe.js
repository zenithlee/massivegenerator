#pragma strict

var wireframeCamera : GameObject;
var isOn : boolean  = false;

function Start()
{
	wireframeCamera.GetComponent(Camera).GetComponent.<Camera>().enabled = isOn;
}

function Update () 
{
	var b : boolean = wireframeCamera.GetComponent(Camera).GetComponent.<Camera>().enabled;
	
	if(Input.GetKeyDown(KeyCode.F2)) wireframeCamera.GetComponent(Camera).GetComponent.<Camera>().enabled = !b;
}