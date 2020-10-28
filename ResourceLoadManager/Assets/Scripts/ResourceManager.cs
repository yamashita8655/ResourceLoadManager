using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// クラス設計思想としては、
// 1. 処理予約をリストに登録
// →これは、同一フレーム内で2回同じ処理が呼び出されると、2回同じリソースを読み込んでしまう可能性があるので
// 　その対策。また、呼び出し側での読み込みと破棄を同一フレーム内で行った場合のエラーを防ぐ目的。
// 2. 1件ずつ読み込み処理
// 3. 呼び出し側で、どのタイプのリソースを読むのかわかっている事前提で使うので、
// 　UnityEngine.Objectを、呼び出し側でリソースの型にキャストして、使用する
// 4. 一度読み込んだリソースはキャッシュされる
// 
// シングルトンで使用する必要は無いが、その場合は更新処理をどこかから呼び出す必要がある。
// キャッシュクリアも、破棄オーダーをリクエストして実行する
//
// 注意点としては、このクラスは呼び出し元の存在確認をしないので、
// リクエストをしている状態で呼び出し元を破棄しないようにする事
public class ResourceManager : SimpleMonoBehaviourSingleton<ResourceManager>
{
    private Loader ResourceLoader = new Loader();

    private List<ExecuteOrder> LoadOrderList = new List<ExecuteOrder>();
    private ExecuteOrder CurrentOrder = null;

    private Dictionary<string, Sprite> SpriteCacheDict = new Dictionary<string, Sprite>();
    private Dictionary<string, GameObject> GameObjectCacheDict = new Dictionary<string, GameObject>();

    private bool IsExecuteNow = false;

    public string PathAndName { get; private set; }
    public Type ResourceType { get; private set; }
    public Action<UnityEngine.Object> EndCallback { get; private set; }

    public void Initialize()
    {
        // 一応、初期化呼び出したタイミングでオブジェクトが生成されるので
        // 使う側の初期化のタイミングでもいいので、これ呼び出す。
        // 後は、必要に応じて初期化処理が欲しければ、ここに書く
    }

    public void RequestExecuteOrder(string pathAndName, ExecuteOrder.Type type, Action<UnityEngine.Object> endCallback)
    {
        ExecuteOrder order = new ExecuteOrder(pathAndName, type, endCallback);
        LoadOrderList.Add(order);
    }

    // Update is called once per frame
    void Update()
    {
        if (IsExecuteNow == false) {
            if (LoadOrderList.Count > 0) {
                CurrentOrder = LoadOrderList[0];
                LoadOrderList.RemoveAt(0);
            }
        }

        if (IsExecuteNow == false) {
            if (CurrentOrder != null) {
                IsExecuteNow = true;
                StartCoroutine(CoExecutionOrder(CurrentOrder));
            }
        }
    }

    private IEnumerator CoExecutionOrder(ExecuteOrder order)
    {
        if (order.ExecuteType == ExecuteOrder.Type.Sprite) {
            Sprite res = null;
            SpriteCacheDict.TryGetValue(order.PathAndName, out res);
            if (res != null) {
                LoadEndFunction(res);
            } else {
                yield return ResourceLoader.LoadSprite(
                    order.PathAndName,
                    (sprite) => {
                        SpriteCacheDict.Add(CurrentOrder.PathAndName, sprite as Sprite);
                        LoadEndFunction(sprite);
                    }
                );
            }


        } else if (order.ExecuteType == ExecuteOrder.Type.GameObject) {
            GameObject res = null;
            GameObjectCacheDict.TryGetValue(order.PathAndName, out res);
            if (res != null) {
                LoadEndFunction(res);
            } else {
                yield return ResourceLoader.LoadGameObject(
                    order.PathAndName,
                    (loadGameObject) => {
                        GameObjectCacheDict.Add(CurrentOrder.PathAndName, loadGameObject as GameObject);
                        LoadEndFunction(loadGameObject);
                    }
                );
            }
        } else if (order.ExecuteType == ExecuteOrder.Type.CachClear) {
            SpriteCacheDict.Clear();
            GameObjectCacheDict.Clear();
            IsExecuteNow = false;
            CurrentOrder.EndCallback(null);
            CurrentOrder = null;
        }
    }

    private void LoadEndFunction(UnityEngine.Object obj)
    {
        CurrentOrder.EndCallback(obj);
        IsExecuteNow = false;
        CurrentOrder = null;
    }


    // デバッグ用処理なので、不要であれば削除する事
    public int GetSpriteCount()
    {
        return SpriteCacheDict.Count;
    }

    // デバッグ用処理なので、不要であれば削除する事
    public int GetGameObjectCount()
    {
        return GameObjectCacheDict.Count;
    }
}

public class ExecuteOrder {
    public enum Type {
        Sprite,
        GameObject,
        CachClear,
    };

    public string PathAndName { get; private set; }
    public Type ExecuteType { get; private set; }
    public Action<UnityEngine.Object> EndCallback { get; private set; }

    public ExecuteOrder(string pathAndName, Type executeType, Action<UnityEngine.Object> endCallback)
    {
        PathAndName = pathAndName;
        ExecuteType = executeType;
        EndCallback = endCallback;
    }
}
