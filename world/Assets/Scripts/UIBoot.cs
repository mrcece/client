using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


class UIBoot : UnityEngine.MonoBehaviour
{
    public UnityEngine.UI.Button start = null;


    private void Awake()
    {
        start.onClick.AddListener(OnStart);
    }

    private void OnStart()
    {
        Main.Ins.GameStart = true;
    }
}

