using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class ResourceManager
{
    public readonly static ResourceManager Ins = new ResourceManager();

    public void Start()
    {
        Main.Ins.Updater += Update;
    }

    public void Update()
    {

    }
}

