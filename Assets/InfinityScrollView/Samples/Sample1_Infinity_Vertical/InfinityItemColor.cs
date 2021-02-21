using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using OneP.InfinityScrollView;
public class InfinityItemColor : InfinityBaseItem {
	public Image image;
	public Text text;
	// Use this for initialization
	void Start () {

	}

	// Update is called once per frame
	void Update () {

	}

	public override void Reload(InfinityScrollView _infinity, int _index){
		base.Reload(_infinity,_index);
		text.text = "Item_" + (Index+1);
		if(_index%2==0)
			text.color = new Color (1,0.9f, 1.0f, 1f);
		else
			text.color = new Color (0.5f,0.9f, 1.0f, 1f);
	}

	public void OnClick()
	{
		Sample1.Instance.OnClickItem (Index + 1);
	}
}
