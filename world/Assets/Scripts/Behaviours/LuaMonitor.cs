using UnityEngine;

public class LuaMonitor : MonoBehaviour {

   
    public int useMemoryKiloBytes;
	void Start () {
		
	}

    // Update is called once per frame
    private float nextUpdateTime = 0;
	void Update () {
        if (Time.time < nextUpdateTime) return;
        if (LuaClient.Instance == null) return;
        var luaState = LuaClient.GetMainState();
        if(luaState != null)
        {
            var cg = luaState.GetFunction("collectgarbage");
            useMemoryKiloBytes =  (int)cg.Invoke<string, double>("count");
        }
        nextUpdateTime = Time.time + 1.0f;
	}
}


