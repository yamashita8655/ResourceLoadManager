using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loader
{
    public Loader()
    {
    }

    // 同じファイルを読み込むときは、必ず最初の読み込みが終わってから呼び出す事
    public IEnumerator LoadSprite(string pathAndName, Action<UnityEngine.Object> endCallback)
    {
        // リソースの非同期読込開始
        ResourceRequest resReq = Resources.LoadAsync<Sprite>(pathAndName);

        // 終わるまで待つ
        while (resReq.isDone == false) {
            yield return null;
        }

        endCallback(resReq.asset);
    }

    public IEnumerator LoadGameObject(string pathAndName, Action<UnityEngine.Object> endCallback)
    {
        // リソースの非同期読込開始
        ResourceRequest resReq = Resources.LoadAsync<GameObject>(pathAndName);
        
        // 終わるまで待つ
        while (resReq.isDone == false) {
            yield return null;
        }

        endCallback(resReq.asset);
    }
}
