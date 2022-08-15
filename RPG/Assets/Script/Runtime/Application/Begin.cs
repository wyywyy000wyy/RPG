using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using XLua;
using UnityEngine.UI;
using System.Runtime.InteropServices;

[LuaCallCSharp]
public class Begin : MonoBehaviour
{
    // Start is called before the first frame update
    public string enterLua = "begin"; //main
    LuaEnv luaenv;
    void Start()
    {
        luaenv = new LuaEnv();
        luaenv.AddLoader(Loader);
        StartCoroutine(StartEnterLua());
    }

    IEnumerator StartEnterLua()
    {
        LoadEmmyLua();
        yield return new WaitForSeconds(0.1f);
        luaenv.DoString("require('" + enterLua + "')");
    }


    public static string GetFileName(string name)
    {
        string path = "Assets/Lua/" + name + ".lua";
        if (File.Exists(path))
        {
            return path;
        }
        path = Application.dataPath + "/Lua/" + name + ".lua";
        return path;
    }

    public static long FileWriteTime(string path)
    {
        var sysTime = File.GetLastWriteTime(path);
        return sysTime.Ticks;
    }

    [DllImport("xlua", CallingConvention = CallingConvention.Cdecl)]
    public static extern int luaopen_emmy_core(System.IntPtr L);

    public static int LoadEmmyCore(System.IntPtr L)
    {
        return luaopen_emmy_core(L);
    }

    public void LoadEmmyLua()
    {
        LoadEmmyCore(luaenv.L);
    }

    public static string ExternPath = "/storage/emulated/0/Lua/";

    private byte[] Loader(ref string name)
    {
        int a = 1;
        a = 8;
        if ("emmy_core".Equals(name))
        {
            return null;
        }
#if UNITY_EDITOR
        string path = "Assets/Script/Lua/" + name.Replace('.', '/') + ".lua";
#else
        string path = ExternPath + name.Replace('.', '/') + ".lua";
        if(!File.Exists(path))
        {
            path = Application.dataPath + "/Lua/" + name.Replace('.', '/') + ".lua";
        }
        if(!File.Exists(path))
        {
            path = Application.persistentDataPath + "/Lua/"+ name.Replace('.', '/') + ".lua";
        }
#endif
        name = "Assets/Script/Lua/" + name.Replace('.', '/') + ".lua";

        byte[] str = null;
        str = File.ReadAllBytes(path);
        return str;
    }

    // Update is called once per frame
    void Update()
    {
        if (luaenv!=null)
        {
            luaenv.Tick();
        }
    }
}
