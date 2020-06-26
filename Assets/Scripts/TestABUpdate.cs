using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityEngine.U2D;
using UnityEngine.UI;
using XLua;
using Random = UnityEngine.Random;

public class TestABUpdate : MonoBehaviour
{
    public AssetReference m_Res; //直接挂在GameObject上的AB引用
    public Text Info;
    LuaEnv luaenv = null;
    private TextAsset LuaFile;
    private Action LoadRes;

    IEnumerator Start()
    {
        //手动初始化AB
        AsyncOperationHandle<IResourceLocator> initHandle = Addressables.InitializeAsync();
        yield return initHandle;

        //手动检查更新
        var checkHandle = Addressables.CheckForCatalogUpdates(false);
        yield return checkHandle;

        if (checkHandle.Result.Count > 0)
        {
            Debug.Log("有更新的CataLog");
            ShowInfo("有更新的CataLog Count : " + checkHandle.Result.Count);

            var updateHandle = Addressables.UpdateCatalogs(checkHandle.Result, false);
            yield return updateHandle;

            List<IResourceLocator> locators = updateHandle.Result;
            foreach (var locator in locators)
            {
                List<object> keys = new List<object>();
                keys.AddRange(locator.Keys);
                var sizeHandle = Addressables.GetDownloadSizeAsync(keys);
                yield return sizeHandle;
                long totalDownloadSize = sizeHandle.Result;
                Debug.Log("Download Size : " + totalDownloadSize);
                ShowInfo($"下载大小 : {totalDownloadSize / 1024} K. ");
                if (totalDownloadSize > 0)
                {
                    var downloadHandle = Addressables.DownloadDependenciesAsync(keys, Addressables.MergeMode.Union);
                    while (!downloadHandle.IsDone)
                    {
                        yield return null;
                    }
                    Debug.Log("下载完成!");
                    ShowInfo("下载完成!");
                    Addressables.Release(downloadHandle);
                }
            }
            Addressables.Release(updateHandle);
        }
        else
        {
            Debug.Log("没有要更新的内容");
            ShowInfo("没有要更新的内容");
        }
        Addressables.Release(checkHandle);

        yield return null;

        //从AB加载Lua脚本
        var Handle = Addressables.LoadAssetAsync<TextAsset>("Assets/LuaScripts/TestLua.lua.txt");
        yield return Handle;

        //Debug.Log(Handle.Result.text);

        LuaFile = Handle.Result;

        //初始化Lua
        luaenv = new LuaEnv();
        luaenv.AddLoader((ref string filename) =>
        {
            if (filename == "TestLua.lua") //自定义Loader从AB里加载Lua
            {
                return LuaFile.bytes;
            }
            return null;
        });
        luaenv.DoString("require('TestLua.lua')");

        var SetInstance = luaenv.Global.Get<Action<TestABUpdate>>("SetInstance");
        SetInstance(this);

        LoadRes = luaenv.Global.Get<Action>("LoadRes");
    }

    public void InitUIPrefab(GameObject pUIRoot, GameObject pUIPrefab, bool pNeedIns = true)
    {
        GameObject go;
        if (pNeedIns)
            go = GameObject.Instantiate(pUIPrefab);
        else
            go = pUIPrefab;

        go.transform.SetParent(pUIRoot.transform);
        var rectTransform = go.GetComponent<RectTransform>();
        if(rectTransform)
        {
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.localScale = Vector3.one;
            rectTransform.localPosition = Vector3.zero;
        }

        //go.transform.Find("GameObject/Button").GetComponent<Image>().sprite = m_AtlasHandle.Result.GetSprite("奔雷项链");
    }

    public void InitGameObject(GameObject pPrefab)
    {
        GameObject g = Instantiate(pPrefab);
        g.transform.localPosition = Random.onUnitSphere * 5;
    }

    public void LoadByLua()
    {
        LoadRes();
    }
     
    public void LoadFromReference()
    {
        StartCoroutine(LoadAndInstan(m_Res));
    }

    IEnumerator LoadAndInstan(string pPath)
    {
        var Handle = Addressables.LoadAssetAsync<GameObject>(pPath);
        yield return Handle;
        if (Handle.IsDone && Handle.Status == AsyncOperationStatus.Succeeded)
        {
            InitGameObject(Handle.Result);
        }
    }

    IEnumerator LoadAndInstan(AssetReference pRes)
    {
        var Handle = pRes.LoadAssetAsync<GameObject>();
        yield return Handle;
        if (Handle.IsDone && Handle.Status == AsyncOperationStatus.Succeeded)
        {
            InitGameObject(Handle.Result);
        }
    }

    void ShowInfo(string pInfo)
    {
        Info.text += pInfo + Environment.NewLine;
    }

    public void CallFromLua(string pPath)
    {
        Debug.Log("Call From Lua : " + pPath);
        StartCoroutine(LoadAndInstan(pPath));
    }
}
