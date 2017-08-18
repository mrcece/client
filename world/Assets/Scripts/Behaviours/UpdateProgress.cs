using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts
{
    class UpdateProgress : MonoBehaviour
    {
        public void Update()
        {
            var text = gameObject.GetComponent<UnityEngine.UI.Text>();
            var dm = DownloadManager.Ins;
            var downloaded = dm.DownloadingBytes;
            var total = 1;// dm.TotalNeedDownloadBytes;
            var progress = total > 0 ? (double)downloaded / total : 0;
            text.text = string.Format("ndownloaded:{0:0.00} M\ntotal:{1:0.00} M\npercent:{2:0.00} %", dm.DownloadingBytes/1e6, total/1e6, progress * 100);
            GameObject.Find("progress").GetComponent<UnityEngine.UI.Slider>().value = (float)progress;
        }
    }
}
