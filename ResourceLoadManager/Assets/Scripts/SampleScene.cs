using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SampleScene : MonoBehaviour
{
    private readonly string SpritePath1 = "Sprite/bigsize";
    private readonly string SpritePath2 = "Sprite/fish";

    private readonly string PrefabPath1 = "Prefab/GameObject1";
    private readonly string PrefabPath2 = "Prefab/GameObject2";

    [SerializeField]
    private Image OutputImage = null;

    [SerializeField]
    private Text OutputSpriteCountText = null;

    [SerializeField]
    private Text OutputPrefabCountText = null;

    [SerializeField]
    private GameObject PrefabAttachRoot = null;

    // Start is called before the first frame update
    void Start()
    {
        ResourceManager.Instance.Initialize();
    }

    public void OnClickLoadSprite1()
    {
        ResourceManager.Instance.RequestExecuteOrder(
            SpritePath1,
            ExecuteOrder.Type.Sprite,
            (obj) => {
                OutputImage.sprite = obj as Sprite;
            }
        );
    }

    public void OnClickLoadSprite2()
    {
        ResourceManager.Instance.RequestExecuteOrder(
            SpritePath2,
            ExecuteOrder.Type.Sprite,
            (obj) => {
                OutputImage.sprite = obj as Sprite;
            }
        );
    }

    public void OnClickLoadPrefab1()
    {
        ResourceManager.Instance.RequestExecuteOrder(
            PrefabPath1,
            ExecuteOrder.Type.GameObject,
            (obj) => {
                GameObject go = Instantiate(obj) as GameObject;
                go.transform.SetParent(PrefabAttachRoot.transform);
                go.transform.localPosition = Vector3.zero;
                go.transform.localScale = Vector3.one;
            }
        );
    }

    public void OnClickLoadPrefab2()
    {
        ResourceManager.Instance.RequestExecuteOrder(
            PrefabPath2,
            ExecuteOrder.Type.GameObject,
            (obj) => {
                GameObject go = Instantiate(obj) as GameObject;
                go.transform.SetParent(PrefabAttachRoot.transform);
                go.transform.localPosition = Vector3.zero;
                go.transform.localScale = Vector3.one;
            }
        );
    }

    public void OnClickViewSpriteCount()
    {
        OutputSpriteCountText.text = ResourceManager.Instance.GetSpriteCount().ToString();
    }

    public void OnClickViewPrefabCount()
    {
        OutputPrefabCountText.text = ResourceManager.Instance.GetGameObjectCount().ToString();
    }

    public void OnClickTestFunction1()
    {
        // 試しに、同一フレーム内で複数回同じ画像読み込み処理を呼び出してみる
        // 出力が1なら、正しい挙動
        OnClickLoadSprite1();
        OnClickLoadSprite1();
        OnClickLoadSprite1();
    }

    public void OnClickCacheClear()
    {
        ResourceManager.Instance.RequestExecuteOrder(
            "",
            ExecuteOrder.Type.CachClear,
            (nullObject) => {
            }
        );
    }
}
